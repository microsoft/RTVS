using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Engine
{
	public interface IRSessionRequest
	{
		string Prompt { get; }
		int MaxLength { get; }
		IReadOnlyCollection<IRContext> Contexts { get; }
		Task<string> Send(string messageText);
	}
}