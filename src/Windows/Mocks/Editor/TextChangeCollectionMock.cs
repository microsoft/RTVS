// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class TextChangeCollectionMock : INormalizedTextChangeCollection
    {
        TextChangeMock _change;

        public TextChangeCollectionMock(TextChangeMock change)
        {
            _change = change;
        }

        #region INormalizedTextChangeCollection Members

        public bool IncludesLineChanges
        {
            get { return true; }
        }

        #endregion

        #region IList<ITextChange> Members

        public int IndexOf(ITextChange item)
        {
            return 0;
        }

        public void Insert(int index, ITextChange item)
        {
        }

        public void RemoveAt(int index)
        {
        }

        public ITextChange this[int index]
        {
            get
            {
                return _change;
            }
            set
            {
            }
        }

        #endregion

        #region ICollection<ITextChange> Members

        public void Add(ITextChange item)
        {
         }

        public void Clear()
        {
        }

        public bool Contains(ITextChange item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ITextChange[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return 1; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(ITextChange item)
        {
            return false;
        }

        #endregion

        #region IEnumerable<ITextChange> Members

        public IEnumerator<ITextChange> GetEnumerator()
        {
            return new List<ITextChange>() { _change}.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new List<ITextChange>() { _change }.GetEnumerator();
        }

        #endregion
    }
}
