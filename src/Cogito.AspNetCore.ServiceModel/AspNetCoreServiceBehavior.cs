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
    public class AspNetCoreServiceBehavior :
        IServiceBehavior
    {

        readonly AspNetCoreRequestQueue queue;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="queue"></param>
        public AspNetCoreServiceBehavior(AspNetCoreRequestQueue queue)
        {
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            bindingParameters.Add(queue);
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }

    }

}
