using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Engine
{
	public interface IREndpoint
	{
		Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, int maxLength, bool addToHistoty);
		Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string message, bool isError); // Matches Rstart->WriteConsoleEx callback
	}
}
