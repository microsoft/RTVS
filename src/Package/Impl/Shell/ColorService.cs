// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.R.Package.Shell {
    [Export(typeof(IColorService))]
    internal sealed class ColorService : IColorService {
        public bool IsDarkTheme {
            get {
                var defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
                return defaultBackground.GetBrightness() < 0.5;
            }
        }
    }
}
