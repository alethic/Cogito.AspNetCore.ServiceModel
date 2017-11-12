using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;

using Cogito.Collections;

namespace Cogito.AspNetCore.ServiceModel
{

    public class AspNetCoreBasicBinding :
        AspNetCoreBindingBase
    {

        readonly BasicHttpMessageSecurity messageSecurityElement;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public AspNetCoreBasicBinding() :
            base()
        {
            messageSecurityElement = new BasicHttpMessageSecurity();
        }

        /// <summary>
        /// Gets or sets the level of security to apply.
        /// </summary>
        [DefaultValue(AspNetCoreBasicSecurityMode.None)]
        public AspNetCoreBasicSecurityMode SecurityMode { get; set; } = AspNetCoreBasicSecurityMode.None;

        /// <summary>
        /// Creates the binding elements.
        /// </summary>
        /// <returns></returns>
        public override BindingElementCollection CreateBindingElements()
        {
            var c = new BindingElementCollection();
            c.AddRange(base.CreateBindingElements());
            return c.Clone();
        }

    }

}
