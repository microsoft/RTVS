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
        public async Task InvokeAsync(string function, IEnumerable<RFunctionArg> arguments, CancellationToken cancellationToken = default(CancellationToken)) {
            var fc = function.ToRFunctionCall(arguments);
            await ExecuteAsync(fc);
        }
        public async Task<string> InvokeAndReturnAsync(string function, IEnumerable<RFunctionArg> arguments, CancellationToken cancellationToken = default(CancellationToken)) {
            var fc = function.ToRFunctionCall(arguments);
            var result = Invariant($"rtvs.{function}.result");
            string statement = Invariant($"{result} <- {fc}");
            await ExecuteAsync(statement);
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

        private const string _tempVariableName = ".rtvs.temp";
        public async Task<RDataFrame> GetDataFrameAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            await ExecuteAsync(Invariant($".rtvs.temp <- {expression}"), cancellationToken);
            await ExecuteAsync(".rtvs.temp.rn <- rownames(.rtvs.temp)", cancellationToken);

            REvaluationResultProperties properties =
                         ExpressionProperty |
                         AccessorKindProperty |
                         TypeNameProperty |
                         ClassesProperty |
                         LengthProperty |
                         SlotCountProperty |
                         AttributeCountProperty |
                         DimProperty |
                         FlagsProperty |
                         CanCoerceToDataFrameProperty;
            var r = await DescribeChildrenAsync(expression, properties, 100, cancellationToken);

            var info = await EvaluateAndDescribeAsync(expression, properties, cancellationToken);
            //var info = await session.EvaluateAndDescribeAsync(expression, CanCoerceToDataFrameProperty, cancellationToken);
            if(!info.CanCoerceToDataFrame) {
                throw new ArgumentException(Invariant($"{nameof(expression)} cannot be coerced to the data frame"));
            }
            await ExecuteAsync(Invariant($"{_tempVariableName} < - as.dataframe({expression})"), cancellationToken);
            info = await EvaluateAndDescribeAsync(Invariant($"{_tempVariableName}"), DimProperty, cancellationToken);
            //var children = await session.DescribeChildrenAsync(expression, HasChildrenProperty, null, cancellationToken);
            return new RDataFrame(new List<string>(), new List<string>(), new List<List<object>>());
        }

        public async Task<T[,]> GetMatrixAsync<T>(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            var children = await DescribeChildrenAsync(expression, HasChildrenProperty, null, cancellationToken);
            var m = new T[0, 0];
            return m;
        }
    }
}
