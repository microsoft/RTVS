// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Microsoft.R.Editor.Windows;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Roxygen {
    [ExcludeFromCodeCoverage]
    internal sealed class RoxygenClassificationDefinitions {
        public const string RoxygenKeywordClassificationFormatName = "Roxygen Keyword";

        [Export]
        [Name(RoxygenKeywordClassificationFormatName)]
        internal ClassificationTypeDefinition RoxygenKeywordClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = RoxygenKeywordClassificationFormatName)]
        [Name(RoxygenKeywordClassificationFormatName)]
        [Order(After = PredefinedClassificationTypeNames.Comment)]
        [ExcludeFromCodeCoverage]
        internal sealed class RoxygenKeywordClassificationFormat : ClassificationFormatDefinition {
            public RoxygenKeywordClassificationFormat() {
                ForegroundColor = Color.FromArgb(0xFF, 0, 0x9d, 0xFF);
                DisplayName = Windows_Resources.ColorName_R_RoxygenKeyword;
            }
        }

        public const string RoxygenExportClassificationFormatName = "Roxygen Export";

        [Export]
        [Name(RoxygenExportClassificationFormatName)]
        internal ClassificationTypeDefinition RoxygenExportClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = RoxygenExportClassificationFormatName)]
        [Name(RoxygenExportClassificationFormatName)]
        [Order(After = PredefinedClassificationTypeNames.Comment)]
        [ExcludeFromCodeCoverage]
        internal sealed class RoxygenExportClassificationFormat : ClassificationFormatDefinition {
            public RoxygenExportClassificationFormat() {
                ForegroundColor = Color.FromArgb(0xFF, 0x90, 0, 0xFF);
                DisplayName = Windows_Resources.ColorName_R_RoxygenExport;
            }
        }
    }
}
