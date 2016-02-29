// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Packages.Markdown {
    public static class MdGuidList {
        public const string MdPackageGuidString = "C765E811-8EE4-4040-84FC-F4DCBC12E6C4";
        public const string MdEditorFactoryGuidString = "998B021A-4AA4-41C5-A4C0-205470AC4BC4";
        public const string MdLanguageServiceGuidString = "445F1AAC-DC2C-4A2D-93B3-E4D9BDABF360";

        public static readonly Guid MdPackageGuid = new Guid(MdPackageGuidString);
        public static readonly Guid MdEditorFactoryGuid = new Guid(MdEditorFactoryGuidString);
        public static readonly Guid MdLanguageServiceGuid = new Guid(MdLanguageServiceGuidString);

        public const string MdCmdSetGuidString = "0BF33C69-94C2-4985-81A0-2556F8DB88A6";
        public static readonly Guid MdCmdSetGuid = new Guid(MdCmdSetGuidString);
    };
}