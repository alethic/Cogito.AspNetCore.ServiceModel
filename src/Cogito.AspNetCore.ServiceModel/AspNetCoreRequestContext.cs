using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Cogito.ServiceModel.AspNetCore
{

    class AspNetCoreRequestContext :
        AsyncRequestContext
    {

        readonly AspNetCoreRequest request;
        readonly AspNetCoreReplyChannel reply;
        readonly Message message;

        public override Message RequestMessage
        {
            get { return message; }
        }

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

        protected override Task ReplyAsync(Message message, TimeSpan timeout)
        {
            if (request.Context.RequestAborted.IsCancellationRequested)
                throw new CommunicationObjectAbortedException();

            return reply.ReplyAsync(message, request, timeout);
        }

        protected override Task ReplyAsync(Message message)
        {
            return ReplyAsync(message, TimeSpan.MaxValue);
        }

    }

}
