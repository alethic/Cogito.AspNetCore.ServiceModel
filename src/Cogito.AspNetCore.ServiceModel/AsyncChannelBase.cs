using System;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

using Cogito.AspNetCore.ServiceModel.Threading;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Provides an async implementation of <see cref="ChannelBase"/>.
    /// </summary>
    abstract class AsyncChannelBase :
        ChannelBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="channelManager"></param>
        protected AsyncChannelBase(ChannelManagerBase channelManager) :
            base(channelManager)
        {

        }

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
            OnOpenAsync(timeout).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        protected abstract Task OnOpenAsync(TimeSpan timeout);

        protected override sealed IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return OnCloseAsync(timeout).ToAsyncBegin(callback, state);
        }

        protected override sealed void OnEndClose(IAsyncResult result)
        {
            ((Task)result).ToAsyncEnd();
        }

        protected override sealed void OnClose(TimeSpan timeout)
        {
            OnCloseAsync(timeout).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        protected abstract Task OnCloseAsync(TimeSpan timeout);

    }

}
