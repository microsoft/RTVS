// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.SuggestedActions.Actions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.SuggestedActions {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    [Category.Interactive]
    public class SmartTagsTest {
        private readonly IServiceContainer _services;
        private readonly EditorHostMethodFixture _editorHost;

        public SmartTagsTest(IServiceContainer services, EditorHostMethodFixture editorHost) {
            _services = services;
            _editorHost = editorHost;
        }

        [Test]
        public async Task R_LibrarySuggestedActions() {
            using (var script = await _editorHost.StartScript(_services, " library(base)", RContentTypeDefinition.ContentType)) {

                VerifySession(script, 1, s => { });

                script.DoIdle(200);
                script.MoveRight(2);
                script.DoIdle(200);

                VerifySession(script, 1, s => { });
                script.DoIdle(3000);

                VerifySession(script, 1, s => {
                    var set = s.First();
                    set.Actions.Should().HaveCount(2);
                    var actions = set.Actions.ToArray();
                    actions[0].Should().BeOfType(typeof(InstallPackageSuggestedAction));
                    actions[1].Should().BeOfType(typeof(LoadLibrarySuggestedAction));
                });
            }
        }

        private void VerifySession(IEditorScript script, int expectedSetCount, Action<IEnumerable<SuggestedActionSet>> actionsCheck) {
            script.Invoke(() => {
                var broker = _services.GetService<ILightBulbBroker2>();
                var svc = _services.GetService<ISuggestedActionCategoryRegistryService>();
                broker.CreateSession(svc.AllCodeFixes, script.View, svc.AllCodeFixes);

                var session = script.GetLightBulbSession();
                session.Should().NotBeNull();

                session.Expand();
                session.TryGetSuggestedActionSets(out IEnumerable<SuggestedActionSet> sets);

                sets.Should().NotBeNull();
                sets.Should().HaveCount(1);
                actionsCheck(sets);

                session.Dismiss();
            });
        }
    }
}
