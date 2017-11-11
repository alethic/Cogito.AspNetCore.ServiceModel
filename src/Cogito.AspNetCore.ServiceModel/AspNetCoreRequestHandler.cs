using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Dispatches requests to the ASP.Net Core channel listener.
    /// </summary>
    public class AspNetCoreRequestHandler
    {

        readonly BufferBlock<AspNetCoreRequest> buffer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="buffer"></param>
        public AspNetCoreRequestHandler()
        {
            buffer = new BufferBlock<AspNetCoreRequest>();
        }

        /// <summary>
        /// Sends a request to WCF.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task SendAsync(HttpContext context)
        {
            var request = new AspNetCoreRequest(context);
            await buffer.SendAsync(request);
            await request.Task;
        }

        /// <summary>
        /// Receives the next request or waits for a timeout.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal Task<AspNetCoreRequest> ReceiveAsync(TimeSpan timeout)
        {
            return buffer.ReceiveAsync(timeout);
        }

        /// <summary>
        /// Waits for a request to become available.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal Task<bool> WaitAsync(TimeSpan timeout)
        {
            return buffer.OutputAvailableAsync(new CancellationTokenSource(timeout).Token);
        }

    }

}
