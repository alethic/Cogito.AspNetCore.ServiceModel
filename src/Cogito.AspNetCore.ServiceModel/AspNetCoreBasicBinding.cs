using System.ComponentModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;

using Cogito.Collections;

namespace Cogito.ServiceModel.AspNetCore
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
        /// Initializes a new instance.
        /// </summary>
        /// <param name="configurationName"></param>
        public AspNetCoreBasicBinding(string configurationName) :
            this()
        {
            var section = (AspNetCoreBasicBindingCollectionElement)ConfigurationManager.GetSection("system.serviceModel/bindings/aspNetCoreBasicHttpBinding");
            var element = section.Bindings[configurationName];
            if (element == null)
                throw new ConfigurationErrorsException($"There is no binding named {configurationName} at {section.BindingName}.");

            element.ApplyConfiguration(this);
        }

        /// <summary>
        /// Gets the Scheme of the binding.
        /// </summary>
        public override string Scheme => "http";

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
