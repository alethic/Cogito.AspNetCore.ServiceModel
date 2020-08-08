using System;

using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    public static class AspNetCoreUri
    {

        /// <summary>
        /// Name of the item injected into a <see cref="HttpContext"/> that stores the original base URI.
        /// </summary>
        public static string UriContextItemName = nameof(AspNetCoreUri);

        /// <summary>
        /// Returns the 'aspnetcore' scheme-based URI for the given context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Uri GetUri(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return (Uri)context.Items[UriContextItemName];
        }

    }

}
