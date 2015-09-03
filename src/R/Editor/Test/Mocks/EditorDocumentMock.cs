using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Tree.Definitions;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class EditorDocumentMock : IREditorDocument
    {
        public EditorDocumentMock(IEditorTree tree)
        {
            EditorTree = tree;
            ServiceManager.AddService<IREditorDocument>(this, tree.TextBuffer);
        }

        public IEditorTree EditorTree { get; private set; }

        public bool IsClosed { get; private set; }

        public bool IsMassiveChangeInProgress
        {
            get { return false; }
        }

        public ITextBuffer TextBuffer
        {
            get { return EditorTree.TextBuffer; }
        }

        public IWorkspace Workspace
        {
            get { return null; }
        }

        public IWorkspaceItem WorkspaceItem
        {
            get { return null; }
        }

#pragma warning disable 67
        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;
        public event EventHandler<EventArgs> DocumentClosing;
        public event EventHandler<EventArgs> MassiveChangeBegun;
        public event EventHandler<EventArgs> MassiveChangeEnded;

        public void BeginMassiveChange()
        {
        }

        public void Dispose()
        {
        }

        public bool EndMassiveChange()
        {
            return true;
        }
    }
}
