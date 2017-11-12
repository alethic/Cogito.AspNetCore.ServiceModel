using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Cogito.AspNetCore.ServiceModel
{

    public class AspNetCoreTransportBindingElement :
        TransportBindingElement
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
            this()
        {

        }

        /// <summary>
        /// Gets the scheme supported by this binding element.
        /// </summary>
        public override string Scheme => "aspnetcore";

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

            var queue = context.BindingParameters.Find<AspNetCoreRequestQueue>();
            if (queue == null)
                throw new CommunicationException($"Unable to locate {nameof(AspNetCoreRequestQueue)} binding parameter.");

            return (IChannelListener<TChannel>)new AspNetCoreReplyChannelListener(queue, this, context);
        }

    }

}
