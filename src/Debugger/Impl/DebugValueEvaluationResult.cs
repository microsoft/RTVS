// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    /// <summary>
    /// Describes the result of evaluating an expression that produced a value that is not a promise or an active binding. 
    /// </summary>
    /// <remarks>
    /// Note that most properties of the object will only have a meaningful value if the corresponding <see cref="DebugEvaluationResultFields"/>
    /// flag was specified when producing the result. All properties which were not so requested will be <see langword="null"/>.
    /// </remarks>
    public class DebugValueEvaluationResult : DebugEvaluationResult, IDebugValueEvaluationResult {

        #region IDebugValueEvaluationResult
        /// <summary>
        /// String representation of the value.
        /// </summary>
        public string Representation { get; }

        /// <summary>
        /// The kind of accessor that was used to obtain this <see cref="DebugValueEvaluationResult"/> from its parent.
        /// </summary>
        public DebugChildAccessorKind AccessorKind { get; }
        /// <summary>
        /// Type of the value, as computed by <c>typeof(...)</c>.
        /// </summary>
        public string TypeName { get; }
        /// <summary>
        /// List of classes of the value, as computed by <c>classes(...)</c>.
        /// </summary>
        /// <remarks>
        /// If <see cref="DebugEvaluationResultFields.Classes"/> was not specified, this property will be
        /// <see langword="null"/>, rather than an empty collection.
        /// </remarks>
        public IReadOnlyList<string> Classes { get; }
        /// <summary>
        /// Length of the value, as computed by <c>length(...)</c>.
        /// </summary>
        public int? Length { get; }
        /// <summary>
        /// Number of attributes that this value has, as computed by <c>length(attributes(...))</c>.
        /// </summary>
        public int? AttributeCount { get; }
        /// <summary>
        /// Number of slots that this value has, as computed by <c>length(slotNames(class(...)))</c>.
        /// </summary>
        public int? SlotCount { get; }
        /// <summary>
        /// Number of names that the children of value have, as computed by <c>length(names(...))</c>.
        /// </summary>
        public int? NameCount { get; }
        /// <summary>
        /// Dimensions that this value has, as computed by <c>dim(...)</c>.
        /// </summary>
        /// <remarks>
        /// If <see cref="DebugEvaluationResultFields.Dim"/> was not specified, this property will be
        /// <see langword="null"/>, rather than an empty collection.
        /// </remarks>
        public IReadOnlyList<int> Dim { get; }
        /// <summary>
        /// Various miscellaneous flags describing this value.
        /// </summary>
        public DebugValueEvaluationResultFlags Flags { get; }
        #endregion

        /// <seealso cref="DebugValueEvaluationResultFlags.Atomic"/>
        public bool IsAtomic => Flags.HasFlag(DebugValueEvaluationResultFlags.Atomic);

        /// <seealso cref="DebugValueEvaluationResultFlags.Recursive"/>
        public bool IsRecursive => Flags.HasFlag(DebugValueEvaluationResultFlags.Recursive);

        /// <summary>
        /// Whether this value has any attributes.
        /// </summary>
        public bool HasAttributes => AttributeCount != null && AttributeCount != 0;

        public bool HasSlots => SlotCount != null && SlotCount != 0;

        /// <summary>
        /// <see langword="true"/> if <see cref="GetChildrenAsync"/> will return any items, otherwise <see langword="false"/>.
        /// </summary>
        public bool HasChildren {
            get {
                if (HasSlots) {
                    return true;
                }

                // These have length 1, but are not subsettable, so report no children.
                if (TypeName == "closure" || TypeName == "symbol") {
                    return false;
                }

                if (Length != null) {
                    if (IsAtomic) {
                        // If it is a single-element vector, do not list the element as a child, because it is identical
                        // to the vector itself. However, if the element is named, list it to provide access to the name.
                        if (Length > 1 || (NameCount != null && NameCount != 0)) {
                            return true;
                        }
                    } else {
                        if (Length != 0) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        internal DebugValueEvaluationResult(DebugSession session, string environmentExpression, string expression, string name, JObject json)
            : base(session, environmentExpression, expression, name) {

            Representation = json.Value<string>("repr");
            TypeName = json.Value<string>("type");
            Length = json.Value<int?>("length");
            AttributeCount = json.Value<int?>("attr_count");
            SlotCount = json.Value<int?>("slot_count");
            NameCount = json.Value<int?>("name_count");

            var classes = json.Value<JArray>("classes");
            if (classes != null) {
                Classes = classes.Select(t => t.Value<string>()).ToArray();
            }

            var dim = json.Value<JArray>("dim");
            if (dim != null) {
                Dim = dim.Select(t => t.Value<int>()).ToArray();
            }

            var kind = json.Value<string>("kind");
            switch (kind) {
                case null:
                    AccessorKind = DebugChildAccessorKind.None;
                    break;
                case "[[":
                    AccessorKind = DebugChildAccessorKind.Brackets;
                    break;
                case "$":
                    AccessorKind = DebugChildAccessorKind.Dollar;
                    break;
                case "@":
                    AccessorKind = DebugChildAccessorKind.At;
                    break;
                default:
                    throw new InvalidDataException(Invariant($"Invalid kind '{kind}' in:\n\n{json}"));
            }

            var flags = json.Value<JArray>("flags")?.Select(v => v.Value<string>());
            if (flags != null) {
                foreach (var flag in flags) {
                    switch (flag) {
                        case "atomic":
                            Flags |= DebugValueEvaluationResultFlags.Atomic;
                            break;
                        case "recursive":
                            Flags |= DebugValueEvaluationResultFlags.Recursive;
                            break;
                        case "has_parent_env":
                            Flags |= DebugValueEvaluationResultFlags.HasParentEnvironment;
                            break;
                        default:
                            throw new InvalidDataException(Invariant($"Unrecognized flag '{flag}' in:\n\n{json}"));
                    }
                }
            }
        }

        /// <summary>
        /// Computes the children of this value, and returns a collection of evaluation results describing each child.
        /// See <see cref="DebugSession.EvaluateAsync"/> for the meaning of parameters.
        /// </summary>
        /// <param name="maxCount">If not <see langword="null"/>, return at most that many children.</param>
        /// <remarks>
        /// <para>
        /// Where order matters (e.g. for children of atomic vectors and lists), children are returned in that order.
        /// Otherwise, the order is undefined. If an object has both ordered and unordered children (e.g. it is a vector
        /// with slots), then it is guaranteed that each group is reported as a contiguous sequence within the returned
        /// collection, and order is honored within each group; but groups themselves are not ordered relative to each other.
        /// </para>
        /// <para>
        /// This method does not respect snapshot semantics - that is, it will re-evaluate the expression that produced
        /// its value, and will obtain the most current list of children, rather than the ones that were there when
        /// the result was originally produced. If the context in which this result was produced is no longer available
        /// (e.g. if it was a stack frame that has since went away), the results are undefined.
        /// </para>
        /// </remarks>
        /// <exception cref="RException">Raised if child retrieval fails.</exception>
        public async Task<IReadOnlyList<DebugEvaluationResult>> GetChildrenAsync(
            DebugEvaluationResultFields fields,
            int? maxCount = null,
            string repr = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (EnvironmentExpression == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that does not have an associated environment expression.");
            }
            if (Expression == null) {
                throw new InvalidOperationException("Cannot retrieve children of an evaluation result that does not have an associated expression.");
            }

            if (!HasChildren) {
                return new DebugEvaluationResult[0];
            }

            var call = Invariant($"rtvs:::describe_children({Expression.ToRStringLiteral()}, {EnvironmentExpression}, {fields.ToRVector()}, {maxCount}, {repr})");
            var jChildren = await Session.RSession.EvaluateAsync<JArray>(call, REvaluationKind.Normal, cancellationToken);
            Trace.Assert(
                jChildren.Children().All(t => t is JObject),
                Invariant($"rtvs:::describe_children(): object of objects expected.\n\n{jChildren}"));

            var children = new List<DebugEvaluationResult>();
            foreach (var child in jChildren) {
                var childObject = (JObject)child;
                Trace.Assert(
                    childObject.Count == 1,
                    Invariant($"rtvs:::describe_children(): each object is expected contain one object\n\n"));
                foreach (var kv in childObject) {
                    var name = kv.Key;
                    var jEvalResult = (JObject)kv.Value;
                    var evalResult = Parse(Session, EnvironmentExpression, name, jEvalResult);
                    children.Add(evalResult);
                }
            }

            return children;
        }
    }
}
