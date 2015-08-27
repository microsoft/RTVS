using System;

namespace Microsoft.R.Support.Engine
{
	public interface IRSessionProvider
	{
		IRSession Create(Guid sessionId);
		IRSession Current { get; }
	}
}