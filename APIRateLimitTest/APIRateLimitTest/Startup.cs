using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace APIRateLimitTest
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
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "APIRateLimitTest", Version = "v1"});
            });
            
            services.AddOptions();
            
            #region RateLimit

            services.Configure<IpRateLimitOptions>(options =>
            {
                options.GeneralRules = new List<RateLimitRule>()
                {
                    new()
                    {
                        Endpoint = ":/status",
                        Period = "1s",
                        Limit = 1,
                        QuotaExceededResponse = new QuotaExceededResponse
                        {
                            Content = "Too Many Requests in 1s",
                            ContentType = "application/text",
                            StatusCode = 429
                        },
                    }
                };
                options.EnableEndpointRateLimiting = true;
                options.EnableRegexRuleMatching = true;
            });
            
            services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect("127.0.0.1")); //Redis IP
            services.AddRedisRateLimiting();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            #endregion

            // Add framework services.
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "APIRateLimitTest v1"));
            }
            
            app.UseIpRateLimiting();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}