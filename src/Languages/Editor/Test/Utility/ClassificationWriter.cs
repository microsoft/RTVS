// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.Languages.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class ClassificationWriter {
        public static string WriteClassifications(IEnumerable<ClassificationSpan> classifications) {
            var sb = new StringBuilder();

            foreach (var c in classifications) {
                sb.Append('[');
                sb.Append(c.Span.Start.Position.ToString());
                sb.Append(':');
                sb.Append(c.Span.Length);
                sb.Append(']');
                sb.Append(' ');
                sb.Append(c.ClassificationType.Classification);
                sb.Append('\r');
                sb.Append('\n');
            }

            return sb.ToString();
        }
    }
}
