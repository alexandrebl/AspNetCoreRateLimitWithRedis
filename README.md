# AspNetCoreRateLimitWithRedis
AspNet Core Rate Limit with Redis

## Passo 1: Instalar NuGets

```cs
install-package AspNetCoreRateLimit
install-package AspNetCoreRateLimit.Redis
```

##  Passo 2: Trecho de código a ser adicionado na class Startup.cs seção ConfigureServices

```cs
#region RateLimit

services.Configure<IpRateLimitOptions>(options =>               // Defini o limite de cota por IP de Origem
{
    options.GeneralRules = new List<RateLimitRule>()            // Regra de limite de requisição
    {
        new()
        {
            Endpoint = ":/status",                              // Expressão regular para filtrar o recurso http a ser monitorado
            Period = "1s",                                      // Período 1s = um segundo. Use m: minuto, h: hora e d: dia
            Limit = 1,                                          // Total de requisições permitidas dentro do período
            QuotaExceededResponse = new QuotaExceededResponse   //Padronizaçãop da resposta
            {
                Content = "Too Many Requests in 1s",            //Resposta
                ContentType = "application/text",               // Tipo da resposta. Use application/json para retorno JSON
                StatusCode = 429                                //Codigo Http de retorno de estado
            },
        }
    };
    options.EnableEndpointRateLimiting = true;                  // Ativa cota de limite para endpoint customizado
    options.EnableRegexRuleMatching = true;                     // Habilita Regex
});

services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect("127.0.0.1")); //Redis IP
services.AddRedisRateLimiting();
services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
#endregion
```

## Passo 3: Trecho de código a ser adicionado na class Startup.cs seção Configure

```cs
app.UseIpRateLimiting();    // Ativa o uso do Middleware de RateLimit
```

<hr />

## Exemplo de arquivo Startup.cs completo

```cs
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

            services.Configure<IpRateLimitOptions>(options =>               // Defini o limite de cota por IP de Origem
            {
                options.GeneralRules = new List<RateLimitRule>()            // Regra de limite de requisição
                {
                    new()
                    {
                        Endpoint = ":/status",                              // Expressão regular para filtrar o recurso http a ser monitorado
                        Period = "1s",                                      // Período 1s = um segundo. Use m: minuto, h: hora e d: dia
                        Limit = 1,                                          // Total de requisições permitidas dentro do período
                        QuotaExceededResponse = new QuotaExceededResponse   //Padronizaçãop da resposta
                        {
                            Content = "Too Many Requests in 1s",            //Resposta
                            ContentType = "application/text",               // Tipo da resposta. Use application/json para retorno JSON
                            StatusCode = 429                                //Codigo Http de retorno de estado
                        },
                    }
                };
                options.EnableEndpointRateLimiting = true;                  // Ativa cota de limite para endpoint customizado
                options.EnableRegexRuleMatching = true;                     // Habilita Regex
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
            
            app.UseIpRateLimiting();    // Ativa o uso do Middleware de RateLimit

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
```