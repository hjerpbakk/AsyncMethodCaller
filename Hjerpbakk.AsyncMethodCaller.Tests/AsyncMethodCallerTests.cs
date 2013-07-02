using System;
using Hjerpbakk.AsyncMethodCaller.TestUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hjerpbakk.AsyncMethodCaller.Tests {
    [TestClass]
    public class AsyncMethodCallerTests {
        private TestAsyncMethodCaller asyncMethodCaller;
        private IServiceInterface service;
        private ViewModel viewModel;

        private static bool loadContentCalled;
        private static bool saveCalled;

        [TestCleanup]
        public void Cleanup() {
            loadContentCalled = false;
            saveCalled = false;
        }

        [TestMethod]
        public void CallMethodAndContinue_ServiceWithResultCompletes_ResultIsReturnedAndCallbackCalled() {
            InitWithService();

            viewModel.LoadContent();

            Assert.AreEqual("Loading content...", viewModel.MessageToUser);

            asyncMethodCaller.StartServiceAndWait();

            Assert.AreEqual("Content is 1", viewModel.MessageToUser);
            Assert.IsTrue(loadContentCalled);
            Assert.IsFalse(saveCalled);
        }

        [TestMethod]
        public void CallMethodAndContinue_ServiceWithResultFails_ExceptionIsReturnedAndCallbackCalled() {
            InitWithFailingService();

            viewModel.LoadContent();

            Assert.AreEqual("Loading content...", viewModel.MessageToUser);

            asyncMethodCaller.StartServiceAndWait();

            Assert.AreEqual("Loading content failed: LoadingException Message", viewModel.MessageToUser);
            Assert.IsTrue(loadContentCalled);
            Assert.IsFalse(saveCalled);
        }

        [TestMethod]
        public void CallMethodAndContinue_ServiceCompletes_CallbackCalled() {
            InitWithService();

            viewModel.Save();

            Assert.AreEqual("Saving content...", viewModel.MessageToUser);

            asyncMethodCaller.StartServiceAndWait();

            Assert.AreEqual("Content saved", viewModel.MessageToUser);
            Assert.IsTrue(saveCalled);
            Assert.IsFalse(loadContentCalled);
        }

        [TestMethod]
        public void CallMethodAndContinue_ServiceFails_ExceptionIsReturnedAndCallbackCalled() {
            InitWithFailingService();

            viewModel.Save();

            Assert.AreEqual("Saving content...", viewModel.MessageToUser);

            asyncMethodCaller.StartServiceAndWait();

            Assert.AreEqual("Saving content failed: SavingException Message", viewModel.MessageToUser);
            Assert.IsTrue(saveCalled);
            Assert.IsFalse(loadContentCalled);
        }

        private void InitWithService() {
            service = new ServiceImplmentation();
            Init();
        }

        private void InitWithFailingService() {
            service = new FailingServiceImplementation();
            Init();
        }

        private void Init() {
            // The TestAsyncMethodCaller can wait for asynchronous calls to complete. 
            asyncMethodCaller = new TestAsyncMethodCaller();
            viewModel = new ViewModel(asyncMethodCaller, service);
        }

        private class ViewModel {
            private readonly AsyncMethodCaller asyncMethodCaller;
            private readonly IServiceInterface service;

            public string MessageToUser { get; private set; }

            /// <summary>
            /// The dependencies are injected.
            /// </summary>
            /// <param name="asyncMethodCaller">Use TestAsyncMethodCaller for tests, AsyncMethodCaller in production.</param>
            /// <param name="service">Inject the service for easier faking and faster tests.</param>
            public ViewModel(AsyncMethodCaller asyncMethodCaller, IServiceInterface service) {
                this.asyncMethodCaller = asyncMethodCaller;
                this.service = service;
            }

            public void LoadContent() {
                MessageToUser = "Loading content...";
                
                // Separate methods for each callback.
                asyncMethodCaller.CallMethodAndContinue(() => service.LoadContent(),
                    LoadContentCompleted,
                    LoadContentFailed);
            }

            public void Save() {
                MessageToUser = "Saving content...";

                // Example of inline callbacks.
                asyncMethodCaller.CallMethodAndContinue(() => service.Save(),
                    () => { MessageToUser = "Content saved"; },
                    exception => { MessageToUser = "Saving content failed: " + exception.InnerException.Message; });
            }

            private void LoadContentCompleted(int content) {
                MessageToUser = "Content is " + content;
            }

            private void LoadContentFailed(Exception exception) {
                MessageToUser = "Loading content failed: " + exception.InnerException.Message;
            }
        }

        /// <summary>
        /// Example of a service interface.
        /// </summary>
        private interface IServiceInterface {
            int LoadContent();
            void Save();
        }

        /// <summary>
        /// Example of failing service calls.
        /// </summary>
        private class FailingServiceImplementation : IServiceInterface {
            public int LoadContent() {
                loadContentCalled = true;
                throw new Exception("LoadingException Message");
            }

            public void Save() {
                saveCalled = true;
                throw new Exception("SavingException Message");
            }
        }

        /// <summary>
        /// Example of successful service calls.
        /// </summary>
        private class ServiceImplmentation : IServiceInterface {
            public int LoadContent() {
                loadContentCalled = true;
                return 1;
            }

            public void Save() {
                saveCalled = true;
            }
        }
    }
}
