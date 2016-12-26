// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.DataInspection;
using static System.FormattableString;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.R.Host.Client.API {
    public partial class RHostSession {
        public async Task InvokeAsync(string function, CancellationToken cancellationToken = default(CancellationToken), params object[] args) {
            var fc = function.ToRFunctionCall(args);
            await ExecuteAsync(fc, cancellationToken);
        }
        public async Task<string> InvokeAndReturnAsync(string function, CancellationToken cancellationToken = default(CancellationToken), params object[] args) {
            var fc = function.ToRFunctionCall(args);
            var result = Invariant($"rtvs.{function}.result");
            string statement = Invariant($"{result} <- {fc}");
            await ExecuteAsync(statement, cancellationToken);
            return result;
        }

        public async Task<List<object>> GetListAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            var children = await DescribeChildrenAsync(Invariant($"as.list({expression})"), HasChildrenProperty, null, cancellationToken);
            var list = new List<object>();
            foreach (var c in children) {
                REvaluationResult r = await EvaluateAsync(c.Expression, REvaluationKind.Normal);
                list.Add(r.Result.ToObject<object>());
            }
            return list;
        }

        private const string _dfTempVariableName = ".rtvs.gdf.temp";
        public async Task<RDataFrame> GetDataFrameAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            await ExecuteAsync(Invariant($"{_dfTempVariableName} <- {expression}"), cancellationToken);

            var properties = LengthProperty | DimProperty | CanCoerceToDataFrameProperty;
            var info = await EvaluateAndDescribeAsync(expression, properties, cancellationToken);
            if (!info.CanCoerceToDataFrame) {
                throw new ArgumentException(Invariant($"{nameof(expression)} cannot be coerced to the data frame"));
            }

            await ExecuteAsync(Invariant($"{_dfTempVariableName}.df <- as.data.frame({_dfTempVariableName})"), cancellationToken);

            await ExecuteAsync(Invariant($"{_dfTempVariableName}.rn <- rownames({_dfTempVariableName})"), cancellationToken);
            var rowNames = (await GetListAsync(Invariant($"{_dfTempVariableName}.rn"), cancellationToken)).ToListOf<string>();

            await ExecuteAsync(Invariant($"{_dfTempVariableName}.cn <- colnames({_dfTempVariableName})"), cancellationToken);
            var colNames = (await GetListAsync(Invariant($"{_dfTempVariableName}.cn"), cancellationToken)).ToListOf<string>();

            var data = new List<List<object>>();
            for(int i = 1; i <= colNames.Count; i++) {
                 var list = await GetListAsync(Invariant($"{_dfTempVariableName}.df[[{i}]]"), cancellationToken);
                data.Add(list);
            }

            await ExecuteAsync(Invariant($"rm({_dfTempVariableName}.rn)"));
            await ExecuteAsync(Invariant($"rm({_dfTempVariableName}.cn)"));
            await ExecuteAsync(Invariant($"rm({_dfTempVariableName}.df)"));
            await ExecuteAsync(Invariant($"rm({_dfTempVariableName})"));

            return new RDataFrame(rowNames, colNames, data);
        }

        public async Task<T[,]> GetMatrixAsync<T>(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            var children = await DescribeChildrenAsync(expression, HasChildrenProperty, null, cancellationToken);
            var m = new T[0, 0];
            return m;
        }
    }
}
