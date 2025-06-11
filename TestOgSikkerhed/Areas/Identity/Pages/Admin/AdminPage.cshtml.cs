using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestOgSikkerhed.Areas.Identity.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminPageModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminPageModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();

        public async Task OnGetAsync()
        {
            var users = _userManager.Users.ToList();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = string.Join(", ", roles),
                    EmailConfirmed = user.EmailConfirmed
                });
            }
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Roles { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}