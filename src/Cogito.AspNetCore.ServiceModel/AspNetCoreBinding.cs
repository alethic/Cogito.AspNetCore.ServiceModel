using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Abstract ASP.Net Core Binding implementation.
    /// </summary>
    public abstract class AspNetCoreBinding :
        Binding
    {

        readonly TextMessageEncodingBindingElement textEncoding;
        readonly MtomMessageEncodingBindingElement mtomEncoding;
        readonly AspNetCoreTransportBindingElement transport;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="incoming"></param>
        protected AspNetCoreBinding()
        {
            this.transport = new AspNetCoreTransportBindingElement();
            this.textEncoding = new TextMessageEncodingBindingElement();
            this.textEncoding.MessageVersion = MessageVersion.Soap11;
            this.mtomEncoding = new MtomMessageEncodingBindingElement();
            this.mtomEncoding.MessageVersion = MessageVersion.Soap11;

            // default to MTOM defaults
            textEncoding.ReaderQuotas = mtomEncoding.ReaderQuotas;
            textEncoding.MaxReadPoolSize = mtomEncoding.MaxReadPoolSize;
            textEncoding.MaxWritePoolSize = mtomEncoding.MaxWritePoolSize;
            textEncoding.MessageVersion = mtomEncoding.MessageVersion;
            textEncoding.WriteEncoding = mtomEncoding.WriteEncoding;
        }

        /// <summary>
        /// Gets the scheme of the binding.
        /// </summary>
        public override string Scheme => "aspnetcore";

        /// <summary>
        /// Gets the text encoding element.
        /// </summary>
        public TextMessageEncodingBindingElement TextEncodingElement => textEncoding;

        /// <summary>
        /// Gets the MTOM encoding element.
        /// </summary>
        public MtomMessageEncodingBindingElement MtomEncodingElement => mtomEncoding;

        /// <summary>
        /// Gets or sets the message encoding.
        /// </summary>
        [DefaultValue(WSMessageEncoding.Text)]
        public WSMessageEncoding MessageEncoding { get; set; } = WSMessageEncoding.Text;

        /// <summary>
        /// Gets the maximum buffer size.
        /// </summary>
        [DefaultValue(AspNetCoreTransportDefaults.MaxBufferSize)]
        public int MaxBufferSize
        {
            get { return transport.MaxBufferSize; }
            set { transport.MaxBufferSize = mtomEncoding.MaxBufferSize = value; }
        }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of any buffer pools used by the transport.
        /// </summary>
        public long MaxBufferPoolSize
        {
            get { return transport.MaxBufferPoolSize; }
            set { transport.MaxBufferPoolSize = value; }
        }

        /// <summary>
        /// Gets and sets the maximum allowable message size, in bytes, that can be received.
        /// </summary>
        public long MaxReceivedMessageSize
        {
            get { return transport.MaxReceivedMessageSize; }
            set { transport.MaxReceivedMessageSize = value; }
        }

        /// <summary>
        /// Maximum size of a fault message.
        /// </summary>
        [DefaultValue(AspNetCoreTransportDefaults.MaxFaultSize)]
        public int MaxFaultSize
        {
            get { return transport.MaxFaultSize; }
            set { transport.MaxFaultSize = value; }
        }

        /// <summary>
        /// Gets or sets the number of readers that are allocated to a pool and ready for use to process incoming messages.
        /// </summary>
        public int MaxReadPoolSize
        {
            get { return textEncoding.MaxReadPoolSize; }
            set { textEncoding.MaxReadPoolSize = mtomEncoding.MaxReadPoolSize = value; }
        }

        /// <summary>
        /// Gets or sets the number of writers that are allocated to a pool and ready for use to process outgoing messages.
        /// </summary>
        public int MaxWritePoolSize
        {
            get { return textEncoding.MaxWritePoolSize; }
            set { textEncoding.MaxWritePoolSize = mtomEncoding.MaxWritePoolSize = value; }
        }

        /// <summary>
        /// Gets or sets constraints on the complexity of XML messages that can be processed by endpoints configured
        /// with this binding.
        /// </summary>
        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return mtomEncoding.ReaderQuotas; }
        }

        /// <summary>
        /// Creates the binding elements.
        /// </summary>
        /// <returns></returns>
        public override BindingElementCollection CreateBindingElements()
        {
            var c = new BindingElementCollection();

            // text encoding specified
            if (MessageEncoding == WSMessageEncoding.Text)
                c.Add(TextEncodingElement);

            // mtom encoding specified
            if (MessageEncoding == WSMessageEncoding.Mtom)
                c.Add(MtomEncodingElement);

            // add ASP.Net Core Transport
            c.Add(transport);

            return c;
        }

    }

}
