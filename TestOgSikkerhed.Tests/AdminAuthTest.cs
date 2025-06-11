using Microsoft.AspNetCore.Authorization;
using TestOgSikkerhed.Controllers;

namespace TestOgSikkerhed.Tests
{
    public class AdminAuthTest
    {
        [Fact]
        public void Privacy_HasAuthorizeAttribute_WithAdminRole()

        {
            // Arrange
            var methodInfo = typeof(HomeController).GetMethod("Privacy");
            // Finder metainformation (reflection) om 'Privacy' metoden i HomeController. 

            //Act
            var authorizeAttribute = methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).FirstOrDefault() as AuthorizeAttribute;
            // Henter Authorize attributten hvis den findes. 

            // Assert
            Assert.NotNull(authorizeAttribute);
            // Tester at der findes en Authorize attribut.

            Assert.Equal("Admin", authorizeAttribute.Roles);
            // Tester at Authorize attributten kræver rollen 'Admin'.
        }
    }
}
