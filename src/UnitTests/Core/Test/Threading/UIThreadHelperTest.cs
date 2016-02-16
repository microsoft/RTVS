using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.UnitTests.Core.Test.Threading {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class UIThreadHelperTest {
        [Test]
        public async Task InvokeAsync() {
            var task = UIThreadHelper.Instance.InvokeAsync(() => Thread.Sleep(500));
            await task;
            task.Status.Should().Be(TaskStatus.RanToCompletion);
        }
    }
}
