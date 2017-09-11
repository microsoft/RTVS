// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit.MessageBusInjections {
    internal class ExecuteBeforeAfterAttributesMessageBusInjection : ITestAfterStartingBeforeFinishedInjection
    {
        private readonly Stack<BeforeCtorAfterDisposeAttribute> _attributesStack = new Stack<BeforeCtorAfterDisposeAttribute>();
        private readonly List<BeforeCtorAfterDisposeAttribute> _beforeAfterAttributes;
        private readonly List<Exception> _exceptions = new List<Exception>();
        private ExecutionTimer _timer;
        
        public ExecuteBeforeAfterAttributesMessageBusInjection(IMethodInfo method, ITypeInfo typeInfo)
        {
            _beforeAfterAttributes = typeInfo.ToRuntimeType().GetTypeInfo().GetCustomAttributes(typeof(BeforeCtorAfterDisposeAttribute))
                .Concat(method.ToRuntimeMethod().GetCustomAttributes(typeof(BeforeCtorAfterDisposeAttribute)))
                .Cast<BeforeCtorAfterDisposeAttribute>()
                .ToList();
        }

        public bool AfterStarting(IMessageBus messageBus, TestStarting message)
        {
            _timer = new ExecutionTimer();
            var testMethod = message.TestMethod.Method.ToRuntimeMethod();

            foreach (var beforeAfterTestCaseAttribute in _beforeAfterAttributes)
            {
                try
                {
                    _timer.Aggregate(() => beforeAfterTestCaseAttribute.Before(testMethod));
                    _attributesStack.Push(beforeAfterTestCaseAttribute);
                }
                catch (Exception e)
                {
                    _exceptions.Add(e);
                }
            }

            return true;
        }

        public bool BeforeFinished(IMessageBus messageBus, TestFinished message)
        {
            var testMethod = message.TestMethod.Method.ToRuntimeMethod();

            foreach (BeforeCtorAfterDisposeAttribute beforeAfterTestCaseAttribute in _attributesStack)
            {
                try
                {
                    _timer.Aggregate(() => beforeAfterTestCaseAttribute.After(testMethod));
                }
                catch (Exception e)
                {
                    _exceptions.Add(e);
                }
            }

            if (_exceptions.Any())
            {
                return messageBus.QueueMessage(new TestCleanupFailure(message.Test, new AggregateException(_exceptions)));
            }

            return true;
        }
    }
}