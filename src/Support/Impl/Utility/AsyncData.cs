using System;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Utility.Definitions;

namespace Microsoft.R.Support.Utility
{
    /// <summary>
    /// Implements pattern similar to EAP (event async pattern).
    /// direct use of EAP classes such as AsyncCompletedEventHandler
    /// is not very convenient since we need additional information
    /// attached to the data such as when data is used in intellisense
    /// we need to know completion context with the completion session
    /// and which specific completion object to populated with the data.
    /// </summary>
    public abstract class AsyncData<T>
    {
        private T _data;

        /// <summary>
        /// Method to call when data becomes available
        /// </summary>
        private Action<T> _dataReadyCallBack;

        /// <summary>
        /// Data that becomes available asynchronously
        /// </summary>
        public T Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Called by the derived class that actually fetches data. 
        /// Transitions to the UI thread, sets the data field and 
        /// fires data ready event.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void SetData(T data)
        {
            EditorShell.DispatchOnUIThread(() =>
            {
                _data = data;
                _dataReadyCallBack(_data);
            });
        }

        /// <summary>
        /// Notifies derived class from UI thread
        /// that the data is ready.
        /// </summary>
        protected virtual void OnDataReady(T data)
        {
        }

        public AsyncData(Action _dataReadyCallBack)
        {
        }

        /// <summary>
        /// Creates async object in the immediate ready state.
        /// </summary>
        /// <param name="data"></param>
        public AsyncData(T data)
        {
            SetData(data);
        }
    }
}
