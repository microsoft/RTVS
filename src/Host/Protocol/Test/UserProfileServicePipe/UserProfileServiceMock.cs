// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;

namespace Microsoft.R.Host.Protocol.Test.UserProfileServicePipe {
    internal class UserProfileServiceMock : IUserProfileServices {
        public UserProfileServiceMock() { }

        public bool TestingValidParse { get; private set; }
        public bool TestingValidAccount { get; private set; }
        public bool TestingExistingAccount { get; private set; }

        public string ExpectedUsername { get; private set; }
        public string ExpectedDomain { get; private set; }
        public string ExpectedSid { get; private set; }
        

        public IUserProfileServiceResult CreateUserProfile(IUserCredentials credMock, ILogger logger) {
            if (!TestingValidParse) {
                // If parse succeeded but did not generate an object,
                // for example, JSON parse of white spaces.
                credMock.Should().BeNull();
                return UserProfileResultMock.Create(false, false);
            }

            ValidateUsername(credMock?.Username);
            ValidateDomain(credMock?.Domain);
            ValidateSid(credMock?.Sid);

            return UserProfileResultMock.Create(TestingValidAccount, TestingExistingAccount);
        }

        public IUserProfileServiceResult DeleteUserProfile(IUserCredentials credMock, ILogger logger) {
            if (!TestingValidParse) {
                // If parse succeeded but did not generate an object,
                // for example, JSON parse of white spaces.
                credMock.Should().BeNull();
                return UserProfileResultMock.Create(false, false);
            }

            ValidateUsername(credMock?.Username);
            ValidateDomain(credMock?.Domain);
            ValidateSid(credMock?.Sid);

            return UserProfileResultMock.Create(TestingValidAccount, TestingExistingAccount);
        }

        private void ValidateString(string actual, string expected) {
            actual.Should().Be(expected);
        }

        private void ValidateUsername(string username) {
            ValidateString(username, ExpectedUsername);
        }

        private void ValidateDomain(string domain) {
            ValidateString(domain, ExpectedDomain);
        }

        private void ValidateSid(string sid) {
            ValidateString(sid, ExpectedSid);
        }

        public static UserProfileServiceMock Create(string username, string domain, string sid, bool validParse, bool validAccount, bool existingAccount) {
            var creator = new UserProfileServiceMock();
            creator.ExpectedUsername= username;
            creator.ExpectedDomain= domain;
            creator.ExpectedSid = sid;
            creator.TestingValidParse = validParse;
            creator.TestingValidAccount = validAccount;
            creator.TestingExistingAccount = existingAccount;
            return creator;
        }
    }
}
