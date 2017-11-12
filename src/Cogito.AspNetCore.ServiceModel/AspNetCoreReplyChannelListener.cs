using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace Cogito.AspNetCore.ServiceModel
{

    class AspNetCoreReplyChannelListener :
        AsyncChannelListenerBase<IReplyChannel>
    {

        readonly AspNetCoreRequestRouter router;
        readonly BufferManager bufferManager;
        readonly MessageEncoderFactory encoderFactory;
        readonly Uri uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="router"></param>
        /// <param name="transportElement"></param>
        /// <param name="context"></param>
        public AspNetCoreReplyChannelListener(
            AspNetCoreRequestRouter router,
            AspNetCoreTransportBindingElement transportElement,
            BindingContext context) :
            base(context.Binding)
        {
            this.router = router ?? throw new ArgumentNullException(nameof(router));
            this.bufferManager = BufferManager.CreateBufferManager(transportElement.MaxBufferPoolSize, (int)transportElement.MaxReceivedMessageSize);
            this.encoderFactory = context.BindingParameters.Remove<MessageEncodingBindingElement>().CreateMessageEncoderFactory();
            this.uri = AspNetCoreUri.GetUri(context.ListenUriBaseAddress.AbsolutePath + context.ListenUriRelativeAddress);
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
            await Task.Delay(TimeSpan.FromSeconds(1));
            return new AspNetCoreReplyChannel(
                await router.GetQueueAsync(uri),
                encoderFactory,
                bufferManager,
                new EndpointAddress(Uri),
                this);
        }

    }

}
