using System.ServiceModel;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Basic binding with MTOM encoding enabled.
    /// </summary>
    public class AspNetCoreBasicMtomBinding :
        AspNetCoreBasicBinding
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspNetCoreBasicMtomBinding() :
            base()
        {
            MessageEncoding = WSMessageEncoding.Mtom;
        }

    }

}
