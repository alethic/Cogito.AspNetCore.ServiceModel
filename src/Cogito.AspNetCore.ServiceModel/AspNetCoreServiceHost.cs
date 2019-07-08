using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Returns all base types implemented by this type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<Type> TraverseBaseType(Type type)
        {
            var t = type;
            do
            {
                yield return t;
                t = t.BaseType;
            }
            while (t != null);
        }

        ContractDescription TryGetContractDescription(Type t)
        {
            try
            {
                return ContractDescription.GetContract(t);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

        }

        public override ReadOnlyCollection<ServiceEndpoint> AddDefaultEndpoints()
        {
            // find a list of all implemented types
            var a = Description.ServiceType.GetInterfaces();
            var b = TraverseBaseType(Description.ServiceType);
            var l = a.Concat(b).Distinct().Select(i => TryGetContractDescription(i)).Where(i => i != null).ToList();

            var o = new List<Type>();

            for (var i = 0; i < l.Count; i++)
            {
                var keep = true;

                for (var j = 0; j < l.Count; j++)
                {
                    if (i == j)
                        continue;

                    var n = l[i].ContractType;
                    var m = l[j].ContractType;
                    if (n.IsAssignableFrom(m))
                    {
                        keep = false;
                        break;
                    }
                }

                if (keep)
                    o.Add(l[i].ContractType);
            }

            return new ReadOnlyCollection<ServiceEndpoint>(o.SelectMany(i => addDefaultEndpoints?.Invoke(i)).ToList());
        }

    }

}
