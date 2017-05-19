// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Editor.Settings;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Editors {
    /// <summary>
    /// Base class of VS settings. Provides implementation of <see cref="IWritableEditorSettingsStorage"/> for 
    /// application-agnostic editor code. This class also provides tracking of tab/indent settings via VS
    /// text manager (<seealso cref="IVsTextManager"/>) and language preferences (<seealso cref="LANGPREFERENCES"/>).
    /// </summary>
    public sealed class LanguageSettingsStorage : IVsTextManagerEvents4, IWritableEditorSettingsStorage, IDisposable {
        private readonly Guid _languageServiceId;
        private readonly Dictionary<string, bool> _booleanSettings = new Dictionary<string, bool>();
        private readonly Dictionary<string, int> _integerSettings = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _stringSettings = new Dictionary<string, string>();
        private readonly IVsTextManager4 _textManager;
        private readonly IEnumerable<string> _automationObjectNames;
        private readonly ICoreShell _shell;

        private Guid _packageGuid;
        private ConnectionPointCookie _textManagerEventsCookie;
        private LANGPREFERENCES3? _langPrefs;

        public LanguageSettingsStorage(ICoreShell coreShell, Guid languageServiceId, Guid packageId, IEnumerable<string> automationObjectNames) {
            _shell = coreShell;
            _languageServiceId = languageServiceId;
            _packageGuid = packageId;
            _automationObjectNames = automationObjectNames;

            _textManager = _shell.GetService<IVsTextManager4>(typeof(SVsTextManager));
            _textManagerEventsCookie = new ConnectionPointCookie(_textManager, this, typeof(IVsTextManagerEvents4));

            LoadFromStorage();
        }

        public event EventHandler<EventArgs> SettingsChanged;

        /// <summary>
        /// VS language preferences: tab size, indent size, spaces or tabs.
        /// </summary>
        private LANGPREFERENCES3 LangPrefs => _langPrefs.Value;

        private void SetLangPrefs(LANGPREFERENCES3 newPreferences)
            => _textManager.SetUserPreferences4(null, new[] { newPreferences }, null);

        #region IVsTextManagerEvents4
        public int OnUserPreferencesChanged4(VIEWPREFERENCES3[] viewPrefs, LANGPREFERENCES3[] langPrefs, FONTCOLORPREFERENCES2[] colorPrefs) {
            if (langPrefs != null && langPrefs[0].guidLang == _languageServiceId) {
                _langPrefs = langPrefs[0];
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }

            return VSConstants.S_OK;
        }
        #endregion

        /// <summary>
        /// Loads settings via language (editor) tools options page
        /// </summary>
        public void LoadFromStorage() {
            var vsShell = _shell.GetService<IVsShell>(typeof(SVsShell));
            if (vsShell != null) {
                vsShell.LoadPackage(ref _packageGuid, out IVsPackage package);
                if (package != null) {
                    foreach (var curAutomationObjectName in _automationObjectNames) {
                        package.GetAutomationObject(curAutomationObjectName, out object automationObject);
                    }
                }
                LoadLanguagePreferences();
            }
        }

        private void LoadLanguagePreferences() {
            var langPrefs = new LANGPREFERENCES3[1];
            langPrefs[0].guidLang = _languageServiceId;

            var hr = _textManager.GetUserPreferences4(null, langPrefs, null);
            if (hr == VSConstants.S_OK) {
                _langPrefs = langPrefs[0];
            }
            Debug.Assert(_langPrefs != null);
        }

        /// <summary>
        /// Called when VS resets default settings through "Tools|Import/Export Settings"
        /// </summary>
        public void ResetSettings() {
            // LangPrefs will be reset by VS, this code doesn't need to do it

            _booleanSettings.Clear();
            _integerSettings.Clear();
            _stringSettings.Clear();

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        #region IEditorSettingsStorage
        public T Get<T>(string name, T defaultValue) {
            if (defaultValue is string) {
                return (T)(object)GetString(name, (string)(object)defaultValue);
            }
            if (defaultValue is int) {
                return (T)(object)GetInteger(name, Convert.ToInt32(defaultValue));
            }
            if (defaultValue is bool) {
                return (T)(object)GetBoolean(name, Convert.ToBoolean(defaultValue));
            }
            Debug.Fail("Unknown editor setting type");
            return defaultValue;
        }

        private string GetString(string name, string defaultValue) {
            string value;
            return _stringSettings.TryGetValue(name, out value) ? value : defaultValue;
        }

        private int GetInteger(string name, int defaultValue) {
            switch (name) {
                case EditorSettings.IndentStyleKey:
                    return (int)LangPrefs.IndentStyle;

                case EditorSettings.FormatterIndentSizeKey:
                    return (int)LangPrefs.uIndentSize;

                case EditorSettings.FormatterIndentTypeKey:
                    if (LangPrefs.fInsertTabs != 0 && LangPrefs.uTabSize != 0 && LangPrefs.uIndentSize % LangPrefs.uTabSize == 0) {
                        return (int)IndentType.Tabs;
                    }
                    return (int)IndentType.Spaces;

                case EditorSettings.FormatterTabSizeKey:
                    return (int)LangPrefs.uTabSize;
            }

            int value;
            return _integerSettings.TryGetValue(name, out value) ? value : defaultValue;
        }

        private bool GetBoolean(string name, bool defaultValue) {
            switch (name) {
                case EditorSettings.InsertMatchingBracesKey:
                    return LangPrefs.fBraceCompletion != 0;

                case EditorSettings.CompletionEnabledKey:
                    return LangPrefs.fAutoListMembers != 0;

                case EditorSettings.SignatureHelpEnabledKey:
                    return LangPrefs.fAutoListParams != 0;
            }

            bool value;
            return _booleanSettings.TryGetValue(name, out value) ? value : defaultValue;
        }
        #endregion

        #region IWritableEditorSettingsStorage
        public void Set<T>(string name, T value) {
            if (value is string) {
                SetString(name, (string)(object)value);
            } else if (value is int) {
                SetInteger(name, Convert.ToInt32(value));
            } else if (value is bool) {
                SetBoolean(name, Convert.ToBoolean(value));
            } else {
                Debug.Fail("Unknown editor setting type");
            }
        }

        private void SetString(string name, string value) {
            // Not allowed to save null strings
            value = value ?? string.Empty;

            if (!_stringSettings.ContainsKey(name) || !value.Equals(_stringSettings[name], StringComparison.Ordinal)) {
                _stringSettings[name] = value;
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetInteger(string name, int value) {
            LANGPREFERENCES3 langPrefs = LangPrefs;

            switch (name) {
                case EditorSettings.IndentStyleKey:
                    if (langPrefs.IndentStyle != (vsIndentStyle)value) {
                        langPrefs.IndentStyle = (vsIndentStyle)value;
                        SetLangPrefs(langPrefs);
                    }
                    break;

                case EditorSettings.FormatterIndentSizeKey:
                    if (langPrefs.uIndentSize != (uint)value) {
                        langPrefs.uIndentSize = (uint)value;
                        SetLangPrefs(langPrefs);
                    }
                    break;

                case EditorSettings.FormatterIndentTypeKey:
                    if (langPrefs.fInsertTabs != (uint)value) {
                        langPrefs.fInsertTabs = (uint)value;
                        SetLangPrefs(langPrefs);
                    }
                    break;

                case EditorSettings.FormatterTabSizeKey:
                    if (langPrefs.uTabSize != (uint)value) {
                        langPrefs.uTabSize = (uint)value;
                        SetLangPrefs(langPrefs);
                    }
                    break;

                default:
                    if (!_integerSettings.ContainsKey(name) || value != _integerSettings[name]) {
                        _integerSettings[name] = value;
                        SettingsChanged?.Invoke(this, EventArgs.Empty);
                    }
                    break;
            }
        }

        private void SetBoolean(string name, bool value) {
            var langPrefs = LangPrefs;

            switch (name) {
                case EditorSettings.InsertMatchingBracesKey:
                    if (langPrefs.fBraceCompletion != (uint)(value ? 1 : 0)) {
                        langPrefs.fBraceCompletion = (uint)(value ? 1 : 0);
                        SetLangPrefs(langPrefs);
                    }
                    break;

                case EditorSettings.CompletionEnabledKey:
                    if (langPrefs.fAutoListMembers != (uint)(value ? 1 : 0)) {
                        langPrefs.fAutoListMembers = (uint)(value ? 1 : 0);
                        SetLangPrefs(langPrefs);
                    }
                    break;

                case EditorSettings.SignatureHelpEnabledKey:
                    if (langPrefs.fAutoListParams != (uint)(value ? 1 : 0)) {
                        langPrefs.fAutoListParams = (uint)(value ? 1 : 0);
                        SetLangPrefs(langPrefs);
                    }
                    break;

                default:
                    if (!_booleanSettings.ContainsKey(name) || value != _booleanSettings[name]) {
                        _booleanSettings[name] = value;
                        SettingsChanged?.Invoke(this, EventArgs.Empty);
                    }
                    break;
            }
        }
        #endregion

        #region IDisposable
        public void Dispose() {
            if (_textManagerEventsCookie != null) {
                _textManagerEventsCookie.Dispose();
                _textManagerEventsCookie = null;
            }
        }
        #endregion
    }
}
