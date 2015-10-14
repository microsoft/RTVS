using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Packages
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DefaultIndentAttribute : RegistrationAttribute
    {
        private string _language;
        private int _indentSize;
        private bool _keepSpaces;

        public DefaultIndentAttribute(string language, int indentSize, bool keepSpaces)
        {
            _language = language;
            _indentSize = indentSize;
            _keepSpaces = keepSpaces;
        }

        public override void Register(RegistrationContext context)
        {
            using (Key key = context.CreateKey(@"Text Editor\" + _language))
            {
                key.SetValue("Indent Size", _indentSize);
                key.SetValue("Tab Size", _indentSize);
                key.SetValue("Insert Tabs", _keepSpaces ? 0 : 1);
            }
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}
