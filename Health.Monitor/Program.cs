using System;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Health_Monitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
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
                        : configuration.GetValue<string>("Kestrel:Certificates:Development:Password");

                    var certPath = (localDebug)
                        ? $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{configuration.GetValue<string>("Kestrel:Certificates:LocalDev:Path")}"
                        : configuration.GetValue<string>("Kestrel:Certificates:Development:Path");

                    options.Listen(IPAddress.Any, httpPort);
                    options.Listen(IPAddress.Any, httpsPort, listenOptions => { listenOptions.UseHttps(certPath, certPassword); });
                })
                .UseStartup<Startup>();
    }
}
