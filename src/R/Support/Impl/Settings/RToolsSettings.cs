// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Shell;

namespace Microsoft.R.Support.Settings {
    public static class RToolsSettings {
        private static IRToolsSettings _settings;

        public static IRToolsSettings Current {
            get {
                if (_settings == null) {
                    _settings = shell.Current.ExportProvider.GetExport<IRToolsSettings>().Value;
                }
                return _settings;
            }
            set {
                _settings = value;
            }
        }
    }
}
