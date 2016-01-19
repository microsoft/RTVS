using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Host.Client.Test {
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public class GraphicsDeviceTestFilesFixture : DeployFilesFixture {
        public string ExportToPdfResultPath { get; }
        public string ExportToBmpResultPath { get; }
        public string ExportToPngResultPath { get; }
        public string ExportToJpegResultPath { get; }
        public string ExportToTiffResultPath { get; }
        public string ExportPreviousPlotToImageResultPath { get; }
        public string ExpectedExportPreviousPlotToImagePath { get; }
        public string ExpectedExportToPdfPath { get; }

        public GraphicsDeviceTestFilesFixture() : base(@"Host\Client\Test\Files", "Files") {
            // Path to files that are generated when tests are executed
            ExportToPdfResultPath = Path.Combine(DestinationPath, "ExportToPdfResult.pdf");
            ExportToBmpResultPath = Path.Combine(DestinationPath, "ExportToBmpResult.bmp");
            ExportToPngResultPath = Path.Combine(DestinationPath, "ExportToPngResult.png");
            ExportToJpegResultPath = Path.Combine(DestinationPath, "ExportToJpegResult.jpg");
            ExportToTiffResultPath = Path.Combine(DestinationPath, "ExportToTiffResult.tif");
            ExportPreviousPlotToImageResultPath = Path.Combine(DestinationPath, "ExportPreviousPlotToImageResultPath.bmp");

            // Path to files that are compared against and are included as part of test sources
            ExpectedExportPreviousPlotToImagePath = Path.Combine(DestinationPath, "ExportPreviousPlotToImageExpectedResult.bmp");
            ExpectedExportToPdfPath = Path.Combine(DestinationPath, "ExportToPdfExpectedResult.pdf");
        }
    }
}