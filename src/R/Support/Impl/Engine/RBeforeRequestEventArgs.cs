using System;
using System.Collections.Generic;

namespace Microsoft.R.Support.Engine
{
	public class RBeforeRequestEventArgs : EventArgs
	{
		public IReadOnlyCollection<IRContext> Contexts { get; }
		public string Prompt { get; }
		public int MaxLength { get; }
		public bool AddToHistoty { get; }

		public RBeforeRequestEventArgs(IReadOnlyCollection<IRContext> contexts, string prompt, int maxLength, bool addToHistoty)
		{
			Contexts = contexts;
			Prompt = prompt;
			MaxLength = maxLength;
			AddToHistoty = addToHistoty;
		}
	}
}