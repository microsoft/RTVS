// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.R.Package.Workspace;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.Markdown.Editor.Document;

namespace Microsoft.VisualStudio.R.Package.Document.Markdown {
    internal class VsMdEditorDocument : MdEditorDocument {
        private IEditorInstance _editorInstance;
        private VsWorkspaceItem _workspaceItem;

        public VsMdEditorDocument(IEditorInstance editorInstance)
            : base(editorInstance.ViewBuffer, editorInstance.WorkspaceItem) {

            _editorInstance = editorInstance;
            _workspaceItem = editorInstance.WorkspaceItem as VsWorkspaceItem;

            ServiceProvider = ServiceProvider.GlobalProvider;
            ServiceManager.AddService<VsMdEditorDocument>(this, TextBuffer);
        }

        public override void Close() {
            ServiceManager.RemoveService<VsMdEditorDocument>(TextBuffer);

            base.Close();

            if (_editorInstance != null) {
                _editorInstance.Dispose();
                _editorInstance = null;
            }
        }

        public IVsHierarchy Hierarchy { get { return _workspaceItem.Hierarchy; } }
        public VSConstants.VSITEMID ItemId { get { return _workspaceItem.ItemId; } }

        public ServiceProvider ServiceProvider { get; private set; }

        public string FileName {
            get { return _workspaceItem.Path; }
        }

        /// <summary>
        /// Detemines if a given file is currently an active document in the IDE
        /// </summary>
        public bool IsActive {
            get {
                var windowFrame = ViewUtilities.GetActiveFrame();
                if (windowFrame != null) {
                    object itemidObject;
                    windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out itemidObject);

                    if ((itemidObject is int) && (int)itemidObject == (int)this.ItemId) {
                        object unkHierarchy;
                        windowFrame.GetIUnknownProperty(__VSFPROPID.VSFPROPID_Hierarchy, out unkHierarchy);

                        if ((unkHierarchy is IVsHierarchy) && unkHierarchy.Equals(Hierarchy)) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public string[] FileNames {
            get { return new string[] { _workspaceItem.Path }; }
        }
    }
}