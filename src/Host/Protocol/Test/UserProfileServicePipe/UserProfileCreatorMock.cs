// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Extensions.Logging;

namespace Microsoft.R.Host.Protocol.Test.UserProfileServicePipe {
    internal class UserProfileCreatorMock : IUserProfileServices {
        public UserProfileCreatorMock() { }

        public bool TestingValidParse { get; private set; }
        public bool TestingValidAccount { get; private set; }
        public bool TestingExistingAccount { get; private set; }

        public string ExpectedUsername { get; private set; }
        public string ExpectedDomain { get; private set; }
        public string ExpectedPassword { get; private set; }
        

        public IUserProfileCreatorResult CreateUserProfile(IUserCredentials credMock, ILogger logger) {
            if (!TestingValidParse) {
                // If parse succeeded but did not generate an object,
                // for example, JSON parse of white spaces.
                credMock.Should().BeNull();
                return UserProfileResultMock.Create(false, false);
            }

            ValidateUsername(credMock?.Username);
            ValidateDomain(credMock?.Domain);
            ValidatePassword(credMock?.Password.ToUnsecureString());

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

        private void ValidatePassword(string password) {
            ValidateString(password, ExpectedPassword);
        }

        public static UserProfileCreatorMock Create(string username, string domain, string password, bool validParse, bool validAccount, bool existingAccount) {
            var creator = new UserProfileCreatorMock();
            creator.ExpectedUsername= username;
            creator.ExpectedDomain= domain;
            creator.ExpectedPassword= password;
            creator.TestingValidParse = validParse;
            creator.TestingValidAccount = validAccount;
            creator.TestingExistingAccount = existingAccount;
            return creator;
        }
    }
}
