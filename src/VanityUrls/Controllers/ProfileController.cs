using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using VanityUrls.Models;
using VanityUrls.Models.ProfileViewModel;
using VanityUrls.Features;

namespace VanityUrls.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Details(string id)
        {
            var resolvedUserFeature = HttpContext.Features.Get<VanityUrlResolvedUser>();
            var user = resolvedUserFeature?.User;
            if (user == null)
            {
                user = await _userManager.FindByIdAsync(id);
            }

            return View(new Profile { Email = user.Email, Name = user.UserName, VanityUrl = user.VanityUrl });
        }
    }
}