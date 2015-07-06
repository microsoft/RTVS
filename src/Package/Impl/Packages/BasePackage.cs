using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Packages
{
    public abstract class BasePackage<TLanguageService> : Microsoft.VisualStudio.Shell.Package
        where TLanguageService : class, new()
    {
        protected BasePackage()
        {
        }

        protected abstract IEnumerable<IVsEditorFactory> CreateEditorFactories();

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that relies on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            AppShell.AddRef();

            IServiceContainer container = this as IServiceContainer;
            container.AddService(typeof(TLanguageService), new TLanguageService(), true);

            foreach (var editorFactory in CreateEditorFactories())
            {
                base.RegisterEditorFactory(editorFactory);
            }
        }

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IServiceContainer container = this as IServiceContainer;
                container.RemoveService(typeof(TLanguageService));

                AppShell.Release();

                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
