using System;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Engine
{
	public interface IRSession
	{
		event EventHandler<RBeforeRequestEventArgs> BeforeRequest;
		event EventHandler<RResponseEventArgs> Response;
		event EventHandler<RErrorEventArgs> Error;
		Task<IRSessionRequest> CreateRequest(bool isVisible = true, bool isContextBound = false);
	}
}