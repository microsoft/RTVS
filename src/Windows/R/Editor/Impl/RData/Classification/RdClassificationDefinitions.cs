// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Microsoft.R.Editor.Windows;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.RData.Classification {
    [ExcludeFromCodeCoverage]
    internal sealed class RdClassificationDefinitions {
        [Export]
        [Name("RD Braces")]
        internal ClassificationTypeDefinition RdBracesClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "RD Braces")]
        [Name("RD Braces")]
        internal sealed class RdBracesClassificationFormat : ClassificationFormatDefinition {
            public RdBracesClassificationFormat() {
                ForegroundColor = Colors.Gray;
                DisplayName = Windows_Resources.ColorName_RD_CurlyBraces;
            }
        }
    }
}
