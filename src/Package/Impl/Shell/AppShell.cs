using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.VisualStudio.R.Package.Shell
{
    /// <summary>
    /// Application shell provides access to services
    /// such as composition container, export provider, global VS IDE
    /// services and so on.
    /// </summary>
    public class AppShell : IApplicationShell
    {
        private static IApplicationShell instance;
        private static int _refCount;
        private static IEditorShell _shell;
        private static bool _appTerminated;

        /// <summary>
        /// Current application shell instance. Provides access to services
        /// such as composition container, export provider, global VS IDE
        /// services and so on.
        /// </summary>
        public static IApplicationShell Current
        {
            get
            {
                if (AppShell.instance == null)
                {
                    AppShell.instance = new AppShell();
                }

                return AppShell.instance;
            }
            internal set
            {
                // Only used in component tests
                AppShell.instance = value;
            }
        }

        public AppShell()
        {
            // Check if test assemblies are loaded into the VS process
            this.DetemineTestEnvironment();
        }

        #region IApplicationShell
        /// <summary>
        /// Retreieves Visual Studio global service from global VS service provider.
        /// This method is not thread safe and should not be called from async methods.
        /// </summary>
        /// <typeparam name="T">Service interface type such as IVsUiShell</typeparam>
        /// <param name="type">Service type if different from T, such as typeof(SVSUiShell)</param>
        /// <returns>Service instance of null if not found.</returns>
        public T GetGlobalService<T>(Type type = null) where T : class
        {
            if(IsTestEnvironment)
            {
                IServiceProvider sp = RPackage.Current;
                return sp.GetService(type ?? typeof(T)) as T;
            }

            return Microsoft.VisualStudio.Shell.Package.GetGlobalService(type ?? typeof(T)) as T;
        }

        /// <summary>
        /// Visual Studio global service provider.
        /// The service provider should not be called from async methods.
        /// </summary>
        public IServiceProvider GlobalServiceProvider
        {
            get { return ServiceProvider.GlobalProvider; }
        }

        /// <summary>
        /// Visual Studio OLE service provider.
        /// The service provider should not be called from async methods.
        /// </summary>
        public Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
        {
            get { return GetGlobalService<Microsoft.VisualStudio.OLE.Interop.IServiceProvider>(); }
        }

        /// <summary>
        /// Retreieves Visual Studio global service from global VS service provider.
        /// This method is not thread safe and should not be called from async methods.
        /// </summary>
        /// <typeparam name="T">Service interface type such as IVsUiShell</typeparam>
        /// <param name="sid">Service GUID</param>
        /// <returns>Service instance of null if not found.</returns>
        public T GetGlobalService<T>(Guid sid) where T : class
        {
            object service;
            ServiceProvider.GlobalProvider.QueryService(sid, out service);

            return service as T;
        }

        /// <summary>
        /// Returns VS component model (MEF container)
        /// </summary>
        public IComponentModel ComponentModel
        {
            get { return GetGlobalService<IComponentModel>(typeof(SComponentModel)); }
        }

        /// <summary>
        /// Visual Studio MEF composition service.
        /// </summary>
        public ICompositionService CompositionService
        {
            get { return ComponentModel.DefaultCompositionService; }
        }

        /// <summary>
        /// Visual Studio MEF export provider.
        /// </summary>
        public ExportProvider ExportProvider
        {
            get { return ComponentModel.DefaultExportProvider; }
        }

        /// <summary>
        /// Returns true if Visual Studio instance is running in the UI 
        /// test environment such as Apex/Omni.
        /// </summary>
        public bool IsTestEnvironment { get; private set; }

        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        // Check if test assemblies are loaded into the VS process
        private void DetemineTestEnvironment()
        {
            AppDomain ad = AppDomain.CurrentDomain;
            Assembly[] loadedAssemblies = ad.GetAssemblies();

            Assembly testAssembly = loadedAssemblies.FirstOrDefault((asm) =>
            {
                AssemblyName assemblyName = asm.GetName();
                string name = assemblyName.Name;
                return name.IndexOf("apex", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("r.editor.test.", StringComparison.OrdinalIgnoreCase) >= 0;
            });

            this.IsTestEnvironment = testAssembly != null;
        }

        internal static void AddRef()
        {
            if (++_refCount == 1)
            {
                Initialize();
            }
        }

        internal static void Release()
        {
            Debug.Assert(_refCount > 0);

            if (--_refCount == 0)
            {
                Close();
            }
        }

        private static void Initialize()
        {
            if (_shell == null)
            {
                Debug.Assert(!_appTerminated, "R Tools: Editor shell shouldn't be created when quitting the app");
                if (_appTerminated)
                {
                    throw new InvalidOperationException("R Tools: AppShell.Initialize can't be called during shutdown.");
                }

                IEditorShell existingShell = EditorShell.HasShell ? EditorShell.Current : null;
                _shell = existingShell;

                // Don't create my own host if one has already been set (like during unit tests)
                if (_shell == null && existingShell == null)
                {
                    _shell = new VsEditorShell();
                    EditorShell.SetShell(_shell);
                }

                if (_shell != null)
                {
                    _shell.Terminating += OnTerminateApp;
                }
            }
        }

        private static void Close()
        {
            if (_shell != null)
            {
                IEditorShell shell = _shell;

                _shell = null;
                EditorShell.RemoveShell(shell);
                shell.Terminating -= OnTerminateApp;

                if (shell is IDisposable)
                {
                    ((IDisposable)shell).Dispose();
                }
            }
        }

        private static void OnTerminateApp(object sender, EventArgs eventArgs)
        {
            // Wait for the public OnTerminateApp() to be called to do the cleanup
        }

        public static void OnTerminateApp()
        {
            Close();
            _appTerminated = true;
        }
    }
}
