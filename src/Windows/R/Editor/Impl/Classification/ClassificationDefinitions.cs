// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Microsoft.R.Editor.Windows;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Classification {
    [ExcludeFromCodeCoverage]
    internal sealed class ClassificationDefinitions {
        public const string TypeFunctionClassificationFormatName = "R Type Function";

        [Export]
        [Name(TypeFunctionClassificationFormatName)]
        internal ClassificationTypeDefinition TypeFunctionClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = TypeFunctionClassificationFormatName)]
        [Name(TypeFunctionClassificationFormatName)]
        [ExcludeFromCodeCoverage]
        internal sealed class TypeFunctionClassificationFormat : ClassificationFormatDefinition {
            public TypeFunctionClassificationFormat() {
                ForegroundColor = Colors.Teal;
                DisplayName = Windows_Resources.ColorName_R_TypeFunction;
            }
        }

        public const string FunctionDefaultParameterClassificationFormatName = "R Function Default Parameter";

        [Export]
        [Name(FunctionDefaultParameterClassificationFormatName)]
        internal ClassificationTypeDefinition FunctionDefaultParameterClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = FunctionDefaultParameterClassificationFormatName)]
        [Name(FunctionDefaultParameterClassificationFormatName)]
        [ExcludeFromCodeCoverage]
        internal sealed class FunctionDefaultParameterClassificationFormat : ClassificationFormatDefinition {
            public FunctionDefaultParameterClassificationFormat() {
                ForegroundColor = Colors.DarkGray;
                DisplayName = Windows_Resources.ColorName_R_FunctionDefaultParameter;
            }
        }

        public const string FunctionReferenceClassificationFormatName = "R Function Reference";

        [Export]
        [Name(FunctionReferenceClassificationFormatName)]
        internal ClassificationTypeDefinition FunctionReferenceClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = FunctionReferenceClassificationFormatName)]
        [Name(FunctionReferenceClassificationFormatName)]
        [ExcludeFromCodeCoverage]
        internal sealed class FunctionReferenceClassificationFormat : ClassificationFormatDefinition {
            public FunctionReferenceClassificationFormat() {
                ForegroundColor = Colors.Maroon;
                DisplayName = Windows_Resources.ColorName_R_FunctionReference;
            }
        }
    }
}
