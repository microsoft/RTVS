// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client.Test.Stubs {
    public class RSessionCallbackStub : IRSessionCallback {
        public IList<string> ShowErrorMessageCalls { get; } = new List<string>();
        public IList<Tuple<string, MessageButtons>> ShowMessageCalls { get; } = new List<Tuple<string, MessageButtons>>();
        public IList<string> HelpUrlCalls { get; } = new List<string>();
        public IList<Tuple<string, CancellationToken>> PlotCalls { get; } = new List<Tuple<string, CancellationToken>>();
        public IList<Tuple<string, int, CancellationToken>> ReadUserInputCalls { get; } = new List<Tuple<string, int, CancellationToken>>();
        public IList<string> CranUrlFromNameCalls { get; } = new List<string>();

        public Func<string, MessageButtons, Task<MessageButtons>> ShowMessageCallsHandler { get; set; } = (m, b) => Task.FromResult(MessageButtons.OK);
        public Func<string, int, CancellationToken, Task<string>> ReadUserInputHandler { get; set; } = (m, l, ct) => Task.FromResult("\n");
        public Func<string, string> CranUrlFromNameHandler { get; set; } = s => "https://cran.rstudio.com";

        public Task ShowErrorMessage(string message) {
            ShowErrorMessageCalls.Add(message);
            return Task.CompletedTask;
        }

        public Task<MessageButtons> ShowMessage(string message, MessageButtons buttons) {
            ShowMessageCalls.Add(new Tuple<string, MessageButtons>(message, buttons));
            return ShowMessageCallsHandler != null ? ShowMessageCallsHandler(message, buttons) : Task.FromResult(default(MessageButtons));
        }

        public Task ShowHelp(string url) {
            HelpUrlCalls.Add(url);
            return Task.CompletedTask;
        }

        public Task Plot(string filePath, CancellationToken ct) {
            PlotCalls.Add(new Tuple<string, CancellationToken>(filePath, ct));
            return Task.CompletedTask;
        }

        public Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct) {
            ReadUserInputCalls.Add(new Tuple<string, int, CancellationToken>(prompt, maximumLength, ct));
            return ReadUserInputHandler != null ? ReadUserInputHandler(prompt, maximumLength, ct) : Task.FromResult(string.Empty);
        }

        public string CranUrlFromName(string name) {
            CranUrlFromNameCalls.Add(name);
            return CranUrlFromNameHandler != null ? CranUrlFromNameHandler(name) : string.Empty;
        }
    }
}
