﻿using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Cogito.AspNetCore.ServiceModel
{

    public class AspNetCoreTransportBindingElement :
        TransportBindingElement
    {

        readonly bool secure;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="secure"></param>
        public AspNetCoreTransportBindingElement(bool secure)
        {
            this.secure = secure;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspNetCoreTransportBindingElement(AspNetCoreTransportBindingElement other) :
            base(other)
        {
            MaxBufferSize = other.MaxBufferSize;
            MaxFaultSize = other.MaxFaultSize;
        }

        /// <summary>
        /// Returns whether or not this transport element is associated with HTTPs.
        /// </summary>
        public bool Secure => secure;

        /// <summary>
        /// Gets the scheme supported by this binding element.
        /// </summary>
        public override string Scheme => secure ? "https" : "http";

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

    }

}
