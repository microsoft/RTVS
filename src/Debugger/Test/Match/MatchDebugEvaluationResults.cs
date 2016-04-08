// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Common.Core.Test.Match;

namespace Microsoft.R.Debugger.Test.Match {
    internal class MatchDebugEvaluationResults : IEnumerable<IEquatable<DebugEvaluationResult>> {
        private readonly List<IEquatable<DebugEvaluationResult>> _results = new List<IEquatable<DebugEvaluationResult>>();

        public IEnumerator<IEquatable<DebugEvaluationResult>> GetEnumerator() {
            return _results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(IEquatable<DebugEvaluationResult> match) {
            _results.Add(match);
        }

        public void Add(IEquatable<string> name, IEquatable<string> expr, IEquatable<string> deparse, IEquatable<string> type, IEquatable<string>[] classes) {
            _results.Add(
                MatchAny<DebugEvaluationResult>.ThatMatches(
                    new MatchMembers<DebugValueEvaluationResult>()
                        .Matching(r => r.Name, name)
                        .Matching(r => r.Expression, expr)
                        .Matching(r => r.Classes, new MatchElements<string>(false, classes))
                        .Matching(r => r.GetRepresentation().Deparse, deparse)));
        }
    }
}
