using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    class AspNetCoreWsdlEndpointBehavior : IEndpointBehavior, IWsdlExportExtension
    {

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

        public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {

        }

        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            foreach (System.Web.Services.Description.ServiceDescription document in exporter.GeneratedWsdlDocuments)
                RewriteServiceDescription(exporter, context, document);
        }

        void RewriteServiceDescription(WsdlExporter exporter, WsdlEndpointConversionContext context, System.Web.Services.Description.ServiceDescription document)
        {
            foreach (XmlSchema schema in document.Types.Schemas)
                RewriteSchema(exporter, context, schema);

            foreach (Service service in document.Services)
                RewriteService(exporter, context, service);
        }

        void RewriteSchema(WsdlExporter exporter, WsdlEndpointConversionContext context, XmlSchema schema)
        {
            var properties = AspNetCoreMessageProperty.GetValue(OperationContext.Current.IncomingMessageProperties);
            if (properties != null)
            {
                var b = new UriBuilder(properties.RoutingUri);
                b.Host = properties.Context.Request.Host.Host;
                b.Port = properties.Context.Request.Host.Port ?? b.Port;
                b.Path = new PathString(b.Path) + properties.Context.Request.PathBase + properties.Context.Request.Path;

                for (int i = 0; i < schema.Includes.Count; i++)
                {
                    b.Query = $"xsd=xsd{i}";

                    switch (schema.Includes[i])
                    {
                        case XmlSchemaInclude include:
                            if (include.SchemaLocation == null)
                                include.SchemaLocation = b.Uri.ToString();
                            break;
                        case XmlSchemaImport import:
                            if (import.SchemaLocation == null)
                                import.SchemaLocation = b.Uri.ToString();
                            break;
                    }
                }
            }
        }

        void RewriteService(WsdlExporter exporter, WsdlEndpointConversionContext context, Service service)
        {
            foreach (Port port in service.Ports)
                RewritePort(exporter, context, port);
        }

        void RewritePort(WsdlExporter exporter, WsdlEndpointConversionContext context, Port port)
        {
            foreach (ServiceDescriptionFormatExtension extension in port.Extensions)
                RewriteExtension(exporter, context, extension);
        }

        void RewriteExtension(WsdlExporter exporter, WsdlEndpointConversionContext context, ServiceDescriptionFormatExtension extension)
        {
            if (extension is SoapAddressBinding addressBinding)
                RewriteSoapAddressBinding(exporter, context, addressBinding);
        }

        void RewriteSoapAddressBinding(WsdlExporter exporter, WsdlEndpointConversionContext context, SoapAddressBinding addressBinding)
        {
            if (addressBinding.Location != null)
                addressBinding.Location = RewriteAddress(addressBinding.Location);
        }

        string RewriteAddress(string address)
        {
            var properties = AspNetCoreMessageProperty.GetValue(OperationContext.Current.IncomingMessageProperties);
            if (properties != null)
            {
                var b = new UriBuilder(address);
                if (b.Uri.Host == properties.RoutingUri.Host)
                {
                    b.Host = properties.Context.Request.Host.Host;
                    b.Port = properties.Context.Request.Host.Port ?? b.Port;
                    b.Path = new PathString(b.Path) + properties.Context.Request.PathBase + properties.Context.Request.Path;
                    return b.Uri.ToString();
                }
            }

            return address;
        }

    }

}
