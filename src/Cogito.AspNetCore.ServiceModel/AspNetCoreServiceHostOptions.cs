using System;

namespace Cogito.AspNetCore.ServiceModel
{

    public class AspNetCoreServiceHostOptions
    {

        /// <summary>
        /// Service type to host.
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// Binding to host service type with.
        /// </summary>
        public Type BindingType { get; set; }

        /// <summary>
        /// Configurator to apply optional configuration.
        /// </summary>
        public Action<AspNetCoreServiceHostConfigurator> Configure { get; set; }

    }

}
