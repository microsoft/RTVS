using System.ComponentModel.Composition;
using Microsoft.R.Engine.Settings.Definitions;

namespace Microsoft.VisualStudio.R.Package.Options.R
{
    [Export(typeof(IRToolsSettings))]
    internal sealed class RToolsSettings : IRToolsSettings
    {
        public string GetRVersionPath()
        {
            // TODO: implement
            return string.Empty;
        }

        public int HelpPortNumber
        {
            get { return 8186; }
        }
    }
}
