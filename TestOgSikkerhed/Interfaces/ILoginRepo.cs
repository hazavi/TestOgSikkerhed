using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace TestOgSikkerhed.Interfaces
{
    public interface ILoginRepo
    {
        Task<SignInResult> LoginLoginRepo(string email, string password, bool rememberMe, bool lockoutOnFailure);
    }
}
