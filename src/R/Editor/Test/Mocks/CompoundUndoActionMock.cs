using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Undo;

namespace Microsoft.R.Editor.Test.Mocks {
    [ExcludeFromCodeCoverage]
    public class CompoundUndoActionMock : ICompoundUndoAction {
        public void Dispose() { }
        public void Open(string name) { }
        public void Commit() { }
    }
}