using System;
using System.IO;
using System.Net;
using Lib_Service.Infrastructure.Middleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Svc_C.Context;

//using Microsoft.AspNetCore.HealthChecks;

namespace Svc_C
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<SvcDbContext>();
                    DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    var configuration = (IConfiguration)options.ApplicationServices.GetService(typeof(IConfiguration));
                    const int httpPort = 80;
                    const int httpsPort = 443;

                    var localDebug = configuration.GetValue("ASPNETCORE_LOCAL_DEBUG", false);

                    var certPassword = (localDebug)
                        ? configuration.GetValue<string>("Kestrel:Certificates:LocalDev:Password")
                        : configuration.GetValue<string>("Kestrel:Certificates:ContainerDev:Password");

                    var certPath = (localDebug)
                        ? $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{configuration.GetValue<string>("Kestrel:Certificates:LocalDev:Path")}"
                        : configuration.GetValue<string>("Kestrel:Certificates:ContainerDev:Path");

                    options.Listen(IPAddress.Any, httpPort);
                    options.Listen(IPAddress.Any, httpsPort, listenOptions => { listenOptions.UseHttps(certPath, certPassword); });
                })
                .UseFailing(options =>
                {
                    options.ConfigPath = "/Failing";
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var builtConfig = config.Build();

                    var configurationBuilder = new ConfigurationBuilder();

                    if (Convert.ToBoolean(builtConfig["UseVault"]))
                    {
                        configurationBuilder.AddAzureKeyVault(
                            $"https://{builtConfig["Vault:Name"]}.vault.azure.net/",
                            builtConfig["Vault:ClientId"],
                            builtConfig["Vault:ClientSecret"]);
                    }

                    configurationBuilder.AddEnvironmentVariables();

                    config.AddConfiguration(configurationBuilder.Build());
                })
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .UseApplicationInsights();
    }
}
