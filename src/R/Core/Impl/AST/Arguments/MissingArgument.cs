using System.Diagnostics;

namespace Microsoft.R.Core.AST.Arguments
{
    /// <summary>
    /// Represents missing argument like in a[,1]. This is 
    /// different from argument missing because of a syntax 
    /// error such as in 'func(,,' where third argument is 
    /// a 'stub' argument. <seealso cref="ErrorArgument"/>
    /// </summary>
    [DebuggerDisplay("Missing Argument [{Start}...{End})")]
    public class MissingArgument : CommaSeparatedItem
    {
        public override string ToString()
        {
            return "{Missing}";
        }
    }
}
