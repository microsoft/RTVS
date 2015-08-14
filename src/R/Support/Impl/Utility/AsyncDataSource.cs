using System;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;

namespace Microsoft.R.Support.Utility
{
    public abstract class AsyncDataSource<T>
    {
        private T _data;

        /// <summary>
        /// Data that becomes available asynchronously
        /// </summary>
        public T Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Indictes if data is available
        /// </summary>
        public bool IsReady
        {
            get { return _data != null; }
        }

        /// <summary>
        /// Fires when data becomes available
        /// </summary>
        public event EventHandler<T> DataReady;

        /// <summary>
        /// Any user data
        /// </summary>
        public object Tag { get; set; }

        protected void SetData (T data)
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
    }
}
