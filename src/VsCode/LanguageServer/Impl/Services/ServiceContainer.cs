// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Tasks;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor;
using Microsoft.R.LanguageServer.Documents;
using Microsoft.R.LanguageServer.InteractiveWorkflow;
using Microsoft.R.LanguageServer.Services.Editor;
using Microsoft.R.LanguageServer.Threading;

#if NETCOREAPP1_1
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.Common.Core;
#endif

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class ServiceContainer : ServiceManager {
        public ServiceContainer() {
            var mt = new MainThread();
            SynchronizationContext.SetSynchronizationContext(mt.SynchronizationContext);

                AddService<IActionLog>(s => new Logger("VSCode-R", Path.GetTempPath(), s))
            .AddService(mt)
            .AddService(new ContentTypeServiceLocator())
            .AddService<ISettingsStorage, SettingsStorage>()
            .AddService<ITaskService, TaskService>()
            .AddService<IImageService, ImageService>()
            .AddService(new Application())
            .AddService<IRInteractiveWorkflowProvider, RInteractiveWorkflowProvider>()
            .AddService<ICoreShell, CoreShell>()
            .AddService(new IdleTimeService(this))
            .AddService(new DocumentCollection(this))
            .AddService(new ViewSignatureBroker())
            .AddEditorServices();

            AddPlatformSpecificServices();
        }

        private void AddPlatformSpecificServices() {
#if NETCOREAPP1_1
            var thisAssembly = Assembly.GetEntryAssembly();
            Assembly assembly;
            var platformAssemblyName = GetPlatformServiceProviderAssemblyName();
            try {
                var thisAssemblyName = thisAssembly.GetName();
                var name = Path.GetFileNameWithoutExtension(platformAssemblyName);
                var token = thisAssemblyName.GetPublicKeyToken();
                var tokenString = token != null && token.Length > 0 ? Encoding.ASCII.GetString(token) : "null";
                var asmName = new AssemblyName($"{name}, Version={thisAssemblyName.Version}, Culture=neutral, PublicKeyToken={tokenString}");
                assembly = Assembly.Load(asmName);
            } catch(FileLoadException) {
                var thisAssemblyPath = thisAssembly.GetAssemblyPath();
                var assemblyLoc = Path.GetDirectoryName(thisAssemblyPath);
                var platformServicesAssemblyPath = Path.Combine(assemblyLoc, platformAssemblyName);
                assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(platformServicesAssemblyPath);
            }

            var classType = assembly.GetType("Microsoft.R.Platform.ServiceProvider");
            var mi = classType.GetMethod("ProvideServices", BindingFlags.Static | BindingFlags.Public);
            mi.Invoke(null, new object[] { this });
#endif
        }

        private static string GetPlatformServiceProviderAssemblyName() {
            var suffix = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".Windows.dll" : ".Linux.dll";
            return "Microsoft.R.Platform" + suffix;
        }
    }
}
