// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Linq;
using Microsoft.Common.Core.Logging;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    internal sealed class LogLevelTypeConverter : EnumTypeConverter<LogVerbosity> {
        private static readonly string[] _permittedSettings = {
            Resources.LoggingLevel_None,
            Resources.LoggingLevel_Minimal,
            Resources.LoggingLevel_Normal,
            Resources.LoggingLevel_Traffic
        };
        private readonly int _maxLogLevel;

        public LogLevelTypeConverter(): base(Resources.LoggingLevel_None, Resources.LoggingLevel_Minimal, Resources.LoggingLevel_Normal, Resources.LoggingLevel_Traffic) {
            var permissions = VsAppShell.Current.Services.LoggingPermissions;
            _maxLogLevel = (int)permissions.MaxVerbosity;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            return new StandardValuesCollection(_permittedSettings.Take(_maxLogLevel + 1).ToList());
        }
    }
}
