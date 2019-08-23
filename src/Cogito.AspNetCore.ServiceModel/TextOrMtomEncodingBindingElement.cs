using System.ServiceModel.Channels;
using System.Xml;

namespace Cogito.AspNetCore.ServiceModel
{

    public class TextOrMtomEncodingBindingElement : MessageEncodingBindingElement
    {

        public const string IsIncomingMessageMtomPropertyName = "IncomingMessageIsMtom";
        public const string MtomBoundaryPropertyName = "__MtomBoundary";
        public const string MtomStartInfoPropertyName = "__MtomStartInfo";
        public const string MtomStartUriPropertyName = "__MtomStartUri";

        /// <summary>
        /// Creates a factory to generate encoders.
        /// </summary>
        /// <returns></returns>
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new TextOrMtomEncoderFactory(MessageVersion, ReaderQuotas, MaxReadPoolSize, MaxWritePoolSize, MaxBufferSize);
        }

        /// <summary>
        /// Gets or sets the message version handled by the binding.
        /// </summary>
        public override MessageVersion MessageVersion { get; set; } = MessageVersion.Default;

        /// <summary>
        /// Gets or sets constraints on the complexity of XML messages that can be processed by endpoints configured with this binding element.
        /// </summary>
        public XmlDictionaryReaderQuotas ReaderQuotas { get; set; } = new XmlDictionaryReaderQuotas();

        /// <summary>
        /// 
        /// </summary>
        public int MaxReadPoolSize { get; set; } = 64;

        /// <summary>
        /// 
        /// </summary>
        public int MaxWritePoolSize { get; set; } = 16;

        /// <summary>
        /// 
        /// </summary>
        public int MaxBufferSize { get; set; } = 65536;

        /// <summary>
        /// Creates a copy of this binding element.
        /// </summary>
        /// <returns></returns>
        public override BindingElement Clone()
        {
            return this;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return context.CanBuildInnerChannelListener<TChannel>();
        }

    }

}
