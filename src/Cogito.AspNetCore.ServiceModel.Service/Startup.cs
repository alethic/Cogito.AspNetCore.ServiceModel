using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;


namespace Cogito.AspNetCore.ServiceModel.Service
{
    public class Startup
    {

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModel();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            //app.UseServiceHost<MathService>("", configure =>
            //{
            //    configure.AddServiceEndpoint<IMathService>("");
            //    configure.AddServiceEndpoint<IMathService>("/2");
            //});

            app.MapWhen(i => i.Request.Path.Value.Split('/').Length == 3, app2 =>
            {
                //app2.Use(async (context, next) =>
                //{
                //    await context.Response.WriteAsync("MAPPED");
                //});

                // make relative to matched path
                app2.Use(async (context, next) =>
                {
                    context.Request.PathBase += context.Request.Path;
                    context.Request.Path = null;
                    await next();
                });

                app2.UseServiceHost<MathService>(configure =>
                {
                    configure.AddServiceEndpoint<IMathService>("");
                    configure.AddServiceEndpoint<IMathService>("/2");
                });
            });
        }

    }

}
