// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Html.Core.Tree.Utility {
    /// <summary>
    /// Type of location (or type of completion that need to be shown)
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    [SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags")]
    [Flags]
    public enum HtmlPositionType {
        Undefined = 0x0000,
        InStartTag = 0x0001,
        ElementName = 0x0002 | InStartTag,
        AttributeName = 0x0004 | InStartTag,
        BeforeEqualsSign = 0x0008 | InStartTag,
        EqualsSign = 0x0010 | InStartTag,
        AfterEqualsSign = 0x0020 | InStartTag,
        AttributeValue = 0x0040 | InStartTag,
        InEndTag = 0x0080,

        InTag = InStartTag | InEndTag,

        InContent = 0x0400,
        InInlineStyle = 0x0800 | AttributeValue,
        InInlineScript = 0x1000 | AttributeValue,
        InStyleBlock = 0x2000 | InContent,
        InScriptBlock = 0x4000 | InContent,
    }
}
