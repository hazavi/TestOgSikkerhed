using Microsoft.AspNetCore.Identity;

namespace TestOgSikkerhed.Interfaces
{
    public class LoginRepo : ILoginRepo
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public LoginRepo(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<SignInResult> LoginLoginRepo(string email, string password, bool rememberMe, bool lockoutOnFailure)
        {
            return await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure);
        }
    }
}
