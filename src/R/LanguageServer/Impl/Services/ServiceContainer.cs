// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Editor;
using Microsoft.R.LanguageServer.Common;
using Microsoft.R.LanguageServer.InteractiveWorkflow;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class ServiceContainer : IServiceContainer {
        private readonly ServiceManager _services = new ServiceManager();

        public ServiceContainer() {
            _services.AddService<IActionLog>(s => new Logger("VSCode-R", Path.GetTempPath(), s))
                .AddService<IFileSystem, FileSystem>()
                .AddService<ILoggingPermissions, LoggingPermissions>()
                .AddService<IMainThread, MainThread>()
                .AddService<IProcessServices, ProcessServices>()
                .AddService<ISettingsStorage, SettingsStorage>()
                .AddService<IRSettings, RSettings>()
                .AddService<ISecurityService, SecurityService>()
                .AddService<ITaskService, TaskService>()
                .AddService<IImageService, ImageService>()
                .AddService(new Application())
                .AddService<IRInteractiveWorkflowProvider, RInteractiveWorkflowProvider>()
                .AddService<ICoreShell, CoreShell>()
                .AddService<IREditorSettings, REditorSettings>()
                .AddService<IIdleTimeService, IdleTimeService>()
                .AddService<IContentTypeServiceLocator, ContentTypeServiceLocator>()
                .AddEditorServices();
        }

        public T GetService<T>(Type type = null) where T : class => _services.GetService<T>(type);
        public IEnumerable<Type> AllServices => _services.AllServices;
        public IEnumerable<T> GetServices<T>() where T : class => _services.GetServices<T>();
    }
}
