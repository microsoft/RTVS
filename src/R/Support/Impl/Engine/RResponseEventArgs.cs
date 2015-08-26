using System;
using System.Collections.Generic;

namespace Microsoft.R.Support.Engine
{
	public class RResponseEventArgs : EventArgs
	{
		public IReadOnlyCollection<IRContext> Contexts { get; }
		public string Message { get; }

		public RResponseEventArgs(IReadOnlyCollection<IRContext> contexts, string message)
		{
			Contexts = contexts;
			Message = message;
		}
	}
}