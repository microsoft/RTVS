// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.DataInspection;

namespace Microsoft.R.Editor.Data {
    public interface IRSessionDataObject {
        string Name { get; }

        string Value { get; }

        string TypeName { get; }

        string Class { get; }

        bool HasChildren { get; }

        IReadOnlyList<long> Dimensions { get; }

        bool IsHidden { get; }

        string Expression { get; }

        Task<IReadOnlyList<IRSessionDataObject>> GetChildrenAsync();

        IREvaluationResultInfo DebugEvaluation { get; }
    }
}
