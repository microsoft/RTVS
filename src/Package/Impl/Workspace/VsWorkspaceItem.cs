using System;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Workspace
{
    /// <summary>
    /// Workspace item implementation based on VS project system.
    /// Provides host-agnostic implementation of the workspace
    /// functions for the editor layer.
    /// </summary>
    internal sealed class VsWorkspaceItem :
        IVsWorkspaceItem,
        IVsRunningDocTableEvents,
        IVsRunningDocTableEvents2,
        IDisposable
    {
        private IVsRunningDocumentTable _rdt;
        private uint _rdtCookie;
        private IVsHierarchy _hierarchy;
        private VSConstants.VSITEMID _itemId;

        public VsWorkspaceItem(string name, string filePath) :
            this(name, filePath, null, VSConstants.VSITEMID.Nil)
        {
        }

        public VsWorkspaceItem(string name, string filePath, IVsHierarchy hierarchy, VSConstants.VSITEMID itemId)
        {
            Moniker = name;
            Path = filePath;
            _hierarchy = hierarchy;
            _itemId = itemId;

            _rdt = AppShell.Current.GetGlobalService<IVsRunningDocumentTable>();
            _rdt.AdviseRunningDocTableEvents(this, out _rdtCookie);
        }

        private void EnsureInitialized()
        {
            if (_hierarchy == null)
            {
                VsFileInfo fileInfo = VsFileInfo.FromPath(Path);

                // During a rename, it's possible this object is constructed before the document's new name is fully registered.
                _hierarchy = fileInfo.Hierarchy;
                _itemId = (VSConstants.VSITEMID)fileInfo.HierarchyItemId;
            }
        }

        #region IDisposable
        public void Dispose()
        {
            if (_rdtCookie != 0 && _rdt != null)
            {
                _rdt.UnadviseRunningDocTableEvents(_rdtCookie);

                _rdtCookie = 0;
                _rdt = null;
            }
        }
        #endregion

        #region IWorkspaceItem Members

        /// <summary>
        /// Item moniker. For a disk-based document the same as PhysicalPath.
        /// May be something else for workspace items that are not disk items.
        /// </summary>
        public string Moniker { get; private set; }

        /// <summary>
        /// Physical path to the item on disk
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Fires when item name, path or other workspace parameters changed
        /// </summary>
        public event EventHandler<EventArgs> Changed;

        #endregion

        #region IVsWorkspaceItem

        /// <summary>
        /// Returns Visual Studio hierarchy this item belongs to
        /// </summary>
        public IVsHierarchy Hierarchy
        {
            get
            {
                EnsureInitialized();

                return _hierarchy;
            }
        }

        /// <summary>
        /// Visual Studio item id in the hierarchy
        /// </summary>
        public VSConstants.VSITEMID ItemId
        {
            get
            {
                EnsureInitialized();

                return _itemId;
            }
        }
        #endregion

        #region IVsRunningDocTableEvents
        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsRunningDocTableEvents2
        public int OnAfterAttributeChangeEx(uint docCookie, uint attributes, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            bool changed = false;

            VsFileInfo fileInfo = VsFileInfo.FromPath(Path);
            uint rdtCookie = fileInfo.RunningDocumentItemCookie;

            if (docCookie == rdtCookie)
            {
                if ((attributes & (uint)__VSRDTATTRIB.RDTA_MkDocument) != 0)
                {
                    Path = pszMkDocumentNew;
                    Moniker = Path;
                    changed = true;
                }

                if ((attributes & (uint)__VSRDTATTRIB.RDTA_ItemID) != 0)
                {
                    _itemId = (VSConstants.VSITEMID)itemidNew;
                    changed = true;
                }

                if ((attributes & (uint)__VSRDTATTRIB.RDTA_Hierarchy) != 0)
                {
                    _hierarchy = pHierNew;
                    changed = true;
                }

                if (changed && Changed != null)
                    Changed(this, EventArgs.Empty);
            }

            return VSConstants.S_OK;
        }
        #endregion
    }
}