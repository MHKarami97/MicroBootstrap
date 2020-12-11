using Autofac;
using MicroBootstrap.WebApi;
using MicroBootstrap;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicroBootstrap.Swagger;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Game.Services.EventProcessor.Application;
using Game.Services.EventProcessor.Infrastructure;

namespace Game.Services.EventProcessor.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private static readonly string[] Headers = new[] { "X-Operation", "X-Resource", "X-Total-Count" };

        // public ILifetimeScope AutofacContainer { get; private set; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebApi();
            services.AddHealthChecks();
            services.AddSwaggerDocs();
            //services.AddJwt();

            services.AddApplication();
            services.AddInfrastructure();
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
        // public void ConfigureContainer(ContainerBuilder builder)
        // {
        //     // Register your own things directly with Autofac
        // }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            // If, for some reason, you need a reference to the built container, you
            // can use the convenience extension method GetAutofacRoot.
            //this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseInfrastructure();
            app.UseInitializers();
            app.UseRouting();
            app.UseStaticFiles();
            // app.UseAuthentication();
            // app.UseAuthorization();
            // app.UseAccessTokenValidator();
            app.UseServiceId();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapGet("/", async context =>
                         await context.Response.WriteAsync(context.RequestServices.GetService<AppOptions>().Name));
                endpoints.MapHealthChecks("/healthz");
            });

            app.UseAllForwardedHeaders();
            app.UseErrorHandler();
            app.UseSwaggerDocs();
            applicationLifetime.ApplicationStopped.Register(() =>
            {
              
            });
        }

    }
}
