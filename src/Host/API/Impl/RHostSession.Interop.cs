// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Newtonsoft.Json.Linq;
using static System.FormattableString;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.R.Host.Client.API {
    public partial class RHostSession {
        public async Task InvokeAsync(string function, CancellationToken cancellationToken = default(CancellationToken), params object[] args) {
            Check.ArgumentNull(nameof(function), function);
            var fc = function.ToRFunctionCall(args);
            await ExecuteAsync(fc, cancellationToken);
        }
        public async Task<string> InvokeAndReturnAsync(string function, CancellationToken cancellationToken = default(CancellationToken), params object[] args) {
            Check.ArgumentNull(nameof(function), function);
            var fc = function.ToRFunctionCall(args);
            var result = Invariant($"rtvs.{function}.result");
            string statement = Invariant($"{result} <- {fc}");
            await ExecuteAsync(statement, cancellationToken);
            return result;
        }

        public async Task<List<object>> GetListAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            var array = await GetJArrayAsync(expression, cancellationToken);
            return JArrayToObjectList(array);
        }

        public async Task<List<T>> GetListAsync<T>(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            if (typeof(T) == typeof(object)) {
                throw new ArgumentException(nameof(T), "Use GetListAsync(...) instead of GetListAsync<object>(...)");
            }
            var array = await GetJArrayAsync(expression, cancellationToken);
            return array.Values<T>().ToList();
        }

        private Task<JArray> GetJArrayAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            var exp = Invariant($"as.list({expression})");
            return EvaluateAsync<JArray>(exp, cancellationToken);
        }

        private const string _dfTempVariableName = ".rtvs.gdf.temp";
        public async Task<DataFrame> GetDataFrameAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            await ExecuteAsync(Invariant($"{_dfTempVariableName} <- {expression}"), cancellationToken);

            var properties = CanCoerceToDataFrameProperty;
            var info = await EvaluateAndDescribeAsync(expression, properties, cancellationToken);
            if (!info.CanCoerceToDataFrame) {
                throw new ArgumentException(Invariant($"{nameof(expression)} cannot be coerced to the data frame"));
            }

            await ExecuteAsync(Invariant($"{_dfTempVariableName}.rn <- rownames({_dfTempVariableName})"), cancellationToken);
            var rowNames = (await GetListAsync<string>(Invariant($"{_dfTempVariableName}.rn"), cancellationToken));

            await ExecuteAsync(Invariant($"{_dfTempVariableName}.cn <- colnames({_dfTempVariableName})"), cancellationToken);
            var colNames = (await GetListAsync<string>(Invariant($"{_dfTempVariableName}.cn"), cancellationToken));

            await ExecuteAsync(Invariant($"{_dfTempVariableName}.df <- as.data.frame({_dfTempVariableName})"), cancellationToken);
            var data = new List<List<object>>();

            for (int i = 1; i <= colNames.Count; i++) {
                var list = await GetListAsync(Invariant($"{_dfTempVariableName}.df[[{i}]]"), cancellationToken);
                data.Add(list);
            }

            await ExecuteAsync(Invariant($"rm({_dfTempVariableName}.rn)"));
            await ExecuteAsync(Invariant($"rm({_dfTempVariableName}.cn)"));
            await ExecuteAsync(Invariant($"rm({_dfTempVariableName}.df)"));
            await ExecuteAsync(Invariant($"rm({_dfTempVariableName})"));

            return new DataFrame(rowNames, colNames, data);
        }

        private List<object> JArrayToObjectList(JArray array) {
            if (array.Count == 0) {
                return new List<object>();
            }

            Type type;
            switch (array[0].Type) {
                case JTokenType.Integer: type = typeof(int); break;
                case JTokenType.Float: type = typeof(double); break;
                case JTokenType.Boolean: type = typeof(bool); break;
                case JTokenType.String: type = typeof(string); break;
                default:
                    throw new ArgumentException(nameof(array), Invariant($"Unsupported JSON type {array[0].Type}"));
            }
            return new List<object>(array.Select(jt => jt.ToObject(type)));
        }
    }
}
