
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace ChatServer
{
    public class Startup
    {
        private const string CorsPolicyName = "CorsPolicy";

        public IConfiguration Config { get; }

        public Startup(IConfiguration config)
        {
            Config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            var allowedOrigins = Config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
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
                services.AddSingleton<DatabaseService>(new DatabaseService(Config.GetValue<string>("login"), Config.GetValue<string>("password")));
                services.AddMvcCore()
                    .AddAuthorization();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(CorsPolicyName);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

            app.Run(async (context) =>
            {
                await context.Response.Body.WriteAsync(Encoding.ASCII.GetBytes($"{Assembly.GetEntryAssembly().GetName().Name} is running!"));
            });
        }
    }
}
