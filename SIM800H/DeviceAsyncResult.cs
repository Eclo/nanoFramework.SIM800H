////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;
using System.Threading;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// An asynchronous result object
    /// </summary>
    public class DeviceAsyncResult : IAsyncResult
    {
        /// <summary>
        /// The possibly states of an AsyncResult
        /// </summary>
        private enum CompletedState
        {
            Pending = 0,
            CompletedSynchronously,
            CompletedAsynchronously
        }

        private readonly AsyncCallback _asyncCallback;

        /// <summary>
        /// The state object stored against this AsyncResult
        /// </summary>
        public object AsyncState { get; private set; }

        private int _completedState = (int)CompletedState.Pending;
        private bool _ending;

        private ManualResetEvent _asyncWaitHandle;
        private readonly object _asyncWaitHandleLock = new object();

        private Exception _exception;

        /// <summary>
        /// Returns <c>true</c> if this AsyncResult has been completed synchronously
        /// </summary>
        public bool CompletedSynchronously
        {
            get
            {
                return _completedState == (int)CompletedState.CompletedSynchronously;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if this AsyncResult has been completed
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return _completedState != (int)CompletedState.Pending;
            }
        }

        /// <summary>
        /// The WaitHandle for this AsyncResult
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_asyncWaitHandle == null)
                {
                    bool done = IsCompleted;

                    bool waitEventCreated = false;
                    lock (_asyncWaitHandleLock)
                    {
                        if (_asyncWaitHandle == null)
                        {
                            _asyncWaitHandle = new ManualResetEvent(done);
                            waitEventCreated = true;
                        }
                    }

                    if (waitEventCreated)
                    {
                        if (!done && IsCompleted)
                        {
                            _asyncWaitHandle.Set();
                        }
                    }
                }

                return _asyncWaitHandle;
            }
        }

        /// <summary>
        /// Creates an AsyncResult
        /// </summary>
        /// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
        /// <param name="asyncState">The state object to be stored against this AsyncResult, optional</param>
        public DeviceAsyncResult(AsyncCallback asyncCallback = null, object asyncState = null)
        {
            _asyncCallback = asyncCallback;
            AsyncState = asyncState;

            SIM800H.Instance.AddAsyncTask(this);
        }

        /// <summary>
        /// Finishes the asynchronous processing and throws an exception if one was generated
        /// <remarks>Blocks until the asynchronous processing has completed</remarks>
        /// </summary>
        public void End()
        {
            if (_ending)
            {
                throw new InvalidOperationException("End has already been called");
            }
            _ending = true;

            if (!IsCompleted)
            {
                AsyncWaitHandle.WaitOne();
                _asyncWaitHandle = null;
            }

            if (_exception != null)
            {
                throw _exception;
            }
        }

        /// <summary>
        /// Called when the asynchronous processing has been completed
        /// </summary>
        /// <param name="exception">The exception generated while processing, optional</param>
        /// <param name="completedSynchronously"><c>True</c> if the processing was completed synchronously, optional, defaults to <c>false</c></param>
        /// <returns>Returns <c>true</c> if this is the first time this method has been called on this AsyncResult</returns>
        protected bool Complete(Exception exception = null, bool completedSynchronously = false)
        {
            bool completed = false;

            CompletedState previousState = (CompletedState)Interlocked.Exchange(
                ref _completedState,
                completedSynchronously
                    ? (int)CompletedState.CompletedSynchronously
                    : (int)CompletedState.CompletedAsynchronously
                );
            if (previousState == CompletedState.Pending)
            {
                _exception = exception;

                if (_asyncWaitHandle != null)
                {
                    _asyncWaitHandle.Set();
                }

                if (_asyncCallback != null)
                {
                    _asyncCallback(this);
                }

                completed = true;
            }

            return completed;
        }

        /// <summary>
        /// The method used to perform the asynchronous processing
        /// </summary>
        public virtual void Process()
        {
            Complete();
        }
    }

}
