// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Package.SurveyNews {
    [Serializable]
    public sealed class SurveyNewsFeedException : Exception {
        public SurveyNewsFeedException(string message, Exception innerException = null)
            : base(message, innerException) {
        }
    }
}
