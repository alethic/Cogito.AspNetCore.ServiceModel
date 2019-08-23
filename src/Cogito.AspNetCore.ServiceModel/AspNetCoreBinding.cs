using System.ComponentModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Abstract ASP.Net Core Binding implementation.
    /// </summary>
    public abstract class AspNetCoreBinding : Binding, IBindingRuntimePreferences
    {

        readonly TextOrMtomEncodingBindingElement encoding;
        readonly AspNetCoreTransportBindingElement transport;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected AspNetCoreBinding()
        {
            transport = new AspNetCoreTransportBindingElement();
            encoding = new TextOrMtomEncodingBindingElement() { MessageVersion = MessageVersion.Soap11 };
        }

        /// <summary>
        /// Returns whether or not this binding is associated with HTTPS requests.
        /// </summary>
        public bool Secure
        {
            get => transport.Secure;
            set => transport.Secure = value;
        }

        /// <summary>
        /// Returns the method to be considered by this binding.
        /// </summary>
        public string Method
        {
            get => transport.Method;
            set => transport.Method = value;
        }

        /// <summary>
        /// Gets the scheme of the binding.
        /// </summary>
        public override string Scheme => transport.Scheme;

        /// <summary>
        /// Gets the text encoding element.
        /// </summary>
        public TextOrMtomEncodingBindingElement EncodingElement => encoding;

        /// <summary>
        /// Gets or sets the SOAP and WS-Addressing versions that are used to format the text message.
        /// </summary>
        public new MessageVersion MessageVersion
        {
            get => encoding.MessageVersion;
            set => encoding.MessageVersion = value;
        }

        /// <summary>
        /// Gets the maximum buffer size.
        /// </summary>
        [DefaultValue(AspNetCoreTransportDefaults.MaxBufferSize)]
        public int MaxBufferSize
        {
            get => transport.MaxBufferSize;
            set => transport.MaxBufferSize = encoding.MaxBufferSize = value;
        }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of any buffer pools used by the transport.
        /// </summary>
        public long MaxBufferPoolSize
        {
            get => transport.MaxBufferPoolSize;
            set => transport.MaxBufferPoolSize = value;
        }

        /// <summary>
        /// Gets and sets the maximum allowable message size, in bytes, that can be received.
        /// </summary>
        public long MaxReceivedMessageSize
        {
            get => transport.MaxReceivedMessageSize;
            set => transport.MaxReceivedMessageSize = value;
        }

        /// <summary>
        /// Maximum size of a fault message.
        /// </summary>
        [DefaultValue(AspNetCoreTransportDefaults.MaxFaultSize)]
        public int MaxFaultSize
        {
            get => transport.MaxFaultSize;
            set => transport.MaxFaultSize = value;
        }

        /// <summary>
        /// Gets or sets the number of readers that are allocated to a pool and ready for use to process incoming messages.
        /// </summary>
        public int MaxReadPoolSize
        {
            get => encoding.MaxReadPoolSize;
            set => encoding.MaxReadPoolSize = value;
        }

        /// <summary>
        /// Gets or sets the number of writers that are allocated to a pool and ready for use to process outgoing messages.
        /// </summary>
        public int MaxWritePoolSize
        {
            get => encoding.MaxWritePoolSize;
            set => encoding.MaxWritePoolSize = value;
        }

        /// <summary>
        /// Gets or sets constraints on the complexity of XML messages that can be processed by endpoints configured
        /// with this binding.
        /// </summary>
        public XmlDictionaryReaderQuotas ReaderQuotas => encoding.ReaderQuotas;

        /// <summary>
        /// Creates the binding elements.
        /// </summary>
        /// <returns></returns>
        public override BindingElementCollection CreateBindingElements()
        {
            var c = new BindingElementCollection();
            c.Add(EncodingElement);
            c.Add(transport);
            return c;
        }

        /// <summary>
        /// Operation in async mode.
        /// </summary>
        bool IBindingRuntimePreferences.ReceiveSynchronously => false;

    }

}
