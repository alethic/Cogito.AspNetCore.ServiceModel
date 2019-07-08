using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.AspNetCore.ServiceModel.Collections
{

    /// <summary>
    /// Various extension methods for working with collection
    /// </summary>
    static class CollectionExtensions
    {

        /// <summary>
        /// Adds all of the items to the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="add"></param>
        public static void AddRange<T>(this ICollection<T> self, IEnumerable<T> add)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (add == null)
                throw new ArgumentNullException(nameof(add));

            foreach (var i in add)
                self.Add(i);
        }

        /// <summary>
        /// Removes all of the items from the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="remove"></param>
        public static void RemoveRange<T>(this ICollection<T> self, IEnumerable<T> remove)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (remove == null)
                throw new ArgumentNullException(nameof(remove));

            foreach (var i in remove)
                self.Remove(i);
        }

    }

}
