// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Collection of language blocks (spans) in the file. Represents ranges
    /// that belong to different languages in the file. Example: HTML/CSS/Script 
    /// in Web scenarios or Markdown/R in R Markdown files.
    /// </summary>
    public sealed class LanguageBlockCollection : IEnumerable<LanguageBlock> {
        /// <summary>
        /// List of secondary language blocks in the file
        /// </summary>
        private TextRangeCollection<LanguageBlock> _blockList = new TextRangeCollection<LanguageBlock>();

        /// <summary>
        /// Index of the most recently accesses language block
        /// </summary>
        private int _lastBlockIndex = -1;

        /// <summary>
        /// Fires when language block is added to the collection
        /// </summary>
        public event EventHandler<LanguageBlockCollectionEventArgs> BlockAdded;

        /// <summary>
        /// Fires when language block is removed from the collection
        /// </summary>
        public event EventHandler<LanguageBlockCollectionEventArgs> BlockRemoved;

        /// <summary>
        /// Fires when all blocks were removed from the collection
        /// </summary>
        public event EventHandler<EventArgs> Cleared;

        /// <summary>
        /// Number of blocks in the collection
        /// </summary>
        public int Count => _blockList.Count;

        /// <summary>
        /// Updates collection reflecting change made to the source text buffer
        /// </summary>
        internal void ReflectTextChange(int start, int oldLength, int newLength) {
            var removedBlocks = _blockList.ReflectTextChange(start, oldLength, newLength);
            if (removedBlocks.Count > 0) {
                ClearCache();
                foreach (var block in removedBlocks) {
                    BlockRemoved?.Invoke(this, new LanguageBlockCollectionEventArgs(block));
                }
            }
        }

        /// <summary>
        /// Remove all language blocks from the collection
        /// </summary>
        public void Clear() {
            _blockList.Clear();
            Cleared?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds language block to the collection
        /// </summary>
        /// <param name="block">Language block</param>
        public void AddBlock(LanguageBlock block) {
            ClearCache();
            _blockList.Add(block);
            BlockAdded?.Invoke(this, new LanguageBlockCollectionEventArgs(block));
        }

        public void RemoveBlockAt(int index) {
            ClearCache();

            var block = _blockList[index];
            _blockList.RemoveAt(index);
            BlockRemoved?.Invoke(this, new LanguageBlockCollectionEventArgs(block));
        }

        public void RemoveBlock(LanguageBlock block) {
            for (int i = 0; i < _blockList.Count; i++) {
                if (_blockList[i] == block) {
                    RemoveBlockAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Retrieves language block by index
        /// </summary>
        /// <param name="index">Index into the collection</param>
        /// <returns>Language block</returns>
        public LanguageBlock this[int index] => _blockList[index];

        /// <summary>
        /// Locates language block at a given position in the document text buffer
        /// </summary>
        /// <param name="position">Position in the text buffer</param>
        /// <returns>Language block</returns>
        public LanguageBlock GetAtPosition(int position) {
            int index = GetIndexAtPosition(position);
            if (index < 0)
                return null;

            return _blockList[index];
        }

        private int GetIndexAtPosition(int position) {
            if (_lastBlockIndex >= 0 && _blockList.Count > 0) {
                if (_blockList[_lastBlockIndex].Contains(position))
                    return _lastBlockIndex;
            }

            _lastBlockIndex = -1;

            if (_blockList.Count > 0) {
                int min = 0;
                int max = Count - 1;

                if (position >= _blockList[0].Start) {
                    while (min <= max) {
                        int mid = min + (max - min) / 2;

                        if (_blockList[mid].Contains(position)) {
                            _lastBlockIndex = mid;
                            break;
                        } else if (mid < Count - 1 &&
                                  this[mid].End <= position && position < this[mid + 1].Start) {
                            _lastBlockIndex = -1;
                            break;
                        }

                        if (position < this[mid].Start) {
                            max = mid - 1;
                        } else {
                            min = mid + 1;
                        }
                    }
                }
            }

            return _lastBlockIndex;
        }


        /// <summary>
        /// Removes language blocks at given positions
        /// </summary>
        /// <param name="positions">Array of positions in a text buffer</param>
        public void RemoveAtPositions(int[] positions) {
            for (int i = 0; i < positions.Length; i++) {
                int index = GetIndexAtPosition(positions[i]);

                if (index >= 0)
                    RemoveBlockAt(index);
            }
            ClearCache();
        }

        public void SortByPosition() {
            _blockList.Sort();
            ClearCache();
        }

        private void ClearCache() {
            _lastBlockIndex = -1;
        }

        #region IEnumerable<LanguageBlock> Members
        public IEnumerator<LanguageBlock> GetEnumerator() {
            return _blockList.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator() {
            return _blockList.GetEnumerator();
        }
        #endregion
    }
}
