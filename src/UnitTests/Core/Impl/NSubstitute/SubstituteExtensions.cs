using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.Core;

namespace Microsoft.UnitTests.Core.NSubstitute
{
    [ExcludeFromCodeCoverage]
    public static class SubstituteExtensions
    {
        public static ConfiguredCall ReturnsAsync<T>(this Task<T> value, T returnThis, params T[] returnThese)
        {
            return value.Returns(Task.FromResult(returnThis), returnThese.Select(Task.FromResult).ToArray());
        }

        public static ConfiguredCall ReturnsAsync(this Task value)
        {
            return value.Returns(Task.CompletedTask);
        }
    }
}