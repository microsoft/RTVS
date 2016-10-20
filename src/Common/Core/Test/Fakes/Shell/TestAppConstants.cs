// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    public sealed class TestAppConstants : IApplicationConstants {
        public string ApplicationName => "TestApplication";
        public uint LocaleId => 1033;
        public string LocalMachineHive => null;
        public IntPtr ApplicationWindowHandle => IntPtr.Zero;
        public UIColorTheme UIColorTheme => UIColorTheme.Light;
    }
}
