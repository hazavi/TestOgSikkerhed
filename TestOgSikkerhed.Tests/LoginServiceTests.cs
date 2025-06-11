using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using TestOgSikkerhed.Interfaces;

namespace TestOgSikkerhed.Tests
{
    public class LoginServiceTests
    {
        [Fact]
        public async Task LoginLoginService_ReturnsSuccess_WhenCredentialsAreCorrect()
        {
            var loginRepo = Substitute.For<ILoginRepo>();
            var loginService = new LoginService(loginRepo);
            loginRepo.LoginLoginRepo("test@example.com", "password", true, false)
                .Returns(Task.FromResult(SignInResult.Success));

            var result = await loginService.LoginLoginService("test@example.com", "password", true, false);

            Assert.Equal(SignInResult.Success, result);
        }

        [Fact]
        public async Task LoginLoginService_ReturnsFailed_WhenCredentialsAreInvalid()
        {
            var loginRepo = Substitute.For<ILoginRepo>();
            var loginService = new LoginService(loginRepo);
            loginRepo.LoginLoginRepo("wrong@example.com", "badpassword", false, false)
                .Returns(Task.FromResult(SignInResult.Failed));

            var result = await loginService.LoginLoginService("wrong@example.com", "badpassword", false, false);

            Assert.Equal(SignInResult.Failed, result);
        }

        [Fact]
        public async Task LoginLoginService_CallsRepo_WithCorrectArguments()
        {
            var loginRepo = Substitute.For<ILoginRepo>();
            var loginService = new LoginService(loginRepo);

            await loginService.LoginLoginService("user@test.com", "pass123", true, false);

            await loginRepo.Received(1).LoginLoginRepo("user@test.com", "pass123", true, false);
        }
    }
}
