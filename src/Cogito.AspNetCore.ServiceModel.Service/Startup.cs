using System.ServiceModel;
using System.ServiceModel.Channels;
using LexisNexis.EFM;
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
            app.UseServiceHost<MathService>("/Service.asmx", MessageVersion.Soap11WSAddressingAugust2004, c => { });
        }

    }

}
