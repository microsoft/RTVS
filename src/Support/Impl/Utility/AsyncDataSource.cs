using System;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Utility.Definitions;

namespace Microsoft.R.Support.Utility
{
    /// <summary>
    /// Represents data that will code asynchronously.
    /// When data is ready object will file DataReady event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AsyncDataSource<T> : IAsyncDataSource<T>
    {
        private T _data;

        #region IAsyncDataSource
        /// <summary>
        /// Data that becomes available asynchronously
        /// </summary>
        public T Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Fires when data becomes available
        /// </summary>
        public event EventHandler<T> DataReady;

        /// <summary>
        /// Any user data
        /// </summary>
        public object Tag { get; set; }
        #endregion

        protected virtual void SetData(T data)
        {
            EditorShell.DispatchOnUIThread(() =>
            {
                _data = data;

                if (DataReady != null)
                {
                    DataReady(this, data);
                }
            });
        }

        public AsyncDataSource()
        {
        }

        /// <summary>
        /// Creates async object in the immediate ready state.
        /// </summary>
        /// <param name="data"></param>
        protected AsyncDataSource(T data)
        {
            _data = data;
        }
    }
}
