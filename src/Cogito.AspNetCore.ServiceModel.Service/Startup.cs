﻿using System.ServiceModel.Channels;

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
            app.UseServiceHost<MathService, IMathService>("/Service.asmx", MessageVersion.Soap11, c => { });
        }

    }

}
