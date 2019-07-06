using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cogito.AspNetCore.ServiceModel.Threading
{

    /// <summary>
    /// Provides some extension methods for <see cref="Task"/> objects.
    /// </summary>
    static class TaskExtensions
    {

        /// <summary>
        /// Implements the Begin method of the Asynchronous Programming Model pattern for a <see cref="Task"/>. 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IAsyncResult ToAsyncBegin<TResult>(this Task<TResult> task, AsyncCallback callback, object state)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var ec = ExecutionContext.Capture();
            var cs = new TaskCompletionSource<TResult>(state);

            task.ContinueWith(_ =>
            {
                cs.TrySetFrom(_);

                // invoke callback, which should invoke EndToAsync
                if (callback != null)
                    ExecutionContext.Run(ec, __ => callback((Task<TResult>)__), cs.Task);
            });

            return cs.Task;
        }

        /// <summary>
        /// Implements the Begin method of the Asynchronous Programming Model pattern for a <see cref="Task"/>. 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IAsyncResult ToAsyncBegin(this Task task, AsyncCallback callback, object state)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var ec = ExecutionContext.Capture();
            var cs = new TaskCompletionSource<object>(state);

            task.ContinueWith(_ =>
            {
                cs.TrySetFrom(_);

                // invoke callback, which should invoke EndToAsync
                if (callback != null)
                    ExecutionContext.Run(ec, __ => callback((Task)__), cs.Task);
            });

            return cs.Task;
        }

        /// <summary>
        /// Implements the End method of the Asynchronous Programming Model pattern for a <see cref="Task"/>. 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static TResult ToAsyncEnd<TResult>(this Task<TResult> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Implements the End method of the Asynchronous Programming Model pattern for a <see cref="Task"/>. 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static void ToAsyncEnd(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

    }

}
