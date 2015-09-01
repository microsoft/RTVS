using System;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client
{
    public interface IRSessionProvider : IDisposable
    {
        IRSession Create(int sessionId);
        IRSession Current { get; }
    }
}