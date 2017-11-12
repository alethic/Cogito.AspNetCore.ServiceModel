using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

using Cogito.Threading;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Provides as async implementation of <see cref="ChannelListenerBase{TChannel}"/>.
    /// </summary>
    /// <typeparam name="TChannel"></typeparam>
    abstract class AsyncChannelListenerBase<TChannel> :
        ChannelListenerBase<TChannel>
        where TChannel : class, IChannel
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected AsyncChannelListenerBase()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="timeouts"></param>
        protected AsyncChannelListenerBase(IDefaultCommunicationTimeouts timeouts) : 
            base(timeouts)
        {

        }

        protected override sealed IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return OnAcceptChannelAsync(timeout).ToAsyncBegin(callback, state);
        }

        protected override sealed TChannel OnEndAcceptChannel(IAsyncResult result)
        {
            return ((Task<TChannel>)result).ToAsyncEnd();
        }

        protected override sealed TChannel OnAcceptChannel(TimeSpan timeout)
        {
            return OnAcceptChannelAsync(timeout).Result;
        }

        protected abstract Task<TChannel> OnAcceptChannelAsync(TimeSpan timeout);

        protected override sealed IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return OnWaitForChannelAsync(timeout).ToAsyncBegin(callback, state);
        }

        protected override sealed bool OnEndWaitForChannel(IAsyncResult result)
        {
            return ((Task<bool>)result).ToAsyncEnd();
        }

        protected override sealed bool OnWaitForChannel(TimeSpan timeout)
        {
            return OnWaitForChannelAsync(timeout).Result;
        }

        protected abstract Task<bool> OnWaitForChannelAsync(TimeSpan timeout);

        protected override sealed IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return OnOpenAsync(timeout).ToAsyncBegin(callback, state);
        }

        protected override sealed void OnEndOpen(IAsyncResult result)
        {
            ((Task)result).ToAsyncEnd();
        }

        protected override sealed void OnOpen(TimeSpan timeout)
        {
            OnOpenAsync(timeout).Wait();
        }

        protected abstract Task OnOpenAsync(TimeSpan timeout);

        protected override sealed IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return OnCloseAsync(timeout).ToAsyncBegin(callback, state);
        }

        protected override sealed void OnEndClose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        protected override sealed void OnClose(TimeSpan timeout)
        {
            OnCloseAsync(timeout).Wait();
        }

        protected abstract Task OnCloseAsync(TimeSpan timeout);

    }

}
