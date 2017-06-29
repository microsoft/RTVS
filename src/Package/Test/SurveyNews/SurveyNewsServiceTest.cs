// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Components.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.SurveyNews;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.SurveyNews {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class SurveyNewsServiceTest {
        [CompositeTest]
        [Category.SurveyNews]
        [InlineData(SurveyNewsPolicy.Disabled)]
        [InlineData(SurveyNewsPolicy.CheckOnceDay)]
        [InlineData(SurveyNewsPolicy.CheckOnceWeek)]
        [InlineData(SurveyNewsPolicy.CheckOnceMonth)]
        public async void ItemPageWhenItemAvailableAndForceCheck(SurveyNewsPolicy policy) {
            // When the user initiates the check via the menu command,
            // and there are available 'not voted' items in the feed,
            // then the first available item will show.
            await CheckSurveyNews(
                MockFeeds.Items,
                policy,
                LastChangedDates.Get(LastChangedDates.Never),
                true,
                MockUrls.NotVoted1,
                UpdatedLastChangedDates.Get(UpdatedLastChangedDates.Now)
            );
        }

        [CompositeTest]
        [Category.SurveyNews]
        [InlineData(SurveyNewsPolicy.Disabled)]
        [InlineData(SurveyNewsPolicy.CheckOnceDay)]
        [InlineData(SurveyNewsPolicy.CheckOnceWeek)]
        [InlineData(SurveyNewsPolicy.CheckOnceMonth)]
        public async void IndexPageWhenNoItemAvailableAndForceCheck(SurveyNewsPolicy policy) {
            // When the user initiates the check via the menu command,
            // and there are no available 'not voted' items in the feed,
            // then the index will show.
            await CheckSurveyNews(
                MockFeeds.NoItems,
                policy,
                LastChangedDates.Get(LastChangedDates.Never),
                true,
                MockUrls.Index,
                UpdatedLastChangedDates.Get(UpdatedLastChangedDates.Now)
            );
        }

        [Test]
        [Category.SurveyNews]
        public async void ErrorPageWhenFeedNotFoundAndForceCheck() {
            // When the user initiates the check via the menu command,
            // and the feed cannot be retrieved,
            // then an error will show.
            await CheckSurveyNews(
                MockFeeds.NotFound,
                SurveyNewsPolicy.Disabled,
                LastChangedDates.Get(LastChangedDates.Never),
                true,
                MockUrls.CannotConnect,
                UpdatedLastChangedDates.Get(UpdatedLastChangedDates.Now)
            );
        }

        [CompositeTest]
        [Category.SurveyNews]
        [InlineData(SurveyNewsPolicy.Disabled, LastChangedDates.MoreThanADay)]
        [InlineData(SurveyNewsPolicy.Disabled, LastChangedDates.MoreThanAWeek)]
        [InlineData(SurveyNewsPolicy.Disabled, LastChangedDates.MoreThanAMonth)]
        [InlineData(SurveyNewsPolicy.CheckOnceDay, LastChangedDates.LessThanADay)]
        [InlineData(SurveyNewsPolicy.CheckOnceWeek, LastChangedDates.LessThanADay)]
        [InlineData(SurveyNewsPolicy.CheckOnceWeek, LastChangedDates.LessThanAWeek)]
        [InlineData(SurveyNewsPolicy.CheckOnceMonth, LastChangedDates.LessThanADay)]
        [InlineData(SurveyNewsPolicy.CheckOnceMonth, LastChangedDates.LessThanAWeek)]
        [InlineData(SurveyNewsPolicy.CheckOnceMonth, LastChangedDates.LessThanAMonth)]
        public async void NoPageWhenPolicyAndLastCheckDateRejected(SurveyNewsPolicy policy, int lastCheckedDate) {
            // When the check is done on project load in the background,
            // and the last check date and the policy determine the feed should NOT be retrieved,
            // then nothing will show.
            await CheckSurveyNews(
                MockFeeds.Items,
                policy,
                LastChangedDates.Get(lastCheckedDate),
                false,
                MockUrls.None,
                UpdatedLastChangedDates.Get(UpdatedLastChangedDates.Unchanged)
            );
        }

        [CompositeTest]
        [Category.SurveyNews]
        [InlineData(SurveyNewsPolicy.CheckOnceDay, LastChangedDates.MoreThanADay)]
        [InlineData(SurveyNewsPolicy.CheckOnceDay, LastChangedDates.MoreThanAWeek)]
        [InlineData(SurveyNewsPolicy.CheckOnceDay, LastChangedDates.MoreThanAMonth)]
        [InlineData(SurveyNewsPolicy.CheckOnceWeek, LastChangedDates.MoreThanAWeek)]
        [InlineData(SurveyNewsPolicy.CheckOnceWeek, LastChangedDates.MoreThanAMonth)]
        [InlineData(SurveyNewsPolicy.CheckOnceMonth, LastChangedDates.MoreThanAMonth)]
        public async void ItemPageWhenPolicyAndLastCheckDateAccepted(SurveyNewsPolicy policy, int lastCheckedDate) {
            // When the check is done on project load in the background,
            // and the last check date and the policy determine the feed should be retrieved,
            // and there are available 'not voted' items in the feed,
            // then the first available item will show.
            await CheckSurveyNews(
                MockFeeds.Items,
                policy,
                LastChangedDates.Get(lastCheckedDate),
                false,
                MockUrls.NotVoted1,
                UpdatedLastChangedDates.Get(UpdatedLastChangedDates.Now)
            );
        }

        [CompositeTest]
        [Category.SurveyNews]
        [InlineData(SurveyNewsPolicy.Disabled, MockUrls.None, UpdatedLastChangedDates.FourDaysAgo)]
        [InlineData(SurveyNewsPolicy.CheckOnceDay, MockUrls.NotVoted1, UpdatedLastChangedDates.Now)]
        [InlineData(SurveyNewsPolicy.CheckOnceWeek, MockUrls.None, UpdatedLastChangedDates.FourDaysAgo)]
        [InlineData(SurveyNewsPolicy.CheckOnceMonth, MockUrls.None, UpdatedLastChangedDates.FourDaysAgo)]
        public async void ItemPageBasedOnPolicyAndNeverCheckedBefore(SurveyNewsPolicy policy, string expectedUrl, int expectedUpdatedLastCheckedDate) {
            // When the check is done on project load in the background,
            // and the last check date has never been set,
            // the last check date is automatically set to 4 days ago,
            // the policy is evaluated against the new last check date,
            // then the first available item will only show if once a day, otherwise nothing will show.
            await CheckSurveyNews(
                MockFeeds.Items,
                policy,
                LastChangedDates.Get(LastChangedDates.Never),
                false,
                expectedUrl,
                UpdatedLastChangedDates.Get(expectedUpdatedLastCheckedDate)
            );
        }

        [CompositeTest]
        [Category.SurveyNews]
        [InlineData(SurveyNewsPolicy.Disabled, UpdatedLastChangedDates.Unchanged)]
        [InlineData(SurveyNewsPolicy.CheckOnceDay, UpdatedLastChangedDates.Now)]
        [InlineData(SurveyNewsPolicy.CheckOnceWeek, UpdatedLastChangedDates.Now)]
        [InlineData(SurveyNewsPolicy.CheckOnceMonth, UpdatedLastChangedDates.Now)]
        public async void NoPageWhenNoItemAvailable(SurveyNewsPolicy policy, int expectedUpdatedLastCheckDateIndex) {
            // When the check is done on project load in the background,
            // and there are no available 'not voted' items in the feed,
            // then nothing will show.
            await CheckSurveyNews(
                MockFeeds.NoItems,
                policy,
                LastChangedDates.Get(LastChangedDates.MoreThanAMonth),
                false,
                MockUrls.None,
                UpdatedLastChangedDates.Get(expectedUpdatedLastCheckDateIndex)
            );
        }

        [Test]
        [Category.Telemetry]
        public async void NoPageWhenFeedNotFound() {
            await CheckSurveyNews(
                MockFeeds.NotFound,
                SurveyNewsPolicy.CheckOnceDay,
                LastChangedDates.Get(LastChangedDates.MoreThanAMonth),
                false,
                MockUrls.None,
                UpdatedLastChangedDates.Get(UpdatedLastChangedDates.Now)
            );
        }

        private async Task CheckSurveyNews(SurveyNewsFeed feed, SurveyNewsPolicy policy,
            DateTime lastChecked, bool forceCheck, string expectedNavigatedUrl, DateTime? expectedLastChecked) {
            string navigatedUrl = null;

            // Create the test objects
            var shell = TestCoreShell.CreateSubstitute();

            var ps = shell.Process();
            ps.When(x => x.Start(Arg.Any<string>())).Do(x => {
                navigatedUrl = (string)x.Args()[0];
            });

            var options = new MockSurveyNewsOptions(policy, lastChecked);
            var feedClient = new MockSurveyNewsFeedClient(feed);


            // Invoke the real survey/news service
            var service = new SurveyNewsService(feedClient, options, shell);
            await service.CheckSurveyNewsAsync(forceCheck);

            // Check that we navigated to the right url (or didn't navigate at all)
            navigatedUrl.Should().Be(expectedNavigatedUrl);

            // Check that the last checked date has been updated (or not updated at all)
            if (expectedLastChecked.HasValue) {
                var delta = options.SurveyNewsLastCheck - expectedLastChecked.Value;
                delta.Duration().Should().BeLessOrEqualTo(TimeSpan.FromSeconds(5));
            } else {
                options.SurveyNewsLastCheck.Should().Be(lastChecked);
            }
        }

        private static class MockFeeds {
            public static readonly SurveyNewsFeed Items = new SurveyNewsFeed() {
                CannotVoteAgainUrls = new string[] { MockUrls.CannotVoteAgain1, MockUrls.CannotVoteAgain2 },
                CanVoteAgainUrls = new string[] { MockUrls.CanVoteAgain1, MockUrls.CanVoteAgain2 },
                NotVotedUrls = new string[] { MockUrls.NotVoted1, MockUrls.NotVoted2 },
            };

            public static readonly SurveyNewsFeed NoItems = new SurveyNewsFeed() {
                CannotVoteAgainUrls = new string[] { MockUrls.CannotVoteAgain1 },
                CanVoteAgainUrls = new string[] { MockUrls.CanVoteAgain1 },
                NotVotedUrls = new string[] { },
            };

            public static readonly SurveyNewsFeed NotFound = null;
        }

        private static class LastChangedDates {
            // Indices for InlineData attribute
            public const int LessThanADay = 0;
            public const int MoreThanADay = 1;
            public const int LessThanAWeek = 2;
            public const int MoreThanAWeek = 3;
            public const int LessThanAMonth = 4;
            public const int MoreThanAMonth = 5;
            public const int Never = 6;

            public static DateTime Get(int index) { return Instances[index]; }

            private static readonly DateTime[] Instances = new DateTime[] {
                DateTime.Now - TimeSpan.FromHours(22),
                DateTime.Now - TimeSpan.FromHours(25),
                DateTime.Now - TimeSpan.FromDays(6),
                DateTime.Now - TimeSpan.FromDays(8),
                DateTime.Now - TimeSpan.FromDays(25),
                DateTime.Now - TimeSpan.FromDays(32),
                DateTime.MinValue,
            };
        }

        private static class UpdatedLastChangedDates {
            // Indices for InlineData attribute
            public const int Unchanged = 0;
            public const int Now = 1;
            public const int FourDaysAgo = 2;

            public static DateTime? Get(int index) { return Instances[index]; }

            private static readonly DateTime?[] Instances = new DateTime?[] {
                null,
                DateTime.Now,
                DateTime.Now - TimeSpan.FromDays(4),
            };
        }

        private class MockUrls {
            public const string None = null;
            public const string NotVoted1 = "http://availableitem1";
            public const string NotVoted2 = "http://availableitem2";
            public const string CannotVoteAgain1 = "http://cannotvoteagain1";
            public const string CannotVoteAgain2 = "http://cannotvoteagain2";
            public const string CanVoteAgain1 = "http://canvoteagain1";
            public const string CanVoteAgain2 = "http://canvoteagain2";
            public const string CannotConnect = "http://cannotconnect";
            public const string Feed = "http://feed";
            public const string Index = "http://index";
        }

        private class MockSurveyNewsFeedClient : ISurveyNewsFeedClient {
            private SurveyNewsFeed _feed;

            public MockSurveyNewsFeedClient(SurveyNewsFeed feed) {
                _feed = feed;
            }

            public Task<SurveyNewsFeed> GetFeedAsync(string feedUrl) {
                var tcs = new TaskCompletionSource<SurveyNewsFeed>();
                if (_feed != null) {
                    tcs.SetResult(_feed);
                } else {
                    tcs.SetException(new SurveyNewsFeedException("Error reading survey/news feed."));
                }
                return tcs.Task;
            }
        }

        private class MockSurveyNewsOptions : ISurveyNewsOptions {
            public MockSurveyNewsOptions(SurveyNewsPolicy policy, DateTime lastCheck) {
                this.SurveyNewsCheck = policy;
                this.SurveyNewsLastCheck = lastCheck;
            }

            public string CannotConnectUrl { get; } = MockUrls.CannotConnect;

            public string FeedUrl { get; } = MockUrls.Feed;

            public string IndexUrl { get; } = MockUrls.Index;

            public SurveyNewsPolicy SurveyNewsCheck { get; private set; }

            public DateTime SurveyNewsLastCheck { get; set; }
        }
    }
}
