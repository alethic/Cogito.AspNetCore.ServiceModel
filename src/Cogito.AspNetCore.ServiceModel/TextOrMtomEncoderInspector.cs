using System;
using System.Collections.ObjectModel;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using Cogito.Collections;

namespace Cogito.AspNetCore.ServiceModel
{

    class TextOrMtomEncoderInspector : IServiceBehavior, IEndpointBehavior, IDispatchMessageInspector
    {

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var endpoint in serviceDescription.Endpoints)
                endpoint.EndpointBehaviors.Add(this);
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {

        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return request.Properties.TryGetValue(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, out var result) ? result : null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var isMtom = correlationState is bool boolean && boolean;
            reply.Properties.Add(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, isMtom);
            if (isMtom)
            {
                var boundary = "uuid:" + Guid.NewGuid().ToString();
                var startUri = "http://tempuri.org/0";
                var startInfo = "application/soap+xml";
                var contentType = $"multipart/related; type=\"application/xop+xml\";start=\"<{startUri}>\";boundary=\"{boundary}\";start-info=\"{startInfo}\"";

                // configure the response properties
                var properties = (HttpResponseMessageProperty)reply.Properties.GetOrAdd(HttpResponseMessageProperty.Name, k => new HttpResponseMessageProperty());
                properties.Headers[HttpResponseHeader.ContentType] = contentType;
                properties.Headers["MIME-Version"] = "1.0";

                reply.Properties[TextOrMtomEncodingBindingElement.MtomBoundaryPropertyName] = boundary;
                reply.Properties[TextOrMtomEncodingBindingElement.MtomStartInfoPropertyName] = startInfo;
                reply.Properties[TextOrMtomEncodingBindingElement.MtomStartUriPropertyName] = startUri;
            }
        }

    }

}