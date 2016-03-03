// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Packages {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ShowBraceCompletionAttribute : RegistrationAttribute {
        private string _language;

        public ShowBraceCompletionAttribute(string language) {
            _language = language;
        }

        public override void Register(RegistrationContext context) {
            using (Key key = context.CreateKey(@"Languages\Language Services\" + _language)) {
                key.SetValue("ShowBraceCompletion", 1);
            }

            using (Key key = context.CreateKey(@"Text Editor\" + _language)) {
                key.SetValue("Brace Completion", 1);
            }
        }

        public override void Unregister(RegistrationContext context) { }
    }
}
