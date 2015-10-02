using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Common.Core;

namespace Microsoft.UnitTests.Core.NSubstitute.Mef
{
    public partial class MefSubstituteBuilder
    {
        private readonly HashSet<Type> _types;
        private readonly CompositionBatch _batch;

        public MefSubstituteBuilder()
        {
            _batch = new CompositionBatch();
            _types = new HashSet<Type>();
        }

        public MefSubstituteBuilder Add<T>()
        {
            return AddType(typeof (T));
        }

        public MefSubstituteBuilder AddWithNested<T>()
        {
            return AddTypeWithNested(typeof (T));
        }

        public MefSubstituteBuilder AddTypeWithNested(Type type)
        {
            AddType(type);

            var nestedTypesToAdd = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(t => t.GetCustomAttributes(typeof(ExportAttribute), inherit: true).Any());

            foreach (var nestedType in nestedTypesToAdd)
            {
                AddTypeWithNested(nestedType);
            }

            return this;
        }

        public MefSubstituteBuilder AddType(Type type)
        {
            _types.Add(type);
            return this;
        }

        public MefSubstituteBuilder AddValue<T>(T value)
        {
            return AddValue<T, T>(value);
        }

        public MefSubstituteBuilder AddValue<TContract, T>(T value)
            where T : TContract
        {
            string contractName = AttributedModelServices.GetContractName(typeof(TContract));
            AddValue(contractName, value);
            return this;
        }

        public MefSubstituteBuilder AddValue<T>(string contractName, T value)
        {
            ExportDefinition contractExport = CreateExportDefinition(contractName, typeof(T));
            ComposablePartDefinition partDefinition = CreatePartDefinition(Enumerable.Empty<ImportDefinition>(), contractExport, typeof(T));
            ComposablePart part = AttributedModelServices.CreatePart(partDefinition, value);

            _batch.AddPart(part);
            return this;
        }

        private ComposablePartDefinition CreatePartDefinition(IEnumerable<ImportDefinition> ctorImports, ExportDefinition contractExport, Type type)
        {
            ComposablePartDefinition originalPartDefinition = AttributedModelServices.CreatePartDefinition(type, null);
            if (originalPartDefinition == null)
            {
                throw new InvalidOperationException();
            }

            IList<ImportDefinition> imports = originalPartDefinition.ImportDefinitions
                .Where(idef => !ReflectionModelServices.IsImportingParameter(idef))
                .Concat(ctorImports)
                .ToList();

            IList<ExportDefinition> exports = originalPartDefinition.ExportDefinitions
                .Append(contractExport)
                .ToList();

            IDictionary<string, object> metadata = originalPartDefinition.Metadata;

            return CreatePartDefinition(type, imports, exports, metadata);
        }

        private static ComposablePartDefinition CreatePartDefinition(Type type, IList<ImportDefinition> imports, IList<ExportDefinition> exports, IDictionary<string, object> metadata)
        {
            return ReflectionModelServices.CreatePartDefinition(
                new Lazy<Type>(() => type, LazyThreadSafetyMode.PublicationOnly),
                false,
                new Lazy<IEnumerable<ImportDefinition>>(() => imports),
                new Lazy<IEnumerable<ExportDefinition>>(() => exports),
                new Lazy<IDictionary<string, object>>(() => metadata),
                null);
        }

        private static ExportDefinition CreateExportDefinition(string contractName, Type type)
        {
            LazyMemberInfo memberInfo = new LazyMemberInfo(MemberTypes.TypeInfo, type);

            Lazy<IDictionary<string, object>> metadata = new Lazy<IDictionary<string, object>>(() =>
            {
                string typeIdentity = AttributedModelServices.GetTypeIdentity(type);
                return new Dictionary<string, object>
                {
                    {CompositionConstants.ExportTypeIdentityMetadataName, typeIdentity}
                };
            });

            ExportDefinition exportDefinition = ReflectionModelServices.CreateExportDefinition(memberInfo, contractName, metadata, null);
            return exportDefinition;
        }

        public MefSubstitute Build()
        {
            NSubstituteCompositionContainer compositionContainer = new NSubstituteCompositionContainer(new TypeCatalog(_types));
            compositionContainer.Compose(_batch);
            return MefSubstitute.CreateFrom(compositionContainer);
        }

    }
}