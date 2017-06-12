// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Services;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Brushes = Microsoft.R.Wpf.Brushes;

namespace Microsoft.Markdown.Editor.Margin {
    public class LiveSyncMargin : DockPanel, IWpfTextViewMargin {
        private readonly IServiceContainer _services;
        private readonly RMarkdownOptions _options;
        private Image _image;
        private TextBlock _text;

        public LiveSyncMargin(IWpfTextView view, IServiceContainer services) {
            _services = services;
            _options = _services.GetService<IREditorSettings>().MarkdownOptions;

            SetResourceReference(BackgroundProperty, Brushes.ScrollBarBackgroundBrushKey);
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;

            Initialized += (s, e) => {
                if (view.Properties.TryGetProperty(typeof(BrowserMargin), out BrowserMargin browser)) {
                    CreateControls();
                    UpdateControls();
                }
            };
        }

        private void CreateControls() {
            var panel = new DockPanel {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
                Cursor = Cursors.Hand,
                ToolTip = Editor.Resources.ScrollSync_Tooltip
            };

            panel.MouseUp += OnClick;
            Children.Add(panel);

            _text = new TextBlock { Margin = new Thickness(0, 0, 5, 0) };
            panel.Children.Add(_text);

            _image = new Image { Margin = new Thickness(0, 1, 2, 0) };
            panel.Children.Add(_image);
        }

        private void UpdateControls() {
            var imageName = _options.AutomaticSync ? "Play" : "Pause";
            var imageService = _services.GetService<IImageService>();
            _image.Source = imageService.GetImage(imageName) as ImageSource;
            _text.Text = _options.AutomaticSync ? Editor.Resources.ScrollSync_Active : Editor.Resources.ScrollSync_Paused;
        }

        private void OnClick(object sender, MouseButtonEventArgs e) {
            _options.AutomaticSync = !_options.AutomaticSync;
            UpdateControls();
        }

        public bool Enabled => true;

        public double MarginSize => 12;

        public FrameworkElement VisualElement => this;

        public void Dispose() { }

        public ITextViewMargin GetTextViewMargin(string marginName) => this;
    }
}
