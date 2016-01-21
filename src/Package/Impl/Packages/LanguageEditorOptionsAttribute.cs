using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Packages {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class LanguageEditorOptionsAttribute : RegistrationAttribute {
        private string _language;
        private int _indentSize;
        private bool _keepSpaces;
        private bool _showLineNumbers;

        public LanguageEditorOptionsAttribute(string language, int indentSize, bool keepSpaces, bool showLineNumbers) {
            _language = language;
            _indentSize = indentSize;
            _keepSpaces = keepSpaces;
            _showLineNumbers = showLineNumbers;
        }

        public override void Register(RegistrationContext context) {
            using (Key key = context.CreateKey(@"Text Editor\" + _language)) {
                key.SetValue("Indent Size", _indentSize);
                key.SetValue("Tab Size", _indentSize);
                key.SetValue("Insert Tabs", _keepSpaces ? 0 : 1);
                key.SetValue("Line Numbers", _showLineNumbers ? 1 : 0);
            }
        }

        public override void Unregister(RegistrationContext context) { }
    }
}
