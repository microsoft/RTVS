// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf.Extensions;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Matrix view user control for two dimensional data
    /// Internally Visual is used directly for performance
    /// </summary>
    internal sealed class MatrixView : ContentControl {
        public static readonly DependencyProperty HeaderFontFamilyProperty = DependencyProperty.Register(nameof(HeaderFontFamily), typeof(FontFamily), typeof(MatrixView),
            new FrameworkPropertyMetadata(new FontFamily("Segoe UI"), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty HeaderFontSizeProperty = DependencyProperty.Register(nameof(HeaderFontSize), typeof(double), typeof(MatrixView), 
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

        [Localizability(LocalizationCategory.Font)]
        public FontFamily HeaderFontFamily {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double HeaderFontSize {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        private readonly IServiceContainer _services;
        internal GridPoints Points { get; set; }
        internal IGridProvider<string> DataProvider { get; set; }

        public VisualGridScroller Scroller { get; private set; }
        public VisualGrid ColumnHeader { get; }
        public VisualGrid RowHeader { get; }
        public VisualGrid Data { get; }
        public ScrollBar HorizontalScrollBar { get; }
        public ScrollBar VerticalScrollBar { get; }

        public MatrixViewAutomationPeer AutomationPeer => UIElementAutomationPeer.FromElement(this) as MatrixViewAutomationPeer;

        private Border LeftTopCorner { get; }
        private Border RightTopCorner { get; }
        private Border LeftBottomCorner { get; }
        private Border RightBottomCorner { get; }

        static MatrixView() {
            ForegroundProperty.OverrideMetadata(typeof(MatrixView), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits, OnForegroundPropertyChanged));
        }

        public MatrixView() {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();
            _services = VsAppShell.Current.Services;

            ColumnHeader = new VisualGrid {
                ScrollDirection = ScrollDirection.Horizontal,
                Focusable = false,
                FontWeight = FontWeights.DemiBold
            }.Bind(VisualGrid.HeaderProperty, nameof(CanSort), this)
                .Bind(VisualGrid.FontFamilyProperty, nameof(HeaderFontFamily), this)
                .Bind(VisualGrid.FontSizeProperty, nameof(HeaderFontSize), this);

            RowHeader = new VisualGrid {
                ScrollDirection = ScrollDirection.Vertical,
                Focusable = false,
                FontWeight = FontWeights.DemiBold
            }.Bind(VisualGrid.FontFamilyProperty, nameof(HeaderFontFamily), this)
                .Bind(VisualGrid.FontSizeProperty, nameof(HeaderFontSize), this);

            Data = new VisualGrid {
                ScrollDirection = ScrollDirection.Both,
                Focusable = false
            }.Bind(VisualGrid.FontFamilyProperty, nameof(FontFamily), this)
                .Bind(VisualGrid.FontSizeProperty, nameof(FontSize), this);

            VerticalScrollBar = new ScrollBar { Orientation = Orientation.Vertical };
            VerticalScrollBar.Scroll += VerticalScrollBar_Scroll;

            HorizontalScrollBar = new ScrollBar {Orientation = Orientation.Horizontal};
            HorizontalScrollBar.Scroll += HorizontalScrollBar_Scroll;

            LeftTopCorner = new Border {
                BorderThickness = new Thickness(0,0,1,1),
                MinWidth = 10,
                MinHeight = 10,
                SnapsToDevicePixels = true
            }.Bind(Border.BorderBrushProperty, nameof(HeaderLinesBrush), this)
                .Bind(Border.BackgroundProperty, nameof(HeaderBackground), this);

            RightTopCorner = new Border {
                MinWidth = 10,
                MinHeight = 10
            }.Bind(Border.BackgroundProperty, nameof(Background), VerticalScrollBar);

            LeftBottomCorner = new Border {
                MinWidth = 10,
                MinHeight = 10
            }.Bind(Border.BackgroundProperty, nameof(Background), HorizontalScrollBar);

            RightBottomCorner = new Border {
                MinWidth = 10,
                MinHeight = 10
            }.Bind(Border.BackgroundProperty, nameof(Background), HorizontalScrollBar);;

            Content = new Grid {
                Background = Brushes.Transparent,
                Children = {
                    LeftTopCorner.SetGridPosition(0, 0),
                    ColumnHeader.SetGridPosition(0, 1),
                    RightTopCorner.SetGridPosition(0, 2),
                    RowHeader.SetGridPosition(1, 0),
                    Data.SetGridPosition(1, 1),
                    VerticalScrollBar.SetGridPosition(1, 2),
                    LeftBottomCorner.SetGridPosition(2, 0),
                    HorizontalScrollBar.SetGridPosition(2, 1),
                    RightBottomCorner.SetGridPosition(2, 2)
                },
                ColumnDefinitions = {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions = {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = new GridLength(1.0, GridUnitType.Star) },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            FocusVisualStyle = new Style();
            DataContext = this;
        }

        internal void Initialize(IGridProvider<string> dataProvider) {
            if (Points != null) {
                Points.PointChanged -= Points_PointChanged;
            }

            Points = new GridPoints(dataProvider.RowCount, dataProvider.ColumnCount, Data.RenderSize);
            Points.PointChanged += Points_PointChanged;

            ColumnHeader.Clear();
            RowHeader.Clear();
            Data.Clear();

            DataProvider = dataProvider;

            Scroller?.StopScroller();
            Scroller = new VisualGridScroller(this, _services);
            Refresh();  // initial refresh

            // reset scroll bar position to zero
            HorizontalScrollBar.Value = HorizontalScrollBar.Minimum;
            VerticalScrollBar.Value = VerticalScrollBar.Minimum;
            SetScrollBar(ScrollDirection.Both);

            CanSort = dataProvider.CanSort;
        }

        public void Refresh() {
            Scroller?.EnqueueCommand(GridUpdateType.Refresh, 0);
        }

        public void UpdateSort() {
            Scroller?.EnqueueCommand(GridUpdateType.Sort, 0);
        }

        public bool IsInsideHeader(Point point) {
            var width = Math.Min(ColumnHeader.ActualWidth, Points.ViewportWidth);
            var height = Math.Min(ColumnHeader.ActualHeight, Points.ColumnHeaderHeight);
            return point.X.LessOrCloseTo(width) && point.Y.LessOrCloseTo(height);
        }

        public bool IsInsideData(Point point) {
            var width = Math.Min(Data.ActualWidth, Points.ViewportWidth);
            var height = Math.Min(Data.ActualHeight, Points.ViewportHeight);
            return point.X.LessOrCloseTo(width) && point.Y.LessOrCloseTo(height);
        }

        public void SetCellFocus(long row, long column) {
            Data.HasKeyboardFocus = true;
            ColumnHeader.HasKeyboardFocus = false;
            Scroller.EnqueueCommand(GridUpdateType.SetFocus, new GridIndex(row, column));
        }

        public void SetHeaderFocus(long column) {
            Data.HasKeyboardFocus = false;
            ColumnHeader.HasKeyboardFocus = true;
            Scroller.EnqueueCommand(GridUpdateType.SetHeaderFocus, new GridIndex(0, column));
        }

        public bool CanSort {
            get => (bool)GetValue(CanSortProperty);
            set => SetValue(CanSortProperty, value);
        }

        public static readonly DependencyProperty CanSortProperty =
            DependencyProperty.Register("CanSort", typeof(bool), typeof(MatrixView), new PropertyMetadata(false));

        #region Foreground

        private static void OnForegroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue) {
                ((MatrixView)d).OnForegroundPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnForegroundPropertyChanged(Brush foregroundBrush) {
            ColumnHeader.Foreground = foregroundBrush;
            RowHeader.Foreground = foregroundBrush;
            Data.Foreground = foregroundBrush;

            Refresh();
        }

        #endregion

        #region GridLinesBrush

        public static readonly DependencyProperty GridLinesBrushProperty =
            DependencyProperty.Register(
                "GridLinesBrush",
                typeof(Brush),
                typeof(MatrixView),
                new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnGridLinesBrushPropertyChanged)));

        public Brush GridLinesBrush {
            get { return (Brush)GetValue(GridLinesBrushProperty); }
            set { SetValue(GridLinesBrushProperty, value); }
        }

        private static void OnGridLinesBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue) {
                ((MatrixView)d).OnGridLinesBrushPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnGridLinesBrushPropertyChanged(Brush gridLineBrush) {
            Data.SetGridLineBrush(gridLineBrush);

            Refresh();
        }

        #endregion

        #region GridBackground

        public static readonly DependencyProperty GridBackgroundProperty =
            DependencyProperty.Register(nameof(GridBackground), typeof(Brush), typeof(MatrixView), new FrameworkPropertyMetadata(Brushes.Transparent, OnGridBackgroundPropertyChanged));

        public Brush GridBackground {
            get => (Brush)GetValue(GridBackgroundProperty);
            set => SetValue(GridBackgroundProperty, value);
        }

        private static void OnGridBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!Equals(e.OldValue, e.NewValue)) {
                ((MatrixView)d).OnGridBackgroundPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnGridBackgroundPropertyChanged(Brush gridBackgroundBrush) {
            Data.Background = gridBackgroundBrush;

            // VisualGrid uses OnRender to paint background color, InvalidateVisual will call it
            Data.InvalidateVisual();

            Refresh();
        }

        #endregion
        
        #region GridSelectedBackground

        public static readonly DependencyProperty GridSelectedBackgroundProperty =
            DependencyProperty.Register(nameof(GridSelectedBackground), typeof(Brush), typeof(MatrixView), new FrameworkPropertyMetadata(Brushes.Transparent, OnGridSelectedBackgroundPropertyChanged));

        public Brush GridSelectedBackground {
            get => (Brush)GetValue(GridSelectedBackgroundProperty);
            set => SetValue(GridSelectedBackgroundProperty, value);
        }

        private static void OnGridSelectedBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!Equals(e.OldValue, e.NewValue)) {
                ((MatrixView)d).OnGridSelectedBackgroundPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnGridSelectedBackgroundPropertyChanged(Brush gridSelectedBackgroundBrush) {
            Data.SelectedBackground = gridSelectedBackgroundBrush;
            ColumnHeader.SelectedBackground = gridSelectedBackgroundBrush;

            // VisualGrid uses OnRender to paint background color, InvalidateVisual will call it
            Data.InvalidateVisual();
            ColumnHeader.InvalidateVisual();

            Refresh();
        }

        #endregion

        #region GridSelectedForeground

        public static readonly DependencyProperty GridSelectedForegroundProperty =
            DependencyProperty.Register(nameof(GridSelectedForeground), typeof(Brush), typeof(MatrixView), new FrameworkPropertyMetadata(Brushes.Black, OnGridSelectedForegroundPropertyChanged));

        public Brush GridSelectedForeground {
            get => (Brush)GetValue(GridSelectedForegroundProperty);
            set => SetValue(GridSelectedForegroundProperty, value);
        }

        private static void OnGridSelectedForegroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!Equals(e.OldValue, e.NewValue)) {
                ((MatrixView)d).OnGridSelectedForegroundPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnGridSelectedForegroundPropertyChanged(Brush gridSelectedForegroundBrush) {
            Data.SelectedForeground = gridSelectedForegroundBrush;
            ColumnHeader.SelectedForeground = gridSelectedForegroundBrush;

            Data.InvalidateVisual();
            ColumnHeader.InvalidateVisual();

            Refresh();
        }

        #endregion

        #region HeaderLinesBrush

        public static readonly DependencyProperty HeaderLinesBrushProperty =
            DependencyProperty.Register(
                "HeaderLinesBrush",
                typeof(Brush),
                typeof(MatrixView),
                new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnHeaderLinesBrushPropertyChanged)));

        public Brush HeaderLinesBrush {
            get { return (Brush)GetValue(GridLinesBrushProperty); }
            set { SetValue(GridLinesBrushProperty, value); }
        }

        private static void OnHeaderLinesBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue) {
                ((MatrixView)d).OnHeaderLinesBrushPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnHeaderLinesBrushPropertyChanged(Brush gridLineBrush) {
            ColumnHeader.SetGridLineBrush(gridLineBrush);
            RowHeader.SetGridLineBrush(gridLineBrush);

            Refresh();
        }

        #endregion

        #region HeaderBackground

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(nameof(HeaderBackground), typeof(Brush), typeof(MatrixView), 
            new FrameworkPropertyMetadata(Brushes.Black, OnHeaderBackgroundPropertyChanged));

        public Brush HeaderBackground {
            get => (Brush)GetValue(GridLinesBrushProperty);
            set => SetValue(GridLinesBrushProperty, value);
        }

        private static void OnHeaderBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != e.NewValue) {
                ((MatrixView)d).OnHeaderBackgroundPropertyChanged((Brush)e.NewValue);
            }
        }

        private void OnHeaderBackgroundPropertyChanged(Brush headerBackground) {
            ColumnHeader.Background = headerBackground;
            RowHeader.Background = headerBackground;

            // VisualGrid uses OnRender to paint background color, InvalidateVisual will call it
            ColumnHeader.InvalidateVisual();
            RowHeader.InvalidateVisual();

            Refresh();
        }

        #endregion

        protected override AutomationPeer OnCreateAutomationPeer() => new MatrixViewAutomationPeer(this);

        protected override void OnGotFocus(RoutedEventArgs e) {
            base.OnGotFocus(e);
            if (!Data.HasKeyboardFocus && !ColumnHeader.HasKeyboardFocus) {
                Data.HasKeyboardFocus = true;
                Refresh();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e) {
            base.OnLostFocus(e);
            Data.HasKeyboardFocus = false;
            ColumnHeader.HasKeyboardFocus = false;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (HandleKeyDown(e.Key)) {
                e.Handled = true;
            } else {
                base.OnKeyDown(e);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            // null if not Initialized yet
            Scroller?.EnqueueCommand(GridUpdateType.SizeChange, Data.RenderSize);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            if (Scroller != null && (e.Delta > 0 || e.Delta < 0)) {
                Scroller.EnqueueCommand(GridUpdateType.MouseWheel, e.Delta);
                e.Handled = true;
            }

            if (!e.Handled) {
                base.OnMouseWheel(e);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            Keyboard.Focus(this);
            var point = e.GetPosition(ColumnHeader);
            if (IsInsideHeader(point)) {
                var column = Points.GetColumn(point.X);
                SetHeaderFocus(column);
                var addToSort = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                ColumnHeader.ToggleSort(new GridIndex(0, column), addToSort);
                e.Handled = true;
                return;
            }

            point = e.GetPosition(Data);
            if (IsInsideData(point)) {
                SetCellFocus(Points.GetRow(point.Y), Points.GetColumn(point.X));
                e.Handled = true;
                return;
            }

            base.OnMouseLeftButtonDown(e);
        }

        private bool HandleKeyDown(Key key) {
            switch (key) {
                case Key.Tab:
                    if (Data.HasKeyboardFocus) {
                        Data.HasKeyboardFocus = false;
                        ColumnHeader.HasKeyboardFocus = true;
                        SetHeaderFocus(ColumnHeader.SelectedIndex.Column);
                    } else {
                        Data.HasKeyboardFocus = true;
                        ColumnHeader.HasKeyboardFocus = false;
                        SetCellFocus(Data.SelectedIndex.Row, Data.SelectedIndex.Column);
                    }
                    return true;
                case Key.Home:
                    Scroller?.EnqueueCommand(GridUpdateType.SetVerticalOffset, (0.0, ThumbTrack.None));
                    return true;
                case Key.End:
                    Scroller?.EnqueueCommand(GridUpdateType.SetVerticalOffset, (1.0, ThumbTrack.None));
                    return true;
                case Key.Up:
                    Scroller?.EnqueueCommand(GridUpdateType.FocusUp, 1L);
                    return true;
                case Key.Down:
                    Scroller?.EnqueueCommand(GridUpdateType.FocusDown, 1L);
                    return true;
                case Key.Right when Data.HasKeyboardFocus:
                    Scroller?.EnqueueCommand(GridUpdateType.FocusRight, 1L);
                    return true;
                case Key.Right when ColumnHeader.HasKeyboardFocus:
                    Scroller?.EnqueueCommand(GridUpdateType.HeaderFocusRight, 1L);
                    return true;
                case Key.Left when Data.HasKeyboardFocus:
                    Scroller?.EnqueueCommand(GridUpdateType.FocusLeft, 1L);
                    return true;
                case Key.Left when ColumnHeader.HasKeyboardFocus:
                    Scroller?.EnqueueCommand(GridUpdateType.HeaderFocusLeft, 1L);
                    return true;
                case Key.PageUp:
                    Scroller?.EnqueueCommand(GridUpdateType.FocusPageUp, 1L);
                    return true;
                case Key.PageDown:
                    Scroller?.EnqueueCommand(GridUpdateType.FocusPageDown, 1L);
                    return true;
                default:
                    return false;
            }
        }

        private void VerticalScrollBar_Scroll(object sender, ScrollEventArgs e) {
            if (Scroller == null) {
                return;
            }

            switch (e.ScrollEventType) {
                // page up/down
                case ScrollEventType.LargeDecrement:
                    Scroller.EnqueueCommand(GridUpdateType.PageUp, 1);
                    break;
                case ScrollEventType.LargeIncrement:
                    Scroller.EnqueueCommand(GridUpdateType.PageDown, 1);
                    break;

                // line up/down
                case ScrollEventType.SmallDecrement:
                    Scroller.EnqueueCommand(GridUpdateType.LineUp, 1);
                    break;
                case ScrollEventType.SmallIncrement:
                    Scroller.EnqueueCommand(GridUpdateType.LineDown, 1);
                    break;

                // scroll to here
                case ScrollEventType.ThumbPosition:
                    Scroller.EnqueueCommand(GridUpdateType.SetVerticalOffset, (ComputeVerticalOffset(e), ThumbTrack.None));
                    break;

                // thumb drag
                case ScrollEventType.ThumbTrack:
                    Scroller.EnqueueCommand(GridUpdateType.SetVerticalOffset, (ComputeVerticalOffset(e), ThumbTrack.Track));
                    break;
                case ScrollEventType.EndScroll:
                    Scroller.EnqueueCommand(GridUpdateType.SetVerticalOffset, (ComputeVerticalOffset(e), ThumbTrack.End));
                    break;

                // home/end (scroll to limit)
                case ScrollEventType.First:
                    Scroller.EnqueueCommand(GridUpdateType.SetVerticalOffset, (ComputeVerticalOffset(e), ThumbTrack.None));
                    break;
                case ScrollEventType.Last:
                    Scroller.EnqueueCommand(GridUpdateType.SetVerticalOffset, (ComputeVerticalOffset(e), ThumbTrack.None));
                    break;

                default:
                    break;
            }
        }

        private void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e) {
            if (Scroller == null) {
                return;
            }

            switch (e.ScrollEventType) {
                // page left/right
                case ScrollEventType.LargeDecrement:
                    Scroller.EnqueueCommand(GridUpdateType.PageLeft, 1);
                    break;
                case ScrollEventType.LargeIncrement:
                    Scroller.EnqueueCommand(GridUpdateType.PageRight, 1);
                    break;

                // line left/right
                case ScrollEventType.SmallDecrement:
                    Scroller.EnqueueCommand(GridUpdateType.LineLeft, 1);
                    break;
                case ScrollEventType.SmallIncrement:
                    Scroller.EnqueueCommand(GridUpdateType.LineRight, 1);
                    break;

                // scroll to here
                case ScrollEventType.ThumbPosition:
                    Scroller.EnqueueCommand(GridUpdateType.SetHorizontalOffset, (ComputeHorizontalOffset(e), ThumbTrack.None));
                    break;

                // thumb drag
                case ScrollEventType.ThumbTrack:
                    Scroller.EnqueueCommand(GridUpdateType.SetHorizontalOffset, (ComputeHorizontalOffset(e), ThumbTrack.Track));
                    break;
                case ScrollEventType.EndScroll:
                    Scroller.EnqueueCommand(GridUpdateType.SetHorizontalOffset, (ComputeHorizontalOffset(e), ThumbTrack.End));
                    break;

                // home/end (scroll to limit)
                case ScrollEventType.First:
                    Scroller.EnqueueCommand(GridUpdateType.SetHorizontalOffset, (ComputeHorizontalOffset(e), ThumbTrack.None));
                    break;
                case ScrollEventType.Last:
                    Scroller.EnqueueCommand(GridUpdateType.SetHorizontalOffset, (ComputeHorizontalOffset(e), ThumbTrack.None));
                    break;

                default:
                    break;
            }
        }

        private void Points_PointChanged(object sender, PointChangedEventArgs e) {
            SetScrollBar(e.Direction);
        }

        private double ComputeVerticalOffset(ScrollEventArgs e) {
            return e.NewValue / VerticalScrollBar.Maximum;
        }

        private double ComputeHorizontalOffset(ScrollEventArgs e) {
            return e.NewValue / HorizontalScrollBar.Maximum;
        }

        private void SetScrollBar(ScrollDirection direction) {
            _services.MainThread().CheckAccess();

            if (direction.HasFlag(ScrollDirection.Horizontal)) {
                HorizontalScrollBar.ViewportSize = Points.ViewportWidth;
                HorizontalScrollBar.Maximum = Points.HorizontalExtent - Points.ViewportWidth;
                HorizontalScrollBar.Value = Points.HorizontalOffset;
            }

            if (direction.HasFlag(ScrollDirection.Vertical)) {
                VerticalScrollBar.ViewportSize = Points.ViewportHeight;
                VerticalScrollBar.Maximum = Points.VerticalExtent - Points.ViewportHeight;
                VerticalScrollBar.Value = Points.VerticalOffset;
            }

            AutomationPeer?.ScrollProvider?.UpdateValues();
        }
    }
}