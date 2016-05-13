// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions.Equivalency;
using Microsoft.R.ExecutionTracing;
using Microsoft.R.StackTracing;
using NSubstitute;

namespace Microsoft.R.Host.Client.Test {
    internal class TracebackBuilder : IReadOnlyList<IRStackFrame> {
        public struct AnyType {
            public static implicit operator string (AnyType any) => "<ANY>";
            public static implicit operator int (AnyType any) => -1;
        }
        public static readonly AnyType Any = default(AnyType);

        private readonly List<IRStackFrame> _frames = new List<IRStackFrame>();
        private Func<EquivalencyAssertionOptions<IRStackFrame[]>, EquivalencyAssertionOptions<IRStackFrame[]>> _config = options => options;

        public int Count {
            get {
                return _frames.Count;
            }
        }

        public IRStackFrame this[int index] {
            get {
                return _frames[index];
            }
        }

        public IEnumerator<IRStackFrame> GetEnumerator() {
            return _frames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _frames.GetEnumerator();
        }

        public EquivalencyAssertionOptions<IRStackFrame[]> Configure(EquivalencyAssertionOptions<IRStackFrame[]> options) {
            return _config(options);
        }

        public void Add(string fileName, int? lineNumber, string call, string environmentName) {
            string itemPath = "[" + _frames.Count + "].";
            var frame = Substitute.For<IRStackFrame>();
            if (fileName != Any) {
                frame.FileName.Returns(fileName);
            }
            if (lineNumber != Any) {
                frame.LineNumber.Returns(lineNumber);
            }
            if (call != Any) {
                frame.Call.Returns(call);
            }
            if (environmentName != Any) {
                frame.EnvironmentName.Returns(environmentName);
            }
            _frames.Add(frame);

            var oldConfig = _config;
            _config = options => {
                options = oldConfig(options);
                if (fileName != Any) {
                    options = options.Including(ctx => ctx.SelectedMemberPath == itemPath + nameof(IRStackFrame.FileName));
                }
                if (lineNumber != Any) {
                    options = options.Including(ctx => ctx.SelectedMemberPath == itemPath + nameof(IRStackFrame.LineNumber));
                }
                if (call != Any) {
                    options = options.Including(ctx => ctx.SelectedMemberPath == itemPath + nameof(IRStackFrame.Call));
                }
                if (environmentName != Any) {
                    options = options.Including(ctx => ctx.SelectedMemberPath == itemPath + nameof(IRStackFrame.EnvironmentName));
                }
                return options;
            };
        }

        public void Add(string fileName, int lineNumber, string call) {
            Add(fileName, lineNumber, call, Any);
        }

        public void Add(string fileName, int lineNumber) {
            Add(fileName, lineNumber, Any);
        }

        public void Add(SourceFile sourceFile, int lineNumber, string call, string environmentName) {
            Add(sourceFile.FilePath, lineNumber, call, environmentName);
        }

        public void Add(SourceFile sourceFile, int lineNumber, string call) {
            Add(sourceFile.FilePath, lineNumber, call);
        }

        public void Add(SourceFile sourceFile, int lineNumber) {
            Add(sourceFile.FilePath, lineNumber);
        }

        public void Add(RSourceLocation location, int offset, string call) {
            Add(location.FileName, location.LineNumber + offset, call);
        }

        public void Add(RSourceLocation location, int offset = 0) {
            Add(location.FileName, location.LineNumber + offset, Any);
        }

        public void Add(RSourceLocation location, string call) {
            Add(location.FileName, location.LineNumber, call);
        }
    }
}
