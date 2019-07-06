using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

using Cogito.AspNetCore.ServiceModel.Threading;

namespace Cogito.AspNetCore.ServiceModel
{

    abstract class AsyncReplyChannelBase :
        AsyncChannelBase,
        IReplyChannel
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="channelManager"></param>
        protected AsyncReplyChannelBase(ChannelManagerBase channelManager) :
            base(channelManager)
        {

        }

        /// <summary>
        /// The address on which this <see cref="IReplyChannel"/> receives messages.
        /// </summary>
        public abstract EndpointAddress LocalAddress { get; }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return ReceiveRequestAsync().ToAsyncBegin(callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReceiveRequestAsync(timeout).ToAsyncBegin(callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            return ((Task<RequestContext>)result).ToAsyncEnd();
        }

        public RequestContext ReceiveRequest()
        {
            return ReceiveRequestAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            return ReceiveRequestAsync(timeout).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        protected abstract Task<RequestContext> ReceiveRequestAsync();

        protected abstract Task<RequestContext> ReceiveRequestAsync(TimeSpan timeout);

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            return (context = ReceiveRequest(timeout)) != null;
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
            return ((Task<bool>)result).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            return WaitForRequestAsync(timeout).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        protected abstract Task<bool> WaitForRequestAsync(TimeSpan timeout);

    }

}
