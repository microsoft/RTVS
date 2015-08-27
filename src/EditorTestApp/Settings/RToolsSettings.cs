using System.ComponentModel.Composition;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.Languages.Editor.Application.Settings
{
    [Export(typeof(IRToolsSettings))]
    class RToolsSettings : IRToolsSettings
    {
        public string GetRVersionPath()
        {
            return string.Empty;
        }
    }
}
