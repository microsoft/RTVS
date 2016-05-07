// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Debugger {
    public static class DebuggerGuids {
        public const string VendorGuidString = "994B45C4-E6E9-11D2-903F-00C04FA302A1";
        public static readonly Guid VendorGuid = new Guid(VendorGuidString);

        public const string RuntimeTypeGuidString = "3940F2FC-5CD0-446F-81B0-6641C05F76D4";
        public static readonly Guid RuntimeTypeGuid = new Guid(RuntimeTypeGuidString);

        public const string SymbolProviderGuidString = "366C0C4E-27B0-4847-9BCB-480B139E5CCF";
        public static readonly Guid SymbolProviderGuid = new Guid(SymbolProviderGuidString);

        public const string LanguageGuidString = "652D96EE-B796-4BD7-AD7F-EDEA65528946";
        public static readonly Guid LanguageGuid = new Guid(LanguageGuidString);

        public const string ExceptionCategoryGuidString = "4717209D-0829-4DA2-899B-4F885D627BBF";
        public static readonly Guid ExceptionCategoryGuid = new Guid(ExceptionCategoryGuidString);

        public const string ProgramProviderCLSIDString = "6FA14708-3963-46AF-ADAF-7CD7E3EF57FE";
        public static readonly Guid ProgramProviderCLSID = new Guid(ProgramProviderCLSIDString);

        public const string DebugEngineCLSIDString = "F839D71F-EEF4-4123-B6E7-BE0FC7E6F2A3";
        public static readonly Guid DebugEngineCLSID = new Guid(DebugEngineCLSIDString);

        public const string DebugEngineString = "BC67335F-8EC6-4AA8-AF59-72AEC95947EA";
        public static readonly Guid DebugEngine = new Guid(DebugEngineString);

        public const string PortSupplierCLSIDString = "B89C17B4-320D-44D0-B95C-5D3468644207";
        public static readonly Guid PortSupplierCLSID = new Guid(PortSupplierCLSIDString);

        public const string PortSupplierString = "B3B6414F-D6F8-43A3-BFF4-93F5DD84CB86";
        public static readonly Guid PortSupplier = new Guid(PortSupplierString);

        public const string CustomViewerString = "8FBE2C99-E300-4079-A702-410FC60996EA";
        public static readonly Guid CustomViewer = new Guid(CustomViewerString);
    };
}
