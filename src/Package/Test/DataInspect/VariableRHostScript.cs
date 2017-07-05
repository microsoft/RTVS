// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.DataInspection;
using Microsoft.R.Editor.Data;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.StackTracing;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.Shell;
using static Microsoft.R.DataInspection.REvaluationResultProperties;
using Microsoft.Common.Core.Services;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    public class VariableRHostScript : RHostScript {
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);
        private VariableViewModel _globalEnv;

        public VariableRHostScript(IServiceContainer services)
            : base(services) { }

        public VariableViewModel GlobalEnvrionment => _globalEnv;

        public async Task<IREvaluationResultInfo> EvaluateAsync(string rScript) {
            // One eval at a time
            await _sem.WaitAsync();
            try {
                var frames = await Session.TracebackAsync();
                var frame = frames.FirstOrDefault(f => f.Index == 0);

                const REvaluationResultProperties properties = ClassesProperty | ExpressionProperty | TypeNameProperty | DimProperty| LengthProperty;
                var result = await frame.TryEvaluateAndDescribeAsync(rScript, properties, RValueRepresentations.Str());

                var globalResult = await frame.TryEvaluateAndDescribeAsync("base::environment()", properties, RValueRepresentations.Str());
                _globalEnv = new VariableViewModel(globalResult, Services);

                return result;
            } finally {
                _sem.Release();
            }
        }

        /// <summary>
        /// evaluate R script and assert if the expectation is not found in global environment
        /// </summary>
        public async Task<IRSessionDataObject> EvaluateAndAssert(
            string rScript,
            VariableExpectation expectation,
            Action<IRSessionDataObject, VariableExpectation> assertAction) {

            await EvaluateAsync(rScript);

            var children = await _globalEnv.GetChildrenAsync();

            // must contain one and only expectation in result
            var evaluation = children.First(v => v.Name == expectation.Name);
            assertAction(evaluation, expectation);

            return evaluation;
        }

        public static void AssertEvaluationWrapper(IRSessionDataObject rdo, VariableExpectation expectation) {
            var v = (VariableViewModel)rdo;
            v.ShouldBeEquivalentTo(expectation, o => o.ExcludingMissingMembers());
        }

        public static void AssertEvaluationWrapper_ValueStartWith(IRSessionDataObject rdo, VariableExpectation expectation) {
            var v = (VariableViewModel)rdo;
            v.Name.ShouldBeEquivalentTo(expectation.Name);
            v.Value.Should().StartWith(expectation.Value);
            v.Class.ShouldBeEquivalentTo(expectation.Class);
            v.TypeName.ShouldBeEquivalentTo(expectation.TypeName);
            v.HasChildren.ShouldBeEquivalentTo(expectation.HasChildren);
        }
     }
}
