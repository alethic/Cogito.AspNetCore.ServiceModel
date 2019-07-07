using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Private <see cref="ServiceHost"/> implementation.
    /// </summary>
    class AspNetCoreServiceHost : ServiceHost
    {

        readonly Func<Type, ServiceEndpoint[]> addDefaultEndpoints;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="addDefaultEndpoints"></param>
        /// <param name="httpBaseAddress"></param>
        /// <param name="httpsBaseAddress"></param>
        public AspNetCoreServiceHost(Type serviceType, Func<Type, ServiceEndpoint[]> addDefaultEndpoints, Uri httpBaseAddress, Uri httpsBaseAddress) :
            base(serviceType, new[] { httpBaseAddress, httpsBaseAddress })
        {
            this.addDefaultEndpoints = addDefaultEndpoints ?? throw new ArgumentNullException(nameof(addDefaultEndpoints));
        }

        public override ReadOnlyCollection<ServiceEndpoint> AddDefaultEndpoints()
        {
            return new ReadOnlyCollection<ServiceEndpoint>(
                Description.ServiceType.GetInterfaces()
                    .SelectMany(i => addDefaultEndpoints?.Invoke(i))
                    .ToList());
        }

    }

}
