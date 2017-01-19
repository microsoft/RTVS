// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Json;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Settings;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    public class RToolsOptionsPage : DialogPage {
        private readonly IRPersistentSettings _settings;
        private SettingsHolder _holder;

        public RToolsOptionsPage() {
            _settings = VsAppShell.Current.ExportProvider.GetExportedValue<IRPersistentSettings>();
            _holder = new SettingsHolder(_settings);
        }

        [Browsable(false)]
        public bool IsLoadingFromStorage { get; private set; }

        [LocCategory("Settings_GeneralCategory")]
        [CustomLocDisplayName("Settings_CranMirror")]
        [LocDescription("Settings_CranMirror_Description")]
        [TypeConverter(typeof(CranMirrorTypeConverter))]
        [DefaultValue(null)]
        public string CranMirror {
            get { return _holder.GetValue<string>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory(nameof(Resources.Settings_WorkspaceCategory))]
        [CustomLocDisplayName(nameof(Resources.Settings_ShowWorkspaceSwitchConfirmationDialog))]
        [LocDescription(nameof(Resources.Settings_ShowWorkspaceSwitchConfirmationDialog_Description))]
        [TypeConverter(typeof(YesNoTypeConverter))]
        [DefaultValue(YesNoAsk.No)]
        public YesNo ShowWorkspaceSwitchConfirmationDialog {
            get { return _holder.GetValue(YesNo.No); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_WorkspaceCategory")]
        [CustomLocDisplayName(nameof(Resources.Settings_LoadRDataOnProjectLoad))]
        [LocDescription(nameof(Resources.Settings_LoadRDataOnProjectLoad_Description))]
        [TypeConverter(typeof(YesNoAskTypeConverter))]
        [DefaultValue(YesNoAsk.No)]
        public YesNoAsk LoadRDataOnProjectLoad {
            get { return _holder.GetValue(YesNoAsk.No); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_WorkspaceCategory")]
        [CustomLocDisplayName("Settings_SaveRDataOnProjectUnload")]
        [LocDescription("Settings_SaveRDataOnProjectUnload_Description")]
        [TypeConverter(typeof(YesNoAskTypeConverter))]
        [DefaultValue(YesNoAsk.No)]
        public YesNoAsk SaveRDataOnProjectUnload {
            get { return _holder.GetValue(YesNoAsk.No); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HistoryCategory")]
        [CustomLocDisplayName("Settings_AlwaysSaveHistory")]
        [LocDescription("Settings_AlwaysSaveHistory_Description")]
        [DefaultValue(true)]
        public bool AlwaysSaveHistory {
            get { return _holder.GetValue<bool>(true); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HistoryCategory")]
        [CustomLocDisplayName("Settings_ClearFilterOnAddHistory")]
        [LocDescription("Settings_ClearFilterOnAddHistory_Description")]
        [DefaultValue(true)]
        public bool ClearFilterOnAddHistory {
            get { return _holder.GetValue<bool>(true); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HistoryCategory")]
        [CustomLocDisplayName("Settings_MultilineHistorySelection")]
        [LocDescription("Settings_MultilineHistorySelection_Description")]
        [DefaultValue(true)]
        public bool MultilineHistorySelection {
            get { return _holder.GetValue<bool>(true); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_REngineCategory")]
        [CustomLocDisplayName("Settings_RCodePage")]
        [LocDescription("Settings_RCodePage_Description")]
        [TypeConverter(typeof(EncodingTypeConverter))]
        [DefaultValue(0)]
        public int RCodePage {
            get { return _holder.GetValue<int>(0); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_DebuggingCategory")]
        [CustomLocDisplayName("Settings_EvaluateActiveBindings")]
        [LocDescription("Settings_EvaluateActiveBindings_Description")]
        [DefaultValue(true)]
        public bool EvaluateActiveBindings {
            get { return _holder.GetValue<bool>(true); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_DebuggingCategory")]
        [CustomLocDisplayName("Settings_ShowDotPrefixedVariables")]
        [LocDescription("Settings_ShowDotPrefixedVariables_Description")]
        [DefaultValue(false)]
        public bool ShowDotPrefixedVariables {
            get { return _holder.GetValue<bool>(false); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HelpCategory")]
        [CustomLocDisplayName("Settings_HelpBrowser")]
        [LocDescription("Settings_HelpBrowser_Description")]
        [TypeConverter(typeof(HelpBrowserTypeConverter))]
        [DefaultValue(HelpBrowserType.Automatic)]
        public HelpBrowserType HelpBrowserType {
            get { return _holder.GetValue<HelpBrowserType>(HelpBrowserType.Automatic); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HelpCategory")]
        [CustomLocDisplayName("Settings_WebHelpSearchString")]
        [LocDescription("Settings_WebHelpSearchString_Description")]
        [DefaultValue("R site:stackoverflow.com")]
        public string WebHelpSearchString {
            get { return _holder.GetValue<string>("R site:stackoverflow.com", "WebHelpSearchString"); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HelpCategory")]
        [CustomLocDisplayName("Settings_WebHelpSearchBrowserType")]
        [LocDescription("Settings_WebHelpSearchBrowserType_Description")]
        [TypeConverter(typeof(BrowserTypeConverter))]
        [DefaultValue(BrowserType.Internal)]
        public BrowserType WebHelpSearchBrowserType {
            get { return _holder.GetValue<BrowserType>(BrowserType.Internal); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HtmlCategory")]
        [CustomLocDisplayName("Settings_HtmlBrowserType")]
        [LocDescription("Settings_HtmlBrowserType_Description")]
        [TypeConverter(typeof(BrowserTypeConverter))]
        [DefaultValue(BrowserType.Internal)]
        public BrowserType HtmlBrowserType {
            get { return _holder.GetValue<BrowserType>(BrowserType.Internal); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_MarkdownCategory")]
        [CustomLocDisplayName("Settings_MarkdownBrowserType")]
        [LocDescription("Settings_MarkdownBrowserType_Description")]
        [TypeConverter(typeof(BrowserTypeConverter))]
        [DefaultValue(BrowserType.External)]
        public BrowserType MarkdownBrowserType {
            get { return _holder.GetValue<BrowserType>(BrowserType.External); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_GeneralCategory")]
        [CustomLocDisplayName("Settings_SurveyNewsCheck")]
        [LocDescription("Settings_SurveyNewsCheck_Description")]
        [TypeConverter(typeof(SurveyNewsPolicyTypeConverter))]
        [DefaultValue(SurveyNewsPolicy.CheckOnceWeek)]
        public SurveyNewsPolicy SurveyNewsCheck {
            get { return _holder.GetValue<SurveyNewsPolicy>(SurveyNewsPolicy.CheckOnceWeek); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_LogCategory")]
        [CustomLocDisplayName("Settings_LogLevel")]
        [LocDescription("Settings_LogLevel_Description")]
#if DEBUG
        [DefaultValue(LogVerbosity.Traffic)]
#else
        [DefaultValue(LogVerbosity.Normal)]
#endif
        public LogVerbosity LogVerbosity {
            get {
#if DEBUG
                return _holder.GetValue<LogVerbosity>(LogVerbosity.Traffic);
#else
                return _holder.GetValue<LogVerbosity>(LogVerbosity.Normal);
#endif
            }
            set { _holder.SetValue(value); }
        }

        /// Overrides default methods since we provide custom settings storage
        public override void LoadSettingsFromStorage() { }
        public override void SaveSettingsToStorage() { }

        protected override void OnApply(PageApplyEventArgs e) {
            if (e.ApplyBehavior == ApplyKind.Apply) {
                _holder.Apply();
                RtvsTelemetry.Current.ReportSettings();
            }
            base.OnApply(e);
        }

        /// <summary>
        /// Holds settings (name/values) while they are being edited. We don't 
        /// want to apply changes to the actual settings until user clicks OK.
        /// </summary>
        class SettingsHolder {
            private readonly IRPersistentSettings _settings;
            private readonly IDictionary<string, object> _dict;

            public SettingsHolder(IRPersistentSettings settings) {
                _settings = settings;
                _dict = settings.GetPropertyValueDictionary();
            }

            public T GetValue<T>(T defaultValue, [CallerMemberName] string name = null) {
                object value;
                return _dict.TryGetValue(name, out value) ? (T)value : default (T);
            }

            public T GetValue<T>([CallerMemberName] string name = null) => GetValue<T>(default(T), name);

            public void SetValue(object value, [CallerMemberName] string name = null) {
                Debug.Assert(_dict.ContainsKey(name), Invariant($"Unknown setting {name}. RToolsOptionsPage property name does not match IRToolsSettings"));
                _dict[name] = value;
            }

            public void Apply() {
                _settings.SetProperties(_dict);
                _settings.SaveSettingsAsync().DoNotWait();
            }
        }
    }
}
