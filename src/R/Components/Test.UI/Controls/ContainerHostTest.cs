using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using static System.Windows.Media.Brushes;

namespace Microsoft.R.Components.Test.UI.Controls {
    public class ContainerHostTest {
        [Test]
        public async Task NineControls() {
            const int idle = 250;
            var disposables = new IDisposable[9];
            var brushes = new[] { LightBlue, Blue, DarkBlue, LightPink, Red, Brown, LightGreen, Green, DarkGreen };
            for (var i = 0; i < 9; i++) {
                var brush = brushes[i];
                var control = UIThreadHelper.Instance.Invoke(() => new Border {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Background = brush
                });

                disposables[i] = await ContainerHost.AddContainer(control);
                await Task.Delay(idle);
            }

            var indexes = new[] {6, 5, 1, 8, 7, 0, 4, 2, 3};
            for (var i = 0; i < 9; i++) {
                disposables[indexes[i]].Dispose();
                await Task.Delay(idle);
            }
        }

        [Test]
        public async Task MaximumControlsLimit() {
            const int idle = 250;
            var tasks = new List<Task<IDisposable>>();
            var brushes = new[] { LightBlue, Blue, DarkBlue, Blue, DarkBlue, LightBlue, DarkBlue, LightBlue, Blue, Yellow, Yellow, Yellow };
            for (var i = 0; i < 12; i++) {
                var brush = brushes[i];
                var control = UIThreadHelper.Instance.Invoke(() => new Border {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Background = brush
                });

                tasks.Add(ContainerHost.AddContainer(control));
                await Task.Delay(idle);
            }

            var tasksRunToCompletion = tasks.Where(t => t.Status == TaskStatus.RanToCompletion).ToList();
            tasksRunToCompletion.Should().HaveCount(9);

            var indexes = new[] {1, 3, 8};
            for (var i = 0; i < 3; i++) {
                tasksRunToCompletion[indexes[i]].Result.Dispose();
                await Task.Delay(idle);
            }

            tasks.Where(t => t.Status == TaskStatus.RanToCompletion).Should().HaveCount(12);
            foreach (var task in tasks) {
                task.Result.Dispose();
            }
        }
    }
}
