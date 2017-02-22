// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client.Host; // RHostDisconnectedException
using Newtonsoft.Json.Linq;
using static System.FormattableString;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.R.Host.Client {
    public partial class RHostSession {
        /// <summary>
        /// Invokes R function with a set of arguments. Does not return any value.
        /// </summary>
        /// <param name="function">Function name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="args">Function arguments</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public async Task InvokeAsync(string function, CancellationToken cancellationToken = default(CancellationToken), params object[] args) {
            Check.ArgumentNull(nameof(function), function);
            var fc = function.ToRFunctionCall(args);
            await ExecuteAsync(fc, cancellationToken);
        }

        /// <summary>
        /// Invokes R function with a set of arguments. Returns name of a 
        /// temporary variable that received the result.
        /// </summary>
        /// <param name="function">Function name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="args">Function arguments</param>
        /// <returns>Name of the variable that holds the data returned by the function</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public async Task<string> InvokeAndReturnAsync(string function, CancellationToken cancellationToken = default(CancellationToken), params object[] args) {
            Check.ArgumentNull(nameof(function), function);
            var fc = function.ToRFunctionCall(args);
            var result = Invariant($"rtvs.{function}.result");
            string statement = Invariant($"{result} <- {fc}");
            await ExecuteAsync(statement, cancellationToken);
            return result;
        }

        /// <summary>
        /// Retrieves list of unknown type from R variable
        /// </summary>
        /// <param name="expression">Expression (variable name) to fetch as list</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of objects</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public async Task<List<object>> GetListAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            var array = await GetJArrayAsync(expression, cancellationToken);
            return JArrayToObjectList(array);
        }

        /// <summary>
        /// Retrieves list of specific type from R variable
        /// </summary>
        /// <param name="expression">Expression (variable name) to fetch as list</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of values of the provided type</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
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
        /// <summary>
        /// Retrieves data frame from R variable
        /// </summary>
        /// <param name="expression">Expression (variable name) to fetch as data frame</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Data frame</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
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

        /// <summary>
        /// Retrieves information about R object or expression
        /// </summary>
        /// <param name="expression">Expression (variable name) to describe</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object information</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public async Task<IRObjectInformation> GetInformationAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            var properties = TypeNameProperty | DimProperty | LengthProperty;
            var info = await _session.EvaluateAndDescribeAsync(expression, properties, null, cancellationToken);
            return new RObjectInfo() {
                TypeName = info.TypeName,
                Length = info.Length ?? 0,
                Dim = info.Dim
            };
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

        private class RObjectInfo: IRObjectInformation {
            public string TypeName { get; set; }
            public long Length { get; set; }
            public IReadOnlyList<long> Dim { get; set; }
        }
    }
}
