using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Application.Core
{
    [ExcludeFromCodeCoverage]
    internal class TextViewRoleSet : ITextViewRoleSet
    {
        private HashSet<string> _roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public TextViewRoleSet(params IEnumerable<string>[] roleSet)
        {
            if (roleSet != null)
            {
                for (int i = 0; i < roleSet.Length; i++)
                {
                    foreach (var role in roleSet[i])
                    {
                        // no need to check before adding to HashSet
                        _roles.Add(role);
                    }
                }
            }
        }

        #region ITextViewRoleSet Members

        public bool Contains(string textViewRole)
        {
            return _roles.Contains(textViewRole);
        }

        public bool ContainsAll(IEnumerable<string> textViewRoles)
        {
            return _roles.IsSupersetOf(textViewRoles);
        }

        public bool ContainsAny(IEnumerable<string> textViewRoles)
        {
            return _roles.Overlaps(textViewRoles);
        }

        public ITextViewRoleSet UnionWith(ITextViewRoleSet roleSet)
        {
            return new TextViewRoleSet(this, roleSet);
        }

        #endregion

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return _roles.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _roles.GetEnumerator();
        }

        #endregion
    }
}
