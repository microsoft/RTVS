// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using EnvDTE;
using EnvDTE80;
using System;

namespace Microsoft.VisualStudio.Shell.Mocks {
    public class DteMock : DTE2 {
        public Document ActiveDocument { get; set; }
        public dynamic ActiveSolutionProjects { get; set; }
        public Window ActiveWindow { get; set; }
        public AddIns AddIns { get; set; }
        public DTE Application { get; set; }
        public dynamic CommandBars { get; set; }
        public string CommandLineArguments { get; set; }
        public Commands Commands { get; set; }
        public ContextAttributes ContextAttributes { get; set; }
        public Debugger Debugger { get; set; }
        public vsDisplay DisplayMode { get; set; }
        public Documents Documents { get; set; }
        public DTE DTE { get; set; }
        public string Edition { get; set; }
        public EnvDTE.Events Events { get; set; }
        public string FileName { get; set; }
        public Find Find { get; set; }
        public string FullName { get; set; }
        public Globals Globals { get; set; }
        public ItemOperations ItemOperations { get; set; }
        public int LocaleID { get; set; }
        public Macros Macros { get; set; }
        public DTE MacrosIDE { get; set; }
        public Window MainWindow { get; set; }
        public vsIDEMode Mode { get; set; }
        public string Name { get; set; }
        public ObjectExtenders ObjectExtenders { get; set; }
        public string RegistryRoot { get; set; }
        public SelectedItems SelectedItems { get; set; }
        public Solution Solution { get; set; }
        public SourceControl SourceControl { get; set; }
        public StatusBar StatusBar { get; set; }
        public bool SuppressUI { get; set; }
        public ToolWindows ToolWindows { get; set; }
        public UndoContext UndoContext { get; set; }
        public bool UserControl { get; set; }
        public string Version { get; set; }
        public WindowConfigurations WindowConfigurations { get; set; }
        public EnvDTE.Windows Windows { get; set; }

        public void ExecuteCommand(string CommandName, string CommandArgs = "") {
            throw new NotImplementedException();
        }

        public dynamic GetObject(string Name) {
            throw new NotImplementedException();
        }

        public uint GetThemeColor(vsThemeColors Element) {
            throw new NotImplementedException();
        }

        public bool get_IsOpenFile(string ViewKind, string FileName) {
            throw new NotImplementedException();
        }

        public Properties get_Properties(string Category, string Page) {
            throw new NotImplementedException();
        }

        public wizardResult LaunchWizard(string VSZFile, ref object[] ContextParams) {
            throw new NotImplementedException();
        }

        public Window OpenFile(string ViewKind, string FileName) {
            throw new NotImplementedException();
        }

        public void Quit() {
            throw new NotImplementedException();
        }

        public string SatelliteDllPath(string Path, string Name) {
            throw new NotImplementedException();
        }
    }
}
