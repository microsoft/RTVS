using System.ComponentModel;
using System.Drawing.Design;
using Microsoft.Common.Core.Enums;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.R.Package.Telemetry;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    public class RToolsOptionsPage : DialogPage {
        private bool _loadingFromStorage;

        public RToolsOptionsPage() {
            this.SettingsRegistryPath = @"UserSettings\R_Tools";
        }

        [LocCategory("Settings_ReplCategory")]
        [CustomLocDisplayName("Settings_SendToRepl")]
        [LocDescription("Settings_SendToRepl_Description")]
        [TypeConverter(typeof(ReplShortcutTypeConverter))]
        [DefaultValue(true)]
        public bool SendToReplOnCtrlEnter {
            get { return REditorSettings.SendToReplOnCtrlEnter; }
            set { REditorSettings.SendToReplOnCtrlEnter = value; }
        }

        [LocCategory("Settings_GeneralCategory")]
        [CustomLocDisplayName("Settings_CranMirror")]
        [LocDescription("Settings_CranMirror_Description")]
        [TypeConverter(typeof(CranMirrorTypeConverter))]
        [DefaultValue("0-Cloud [https]")]
        public string CranMirror {
            get { return RToolsSettings.Current.CranMirror; }
            set { RToolsSettings.Current.CranMirror = value; }
        }

        [LocCategory("Settings_GeneralCategory")]
        [CustomLocDisplayName("Settings_RCommandLineArguments")]
        [LocDescription("Settings_RCommandLineArguments_Description")]
        public string RCommandLineArguments {
            get { return RToolsSettings.Current.RCommandLineArguments; }
            set { RToolsSettings.Current.RCommandLineArguments = value; }
        }

        [LocCategory("Settings_WorkspaceCategory")]
        [CustomLocDisplayName("Settings_LoadRDataOnProjectLoad")]
        [LocDescription("Settings_LoadRDataOnProjectLoad_Description")]
        [TypeConverter(typeof(YesNoAskTypeConverter))]
        [DefaultValue(YesNoAsk.No)]
        public YesNoAsk LoadRDataOnProjectLoad {
            get { return RToolsSettings.Current.LoadRDataOnProjectLoad; }
            set { RToolsSettings.Current.LoadRDataOnProjectLoad = value; }
        }

        [LocCategory("Settings_WorkspaceCategory")]
        [CustomLocDisplayName("Settings_SaveRDataOnProjectUnload")]
        [LocDescription("Settings_SaveRDataOnProjectUnload_Description")]
        [TypeConverter(typeof(YesNoAskTypeConverter))]
        [DefaultValue(YesNoAsk.Ask)]
        public YesNoAsk SaveRDataOnProjectUnload {
            get { return RToolsSettings.Current.SaveRDataOnProjectUnload; }
            set { RToolsSettings.Current.SaveRDataOnProjectUnload = value; }
        }

        [LocCategory("Settings_HistoryCategory")]
        [CustomLocDisplayName("Settings_AlwaysSaveHistory")]
        [LocDescription("Settings_AlwaysSaveHistory_Description")]
        [DefaultValue(true)]
        public bool AlwaysSaveHistory {
            get { return RToolsSettings.Current.AlwaysSaveHistory; }
            set { RToolsSettings.Current.AlwaysSaveHistory = value; }
        }

        [LocCategory("Settings_HistoryCategory")]
        [CustomLocDisplayName("Settings_ClearFilterOnAddHistory")]
        [LocDescription("Settings_ClearFilterOnAddHistory_Description")]
        [DefaultValue(true)]
        public bool ClearFilterOnAddHistory {
            get { return RToolsSettings.Current.ClearFilterOnAddHistory; }
            set { RToolsSettings.Current.ClearFilterOnAddHistory = value; }
        }

        [LocCategory("Settings_HistoryCategory")]
        [CustomLocDisplayName("Settings_MultilineHistorySelection")]
        [LocDescription("Settings_MultilineHistorySelection_Description")]
        [DefaultValue(false)]
        public bool MultilineHistorySelection {
            get { return RToolsSettings.Current.MultilineHistorySelection; }
            set { RToolsSettings.Current.MultilineHistorySelection = value; }
        }

        [LocCategory("Settings_GeneralCategory")]
        [CustomLocDisplayName("Settings_RBasePath")]
        [LocDescription("Settings_RBasePath_Description")]
        [Editor(typeof(ChooseRFolderUIEditor), typeof(UITypeEditor))]
        public string RVersion {
            get { return RToolsSettings.Current.RBasePath; }
            set {
                value = ValidateRBasePath(value);
                if (value != null) {
                    if (RToolsSettings.Current.RBasePath != value && !_loadingFromStorage) {
                        VsAppShell.Current.ShowErrorMessage(Resources.RPathChangedRestartVS);
                    }
                    RToolsSettings.Current.RBasePath = value;
                }
            }
        }

        [LocCategory("Settings_GeneralCategory")]
        [CustomLocDisplayName("Settings_HelpBrowser")]
        [LocDescription("Settings_HelpBrowser_Description")]
        [TypeConverter(typeof(HelpBrowserTypeConverter))]
        [DefaultValue(HelpBrowserType.Automatic)]
        public HelpBrowserType HelpBrowser {
            get { return RToolsSettings.Current.HelpBrowser; }
            set { RToolsSettings.Current.HelpBrowser = value; }
        }

        /// <summary>
        /// REPL working directory: not exposed in Tools | Options dialog,
        /// only saved along with other settings.
        /// </summary>
        internal string WorkingDirectory {
            get { return RToolsSettings.Current.WorkingDirectory; }
            set { RToolsSettings.Current.WorkingDirectory = value; }
        }

        internal string[] WorkingDirectoryList {
            get { return RToolsSettings.Current.WorkingDirectoryList; }
            set { RToolsSettings.Current.WorkingDirectoryList = value; }
        }

        public override void LoadSettingsFromStorage() {
            _loadingFromStorage = true;
            try {
                base.LoadSettingsFromStorage();
            } finally {
                _loadingFromStorage = false;
            }
        }

        private string ValidateRBasePath(string path) {
            // If path is null, folder selector dialog was canceled
            if (path != null) {
                bool valid = SupportedRVersions.VerifyRIsInstalled(path, showErrors: !_loadingFromStorage);
                if (!valid) {
                    if (_loadingFromStorage) {
                        // Bad data in the settings storage. Fix the value to default.
                        path = RInstallation.GetLatestEnginePathFromRegistry();
                    } else {
                        path = null; // Prevents assignment of bad values to the property.
                    }
                }
            }

            return path;
        }

        protected override void Dispose(bool disposing)
        {
            RtvsTelemetry.Current.ReportSettings();
            base.Dispose(disposing);
        }
    }
}
