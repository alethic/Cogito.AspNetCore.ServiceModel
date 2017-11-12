using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace Cogito.AspNetCore.ServiceModel
{

    static class AspNetCoreServiceModelExtensions
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

        /// <summary>
        /// Opens the communication object.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Task OpenAsync(this ICommunicationObject self)
        {
            return Task.Factory.FromAsync(self.BeginOpen, self.EndOpen, null);
        }

        /// <summary>
        /// Opens the communication object.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Task CloseAsync(this ICommunicationObject self)
        {
            return Task.Factory.FromAsync(self.BeginClose, self.EndClose, null);
        }

    }

}
