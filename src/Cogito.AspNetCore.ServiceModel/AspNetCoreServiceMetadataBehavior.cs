using System;
using System.Collections.ObjectModel;
using System.Net.Mime;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Cogito.AspNetCore.ServiceModel
{

    class AspNetCoreServiceMetadataBehavior : IServiceBehavior, IEndpointBehavior, IDispatchMessageInspector, IWsdlExportExtension
    {

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
                if (dispatcher.BindingName == "ServiceMetadataBehaviorHttpGetBinding" || dispatcher.BindingName == "ServiceMetadataBehaviorHttpsGetBinding")
                    foreach (var endpoint in dispatcher.Endpoints)
                        endpoint.DispatchRuntime.MessageInspectors.Add(this);
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {

        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {

        }

        public void Validate(ServiceEndpoint endpoint)
        {

        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            // create a copy of the properties to avoid disposal
            return new MessageProperties(request.Properties);
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (reply.IsFault)
                return;

            var http = reply.Properties.GetValue<HttpResponseMessageProperty>(HttpResponseMessageProperty.Name);
            if (http == null)
                return;

            if (http.Headers.Get("Content-Type") is string c)
            {
                var contentType = new ContentType(c);
                if (contentType.MediaType != "text/xml")
                    return;

                var message = reply.CreateBufferedCopy(int.MaxValue);
                var reading = message.CreateMessage().GetReaderAtBodyContents();

                // content is a WSDL
                if (reading.IsStartElement("definitions", "http://schemas.xmlsoap.org/wsdl/"))
                {
                    var service = System.Web.Services.Description.ServiceDescription.Read(reading);
                    if (service != null)
                    {
                        // use rewriter to change service description
                        var property = AspNetCoreMessageProperty.GetValue((MessageProperties)correlationState);
                        var rewriter = new AspNetCoreMetadataRewriter(property);
                        rewriter.RewriteServiceDescription(service);

                        reply = Message.CreateMessage(
                            reply.Version,
                            reply.Headers.Action,
                            new XmlSerializerBodyWriter(System.Web.Services.Description.ServiceDescription.Serializer, service));

                        return;
                    }
                }

                if (reading.IsStartElement("schema", "http://www.w3.org/2001/XMLSchema"))
                {
                    var schema = XmlSchema.Read(reading, (s, a) => { });
                    if (schema != null)
                    {
                        // use rewriter to change service description
                        var property = AspNetCoreMessageProperty.GetValue((MessageProperties)correlationState);
                        var rewriter = new AspNetCoreMetadataRewriter(property);
                        rewriter.RewriteSchema(schema);

                        reply = Message.CreateMessage(
                            reply.Version,
                            reply.Headers.Action,
                            new XmlSchemaBodyWriter(schema));

                        return;
                    }
                }

                // reconstitute original message if we failed to match
                reply = message.CreateMessage();
            }
        }

        /// <summary>
        /// Provides a body writer that operates against an object and an old-style <see cref="XmlSerializer"/>.
        /// </summary>
        class XmlSerializerBodyWriter : BodyWriter
        {

            readonly XmlSerializer serializer;
            readonly object graph;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="serializer"></param>
            /// <param name="graph"></param>
            public XmlSerializerBodyWriter(XmlSerializer serializer, object graph) :
                base(true)
            {
                this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
                this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                serializer.Serialize(writer, graph);
            }

        }

        /// <summary>
        /// Provides a body writer that operates against an <see cref="XmlSchema"/>.
        /// </summary>
        class XmlSchemaBodyWriter : BodyWriter
        {

            readonly XmlSchema schema;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="schema"></param>
            public XmlSchemaBodyWriter(XmlSchema schema) :
                base(true)
            {
                this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                schema.Write(writer);
            }

        }

        public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {

        }

        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            var ctx = OperationContext.Current;
            new AspNetCoreMetadataRewriter(ctx != null ? AspNetCoreMessageProperty.GetValue(ctx.IncomingMessageProperties) : null).RewriteEndpoint(exporter);
        }

    }

}
