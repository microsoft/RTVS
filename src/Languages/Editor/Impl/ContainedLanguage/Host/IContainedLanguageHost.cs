using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.Web.Editor.Workspace;

namespace Microsoft.Languages.Editor.ContainedLanguage
{
    /// <summary>
    /// Contained language host implemented in HTML editor. Provides
    /// additional method used in Web scenarios.
    /// </summary>
    public interface IContainedLanguageHost
    {
        /// <summary>
        /// Retrieves host file workspace item. Master document workspace item helps when 
        /// contained language needs to download files specified in a relative path in 
        /// the master document, such as when script document needs to download references 
        /// specified in a &lt;script> tag.
        /// </summary>
        IWorkspaceItem WorkspaceItem { get; }

        /// <summary>
        /// Retrieves contained command target
        /// </summary>
        ICommandTarget GetContainedCommandTarget(ITextView textView);
    }
}
