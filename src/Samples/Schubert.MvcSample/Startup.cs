using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Schubert.Framework.Scheduling;

namespace Schubert.MvcSample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
             .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSchubertFramework(this.Configuration,
                setup =>
                {
                    setup.AddJobScheduling();
                    setup.AddWebFeature(web =>
                    {
                        web.AddFluentValidationForMvc();
                        web.ConfigureFeature(f => f.MvcFeatures = Schubert.Framework.Web.MvcFeatures.Full);
                    });

                    //setup.AddRedisCache();
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.ApplicationServices.GetRequiredService<ISchedulingServer>();
            loggerFactory.AddConsole(LogLevel.Information);
            loggerFactory.AddDebug(LogLevel.Error);
            app.UseSession();
            app.StartSchubertWebApplication(); 
        }
    }

}
