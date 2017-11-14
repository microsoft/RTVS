// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using Microsoft.Common.Core;
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
using Microsoft.R.LanguageServer.Text;
using Microsoft.R.LanguageServer.Threading;

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
            .AddService(new IdleTimeService(this))
            .AddService(new DocumentCollection(this))
            .AddService(new ViewSignatureBroker())
            .AddService(new EditorSupport())
            .AddService(new REvalSession(this))
            .AddEditorServices();

            AddPlatformSpecificServices();
        }

        private void AddPlatformSpecificServices() {
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
            } catch(IOException) {
                var thisAssemblyPath = thisAssembly.GetAssemblyPath();
                var assemblyLoc = Path.GetDirectoryName(thisAssemblyPath);
                var platformServicesAssemblyPath = Path.Combine(assemblyLoc, platformAssemblyName);
                assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(platformServicesAssemblyPath);
            }

            var classType = assembly.GetType("Microsoft.R.Platform.ServiceProvider");
            var mi = classType.GetMethod("ProvideServices", BindingFlags.Static | BindingFlags.Public);
            mi.Invoke(null, new object[] { this });
        }

        private static string GetPlatformServiceProviderAssemblyName() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return @"Broker\Windows\Microsoft.R.Platform.Windows.Core.dll";
            }
            return "Microsoft.R.Platform.Unix.dll";
        }
    }
}
