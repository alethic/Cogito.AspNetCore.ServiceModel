using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Cogito.AspNetCore.ServiceModel
{

    class AspNetCoreReplyChannelListener :
        AsyncChannelListenerBase<IReplyChannel>
    {

        readonly AspNetCoreRequestQueue queue;
        readonly BufferManager bufferManager;
        readonly MessageEncoderFactory encoderFactory;
        readonly Uri uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="transportElement"></param>
        /// <param name="context"></param>
        public AspNetCoreReplyChannelListener(
            AspNetCoreRequestQueue queue,
            AspNetCoreTransportBindingElement transportElement,
            BindingContext context) :
            base(context.Binding)
        {
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            this.bufferManager = BufferManager.CreateBufferManager(transportElement.MaxBufferPoolSize, (int)transportElement.MaxReceivedMessageSize);
            this.encoderFactory = context.BindingParameters.Remove<MessageEncodingBindingElement>().CreateMessageEncoderFactory();
            this.uri = new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
        }

        /// <summary>
        /// Gets the listen URI.
        /// </summary>
        public override Uri Uri => uri;

        protected override Task OnOpenAsync(TimeSpan timeout)
        {
            return Task.FromResult(true);
        }

        protected override Task OnCloseAsync(TimeSpan timeout)
        {
            return Task.FromResult(true);
        }

        protected override void OnAbort()
        {

        }

        protected override Task<bool> OnWaitForChannelAsync(TimeSpan timeout)
        {
            return Task.FromResult(true);
        }

        protected override async Task<IReplyChannel> OnAcceptChannelAsync(TimeSpan timeout)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            return new AspNetCoreReplyChannel(
                queue,
                encoderFactory,
                bufferManager,
                new EndpointAddress(Uri),
                this);
        }

    }

}
