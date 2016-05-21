// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Packages {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    class ProvideDebugEngineAttribute : RegistrationAttribute {
        public string Name { get; set; }

        public string EngineGuid { get; set; }

        public Type EngineType { get; set; }

        public Type ProgramProviderType { get; set; }

        public bool SupportsAttach { get; set; }

        public bool SupportsSetNextStatement { get; set; }

        public bool SupportsAddressBreakpoints { get; set; }

        public bool SupportsCallstackBreakpoints { get; set; }

        public bool SupportsFunctionBreakpoints { get; set; }

        public bool SupportsConditionalBreakpoints { get; set; }

        public bool SupportsHitCountBreakpoints { get; set; }

        public bool SupportsJustMyCodeStepping { get; set; }

        public bool SupportsExceptions { get; set; }

        public bool SupportsRemoteDebugging { get; set; }

        public int AutoSelectPriority { get; set; } = 6;

        public ProvideDebugEngineAttribute(string name, string engineGuid, Type engineType) {
            Name = name;
            EngineGuid = engineGuid;
            EngineType = engineType;
        }

        public override void Register(RegistrationContext context) {
            var engineKey = context.CreateKey("AD7Metrics\\Engine\\" + new Guid(EngineGuid).ToString("B"));
            engineKey.SetValue("Name", Name);

            engineKey.SetValue("CLSID", EngineType.GUID.ToString("B"));
            if (ProgramProviderType != null) {
                engineKey.SetValue("ProgramProvider", ProgramProviderType.GUID.ToString("B"));
            }
            engineKey.SetValue("PortSupplier", "{708C1ECA-FF48-11D2-904F-00C04FA302A1}"); // {708C1ECA-FF48-11D2-904F-00C04FA302A1}

            engineKey.SetValue("Attach", SupportsAttach ? 1 : 0);
            engineKey.SetValue("AddressBP", SupportsAddressBreakpoints ? 1 : 0);
            engineKey.SetValue("AutoSelectPriority", AutoSelectPriority);
            engineKey.SetValue("CallstackBP", SupportsCallstackBreakpoints ? 1 : 0);
            engineKey.SetValue("ConditionalBP", SupportsConditionalBreakpoints ? 1 : 0);
            engineKey.SetValue("Exceptions", SupportsExceptions ? 1 : 0);
            engineKey.SetValue("SetNextStatement", SupportsSetNextStatement ? 1 : 0);
            engineKey.SetValue("RemoteDebugging", SupportsRemoteDebugging ? 1 : 0);
            engineKey.SetValue("HitCountBP", SupportsHitCountBreakpoints ? 1 : 0);
            engineKey.SetValue("JustMyCodeStepping", SupportsJustMyCodeStepping ? 1 : 0);
            engineKey.SetValue("FunctionBP", SupportsFunctionBreakpoints ? 1 : 0);

            // provide class / assembly so we can be created remotely from the GAC w/o registering a CLSID 
            engineKey.SetValue("EngineClass", EngineType.FullName);
            engineKey.SetValue("EngineAssembly", EngineType.Assembly.FullName);

            // load locally so we don't need to create MSVSMon which would need to know how to
            // get at our provider type.  See AD7ProgramProvider.GetProviderProcessData for more info
            engineKey.SetValue("LoadProgramProviderUnderWOW64", 1);
            engineKey.SetValue("AlwaysLoadProgramProviderLocal", 1);
            engineKey.SetValue("LoadUnderWOW64", 1);

            using (var incompatKey = engineKey.CreateSubkey("IncompatibleList")) {
                incompatKey.SetValue("guidCOMPlusNativeEng", "{92EF0900-2251-11D2-B72E-0000F87572EF}");
                incompatKey.SetValue("guidCOMPlusOnlyEng", "{449EC4CC-30D2-4032-9256-EE18EB41B62B}");
                incompatKey.SetValue("guidScriptEng", "{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}");
                incompatKey.SetValue("guidCOMPlusOnlyEng2", "{5FFF7536-0C87-462D-8FD2-7971D948E6DC}");
                incompatKey.SetValue("guidCOMPlusOnlyEng4", "{FB0D4648-F776-4980-95F8-BB7F36EBC1EE}");
                incompatKey.SetValue("guidNativeOnlyEng", "{3B476D35-A401-11D2-AAD4-00C04F990171}");
            }

            using (var autoSelectIncompatKey = engineKey.CreateSubkey("AutoSelectIncompatibleList")) {
                autoSelectIncompatKey.SetValue("guidNativeOnlyEng", "{3B476D35-A401-11D2-AAD4-00C04F990171}");
            }

            var clsidKey = context.CreateKey("CLSID");
            var clsidGuidKey = clsidKey.CreateSubkey(EngineType.GUID.ToString("B"));
            clsidGuidKey.SetValue("Assembly", EngineType.Assembly.FullName);
            clsidGuidKey.SetValue("Class", EngineType.FullName);
            clsidGuidKey.SetValue("InprocServer32", context.InprocServerPath);
            clsidGuidKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, EngineType.Module.Name));
            clsidGuidKey.SetValue("ThreadingModel", "Free");

            if (ProgramProviderType != null) {
                clsidGuidKey = clsidKey.CreateSubkey(ProgramProviderType.GUID.ToString("B"));
                clsidGuidKey.SetValue("Assembly", ProgramProviderType.Assembly.FullName);
                clsidGuidKey.SetValue("Class", ProgramProviderType.FullName);
                clsidGuidKey.SetValue("InprocServer32", context.InprocServerPath);
                clsidGuidKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, EngineType.Module.Name));
                clsidGuidKey.SetValue("ThreadingModel", "Free");
            }

            using (var exceptionAssistantKey = context.CreateKey("ExceptionAssistant\\KnownEngines\\" + new Guid(EngineGuid).ToString("B"))) {
                exceptionAssistantKey.SetValue("", Name);
            }
        }

        public override void Unregister(RegistrationContext context) { }
    }
}
