using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestOgSikkerhed.Controllers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace TestOgSikkerhed.Tests
{
    public class AuthorizationTests
    {
        [Fact]
        public void User_IsAdmin_ReturnsTrue()
        {
            var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = user };
            var logger = Substitute.For<ILogger<HomeController>>();
            var controller = new HomeController(logger)
            {
                ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = httpContext }
            };

            Assert.True(controller.HttpContext.User.IsInRole("Admin"));
        }

        [Fact]
        public void User_IsNotAdmin_ReturnsFalse()
        {
            var claims = new[] { new Claim(ClaimTypes.Role, "User") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = user };
            var logger = Substitute.For<ILogger<HomeController>>();
            var controller = new HomeController(logger)
            {
                ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = httpContext }
            };

            Assert.False(controller.HttpContext.User.IsInRole("Admin"));
        }
    }
}

