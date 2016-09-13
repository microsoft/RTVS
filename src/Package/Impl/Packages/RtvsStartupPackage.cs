// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package {
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(RGuidList.RtvsStartupPackageGuidString)]
    [InstalledProductRegistration("#7002", "#7003", RtvsProductInfo.VersionString, IconResourceID = 400)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.EmptySolution_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    internal sealed class RtvsStartupPackage : VisualStudio.Shell.Package {
        private static DTEEvents _dteEvents;
        private static Action _onStartupActions = () => {};

        protected override void Initialize() {
            base.Initialize();
            var dte = GetGlobalService(typeof(DTE)) as DTE;
            if (dte != null) {
                _dteEvents = dte.Events.DTEEvents;
                _dteEvents.OnStartupComplete += OnStartupComplete;
            }
        }

        private void OnStartupComplete() {
            _dteEvents.OnStartupComplete -= OnStartupComplete;
            var actions = Interlocked.Exchange(ref _onStartupActions, null);
            actions();
        }

        public static bool ExecuteOnStartupComplete(Action action) {
            if (_dteEvents == null) {
                action();
                return true;
            }

            while (true) {
                var actions = _onStartupActions;
                // If OnStartupComplete was raised already, execute an action
                if (actions == null) {
                    action();
                    return true;
                }

                var oldActions = Interlocked.CompareExchange(ref _onStartupActions, actions + action, actions);
                // If OnStartupComplete was raised while action was added, execute an action 
                if (oldActions == null) {
                    action();
                    return true;
                }

                // If action was scheduled, just exit
                if (oldActions == actions) {
                    return false;
                }
            }
        }
    }
}
