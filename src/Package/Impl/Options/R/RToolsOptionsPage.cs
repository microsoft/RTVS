// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Components.ConnectionManager.Implementation;
using Microsoft.R.Components.Settings;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    public class RToolsOptionsPage : DialogPage {
        private readonly IRToolsSettings _settings;
        private SettingsHolder _holder;

        public RToolsOptionsPage() {
            _settings = VsAppShell.Current.ExportProvider.GetExportedValue<IRToolsSettings>();
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

        [LocCategory("Settings_WorkspaceCategory")]
        [CustomLocDisplayName("Settings_LoadRDataOnProjectLoad")]
        [LocDescription("Settings_LoadRDataOnProjectLoad_Description")]
        [TypeConverter(typeof(YesNoAskTypeConverter))]
        [DefaultValue(YesNoAsk.No)]
        public YesNoAsk LoadRDataOnProjectLoad {
            get { return _holder.GetValue<YesNoAsk>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_WorkspaceCategory")]
        [CustomLocDisplayName("Settings_SaveRDataOnProjectUnload")]
        [LocDescription("Settings_SaveRDataOnProjectUnload_Description")]
        [TypeConverter(typeof(YesNoAskTypeConverter))]
        [DefaultValue(YesNoAsk.No)]
        public YesNoAsk SaveRDataOnProjectUnload {
            get { return _holder.GetValue<YesNoAsk>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HistoryCategory")]
        [CustomLocDisplayName("Settings_AlwaysSaveHistory")]
        [LocDescription("Settings_AlwaysSaveHistory_Description")]
        [DefaultValue(true)]
        public bool AlwaysSaveHistory {
            get { return _holder.GetValue<bool>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HistoryCategory")]
        [CustomLocDisplayName("Settings_ClearFilterOnAddHistory")]
        [LocDescription("Settings_ClearFilterOnAddHistory_Description")]
        [DefaultValue(true)]
        public bool ClearFilterOnAddHistory {
            get { return _holder.GetValue<bool>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HistoryCategory")]
        [CustomLocDisplayName("Settings_MultilineHistorySelection")]
        [LocDescription("Settings_MultilineHistorySelection_Description")]
        [DefaultValue(true)]
        public bool MultilineHistorySelection {
            get { return _holder.GetValue<bool>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_REngineCategory")]
        [CustomLocDisplayName("Settings_RCodePage")]
        [LocDescription("Settings_RCodePage_Description")]
        [TypeConverter(typeof(EncodingTypeConverter))]
        [DefaultValue(0)]
        public int RCodePage {
            get { return _holder.GetValue<int>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_DebuggingCategory")]
        [CustomLocDisplayName("Settings_EvaluateActiveBindings")]
        [LocDescription("Settings_EvaluateActiveBindings_Description")]
        [DefaultValue(true)]
        public bool EvaluateActiveBindings {
            get { return _holder.GetValue<bool>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_DebuggingCategory")]
        [CustomLocDisplayName("Settings_ShowDotPrefixedVariables")]
        [LocDescription("Settings_ShowDotPrefixedVariables_Description")]
        [DefaultValue(false)]
        public bool ShowDotPrefixedVariables {
            get { return _holder.GetValue<bool>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HelpCategory")]
        [CustomLocDisplayName("Settings_HelpBrowser")]
        [LocDescription("Settings_HelpBrowser_Description")]
        [TypeConverter(typeof(HelpBrowserTypeConverter))]
        [DefaultValue(HelpBrowserType.Automatic)]
        public HelpBrowserType HelpBrowser {
            get { return _holder.GetValue<HelpBrowserType>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HelpCategory")]
        [CustomLocDisplayName("Settings_WebHelpSearchString")]
        [LocDescription("Settings_WebHelpSearchString_Description")]
        [DefaultValue("R site:stackoverflow.com")]
        public string WebHelpSearchString {
            get { return _holder.GetValue<string>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HelpCategory")]
        [CustomLocDisplayName("Settings_WebHelpSearchBrowserType")]
        [LocDescription("Settings_WebHelpSearchBrowserType_Description")]
        [TypeConverter(typeof(BrowserTypeConverter))]
        [DefaultValue(BrowserType.Internal)]
        public BrowserType WebHelpSearchBrowserType {
            get { return _holder.GetValue<BrowserType>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_HtmlCategory")]
        [CustomLocDisplayName("Settings_HtmlBrowserType")]
        [LocDescription("Settings_HtmlBrowserType_Description")]
        [TypeConverter(typeof(BrowserTypeConverter))]
        [DefaultValue(BrowserType.Internal)]
        public BrowserType HtmlBrowserType {
            get { return _holder.GetValue<BrowserType>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_MarkdownCategory")]
        [CustomLocDisplayName("Settings_MarkdownBrowserType")]
        [LocDescription("Settings_MarkdownBrowserType_Description")]
        [TypeConverter(typeof(BrowserTypeConverter))]
        [DefaultValue(BrowserType.External)]
        public BrowserType MarkdownBrowserType {
            get { return _holder.GetValue<BrowserType>(); }
            set { _holder.SetValue(value); }
        }

        [LocCategory("Settings_GeneralCategory")]
        [CustomLocDisplayName("Settings_SurveyNewsCheck")]
        [LocDescription("Settings_SurveyNewsCheck_Description")]
        [TypeConverter(typeof(SurveyNewsPolicyTypeConverter))]
        [DefaultValue(SurveyNewsPolicy.CheckOnceWeek)]
        public SurveyNewsPolicy SurveyNewsCheck {
            get { return RToolsSettings.Current.SurveyNewsCheck; }
            set { RToolsSettings.Current.SurveyNewsCheck = value; }
        }

        [LocCategory("Settings_LogCategory")]
        [CustomLocDisplayName("Settings_LogLevel")]
        [LocDescription("Settings_LogLevel_Description")]
#if DEBUG
        [DefaultValue(LogVerbosity.Traffic)]
#else
        [DefaultValue(LogVerbosity.Normal)]
#endif
        public LogVerbosity LogLevel {
            get { return RToolsSettings.Current.LogVerbosity; }
            set { RToolsSettings.Current.LogVerbosity = value; }
        }


        public override void LoadSettingsFromStorage() { }
        public override void SaveSettingsToStorage() { }

        

        protected override void OnApply(PageApplyEventArgs e) {
            if (e.ApplyBehavior == ApplyKind.Apply) {
                _holder.Apply();
                RtvsTelemetry.Current.ReportSettings();
            }
            base.OnApply(e);
        }

        class SettingsHolder {
            private readonly IRToolsSettings _settings;
            private readonly IDictionary<string, object> _dict;

            public SettingsHolder(IRToolsSettings settings) {
                _settings = settings;
                _dict = ((IRPersistentSettings)settings).ToDictionary();
            }

            public T GetValue<T>([CallerMemberName] string name = null) {
                object value;
                _dict.TryGetValue(name, out value);
                return (T)value;
            }

            public void SetValue(object value, [CallerMemberName] string name = null) {
                Debug.Assert(_dict.ContainsKey(name), Invariant($"Unknown setting {name}. RToolsOptionsPage property name does not match IRToolsSettings"));
                _dict[name] = value;
            }

            public void Apply() {
                foreach(var kvp in _dict) {
                    _settings.GetType().GetProperty(kvp.Key).SetValue(_settings, kvp.Value);
                }
            }
        }
    }
}
