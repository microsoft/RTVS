using System;
using System.Collections.Generic;

namespace Microsoft.R.Support.Engine
{
	public class RErrorEventArgs : EventArgs
	{
		public IReadOnlyCollection<IRContext> Contexts { get; }
		public string Message { get; }

		public RErrorEventArgs(IReadOnlyCollection<IRContext> contexts, string message)
		{
			Contexts = contexts;
			Message = message;
		}
	}
}