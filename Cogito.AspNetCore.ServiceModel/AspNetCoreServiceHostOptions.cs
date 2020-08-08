using System;
using System.ServiceModel.Channels;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Options for configuration of the ASP.Net Core WCF service host.
    /// </summary>
    public class AspNetCoreServiceHostOptions
    {

        /// <summary>
        /// Factory to create bindings.
        /// </summary>
        public Type BindingFactoryType { get; set; } = typeof(AspNetCoreBindingFactory);

        /// <summary>
        /// Version of messaging to use.
        /// </summary>
        public MessageVersion MessageVersion { get; set; } = MessageVersion.Default;

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
