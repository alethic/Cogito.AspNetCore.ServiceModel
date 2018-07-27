using System.ServiceModel.Description;
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

            app.UseServiceHost<MathService>("", configure =>
            {
                configure.AddServiceEndpoint<IMathService>("");
                configure.AddServiceEndpoint<IMathService>("/2");

                var d = configure.ServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
                d.IncludeExceptionDetailInFaults = true;
            });
        }

    }

}
