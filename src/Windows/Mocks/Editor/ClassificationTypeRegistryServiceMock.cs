// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class ClassificationTypeRegistryServiceMock : IClassificationTypeRegistryService
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
