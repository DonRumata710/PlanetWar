using System.Net.Http;
using System.Reflection;
using System;
using System.Linq;
using LaunchServer.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;


namespace LaunchServer
{
    public class Startup
    {
        private const string CorsPolicyName = "CorsPolicy";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var allowedOrigins = Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            allowedOrigins = allowedOrigins
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Select(origin => origin.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, builder =>
                {
                    builder
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                    var allowAnyOrigin = allowedOrigins.Length == 0 || allowedOrigins.Any(origin => origin == "*");

                    if (allowAnyOrigin)
                    {
                        builder.SetIsOriginAllowed(_ => true);
                    }
                    else
                    {
                        builder
                            .WithOrigins(allowedOrigins)
                            .AllowCredentials();
                    }
                });
            });

            // adds DI services to DI and configures bearer as the default scheme
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    // identity server issuing token
                    options.Authority = "https://localhost:44332/";
                    options.RequireHttpsMetadata = false;

                    // allow self-signed SSL certs
                    options.BackchannelHttpHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = delegate { return true; } };

                    // the scope id of this api
                    options.Audience = "planetwarapi";
                });

            services.AddSingleton<DatabaseService>(new DatabaseService(Configuration.GetValue<string>("login"), Configuration.GetValue<string>("password")));

            services.AddMvcCore(options => {
                options.EnableEndpointRouting = false;
            })
                .AddAuthorization();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            IdentityModelEventSource.ShowPII = true;

            app.UseCors(CorsPolicyName);
            // adds authentication middleware to the pipeline so authentication will be performed on every request
            app.UseAuthentication();
            app.UseMvc();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync(Assembly.GetEntryAssembly().GetName().Name + " is running!");
            });
        }
    }
}
