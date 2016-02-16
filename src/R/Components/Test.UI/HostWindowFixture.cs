using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Microsoft.R.Components.Test.UI.Controls;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Components.Test.UI {
    [AssemblyFixture]
    public sealed class HostWindowFixture : IAsyncLifetime {
        private Task _createWindowTask;
        private Window _window;
        
        public async Task InitializeAsync() {
            _createWindowTask = ScheduleTask(ShowWindow);
            await ScheduleTask(() => { });
        }

        public async Task DisposeAsync() {
            await ScheduleTask(CloseWindow);
            await _createWindowTask;
        }

        private void ShowWindow() {
            _window = new Window {
                Title = "Test window",
                Height = double.NaN,
                Width = double.NaN,
            };

            _window.Content = new ContainerHost(_window);
            _window.SizeToContent = SizeToContent.WidthAndHeight;
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