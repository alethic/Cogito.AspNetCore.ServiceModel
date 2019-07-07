using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Applies contextual configuration to host a WCF service in ASP.Net core.
    /// </summary>
    class AspNetCoreServiceBehavior :
        IServiceBehavior
    {

        readonly AspNetCoreRequestRouter router;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="router"></param>
        public AspNetCoreServiceBehavior(AspNetCoreRequestRouter router)
        {
            this.router = router ?? throw new ArgumentNullException(nameof(router));
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            bindingParameters.Add(router);
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }

    }

}
