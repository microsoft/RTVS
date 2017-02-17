// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Logging;
using Microsoft.VisualStudio.R.Package.Options.Attributes;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    internal sealed class LogLevelTypeConverter : EnumTypeConverter<LogVerbosity> {
        public LogLevelTypeConverter() :
            base(Resources.LoggingLevel_None, Resources.LoggingLevel_Minimal, Resources.LoggingLevel_Normal, Resources.LoggingLevel_Traffic) {
        }
    }
}
