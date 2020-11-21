using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.WebSamples
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // Default single server
            // services.AddServiceStackRedisCache();

            // Customize single server
            // services.AddServiceStackRedisCache(options =>
            // {
            //     options.SingleServer = "123456@127.0.0.1:6379";
            // });

            // Read and write separation
            // services.AddServiceStackRedisCache(options =>
            // {
            //     options.ReadWriteServers = new[]
            //     {
            // "192.168.1.1:6379", "123456@192.168.1.2:6379", "123456@192.168.1.3:6379", "123456@192.168.1.4:6379"
            //     };
            //     options.ReadOnlyServers = new[]
            //     {
            // "192.168.1.1:6379", "123456@192.168.1.3:6379"
            //     };
            // });

            // Load from configuration
            services.AddServiceStackRedisCache(Configuration.GetSection("ServiceStackRedisOptions"));

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
