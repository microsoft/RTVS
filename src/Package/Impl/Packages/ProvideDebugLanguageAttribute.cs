// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Packages {
    class ProvideDebugLanguageAttribute : RegistrationAttribute {
        private readonly string _languageGuid, _languageName, _engineGuid, _eeGuid, _customViewerGuid;

        public ProvideDebugLanguageAttribute(string languageName, string languageGuid, string eeGuid, string debugEngineGuid, string customViewerGuid = null) {
            _languageName = languageName;
            _languageGuid = new Guid(languageGuid).ToString("B");
            _eeGuid = new Guid(eeGuid).ToString("B");
            _engineGuid = debugEngineGuid;

            if (customViewerGuid != null) {
                _customViewerGuid = new Guid(customViewerGuid).ToString("B");
            }
        }

        public override void Register(RegistrationContext context) {
            var langSvcKey = context.CreateKey("Languages\\Language Services\\" + _languageName + "\\Debugger Languages\\" + _languageGuid);
            langSvcKey.SetValue("", _languageName);
            // 994... is the vendor ID (Microsoft)
            var eeKey = context.CreateKey("AD7Metrics\\ExpressionEvaluator\\" + _languageGuid + "\\{994B45C4-E6E9-11D2-903F-00C04FA302A1}");
            eeKey.SetValue("Language", _languageName);
            eeKey.SetValue("Name", _languageName);
            eeKey.SetValue("CLSID", _eeGuid);

            if (_customViewerGuid != null) {
                eeKey.SetValue("CustomViewerCLSID", _customViewerGuid);
            }

            var engineKey = eeKey.CreateSubkey("Engine");
            engineKey.SetValue("0", _engineGuid);
        }

        public override void Unregister(RegistrationContext context) { }
    }
}
