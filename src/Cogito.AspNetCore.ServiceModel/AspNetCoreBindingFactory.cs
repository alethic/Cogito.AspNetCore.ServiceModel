using System;
using System.ServiceModel.Channels;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Provides registered bindings based on security mode.
    /// </summary>
    public class AspNetCoreBindingFactory
    {

        /// <summary>
        /// Creates a ASP.NET Core Binding based on the specified security mode and optional method filter.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="messageVersion"></param>
        /// <param name="secure"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public virtual Binding CreateBinding(IServiceProvider serviceProvider, MessageVersion messageVersion, bool secure, string method = null)
        {
            return new AspNetCoreBasicBinding()
            {
                MessageVersion = messageVersion,
                Secure = secure,
                Method = method,
            };
        }

    }

}
