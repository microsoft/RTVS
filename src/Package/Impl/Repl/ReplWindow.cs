using System;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl
{
    /// <summary>
    /// Tracks most recently active REPL window
    /// </summary>
    internal sealed class ReplWindow: IVsWindowFrameEvents, IDisposable
    {
        private uint _windowFrameEventsCookie;
        private IVsInteractiveWindow _lastUsedReplWindow;
        private static Lazy<ReplWindow> _instance = new Lazy<ReplWindow>(() => new ReplWindow());

        public ReplWindow()
        {
            IVsUIShell7 shell = AppShell.Current.GetGlobalService<IVsUIShell7>(typeof(SVsUIShell));
            _windowFrameEventsCookie = shell.AdviseWindowFrameEvents(this);
        }

        public static ReplWindow Current
        {
            get { return _instance.Value; }
        }

        public void ExecuteCode(string code)
        {
            IVsInteractiveWindow current = _instance.Value.GetInteractiveWindow();
            if (current != null)
            {
                current.InteractiveWindow.AddInput(code);
                current.InteractiveWindow.Operations.ExecuteInput();
            }
        }

        public void ExecuteCurrentExpression()
        {
            IVsInteractiveWindow current = _instance.Value.GetInteractiveWindow();
            if (current != null)
            {
                current.InteractiveWindow.Operations.ExecuteInput();
            }
        }

        public IVsInteractiveWindow GetInteractiveWindow()
        {
            if (_lastUsedReplWindow == null)
            {
                IVsWindowFrame frame;
                IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));

                Guid persistenceSlot = RGuidList.ReplInteractiveWindowProviderGuid;
                shell.FindToolWindow((int)__VSFINDTOOLWIN.FTW_fForceCreate, ref persistenceSlot, out frame);
                if (frame != null)
                {
                    frame.Show();
                }
            }

            return _lastUsedReplWindow;
        }

        #region IVsWindowFrameEvents
        public void OnFrameCreated(IVsWindowFrame frame)
        {
        }

        public void OnFrameDestroyed(IVsWindowFrame frame)
        {
            if (_lastUsedReplWindow == frame)
            {
                _lastUsedReplWindow = null;
            }
        }

        public void OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible)
        {
        }

        public void OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen)
        {
        }

        public void OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame)
        {
            // Track last recently used REPL window
            if (!CheckReplFrame(oldFrame))
            {
                CheckReplFrame(newFrame);
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_windowFrameEventsCookie != 0)
            {
                IVsUIShell7 shell = AppShell.Current.GetGlobalService<IVsUIShell7>(typeof(SVsUIShell));
                shell.UnadviseWindowFrameEvents(_windowFrameEventsCookie);
                _windowFrameEventsCookie = 0;
            }

            _lastUsedReplWindow = null;
        }
        #endregion

        private bool CheckReplFrame(IVsWindowFrame frame)
        {
            if (frame != null)
            {
                Guid property;
                frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_GuidPersistenceSlot, out property);
                if (property == RGuidList.ReplInteractiveWindowProviderGuid)
                {
                    object docView;
                    frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView);
                    _lastUsedReplWindow = docView as IVsInteractiveWindow;
                    return _lastUsedReplWindow != null;
                }
            }

            return false;
        }
    }
}
