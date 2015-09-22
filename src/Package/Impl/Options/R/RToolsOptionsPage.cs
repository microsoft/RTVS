using System.ComponentModel;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R
{
    public class RToolsOptionsPage : DialogPage
	{
        public RToolsOptionsPage()
        {
            this.SettingsRegistryPath = @"UserSettings\R_Tools";
        }

        private bool _someSetting;

		[Category("Settings_ReplCategory")]
		[CustomLocDisplayName("Settings_SendToRepl")]
		[LocDescription("Settings_SendToRepl_Description")]
		public bool SomeSetting
		{
			get { return this._someSetting; }
			set
			{
				if (this._someSetting != value)
				{
					this._someSetting = value;
				}
			}
		}
	}
}
