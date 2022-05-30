using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TetraPak.XP
{
    /// <summary>
    ///   Provides convenient helper methods for working with <see cref="Task"/>s.
    /// </summary>
    public static class TaskHelper
    {
        /// <summary>
        ///   Examines the status of a <see cref="TaskCompletionSource{TResult}"/> and awaits its
        ///   completion when applicable (the TCS might have already ran to completion) and then returns it.
        /// </summary>
        /// <param name="tcs">
        ///    The <see cref="TaskCompletionSource{TResult}"/> to be awaited.
        /// </param>
        /// <typeparam name="T">
        ///   The task completion source's result type.
        /// </typeparam>
        /// <returns>
        ///   The specified <see cref="TaskCompletionSource{TResult}"/> (<paramref name="tcs"/>). See remarks.
        /// </returns>
        /// <remarks>
        ///   The method always returns the specified <see cref="TaskCompletionSource{TResult}"/> <paramref name="tcs"/>.
        ///   Caller's should <i>not</i> rely on the instance being assigned after completion as it is a common
        ///   pattern for many asynchronous operations to create the TCS while initiating and then removing is
        ///   upon completion.
        /// </remarks>
        public static async Task<TaskCompletionSource<T>> AwaitCompletionAsync<T>(this TaskCompletionSource<T> tcs)
        {
            if (!tcs.IsFinished())
            {
                await tcs.Task.ConfigureAwait(false);
            }

            return tcs;
        }

        /// <summary>
        ///   Gets a value indicating whether the <see cref="TaskCompletionSource{TResult}"/>
        ///   is finished (ran to completion, successfully or not).   
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the task has finished; otherwise <c>false</c>.
        /// </returns>
        public static bool IsFinished<T>(this TaskCompletionSource<T> tcs) => tcs.Task.Status >= TaskStatus.RanToCompletion;

        public static bool IsCanceled<T>(this TaskCompletionSource<T> tcs) => tcs.Task.Status == TaskStatus.Canceled;

        public static bool IsFaulted<T>(this TaskCompletionSource<T> tcs) => tcs.Task.Status == TaskStatus.Faulted;

        /// <summary>
        ///   Blocks the thread while waiting for a result.
        /// </summary>
        /// <param name="task">
        ///   The task to be awaited.
        /// </param>
        /// <param name="timeout">
        ///   (optional)<br/>
        ///   Specifies a timeout. If operation times our a default result will be sent back.
        /// </param>
        /// <param name="cts">
        ///   (optional)<br/>
        ///   A cancellation token source, allowing operation cancellation (from a different thread).
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="task"/> ran to completion; otherwise <c>false</c>.
        /// </returns>
        public static bool Await(
            this Task task,
            CancellationTokenSource? cts = null,
            TimeSpan? timeout = null)
        {
            var awaiter = task.ConfigureAwait(false).GetAwaiter();
            var useTimeout = timeout.HasValue ? XpDateTime.Now.Add(timeout.Value) : DateTime.MaxValue;
            var isTimedOut = false;
            var isCancelled = false;
            while (!awaiter.IsCompleted && !isTimedOut && !isCancelled)
            {
                Task.Delay(10);
                isTimedOut = XpDateTime.Now >= useTimeout;
                isCancelled = cts?.IsCancellationRequested ?? false;
            }

            return task.Status >= TaskStatus.RanToCompletion;
        }

        /// <summary>
        ///   Awaits the outcome of a task completion source while applying cancellation and/or timout support. 
        /// </summary>
        /// <param name="tcs">
        ///   The extended task completion source.
        /// </param>
        /// <param name="timeout">
        ///   (optional; default=<see cref="TimeSpan.MaxValue"/>)<br/>
        ///   A timeout value that will automatically cancel the task if exceeded.
        /// </param>
        /// <param name="cts">
        ///   (optional)<br/>
        ///   Allows manual cancellation.
        /// </param>
        /// <typeparam name="T">
        ///   The expected value type.
        /// </typeparam>
        /// <returns>
        ///   An <see cref="Outcome"/> value, signalling success/failure while also carrying the requested
        ///   result on success; otherwise an <see cref="Exception"/>.
        /// </returns>
        public static Task<Outcome<T>> GetOutcomeAsync<T>(
            this TaskCompletionSource<T> tcs,
            CancellationTokenSource? cts = null,
            TimeSpan? timeout = null) 
            =>
            tcs.getOutcomeAsync(cts, timeout);
        
        public static Task<Outcome<T>> GetOutcomeAsync<T>(
            this TaskCompletionSource<Outcome<T>> tcs,
            CancellationTokenSource? cts = null,
            TimeSpan? timeout = null) 
            =>
            tcs.getOutcomeAsync(cts, timeout);


        /// <summary>
        ///   Blocks execution while awaiting the outcome of a task completion source while applying cancellation and/or timout support. 
        /// </summary>
        /// <param name="tcs">
        ///   The <see cref="TaskCompletionSource{TResult}"/> in use for signalling result is available.
        /// </param>
        /// <param name="timeout">
        ///   (optional)<br/>
        ///   Specifies a timeout. If operation times our a default result will be sent back.
        /// </param>
        /// <param name="cts">
        ///   (optional)<br/>
        ///   A cancellation token source, allowing operation cancellation (from a different thread).
        /// </param>
        /// <typeparam name="T">
        ///   The type of result being requested.
        /// </typeparam>
        /// <returns>
        ///   An <see cref="Outcome"/> value, signalling success/failure while also carrying the requested
        ///   result on success; otherwise an <see cref="Exception"/>.
        /// </returns>
        public static Outcome<T> GetOutcome<T>(
            this TaskCompletionSource<T> tcs, 
            CancellationTokenSource? cts = null,
            TimeSpan? timeout = null)
        {
            // // todo This method is unused/untested
            if (tcs.IsFinished())
                return tcs.IsCanceled()
                    ? Outcome<T>.Cancel()
                    : tcs.IsFaulted()
                        ? tcs.Task.Exception is {} ? Outcome<T>.Fail(tcs.Task.Exception) : Outcome<T>.Fail("Operation failed") 
                        : Outcome<T>.Success(tcs.Task.Result);

            Task.Run(async () => await tcs.GetOutcomeAsync(cts, timeout));
            tcs.Task.Wait();
            return tcs.IsCanceled()
                ? Outcome<T>.Cancel()
                : tcs.IsFaulted()
                    ? tcs.Task.Exception is {} ? Outcome<T>.Fail(tcs.Task.Exception) : Outcome<T>.Fail("Operation failed") 
                    : Outcome<T>.Success(tcs.Task.Result);
            
            //
            // // var isTimedOut = false; obsolete
            // var isCancelled = false;
            // T result;
            // if (tcs.Task.Status < TaskStatus.RanToCompletion)
            // {
            //     var awaiter = tcs.Task.ConfigureAwait(false).GetAwaiter();
            //     if (cts is { } && timeout is { })
            //     {
            //         cts.CancelAfter(timeout.Value);
            //     }
            //     // var useTimeout = timeout.HasValue 
            //     //     ? XpDateTime.Now.Add(timeout.Value) 
            //     //     : DateTime.MaxValue;
            //     //
            //
            //     result = tcs.Task.GetAwaiter().GetResult();
            //     // while (!awaiter.IsCompleted && isCancelled)
            //     // {
            //     //     Task.Delay(10);
            //     //     // isTimedOut = XpDateTime.Now >= useTimeout;
            //     //     isCancelled = cts?.IsCancellationRequested ?? false;
            //     // }
            // }
            //
            // isCancelled = cts?.IsCancellationRequested ?? false;
            //
            // switch (tcs.Task.Status)
            // {
            //     case TaskStatus.Created:
            //     case TaskStatus.WaitingForActivation:
            //     case TaskStatus.WaitingToRun:
            //     case TaskStatus.Running:
            //     case TaskStatus.WaitingForChildrenToComplete:
            //         return isCancelled
            //             ? Outcome<T>.Cancel() 
            //             : Outcome<T>.Fail("Result could not be created before operation timed out");
            //     
            //     case TaskStatus.RanToCompletion:
            //         return Outcome<T>.Success(tcs.Task.Result);
            //         
            //     case TaskStatus.Canceled:
            //         return Outcome<T>.Cancel("Result could not be created. Operation was cancelled");
            //         
            //     case TaskStatus.Faulted:
            //         return Outcome<T>.Fail(
            //             tcs.Task.Exception ?? new Exception("Result could not be created (unhandled error)"));
            //         
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }
        }
        
        static async Task<Outcome<T>> getOutcomeAsync<T>(
            this TaskCompletionSource<T> tcs,
            CancellationTokenSource? cts = null,
            TimeSpan? timeout = null)
        {
            var tasks = new List<Task>(new[] { tcs.Task });
            Task? timeoutTask = null;
            if (timeout is { })
            {
                cts?.CancelAfter(timeout.Value);
                timeoutTask = Task.Delay(timeout.Value);
                tasks.Add(timeoutTask);
            }

            try
            {
                await Task.WhenAny(tasks);
                var isTimedOut = timeoutTask is 
                                     { Status: TaskStatus.RanToCompletion } || (cts?.IsCancellationRequested ?? false);
                if (isTimedOut)
                    return Outcome<T>.Cancel("Operation timed out");

                var value = await tcs.Task;
                return tcs.Task.IsCanceled
                    ? Outcome<T>.Cancel("Operation timed out")
                    : Outcome<T>.Success(value!);
            }
            catch (Exception ex)
            {
                return tcs.Task.IsCanceled
                    ? Outcome<T>.Cancel()
                    : Outcome<T>.Fail(ex);
            }
        }
        
        static async Task<Outcome<T>> getOutcomeAsync<T>(
            this TaskCompletionSource<Outcome<T>> tcs,
            CancellationTokenSource? cts = null,
            TimeSpan? timeout = null)
        {
            var tasks = new List<Task>(new[] { tcs.Task });
            Task? timeoutTask = null;
            if (timeout is { })
            {
                cts?.CancelAfter(timeout.Value);
                timeoutTask = Task.Delay(timeout.Value);
                tasks.Add(timeoutTask);
            }

            try
            {
                await Task.WhenAny(tasks);
                var isTimedOut = timeoutTask is 
                    { Status: TaskStatus.RanToCompletion } || (cts?.IsCancellationRequested ?? false);
                if (isTimedOut)
                    return Outcome<T>.Cancel("Operation timed out");

                var outcome = await tcs.Task;
                return tcs.Task.IsCanceled
                    ? Outcome<T>.Cancel("Operation timed out")
                    : outcome;
            }
            catch (Exception ex)
            {
                return tcs.Task.IsCanceled
                    ? Outcome<T>.Cancel()
                    : Outcome<T>.Fail(ex);
            }
        }

    }
}