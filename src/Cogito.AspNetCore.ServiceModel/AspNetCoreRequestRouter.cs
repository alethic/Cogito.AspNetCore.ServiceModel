using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Accepts requests from the ASP.Net Core pipeline and routes them to a listening queue.
    /// </summary>
    public class AspNetCoreRequestRouter
    {

        readonly ConcurrentDictionary<Uri, AspNetCoreRequestQueue> queues;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspNetCoreRequestRouter()
        {
            this.queues = new ConcurrentDictionary<Uri, AspNetCoreRequestQueue>();
        }

        /// <summary>
        /// Gets the queue for the given routing URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        AspNetCoreRequestQueue GetOrAdd(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (uri.Scheme != AspNetCoreUri.Scheme)
                throw new ArgumentException($"Routing URI scheme must be {AspNetCoreUri.Scheme}.", nameof(uri));

            return queues.GetOrAdd(uri, _ => new AspNetCoreRequestQueue(_));
        }

        /// <summary>
        /// Gets the queue for the given routing URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        AspNetCoreRequestQueue Get(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (uri.Scheme != AspNetCoreUri.Scheme)
                throw new ArgumentException($"Routing URI scheme must be {AspNetCoreUri.Scheme}.", nameof(uri));

            return queues.TryGetValue(uri, out var queue) ? queue : null;
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
            var queue = Get(AspNetCoreUri.GetUri(context));
            if (queue == null)
                throw new AspNetCoreServiceModelException($"Unable to route request '{context.Request.Path}'. No registered listener.");

            // route to queue and wait for request to be handled
            await queue.SendAsync(context);
        }

        /// <summary>
        /// Gets the queue for the given base URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal Task<AspNetCoreRequestQueue> GetQueueAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return Task.FromResult(GetOrAdd(uri));
        }

    }

}
