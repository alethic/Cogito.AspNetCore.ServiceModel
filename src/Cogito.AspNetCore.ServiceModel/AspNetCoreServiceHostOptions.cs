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
        /// Configurator to apply optional configuration.
        /// </summary>
        public Action<AspNetCoreServiceHostConfigurator> Configure { get; set; }

    }

}
