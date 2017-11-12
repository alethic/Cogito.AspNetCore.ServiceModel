﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace Cogito.AspNetCore.ServiceModel.IO
{

    /// <summary>
    /// Provides various extension methods for working with <see cref="Stream"/> instances.
    /// </summary>
    static class StreamExtensions
    {

        /// <summary>
        /// Reads all the data from the <see cref="Stream"/> and returns the resulting array.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this Stream self)
        {
            var stm = new MemoryStream();
            self.CopyTo(stm);
            return stm.ToArray();
        }

        /// <summary>
        /// Reads all the data from the <see cref="Stream"/> and returns the resulting array.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadAllBytesAsync(this Stream self)
        {
            var stm = new MemoryStream();
            await self.CopyToAsync(stm);
            return stm.ToArray();
        }

        /// <summary>
        /// Writes all data from the <paramref name="source"/> <see cref="Stream"/> into this <see cref="Stream"/>.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        /// <param name="bufferSize"></param>
        public static void WriteFrom(this Stream self, Stream source, int bufferSize)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source.CopyTo(self, bufferSize);
        }

        /// <summary>
        /// Writes all data from the <paramref name="source"/> <see cref="Stream"/> into this <see cref="Stream"/>.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        public static void WriteFrom(this Stream self, Stream source)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source.CopyTo(self);
        }

        /// <summary>
        /// Writes all data from the <paramref name="source"/> <see cref="Stream"/> into this <see cref="Stream"/>.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        /// <param name="bufferSize"></param>
        public static Task WriteFromAsync(this Stream self, Stream source, int bufferSize)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.CopyToAsync(self, bufferSize);
        }

        /// <summary>
        /// Writes all data from the <paramref name="source"/> <see cref="Stream"/> into this <see cref="Stream"/>.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        public static Task WriteFromAsync(this Stream self, Stream source)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.CopyToAsync(self);
        }

    }

}
