using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace Cogito.AspNetCore.ServiceModel
{

    class TextOrMtomEncoderFactory : MessageEncoderFactory
    {

        readonly TextOrMtomEncoder encoder;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="messageVersion"></param>
        /// <param name="readerQuotas"></param>
        public TextOrMtomEncoderFactory(MessageVersion messageVersion, XmlDictionaryReaderQuotas readerQuotas, int maxReadPoolSize, int maxWritePoolSize, int maxBufferSize)
        {
            if (messageVersion is null)
                throw new ArgumentNullException(nameof(messageVersion));
            if (readerQuotas is null)
                throw new ArgumentNullException(nameof(readerQuotas));

            encoder = new TextOrMtomEncoder(messageVersion, readerQuotas, maxReadPoolSize, maxWritePoolSize, maxBufferSize);
        }

        public override MessageEncoder Encoder => encoder;

        public override MessageVersion MessageVersion => encoder.MessageVersion;

    }

}