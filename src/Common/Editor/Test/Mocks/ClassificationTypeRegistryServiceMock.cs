using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    public class ClassificationTypeRegistryServiceMock : IClassificationTypeRegistryService
    {
        #region IClassificationTypeRegistryService Members

        public IClassificationType CreateClassificationType(string type, IEnumerable<IClassificationType> baseTypes)
        {
            return new ClassificationTypeMock(type, baseTypes);
        }

        public IClassificationType CreateTransientClassificationType(params IClassificationType[] baseTypes)
        {
            throw new NotImplementedException();
        }

        public IClassificationType CreateTransientClassificationType(IEnumerable<IClassificationType> baseTypes)
        {
            throw new NotImplementedException();
        }

        public IClassificationType GetClassificationType(string type)
        {
            return new ClassificationTypeMock(type, new List<IClassificationType>());
        }

        #endregion
    }
}
