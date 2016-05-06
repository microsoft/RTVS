// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Tree.Nodes {
    public enum HtmlCollectionEventType {
        NodeAdded,
        NodeRemoved,
        NodeReplaced
    }

    [ExcludeFromCodeCoverage]
    public class HtmlNodeCollectionEventArgs : EventArgs {
        public HtmlCollectionEventType EventType { get; private set; }
        public TreeNode AddedNode { get; private set; }
        public TreeNode RemovedNode { get; private set; }

        public HtmlNodeCollectionEventArgs(TreeNode added) :
            this(HtmlCollectionEventType.NodeAdded, added, null) {
        }

        public HtmlNodeCollectionEventArgs(HtmlCollectionEventType eventType, TreeNode added, TreeNode removed) {
            EventType = eventType;
            AddedNode = added;
            RemovedNode = removed;
        }
    }

    public class NodeCollection<T> : TextRangeCollection<T> where T : TreeNode {
        #region Events
        public event EventHandler<HtmlNodeCollectionEventArgs> ItemAdded;
        public event EventHandler<HtmlNodeCollectionEventArgs> ItemRemoved;
        #endregion

        #region Add
        public override void Add(T item) {
            base.Add(item);

            if (ItemAdded != null)
                ItemAdded(this, new HtmlNodeCollectionEventArgs(item as TreeNode));
        }
        #endregion

        #region Item manipulation
        public override void RemoveAt(int index) {
            T node = this[index];

            Items.RemoveAt(index);

            if (ItemRemoved != null)
                ItemRemoved(this, new HtmlNodeCollectionEventArgs(node));

        }
        #endregion

        public override void ShiftStartingFrom(int position, int offset) {
            if (Count > 0 && position >= this[Count - 1].End) {
                // Beyond the last element. Check if element is not closed
                // and if yes, we need to expand its virtual range

                ElementNode node = this[Count - 1] as ElementNode;
                if (node != null && !node.IsShorthand() && node.EndTag == null) {
                    node.ShiftStartingFrom(position, offset);
                }
            } else {
                base.ShiftStartingFrom(position, offset);
            }
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() {
            return String.Format(CultureInfo.InvariantCulture, "Count: {0}", Count);
        }
    }
}
