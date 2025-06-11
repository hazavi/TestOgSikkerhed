using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TestOgSikkerhed.Interfaces
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepo _loginRepo;
        
        public LoginService(ILoginRepo loginRepo)
        {
            _loginRepo = loginRepo;
        }

        public async Task<SignInResult> LoginLoginService(string email, string password, bool rememberMe, bool lockoutOnFailure)
        {
            return await _loginRepo.LoginLoginRepo(email, password, rememberMe, lockoutOnFailure);
        }
    }
}
