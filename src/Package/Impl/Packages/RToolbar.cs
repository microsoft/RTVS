// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using EnvDTE80;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.CommandBars;

namespace Microsoft.VisualStudio.R.Package.Packages {
    internal sealed class RToolbar {
        private readonly DTE2 _dte2;
        private readonly IRSettings _settings;

        public RToolbar(DTE2 dte2, IRSettings settings) {
            _dte2 = dte2;
            _settings = settings;
        }

        public void Show() {
            // First time we show it (default is true in settings). When package closes, 
            // it saves the state so if user closed it it won't be forcibly coming back.
            var tb = GetToolbar();
            if (tb != null && _settings.ShowRToolbar) {
                tb.Visible = true;
            }
        }

        public void SaveState() {
            var tb = GetToolbar();
            if (tb != null) {
                _settings.ShowRToolbar = tb.Visible;
            }
        }

        private CommandBar GetToolbar() {
            var cbs = (CommandBars.CommandBars)_dte2.CommandBars;
            Debug.Assert(cbs != null, "Unable to find R Toolbar");
            if (cbs != null) {
                var cb = cbs["R Toolbar"];
                Debug.Assert(cb != null, "Unable to find R Toolbar");
                return cb;
            }
            return null;
        }
    }
}
