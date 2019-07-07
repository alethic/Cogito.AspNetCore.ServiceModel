using System;
using Microsoft.AspNetCore.Http;

namespace Cogito.AspNetCore.ServiceModel
{

    public class AspNetCoreRequestRoute
    {

        readonly Uri uri;
        readonly string method;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="filter"></param>
        public AspNetCoreRequestRoute(Uri uri, string method = null)
        {
            this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
            this.method = method;
        }

    }

}
