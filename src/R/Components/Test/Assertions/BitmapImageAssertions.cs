// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Microsoft.R.Components.Test.Assertions {
    [ExcludeFromCodeCoverage]
    public class BitmapImageAssertions : ReferenceTypeAssertions<BitmapImage, BitmapImageAssertions> {
        protected override string Context { get; } = "System.Windows.Media.Imaging.BitmapImage";

        public BitmapImageAssertions(BitmapImage image) {
            Subject = image;
        }

        public AndConstraint<BitmapImageAssertions> HaveSamePixels(BitmapImage expected, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            var actualBytes = ImageAsBytes(Subject);
            var expectedBytes = ImageAsBytes(expected);

            Execute.Assertion.ForCondition(actualBytes.SequenceEqual(expectedBytes))
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected images to have identical pixels.", Subject.GetType().Name);

            return new AndConstraint<BitmapImageAssertions>(this);
        }

        private static byte[] ImageAsBytes(BitmapImage img) {
            using (var stream = new MemoryStream()) {
                var frame = BitmapFrame.Create(img);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(frame);
                encoder.Save(stream);
                return stream.GetBuffer();
            }
        }
    }
}
