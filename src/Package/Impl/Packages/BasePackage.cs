// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using Microsoft.VisualStudio.R.Package.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Packages {
    public abstract class BasePackage<TLanguageService> : VisualStudio.Shell.Package, IPackage
        where TLanguageService : class, new() {
        private readonly IList<IDisposable> _disposables = new List<IDisposable>();
        private Dictionary<IVsProjectGenerator, uint> _projectFileGenerators;
        private RToolbar _toolbar;

        protected abstract IEnumerable<IVsEditorFactory> CreateEditorFactories();
        protected virtual IEnumerable<IVsProjectGenerator> CreateProjectFileGenerators() { return new IVsProjectGenerator[0]; }
        protected virtual IEnumerable<IVsProjectFactory> CreateProjectFactories() { return new IVsProjectFactory[0]; }
        protected virtual IEnumerable<MenuCommand> CreateMenuCommands() { return new MenuCommand[0]; }

        #region IPackage
        /// <summary>
        /// Retrieve service local to the package such as IMenuService
        /// </summary>
        public T GetPackageService<T>(Type t = null) where T : class {
            return GetService(t ?? typeof(T)) as T;
        }
        #endregion

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that relies on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize() {
            base.Initialize();

            IServiceContainer container = this;
            container.AddService(typeof(TLanguageService), new TLanguageService(), true);

            foreach (var projectFactory in CreateProjectFactories()) {
                RegisterProjectFactory(projectFactory);
            }

            foreach (var projectFileGenerator in CreateProjectFileGenerators()) {
                RegisterProjectFileGenerator(projectFileGenerator);
            }

            foreach (var editorFactory in CreateEditorFactories()) {
                RegisterEditorFactory(editorFactory);
            }

            var menuCommandService = (IMenuCommandService)GetService(typeof(IMenuCommandService));
            foreach (var commmand in CreateMenuCommands()) {
                menuCommandService.AddCommand(commmand);
            }

            var settings = VsAppShell.Current.GetService<IRSettings>();
            var dte = VsAppShell.Current.GetService<DTE2>(typeof(DTE));
            _toolbar = new RToolbar(dte, settings);
            _toolbar.Show();
        }

        protected override int QueryClose(out bool canClose) {
            _toolbar.SaveState();
            return base.QueryClose(out canClose);
        }

        protected void AdviseExportedWindowFrameEvents<T>() where T : class {
            var windowFrameEvents = VsAppShell.Current.GetService<T>() as IVsWindowFrameEvents;
            var shell = (IVsUIShell7)GetService(typeof(SVsUIShell));
            var cookie = shell.AdviseWindowFrameEvents(windowFrameEvents);
            _disposables.Add(Disposable.Create(() => shell.UnadviseWindowFrameEvents(cookie)));
        }

        protected void AdviseExportedDebuggerEvents<T>() where T : class {
            var debuggerEvents = VsAppShell.Current.GetService<T>() as IVsDebuggerEvents;
            var debugger = (IVsDebugger)GetService(typeof(IVsDebugger));

            uint cookie;
            debugger.AdviseDebuggerEvents(debuggerEvents, out cookie);
            _disposables.Add(Disposable.Create(() => debugger.UnadviseDebuggerEvents(cookie)));
        }

        private void RegisterProjectFileGenerator(IVsProjectGenerator projectFileGenerator) {
            var registerProjectGenerators = GetService(typeof(SVsRegisterProjectTypes)) as IVsRegisterProjectGenerators;
            if (registerProjectGenerators == null) {
                throw new InvalidOperationException(typeof(SVsRegisterProjectTypes).FullName);
            }

            uint cookie;
            Guid riid = projectFileGenerator.GetType().GUID;
            registerProjectGenerators.RegisterProjectGenerator(ref riid, projectFileGenerator, out cookie);

            if (_projectFileGenerators == null) {
                _projectFileGenerators = new Dictionary<IVsProjectGenerator, uint>();
            }

            _projectFileGenerators[projectFileGenerator] = cookie;
        }

        protected override void Dispose(bool disposing) {
            if (!disposing) {
                base.Dispose(false);
                return;
            }

            if (_projectFileGenerators != null) {
                var projectFileGenerators = _projectFileGenerators;
                _projectFileGenerators = null;
                UnregisterProjectFileGenerators(projectFileGenerators);
            }

            foreach (var disposable in _disposables.Reverse()) {
                disposable.Dispose();
            }
            _disposables.Clear();

            IServiceContainer container = this;
            container.RemoveService(typeof(TLanguageService));

            // Base still needs shell to save settings
            base.Dispose(true);
        }

        private void UnregisterProjectFileGenerators(Dictionary<IVsProjectGenerator, uint> projectFileGenerators) {
            try {
                IVsRegisterProjectGenerators registerProjectGenerators = GetService(typeof(SVsRegisterProjectTypes)) as IVsRegisterProjectGenerators;
                if (registerProjectGenerators != null) {
                    foreach (var projectFileGenerator in projectFileGenerators) {
                        try {
                            registerProjectGenerators.UnregisterProjectGenerator(projectFileGenerator.Value);
                        } finally {
                            (projectFileGenerator.Key as IDisposable)?.Dispose();
                        }
                    }
                }
            } catch (Exception e) {
                Debug.Fail(Invariant($"Failed to dispose project file generator for package {GetType().FullName}\n{e.Message}"));
            }
        }
    }
}

