using System.Collections.Generic;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Editor.Completion.Definitions
{
    public interface IVariablesProvider
    {
        IReadOnlyCollection<INamedItemInfo> Variables { get; }

        IReadOnlyCollection<INamedItemInfo> GetMembers(string variableName);
    }
}
