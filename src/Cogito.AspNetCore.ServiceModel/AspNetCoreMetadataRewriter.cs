using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml.Schema;

using Cogito.AspNetCore.ServiceModel.Collections;

using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Provides methods to rewrite service metadata.
    /// </summary>
    class AspNetCoreMetadataRewriter
    {

        readonly Dictionary<object, int> indexes = new Dictionary<object, int>();
        readonly AspNetCoreMessageProperty properties;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspNetCoreMetadataRewriter(AspNetCoreMessageProperty properties)
        {
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <summary>
        /// Gets a builder preloaded with the base URI.
        /// </summary>
        /// <returns></returns>
        UriBuilder CreateBaseUriBuilder()
        {
            var addr = new UriBuilder(properties.RoutingUri);
            addr.Host = properties.Context.Request.Host.Host;
            addr.Port = properties.Context.Request.Host.Port ?? addr.Port;
            addr.Path = new PathString(addr.Path) + properties.Context.Request.PathBase + properties.Context.Request.Path;
            return addr;
        }

        /// <summary>
        /// Rewrites an endpoint description.
        /// </summary>
        /// <param name="exporter"></param>
        public void RewriteEndpoint(WsdlExporter exporter)
        {
            // index wsdl references
            foreach (var (service, index) in exporter.GeneratedWsdlDocuments.Cast<System.Web.Services.Description.ServiceDescription>().Select((i, j) => (i, j)))
                indexes[service] = index;

            // index schema references
            foreach (var (schema, index) in exporter.GeneratedXmlSchemas.Schemas().Cast<XmlSchema>().Select((i, j) => (i, j)))
                indexes[schema] = index;

            foreach (System.Web.Services.Description.ServiceDescription document in exporter.GeneratedWsdlDocuments)
                RewriteServiceDescription(document);
        }

        /// <summary>
        /// Returns the index of the specified object if available.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        int IndexOf(object o) => indexes.GetOrDefault(o);

        public void RewriteServiceDescription(System.Web.Services.Description.ServiceDescription document)
        {
            foreach (Import import in document.Imports)
                RewriteImport(import);

            foreach (XmlSchema schema in document.Types.Schemas)
                RewriteSchema(schema);

            foreach (Service service in document.Services)
                RewriteService(service);
        }

        void RewriteImport(Import import)
        {
            var addr = CreateBaseUriBuilder();

            // import location has never been filled in, attempt to fill it in
            if (import.Location == null && import.ServiceDescription != null)
            {
                addr.Query = $"wsdl=wsdl{IndexOf(import.ServiceDescription)}";
                import.Location = addr.Uri.ToString();
                return;
            }

            // import location is filled in with routing URI
            if (import.Location != null && Uri.TryCreate(import.Location, UriKind.Absolute, out var uri) && uri.Host == properties.RoutingUri.Host)
            {
                addr.Query = uri.Query.TrimStart('?');
                import.Location = addr.Uri.ToString();
                return;
            }
        }

        public void RewriteSchema(XmlSchema schema)
        {
            var addr = CreateBaseUriBuilder();

            foreach (var import in schema.Includes.OfType<XmlSchemaImport>())
            {
                // schema location has never been filled in, attempt to fill it in
                if (import.SchemaLocation == null && import.Schema != null)
                {
                    addr.Query = $"xsd=xsd{IndexOf(import.Schema)}";
                    import.SchemaLocation = addr.Uri.ToString();
                    continue;
                }

                // schema location is filled in with routing URI
                if (import.SchemaLocation != null && Uri.TryCreate(import.SchemaLocation, UriKind.Absolute, out var uri) && uri.Host == properties.RoutingUri.Host)
                {
                    addr.Query = uri.Query.TrimStart('?');
                    import.SchemaLocation = addr.Uri.ToString();
                    continue;
                }
            }
        }

        void RewriteService(Service service)
        {
            foreach (Port port in service.Ports)
                RewritePort(port);
        }

        void RewritePort(Port port)
        {
            foreach (ServiceDescriptionFormatExtension extension in port.Extensions)
                RewriteExtension(extension);
        }

        void RewriteExtension(ServiceDescriptionFormatExtension extension)
        {
            if (extension is SoapAddressBinding addressBinding)
                RewriteSoapAddressBinding(addressBinding);
        }

        void RewriteSoapAddressBinding(SoapAddressBinding addressBinding)
        {
            if (addressBinding.Location != null)
                addressBinding.Location = RewriteAddress(addressBinding.Location);
        }

        string RewriteAddress(string address)
        {
            if (properties != null)
            {
                var addr = new UriBuilder(address);
                if (addr.Uri.Host == properties.RoutingUri.Host)
                {
                    addr.Host = properties.Context.Request.Host.Host;
                    addr.Port = properties.Context.Request.Host.Port ?? addr.Port;
                    addr.Path = new PathString(addr.Path) + properties.Context.Request.PathBase + properties.Context.Request.Path;
                    return addr.Uri.ToString();
                }
            }

            return address;
        }

    }

}
