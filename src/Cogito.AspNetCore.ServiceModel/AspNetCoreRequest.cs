using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Encapsulates an ASP.Net request.
    /// </summary>
    class AspNetCoreRequest : TaskCompletionSource<bool>
    {

        readonly HttpContext context;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="context"></param>
        public AspNetCoreRequest(HttpContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets the <see cref="HttpContext"/> associated with the request.
        /// </summary>
        public HttpContext Context => context;

    }

}
