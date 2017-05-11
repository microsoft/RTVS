// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.Settings;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal class SurveyNewsPolicyTypeConverter : EnumTypeConverter<SurveyNewsPolicy> {
        public SurveyNewsPolicyTypeConverter() : base(Resources.SurveyNewsPolicyDisabled, Resources.SurveyNewsPolicyCheckOnceDay, Resources.SurveyNewsPolicyCheckOnceWeek, Resources.SurveyNewsPolicyCheckOnceMonth) {}
    }
}
