﻿using System;
using System.Threading.Tasks;
using Auth0.AuthenticationApi.Client.Models;
using Auth0.Core;
using Auth0.ManagementApi.Client;
using Auth0.ManagementApi.Client.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Auth0.AuthenticationApi.Client.IntegrationTests
{
    [TestFixture]
    public class DatabaseConnectionTests : TestBase
    {
        private ManagementApiClient managementApiClient;
        private Connection connection;
        private string clientId = "rLNKKMORlaDzrMTqGtSL9ZSXiBBksCQW";

        [SetUp]
        public async Task SetUp()
        {
            managementApiClient = new ManagementApiClient(GetVariable("AUTH0_TOKEN_AUTHENTICATION"), new Uri(GetVariable("AUTH0_MANAGEMENT_API_URL")));

            // We will need a connection to add the users to...
            connection = await managementApiClient.Connections.Create(new ConnectionCreateRequest
            {
                Name = Guid.NewGuid().ToString("N"),
                Strategy = "auth0",
                EnabledClients = new[] { clientId }
            });
        }

        [TearDown]
        public async Task TearDown()
        {
            if (connection != null)
                await managementApiClient.Connections.Delete(connection.Id);
        }

        [Test]
        public async Task Can_signup_user_and_change_password()
        {
            // Arrange
            var authenticationApiClient = new AuthenticationApiClient(new Uri(GetVariable("AUTH0_AUTHENTICATION_API_URL")));

            // Sign up the user
            var signupUserRequest = new SignupUserRequest
            {
                ClientId = clientId,
                Connection = connection.Name,
                Email = $"{Guid.NewGuid().ToString("N")}@nonexistingdomain.aaa",
                Password = "password"
            };
            var signupUserResponse = await authenticationApiClient.SignupUser(signupUserRequest);
            signupUserResponse.Should().NotBeNull();
            signupUserResponse.Email.Should().Be(signupUserRequest.Email);

            // Change the user's email address
            var changePasswordRequest = new ChangePasswordRequest
            {
                ClientId = signupUserRequest.ClientId,
                Connection = signupUserRequest.Connection,
                Email = signupUserRequest.Email,
                Password = "password2"
            };
            string changePasswordResponse = await authenticationApiClient.ChangePassword(changePasswordRequest);
            changePasswordResponse.Should().Be("\"We've just sent you an email to reset your password.\"");
        }
    }
}