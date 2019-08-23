using System;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Cogito.AspNetCore.ServiceModel
{

    class TextOrMtomEncoderInspector : IEndpointBehavior, IDispatchMessageInspector
    {

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

        public object AfterReceiveRequest(ref Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            return request.Properties.TryGetValue(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, out var result) ? result : null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var isMtom = correlationState is bool && (bool)correlationState;
            reply.Properties.Add(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, isMtom);
            if (isMtom)
            {
                var boundary = "uuid:" + Guid.NewGuid().ToString();
                var startUri = "http://tempuri.org/0";
                var startInfo = "application/soap+xml";
                var contentType = $"multipart/related; type=\"application/xop+xml\";start=\"<{startUri}>\";boundary=\"{boundary}\";start-info=\"{startInfo}\"";

                HttpResponseMessageProperty respProp;
                if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
                {
                    respProp = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
                }
                else
                {
                    respProp = new HttpResponseMessageProperty();
                    reply.Properties[HttpResponseMessageProperty.Name] = respProp;
                }

                respProp.Headers[HttpResponseHeader.ContentType] = contentType;
                respProp.Headers["MIME-Version"] = "1.0";

                reply.Properties[TextOrMtomEncodingBindingElement.MtomBoundaryPropertyName] = boundary;
                reply.Properties[TextOrMtomEncodingBindingElement.MtomStartInfoPropertyName] = startInfo;
                reply.Properties[TextOrMtomEncodingBindingElement.MtomStartUriPropertyName] = startUri;
            }
        }

    }

}