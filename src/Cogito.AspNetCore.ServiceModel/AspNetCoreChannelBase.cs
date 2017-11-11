using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

using Cogito.Threading;

using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Base ASP.Net core channel functionality.
    /// </summary>
    abstract class AspNetCoreChannelBase :
        ChannelBase
    {

        const int MaxBufferSize = 64 * 1024;
        const int MaxSizeOfHeaders = 4 * 1024;
        readonly MessageEncoder encoder;

        /// <summary>
        /// 
        /// </summary>
        public EndpointAddress RemoteAddress { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bufferManager"></param>
        /// <param name="encoderFactory"></param>
        /// <param name="address"></param>
        /// <param name="channelManager"></param>
        /// <param name="maxReceivedMessageSize"></param>
        public AspNetCoreChannelBase(
            MessageEncoderFactory encoderFactory,
            EndpointAddress address,
            ChannelManagerBase channelManager) :
            base(channelManager)
        {
            this.RemoteAddress = address;
            this.encoder = encoderFactory.CreateSessionEncoder();
        }

        /// <summary>
        /// Converts an ASP.Net Core exception into an appropriate WCF exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected static Exception ConvertException(Exception exception)
        {
            var exceptionType = exception.GetType();
            // TODO figure out HTTP exceptions
            if (exceptionType == typeof(System.IO.DirectoryNotFoundException) ||
                exceptionType == typeof(System.IO.FileNotFoundException) ||
                exceptionType == typeof(System.IO.PathTooLongException))
                return new EndpointNotFoundException(exception.Message, exception);

            return new CommunicationException(exception.Message, exception);
        }

        protected override void OnAbort()
        {

        }

        Task OnOpenAsync(TimeSpan timeout)
        {
            return Task.FromResult(true);
        }

        Task OnCloseAsync(TimeSpan timeout)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Reads a <see cref="Message"/> from an incoming request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected Message ReadRequest(HttpContext context)
        {
            return encoder.ReadMessage(context.Request.Body, MaxSizeOfHeaders, context.Request.ContentType);
        }

        /// <summary>
        /// Writes a <see cref="Message"/> to the outgoing response.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        protected void WriteResponse(HttpContext context, Message message)
        {
            encoder.WriteMessage(message, context.Response.Body);
        }

        #region APM Methods

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return OnOpenAsync(timeout).ToAsyncBegin(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ((Task)result).ToAsyncEnd();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            OnOpenAsync(timeout).Wait();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return OnCloseAsync(timeout).ToAsyncBegin(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ((Task)result).ToAsyncEnd();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            OnCloseAsync(timeout).Wait();
        }

        #endregion

    }

}
