// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Utility;
using Microsoft.Languages.Editor.Shell;

namespace Microsoft.Languages.Editor.Tasks {
    /// <summary>
    /// Action that should be executed on next idle after certain amount of milliseconds
    /// </summary>
    public class IdleTimeAction {
        private static readonly ConcurrentDictionary<object, IdleTimeAction> _idleActions = new ConcurrentDictionary<object, IdleTimeAction>();

        private readonly Action _action;
        private readonly int _delay;
        private readonly ICoreShell _shell;
        private readonly object _tag;
        private volatile bool _connectedToIdle;
        private DateTime _idleConnectTime;


        /// <summary>
        /// Create delayed idle time action
        /// </summary>
        /// <param name="action">Action to execute on idle</param>
        /// <param name="delay">Minimum number of milliseconds to wait before executing the action</param>
        /// <param name="tag">Object that uniquely identifies the action. Typically creator object.</param>
        /// <param name="shell"></param>
        public static void Create(Action action, int delay, object tag, ICoreShell shell) {
            IdleTimeAction existingAction;

            if (!_idleActions.TryGetValue(tag, out existingAction)) {
                existingAction = new IdleTimeAction(action, delay, tag, shell);
                _idleActions[tag] = existingAction;
            }
        }

        /// <summary>
        /// Cancels idle time action. Has no effect if action has already been executed.
        /// </summary>
        /// <param name="tag">Tag identifying the action to cancel</param>
        public static void Cancel(object tag) {
            IdleTimeAction idleTimeAction;

            if (_idleActions.TryGetValue(tag, out idleTimeAction)) {
                idleTimeAction.DisconnectFromIdle();
                _idleActions.TryRemove(tag, out idleTimeAction);
            }
        }

        private IdleTimeAction(Action action, int delay, object tag, ICoreShell shell) {
            _action = action;
            _delay = delay;
            _tag = tag;
            _shell = shell;

            ConnectToIdle();
        }

        void OnIdle(object sender, EventArgs e) {
            if (TimeUtility.MillisecondsSinceUtc(_idleConnectTime) > _delay) {
                DisconnectFromIdle();
                _action();

                IdleTimeAction idleTimeAction;
                _idleActions.TryRemove(_tag, out idleTimeAction);
            }
        }

        void ConnectToIdle() {
            if (!_connectedToIdle) {
                _shell.Idle += OnIdle;

                _idleConnectTime = DateTime.UtcNow;
                _connectedToIdle = true;
            }
        }

        void DisconnectFromIdle() {
            if (_connectedToIdle) {
                _shell.Idle -= OnIdle;
                _connectedToIdle = false;
            }
        }
    }
}
