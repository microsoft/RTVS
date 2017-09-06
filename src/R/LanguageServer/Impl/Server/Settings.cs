// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

namespace Microsoft.R.LanguageServer.Server {

    public class SettingsRoot {
        public LanguageServerSettings DemoLanguageServer { get; set; }
    }

    public class LanguageServerSettings {
        public int MaxNumberOfProblems { get; set; } = 10;

        public LanguageServerTraceSettings Trace { get; } = new LanguageServerTraceSettings();
    }

    public class LanguageServerTraceSettings {
        public string Server { get; set; }
    }
}
