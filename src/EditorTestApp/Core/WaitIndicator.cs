using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense.Utilities;

namespace Microsoft.Languages.Editor.Application.Core
{
    [Export(typeof(IWaitIndicator))]
    [ExcludeFromCodeCoverage]
    public sealed class WaitIndicator : IWaitIndicator
    {
        public IWaitContext StartWait(string title, string message, bool allowCancel)
        {
            return new WaitContext();
        }

        public WaitIndicatorResult Wait(string title, string message, bool allowCancel, Action<IWaitContext> action)
        {
            return WaitIndicatorResult.Completed;
        }
    }

    class WaitContext : IWaitContext
    {
        public bool AllowCancel { get; set; }

        public CancellationToken CancellationToken
        {
            get { return CancellationToken.None; }
        }

        public string Message { get; set; }

        public void Dispose()
        {
        }

        public void UpdateProgress()
        {
        }
    }
}
