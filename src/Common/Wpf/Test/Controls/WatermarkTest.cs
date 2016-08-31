// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Common.Wpf.Controls;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.Common.Wpf.Test.Controls {
    public class WatermarkTest : IAsyncLifetime {
        private readonly ContainerHostMethodFixture _containerHost;
        private readonly TextBox _textBox;
        private IDisposable _containerDisposable;

        public WatermarkTest(ContainerHostMethodFixture containerHost) {
            _containerHost = containerHost;
            _textBox = new TextBox {
                Width = 300,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        public async Task InitializeAsync() {
            _containerDisposable = await _containerHost.AddToHost(_textBox);
        }

        public Task DisposeAsync() {
            _containerDisposable?.Dispose();
            return Task.CompletedTask;
        }

        [Test(ThreadType.UI)]
        public async Task SetTextBlock() {
            var textBlock = new TextBlock {
                Background = new SolidColorBrush(Color.FromArgb(127, 255, 0, 0)),
                Text = "ABC",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Overlay.SetAdornerContent(_textBox, textBlock);

            await Task.Delay(1000);
            textBlock.Text = "DEF";
            await Task.Delay(1000);
            textBlock.Visibility = Visibility.Collapsed;
            await Task.Delay(1000);
        }
    }
}
