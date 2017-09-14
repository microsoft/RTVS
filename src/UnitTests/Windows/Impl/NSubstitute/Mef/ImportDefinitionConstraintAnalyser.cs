// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.UnitTests.Core.NSubstitute.Mef
{
    /// <summary>
    /// This class should match System.ComponentModel.Composition.ConstraintServices class
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ImportDefinitionConstraintAnalyser
    {
        private static readonly MethodInfo MetadataItemMethod = typeof(IDictionary<string, object>).GetMethod("get_Item");
        private static readonly MethodInfo MetadataEqualsMethod = typeof(object).GetMethod("Equals", new[] { typeof(object) });

        public static string GetRequiredTypeIdentity(Expression<Func<ExportDefinition, bool>> expression)
        {
            Queue<Expression> queue = new Queue<Expression>();
            queue.Enqueue(expression.Body);
            while (queue.Count > 0)
            {
                Expression candidate = queue.Dequeue();
                string result;
                if (TryGetRequiredTypeIdentity(candidate, out result))
                {
                    return result;
                }

                BinaryExpression binary = candidate as BinaryExpression;
                if (binary != null && binary.NodeType == ExpressionType.AndAlso)
                {
                    queue.Enqueue(binary.Left);
                    queue.Enqueue(binary.Right);
                }
            }

            return null;
        }

        //    typeIdentity.Equals(definition.Metadata[CompositionConstants.ExportTypeIdentityMetadataName]);
        private static bool TryGetRequiredTypeIdentity(Expression candidate, out string typeIdentity)
        {
            typeIdentity = null;
            MethodCallExpression call = candidate as MethodCallExpression;
            if (call == null || !MetadataEqualsMethod.Equals(call.Method) || call.Arguments.Count != 1)
            {
                return false;
            }

            ConstantExpression constant = call.Object as ConstantExpression;
            if (constant == null)
            {
                return false;
            }

            typeIdentity = constant.Value as string;
            call = call.Arguments[0] as MethodCallExpression;
            if (call == null || !MetadataItemMethod.Equals(call.Method) || call.Arguments.Count != 1)
            {
                return false;
            }

            return typeIdentity != null;
        }
    }
}
