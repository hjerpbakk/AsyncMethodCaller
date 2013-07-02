using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hjerpbakk.AsyncMethodCaller.TestUtility {
    /// <summary>
    /// Helps with testing of behavior in WPF ViewModels.
    /// </summary>
    public class TestAsyncMethodCaller : AsyncMethodCaller {
        private Task serviceTask;

        /// <summary>
        /// Starts the methodToCall asynchronously and waits for its completion.
        /// </summary>
        public void StartServiceAndWait() {
            serviceTask.Start();
            CallbackTask.Wait();
        }

        /// <summary>
        /// Readies the methodToCall and sets callbacks for successful completion or error handling.
        /// </summary>
        /// <typeparam name="T">The return type of the method to be called.</typeparam>
        /// <param name="methodToCall">The method to be called asynchronously.</param>
        /// <param name="continueWith">The method to be continued with if the previously called method executed successfully.</param>
        /// <param name="failWith">The method to be continued with if the previously called method threw an exception.</param>
        public override void CallMethodAndContinue<T>(Func<T> methodToCall, Action<T> continueWith, Action<Exception> failWith) {
            serviceTask = GetTask(methodToCall, continueWith, failWith);
        }

        /// <summary>
        /// Readies the methodToCall and sets callbacks for successful completion or error handling.
        /// </summary>
        /// <param name="methodToCall">The method to be called asynchronously.</param>
        /// <param name="continueWith">The method to be continued with if the previously called method executed successfully.</param>
        /// <param name="failWith">The method to be continued with if the previously called method threw an exception.</param>
        public override void CallMethodAndContinue(Action methodToCall, Action continueWith, Action<Exception> failWith) {
            serviceTask = GetTask(methodToCall, continueWith, failWith);
        }
    }
}
