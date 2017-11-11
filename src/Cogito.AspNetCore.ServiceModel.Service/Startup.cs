using System;
using System.ServiceModel;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;


namespace Cogito.AspNetCore.ServiceModel.Service
{
    public class Startup
    {

        ServiceHost BuildServiceHost(IServiceProvider services)
        {
            var h = new ServiceHost(typeof(MathService));
            h.Description.Behaviors.Add(new AspNetCoreServiceBehavior(services));
            h.Open();
            return h;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(i => new AspNetCoreRequestHandler());
            services.AddSingleton(i => BuildServiceHost(i));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                var host = app.ApplicationServices.GetRequiredService<ServiceHost>();
                await app.ApplicationServices.GetRequiredService<AspNetCoreRequestHandler>().SendAsync(context);
                await next?.Invoke();
            });
        }

    }

}
