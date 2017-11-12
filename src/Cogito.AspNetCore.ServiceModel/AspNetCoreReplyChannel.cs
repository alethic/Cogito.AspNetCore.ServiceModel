using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using Cogito.IO;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Reflection;

namespace Cogito.AspNetCore.ServiceModel
{

    class AspNetCoreReplyChannel :
        AsyncReplyChannelBase
    {

        /// <summary>
        /// Returns <c>true</c> if the given <see cref="MessageEncoder"/> is a MTOM encoder.
        /// </summary>
        /// <param name="encoder"></param>
        /// <returns></returns>
        static bool IsMtomEncoder(MessageEncoder encoder)
        {
            if (encoder == null)
                throw new ArgumentNullException(nameof(encoder));

            return encoder.GetType().Name == "MtomMessageEncoder";
        }

        /// <summary>
        /// Gets the method info for GetContentType on the MTOM encoder.
        /// </summary>
        /// <param name="encoder"></param>
        /// <returns></returns>
        static MethodInfo GetMtomGetContentTypeMethod(MessageEncoder encoder)
        {
            if (encoder == null)
                throw new ArgumentNullException(nameof(encoder));

            var method = encoder.GetType().GetMethod("GetContentType", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
                throw new MissingMethodException("Could not find the MtomMessageEncoder.GetContentType method");

            return method;
        }

        /// <summary>
        /// Invokes GetContentType on the MTOM encoder.
        /// </summary>
        /// <param name="encoder"></param>
        /// <returns></returns>
        static string GetMtomContentType(MessageEncoder encoder, out string boundary)
        {
            if (encoder == null)
                throw new ArgumentNullException(nameof(encoder));

            // invoke and collect result
            var arg = new object[] { null };
            var ret = (string)GetMtomGetContentTypeMethod(encoder)?.Invoke(encoder, arg);
            boundary = (string)arg[0];
            return ret;
        }

        const int MaxBufferSize = 64 * 1024;
        const int MaxSizeOfHeaders = 4 * 1024;

        readonly AspNetCoreRequestQueue queue;
        readonly MessageEncoder encoder;
        readonly BufferManager bufferManager;
        readonly EndpointAddress localAddress;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="encoderFactory"></param>
        /// <param name="localAddress"></param>
        /// <param name="channelManager"></param>
        public AspNetCoreReplyChannel(
            AspNetCoreRequestQueue queue,
            MessageEncoderFactory encoderFactory,
            BufferManager bufferManager,
            EndpointAddress localAddress,
            AspNetCoreReplyChannelListener parent) :
            base(parent)
        {
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            this.encoder = encoderFactory.CreateSessionEncoder();
            this.bufferManager = bufferManager ?? throw new ArgumentNullException(nameof(bufferManager));
            this.localAddress = localAddress ?? throw new ArgumentNullException(nameof(localAddress));
        }

        /// <summary>
        /// The address on which this reply channel receives messages.
        /// </summary>
        public override EndpointAddress LocalAddress => localAddress;

        /// <summary>
        /// Opens the channel.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected override Task OnOpenAsync(TimeSpan timeout)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Closes the channel.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected override Task OnCloseAsync(TimeSpan timeout)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Aborts the channel.
        /// </summary>
        protected override void OnAbort()
        {

        }

        /// <summary>
        /// Returns <c>true</c> if a request is currently available.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected override Task<bool> WaitForRequestAsync(TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();
            return Task.FromResult(true);
        }

        /// <summary>
        /// Normalizes a timeout.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        CancellationToken FromTimeOut(TimeSpan timeout)
        {
            return new CancellationTokenSource(TimeSpan.FromMilliseconds(System.Math.Min(int.MaxValue, timeout.TotalMilliseconds))).Token;
        }

        /// <summary>
        /// Attempts to dequeue an incoming request.
        /// </summary>
        /// <returns></returns>
        protected override Task<RequestContext> ReceiveRequestAsync()
        {
            return ReceiveRequestAsync(DefaultReceiveTimeout);
        }

        /// <summary>
        /// Attempts to dequeue an incoming request.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected override async Task<RequestContext> ReceiveRequestAsync(TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();

            try
            {
                // wait for next request
                var request = await queue.ReceiveAsync(timeout);
                if (request == null)
                    throw new CommunicationException("No request received.");

                try
                {
                    ThrowIfDisposedOrNotOpen();

                    return new AspNetCoreRequestContext(
                        request,
                        await ReadMessageAsync(request.Context.Request),
                        this);
                }
                catch (CommunicationException e)
                {
                    request.TrySetException(e);
                    throw;
                }
                catch (Exception e)
                {
                    request.TrySetException(e);
                    throw new CommunicationException(e.Message, e);
                }
            }
            catch (CommunicationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CommunicationException(e.Message, e);
            }
        }

        /// <summary>
        /// Sends the reply message by writing it back to the HTTP response.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal async Task ReplyAsync(Message message, AspNetCoreRequest request, TimeSpan timeout)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // write the response body
            try
            {
                ThrowIfDisposedOrNotOpen();
                await WriteMessageAsync(request.Context.Response, message);
                request.TrySetResult(true);
            }
            catch (CommunicationException e)
            {
                request.TrySetException(e);
                throw;
            }
            catch (Exception e)
            {
                request.TrySetException(e);
                throw new CommunicationException(e.Message, e);
            }
        }

        /// <summary>
        /// Describes a completely empty message.
        /// </summary>
        class NullMessage :
            Message
        {

            public override MessageHeaders Headers { get; } = new MessageHeaders(MessageVersion.None);

            public override MessageProperties Properties { get; } = new MessageProperties();

            public override MessageVersion Version => Headers.MessageVersion;

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                writer.WriteElementString("BODY", "");
            }

            protected override void OnBodyToString(XmlDictionaryWriter writer)
            {
                OnWriteBodyContents(writer);
            }

        }

        /// <summary>
        /// Reads and processes a message from the given request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        async Task<Message> ReadMessageAsync(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentException(nameof(request));

            var message = await ReadMessageBodyAsync(request);
            if (message != null)
                ReadMessageAddressing(request, message);

            return message;
        }

        /// <summary>
        /// Reads a message from the given request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        async Task<Message> ReadMessageBodyAsync(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentException(nameof(request));

            try
            {
                // SOAP response message exchange pattern
                if (request.Method == "GET")
                    return new NullMessage();

                if (string.IsNullOrEmpty(request.ContentType))
                    throw new ProtocolException("Content-Type header required.");

                // check that encoder supports this content type
                if (encoder.IsContentTypeSupported(request.ContentType) == false)
                    throw new ProtocolException($"Content-Type mismatch: '{request.ContentType}' != {encoder.ContentType}'");

                // allow multiple reads from the request
                request.EnableRewind();

                // return appropriate message from body
                if (request.Body.Length == 0 &&
                    encoder.MessageVersion == MessageVersion.None)
                    return new NullMessage();
                else
                {
                    // buffer data
                    var raw = await request.Body.ReadAllBytesAsync();
                    var dat = bufferManager.TakeBuffer(raw.Length);
                    Buffer.BlockCopy(raw, 0, dat, 0, raw.Length);

                    // read message from buffer
                    var buf = new ArraySegment<byte>(dat, 0, raw.Length);
                    var msg = encoder.ReadMessage(buf, bufferManager, request.ContentType);

                    // return final message
                    return msg;
                }
            }
            catch (XmlException e)
            {
                throw new ProtocolException(e.Message, e);
            }
        }

        /// <summary>
        /// Process addressing information from HTTP.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        void ReadMessageAddressing(HttpRequest request, Message message)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // apply properties
            ReadProperties(request, message);

            // decide on the default port based on the schema
            int DefaultPort()
            {
                switch (request.Scheme)
                {
                    case "http":
                        return 80;
                    case "https":
                        return 443;
                    default:
                        return 80;
                }
            }

            // encode raw request URL into Via
            message.Properties.Via = new UriBuilder(
                    request.Scheme,
                    request.Host.Host,
                    request.Host.Port ?? DefaultPort(),
                    request.PathBase + request.Path,
                    request.QueryString.Value)
                .Uri;

            // check addressing version none requirements
            if (message.Version.Addressing == AddressingVersion.None)
            {
                try
                {
                    if (message.Headers.Action != null)
                        throw new ProtocolException("Action present when not expected.");
                    if (message.Headers.To != null)
                        throw new ProtocolException("To present when not expected.");
                }
                catch (XmlException)
                {
                    // handled
                }
                catch (CommunicationException)
                {
                    // handled
                }

                // override message to with ASP.Net core URI
                message.Headers.To = AspNetCoreUri.GetUri(request.HttpContext);
            }

            // derive SOAP action
            var action = ReadSoapAction(request, message);
            if (action != null)
            {
                action = WebUtility.UrlDecode(action);

                if (action.Length >= 2 && action[0] == '"' && action[action.Length - 1] == '"')
                    action = action.Substring(1, action.Length - 2);

                if (message.Version.Addressing == AddressingVersion.None)
                    message.Headers.Action = action;

                // check that action isn't mismatched
                if (action.Length > 0 && string.Compare(message.Headers.Action, action, StringComparison.Ordinal) != 0)
                    throw new ProtocolException($"Mismatched action: '{message.Headers.Action}' != '{action}'");
            }
        }

        /// <summary>
        /// Adds the required properties to the message.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="message"></param>
        void ReadProperties(HttpRequest request, Message message)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // existing WCF HTTP request property
            var p1 = new HttpRequestMessageProperty();
            p1.Method = request.Method;
            p1.QueryString = request.QueryString.ToString();
            foreach (var header in request.Headers)
                p1.Headers.Add(header.Key, header.Value);
            message.Properties.Add(HttpRequestMessageProperty.Name, p1);

            // new ASP.Net core property
            var p2 = new AspNetCoreMessageProperty();
            p2.Context = request.HttpContext;
            message.Properties.Add(AspNetCoreMessageProperty.Name, p2);
        }

        /// <summary>
        /// Gets the SOAP action for the given request and message.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        string ReadSoapAction(HttpRequest request, Message message)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.Version.Envelope == EnvelopeVersion.Soap11)
                if (request.Headers.TryGetValue("SOAPAction", out var value))
                    return value.FirstOrDefault();

            if (message.Version.Envelope == EnvelopeVersion.Soap12 && !string.IsNullOrEmpty(request.ContentType))
            {
                var contentType = new ContentType(request.ContentType);
                if (contentType.MediaType == "multipart/related" &&
                    contentType.Parameters.ContainsKey("start-info"))
                    // action in start-info as defined in RFC2387
                    return new ContentType(contentType.Parameters["start-info"]).Parameters["action"];

                // default location for action
                return contentType.Parameters["action"];
            }

            return null;
        }

        /// <summary>
        /// Writes a <see cref="Message"/> to the outgoing response.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="message"></param>
        async Task WriteMessageAsync(HttpResponse response, Message message)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var action = message.Headers.Action;

            if (message.Version.Addressing == AddressingVersion.None)
            {
                message.Headers.Action = null;
                message.Headers.To = null;
            }

            // default values
            response.ContentType = encoder.ContentType;
            response.StatusCode = !message.IsFault ? 200 : (int)HttpStatusCode.InternalServerError;

            // this might need some handholding, like a custom MtomEncoder since too much is internal
            if (IsMtomEncoder(encoder))
            {
                response.ContentType = GetMtomContentType(encoder, out var boundary);
                response.Headers.Add("MIME-Version", "1.0");
            }

            // HEAD requests require no actual content
            if (response.HttpContext.Request.Method == "HEAD")
            {
                response.ContentLength = null;
                response.ContentType = null;
            }

            if (message.IsEmpty == false)
                await WriteMessageBodyAsync(response, message);
        }

        /// <summary>
        /// Writes a message body to the outgoing response.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task WriteMessageBodyAsync(HttpResponse response, Message message)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            await Task.Factory.FromAsync(encoder.BeginWriteMessage, encoder.EndWriteMessage, message, response.Body, null);
        }

    }

}
