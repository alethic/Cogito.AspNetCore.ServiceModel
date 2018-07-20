using System;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Options for configuration of the ASP.Net Core WCF service host.
    /// </summary>
    public class AspNetCoreServiceHostOptions
    {

        /// <summary>
        /// Service type to host.
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// Callback to return the binding to configure the service with.
        /// </summary>
        public Func<IServiceProvider, AspNetCoreBinding> Binding { get; set; } =
            services => (AspNetCoreBinding)services.GetService(typeof(AspNetCoreBasicBinding));

        /// <summary>
        /// Configurator to apply optional configuration.
        /// </summary>
        public Action<AspNetCoreServiceHostConfigurator> Configure { get; set; }

    }

}
