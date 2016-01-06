using System.IO;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Host.Client.Test {
    [AssemblyFixture]
    public class GraphicsDeviceTestFilesFixture : DeployFilesFixture {
        public string ExportToPdfResultPath { get; }
        public string ExportToBmpResultPath { get; }
        public string ExportToPngResultPath { get; }
        public string ExportToJpegResultPath { get; }
        public string ExportToTiffResultPath { get; }

        public GraphicsDeviceTestFilesFixture() : base(@"Host\Client\Test\Files", "Files") {
            ExportToPdfResultPath = Path.Combine(DestinationPath, "ExportToPdfResult.pdf");
            ExportToBmpResultPath = Path.Combine(DestinationPath, "ExportToBmpResult.bmp");
            ExportToPngResultPath = Path.Combine(DestinationPath, "ExportToPngResult.png");
            ExportToJpegResultPath = Path.Combine(DestinationPath, "ExportToJpegResult.jpg");
            ExportToTiffResultPath = Path.Combine(DestinationPath, "ExportToTiffResult.tif");
        }
    }
}