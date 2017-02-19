// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.ExportDialog {
    public class OrientationEnumConverter : EnumValueConverter<OrientationEnum> {
        private static IDictionary<OrientationEnum, string> _enumToString = new Dictionary<OrientationEnum, string>() {
           {OrientationEnum.Portrait, Resources.Combobox_Portrait },
           {OrientationEnum.Landscape,Resources.Combobox_Landscape }
       };
        private static IDictionary<string, OrientationEnum> _stringToEnum = new Dictionary<string, OrientationEnum>() {
           { Resources.Combobox_Portrait,OrientationEnum.Portrait },
           {Resources.Combobox_Landscape ,OrientationEnum.Landscape}
       };

        protected override IDictionary<OrientationEnum, string> GetEnumToString() {
            return _enumToString;
        }

        protected override IDictionary<string, OrientationEnum> GetStringToEnum() {
            return _stringToEnum;
        }
    }
}
