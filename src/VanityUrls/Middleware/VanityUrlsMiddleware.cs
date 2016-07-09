using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VanityUrls.Models;

namespace VanityUrls.Middleware
{
    //See http://stackoverflow.com/questions/36179304/dynamic-url-rewriting-with-mvc-and-asp-net-core/36180880#36180880
    public class VanityUrlsMiddleware
    {
        private readonly Regex _vanityUrlRegex = new Regex(@"[a-z\.\-]+");
        private readonly string _resolvedProfileUrlFormat = "/profile/details/{0}";
        private readonly RequestDelegate _next;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public VanityUrlsMiddleware(RequestDelegate next, UserManager<ApplicationUser> userManager)
        {
            _next = next;
            _userManager = userManager;
        }

        public async Task Invoke(HttpContext context)
        {
            HandleVanityUrl(context);

            //Let the next middleware (MVC routing) handle the request
            //In case the path was updated, the MVC routing will see the updated path
            await _next.Invoke(context);

        }

        private void HandleVanityUrl(HttpContext context)
        {
            //get path from request
            var path = context.Request.Path.ToUriComponent();
            if (path[0] == '/')
            {
                path = path.Substring(1);
            }

            //Check if it matches the VanityUrl format (single segment, only lower case letters, dots and dashes)
            if (!IsVanityUrl(path))
            {
                return;
            }

            //Check if a user with this vanity url can be found
            var user = _userManager.Users.SingleOrDefault(u => u.VanityUrl.Equals(path));
            if (user == null) return;

            //If we got this far, the url matches a vanity url, which can be resolved to the profile details page of the user
            //Replace the request path so the next middleware (MVC) uses the resolved path
            context.Request.Path = String.Format(_resolvedProfileUrlFormat, user.Id);
        }

        private bool IsVanityUrl(string path)
        {
            return _vanityUrlRegex.IsMatch(path);
        }
    }
}
