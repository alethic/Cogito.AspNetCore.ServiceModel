using System.ServiceModel.Channels;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            app.UseServiceHost(new AspNetCoreServiceHostOptions()
            {
                ServiceType = typeof(MathService),
                MessageVersion = MessageVersion.Soap11,
                Configure = c => c.AddServiceEndpoint<IMathService>("")
            });
        }

    }

}
