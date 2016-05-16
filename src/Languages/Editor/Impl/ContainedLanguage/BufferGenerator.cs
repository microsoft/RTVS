// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Projection;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Base class for simple contained language buffer generators
    /// that do not require additional decoration to the code
    /// such as R inside RMD. ASP.NET or Razor would use different 
    /// custom implementation.
    /// </summary>
    public class BufferGenerator: IBufferGenerator {
        #region IBufferGenerator
        public virtual void GenerateContent(ITextBuffer diskBuffer, ITextBuffer secondaryBuffer, IEnumerable<ITextRange> languageBlocks) {
            var mappings = new List<ProjectionMapping>();
            var secondaryIndex = 0;

            var pbm = ProjectionBufferManager.FromTextBuffer(diskBuffer);
            var sb = new StringBuilder();

            foreach (var b in languageBlocks) {
                var text = pbm.ProjectionBuffer.CurrentSnapshot.GetText(b.Start, b.Length);
                secondaryIndex = sb.Length;

                sb.Append(text + Environment.NewLine);
                var m = new ProjectionMapping(b.Start, secondaryIndex, b.Length);
                mappings.Add(m);
            }

            pbm.SetTextAndMappings(sb.ToString(), mappings);
        }
        #endregion
    }
}