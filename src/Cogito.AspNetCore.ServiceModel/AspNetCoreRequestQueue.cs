using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Dispatches requests to the ASP.Net Core channel listener.
    /// </summary>
    public class AspNetCoreRequestQueue :
        IDisposable
    {

        readonly Uri baseUri;
        readonly AsyncQueue<AspNetCoreRequest> buffer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="buffer"></param>
        public AspNetCoreRequestQueue(Uri baseUri)
        {
            this.baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            this.buffer = new AsyncQueue<AspNetCoreRequest>();
        }

        /// <summary>
        /// Gets the 'aspnetcore' scheme base URI this queue accepts messages for.
        /// </summary>
        public Uri BaseUri => baseUri;

        /// <summary>
        /// Closes the queue so no more requests can be processed.
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            if (!buffer.IsCompleted)
                buffer.Complete();
        }

        /// <summary>
        /// Sends a request to WCF.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task SendAsync(HttpContext context)
        {
            var request = new AspNetCoreRequest(context);
            buffer.Enqueue(request);
            await request.Task;
        }

        /// <summary>
        /// Receives the next request or waits for a timeout.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal async Task<AspNetCoreRequest> ReceiveAsync(TimeSpan timeout)
        {
            try
            {
                if (timeout > TimeSpan.FromMilliseconds(int.MaxValue))
                    timeout = TimeSpan.FromMilliseconds(int.MaxValue);

                var sw = new Stopwatch();
                sw.Start();
                var ms = await buffer.DequeueAsync(new CancellationTokenSource(timeout).Token);
                return ms;
            }
            catch (OperationCanceledException e)
            {
                throw new TimeoutException(e.Message, e);
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }

    }

}
