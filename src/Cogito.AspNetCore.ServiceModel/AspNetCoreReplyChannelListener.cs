using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
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
        readonly SemaphoreSlim sync;

        // signal on close
        CancellationTokenSource open;

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
            this.sync = new SemaphoreSlim(5);
        }

        /// <summary>
        /// Gets the listen URI.
        /// </summary>
        public override Uri Uri => uri;

        protected override Task OnOpenAsync(TimeSpan timeout)
        {
            return Task.FromResult(true);
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            open = new CancellationTokenSource();
        }

        protected override Task OnCloseAsync(TimeSpan timeout)
        {
            return Task.FromResult(true);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            open.Cancel();
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
            if (State != CommunicationState.Opened)
                return null;
            if (open.IsCancellationRequested)
                return null;

            // maximum wait time
            if (timeout.TotalMilliseconds > int.MaxValue)
                timeout = TimeSpan.FromMilliseconds(int.MaxValue);

            // wait for reply channel to become free
            if (await sync.WaitAsync(timeout, open.Token) == false)
                throw new TimeoutException("Unable to acquire ReplyChannel within allocated time.");

            try
            {
                // generate new channel instance
                var reply = new AspNetCoreReplyChannel(
                    await router.GetQueueAsync(uri),
                    encoderFactory,
                    bufferManager,
                    new EndpointAddress(Uri),
                    this);

                // when reply channel is closed, clear it out and signal next waiter
                reply.Closed += (s, a) => { reply = null; sync.Release(); };

                return reply;
            }
            catch
            {
                sync.Release();
                throw;
            }
        }

    }

}
