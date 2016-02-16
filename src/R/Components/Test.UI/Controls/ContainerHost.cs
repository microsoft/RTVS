using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.R.Components.Test.UI.Controls {
    internal class ContainerHost : ContentControl {
        private const int MaxContainerCount = 9;

        private static ContainerHost _current;
        private static readonly SemaphoreSlim AvailableSpots = new SemaphoreSlim(MaxContainerCount, MaxContainerCount);

        public static async Task<IDisposable> AddContainer(UIElement element) {
            await TaskUtilities.SwitchToBackgroundThread();
            await AvailableSpots.WaitAsync();
            return UIThreadHelper.Instance.Invoke(() => _current.AddContainerToHost(element));
        }

        private readonly UIElement[] _elements = new UIElement[MaxContainerCount];
        private readonly Window _window;
        private readonly Grid _grid;

        private int _columns = 1;
        private int _rows = 1;
        private int _firstEmptySlot;
        private int _columnWidth = 600;
        private int _rowHeight = 300;

        public ContainerHost(Window window) {
            _window = window;
            _current = this;
            _firstEmptySlot = 0;

            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            _grid = new Grid {
                RowDefinitions = { new RowDefinition() },
                ColumnDefinitions = { new ColumnDefinition() }
            };
            Content = _grid;
            UpdateWindowSize();
        }

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
    }
}
