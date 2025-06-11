using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestOgSikkerhed.Controllers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace TestOgSikkerhed.Tests
{
    public class AuthenticationTests
    {
        [Fact]
        public void User_IsAuthenticated_ReturnsTrue()
        {
            // Arrange
            var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = user };
            var logger = Substitute.For<ILogger<HomeController>>();
            var controller = new HomeController(logger)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            // Act & Assert
            Assert.True(controller.HttpContext.User.Identity.IsAuthenticated);
        }

        [Fact]
        public void User_IsNotAuthenticated_ReturnsFalse()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // No authentication type
            var user = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = user };
            var logger = Substitute.For<ILogger<HomeController>>();
            var controller = new HomeController(logger)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            // Act & Assert
            Assert.False(controller.HttpContext.User.Identity.IsAuthenticated);
        }
    }
}

