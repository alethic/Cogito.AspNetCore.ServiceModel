using System;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web.Services.Description;

namespace Cogito.AspNetCore.ServiceModel
{

    public class AspNetCoreTransportBindingElement : TransportBindingElement, IWsdlExportExtension
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspNetCoreTransportBindingElement()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspNetCoreTransportBindingElement(AspNetCoreTransportBindingElement other) :
            base(other)
        {
            Secure = other.Secure;
            Method = other.Method;
            MaxBufferSize = other.MaxBufferSize;
            MaxFaultSize = other.MaxFaultSize;
        }

        /// <summary>
        /// Returns whether or not this transport element is associated with HTTPs.
        /// </summary>
        public bool Secure { get; set; }

        /// <summary>
        /// Returns the HTTP method to receive messages for.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets the scheme supported by this binding element.
        /// </summary>
        public override string Scheme => Secure ? "https" : "http";

        /// <summary>
        /// Maximum size of a buffer.
        /// </summary>
        [DefaultValue(AspNetCoreTransportDefaults.MaxBufferSize)]
        public int MaxBufferSize { get; set; } = AspNetCoreTransportDefaults.MaxBufferSize;

        /// <summary>
        /// Maximum size of a fault message.
        /// </summary>
        [DefaultValue(AspNetCoreTransportDefaults.MaxFaultSize)]
        public int MaxFaultSize { get; set; } = AspNetCoreTransportDefaults.MaxFaultSize;

        /// <summary>
        /// Creates a copy of this binding element.
        /// </summary>
        /// <returns></returns>
        public override BindingElement Clone()
        {
            return new AspNetCoreTransportBindingElement(this);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return false;
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IReplyChannel);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (CanBuildChannelListener<TChannel>(context) == false)
                throw new ArgumentException($"Unsupported channel type: {typeof(TChannel).Name}.");

            var router = context.BindingParameters.Find<AspNetCoreRequestRouter>();
            if (router == null)
                throw new CommunicationException($"Unable to locate {nameof(AspNetCoreRequestRouter)} binding parameter. Ensure the ServiceBehavior has been associated with the host.");

            return (IChannelListener<TChannel>)new AspNetCoreReplyChannelListener(router, this, context);
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {

        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            if (exporter == null)
                throw new ArgumentNullException(nameof(exporter));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            foreach (var extension in context.WsdlBinding.Extensions.OfType<SoapBinding>())
            {
                extension.Style = SoapBindingStyle.Document;
                extension.Transport = "http://schemas.xmlsoap.org/soap/http";
            }
        }

    }

}
