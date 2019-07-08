﻿using System;
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

            TValue v;
            return self.TryGetValue(key, out v) ? v : self[key] = create(key);
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

            TValue v;
            return self.TryGetValue(key, out v) ? v : self[key] = await create(key);
        }

        /// <summary>
        /// Returns a <see cref="IDictionary"/> implementation which merges the first dictionary with the second.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> second)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var d = new Dictionary<TKey, TValue>(source);

            // merge keys from second into new copy
            foreach (var i in second)
                if (i.Key != null)
                    d[i.Key] = i.Value;

            return d;
        }

        /// <summary>
        /// Converts the <see cref="IEnumerable{TKey,TValue}"/> to a <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDictionary<TKey, TElement> ToDictionary<TKey, TElement>(this IEnumerable<KeyValuePair<TKey, TElement>> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.ToDictionary(i => i.Key, i => i.Value);
        }

        /// <summary>
        /// Returns an empty <see cref="IDictionary{TKey, TValue}"/> if <paramref name="source"/> is null.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> EmptyIfNull<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            return source ?? new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Returns an empty <see cref="IDictionary{TKey, TValue}"/> if <paramref name="source"/> is null.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> EmptyIfNull<TKey, TValue>(this Dictionary<TKey, TValue> source)
        {
            return source ?? new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Transforms the given dictionary of strings to a <see cref="NameValueCollection"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection(this IDictionary<string, string> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var c = new NameValueCollection();

            // add items to collection
            foreach (var i in source)
                c.Add(i.Key, i.Value);

            return c;
        }

    }

}
