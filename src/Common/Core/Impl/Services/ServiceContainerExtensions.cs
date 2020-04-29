// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.Common.Core.Services {
    public static class ServiceContainerExtensions {
        public static IServiceManager Extend(this IServiceContainer serviceContainer) => new ServiceManagerExtension(serviceContainer); 

        public static IActionLog Log(this IServiceContainer sc) => sc.GetService<IActionLog>();
        public static IFileSystem FileSystem(this IServiceContainer sc) => sc.GetService<IFileSystem>();
        public static IProcessServices Process(this IServiceContainer sc) => sc.GetService<IProcessServices>();
        public static ISecurityService Security(this IServiceContainer sc) => sc.GetService<ISecurityService>();
        public static ITaskService Tasks(this IServiceContainer sc) => sc.GetService<ITaskService>();
        public static IUIService UI(this IServiceContainer sc) => sc.GetService<IUIService>();
        public static IMainThread MainThread(this IServiceContainer sc) => sc.GetService<IMainThread>();
        public static IIdleTimeService IdleTime(this IServiceContainer sc) => sc.GetService<IIdleTimeService>();

        /// <summary>
        /// Switches to UI thread asynchonously and then displays the message
        /// </summary>
        public static async Task ShowErrorMessageAsync(this IServiceContainer sc, string message, CancellationToken cancellationToken = default(CancellationToken)) {
            await sc.MainThread().SwitchToAsync(cancellationToken);
            sc.UI().ShowErrorMessage(message);
        }

        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        public static void ShowErrorMessage(this IServiceContainer sc, string message)
            => sc.UI().ShowErrorMessage(message);

        /// <summary>
        /// Shows the context menu with the specified command ID at the specified location
        /// </summary>
        public static void ShowContextMenu(this IServiceContainer sc, CommandId commandId, int x, int y, object commandTarget = null)
            => sc.UI().ShowContextMenu(commandId, x, y, commandTarget);

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        public static MessageButtons ShowMessage(this IServiceContainer sc, string message, MessageButtons buttons, MessageType messageType = MessageType.Information)
            => sc.UI().ShowMessage(message, buttons, messageType);

        public static T CreateInstance<T>(this IServiceContainer s) where T : class => InstanceFactory<T>.New(s);

        private static class InstanceFactory<T> where T : class {
            private static readonly Func<IServiceContainer, T> _factory;

            static InstanceFactory() => _factory = CreateFactory()
                ?? throw new InvalidOperationException($"Type {typeof(T)} should have either default constructor or constructor that accepts IServiceContainer");

            public static T New(IServiceContainer services) => _factory(services);

            private static Func<IServiceContainer, T> CreateFactory() {
                var constructors = typeof(T).GetTypeInfo().DeclaredConstructors
                    .Where(c => c.IsPublic)
                    .ToList();

                foreach (var constructor in constructors) {
                    var parameters = constructor.GetParameters();
                    if (parameters.Length == 1 && typeof(IServiceContainer) == parameters[0].ParameterType) {
                        var parameter = Expression.Parameter(typeof(IServiceContainer), "services");
                        return Expression
                            .Lambda<Func<IServiceContainer, T>>(Expression.New(constructor, parameter), parameter)
                            .Compile();
                    }
                }

                foreach (var constructor in constructors) {
                    if (constructor.GetParameters().Length == 0) {
                        var parameter = Expression.Parameter(typeof(IServiceContainer), "services");
                        return Expression
                            .Lambda<Func<IServiceContainer, T>>(Expression.New(constructor), parameter)
                            .Compile();
                    }
                }

                return null;
            }
        }
    }
}
