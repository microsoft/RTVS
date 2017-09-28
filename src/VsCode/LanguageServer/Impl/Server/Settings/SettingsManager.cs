// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Editor.Settings;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Settings;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor;
using Microsoft.R.Editor.Validation.Lint;
using Microsoft.R.LanguageServer.Settings;

namespace Microsoft.R.LanguageServer.Server.Settings {
    internal sealed class SettingsManager : ISettingsManager {
        private readonly REngineSettings _engineSettings = new REngineSettings();
        private readonly REditorSettings _editorSettings = new REditorSettings();

        public SettingsManager(IServiceManager serviceManager) {
            serviceManager
                .AddService(this)
                .AddService(_engineSettings)
                .AddService(_editorSettings)
                .AddService(new RSettings());
        }

        public void Dispose() => _editorSettings.Dispose();

        public void UpdateSettings(LanguageServerSettings vscodeSettings) {

            var e = vscodeSettings.Editor;
            _editorSettings.FormatScope = e.FormatScope;
            _editorSettings.FormatOptions.BreakMultipleStatements = e.BreakMultipleStatements;
            _editorSettings.FormatOptions.IndentSize = e.TabSize;
            _editorSettings.FormatOptions.TabSize = e.TabSize;
            _editorSettings.FormatOptions.SpaceAfterKeyword = e.SpaceAfterKeyword;
            _editorSettings.FormatOptions.SpaceBeforeCurly = e.SpaceBeforeCurly;
            _editorSettings.FormatOptions.SpacesAroundEquals = e.SpacesAroundEquals;

            _editorSettings.LintOptions = vscodeSettings.Linting;
            _engineSettings.InterpreterIndex = vscodeSettings.Interpreter;
        }

        private sealed class REngineSettings : IREngineSettings {
            public int InterpreterIndex { get; set; }
        }

        private sealed class REditorSettings : IREditorSettings {
            private readonly IEditorSettingsStorage _storage;

            public REditorSettings() {
                _storage = new EditorSettingsStorage();
                LintOptions = new LintOptions(() => _storage);
            }

            public void Dispose() => _storage.Dispose();

            public event EventHandler<EventArgs> SettingsChanged;
            public bool AutoFormat { get; set; } = true;
            public bool CompletionEnabled { get; set; } = true;
            public int IndentSize { get; set; } = 2;
            public IndentType IndentType { get; set; } = IndentType.Spaces;
            public int TabSize { get; set; } = 2;
            public IndentStyle IndentStyle { get; set; } = IndentStyle.Smart;
            public bool SyntaxCheckEnabled { get; set; } = true;
            public bool SignatureHelpEnabled { get; set; } = true;
            public bool InsertMatchingBraces { get; set; } = true;
            public bool FormatOnPaste { get; set; }
            public bool FormatScope { get; set; }
            public bool CommitOnSpace { get; set; } = false;
            public bool CommitOnEnter { get; set; } = true;
            public bool ShowCompletionOnFirstChar { get; set; } = true;
            public bool ShowCompletionOnTab { get; set; } = true;
            public bool SyntaxCheckInRepl { get; set; }
            public bool PartialArgumentNameMatch { get; set; }
            public bool EnableOutlining { get; set; }
            public bool SmartIndentByArgument { get; set; } = true;
            public RFormatOptions FormatOptions { get; set; } = new RFormatOptions();
            public ILintOptions LintOptions { get; set; }
        }

        private sealed class RSettings : IRSettings {
            public event PropertyChangedEventHandler PropertyChanged;
            public void Dispose() { }

            public YesNo ShowWorkspaceSwitchConfirmationDialog { get; set; }
            public YesNo ShowSaveOnResetConfirmationDialog { get; set; }
            public bool AlwaysSaveHistory { get; set; }
            public bool ClearFilterOnAddHistory { get; set; }
            public bool MultilineHistorySelection { get; set; }
            public ConnectionInfo[] Connections { get; set; }
            public ConnectionInfo LastActiveConnection { get; set; }
            public string CranMirror { get; set; }
            public string WorkingDirectory { get; set; }
            public bool ShowPackageManagerDisclaimer { get; set; }
            public HelpBrowserType HelpBrowserType { get; set; }
            public int RCodePage { get; set; }
            public bool EvaluateActiveBindings { get; set; }
            public bool ShowDotPrefixedVariables { get; set; }
            public LogVerbosity LogVerbosity { get; set; }

            public void LoadSettings() { }
            public Task SaveSettingsAsync() => Task.CompletedTask;

            public YesNoAsk LoadRDataOnProjectLoad { get; set; }
            public YesNoAsk SaveRDataOnProjectUnload { get; set; }
            public IEnumerable<string> WorkingDirectoryList { get; set; }
            public SurveyNewsPolicy SurveyNewsCheck { get; set; }
            public DateTime SurveyNewsLastCheck { get; set; }
            public string SurveyNewsFeedUrl { get; set; }
            public string SurveyNewsIndexUrl { get; set; }
            public string WebHelpSearchString { get; set; }
            public BrowserType WebHelpSearchBrowserType { get; set; }
            public BrowserType HtmlBrowserType { get; set; }
            public BrowserType MarkdownBrowserType { get; set; }
            public bool ShowRToolbar { get; set; }
            public bool ShowHostLoadMeter { get; set; }
            public bool GridDynamicEvaluation { get; set; }
        }
    }
}
