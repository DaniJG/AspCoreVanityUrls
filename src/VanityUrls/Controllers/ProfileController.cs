using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using VanityUrls.Models;
using VanityUrls.Models.ProfileViewModel;
using Microsoft.EntityFrameworkCore;
using VanityUrls.Configuration;

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
            var user = HttpContext.Items[VanityUrlConstants.ResolvedUserContextItem] as ApplicationUser;           
            if (user == null)
            {
                user = await _userManager.FindByIdAsync(id);
            }

            return View(new Profile { Email = user.Email, Name = user.UserName, VanityUrl = user.VanityUrl });
        }

        public async Task<JsonResult> ValidateVanityUrl(string vanityUrl)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.VanityUrl == vanityUrl);

            //TODO: Also check the selected vanityUrl doesnt match any of our controllers and routes!

            return Json(user == null);
        }
    }
}