using System;
using System.Diagnostics.CodeAnalysis;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [TraitDiscoverer("Microsoft.UnitTests.Core.XUnit.CategoryTraitDiscoverer", "Microsoft.UnitTests.Core")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    [ExcludeFromCodeCoverage]
    public class CategoryAttribute : Attribute, ITraitAttribute {
        public string[] Categories { get; }

        public CategoryAttribute(params string[] categories) {
            Categories = categories;
        }
    }
}