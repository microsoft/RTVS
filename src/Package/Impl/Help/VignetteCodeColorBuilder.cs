// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Text;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Help {
    [Export(typeof(IVignetteCodeColorBuilder))]
    internal sealed class VignetteCodeColorBuilder: IVignetteCodeColorBuilder {
        private class CssCodeProperty {
            public string CssClassName;
            public string ClassificationTypeName;

            public CssCodeProperty(string cssClassName, string ctName) {
                CssClassName = cssClassName;
                ClassificationTypeName = ctName;
            }
        }

        private static readonly CssCodeProperty[] _cssPropertyMap = new CssCodeProperty[] {
            new CssCodeProperty("kw", PredefinedClassificationTypeNames.Keyword),
            new CssCodeProperty("fl", PredefinedClassificationTypeNames.Number),
            new CssCodeProperty("st", PredefinedClassificationTypeNames.String),
            new CssCodeProperty("vs", PredefinedClassificationTypeNames.Literal),
            new CssCodeProperty("ss", PredefinedClassificationTypeNames.String),
            new CssCodeProperty("co", PredefinedClassificationTypeNames.Comment),
            new CssCodeProperty("fu", PredefinedClassificationTypeNames.Keyword),
            new CssCodeProperty("va", PredefinedClassificationTypeNames.Identifier),
            new CssCodeProperty("ot", PredefinedClassificationTypeNames.Other),
            new CssCodeProperty("cn", PredefinedClassificationTypeNames.Number),
            new CssCodeProperty("dv", PredefinedClassificationTypeNames.Number),
            new CssCodeProperty("cf", PredefinedClassificationTypeNames.Keyword),
            new CssCodeProperty("op", PredefinedClassificationTypeNames.Operator),
            new CssCodeProperty("pp", PredefinedClassificationTypeNames.PreprocessorKeyword),
        };

        private static readonly CssCodeProperty[] _prePropertyMap = new CssCodeProperty[] {
            new CssCodeProperty("keyword", PredefinedClassificationTypeNames.Keyword),
            new CssCodeProperty("number", PredefinedClassificationTypeNames.Number),
            new CssCodeProperty("string", PredefinedClassificationTypeNames.String),
            new CssCodeProperty("literal", PredefinedClassificationTypeNames.Literal),
            new CssCodeProperty("comment", PredefinedClassificationTypeNames.Comment),
            new CssCodeProperty("identifier", PredefinedClassificationTypeNames.Identifier),
            new CssCodeProperty("operator", PredefinedClassificationTypeNames.Operator),
            new CssCodeProperty("paren", PredefinedClassificationTypeNames.Operator),
        };

        private readonly IRInteractiveWorkflowVisualProvider _workflowProvider;
        private readonly IClassificationFormatMapService _formatMapService;
        private readonly IClassificationTypeRegistryService _classificationRegistryService;

        [ImportingConstructor]
        public VignetteCodeColorBuilder(ICoreShell shell) {
            _workflowProvider = shell.GetService<IRInteractiveWorkflowVisualProvider>();
            _formatMapService = shell.GetService<IClassificationFormatMapService>();
            _classificationRegistryService = shell.GetService<IClassificationTypeRegistryService>();
        }

        public string GetCodeColorsCss() {
            var sb = new StringBuilder();
            var workflow = _workflowProvider.GetOrCreate();
            if (workflow.ActiveWindow?.TextView != null) {
                var map = _formatMapService.GetClassificationFormatMap(workflow.ActiveWindow.TextView);
                foreach (var cssCodeProp in _cssPropertyMap) {
                    var props = map.GetTextProperties(_classificationRegistryService.GetClassificationType(cssCodeProp.ClassificationTypeName));
                    sb.AppendLine(Invariant(
$@"code > span.{cssCodeProp.CssClassName} {{
    color: {CssColorFromBrush(props.ForegroundBrush)};
}}
"));
                }

                foreach (var cssCodeProp in _prePropertyMap) {
                    var props = map.GetTextProperties(_classificationRegistryService.GetClassificationType(cssCodeProp.ClassificationTypeName));
                    sb.AppendLine(Invariant(
$@"pre .{cssCodeProp.CssClassName} {{
    color: {CssColorFromBrush(props.ForegroundBrush)};
}}"
));
                }
            }
            return sb.ToString();
        }

        private string CssColorFromBrush(System.Windows.Media.Brush brush) {
            var sb = brush as System.Windows.Media.SolidColorBrush;
            return Invariant($"rgb({sb.Color.R}, {sb.Color.G}, {sb.Color.B})");
        }
    }
}
