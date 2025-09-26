
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace ChatServer
{
    public class Startup
    {
        public IConfiguration Config { get; }

        public Startup(IConfiguration config)
        {
            Config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddCors(options =>
            {
                options.AddPolicy(name: "MyPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader();
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

            //app.UseCors("MyPolicy");
            app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseRouting();

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
