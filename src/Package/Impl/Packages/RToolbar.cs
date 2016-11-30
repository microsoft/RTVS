// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using EnvDTE;
using EnvDTE80;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Packages {
    internal sealed class RToolbar {
        private readonly DTE2 _dte2;
        private readonly IRToolsSettings _settings;

        public RToolbar(DTE2 dte2, IRToolsSettings settings) {
            _dte2 = dte2;
            _settings = settings;
        }

        public void Show() {
            // First time we show it (default is true in settings). When package closes, 
            // it saves the state so if user closed it it won't be forcibly coming back.
            if (_settings.ShowRToolbar) {
                RToolbarAction((cb) => {
                    cb.Visible = true;
                });
            }
        }

        public void SaveState() {
            RToolbarAction((cb) => {
                _settings.ShowRToolbar = cb.Visible;
            });
        }

        private void RToolbarAction(Action<CommandBar> action) {
            var cbs = (CommandBars.CommandBars)_dte2.CommandBars;
            Debug.Assert(cbs != null, "Unable to find R Toolbar");
            if (cbs != null) {
                var cb = cbs["R Toolbar"];
                Debug.Assert(cb != null, "Unable to find R Toolbar");
                if (cb != null) {
                    action(cb);
                }
            }
        }
    }
}
