using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Undo;

namespace Microsoft.Languages.Editor.Application.Undo
{
    [ExcludeFromCodeCoverage]
    internal class CompoundAction : ICompoundUndoAction
    {
        public void Open(string name)
        {
        }

        public void Close(bool discardChanges)
        {
        }
    }
}
