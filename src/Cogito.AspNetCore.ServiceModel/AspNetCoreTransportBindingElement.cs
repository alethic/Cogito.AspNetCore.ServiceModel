using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

using Microsoft.Extensions.DependencyInjection;

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
        public override string Scheme => "http";

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
            return typeof(TChannel) == typeof(IRequestChannel);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IReplyChannel);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (CanBuildChannelFactory<TChannel>(context) == false)
                throw new ArgumentException($"Unsupported channel type: {typeof(TChannel).Name}.");

            throw new NotSupportedException();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (CanBuildChannelListener<TChannel>(context) == false)
                throw new ArgumentException($"Unsupported channel type: {typeof(TChannel).Name}.");

            var services = context.BindingParameters.Find<IServiceProvider>();
            if (services == null)
                throw new CommunicationException($"Unable to locate {nameof(IServiceProvider)} in binding parameters. Ensure the {nameof(AspNetCoreServiceBehavior)} is added to the host.");

            var incoming = services.GetService<AspNetCoreRequestHandler>();
            if (incoming == null)
                throw new CommunicationException($"Unable to locate {nameof(AspNetCoreRequestHandler)}. Ensure ASP.Net core support for WCF has been registered with the service provider.");

            return (IChannelListener<TChannel>)new AspNetCoreReplyChannelListener(incoming, this, context);
        }

    }

}
