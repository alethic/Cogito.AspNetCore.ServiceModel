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
        readonly bool secure;
        readonly string method;

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

            bufferManager = BufferManager.CreateBufferManager(transportElement.MaxBufferPoolSize, (int)transportElement.MaxReceivedMessageSize);
            encoderFactory = context.BindingParameters.Remove<MessageEncodingBindingElement>().CreateMessageEncoderFactory();
            uri = new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
            secure = transportElement.Secure;
            method = transportElement.Method;
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

        protected override async Task<bool> OnWaitForChannelAsync(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds > int.MaxValue)
                timeout = TimeSpan.FromMilliseconds(int.MaxValue);

            using (var lease = router.Acquire(secure, method))
                return await lease.Queue.WaitForRequestAsync(timeout);
        }

        protected override async Task<IReplyChannel> OnAcceptChannelAsync(TimeSpan timeout)
        {
            if (State != CommunicationState.Opened)
                return null;
            if (open.IsCancellationRequested)
                return null;

            if (timeout.TotalMilliseconds > int.MaxValue)
                timeout = TimeSpan.FromMilliseconds(int.MaxValue);

            // acquire a lease on the queue
            using (var lease = router.Acquire(secure, method))
            {
                // wait until a new request is available
                var request = await lease.Queue.ReceiveAsync(timeout).ConfigureAwait(false);
                if (request == null)
                    return null;

                // generate new channel instance
                return new AspNetCoreReplyChannel(
                    request,
                    router.Acquire(secure, method),
                    encoderFactory,
                    bufferManager,
                    new EndpointAddress(Uri),
                    this);
            }
        }

    }

}
