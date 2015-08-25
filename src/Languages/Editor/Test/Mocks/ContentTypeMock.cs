using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    [ExcludeFromCodeCoverage]
    public class ContentTypeMock : IContentType
    {
        private static IContentType _textContentType;
        public static IContentType TextContentType
        {
            get
            {
                if (_textContentType == null)
                    _textContentType = new ContentTypeMock("text");

                return _textContentType;
            }
        }

        public ContentTypeMock(string contentTypeName)
            : this(contentTypeName, Enumerable.Empty<IContentType>())
        {
        }

        public ContentTypeMock(string contentTypeName, IEnumerable<IContentType> baseContentTypes)
        {
            TypeName = contentTypeName;
            BaseTypes = baseContentTypes;
        }

        #region IContentType Members

        public IEnumerable<IContentType> BaseTypes
        {
            get;
            private set;
        }

        public string DisplayName
        {
            get
            {
                return TypeName;
            }
        }

        public bool IsOfType(string type)
        {
            if (String.Equals(type, TypeName, StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (IContentType baseType in BaseTypes)
            {
                if (baseType.IsOfType(type))
                    return true;
            }

            return false;
        }

        public string TypeName
        {
            get;
            private set;
        }

        #endregion
    }
}
