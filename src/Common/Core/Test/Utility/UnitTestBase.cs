using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Common.Core.Tests.Utility {
    /// <summary>
    /// This is the base class for all unit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class UnitTestBase {
        public TestContext TestContext { get; set; }
    }
}
