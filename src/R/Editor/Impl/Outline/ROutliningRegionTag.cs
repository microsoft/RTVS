// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Outline;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Outline {
    /// <summary>
    /// Represents code outline region.
    /// </summary>
    public class ROutliningRegionTag : OutliningRegionTag, IOutliningRegionTag {
        private OutlineRegion _outlineRegion;

        internal ROutliningRegionTag(OutlineRegion outlineRegion) :
            base(false, false, string.Empty, string.Empty) {
            _outlineRegion = outlineRegion;
        }

        object IOutliningRegionTag.CollapsedForm {
            get {
                return _outlineRegion.DisplayText;
            }
        }

        object IOutliningRegionTag.CollapsedHintForm {
            get {
                return _outlineRegion.HoverText;
            }
        }
    }
}
