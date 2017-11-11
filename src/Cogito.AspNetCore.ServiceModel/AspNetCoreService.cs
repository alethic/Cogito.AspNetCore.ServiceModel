using System;
using System.ServiceModel;

namespace Cogito.AspNetCore.ServiceModel
{

    public class AspNetCoreService<TService>
        where TService : class
    {

        readonly AspNetCoreRequestHandler handler;
        readonly ServiceHost host;
        readonly TService singletonInstance;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspNetCoreService(params Uri[] baseAddresses)
        {
            this.handler = new AspNetCoreRequestHandler();
            this.host = new ServiceHost(typeof(TService), baseAddresses);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspNetCoreService(TService singletonInstance, params Uri[] baseAddresses)
        {
            this.handler = new AspNetCoreRequestHandler();
            this.singletonInstance = singletonInstance ?? throw new ArgumentNullException(nameof(singletonInstance));
            this.host = new ServiceHost(singletonInstance, baseAddresses);
        }

    }

}
