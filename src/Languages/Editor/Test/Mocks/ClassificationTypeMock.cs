using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    public class ClassificationTypeMock : IClassificationType
    {
        string _type;
        IEnumerable<IClassificationType> _baseTypes;

        public ClassificationTypeMock(string type, IEnumerable<IClassificationType> baseTypes)
        {
            _type = type;
            _baseTypes = baseTypes;
         }

        #region IClassificationType Members

        public IEnumerable<IClassificationType> BaseTypes
        {
            get { return new List<IClassificationType>(); }
        }

        public string Classification
        {
            get { return "mock"; }
        }

        public bool IsOfType(string type)
        {
            if (String.Compare(type, "mock", StringComparison.OrdinalIgnoreCase) == 0)
                return true;

            foreach (var t in _baseTypes)
            {
                if (t.IsOfType(type))
                    return true;
            }

            return false;
        }

        #endregion
    }
}
