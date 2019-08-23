using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Represents a Message Encoder that supports both text and MTOM depending on the input message.
    /// </summary>
    class TextOrMtomEncoder : MessageEncoder
    {

        readonly MessageEncoder text;
        readonly MessageEncoder mtom;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public TextOrMtomEncoder(MessageVersion messageVersion, XmlDictionaryReaderQuotas readerQuotas, int maxReadPoolSize, int maxWritePoolSize, int maxBufferSize)
        {
            if (messageVersion is null)
                throw new ArgumentNullException(nameof(messageVersion));
            if (readerQuotas is null)
                throw new ArgumentNullException(nameof(readerQuotas));

            var textEncoderBindingElement = new TextMessageEncodingBindingElement(messageVersion, Encoding.UTF8);
            textEncoderBindingElement.MaxReadPoolSize = maxReadPoolSize;
            textEncoderBindingElement.MaxWritePoolSize = maxWritePoolSize;
            readerQuotas.CopyTo(textEncoderBindingElement.ReaderQuotas);
            text = textEncoderBindingElement.CreateMessageEncoderFactory().Encoder;

            if (messageVersion.Envelope != EnvelopeVersion.None)
            {
                var mtomEncoderBindingElement = new MtomMessageEncodingBindingElement(messageVersion, Encoding.UTF8);
                mtomEncoderBindingElement.MaxBufferSize = maxBufferSize;
                mtomEncoderBindingElement.MaxReadPoolSize = maxReadPoolSize;
                mtomEncoderBindingElement.MaxWritePoolSize = maxWritePoolSize;
                readerQuotas.CopyTo(mtomEncoderBindingElement.ReaderQuotas);
                mtom = mtomEncoderBindingElement.CreateMessageEncoderFactory().Encoder;
            }
        }

        /// <summary>
        /// Gets the content type of the handled messages.
        /// </summary>
        public override string ContentType => text.ContentType;

        /// <summary>
        /// Gets the media type of the handled messages.
        /// </summary>
        public override string MediaType => text.MediaType;

        /// <summary>
        /// Gets the version of the message to write.
        /// </summary>
        public override MessageVersion MessageVersion => text.MessageVersion;

        /// <summary>
        //  Returns a value that indicates whether a specified message-level content-type value is supported by the message encoder.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public override bool IsContentTypeSupported(string contentType)
        {
            if (contentType is null)
                throw new ArgumentNullException(nameof(contentType));

            return mtom?.IsContentTypeSupported(contentType) ?? text?.IsContentTypeSupported(contentType) ?? false;
        }

        /// <summary>
        /// Returns a value that indicates whether a specified message-level content-type value is supported by the message encoder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T GetProperty<T>()
        {
            return text.GetProperty<T>() ?? mtom.GetProperty<T>() ?? base.GetProperty<T>();
        }

        /// <summary>
        /// Returns true if the given content type is MTOM.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        static bool IsMtomMessage(string contentType)
        {
            return contentType.IndexOf("type=\"application/xop+xml\"", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Reads a message from the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bufferManager"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            if (IsMtomMessage(contentType))
            {
                if (mtom == null)
                    throw new InvalidOperationException("Unable to read MTOM message, check for supported MessageVersion.");

                var result = mtom.ReadMessage(buffer, bufferManager, contentType);
                result.Properties.Add(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, IsMtomMessage(contentType));
                return result;
            }
            else
            {
                var result = text.ReadMessage(buffer, bufferManager, contentType);
                result.Properties.Add(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, IsMtomMessage(contentType));
                return result;
            }
        }

        /// <summary>
        /// Reads a message from the given stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="maxSizeOfHeaders"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            if (IsMtomMessage(contentType))
            {
                if (mtom == null)
                    throw new InvalidOperationException("Unable to read MTOM message, check for supported MessageVersion.");

                var result = mtom.ReadMessage(stream, maxSizeOfHeaders, contentType);
                result.Properties.Add(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, IsMtomMessage(contentType));
                return result;
            }
            else
            {
                var result = text.ReadMessage(stream, maxSizeOfHeaders, contentType);
                result.Properties.Add(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, IsMtomMessage(contentType));
                return result;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="Message"/> should be written as MTOM.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ShouldWriteMtom(Message message)
        {
            return mtom != null && message.Properties.TryGetValue(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, out var temp) && (bool)temp;
        }

        /// <summary>
        /// Writes the message and returns an output array.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="bufferManager"></param>
        /// <param name="messageOffset"></param>
        /// <returns></returns>
        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            if (ShouldWriteMtom(message))
            {
                using (var ms = new MemoryStream())
                {
                    var mtomWriter = CreateMtomWriter(ms, message);
                    message.WriteMessage(mtomWriter);
                    mtomWriter.Flush();
                    var buffer = bufferManager.TakeBuffer((int)ms.Position + messageOffset);
                    Array.Copy(ms.GetBuffer(), 0, buffer, messageOffset, (int)ms.Position);
                    return new ArraySegment<byte>(buffer, messageOffset, (int)ms.Position);
                }
            }
            else
            {
                return text.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
            }
        }

        /// <summary>
        /// Writes the message to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stream"></param>
        public override void WriteMessage(Message message, Stream stream)
        {
            if (ShouldWriteMtom(message))
            {
                var mtomWriter = CreateMtomWriter(stream, message);
                message.WriteMessage(mtomWriter);
                mtomWriter.Flush();
            }
            else
            {
                text.WriteMessage(message, stream);
            }
        }

        /// <summary>
        /// Creates a <see cref="XmlWriter"/> suitable for outputing a MTOM message.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        XmlDictionaryWriter CreateMtomWriter(Stream stream, Message message)
        {
            var boundary = message.Properties[TextOrMtomEncodingBindingElement.MtomBoundaryPropertyName] as string;
            var startUri = message.Properties[TextOrMtomEncodingBindingElement.MtomStartUriPropertyName] as string;
            var startInfo = message.Properties[TextOrMtomEncodingBindingElement.MtomStartInfoPropertyName] as string;
            return XmlDictionaryWriter.CreateMtomWriter(stream, Encoding.UTF8, int.MaxValue, startInfo, boundary, startUri, false, false);
        }

    }

}
