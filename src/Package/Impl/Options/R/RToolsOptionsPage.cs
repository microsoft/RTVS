using System.ComponentModel;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R
{
    public class RToolsOptionsPage : DialogPage
	{
		private bool _someSetting;

		[Category("SettingCategory")]
		[CustomLocDisplayName("Temp_SettingDisplayName")]
		[LocDescription("Temp_SettingDescription")]
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
