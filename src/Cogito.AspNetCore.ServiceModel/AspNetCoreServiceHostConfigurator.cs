using System;
using System.ServiceModel;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Allows configuration of the service host middleware.
    /// </summary>
    public class AspNetCoreServiceHostConfigurator
    {

        readonly AspNetCoreServiceHostMiddleware middleware;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="middleware"></param>
        internal AspNetCoreServiceHostConfigurator(AspNetCoreServiceHostMiddleware middleware)
        {
            this.middleware = middleware ?? throw new ArgumentNullException(nameof(middleware));
        }

        /// <summary>
        /// Registers a service contract at the given relative path.
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public AspNetCoreServiceHostConfigurator AddServiceEndpoint<TContract>(string relativePath = "")
        {
            middleware.AddServiceEndpoint<TContract>(relativePath);
            return this;
        }

        /// <summary>
        /// Registers a service contract at the given relative path.
        /// </summary>
        /// <param name="contractType"></param>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public AspNetCoreServiceHostConfigurator AddServiceEndpoint(Type contractType, string relativePath = "")
        {
            middleware.AddServiceEndpoint(contractType, relativePath);
            return this;
        }

        /// <summary>
        /// Gets a reference to the service host.
        /// </summary>
        public ServiceHost ServiceHost  => middleware.ServiceHost;

    }

}
