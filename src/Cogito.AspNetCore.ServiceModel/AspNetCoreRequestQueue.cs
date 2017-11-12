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

        readonly AsyncQueue<AspNetCoreRequest> buffer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="buffer"></param>
        public AspNetCoreRequestQueue()
        {
            buffer = new AsyncQueue<AspNetCoreRequest>();
        }

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
                Console.WriteLine(sw.Elapsed);
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
