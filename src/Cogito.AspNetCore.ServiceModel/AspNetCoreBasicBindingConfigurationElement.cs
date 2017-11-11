using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace Cogito.ServiceModel.AspNetCore
{

    public class AspNetCoreBasicBindingConfigurationElement :
        StandardBindingElement
    {

        protected override Type BindingElementType
        {
            get { return typeof(AspNetCoreBasicBinding); }
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            if (binding.GetType() != typeof(AspNetCoreBasicBinding))
                throw new ArgumentException($"Invalid type for binding. Expected type: {typeof(AspNetCoreBasicBinding).AssemblyQualifiedName}. Type passed in: {binding.GetType().AssemblyQualifiedName}.");
        }

    }

}
