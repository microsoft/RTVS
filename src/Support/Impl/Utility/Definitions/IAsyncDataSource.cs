using System;

namespace Microsoft.R.Support.Utility.Definitions
{
    public interface IAsyncDataSource<T>
    {
        /// <summary>
        /// Data that becomes available asynchronously
        /// </summary>
        T Data { get; }

        /// <summary>
        /// Fires when data becomes available
        /// </summary>
        event EventHandler<T> DataReady;

        /// <summary>
        /// Any user data
        /// </summary>
        object Tag { get; set; }
    }
}
