using Microsoft.AspNetCore.Identity;

namespace TestOgSikkerhed.Interfaces
{
    public interface ILoginService
    {
        Task<SignInResult> LoginLoginService(string email, string password, bool rememberMe,

    bool lockoutOnFailure);
    }
}
