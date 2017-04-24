// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace Microsoft.Common.Wpf.Controls {
    public class ExpandCollapseAutomationPeer : ButtonBaseAutomationPeer, IExpandCollapseProvider {
        public ExpandCollapseAutomationPeer(ExpandCollapseButton owner): base(owner) { }

        protected override string GetClassNameCore() => nameof(ExpandCollapseButton);

        public override object GetPattern(PatternInterface pattern) 
            => pattern == PatternInterface.ExpandCollapse ? this : base.GetPattern(pattern);

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;

        public void Expand() {
            if (!IsEnabled())
                throw new ElementNotEnabledException();

            var owner = (ExpandCollapseButton)Owner;
            owner.IsExpanded = true;
        }

        public void Collapse() {
            if (!IsEnabled())
                throw new ElementNotEnabledException();

            var owner = (ExpandCollapseButton)Owner;
            owner.IsExpanded = false;
        }

        public ExpandCollapseState ExpandCollapseState {
            get {
                var owner = (ExpandCollapseButton)Owner;
                return owner.IsExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
            }
        }

        public void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue) {
            RaisePropertyChangedEvent(
                ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
                newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);

            var name = GetName();
            RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, name, name);
        }
    }
}