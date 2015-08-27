using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host {
    /// <summary>
    /// Representation of <c>struct RCTXT</c> in R.
    /// </summary>
    public interface IRContext {
        RContextType CallFlag { get; }
    }

    internal class RContext : IRContext {
        public RContextType CallFlag { get; set; }
    }

    [Flags]
    public enum RContextType {
        TopLevel = 0x0,
        Next = 0x1,
        Break = 0x2,
        Function = 0x4,
        CCode = 0x8,
        Browser = 0x10,
        Restart = 0x20,
        Builtin = 0x40,
        Loop = Break | Next,
        Return = Function | CCode,
        Generic = Function | Browser
    }
}
