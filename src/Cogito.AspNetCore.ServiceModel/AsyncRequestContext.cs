using System;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

using Cogito.Threading;

namespace Cogito.ServiceModel.AspNetCore
{

    /// <summary>
    /// Provides async operations for a <see cref="RequestContext" />.
    /// </summary>
    abstract class AsyncRequestContext :
        RequestContext
    {

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            return ReplyAsync(message).ToAsyncBegin(callback, state);
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReplyAsync(message, timeout).ToAsyncBegin(callback, state);
        }

        public override void EndReply(IAsyncResult result)
        {
            ((Task)result).ToAsyncEnd();
        }

        public override void Reply(Message message)
        {
            ReplyAsync(message).Wait();
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            ReplyAsync(message, timeout).Wait();
        }

        protected abstract Task ReplyAsync(Message message);

        protected abstract Task ReplyAsync(Message message, TimeSpan timeout);

    }

}
