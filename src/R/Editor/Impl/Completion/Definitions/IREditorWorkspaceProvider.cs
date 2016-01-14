namespace Microsoft.R.Editor.Completion.Definitions {
    /// <summary>
    /// Represents R workspace to the editor
    /// </summary>
    public interface IREditorWorkspaceProvider {
        IREditorWorkspace GetWorkspace();
    }
}
