
namespace Microsoft.Languages.Editor.Undo {
    /// <summary>
    /// Allows text buffer changes to be batched together in a single named action.
    /// This applies to a single text buffer.
    /// </summary>
    public interface ICompoundUndoAction {
        void Open(string name);
        void Close(bool discardChanges);

    }

    public interface ICompoundUndoActionOptions {
        // These can only be called between Open and Close
        void SetMergeDirections(bool mergePrevious, bool mergeNext);
        void SetUndoAfterClose(bool undoAfterClose);
    }
}
