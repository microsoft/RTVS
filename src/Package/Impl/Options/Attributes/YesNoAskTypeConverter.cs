// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Enums;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal class YesNoAskTypeConverter : EnumTypeConverter<YesNoAsk> {
        public YesNoAskTypeConverter() : base(Resources.Yes, Resources.No, Resources.Ask) {}
    }
}
