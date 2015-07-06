using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Microsoft.VisualStudio.R.Package.Shell
{
	/// <summary>
	/// Application shell provides access to services such as 
    /// composition container, export provider, global VS IDE
	/// services and so on.
	/// </summary>
	public interface IApplicationShell : IDisposable
	{
		/// <summary>
		/// Retreieves Visual Studio global service from global VS service provider.
		/// This method is not thread safe and should not be called from async methods.
		/// </summary>
		/// <typeparam name="T">Service interface type such as IVsUiShell</typeparam>
		/// <param name="type">Service type if different from T, such as typeof(SVSUiShell)</param>
		/// <returns>Service instance of null if not found.</returns>
		T GetGlobalService<T>(Type type = null) where T : class;

		/// <summary>
		/// Visual Studio global service provider.
		/// The service provider should not be called from async methods.
		/// </summary>
		IServiceProvider GlobalServiceProvider { get; }

		/// <summary>
		/// Visual Studio OLE service provider.
		/// The service provider should not be called from async methods.
		/// </summary>
		Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider { get; }

		/// <summary>
		/// Visual Studio MEF composition service.
		/// </summary>
		ICompositionService CompositionService { get; }

		/// <summary>
		/// Visual Studio MEF export provider.
		/// </summary>
		ExportProvider ExportProvider { get; }

		/// <summary>
		/// Returns true if Visual Studio instance is running in the UI 
		/// test environment such as Apex/Omni.
		/// </summary>
		bool IsTestEnvironment { get; }
	}
}
