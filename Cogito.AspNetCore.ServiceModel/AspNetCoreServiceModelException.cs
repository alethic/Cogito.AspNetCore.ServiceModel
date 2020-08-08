using System;

namespace Cogito.AspNetCore.ServiceModel
{


    public class AspNetCoreServiceModelException :
        Exception
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="message"></param>
        public AspNetCoreServiceModelException(string message) :
            base(message)
        {



        }

    }

}
