using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hjerpbakk.AsyncMethodCaller {
    /// <summary>
    /// Used to call methods asynchronously and continue with other methods
    /// after execution completes. Very useful in ViewModels.
    /// </summary>
    public class AsyncMethodCaller {
        /// <summary>
        /// The final <see cref="Task"/> to be executed, can be awaited. 
        /// </summary>
        protected Task CallbackTask;

        private readonly TaskScheduler taskScheduler;

        public AsyncMethodCaller() {
            taskScheduler = SynchronizationContext.Current == null ? TaskScheduler.Default : TaskScheduler.FromCurrentSynchronizationContext();
        }

        /// <summary>
        /// Calls the given method asynchronously, and completes with the continueWith <see cref="T:System.Action`1{T}"/>
        /// if the method call succeeds or the failWith <see cref="T:System.Action`1{T}"/> if the method throws an exception.
        /// </summary>
        /// <typeparam name="T">The return type of the method to be called.</typeparam>
        /// <param name="methodToCall">The method to be called asynchronously.</param>
        /// <param name="continueWith">The method to be continued with if the previously called method executed successfully.</param>
        /// <param name="failWith">The method to be continued with if the previously called method threw an exception.</param>
        public virtual void CallMethodAndContinue<T>(Func<T> methodToCall, Action<T> continueWith, Action<Exception> failWith) {
            try {
                var serviceTask = GetTask(methodToCall, continueWith, failWith);
                serviceTask.Start();
            } catch (Exception exception) {
                failWith(exception);
            }
        }

        /// <summary>
        /// Calls the given method asynchronously, and completes with the continueWith <see cref="Action"/>
        /// if the method call succeeds or the failWith <see cref="Action"/> if the method throws an exception.
        /// </summary>
        /// <param name="methodToCall">The method to be called asynchronously.</param>
        /// <param name="continueWith">The method to be continued with if the previously called method executed successfully.</param>
        /// <param name="failWith">The method to be continued with if the previously called method threw an exception.</param>
        public virtual void CallMethodAndContinue(Action methodToCall, Action continueWith, Action<Exception> failWith) {
            try {
                var serviceTask = GetTask(methodToCall, continueWith, failWith);
                serviceTask.Start();
            } catch (Exception exception) {
                failWith(exception);
            }
        }

        /// <summary>
        /// Gets a <see cref="Task"/> with callbacks for successful completion or error handling.
        /// </summary>
        /// <typeparam name="T">The return type of the method to be called.</typeparam>
        /// <param name="methodToCall">The method to be called asynchronously.</param>
        /// <param name="continueWith">The method to be continued with if the previously called method executed successfully.</param>
        /// <param name="failWith">The method to be continued with if the previously called method threw an exception.</param>
        /// <returns>A <see cref="Task"/> with callbacks for successful completion or error handling.</returns>
        protected Task GetTask<T>(Func<T> methodToCall, Action<T> continueWith, Action<Exception> failWith) {
            var serviceTask = new Task<T>(methodToCall);
            CallbackTask = serviceTask.ContinueWith(antecedent => {
                if (antecedent.IsFaulted) {
                    failWith(antecedent.Exception);
                } else {
                    continueWith(serviceTask.Result);
                }
            }, taskScheduler);
            return serviceTask;
        }

        /// <summary>
        /// Gets a <see cref="Task"/> with callbacks for successful completion or error handling.
        /// </summary>
        /// <param name="methodToCall">The method to be called asynchronously.</param>
        /// <param name="continueWith">The method to be continued with if the previously called method executed successfully.</param>
        /// <param name="failWith">The method to be continued with if the previously called method threw an exception.</param>
        /// <returns>A <see cref="Task"/> with callbacks for successful completion or error handling.</returns>
        protected Task GetTask(Action methodToCall, Action continueWith, Action<Exception> failWith) {
            var serviceTask = new Task(methodToCall);
            CallbackTask = serviceTask.ContinueWith(antecedent => {
                if (antecedent.IsFaulted) {
                    failWith(antecedent.Exception);
                } else {
                    continueWith();
                }
            }, taskScheduler);
            return serviceTask;
        }
    }
}
