using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.UnitTests.Core.Threading;
using Screen = System.Windows.Forms.Screen;

namespace Microsoft.Common.Core.Test.Controls {
    /// <summary>
    /// Control window
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class ControlWindow {
        [ExcludeFromCodeCoverage]
        class ControlTestRequest {
            public Type ControlType { get; }

            public ControlTestRequest(Type controlType) {
                ControlType = controlType;
            }
        }

        /// <summary>
        /// Control that is being tested
        /// </summary>
        public static Control Control { get; private set; }
        /// <summary>
        /// WPF window that contains the control
        /// </summary>
        public static Window Window { get; private set; }

        /// <summary>
        /// <summary>
        /// Creates WPF window and control instance then hosts control in the window.
        /// </summary>
        public static void Create(Type controlType) {
            var evt = new ManualResetEventSlim(false);
            Task.Run(() => UIThreadHelper.Instance.Invoke(() => CreateWindowInstance(new ControlTestRequest(controlType), evt)));
            evt.Wait();
        }

        private static void CreateWindowInstance(ControlTestRequest request, ManualResetEventSlim evt) {

            Window = new Window();

            if (Screen.AllScreens.Length == 1) {
                Window.Left = 0;
                Window.Top = 50;
            }
            else {
                Screen secondary = Screen.AllScreens.FirstOrDefault(x => !x.Primary);
                Window.Left = secondary.WorkingArea.Left;
                Window.Top = secondary.WorkingArea.Top + 50;
            }

            Window.Width = 800;
            Window.Height = 600;

            Control = Activator.CreateInstance(request.ControlType) as Control;
            Window.Title = "Control - " + request.ControlType.ToString();
            Window.Content = Control;

            evt.Set();

            Window.Topmost = true;
            Window.ShowDialog();
        }

        /// <summary>
        /// Closes editor window
        /// </summary>
        public static void Close() {
            var action = new Action(() => {
                IDisposable disp = Window.Content as IDisposable;
                disp?.Dispose();
                Window.Close();
            });

            UIThreadHelper.Instance.Invoke(action);
        }
    }
}
