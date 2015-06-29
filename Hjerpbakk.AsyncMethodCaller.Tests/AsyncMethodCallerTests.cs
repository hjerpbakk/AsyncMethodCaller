using System;
using NUnit.Framework;
using Hjerpbakk.AsyncMethodCaller.TestUtility;

namespace Hjerpbakk.AsyncMethodCaller.Tests {
	[TestFixture]
	public class AsyncMethodCallerTests {
		TestAsyncMethodCaller asyncMethodCaller;
		IServiceInterface service;
		ViewModel viewModel;

		static bool loadContentCalled;
		static bool saveCalled;

		[TearDown]
		public void Cleanup() {
			loadContentCalled = false;
			saveCalled = false;
		}

		[Test]
		public void CallMethodAndContinue_ServiceWithResultCompletes_ResultIsReturnedAndCallbackCalled() {
			InitWithService();

			viewModel.LoadContent();

			Assert.AreEqual("Loading content...", viewModel.MessageToUser);

			asyncMethodCaller.StartServiceAndWait();

			Assert.AreEqual("Content is 1", viewModel.MessageToUser);
			Assert.IsTrue(loadContentCalled);
			Assert.IsFalse(saveCalled);
		}

		[Test]
		public void CallMethodAndContinue_ServiceWithResultFails_ExceptionIsReturnedAndCallbackCalled() {
			InitWithFailingService();

			viewModel.LoadContent();

			Assert.AreEqual("Loading content...", viewModel.MessageToUser);

			asyncMethodCaller.StartServiceAndWait();

			Assert.AreEqual("Loading content failed: LoadingException Message", viewModel.MessageToUser);
			Assert.IsTrue(loadContentCalled);
			Assert.IsFalse(saveCalled);
		}

		[Test]
		public void CallMethodAndContinue_ServiceCompletes_CallbackCalled() {
			InitWithService();

			viewModel.Save();

			Assert.AreEqual("Saving content...", viewModel.MessageToUser);

			asyncMethodCaller.StartServiceAndWait();

			Assert.AreEqual("Content saved", viewModel.MessageToUser);
			Assert.IsTrue(saveCalled);
			Assert.IsFalse(loadContentCalled);
		}

		[Test]
		public void CallMethodAndContinue_ServiceFails_ExceptionIsReturnedAndCallbackCalled() {
			InitWithFailingService();

			viewModel.Save();

			Assert.AreEqual("Saving content...", viewModel.MessageToUser);

			asyncMethodCaller.StartServiceAndWait();

			Assert.AreEqual("Saving content failed: SavingException Message", viewModel.MessageToUser);
			Assert.IsTrue(saveCalled);
			Assert.IsFalse(loadContentCalled);
		}

		void InitWithService() {
			service = new ServiceImplmentation();
			Init();
		}

		void InitWithFailingService() {
			service = new FailingServiceImplementation();
			Init();
		}

		void Init() {
			// The TestAsyncMethodCaller can wait for asynchronous calls to complete. 
			asyncMethodCaller = new TestAsyncMethodCaller();
			viewModel = new ViewModel(asyncMethodCaller, service);
		}

		class ViewModel {
			readonly AsyncMethodCaller asyncMethodCaller;
			readonly IServiceInterface service;

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
				asyncMethodCaller.CallMethodAndContinue(service.Save,
					() => { MessageToUser = "Content saved"; },
					exception => { MessageToUser = "Saving content failed: " + exception.InnerException.Message; });
			}

			void LoadContentCompleted(int content) {
				MessageToUser = "Content is " + content;
			}

			void LoadContentFailed(Exception exception) {
				MessageToUser = "Loading content failed: " + exception.InnerException.Message;
			}
		}

		/// <summary>
		/// Example of a service interface.
		/// </summary>
		interface IServiceInterface {
			int LoadContent();
			void Save();
		}

		/// <summary>
		/// Example of failing service calls.
		/// </summary>
		class FailingServiceImplementation : IServiceInterface {
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
		class ServiceImplmentation : IServiceInterface {
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
