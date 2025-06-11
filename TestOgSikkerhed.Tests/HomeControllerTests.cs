using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Security.Claims;
using System.Linq;
using TestOgSikkerhed.Controllers;

namespace TestOgSikkerhed.Tests
{
    public class HomeControllerTests
    {
        private HomeController GetControllerWithRoles(params string[] roles)
        {
            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r));
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = user };
            var logger = Substitute.For<ILogger<HomeController>>();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);
            var controller = new HomeController(logger)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = tempData
            };
            return controller;
        }

        [Fact]
        public void Privacy_UserIsAdmin_ReturnsView()
        {
            var controller = GetControllerWithRoles("Admin");
            var result = controller.Privacy();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Privacy", viewResult.ViewName);
        }
    }
}

