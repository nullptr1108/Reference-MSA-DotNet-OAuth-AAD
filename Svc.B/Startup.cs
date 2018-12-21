using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using Lib_HealthChecks;
using Lib_Service.Infrastructure.Filters;
using Lib_Service.Infrastructure.Middleware;
using Lib_Service.Services;
using Lib_Service.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Svc_B.Context;
using Swashbuckle.AspNetCore.Swagger;

namespace Svc_B
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            RegisterAppInsights(services);

            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                options.Filters.Add(typeof(ValidateModelStateFilter));

            }).AddControllersAsServices().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            ConfigureAuthService(services);

            services.AddHealthChecks()
                .AddCheck<HeartBeat>("HeartBeat");

            services.Configure<ServiceSettings>(Configuration);

            //By connecting here we are making sure that our service
            //cannot start until redis is ready. This might slow down startup,
            //but given that there is a delay on resolving the ip address
            //and then creating the connection it seems reasonable to move
            //that cost to startup instead of having the first request pay the
            //penalty.
            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<ServiceSettings>>().Value;
                var configuration = ConfigurationOptions.Parse(settings.ConnectionString, true);

                configuration.ResolveDns = true;

                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddNodeServices();
            services.AddHttpContextAccessor();

            services.AddDbContext<SvcDbContext>(opt => opt.UseInMemoryDatabase(databaseName: "SvcDb"));

            var azureAdB2CTenant = Configuration.GetValue<string>("AzureAdB2C:Tenant");
            var azureAdB2CAppIdUri = Configuration.GetValue<string>("AzureAdB2C:AppIdUri");
            var azureAdB2CPolicy = Configuration.GetValue<string>("AzureAdB2C:Policy");

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Svc B API",
                    Version = "v1",
                    Description = "The Service B HTTP API",
                    TermsOfService = "Terms Of Service"
                });

                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", new[] { "openid", $"https://{azureAdB2CTenant}/{azureAdB2CAppIdUri}/read.access", $"https://{azureAdB2CTenant}/{azureAdB2CAppIdUri}/write.access" } }
                });

                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = $"https://login.microsoftonline.com/{azureAdB2CTenant}/oauth2/v2.0/authorize?p={azureAdB2CPolicy}&response_mode=fragment",
                    Scopes = new Dictionary<string, string>
                    {
                        {"openid", "OpenID"},
                        {$"https://{azureAdB2CTenant}/{azureAdB2CAppIdUri}/read.access", "API Access for Reads" },
                        {$"https://{azureAdB2CTenant}/{azureAdB2CAppIdUri}/write.access", "API Access for Writes" }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IIdentityService, IdentityService>();

            services.AddOptions();

            var container = new ContainerBuilder();
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            //loggerFactory.AddAzureWebAppDiagnostics();
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            app.Map("/liveness", lapp => lapp.Run(async ctx => ctx.Response.StatusCode = 200));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            app.UseStaticFiles();
            app.UseCors("CorsPolicy");

            ConfigureAuth(app);

            app.UseMvcWithDefaultRoute();

            var azureAdB2CClientId = Configuration.GetValue<string>("AzureAdB2C:ClientId");
            var azureAdB2CAppIdUri = Configuration.GetValue<string>("AzureAdB2C:AppIdUri");

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Svc.B API V1");
                   c.OAuthClientId(azureAdB2CClientId);
                   c.OAuthAppName(azureAdB2CAppIdUri);
               });

            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
            });

            app.UseHealthChecks("/health-ui", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            //ConfigureEventBus(app);
        }

        private void RegisterAppInsights(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
            var orchestratorType = Configuration.GetValue<string>("OrchestratorType");

            if (orchestratorType?.ToUpper() == "K8S")
            {
                // Enable K8s telemetry initializer
                services.AddApplicationInsightsKubernetesEnricher();
            }
        }

        private void ConfigureAuthService(IServiceCollection services)
        {
            // prevent from mapping "sub" claim to nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var azureAdB2CTenant = Configuration.GetValue<string>("AzureAdB2C:Tenant");
            var azureAdB2CClientId = Configuration.GetValue<string>("AzureAdB2C:ClientId");
            var azureAdB2CPolicy = Configuration.GetValue<string>("AzureAdB2C:Policy");

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(jwtOptions =>
            {
                jwtOptions.Authority = $"https://login.microsoftonline.com/tfp/{azureAdB2CTenant}/{azureAdB2CPolicy}/v2.0/";
                jwtOptions.Audience = azureAdB2CClientId;
                jwtOptions.RequireHttpsMetadata = false;
            });
        }

        protected virtual void ConfigureAuth(IApplicationBuilder app)
        {
            if (Configuration.GetValue<bool>("UseLoadTest"))
            {
                app.UseMiddleware<ByPassAuthMiddleware>();
            }

            app.UseAuthentication();
        }


    }
}
