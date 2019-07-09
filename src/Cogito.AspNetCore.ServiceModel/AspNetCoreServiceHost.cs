using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;

using Cogito.Reflection;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Private <see cref="ServiceHost"/> implementation.
    /// </summary>
    class AspNetCoreServiceHost : ServiceHost
    {

        readonly Func<Type, IEnumerable<ServiceEndpoint>> addDefaultEndpoints;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="addDefaultEndpoints"></param>
        /// <param name="httpBaseAddress"></param>
        /// <param name="httpsBaseAddress"></param>
        public AspNetCoreServiceHost(Type serviceType, Func<Type, IEnumerable<ServiceEndpoint>> addDefaultEndpoints, Uri httpBaseAddress, Uri httpsBaseAddress) :
            base(serviceType, new[] { httpBaseAddress, httpsBaseAddress })
        {
            this.addDefaultEndpoints = addDefaultEndpoints ?? throw new ArgumentNullException(nameof(addDefaultEndpoints));
        }

        /// <summary>
        /// Adds service endpoints for all base addresses in each contract found in the service host with the default bindings.
        /// </summary>
        /// <returns></returns>
        public override ReadOnlyCollection<ServiceEndpoint> AddDefaultEndpoints()
        {
            var t = Description.ServiceType.GetAssignableTypes().Where(i => i.GetCustomAttribute<ServiceContractAttribute>() != null);
            var c = t.Where(i => !t.Any(j => i != j && i.IsAssignableFrom(j))).ToArray();
            var e = c.SelectMany(i => addDefaultEndpoints(i)).ToList();
            return new ReadOnlyCollection<ServiceEndpoint>(e);
        }

    }

}
