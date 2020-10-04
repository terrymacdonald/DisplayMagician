using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosPlus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary> 
    /// A class used by Tasks to report progress or completion updates back to the UI. 
    /// </summary> 
    public sealed class ProgressReporter
    {
        /// <summary> 
        /// The underlying scheduler for the UI's synchronization context. 
        /// </summary> 
        private readonly TaskScheduler scheduler;

        /// <summary> 
        /// Initializes a new instance of the <see cref="ProgressReporter"/> class.
        /// This should be run on a UI thread. 
        /// </summary> 
        public ProgressReporter()
        {
            this.scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        /// <summary> 
        /// Gets the task scheduler which executes tasks on the UI thread. 
        /// </summary> 
        public TaskScheduler Scheduler
        {
            get { return this.scheduler; }
        }

        /// <summary> 
        /// Reports the progress to the UI thread. This method should be called from the task.
        /// Note that the progress update is asynchronous with respect to the reporting Task.
        /// For a synchronous progress update, wait on the returned <see cref="Task"/>. 
        /// </summary> 
        /// <param name="action">The action to perform in the context of the UI thread.
        /// Note that this action is run asynchronously on the UI thread.</param> 
        /// <returns>The task queued to the UI thread.</returns> 
        public Task ReportProgressAsync(Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, this.scheduler);
        }

        /// <summary> 
        /// Reports the progress to the UI thread, and waits for the UI thread to process
        /// the update before returning. This method should be called from the task. 
        /// </summary> 
        /// <param name="action">The action to perform in the context of the UI thread.</param> 
        public void ReportProgress(Action action)
        {
            this.ReportProgressAsync(action).Wait();
        }

        /// <summary> 
        /// Registers a UI thread handler for when the specified task finishes execution,
        /// whether it finishes with success, failiure, or cancellation. 
        /// </summary> 
        /// <param name="task">The task to monitor for completion.</param> 
        /// <param name="action">The action to take when the task has completed, in the context of the UI thread.</param> 
        /// <returns>The continuation created to handle completion. This is normally ignored.</returns> 
        public Task RegisterContinuation(Task task, Action action)
        {
            return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.None, this.scheduler);
        }

        /// <summary> 
        /// Registers a UI thread handler for when the specified task finishes execution,
        /// whether it finishes with success, failiure, or cancellation. 
        /// </summary> 
        /// <typeparam name="TResult">The type of the task result.</typeparam> 
        /// <param name="task">The task to monitor for completion.</param> 
        /// <param name="action">The action to take when the task has completed, in the context of the UI thread.</param> 
        /// <returns>The continuation created to handle completion. This is normally ignored.</returns> 
        public Task RegisterContinuation<TResult>(Task<TResult> task, Action action)
        {
            return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.None, this.scheduler);
        }

        /// <summary> 
        /// Registers a UI thread handler for when the specified task successfully finishes execution. 
        /// </summary> 
        /// <param name="task">The task to monitor for successful completion.</param> 
        /// <param name="action">The action to take when the task has successfully completed, in the context of the UI thread.</param> 
        /// <returns>The continuation created to handle successful completion. This is normally ignored.</returns> 
        public Task RegisterSucceededHandler(Task task, Action action)
        {
            return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, this.scheduler);
        }

        /// <summary> 
        /// Registers a UI thread handler for when the specified task successfully finishes execution
        /// and returns a result. 
        /// </summary> 
        /// <typeparam name="TResult">The type of the task result.</typeparam> 
        /// <param name="task">The task to monitor for successful completion.</param> 
        /// <param name="action">The action to take when the task has successfully completed, in the context of the UI thread.
        /// The argument to the action is the return value of the task.</param> 
        /// <returns>The continuation created to handle successful completion. This is normally ignored.</returns> 
        public Task RegisterSucceededHandler<TResult>(Task<TResult> task, Action<TResult> action)
        {
            return task.ContinueWith(t => action(t.Result), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, this.Scheduler);
        }

        /// <summary> 
        /// Registers a UI thread handler for when the specified task becomes faulted. 
        /// </summary> 
        /// <param name="task">The task to monitor for faulting.</param> 
        /// <param name="action">The action to take when the task has faulted, in the context of the UI thread.</param> 
        /// <returns>The continuation created to handle faulting. This is normally ignored.</returns> 
        public Task RegisterFaultedHandler(Task task, Action<Exception> action)
        {
            return task.ContinueWith(t => action(t.Exception), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, this.Scheduler);
        }

        /// <summary> 
        /// Registers a UI thread handler for when the specified task becomes faulted. 
        /// </summary> 
        /// <typeparam name="TResult">The type of the task result.</typeparam> 
        /// <param name="task">The task to monitor for faulting.</param> 
        /// <param name="action">The action to take when the task has faulted, in the context of the UI thread.</param> 
        /// <returns>The continuation created to handle faulting. This is normally ignored.</returns> 
        public Task RegisterFaultedHandler<TResult>(Task<TResult> task, Action<Exception> action)
        {
            return task.ContinueWith(t => action(t.Exception), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, this.Scheduler);
        }

        /// <summary> 
        /// Registers a UI thread handler for when the specified task is cancelled. 
        /// </summary> 
        /// <param name="task">The task to monitor for cancellation.</param> 
        /// <param name="action">The action to take when the task is cancelled, in the context of the UI thread.</param> 
        /// <returns>The continuation created to handle cancellation. This is normally ignored.</returns> 
        public Task RegisterCancelledHandler(Task task, Action action)
        {
            return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, this.Scheduler);
        }

        /// <summary> 
        /// Registers a UI thread handler for when the specified task is cancelled. 
        /// </summary> 
        /// <typeparam name="TResult">The type of the task result.</typeparam> 
        /// <param name="task">The task to monitor for cancellation.</param> 
        /// <param name="action">The action to take when the task is cancelled, in the context of the UI thread.</param> 
        /// <returns>The continuation created to handle cancellation. This is normally ignored.</returns> 
        public Task RegisterCancelledHandler<TResult>(Task<TResult> task, Action action)
        {
            return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, this.Scheduler);
        }
    }
}
