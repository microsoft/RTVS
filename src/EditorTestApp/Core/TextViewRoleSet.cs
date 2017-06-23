// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Application.Core {
    [ExcludeFromCodeCoverage]
    internal class TextViewRoleSet : ITextViewRoleSet {
        private readonly HashSet<string> _roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public TextViewRoleSet(params IEnumerable<string>[] roleSet) {
            if (roleSet != null) {
                foreach (var t in roleSet) {
                    foreach (var role in t) {
                        // no need to check before adding to HashSet
                        _roles.Add(role);
                    }
                }
            }
        }

        #region ITextViewRoleSet Members
        public bool Contains(string textViewRole) => _roles.Contains(textViewRole);
        public bool ContainsAll(IEnumerable<string> textViewRoles) => _roles.IsSupersetOf(textViewRoles);
        public bool ContainsAny(IEnumerable<string> textViewRoles) => _roles.Overlaps(textViewRoles);
        public ITextViewRoleSet UnionWith(ITextViewRoleSet roleSet) => new TextViewRoleSet(this, roleSet);
        #endregion

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator() {
            return _roles.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _roles.GetEnumerator();
        }

        #endregion
    }
}
