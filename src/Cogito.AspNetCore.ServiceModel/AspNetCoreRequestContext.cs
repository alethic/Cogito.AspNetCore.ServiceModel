using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace Cogito.AspNetCore.ServiceModel
{

    class AspNetCoreRequestContext :
        AsyncRequestContext
    {

        readonly AspNetCoreRequest request;
        readonly AspNetCoreReplyChannel reply;
        readonly Message message;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="message"></param>
        /// <param name="reply"></param>
        public AspNetCoreRequestContext(AspNetCoreRequest request, Message message, AspNetCoreReplyChannel reply)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
            this.message = message ?? throw new ArgumentNullException(nameof(message));
            this.reply = reply ?? throw new ArgumentNullException(nameof(reply));
        }

        /// <summary>
        /// Gets the message associated with the request.
        /// </summary>
        public override Message RequestMessage => message;

        public override void Abort()
        {
            request.Context.Abort();
        }

        public override void Close(TimeSpan timeout)
        {
            
        }

        public override void Close()
        {

        }

        protected override async Task ReplyAsync(Message message, TimeSpan timeout)
        {
            if (request.Context.RequestAborted.IsCancellationRequested)
                throw new CommunicationObjectAbortedException();

            await reply.ReplyAsync(message, request, timeout);
        }

        protected override async Task ReplyAsync(Message message)
        {
            await ReplyAsync(message, Timeout.InfiniteTimeSpan);
        }

    }

}
