// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.SurveyNews;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.SurveyNews {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class SurveyNewsFeedClientTest {
        private readonly SurveyNewsTestFilesFixture _testFiles;

        public SurveyNewsFeedClientTest(SurveyNewsTestFilesFixture fixture) {
            _testFiles = fixture;
        }

        [Test]
        [Category.SurveyNews]
        public async void Invalid() {
            Func<Task> f = () => GetFeed("Invalid.txt");
            await f.ShouldThrowAsync<SurveyNewsFeedException>();
        }

        [Test]
        [Category.SurveyNews]
        public async void Empty() {
            var feed = await GetFeed("Empty.txt");
            feed.Should().BeNull();
        }

        [Test]
        [Category.SurveyNews]
        public async void NoItems() {
            var feed = await GetFeed("NoItems.txt");
            feed.NotVotedUrls.Length.Should().Be(0);
            feed.CannotVoteAgainUrls.Length.Should().Be(0);
            feed.CanVoteAgainUrls.Length.Should().Be(0);
        }

        [Test]
        [Category.SurveyNews]
        public async void Items() {
            var feed = await GetFeed("Items.txt");
            feed.CanVoteAgainUrls.ShouldBeEquivalentTo(new string[] { "http://rtvs.azurewebsites.net/news/1" });
            feed.NotVotedUrls.ShouldBeEquivalentTo(new string[] { "http://rtvs.azurewebsites.net/news/2" });
            feed.CannotVoteAgainUrls.ShouldBeEquivalentTo(new string[] { "http://rtvs.azurewebsites.net/news/3" });
        }

        private async Task<SurveyNewsFeed> GetFeed(string name) {
            var client = new SurveyNewsFeedClient();
            var filePath = _testFiles.GetDestinationPath(name);
            var feed = await client.GetFeedAsync(filePath);
            return feed;
        }
    }
}
