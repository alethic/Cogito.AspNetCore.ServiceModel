using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

using Cogito.Threading;

namespace Cogito.AspNetCore.ServiceModel
{

    class AspNetCoreReplyChannelListener :
        ChannelListenerBase<IReplyChannel>
    {

        readonly Uri uri;
        readonly MessageEncoderFactory encoderFactory;
        readonly AspNetCoreReplyChannel reply;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="transportElement"></param>
        /// <param name="context"></param>
        public AspNetCoreReplyChannelListener(
            AspNetCoreRequestHandler incoming,
            AspNetCoreTransportBindingElement transportElement,
            BindingContext context) :
            base(context.Binding)
        {
            this.uri = new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
            var messageElement = context.BindingParameters.Remove<MessageEncodingBindingElement>();
            this.encoderFactory = messageElement.CreateMessageEncoderFactory();
            this.reply = new AspNetCoreReplyChannel(incoming, encoderFactory, new EndpointAddress(Uri), this);
        }

        /// <summary>
        /// Gets the listen URI.
        /// </summary>
        public override Uri Uri => uri;

        protected override void OnOpen(TimeSpan timeout)
        {

        }

        protected override void OnClose(TimeSpan timeout)
        {

        }

        protected override IReplyChannel OnAcceptChannel(TimeSpan timeout)
        {
            return OnAcceptChannelAsync(timeout).Result;
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return OnAcceptChannelAsync(timeout).ToAsyncBegin(callback, state);
        }

        protected override IReplyChannel OnEndAcceptChannel(IAsyncResult result)
        {
            return ((Task<IReplyChannel>)result).ToAsyncEnd();
        }

        Task<IReplyChannel> OnAcceptChannelAsync(TimeSpan timeout)
        {
            return Task.FromResult((IReplyChannel)reply);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        protected override void OnAbort()
        {
            throw new NotImplementedException();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

    }

}
