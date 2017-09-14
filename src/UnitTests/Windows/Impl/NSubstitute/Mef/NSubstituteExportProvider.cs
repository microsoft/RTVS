// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using NSubstitute;

namespace Microsoft.UnitTests.Core.NSubstitute.Mef
{
    [ExcludeFromCodeCoverage]
    public class NSubstituteExportProvider : ExportProvider
    {
        private readonly IDictionary<string, ExportSource> _exactlyOneExports = new Dictionary<string, ExportSource>();

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            if (definition.Cardinality != ImportCardinality.ExactlyOne)
            {
                return Enumerable.Empty<Export>();
            }

            ExportSource exportSource;
            if (_exactlyOneExports.TryGetValue(definition.ContractName, out exportSource))
            {
                AddMemberType(exportSource, definition);
                return new[] { exportSource.Export };
            }

            string typeName = ImportDefinitionConstraintAnalyser.GetRequiredTypeIdentity(definition.Constraint);
            Type type = GetType(typeName);
            if (!CanHandleType(type))
            {
                return Enumerable.Empty<Export>();
            }

            exportSource = new ExportSource(definition.ContractName, definition.Metadata);
            exportSource.AddType(type);
            AddMemberType(exportSource, definition);

            _exactlyOneExports[definition.ContractName] = exportSource;
            return new[] { exportSource.Export };
        }

        private static void AddMemberType(ExportSource exportSource, ImportDefinition definition)
        {
            try
            {
                LazyMemberInfo member = ReflectionModelServices.GetImportingMember(definition);
                switch (member.MemberType)
                {
                    case MemberTypes.Property:
                        MemberInfo[] accessors = member.GetAccessors();
                        MethodInfo setter = accessors.OfType<MethodInfo>().Single(m => m.ReturnType == typeof (void));
                        Type type = setter.GetParameters()[0].ParameterType;
                        exportSource.AddType(type);
                        return;
                    default:
                        return;
                }
            }
            catch (ArgumentException)
            {
                // There is no TryGetImportingMember method, so if definition is of the wrong type, we just swallow exception
            }
        }

        private static bool CanHandleType(Type type)
        {
            if (type.IsValueType)
            {
                return true;
            }

            if (type.IsClass)
            {
                if (type.GetConstructors().Any(ci => !ci.GetParameters().Any()))
                {
                    return true;
                }

                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Types without default constructors aren't supported. Provide instance of type {0} explicitly", type.FullName));
            }

            if (type.IsGenericTypeDefinition)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Generic type definition can't be substituted. Provide explicit stub for {0}", type.FullName));
            }

            if (type.IsConstructedGenericType && !type.GetGenericArguments().All(t => t.IsPublic))
            {
                // NSubstitute can't construct proxy for an interface with non-public arguments,
                // but it is possible that generic definition interface will be satisfied, so skip it for now
                return false;
            }

            return true;
        }

        private static Type GetType(string fullName)
        {
            int openBraceIndex = fullName.IndexOf("(", StringComparison.Ordinal);
            if (openBraceIndex == -1)
            {
                return GetCurrentAppDomainType(fullName);
            }

            int closeBraceIndex = fullName.LastIndexOf(")", StringComparison.Ordinal);

            // support only one generic argument for now
            Type[] genericTypeArguments = new []{ fullName.Substring(openBraceIndex + 1, closeBraceIndex - openBraceIndex - 1) }
                .Select(GetCurrentAppDomainType)
                .ToArray();

            Type type = GetCurrentAppDomainType(fullName.Substring(0, openBraceIndex) + "`" + genericTypeArguments.Length);
            return type.MakeGenericType(genericTypeArguments);
        }

        private static Type GetCurrentAppDomainType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(fullName)).FirstOrDefault(t => t != null);
        }

        [ExcludeFromCodeCoverage]
        private class ExportSource
        {
            public Export Export { get; }
            private Lazy<object> LazyValueGetter { get; }
            private ISet<Type> Types { get; }

            public ExportSource(string contractName, IDictionary<string, object> metadata)
            {
                Types = new HashSet<Type>();
                LazyValueGetter = new Lazy<object>(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication);
                Export = new Export(contractName, metadata, () => LazyValueGetter.Value);
            }

            public void AddType(Type type)
            {
                Types.Add(type);
            }

            private object CreateInstance()
            {

                if (Types.Any(t => t.IsValueType || (t.IsClass && !t.IsPublic)))
                {
                    if (Types.Count != 1)
                    {
                        throw new InvalidOperationException();
                    }

                    return Activator.CreateInstance(Types.Single());
                }

                return Substitute.For(Types.ToArray(), new object[0]);
            }
        }
    }
}