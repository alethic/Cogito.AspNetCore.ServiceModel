using System;
using System.ServiceModel;

using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    public static class OperationContextExtensions
    {

        /// <summary>
        /// Gets the ASP.Net Core <see cref="HttpContext"/> executing the given request context.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static HttpContext GetAspNetCoreContext(this OperationContext self)
        {
            return AspNetCoreMessageProperty.GetValue(self.IncomingMessageProperties)?.Context;
        }

        /// <summary>
        /// Gets the ASP.Net Core ServiceModel routing URI for the given request context.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Uri GetAspNetCoreRoutingUri(this OperationContext self)
        {
            return AspNetCoreMessageProperty.GetValue(self.IncomingMessageProperties)?.RoutingUri;
        }

    }

}
