using System.ServiceModel.Channels;

namespace Cogito.AspNetCore.ServiceModel
{

    static class AspNetCoreExtensions
    {

        /// <summary>
        /// Gets the property with the specified name.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TProperty GetValue<TProperty>(this MessageProperties self, string name)
            where TProperty : class
        {
            return self.TryGetValue(name, out var value) ? (TProperty)value : null;
        }

    }

}
