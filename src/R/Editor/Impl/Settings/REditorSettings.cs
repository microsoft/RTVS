using System;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.ContentType;

namespace Microsoft.R.Editor.Settings
{
    public static class REditorSettings
    {
        public const string FormatOnPasteKey = "FormatOnPaste";

        private static bool _initialized = false;

        private static IEditorSettingsStorage Storage
        {
            get
            {
                var storage = EditorShell.GetSettings(RContentTypeDefinition.LanguageName) as IEditorSettingsStorage;

                if (!_initialized)
                {
                    storage.SettingsChanged += OnSettingsChanged;
                    _initialized = true;
                }

                return storage;
            }
        }

        private static IWritableEditorSettingsStorage WritableStorage
        {
            get { return Storage as IWritableEditorSettingsStorage; }
        }

        private static bool IsWritable
        {
            get { return Storage is IWritableEditorSettingsStorage; }
        }

        public static event EventHandler<EventArgs> Changed;

        public static void BeginBatchChange()
        {
            if (IsWritable)
                WritableStorage.BeginBatchChange();
        }

        public static void EndBatchChange()
        {
            if (IsWritable)
                WritableStorage.EndBatchChange();
        }

        public static void ResetSettings()
        {
            if (IsWritable)
                WritableStorage.ResetSettings();
        }

        private static void OnSettingsChanged(object sender, EventArgs e)
        {
            if (Changed != null)
                Changed(null, EventArgs.Empty);
        }

        public static bool CompletionEnabled
        {
            get
            {
                return CommonSettings.GetCompletionEnabled(Storage);
            }
        }

        public static bool SignatureHelpEnabled
        {
            get
            {
                return CommonSettings.GetSignatureHelpEnabled(Storage);
            }
        }

        public static bool ValidationEnabled
        {
            get
            {
                return CommonSettings.GetValidationEnabled(Storage);
            }

            set
            {
                if (IsWritable)
                    WritableStorage.SetBoolean(CommonSettings.ValidationEnabledKey, value);
            }
        }

        public static bool InsertMatchingBraces
        {
            get
            {
                return Storage.GetBoolean(CommonSettings.InsertMatchingBracesKey, true);
            }

            set
            {
                if (IsWritable)
                    WritableStorage.SetBoolean(CommonSettings.InsertMatchingBracesKey, value);
            }
        }

        public static bool FormatOnPaste
        {
            get
            {
                return Storage.GetBoolean(FormatOnPasteKey, true);
            }

            set
            {
                if (IsWritable)
                    WritableStorage.SetBoolean(FormatOnPasteKey, value);
            }
        }

        public static IndentType IndentType
        {
            get
            {
                return (IndentType)Storage.GetInteger(CommonSettings.FormatterIndentTypeKey, (int)IndentType.Spaces);
            }

            set
            {
                if (IsWritable)
                    WritableStorage.SetInteger(CommonSettings.FormatterIndentTypeKey, (int)value);
            }
        }

        public static int IndentSize
        {
            get
            {
                return Storage.GetInteger(CommonSettings.FormatterIndentSizeKey, 4);
            }

            set
            {
                if (IsWritable)
                    WritableStorage.SetInteger(CommonSettings.FormatterIndentSizeKey, value);
            }
        }

        public static IndentStyle IndentStyle
        {
            get
            {
                return (IndentStyle)Storage.GetInteger(CommonSettings.IndentStyleKey, (int)IndentStyle.Smart);
            }

            set
            {
                if (IsWritable)
                    WritableStorage.SetInteger(CommonSettings.IndentStyleKey, (int)value);
            }
        }

        public static int TabSize
        {
            get
            {
                return Storage.GetInteger(CommonSettings.FormatterTabSizeKey, 4);
            }

            set
            {
                if (IsWritable)
                    WritableStorage.SetInteger(CommonSettings.FormatterTabSizeKey, value);
            }
        }
    }
}
