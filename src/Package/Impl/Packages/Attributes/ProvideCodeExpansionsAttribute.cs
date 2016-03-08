// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Packages {
    /// <summary>
    /// This attribute registers code snippets for a package.  The attributes on a 
    /// package do not control the behavior of the package, but they can be used by registration 
    /// tools to register the proper information with Visual Studio.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    // Disable the "IdentifiersShouldNotHaveIncorrectSuffix" warning.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711")]
    sealed class ProvideCodeExpansionsAttribute : RegistrationAttribute {
        private readonly Guid _languageGuid;

        /// <summary>
        /// Creates a new RegisterSnippetsAttribute.
        /// </summary>
        public ProvideCodeExpansionsAttribute(string languageGuid, bool showRoots, short displayName,
                                          string languageStringId, string indexPath) {
            _languageGuid = new Guid(languageGuid);
            ShowRoots = showRoots;
            DisplayName = displayName;
            LanguageStringId = languageStringId;
            IndexPath = indexPath;
        }

        /// <summary>
        /// Returns the language guid.
        /// </summary>
        public string LanguageGuid {
            get { return _languageGuid.ToString("B"); }
        }

        /// <summary>
        /// Returns true if roots are shown.
        /// </summary>
        public bool ShowRoots { get; }

        /// <summary>
        /// Returns string ID corresponding to the language name.
        /// </summary>
        public short DisplayName { get; }

        /// <summary>
        /// Returns the string to use for the language name.
        /// </summary>
        public string LanguageStringId { get; }

        /// <summary>
        /// Returns the relative path to the snippet index file.
        /// </summary>
        public string IndexPath { get; }

        /// <summary>
        /// The reg key name of the project.
        /// </summary>
        private string LanguageName() {
            return Invariant($"Languages\\CodeExpansions\\{LanguageStringId}");
        }

        /// <summary>
        /// Called to register this attribute with the given context.
        /// </summary>
        /// <param name="context">
        /// Contains the location where the registration information should be placed.
        /// It also contains other informations as the type being registered and path information.
        /// </param>
        public override void Register(RegistrationContext context) {
            if (context == null) {
                return;
            }
            using (Key childKey = context.CreateKey(LanguageName())) {
                childKey.SetValue("", LanguageGuid);

                string snippetIndexPath = context.ComponentPath;
                snippetIndexPath = Path.Combine(snippetIndexPath, IndexPath);
                snippetIndexPath = context.EscapePath(System.IO.Path.GetFullPath(snippetIndexPath));

                childKey.SetValue("DisplayName", DisplayName.ToString(CultureInfo.InvariantCulture));
                childKey.SetValue("IndexPath", snippetIndexPath);
                childKey.SetValue("LangStringId", LanguageStringId.ToLowerInvariant());
                childKey.SetValue("Package", context.ComponentType.GUID.ToString("B"));
                childKey.SetValue("ShowRoots", ShowRoots ? 1 : 0);

                //The following enables VS to look into a user directory for more user-created snippets
                string myDocumentsPath = @"%MyDocs%\Code Snippets\" + LanguageStringId + @"\My Code Snippets\";
                using (Key forceSubKey = childKey.CreateSubkey("ForceCreateDirs")) {
                    forceSubKey.SetValue(LanguageStringId, myDocumentsPath);
                }

                using (Key pathsSubKey = childKey.CreateSubkey("Paths")) {
                    pathsSubKey.SetValue(LanguageStringId, myDocumentsPath);
                }
            }
        }

        /// <summary>
        /// Called to unregister this attribute with the given context.
        /// </summary>
        /// <param name="context">
        /// Contains the location where the registration information should be placed.
        /// It also contains other informations as the type being registered and path information.
        /// </param>
        public override void Unregister(RegistrationContext context) {
            if (context != null) {
                context.RemoveKey(LanguageName());
            }
        }
    }
}
