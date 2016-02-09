using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.VisualStudio.R.Package.Snippets.Definitions;

namespace Microsoft.VisualStudio.R.Package.Snippets {
    internal sealed class SnippetInfo : IComparable, ISnippetInfo {
        public SnippetInfo() {
            ShouldFormat = true;
            ProjectTypeGuids = new HashSet<Guid>();
            FileExtensions = new HashSet<string>();
        }

        public SnippetInfo(SnippetInfo snippetInfo) {
            this.CopyFrom(snippetInfo);
        }

        public void CopyFrom(SnippetInfo snippetInfo) {
            this.Path = snippetInfo.Path;
            this.Title = snippetInfo.Title;
            this.Shortcut = snippetInfo.Shortcut;
            this.Value = snippetInfo.Value;
            this.Description = snippetInfo.Description;
            this.Kind = snippetInfo.Kind;
            this.Key = snippetInfo.Key;
            this.ShouldFormat = snippetInfo.ShouldFormat;
            this.IsGeneric = snippetInfo.IsGeneric;
            this.XmlDocument = snippetInfo.XmlDocument;

            this.ProjectTypeGuids = new HashSet<Guid>(snippetInfo.ProjectTypeGuids);
            this.FileExtensions = new HashSet<string>(snippetInfo.FileExtensions);
        }

        public string Title { get; internal set; }
        public string Description { get; internal set; }
        public string Key { get; internal set; }
        public string Kind { get; internal set; }
        public string Value { get; internal set; }
        public string Path { get; internal set; }
        public string Shortcut { get; internal set; }
        public bool ShouldFormat { get; internal set; }
        public bool IsGeneric { get; internal set; }
        public HashSet<Guid> ProjectTypeGuids { get; internal set; }
        public HashSet<string> FileExtensions { get; internal set; }
        public XmlDocument XmlDocument { get; internal set; }

        public static int Compare(SnippetInfo lhs, SnippetInfo rhs) {
            if (object.ReferenceEquals(lhs, null) &&
                object.ReferenceEquals(rhs, null)) {
                return 0;
            }

            if (!object.ReferenceEquals(lhs, null) &&
                !object.ReferenceEquals(rhs, null)) {
                return String.Compare(lhs.Shortcut, rhs.Shortcut, StringComparison.OrdinalIgnoreCase);
            }

            return object.ReferenceEquals(lhs, null) ? -1 : 1;
        }

        public int CompareTo(object obj) {
            return Compare(this, obj as SnippetInfo);
        }

        public override bool Equals(object obj) {
            return Compare(this, obj as SnippetInfo) == 0;
        }

        public static bool operator ==(SnippetInfo left, SnippetInfo right) {
            return Compare(left, right) == 0;
        }

        public static bool operator !=(SnippetInfo left, SnippetInfo right) {
            return Compare(left, right) != 0;
        }

        public static bool operator <(SnippetInfo left, SnippetInfo right) {
            return Compare(left, right) < 0;
        }

        public static bool operator >(SnippetInfo left, SnippetInfo right) {
            return Compare(left, right) > 0;
        }

        public override int GetHashCode() {
            return Shortcut != null ? Shortcut.GetHashCode() : 0;
        }
    }
}
