using System;
using System.Text;
using aspnet_websocket_sample.Middlewares;
using EchoApp.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EchoApp
{
    public class Startup
    {
        private ILogger _logger;
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //For BroadcastController keep sent message history
            services.AddMemoryCache();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var useAzureSignalr = IsSetUseAzureSignalr();

            var signalRServerBuilder = services.AddSignalR().AddHubOptions<EchoHub>(options =>
            {
                options.EnableDetailedErrors = true;
            });

            if (useAzureSignalr)
            {
                signalRServerBuilder.AddAzureSignalR();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //for System.Encoding.GetEncoding() to work.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _logger = loggerFactory.CreateLogger<Startup>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }


            app.UseLogRequest();

            var useAzureSignalr = IsSetUseAzureSignalr();

            if (!useAzureSignalr)
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<EchoHub>("/ws", options =>
                    {
                        options.WebSockets.CloseTimeout = new TimeSpan(0, 0, 1);
                    });
                });
            }
            
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute("broadcast", "{controller}/{action=Index}");
            });

            app.UseFileServer();

            if (useAzureSignalr)
            {
                app.UseAzureSignalR(routes =>
                {
                    routes.MapHub<EchoHub>("/ws");
                });
            }
        }

        private bool IsSetUseAzureSignalr()
        {
            return !string.IsNullOrEmpty(Configuration["UseAzureSignalR"]) && bool.Parse(Configuration["UseAzureSignalR"].Trim());
        }
    }
}
