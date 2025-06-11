using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq;
using System.Threading.Tasks;
using TestOgSikkerhed.Areas.Identity.Pages.Account;
using TestOgSikkerhed.Interfaces;
using Xunit;
//using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;

namespace TestOgSikkerhed.Tests;
public class LoginModelTests
{
    private readonly SignInManager<IdentityUser> _signInManagerSub;
    private readonly ILogger<LoginModel> _loggerSub;
    private readonly ILoginService _loginServiceSub;

    public LoginModelTests()
    {
        var userManager = Substitute.For<UserManager<IdentityUser>>(
            Substitute.For<IUserStore<IdentityUser>>(),
            null, null, null, null, null, null, null, null
        );

        _signInManagerSub = Substitute.For<SignInManager<IdentityUser>>(
            userManager,
            Substitute.For<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<IdentityUser>>(),
            null, null, null, null
        );

        _signInManagerSub.GetExternalAuthenticationSchemesAsync()
            .Returns(Task.FromResult(Enumerable.Empty<AuthenticationScheme>()));

        _loggerSub = Substitute.For<ILogger<LoginModel>>();
        _loginServiceSub = Substitute.For<ILoginService>();
    }

    [Fact]
    public async Task OnPostAsync_LoginSuccessful_RedirectsToHomePage()
    {
        var loginModel = new LoginModel(_signInManagerSub, _loggerSub, _loginServiceSub)
        {
            Input = new LoginModel.InputModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = false
            }
        };

        loginModel.Url = Substitute.For<IUrlHelper>();
        loginModel.Url.Content("~/").Returns("/");

        _loginServiceSub.LoginLoginService("test@example.com", "Password123!", false, true)
            .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

        var result = await loginModel.OnPostAsync();
        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/", redirectResult.Url);
    }

    [Fact]
    public async Task OnPostAsync_LoginFailed_ReturnsPageWithError()
    {
        var loginModel = new LoginModel(_signInManagerSub, _loggerSub, _loginServiceSub)
        {
            Input = new LoginModel.InputModel
            {
                Email = "wrong@example.com",
                Password = "WrongPassword!",
                RememberMe = false
            }
        };

        loginModel.Url = Substitute.For<IUrlHelper>();
        loginModel.Url.Content("~/").Returns("/");

        _loginServiceSub.LoginLoginService("wrong@example.com", "WrongPassword!", false, true)
            .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

        var result = await loginModel.OnPostAsync();
        var pageResult = Assert.IsType<PageResult>(result);
        Assert.False(loginModel.ModelState.IsValid);
        Assert.Contains(loginModel.ModelState, m => m.Value.Errors.Count > 0);
    }

    [Fact]
    public async Task OnPostAsync_LockedOut_ReturnsPageWithLockedOut()
    {
        var loginModel = new LoginModel(_signInManagerSub, _loggerSub, _loginServiceSub)
        {
            Input = new LoginModel.InputModel
            {
                Email = "locked@out.com",
                Password = "LockedOut123!",
                RememberMe = false
            }
        };

        loginModel.Url = Substitute.For<IUrlHelper>();
        loginModel.Url.Content("~/").Returns("/");

        _loginServiceSub.LoginLoginService("locked@out.com", "LockedOut123!", false, true)
            .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.LockedOut));

        var result = await loginModel.OnPostAsync();
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("./Lockout", redirectResult.PageName);
    }
}
