using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Core.Test.Utility
{
    /// <summary>
    /// This is the base class for all unit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UnitTestBase
    {
        public TestContext TestContext { get; set; }
    }
}
