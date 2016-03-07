// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Excel;
using Microsoft.R.Editor.Data;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.DataInspect.Office;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.DataInspect;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Office {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class RExcelTest {
        [Test]
        public async Task FetchData() {
            using (var script = new VariableRHostScript()) {
                var o = await GetMtCars(script);
                o.Should().NotBeNull();

                ExcelData xlData = ExcelInterop.GenerateExcelData("x", null, o.Dimensions[0], o.Dimensions[1]);

                int rows = o.Dimensions[0];
                int cols = o.Dimensions[1];

                xlData.CellData.GetLength(0).Should().Be(rows);
                xlData.CellData.GetLength(1).Should().Be(cols);

                xlData.RowNames.Length.Should().Be(rows);
                xlData.ColNames.Length.Should().Be(cols);

                xlData.CellData[0, 0].Should().Be("21.0");
                xlData.CellData[rows - 1, cols - 1].Should().Be("2");
            }
        }

        [Test]
        public void FetchDataErrors01() {
            ExcelData xlData = ExcelInterop.GenerateExcelData("x", null, -10, -10);
            xlData.Should().BeNull();
         }

        [Test]
        public void FetchDataErrors02() {
            using (var script = new VariableRHostScript()) {
                ExcelData xlData = ExcelInterop.GenerateExcelData("zzzzzz", null, 2, 2);
                xlData.Should().BeNull();
            }
        }

        private async Task<IRSessionDataObject> GetMtCars(VariableRHostScript script) {
            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var session = sessionProvider.GetInteractiveWindowRSession();

            using (var e = await session.BeginInteractionAsync()) {
                await e.RespondAsync("x <- mtcars" + Environment.NewLine);
            }

            await script.EvaluateAsync("x" + Environment.NewLine);
            IReadOnlyList<IRSessionDataObject> vars = await script.GlobalEnvrionment.GetChildrenAsync();
            return vars.FirstOrDefault(x => x.Name == "x");
        }

        //private Application MockExcel(string workSheetName) {
        //    var ws = Substitute.For<Worksheet>();
        //    var wb = Substitute.For<Workbook>();

        //    var wbs = Substitute.For<Workbooks>();
        //    wbs[ExcelInterop.WorkbookName].Returns(wb);

        //    var xlApp = Substitute.For<Application>();
        //    xlApp.Workbooks.Returns(wbs);

        //    return xlApp;
        //}
    }
}
