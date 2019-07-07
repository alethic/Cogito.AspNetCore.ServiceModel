using System;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Records access to a request queue, until disposed.
    /// </summary>
    class AspNetCoreRequestQueueLease : IDisposable
    {

        readonly object sync = new object();
        readonly AspNetCoreRequestQueue queue;
        readonly Action release;
        bool disposed;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="release"></param>
        public AspNetCoreRequestQueueLease(AspNetCoreRequestQueue queue, Action release)
        {
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            this.release = release ?? throw new ArgumentNullException(nameof(release));
        }

        /// <summary>
        /// Gets the leased queue.
        /// </summary>
        public AspNetCoreRequestQueue Queue
        {
            get
            {
                lock (sync)
                    return disposed == false ? queue : throw new ObjectDisposedException(nameof(AspNetCoreRequestQueueLease));
            }
        }

        /// <summary>
        /// Disposes of the instance and releases the lease.
        /// </summary>
        public void Dispose()
        {
            if (disposed == false)
            {
                lock (sync)
                {
                    if (disposed == false)
                    {
                        release?.Invoke();
                        disposed = true;
                    }
                }
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~AspNetCoreRequestQueueLease()
        {
            Dispose();
        }

    }

}
