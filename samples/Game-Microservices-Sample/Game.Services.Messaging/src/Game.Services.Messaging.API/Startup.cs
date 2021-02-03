using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MicroBootstrap;
using MicroBootstrap.Redis;
using MicroBootstrap.Swagger;
using MicroBootstrap.WebApi;
using Game.Services.Messaging.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Game.Services.Messaging.Application;
using Game.Services.Messaging.Infrastructure;

namespace Game.Services.Messaging.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private static readonly string[] Headers = new[] { "X-Resource" };
        public ILifetimeScope AutofacContainer { get; private set; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebApi();
            services.AddHealthChecks();
            services.AddSwaggerDocs();
            //services.AddJwt();

            services.AddInfrastructure();
            services.AddApplication();

            AddSignalR(services);

            //RestEase Services
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", cors =>
                    cors.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders(Headers));
            });

            services.AddInitializers();

            //RestEase Register Services

        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register your own things directly with Autofac
        }

        private void AddSignalR(IServiceCollection services)
        {
            var options = Configuration.GetOptions<SignalrOptions>("signalr");
            services.AddSingleton(options);
            var builder = services.AddSignalR();
            if (!options.Backplane.Equals("redis", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            var redisOptions = Configuration.GetOptions<RedisOptions>("redis");
            builder.AddRedis(redisOptions.ConnectionString);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
          IHostApplicationLifetime applicationLifetime)
        {
            // If, for some reason, you need a reference to the built container, you
            // can use the convenience extension method GetAutofacRoot.
            this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseInfrastructure();
            app.UseInitializers();
            app.UseRouting();
            app.UseAuthorization();
            app.UseDefaultFiles(); //set startup url to index static file
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapGet("/", async context =>
                                     await context.Response.WriteAsync(GetHomeHtml(context)));
                endpoints.MapHealthChecks("/healthz");
                var signalrOptions = app.ApplicationServices.GetRequiredService<SignalrOptions>();
                endpoints.MapHub<GameHub>($"/{signalrOptions.Hub}");
            });
            app.UseAllForwardedHeaders();
            app.UseSwaggerDocs();
            app.UseErrorHandler();
            app.UseServiceId();
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                AutofacContainer.Dispose();
            });
        }
        private string GetHomeHtml(HttpContext context)
        {
            
            string hostUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            string signalrAddress = hostUrl + "/signalr";
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append(context.RequestServices.GetService<AppOptions>().Name);
            builder.Append($"<br><br>signalR-address is: <a href='{signalrAddress}'>{signalrAddress}</a>");
            var html = $"<html><body>{builder}</body></html>";
            return html;
        }

    }
}
