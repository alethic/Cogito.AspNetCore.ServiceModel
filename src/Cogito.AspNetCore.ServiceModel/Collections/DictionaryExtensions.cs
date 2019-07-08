using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Cogito.AspNetCore.ServiceModel.Collections
{

    /// <summary>
    /// Provides extension methods for working with Dictionaries.
    /// </summary>
    static class DictionaryExtensions
    {

        /// <summary>
        /// Gets the value for the specified key, or the default value of the type.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return self.TryGetValue(key, out var v) ? v : default;
        }

        /// <summary>
        /// Gets the value for the specified key or creates it.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> create)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (create == null)
                throw new ArgumentNullException(nameof(create));

            // ConcurrentDictionary provides it's own thread-safe version
            if (self is ConcurrentDictionary<TKey, TValue>)
                return ((ConcurrentDictionary<TKey, TValue>)self).GetOrAdd(key, create);

            return self.TryGetValue(key, out var v) ? v : self[key] = create(key);
        }

        /// <summary>
        /// Gets the value for the specified key or creates it asynchronously. This is not thread-safe.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public static async Task<TValue> GetOrAddAsync<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, Task<TValue>> create)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (create == null)
                throw new ArgumentNullException(nameof(create));

            return self.TryGetValue(key, out var v) ? v : self[key] = await create(key);
        }

    }

}
