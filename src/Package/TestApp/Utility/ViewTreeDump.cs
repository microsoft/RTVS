// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Utility {
    [ExcludeFromCodeCoverage]
    internal class ViewTreeDump {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = true;

        public static void CompareVisualTrees(DeployFilesFixture fixture, VisualTreeObject actual, string fileName) {
            CompareVisualTreesImplementation(fixture, actual, fileName);
        }

        private static void CompareVisualTreesImplementation(DeployFilesFixture fixture, VisualTreeObject actual, string fileName) {
            string testFileName = fileName + ".tree";
            string testFilePath = fixture.GetDestinationPath(testFileName);

            if (_regenerateBaselineFiles) {
                var serializedActual = SerializeVisualTree(actual);
                string baselineFilePath = fixture.GetSourcePath(testFileName);
                TestFiles.UpdateBaseline(baselineFilePath, serializedActual);
            } else {
                var deserializedExpected = DeserializeVisualTree(testFilePath);
                CompareVisualTree(actual, deserializedExpected);
            }
        }

        private static void CompareVisualTree(VisualTreeObject actual, VisualTreeObject expected, bool compareProperty = true) {
            bool visible = true;
            
            // compare
            actual.Name.Should().Be(expected.Name);

            var visibility = actual.Properties.FirstOrDefault(p => p.Name == "Visibility");
            if (visibility != null) {
                visible = (visibility.Value != "Collapsed");
            }

            if (compareProperty && visible) {
                var filteredActual = actual.Properties.Where(p => SupportedWpfProperties.IsSupported(p.Name));
                var filteredExpected = expected.Properties.Where(p => SupportedWpfProperties.IsSupported(p.Name));

                filteredActual.Should().BeEquivalentTo(filteredExpected);
            }

            actual.Children.Count.ShouldBeEquivalentTo(expected.Children.Count);

            if (visible) {
                var sortedActualChildren = actual.Children.OrderBy(c => c.Name).ToList();
                var sortedExpectedChildren = expected.Children.OrderBy(c => c.Name).ToList();

                sortedActualChildren.Count.Should().Be(sortedExpectedChildren.Count);

                for (int i = 0; i < actual.Children.Count; i++) {
                    CompareVisualTree(sortedActualChildren[i], sortedExpectedChildren[i], compareProperty);
                }
            }
        }

        private static string SerializeVisualTree(VisualTreeObject o) {
            var serializer = new XmlSerializer(typeof(VisualTreeObject));
            using (var stringWriter = new StringWriter()) {
                serializer.Serialize(stringWriter, o);
                return stringWriter.ToString();
            }
        }

        private static VisualTreeObject DeserializeVisualTree(string filePath) {
            var serializer = new XmlSerializer(typeof(VisualTreeObject));

            using (var stringReader = new StreamReader(filePath)) {
                return (VisualTreeObject)serializer.Deserialize(stringReader);
            }
        }
    }
}
