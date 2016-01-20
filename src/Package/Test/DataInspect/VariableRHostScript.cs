using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Data;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    /// <summary>
    /// contains expectation for EvaluationWrapper
    /// </summary>
    [ExcludeFromCodeCoverage]
    class VariableExpectation {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Class { get; set; }
        public string TypeName { get; set; }
        public bool HasChildren { get; set; }
        public bool CanShowDetail { get; set; }
    }

    [ExcludeFromCodeCoverage]
    class VariableRHostScript : RHostScript {
        private VariableProvider _variableProvider;

        public VariableRHostScript() :
            base(VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>()) {

            _variableProvider = VariableProvider.Current;
            DoIdle(100);
        }

        public EvaluationWrapper GlobalEnvironment {
            get {
                return _variableProvider.GlobalEnvironment;
            }
        }

        private ManualResetEventSlim _mre;
        public async Task EvaluateAsync(string rScript) {
            try {
                _mre = new ManualResetEventSlim();
                _variableProvider.VariableChanged += VariableProvider_VariableChanged;
                using (var evaluation = await base.Session.BeginEvaluationAsync()) {
                    await evaluation.EvaluateAsync(rScript);
                }

                if (!_mre.Wait(TimeSpan.FromMilliseconds(1000))) {
                    throw new TimeoutException("Evaluate time out");
                }
            } finally {
                _variableProvider.VariableChanged -= VariableProvider_VariableChanged;
            }
        }

        /// <summary>
        /// evaluate R script and assert if the expectation is not found in global environment
        /// </summary>
        /// <param name="rScript"></param>
        /// <param name="expectation"></param>
        /// <returns></returns>
        public async Task EvaluateAndAssert(string rScript, VariableExpectation expectation) {
            await EvaluateAsync(rScript);

            var children = await GlobalEnvironment.GetChildrenAsync();

            // must contain one and only expectation in result
            var evaluation = children.First(v => v.Name == expectation.Name);
            AssertEvaluationWrapper(evaluation, expectation);
        }

        private static void AssertEvaluationWrapper(IRSessionDataObject v, VariableExpectation expectation) {
            v.ShouldBeEquivalentTo(expectation, o => o.ExcludingMissingMembers());
        }

        private void VariableProvider_VariableChanged(object sender, VariableChangedArgs e) {
            _mre.Set();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                _variableProvider.Dispose();
            }
        }

        public static void DoIdle(int ms) {
            UIThreadHelper.Instance.Invoke(() => {
                int time = 0;
                while (time < ms) {
                    TestScript.DoEvents();
                    VsAppShell.Current.DoIdle();
                    EditorShell.Current.DoIdle();

                    Thread.Sleep(20);
                    time += 20;
                }
            });
        }
    }
}
