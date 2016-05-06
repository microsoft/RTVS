// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Tree.Nodes {
    /// <summary>
    /// Node representing start or end tag of an element
    /// </summary>
    public class TagNode : TreeNode {
        string _name;

        public NameToken NameToken { get; protected set; }

        /// <summary>
        /// True if tag is shorthand, i.e. terminated with /&lt;
        /// </summary>
        public bool IsShorthand { get; protected set; }

        /// <summary>
        /// True if tag is closed, i.e. has closing &lt;
        /// </summary>
        public bool IsClosed { get; protected set; }

        /// <summary>
        /// Element is self-closing per schema
        /// </summary>
        public bool IsSelfClosing { get; protected set; }

        /// <summary>
        /// Node has been completed (as opposed to still waiting for tre builder to find its end tag).
        /// </summary>
        public bool IsComplete { get { return Attributes != null; } }

        /// <summary>
        /// Collection of tag attributes
        /// </summary>
        public ReadOnlyCollection<AttributeNode> Attributes { get; internal set; }

        int _start = 0;
        int _end = 0;

        public TagNode(ElementNode parent, int openAngleBracketPosition, NameToken nameToken, int maxEnd) {
            NameToken = nameToken;

            _name = nameToken.HasName() ? parent.GetText(nameToken.NameRange) : String.Empty;

            _start = openAngleBracketPosition;
            _end = maxEnd;

            IsClosed = false;
            IsShorthand = false;
        }

        #region ITextRange
        public override void Shift(int offset) {
            _start += offset;
            _end += offset;

            NameToken.Shift(offset);

            if (Attributes != null) {
                int count = Attributes.Count;
                for (int i = 0; i < count; i++) {
                    Attributes[i].Shift(offset);
                }
            }
        }

        /// <summary>
        /// Start position of the tag (position of the &lt;)
        /// </summary>
        public override int Start { get { return _start; } }

        /// <summary>
        /// End position of the tag. Either closing &gt; or start of the next element or end of file.
        /// </summary>
        public override int End { get { return _end; } }
        #endregion

        public override ITextRange NameRange { get { return NameToken != null ? NameToken.NameRange : TextRange.EmptyRange; } }

        public override ITextRange PrefixRange { get { return NameToken != null ? NameToken.PrefixRange : TextRange.EmptyRange; } }

        public override ITextRange QualifiedNameRange { get { return NameToken != null ? NameToken.QualifiedName : TextRange.EmptyRange; } }

        public override ITextRange InnerRange { get { return TextRange.FromBounds(QualifiedNameRange.Start, this.End - ClosingSequenceLength); } }

        public int ClosingSequenceLength {
            get {
                int closingSequenceLength = IsClosed ? 1 : 0;
                if (IsShorthand)
                    closingSequenceLength++;

                return closingSequenceLength;
            }
        }

        public override bool HasPrefix() { return Prefix != null && Prefix.Length > 0; }

        public override string Name { get { return _name; } }
        public override string Prefix { get { return String.Empty; } }
        public override string QualifiedName { get { return Name; } }

        public void Complete(ReadOnlyCollection<AttributeNode> attributes, ITextRange closingSequence, bool closed, bool isShorthand, bool selfClosing) {
            IsClosed = closed;
            IsShorthand = isShorthand;
            IsSelfClosing = selfClosing;

            _end = closingSequence.End;

            Attributes = attributes;
        }

        public override void ShiftStartingFrom(int position, int offset) {
            if (NameToken.Start >= position) {
                Shift(offset);
            } else {
                _end += offset;

                if (Attributes != null) // Can be null in end tag
                {
                    int count = Attributes.Count;
                    for (int i = 0; i < count; i++) {
                        Attributes[i].ShiftStartingFrom(position, offset);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if node contains given position
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        public override bool Contains(int position) {
            if (base.Contains(position)) {
                return true;
            }

            if (position == End && !IsClosed) {
                return true;
            }

            return false;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() {
            return String.Format(
                CultureInfo.InvariantCulture, "Tag: [{0}...{1}] Shorthand: {2} Closed: {3} Attributes: {4}",
                        Start, End, IsShorthand, IsClosed, Attributes.Count);
        }
    }
}
