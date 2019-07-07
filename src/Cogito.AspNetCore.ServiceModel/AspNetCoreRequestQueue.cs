using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Dispatches requests to the ASP.Net Core channel listener.
    /// </summary>
    public class AspNetCoreRequestQueue :
        IDisposable
    {

        readonly BufferBlock<AspNetCoreRequest> buffer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="buffer"></param>
        public AspNetCoreRequestQueue()
        {
            this.buffer = new BufferBlock<AspNetCoreRequest>();
        }

        /// <summary>
        /// Closes the queue so no more requests can be processed.
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
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

            // ensure data makes it into buffer
            while (!await buffer.SendAsync(request).ConfigureAwait(false))
                await Task.Yield();

            // wait for completion of request
            await request.Task;
        }

        /// <summary>
        /// Waits until a new request is available to be processed.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal async Task<bool> WaitForRequestAsync(TimeSpan timeout)
        {
            return await buffer.OutputAvailableAsync(new CancellationTokenSource(timeout).Token).ConfigureAwait(false);
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
                var ms = await buffer.ReceiveAsync(timeout).ConfigureAwait(false);
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
