using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Cogito.AspNetCore.ServiceModel.Collections;

using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Accepts requests from the ASP.Net Core pipeline and routes them to a listening queue.
    /// </summary>
    class AspNetCoreRequestRouter
    {

        readonly Dictionary<(bool Secure, string Method), AspNetCoreRequestQueueResource> indexedQueues;
        readonly Dictionary<bool, AspNetCoreRequestQueueResource> defaultQueues;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspNetCoreRequestRouter()
        {
            this.indexedQueues = new Dictionary<(bool Secure, string Method), AspNetCoreRequestQueueResource>();
            this.defaultQueues = new Dictionary<bool, AspNetCoreRequestQueueResource>();
        }

        /// <summary>
        /// Acquires a lease on the given routing queue, optionally by HTTP method.
        /// </summary>
        /// <param name="secure"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public AspNetCoreRequestQueueLease Acquire(bool secure, string method = null)
        {
            if (method != null)
                lock (indexedQueues)
                    return indexedQueues.GetOrAdd((secure, method), _ => CreateQueueResource(indexedQueues, _)).Acquire();
            else
                lock (defaultQueues)
                    return defaultQueues.GetOrAdd(secure, _ => CreateQueueResource(defaultQueues, _)).Acquire();
        }

        /// <summary>
        /// Creates a new queue resource for the given HTTP method.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        AspNetCoreRequestQueueResource CreateQueueResource<TKey>(IDictionary<TKey, AspNetCoreRequestQueueResource> map, TKey key)
        {
            var resource = new AspNetCoreRequestQueueResource(new AspNetCoreRequestQueue());
            resource.Complete += (s, a) => { lock (map) { map.Remove(key); }; };
            return resource;
        }

        /// <summary>
        /// Routes the request to the appropriate queue and initiates a blockig send for completion.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task RunAsync(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // acquire registered queue
            var queue = GetIndexedQueue(context.Request.IsHttps, context.Request.Method) ?? GetDefaultQueue(context.Request.IsHttps);
            if (queue == null)
                throw new AspNetCoreServiceModelException($"Unable to route request '{context.Request.Path}'. No registered listener queues.");

            // route to queue and wait for request to be handled
            await queue.SendAsync(context).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the indexed queue by the specified parameters.
        /// </summary>
        /// <param name="secure"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        AspNetCoreRequestQueue GetIndexedQueue(bool secure, string method)
        {
            lock (indexedQueues)
                return indexedQueues.GetOrDefault((secure, method))?.Queue;
        }

        /// <summary>
        /// Gets the default queue by the specified parameters.
        /// </summary>
        /// <param name="secure"></param>
        /// <returns></returns>
        AspNetCoreRequestQueue GetDefaultQueue(bool secure)
        {
            lock (defaultQueues)
                return defaultQueues.GetOrDefault(secure)?.Queue;
        }

    }

}
