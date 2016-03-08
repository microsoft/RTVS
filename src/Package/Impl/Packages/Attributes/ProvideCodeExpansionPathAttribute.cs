// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.VisualStudio.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Packages {

    /// <summary>
    /// This attribute registers an additional path for code snippets to live
    /// in for a particular language.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    // Disable the "IdentifiersShouldNotHaveIncorrectSuffix" warning.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711")]
    sealed class ProvideCodeExpansionPathAttribute : RegistrationAttribute {
        private readonly string _description;

        /// <summary>
        /// Creates a new RegisterSnippetsAttribute.
        /// </summary>
        public ProvideCodeExpansionPathAttribute(string languageStringId, string description, string paths) {
            LanguageStringId = languageStringId;
            _description = description;
            Paths = paths;
        }

        /// <summary>
        /// Returns the string to use for the language name.
        /// </summary>
        public string LanguageStringId { get; }
 
        /// <summary>
        /// Returns the paths to look for snippets.
        /// </summary>
        public string Paths { get; }

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
            using (Key childKey = context.CreateKey(LanguageName())) {
                string snippetPaths = context.ComponentPath;
                snippetPaths = Path.Combine(snippetPaths, Paths);
                snippetPaths = context.EscapePath(Path.GetFullPath(snippetPaths));

                using (Key pathsSubKey = childKey.CreateSubkey("Paths")) {
                    pathsSubKey.SetValue(_description, snippetPaths);
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
        }
    }
}