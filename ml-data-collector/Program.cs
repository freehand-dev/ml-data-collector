using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace ml_data_collector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.Sources.Clear();
                    IHostEnvironment env = builderContext.HostingEnvironment;

                    #region WorkingDirectory
                    var workingDirectory = env.ContentRootPath;
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        workingDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FreeHand", env.ApplicationName);
                    }
                    else if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        workingDirectory = System.IO.Path.Combine($"/opt/", env.ApplicationName, "etc", env.ApplicationName);
                    }
                    if (!System.IO.Directory.Exists(workingDirectory))
                        System.IO.Directory.CreateDirectory(workingDirectory);

                    config.SetBasePath(workingDirectory);

                    // add workingDirectory service configuration
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                       {"WorkingDirectory", workingDirectory}
                    });
                    #endregion

                    //
                    Console.WriteLine($"$Env:EnvironmentName={ env.EnvironmentName }");
                    Console.WriteLine($"$Env:ApplicationName={ env.ApplicationName }");
                    Console.WriteLine($"$Env:ContentRootPath={ env.ContentRootPath }");
                    Console.WriteLine($"WorkingDirectory={ workingDirectory }");

                    config.AddIniFile($"{ env.ApplicationName }.conf", optional: true, reloadOnChange: true);
                    config.AddCommandLine(args);
                    config.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel((context, serverOptions) =>
                    {
                        serverOptions.Configure(context.Configuration.GetSection("Kestrel"))
                            .Endpoint("HTTPS", listenOptions =>
                            {
                                listenOptions.HttpsOptions.SslProtocols = SslProtocols.Tls12;
                            });
                    }).UseStartup<Startup>();
                });
    }
}
