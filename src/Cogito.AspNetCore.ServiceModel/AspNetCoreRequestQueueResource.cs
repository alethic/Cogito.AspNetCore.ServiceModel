using System;
using System.Threading;

namespace Cogito.AspNetCore.ServiceModel
{

    /// <summary>
    /// Tracks the lifetime of a queue.
    /// </summary>
    class AspNetCoreRequestQueueResource
    {

        readonly AspNetCoreRequestQueue queue;
        int leases;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="queue"></param>
        public AspNetCoreRequestQueueResource(AspNetCoreRequestQueue queue)
        {
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        /// <summary>
        /// Acquires a new lease on the queue.
        /// </summary>
        /// <returns></returns>
        public AspNetCoreRequestQueueLease Acquire()
        {
            Interlocked.Increment(ref leases);
            return new AspNetCoreRequestQueueLease(queue, Release);
        }

        /// <summary>
        /// Releases a lease on the queue.
        /// </summary>
        void Release()
        {
            if (Interlocked.Decrement(ref leases) == 0)
                Complete?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when all users of the resource have been released.
        /// </summary>
        public event EventHandler Complete;

        /// <summary>
        /// Gets a reference to the queue.
        /// </summary>
        public AspNetCoreRequestQueue Queue => queue;

    }

}
