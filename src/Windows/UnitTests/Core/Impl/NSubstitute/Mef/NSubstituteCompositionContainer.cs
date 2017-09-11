// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Common.Core;

namespace Microsoft.UnitTests.Core.NSubstitute.Mef
{
    [ExcludeFromCodeCoverage]
    public class NSubstituteCompositionContainer : CompositionContainer
    {
        private ExportProvider SubstituteProvider => Providers.Last();

        public NSubstituteCompositionContainer()
            : base(new NSubstituteExportProvider())
        {
        }

        public NSubstituteCompositionContainer(params ExportProvider[] providers)
            : base (AddSubstituteProvider(providers))
        {
        }

        public NSubstituteCompositionContainer(CompositionOptions compositionOptions, params ExportProvider[] providers)
            : base(compositionOptions, AddSubstituteProvider(providers))
        {
        }

        public NSubstituteCompositionContainer(ComposablePartCatalog catalog, params ExportProvider[] providers)
            : base(catalog, AddSubstituteProvider(providers))
        {
        }

        private static ExportProvider[] AddSubstituteProvider(ExportProvider[] providers)
        {
            if (providers.Length == 0)
            {
                return new ExportProvider[] {new NSubstituteExportProvider()};
            }

            ExportProvider[] newProviders = new ExportProvider[providers.Length + 1];
            providers.CopyTo(newProviders, 0);
            newProviders[newProviders.Length - 1] = new NSubstituteExportProvider();
            return newProviders;
        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            List<Export> exports = base.GetExportsCore(definition, atomicComposition).AsList();

            object source;
            if (definition.Cardinality == ImportCardinality.ExactlyOne
                && exports.Count == 0
                && definition.Metadata.TryGetValue(CompositionConstants.ImportSourceMetadataName, out source)
                && (ImportSource)source == ImportSource.Local)
            {
                return SubstituteProvider.GetExports(definition, atomicComposition);
            }

            return exports;
        }
    }
}