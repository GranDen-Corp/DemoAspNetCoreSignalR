using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;
using System.IO;

namespace EchoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                LoggerConfiguration logConfig = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.Trace();


                Log.Logger = logConfig
                        .Enrich.FromLogContext().CreateLogger();

                IWebHost host = CreateWebHostBuilder(args).Build();

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "start failed");
            }

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            return builder.UseKestrel()
                          .UseContentRoot(Directory.GetCurrentDirectory())
                          .UseIISIntegration()
                          .UseSerilog()
                          .UseStartup<Startup>();
        }
    }
}
