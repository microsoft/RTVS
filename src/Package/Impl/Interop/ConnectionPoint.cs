using System;
using System.Diagnostics;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudio.R.Package.Interop
{
    /// <summary>
    /// Class that encapsulates a connection point cookie for COM event handling.
    /// </summary>
    internal sealed class ConnectionPointCookie : IDisposable
    {
        private IConnectionPointContainer _connectionPointContainer;
        private IConnectionPoint _connectionPoint;
        private uint _cookie;

#if DEBUG
//        private string _callStack = "(none)";
//        private Type _eventInterface;
#endif

        /// <summary>
        /// Creates a connection point to of the given interface type
        /// which will call on a managed code sink that implements that interface.
        /// </summary>
        public ConnectionPointCookie(object source, object sink, Type eventInterface)
            : this(source, sink, eventInterface, true)
        {
        }

        /// <summary>
        /// Creates a connection point to of the given interface type
        /// which will call on a managed code sink that implements that interface.
        /// </summary>
        public ConnectionPointCookie(object source, object sink, Type eventInterface, bool throwException)
        {
            Exception ex = null;

            if (source is IConnectionPointContainer)
            {
                _connectionPointContainer = (IConnectionPointContainer)source;

                try
                {
                    Guid tmp = eventInterface.GUID;
                    _connectionPointContainer.FindConnectionPoint(ref tmp, out _connectionPoint);
                }
                catch
                {
                    _connectionPoint = null;
                }

                if (_connectionPoint == null)
                {
                    ex = new NotSupportedException();
                }
                else if (sink == null || !eventInterface.IsInstanceOfType(sink))
                {
                    ex = new InvalidCastException();
                }
                else
                {
                    try
                    {
                        _connectionPoint.Advise(sink, out _cookie);
                    }
                    catch
                    {
                        _cookie = 0;
                        _connectionPoint = null;
                        ex = new Exception();
                    }
                }
            }
            else
            {
                ex = new InvalidCastException();
            }

            if (throwException && (_connectionPoint == null || _cookie == 0))
            {
                Dispose();

                if (ex == null)
                {
                    throw new ArgumentException("Exception null, but cookie was zero or the connection point was null");
                }
                else
                {
                    throw ex;
                }
            }

#if DEBUG
            //_callStack = Environment.StackTrace;
            //this._eventInterface = eventInterface;
#endif
        }

#if DEBUG
        /// <summary>
        /// Debug finalizer to catch a missing call to Dispose()
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1821:RemoveEmptyFinalizers")]
        ~ConnectionPointCookie()
        {
            Debug.Assert(_connectionPoint == null || _cookie == 0,
                "We should never finalize an active connection point.");// (Interface = " +
                //_eventInterface.FullName +
                //"), allocating code (see stack) is responsible for unhooking the ConnectionPoint by calling Disconnect. Hookup Stack =\r\n" +
                //_callStack);
        }
        
#endif

        public void Dispose()
        {
            try
            {
                if (_connectionPoint != null && _cookie != 0)
                {
                    _connectionPoint.Unadvise(_cookie);
                }
            }
            finally
            {
                _cookie = 0;
                _connectionPoint = null;
                _connectionPointContainer = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
