// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.UnitTests.Core.UI {
    internal class ContainerHost : ContentControl {
        private const int MaxContainerCount = 9;

        private static int _count;
        private static ContainerHost _current;
        private static readonly SemaphoreSlim AvailableSpots = new SemaphoreSlim(MaxContainerCount, MaxContainerCount);

        public static async Task Increment() {
            if (Interlocked.Increment(ref _count) == 1) {
                _current = UIThreadHelper.Instance.Invoke(() => new ContainerHost());
                await _current.ShowWindowAsync();
            }
        }

        public static async Task Decrement() {
            if (Interlocked.Decrement(ref _count) == 0) {
                await _current.CloseWindowAsync();
                _current = null;
            }
        }

        public static async Task<IDisposable> AddContainer(UIElement element) {
            await TaskUtilities.SwitchToBackgroundThread();
            await AvailableSpots.WaitAsync();
            var removeFromHost = UIThreadHelper.Instance.Invoke(() => _current.AddContainerToHost(element));
            await UIThreadHelper.Instance.DoEventsAsync();
            return removeFromHost;
        }

        private readonly UIElement[] _elements = new UIElement[MaxContainerCount];
        private Grid _grid;

        private int _columns = 1;
        private int _rows = 1;
        private int _firstEmptySlot;
        private int _columnWidth = 600;
        private int _rowHeight = 300;

        private Window _window;
        private Task _createWindowTask;

        private void UpdateWindowSize() {
            Width = _columns * _columnWidth;
            Height = _rows * _rowHeight;
        }

        private IDisposable AddContainerToHost(UIElement element) {
            var index = _firstEmptySlot;
            _elements[index] = element;
            FindNextEmptySlot();
            ExtendGridIfRequired();

            Grid.SetRow(element, index / 3);
            Grid.SetColumn(element, index % 3);
            _grid.Children.Add(element);
            return Disposable.Create(() => UIThreadHelper.Instance.Invoke(() => RemoveContainerFromHost(index)));
        }

        private void RemoveContainerFromHost(int index) {
            var element = _elements[index];
            _elements[index] = null;
            if (index < _firstEmptySlot) {
                _firstEmptySlot = index;
            }

            _grid.Children.Remove(element);
            ShrinkGridIfPossible();

            AvailableSpots.Release();
        }

        private void FindNextEmptySlot() {
            while (_firstEmptySlot < MaxContainerCount && _elements[_firstEmptySlot] != null) {
                _firstEmptySlot++;
            }
        }

        private void ExtendGridIfRequired() {
            if (_elements.Length == 0) {
                return;
            }

            var requiredRows = GetRequiredRows();
            var requiredColumns = GetRequiredColumns();

            if (_rows >= requiredRows && _columns >= requiredColumns) {
                return;
            }

            while (_rows < requiredRows) {
                _rows++;
                _grid.RowDefinitions.Add(new RowDefinition());
            }

            while (_columns < requiredColumns) {
                _columns++;
                _grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            UpdateWindowSize();
        }

        private void ShrinkGridIfPossible() {
            if (_elements[8] != null) {
                return;
            }

            var requiredRows = GetRequiredRows();
            var requiredColumns = GetRequiredColumns();

            if (_rows <= requiredRows && _columns <= requiredColumns) {
                return;
            }

            if (_rows > requiredRows) {
                _grid.RowDefinitions.RemoveRange(requiredRows, _rows - requiredRows);
            }

            if (_columns > requiredColumns) {
                _grid.ColumnDefinitions.RemoveRange(requiredColumns, _columns - requiredColumns);
            }

            _rows = requiredRows;
            _columns = requiredColumns;

            UpdateWindowSize();
        }

        private int GetRequiredRows() {
            if (_elements[6] != null || _elements[7] != null) {
                return 3;
            }

            if (_elements[3] != null || _elements[4] != null || _elements[5] != null) {
                return 2;
            }

            return 1;
        }

        private int GetRequiredColumns() {
            if (_elements[2] != null || _elements[5] != null) {
                return 3;
            }

            if (_elements[1] != null || _elements[4] != null || _elements[7] != null) {
                return 2;
            }

            return 1;
        }

        private async Task ShowWindowAsync() {
            await TaskUtilities.SwitchToBackgroundThread();
            _createWindowTask = ScheduleTask(ShowWindow);
             await UIThreadHelper.Instance.DoEventsAsync();
        }

        private async Task CloseWindowAsync() {
            await ScheduleTask(CloseWindow);
            await _createWindowTask;
        }

        private void ShowWindow() {
            _firstEmptySlot = 0;

            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            _grid = new Grid {
                RowDefinitions = { new RowDefinition() },
                ColumnDefinitions = { new ColumnDefinition() }
            };

            Content = _grid;

            var checkerBrush = new DrawingBrush {
                Drawing = new DrawingGroup {
                    Children = new DrawingCollection {
                        new GeometryDrawing(Brushes.White, null, new RectangleGeometry(new Rect(0, 0, 10, 10))),
                        new GeometryDrawing(Brushes.LightGray, null, new GeometryGroup {
                            Children = new GeometryCollection {
                                new RectangleGeometry(new Rect(0, 0, 5, 5)),
                                new RectangleGeometry(new Rect(5, 5, 5, 5))
                            }
                        })
                    }
                },
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 10, 10),
                ViewportUnits = BrushMappingMode.Absolute
            };

            _window = new Window {
                Title = "Test window",
                Height = double.NaN,
                Width = double.NaN,
                Content = this,
                SizeToContent = SizeToContent.WidthAndHeight,
                Background = checkerBrush
            };

            UpdateWindowSize();

            if (Screen.AllScreens.Length == 1) {
                _window.Left = 0;
                _window.Top = 50;
            } else {
                var secondary = Screen.AllScreens.First(x => !x.Primary);
                _window.Left = secondary.WorkingArea.Left;
                _window.Top = secondary.WorkingArea.Top + 80;
            }

            _window.Topmost = true;
            _window.ShowDialog();
        }

        private void CloseWindow() {
            _window?.Close();
            _window = null;
        }

        private static Task ScheduleTask(Action action) {
            return Task.Run(() => UIThreadHelper.Instance.Invoke(action));
        }
    }
}
