// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Category.Interactive]
    [Collection(CollectionNames.NonParallel)]
    public class SignatureTest : IAsyncLifetime {
        private readonly IServiceContainer _services;
        private readonly EditorHostMethodFixture _editorHost;
        private readonly IRSessionProvider _sessionProvider;

        public SignatureTest(IServiceContainer services, EditorHostMethodFixture editorHost) {
            _services = services;
            _sessionProvider = UIThreadHelper.Instance.Invoke(() => _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate()).RSessions;
            _editorHost = editorHost;
        }

        public Task InitializeAsync() => _sessionProvider.TrySwitchBrokerAsync(nameof(SignatureTest));

        public Task DisposeAsync() => Task.CompletedTask;

        [Test]
        [Category.Interactive]
        public async Task R_SignatureParametersMatch() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType, _sessionProvider)) {
                await _editorHost.FunctionIndex.GetPackageNameAsync("lm");

                script.Type("x <- lm(");
                script.DoIdle(2000);

                var session = script.GetSignatureSession();
                session.Should().NotBeNull();
                var parameter = session.SelectedSignature.CurrentParameter;
                parameter.Should().NotBeNull();

                parameter.Name.Should().Be("formula");

                script.Type("sub");
                script.DoIdle(500);
                script.Type("{TAB}");
                script.DoIdle(1000);

                parameter = session.SelectedSignature.CurrentParameter;
                parameter.Name.Should().Be("subset");

                var actual = script.EditorText;
                actual.Should().Be("x <- lm(subset = )");
            }
        }

        [Test]
        public async Task R_SignatureSessionNavigation() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType, _sessionProvider)) {
                await _editorHost.FunctionIndex.GetPackageNameAsync("lm");

                script.Type("x <- lm(subset = a, sing");
                script.DoIdle(1000);
                script.Type("{TAB}");
                script.DoIdle(1000);

                var session = script.GetSignatureSession();
                session.Should().NotBeNull();

                script.DoIdle(200);
                var parameter = session.SelectedSignature.CurrentParameter;
                parameter.Should().NotBeNull();
                parameter.Name.Should().Be("singular.ok");

                script.MoveLeft(17);
                script.DoIdle(200);
                parameter = session.SelectedSignature.CurrentParameter;
                parameter.Name.Should().Be("subset");

                script.MoveRight(3);
                script.DoIdle(200);
                parameter = session.SelectedSignature.CurrentParameter;
                parameter.Name.Should().Be("singular.ok");
            }
        }

        [Test]
        public async Task R_EqualsCompletion01() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType, _sessionProvider)) {
                await _editorHost.FunctionIndex.GetPackageNameAsync("addmargins");

                script.DoIdle(100);
                script.Type("addmargins(Fu");
                script.DoIdle(300);
                script.Type("=");
                script.DoIdle(300);

                var expected = "addmargins(FUN = )";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }
    }
}
