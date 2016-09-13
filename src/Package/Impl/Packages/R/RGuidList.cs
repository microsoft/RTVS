// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Packages.R {
    public static class RGuidList {
        public const string RtvsStartupPackageGuidString = "1E27AE60-1095-4A62-9DA5-37038BCBDD26";
        public const string RPackageGuidString = "6D7C5336-C0CA-4857-A7E7-2E964EA836BF";
        public const string REditorFactoryGuidString = "EE606CC0-077A-4FDE-91C3-24EC012C8389";
        public const string RProjEditorFactoryGuidString = "3B0A6D8B-C380-428A-93D8-45FD46E95581";
        public const string RLanguageServiceGuidString = "29C0D8E0-C01C-412B-BEE8-7A7A253A31E6";
        public const string RProjLanguageServiceGuidString = "49E61C2E-1F5B-4E24-961F-388B96FAE77F";

        public const string RCmdSetGuidString = "AD87578C-B324-44DC-A12A-B01A6ED5C6E3";
        public const string ProjectFileGeneratorGuidString = "62FC63E0-1A66-4B4A-B61A-A6C8BA558FC6";
        public const string CpsProjectFactoryGuidString = "DA7A21FA-8162-4350-AD77-A8D1B671F3ED";

        public const string ReplInteractiveWindowProviderGuidString = "C2582843-58C9-4FE7-B4BD-864C17AD7CE2";
        public const string ReplWindowGuidString = "7026C640-8831-43A4-A93A-A56AA6BB9552";

        public static readonly Guid RPackageGuid = new Guid(RPackageGuidString);
        public static readonly Guid RtvsStartupPackageGuid = new Guid(RtvsStartupPackageGuidString);
        public static readonly Guid REditorFactoryGuid = new Guid(REditorFactoryGuidString);
        public static readonly Guid RProjEditorFactoryGuid = new Guid(RProjEditorFactoryGuidString);
        public static readonly Guid RLanguageServiceGuid = new Guid(RLanguageServiceGuidString);
        public static readonly Guid RProjLanguageServiceGuid = new Guid(RProjLanguageServiceGuidString);

        public static readonly Guid RCmdSetGuid = new Guid(RCmdSetGuidString);
        public static readonly Guid CpsProjectFactoryGuid = new Guid(CpsProjectFactoryGuidString);

        public static readonly Guid ReplInteractiveWindowProviderGuid = new Guid(ReplInteractiveWindowProviderGuidString);
        public static readonly Guid ReplWindowGuid = new Guid(ReplWindowGuidString);

        public const string VariableGridWindowGuidString = "3F6855E6-E2DB-46F2-9820-EDC794FE8AFE";
        public const string VariableGridWindowBracedGuidString = "{3F6855E6-E2DB-46F2-9820-EDC794FE8AFE}";
        public static readonly Guid VariableGridWindowGuid = new Guid(VariableGridWindowGuidString);

        public const string WebHelpWindowGuidString = "153D83A7-016A-43A7-871F-D52525E82B9D";
        public const string ShinyWindowGuidString = "662EEE61-51CD-442E-B08C-D0A95DF1EC94";
        public const string MarkdownWindowGuidString = "E24645D2-A20E-442D-A474-2F8DB29C9578";

        public static readonly Guid WebHelpWindowGuid = new Guid(WebHelpWindowGuidString);
        public static readonly Guid ShinyWindowGuid = new Guid(ShinyWindowGuidString);
        public static readonly Guid MarkdownWindowGuid = new Guid(MarkdownWindowGuidString);

        /// <summary>
        /// Miscellanious files project (no project is opened in IDE)
        /// </summary>
        public const string MiscFilesProjectGuidString = "A2FE74E1-B743-11D0-AE1A-00A0C90FFFC3";

        /// <summary>
        /// SQLProj project guid
        /// </summary>
        public const string SqlProjectGuidString = "00D1A9C2-B5F0-4AF3-8072-F6C62B433612";
    };
}