using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Packages
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ShowBraceCompletionAttribute: RegistrationAttribute
    {
        private string _language;

        public ShowBraceCompletionAttribute(string language)
        {
            _language = language;
        }

        public override void Register(RegistrationContext context)
        {
            Key key = context.CreateKey(@"Languages\Language Services\" + _language);
            key.SetValue("ShowBraceCompletion", 1);
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}
