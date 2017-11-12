using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Cogito.AspNetCore.ServiceModel
{

    public abstract class AspNetCoreBindingBase :
        Binding
    {

        readonly TextMessageEncodingBindingElement textEncoding;
        readonly MtomMessageEncodingBindingElement mtomEncoding;
        readonly AspNetCoreTransportBindingElement transport;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="incoming"></param>
        protected AspNetCoreBindingBase()
        {
            this.transport = new AspNetCoreTransportBindingElement();
            this.textEncoding = new TextMessageEncodingBindingElement();
            this.textEncoding.MessageVersion = MessageVersion.Soap11;
            this.mtomEncoding = new MtomMessageEncodingBindingElement();
            this.mtomEncoding.MessageVersion = MessageVersion.Soap11;
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
        /// Maximum size of a fault message.
        /// </summary>
        [DefaultValue(AspNetCoreTransportDefaults.MaxFaultSize)]
        public int MaxFaultSize
        {
            get { return transport.MaxFaultSize; }
            set { transport.MaxFaultSize = value; }
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
