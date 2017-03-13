// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;
using IThreadHandling = Microsoft.VisualStudio.ProjectSystem.IProjectThreadingService;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages {
    internal abstract class PropertyPage : UserControl, IPropertyPage, IVsDebuggerEvents {
        private IPropertyPageSite _site = null;
        private bool _isDirty = false;
        private bool _ignoreEvents = false;
        private IVsDebugger _debugger;
        private uint _debuggerCookie;

        // WIN32 Constants
        private const int SW_HIDE = 0;

        protected UnconfiguredProject UnconfiguredProject { get; set; }
        protected ProjectProperties[] ConfiguredProperties { get; set; }
        private IThreadHandling ThreadHandling { get; set; }
        protected abstract string PropertyPageName { get; }

        protected bool IsDirty {
            get { return _isDirty; }
            set {
                // Only process real changes
                if (value != _isDirty && !_ignoreEvents) {
                    _isDirty = value;
                    // If dirty, this causes Apply to be called
                    if (_site != null) {
                        _site.OnStatusChange((uint)(this._isDirty ? PROPPAGESTATUS.PROPPAGESTATUS_DIRTY : PROPPAGESTATUS.PROPPAGESTATUS_CLEAN));
                    }
                }
            }
        }

        private T WaitForAsync<T>(Func<Task<T>> asyncFunc)=> ThreadHandling != null ? ThreadHandling.ExecuteSynchronously<T>(asyncFunc) : default(T);
        private void WaitForAsync(Func<Task> asyncFunc)  => ThreadHandling?.ExecuteSynchronously(asyncFunc);

        protected abstract Task<int> OnApply();
        protected abstract Task OnDeactivate();
        protected abstract Task OnSetObjects(bool isClosing);

        public void Activate(IntPtr hWndParent, RECT[] pRect, int bModal) {
            AdviseDebugger();
            this.SuspendLayout();
            // Initialization can cause some events to be fired when we change some values
            // so we use this flag (_ignoreEvents) to notify IsDirty to ignore
            // any changes that happen during initialization
            Win32Methods.SetParent(this.Handle, hWndParent);
            this.ResumeLayout();
        }

        public int Apply() => WaitForAsync<int>(OnApply);

        public void Deactivate() {
            WaitForAsync(OnDeactivate);
            UnadviseDebugger();
            Dispose(true);
        }

        public void GetPageInfo(PROPPAGEINFO[] pPageInfo) {
            PROPPAGEINFO info = new PROPPAGEINFO();

            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = null;
            info.pszHelpFile = null;
            info.pszTitle = this.PropertyPageName;
            // set the size to 0 so the host doesn't use scroll bars
            // we want to do that within our own container.
            info.SIZE.cx = 0;
            info.SIZE.cy = 0;
            if (pPageInfo != null && pPageInfo.Length > 0) {
                pPageInfo[0] = info;
            }
        }

        public void Help(string pszHelpDir) { }
        public int IsPageDirty() => IsDirty ? VSConstants.S_OK : VSConstants.S_FALSE;

        public new void Move(RECT[] pRect) {
            if (pRect == null || pRect.Length <= 0) {
                throw new ArgumentNullException(nameof(pRect));
            }

            RECT r = pRect[0];
            Location = new Point(r.left, r.top);
            Size = new Size(r.right - r.left, r.bottom - r.top);
        }

        internal void SetObjects(bool isClosing) {
            WaitForAsync(async () => await OnSetObjects(isClosing));
        }

        public void SetObjects(uint cObjects, object[] ppunk) {
            // If asked to, release our cached selected Project object(s)
            UnconfiguredProject = null;
            ConfiguredProperties = null;

            if (cObjects == 0) {
                // If we have never configured anything (maybe a failure occurred
                // on open so app designer is closing us). In this case do nothing.
                if (ThreadHandling != null) {
                    SetObjects(true);
                }
                return;
            }

            if (ppunk.Length < cObjects) {
                throw new ArgumentOutOfRangeException(nameof(cObjects));
            }

            var configuredProjectsProperties = new List<ProjectProperties>();

            // Look for an IVsBrowseObject
            for (int i = 0; i < cObjects; ++i) {
                var browseObj = ppunk[i] as IVsBrowseObject;
                if (browseObj != null) {
                    IVsHierarchy hier = null;
                    uint itemid;
                    int hr;
                    hr = browseObj.GetProjectItem(out hier, out itemid);
                    Debug.Assert(itemid == VSConstants.VSITEMID_ROOT, "Selected object should be project root node");

                    if (hr == VSConstants.S_OK && itemid == VSConstants.VSITEMID_ROOT) {
                        UnconfiguredProject = hier.GetUnconfiguredProject();
                        // We need to save ThreadHandling because the app designer will call SetObjects with null, and then call
                        // Deactivate(). We need to run async code during Deactivate() which requires ThreadHandling.
                        ThreadHandling = UnconfiguredProject.Services.ExportProvider.GetExportedValue<IThreadHandling>();

                        var pcg = ppunk[i] as IVsProjectCfg2;
                        if (pcg != null) {
                            string vsConfigName;
                            pcg.get_CanonicalName(out vsConfigName);

                            ThreadHandling.ExecuteSynchronously(async delegate {
                                var provider = new ConfiguredRProjectExportProvider();
                                var configuredProjProps = await provider.GetExportAsync<ProjectProperties>(UnconfiguredProject, vsConfigName);
                                configuredProjectsProperties.Add(configuredProjProps);
                            });
                        }
                    }
                }
                ConfiguredProperties = configuredProjectsProperties.ToArray();
            }
            SetObjects(false);
        }

        public void SetPageSite(IPropertyPageSite pPageSite) {
            _site = pPageSite;
        }

        public void Show(uint nCmdShow) {
            if (nCmdShow !=  SW_HIDE) {
                this.Show();
            } else {
                this.Hide();
            }
        }

        public int TranslateAccelerator(MSG[] pMsg) {
            if (pMsg == null) {
                return VSConstants.E_POINTER;
            }

            Message m = Message.Create(pMsg[0].hwnd, (int)pMsg[0].message, pMsg[0].wParam, pMsg[0].lParam);
            bool used = false;

            // Preprocessing should be passed to the control whose handle the message refers to.
            Control target = Control.FromChildHandle(m.HWnd);
            if (target != null) {
                used = target.PreProcessMessage(ref m);
            }

            if (used) {
                pMsg[0].message = (uint)m.Msg;
                pMsg[0].wParam = m.WParam;
                pMsg[0].lParam = m.LParam;
                // Returning S_OK indicates we handled the message ourselves
                return VSConstants.S_OK;
            }

            // Returning S_FALSE indicates we have not handled the message
            int result = 0;
            if (this._site != null) {
                result = _site.TranslateAccelerator(pMsg);
            }

            return result;
        }

        internal void AdviseDebugger() {
            System.IServiceProvider sp = _site as System.IServiceProvider;
            if (sp != null) {
                _debugger = (IVsDebugger)sp.GetService(typeof(IVsDebugger));
                if (_debugger != null) {
                    _debugger.AdviseDebuggerEvents(this, out _debuggerCookie);
                    DBGMODE[] dbgMode = new DBGMODE[1];
                    _debugger.GetMode(dbgMode);
                    ((IVsDebuggerEvents)this).OnModeChange(dbgMode[0]);
                }
            }
        }
        private void UnadviseDebugger() {
            if (_debuggerCookie != 0 && _debugger != null) {
                _debugger.UnadviseDebuggerEvents(_debuggerCookie);
            }
            _debugger = null;
            _debuggerCookie = 0;
        }

        public int OnModeChange(DBGMODE dbgmodeNew) {
            Enabled = (dbgmodeNew == DBGMODE.DBGMODE_Design);
            return VSConstants.S_OK;
        }
    }
}
