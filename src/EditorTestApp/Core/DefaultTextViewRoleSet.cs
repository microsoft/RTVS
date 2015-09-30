using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Application.Core
{
    [ExcludeFromCodeCoverage]
    internal class DefaultTextViewRoleSet : TextViewRoleSet
    {
        static private readonly string[] _predefinedRoles = 
            {
                PredefinedTextViewRoles.Analyzable, 
                PredefinedTextViewRoles.Document,
                PredefinedTextViewRoles.Editable,
                PredefinedTextViewRoles.Interactive,
                PredefinedTextViewRoles.PrimaryDocument,
                PredefinedTextViewRoles.Structured,
                PredefinedTextViewRoles.Zoomable
            };

        public DefaultTextViewRoleSet()
            : base(_predefinedRoles)
        {
        }
    }
}
