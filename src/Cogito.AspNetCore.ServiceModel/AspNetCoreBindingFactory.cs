using System.ServiceModel.Channels;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Provides registered bindings based on security mode.
    /// </summary>
    public class AspNetCoreBindingFactory
    {

        /// <summary>
        /// Creates a ASP.NET Core Binding based on the specified security mode.
        /// </summary>
        /// <param name="messageVersion"></param>
        /// <param name="secure"></param>
        /// <returns></returns>
        public virtual Binding CreateBinding(MessageVersion messageVersion, bool secure)
        {
            return new AspNetCoreBasicBinding(secure)
            {
                MessageVersion = messageVersion
            };
        }

    }

}
