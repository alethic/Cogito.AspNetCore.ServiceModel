using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

using Cogito.Threading;

using Microsoft.AspNetCore.Http;

namespace Cogito.ServiceModel.AspNetCore
{

    class AspNetCoreReplyChannel :
        AspNetCoreChannelBase,
        IReplyChannel
    {

        readonly AspNetCoreRequestHandler requests;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="encoderFactory"></param>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        public AspNetCoreReplyChannel(
            AspNetCoreRequestHandler incoming,
            MessageEncoderFactory encoderFactory,
            EndpointAddress address,
            AspNetCoreReplyChannelListener parent) :
            base(encoderFactory, address, parent)
        {
            this.requests = incoming;
            LocalAddress = address;
        }

        public EndpointAddress LocalAddress { get; }

        /// <summary>
        /// Attempts to dequeue an incoming request.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        async Task<RequestContext> ReceiveRequestAsync(TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();

            try
            {
                var request = await requests.ReceiveAsync(timeout);
                return new AspNetCoreRequestContext(request, ReadRequest(request.Context), this);
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        /// <summary>
        /// Sends the reply message by writing it back to the HTTP response.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal Task ReplyAsync(Message message, AspNetCoreRequest request, TimeSpan timeout)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            WriteResponse(request.Context, message);
            request.TrySetResult(true);
            return Task.FromResult(true);
        }

        /// <summary>
        /// Returns <c>true</c> if a request is currently available.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<bool> WaitForRequestAsync(TimeSpan timeout)
        {
            return requests.WaitAsync(timeout);
        }

        #region APM Methods

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReceiveRequestAsync(timeout).ToAsyncBegin(callback, state);
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return ReceiveRequestAsync(DefaultReceiveTimeout).ToAsyncBegin(callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            return ((Task<RequestContext>)result).ToAsyncEnd();
        }

        public RequestContext ReceiveRequest()
        {
            return ReceiveRequest(DefaultReceiveTimeout);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            return ReceiveRequestAsync(timeout).Result;
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            return (context = ReceiveRequestAsync(timeout).Result) != null;
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReceiveRequestAsync(timeout).ToAsyncBegin(callback, state);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            return (context = ((Task<RequestContext>)result).ToAsyncEnd()) != null;
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return WaitForRequestAsync(timeout).ToAsyncBegin(callback, state);
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return ((Task<bool>)result).Result;
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();

            return WaitForRequestAsync(timeout).Result;
        }

        #endregion

    }

}
