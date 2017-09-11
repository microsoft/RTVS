// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if DESKTOP
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Xunit.Sdk;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.UnitTests.Core.XUnit.MessageBusInjections
{
    [ExcludeFromCodeCoverage]
    internal class VerifyGlobalProviderMessageBusInjection : ITestCaseAfterStartingBeforeFinishedInjection
    {
        private IServiceProvider _oleServiceProvider;

        public bool AfterStarting(IMessageBus messageBus, TestCaseStarting message)
        {
            _oleServiceProvider = UIThreadHelper.Instance.Invoke(() => GetOleServiceProvider());
            return true;
        }

        public bool BeforeFinished(IMessageBus messageBus, TestCaseFinished message)
        {
            if (UIThreadHelper.Instance.Invoke(() => ThreadHelper.CheckAccess()))
            {
                return true;
            }

            // Try to restore service provider and original thread.
            // Terminate test execution if not succeeded
            try
            {
                UIThreadHelper.Instance.Invoke(() => ServiceProvider.CreateFromSetSite(_oleServiceProvider));
            }
            catch (Exception ex)
            {
                messageBus.QueueMessage(new TestCaseCleanupFailure(message.TestCase, ex));
                return false;
            }

            InvalidOperationException exception = new InvalidOperationException("Test changes or accesses ServiceProvider.GlobalProvider from non-UI thread. Try annotate it with ThreadType.UI");
            return messageBus.QueueMessage(new TestCaseCleanupFailure(message.TestCase, exception));
        }

        private static IServiceProvider GetOleServiceProvider()
        {
            object service;
            var result = ServiceProvider.GlobalProvider.QueryService(typeof(IServiceProvider).GUID, out service);
            return result == VSConstants.S_OK ? (IServiceProvider)service : null;
        }
    }
}
#endif
