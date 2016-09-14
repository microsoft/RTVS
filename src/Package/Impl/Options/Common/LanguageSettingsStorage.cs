// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Settings;
using Microsoft.Languages.Editor.Settings;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.Common {
    /// <summary>
    /// Base class of VS settings. Provides implementation of <see cref="IWritableEditorSettingsStorage"/> for 
    /// application-agnostic editor code. This class also provides tracking of tab/indent settings via VS
    /// text manager (<seealso cref="IVsTextManager"/>) and language preferences (<seealso cref="LANGPREFERENCES"/>).
    /// </summary>
    public abstract class LanguageSettingsStorage
        : IVsTextManagerEvents4
        , IWritableSettingsStorage
        , IDisposable {

        public event EventHandler<EventArgs> SettingsChanged;

        private LANGPREFERENCES3? _langPrefs;
        private ConnectionPointCookie _textManagerEventsCookie;
        private Guid _languageServiceId;
        private Dictionary<string, bool> _booleanSettings;
        private Dictionary<string, int> _integerSettings;
        private Dictionary<string, string> _stringSettings;
        private bool _inBatchChange;
        private bool _changedDuringBatch;

        protected LanguageSettingsStorage(Guid languageServiceId) {
            _languageServiceId = languageServiceId;
            _booleanSettings = new Dictionary<string, bool>();
            _integerSettings = new Dictionary<string, int>();
            _stringSettings = new Dictionary<string, string>();

            HookTextManagerEvents();
        }

        private void HookTextManagerEvents() {
            IVsTextManager4 textManager = VsAppShell.Current.GetGlobalService<IVsTextManager4>(typeof(SVsTextManager));
            Debug.Assert(textManager != null);

            if (textManager != null) {
                // Hook into the "preferences changed" event so that I can update _langPrefs as needed
                _textManagerEventsCookie = new ConnectionPointCookie(textManager, this, typeof(IVsTextManagerEvents4));
            }
        }

        /// <summary>
        /// VS language preferences: tab size, indent size, spaces or tabs.
        /// </summary>
        protected LANGPREFERENCES3 LangPrefs {
            get {
                if (!_langPrefs.HasValue) {
                    IVsTextManager4 textManager = VsAppShell.Current.GetGlobalService<IVsTextManager4>(typeof(SVsTextManager));
                    Debug.Assert(textManager != null);

                    if (textManager != null) {
                        // Get the language preferences, like "is intellisense turned on?"
                        LANGPREFERENCES3[] langPrefs = new LANGPREFERENCES3[1];
                        langPrefs[0].guidLang = _languageServiceId;

                        int hr = textManager.GetUserPreferences4(null, langPrefs, null);
                        if (hr == VSConstants.S_OK) {
                            _langPrefs = langPrefs[0];
                        }
                    }

                    if (!_langPrefs.HasValue) {
                        Debug.Fail("Invalid language service when fetching lang prefs: " + _languageServiceId.ToString());
                        LANGPREFERENCES3 langPrefs = new LANGPREFERENCES3();
                        langPrefs.guidLang = _languageServiceId;
                        _langPrefs = langPrefs;
                    }
                }

                return _langPrefs.Value;
            }
        }

        private void SetLangPrefs(LANGPREFERENCES3 newPreferences) {
            IVsTextManager4 textManager = VsAppShell.Current.GetGlobalService<IVsTextManager4>(typeof(SVsTextManager));
            Debug.Assert(textManager != null);

            if (textManager != null) {
                // Set the language preferences, like "is intellisense turned on?"
                LANGPREFERENCES3[] langPrefs = { newPreferences };

                textManager.SetUserPreferences4(null, langPrefs, null);
            }
        }

        public int OnUserPreferencesChanged4(
            VIEWPREFERENCES3[] viewPrefs,
            LANGPREFERENCES3[] langPrefs,
            FONTCOLORPREFERENCES2[] colorPrefs) {
            if (langPrefs != null && langPrefs[0].guidLang == _languageServiceId) {
                _langPrefs = langPrefs[0];
                FireSettingsChanged();
            }

            return VSConstants.S_OK;
        }

        public void OnRegisterMarkerType(int markerType) {
        }

        public void OnRegisterView(IVsTextView view) {
        }

        public void OnUnregisterView(IVsTextView view) {
        }

        public abstract void LoadFromStorage();

        /// <summary>
        /// Called when VS resets default settings through "Tools|Import/Export Settings"
        /// </summary>
        public void ResetSettings() {
            // LangPrefs will be reset by VS, this code doesn't need to do it

            _booleanSettings.Clear();
            _integerSettings.Clear();
            _stringSettings.Clear();

            FireSettingsChanged();
        }

        public virtual string GetString(string name, string defaultValue) {
            string value;
            if (_stringSettings.TryGetValue(name, out value))
                return value;

            return defaultValue;
        }

        public virtual int GetInteger(string name, int defaultValue) {
            switch (name) {
                case CommonSettings.IndentStyleKey:
                    return (int)LangPrefs.IndentStyle;

                case CommonSettings.FormatterIndentSizeKey:
                    return (int)LangPrefs.uIndentSize;

                case CommonSettings.FormatterIndentTypeKey:
                    if (LangPrefs.fInsertTabs != 0 && LangPrefs.uTabSize != 0 && LangPrefs.uIndentSize % LangPrefs.uTabSize == 0) {
                        return (int)IndentType.Tabs;
                    }
                    return (int)IndentType.Spaces;

                case CommonSettings.FormatterTabSizeKey:
                    return (int)LangPrefs.uTabSize;
            }

            int value;
            if (_integerSettings.TryGetValue(name, out value))
                return value;

            return defaultValue;
        }

        public virtual bool GetBoolean(string name, bool defaultValue) {
            switch (name) {
                case CommonSettings.InsertMatchingBracesKey:
                    return LangPrefs.fBraceCompletion != 0;

                case CommonSettings.CompletionEnabledKey:
                    return LangPrefs.fAutoListMembers != 0;

                case CommonSettings.SignatureHelpEnabledKey:
                    return LangPrefs.fAutoListParams != 0;
            }

            bool value;
            if (_booleanSettings.TryGetValue(name, out value))
                return value;

            return defaultValue;
        }

        public void SetString(string name, string value) {
            // Not allowed to save null strings
            value = value ?? string.Empty;

            if (!_stringSettings.ContainsKey(name) || !value.Equals(_stringSettings[name], StringComparison.Ordinal)) {
                _stringSettings[name] = value;
                FireSettingsChanged();
            }
        }

        public void SetInteger(string name, int value) {
            LANGPREFERENCES3 langPrefs = LangPrefs;

            switch (name) {
                case CommonSettings.IndentStyleKey:
                    if (langPrefs.IndentStyle != (vsIndentStyle)value) {
                        langPrefs.IndentStyle = (vsIndentStyle)value;
                        SetLangPrefs(langPrefs);
                    }
                    break;

                case CommonSettings.FormatterIndentSizeKey:
                    if (langPrefs.uIndentSize != (uint)value) {
                        langPrefs.uIndentSize = (uint)value;
                        SetLangPrefs(langPrefs);
                    }
                    break;

                case CommonSettings.FormatterIndentTypeKey:
                    if (langPrefs.fInsertTabs != (uint)value) {
                        langPrefs.fInsertTabs = (uint)value;
                        SetLangPrefs(langPrefs);
                    }
                    break;

                case CommonSettings.FormatterTabSizeKey:
                    if (langPrefs.uTabSize != (uint)value) {
                        langPrefs.uTabSize = (uint)value;
                        SetLangPrefs(langPrefs);
                    }
                    break;

                default:
                    if (!_integerSettings.ContainsKey(name) || value != _integerSettings[name]) {
                        _integerSettings[name] = value;

                        // The SetLangPrefs call indirectly calls FireSettingsChanged, so this is the only code path that
                        //   needs to do this explicitly
                        FireSettingsChanged();
                    }
                    break;
            }
        }

        public void SetBoolean(string name, bool value) {
            LANGPREFERENCES3 langPrefs = LangPrefs;

            switch (name) {
                case CommonSettings.InsertMatchingBracesKey:
                    if (langPrefs.fBraceCompletion != (uint)(value ? 1 : 0)) {
                        langPrefs.fBraceCompletion = (uint)(value ? 1 : 0);
                        SetLangPrefs(langPrefs);
                    }
                    break;

                case CommonSettings.CompletionEnabledKey:
                    if (langPrefs.fAutoListMembers != (uint)(value ? 1 : 0)) {
                        langPrefs.fAutoListMembers = (uint)(value ? 1 : 0);
                        SetLangPrefs(langPrefs);
                    }
                    break;

                case CommonSettings.SignatureHelpEnabledKey:
                    if (langPrefs.fAutoListParams != (uint)(value ? 1 : 0)) {
                        langPrefs.fAutoListParams = (uint)(value ? 1 : 0);
                        SetLangPrefs(langPrefs);
                    }
                    break;

                default:
                    if (!_booleanSettings.ContainsKey(name) || value != _booleanSettings[name]) {
                        _booleanSettings[name] = value;

                        // The SetLangPrefs call indirectly calls FireSettingsChanged, so this is the only code path that
                        //   needs to do this explicitly
                        FireSettingsChanged();
                    }
                    break;
            }
        }

        public void BeginBatchChange() {
            _inBatchChange = true;
            _changedDuringBatch = false;
        }

        public void EndBatchChange() {
            _inBatchChange = false;

            if (_changedDuringBatch)
                FireSettingsChanged();

            _changedDuringBatch = false;
        }

        private void FireSettingsChanged() {
            if (_inBatchChange) {
                _changedDuringBatch = true;
            } else if (SettingsChanged != null) {
                SettingsChanged(this, EventArgs.Empty);
            }
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_textManagerEventsCookie != null) {
                    _textManagerEventsCookie.Dispose();
                    _textManagerEventsCookie = null;
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
