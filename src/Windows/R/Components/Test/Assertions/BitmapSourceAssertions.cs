// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Media.Imaging;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Microsoft.R.Components.Test.Assertions {
    [ExcludeFromCodeCoverage]
    public class BitmapSourceAssertions : ReferenceTypeAssertions<BitmapSource, BitmapSourceAssertions> {
        protected override string Context { get; } = "System.Windows.Media.Imaging.BitmapSource";

        public BitmapSourceAssertions(BitmapSource image) {
            Subject = image;
        }

        public AndConstraint<BitmapSourceAssertions> HaveSamePixels(BitmapSource expected, string because = "", params object[] reasonArgs) {
            Subject.Should().NotBeNull();

            if (expected.Format != Subject.Format) {
                // We don't care if they are in different formats, just convert one of them before we compare data
                expected = new FormatConvertedBitmap(expected, Subject.Format, Subject.Palette, 0);
            }

            Execute.Assertion.ForCondition(Subject.PixelWidth == expected.PixelWidth)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected images to have identical width.", base.Subject.GetType().Name);

            Execute.Assertion.ForCondition(Subject.PixelHeight == expected.PixelHeight)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected images to have identical height.", base.Subject.GetType().Name);

            Execute.Assertion.ForCondition(Math.Round(Subject.DpiX) == Math.Round(expected.DpiX))
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected images to have identical dpi x.", base.Subject.GetType().Name);

            Execute.Assertion.ForCondition(Math.Round(Subject.DpiY) == Math.Round(expected.DpiY))
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected images to have identical dpi y.", base.Subject.GetType().Name);

            Execute.Assertion.ForCondition(Subject.Format == expected.Format)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected images to have identical format.", base.Subject.GetType().Name);

            int width = Subject.PixelWidth;
            int height = Subject.PixelHeight;

            byte[] actualPixels = new byte[width * height * 4];
            byte[] expectedPixels = new byte[width * height * 4];

            Subject.CopyPixels(actualPixels, width * 4, 0);
            expected.CopyPixels(expectedPixels, width * 4, 0);

            Execute.Assertion.ForCondition(actualPixels.SequenceEqual(expectedPixels))
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected images to have identical pixels.", base.Subject.GetType().Name);

            return new AndConstraint<BitmapSourceAssertions>(this);
        }
    }
}
