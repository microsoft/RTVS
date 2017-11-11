// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.R.Package.Windows;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    internal class ViewContainerToolWindowFactory {
        private readonly Dictionary<Guid, IViewContainerToolWindowFactory> _factories = new Dictionary<Guid, IViewContainerToolWindowFactory>();
        
        public ViewContainerToolWindowFactory Register<TToolWindow, TView>(IServiceContainer services)
            where TView : UserControl
            where TToolWindow : ViewContainerToolWindow, new() {

            _factories[typeof(TToolWindow).GUID] = new Factory<TToolWindow, TView>(services);
            return this;
        }

        public TToolWindow GetOrCreate<TToolWindow>(int instanceId) where TToolWindow : ViewContainerToolWindow
            => (TToolWindow)_factories[typeof(TToolWindow).GUID].GetOrCreate(instanceId);

        public ViewContainerToolWindow GetOrCreate(Guid toolWindowGuid, int instanceId) 
            => _factories.TryGetValue(toolWindowGuid, out IViewContainerToolWindowFactory factory) ? factory.GetOrCreate(instanceId) : null;

        private interface IViewContainerToolWindowFactory {
            ViewContainerToolWindow GetOrCreate(int instanceId);
        }

        private class Factory<TToolWindow, TView> : ToolWindowPaneFactory<TToolWindow>, IViewContainerToolWindowFactory
            where TView : UserControl
            where TToolWindow : ViewContainerToolWindow, new() {

            public Factory(IServiceContainer services) : base(services) { }

            public ViewContainerToolWindow GetOrCreate(int instanceId) => GetOrCreate(instanceId, CreateToolWindow);

            private TToolWindow CreateToolWindow() {
                var view = Services.CreateInstance<TView>();
                var toolWindow = new TToolWindow {
                    Content = view,
                    ViewModel = view.DataContext,
                    Services = Services
                };
                ViewContainerToolWindow.SetToolWindow(view, toolWindow);
                return toolWindow;
            }
        }
    }
}