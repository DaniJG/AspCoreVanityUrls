using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VanityUrls.Configuration;
using VanityUrls.Models;

namespace VanityUrls.Middleware
{
    // This middleware could be replaced by the new Rewrite middleware and a customVanityUrlsRewriteRule in ASP.Net Core 1.1
    // I might still prefer this custom middleware. It supports async code and feels a better approach when involving business logic.

    //See http://stackoverflow.com/questions/36179304/dynamic-url-rewriting-with-mvc-and-asp-net-core/36180880#36180880
    public class VanityUrlsMiddleware
    {
        private readonly Regex _vanityUrlRegex = new Regex(VanityUrlConstants.VanityUrlRegex);
        private readonly string _resolvedProfileUrlFormat;
        private readonly RequestDelegate _next;
        private readonly UserManager<ApplicationUser> _userManager;

        public VanityUrlsMiddleware(RequestDelegate next, UserManager<ApplicationUser> userManager, IOptions<VanityUrlsMiddlewareOptions> options)
        {
            _next = next;
            _userManager = userManager;
            _resolvedProfileUrlFormat = options.Value.ResolvedProfileUrlFormat;
        }

        public async Task Invoke(HttpContext context)
        {
            await HandleVanityUrl(context);

            //Let the next middleware (MVC routing) handle the request
            //In case the path was updated, the MVC routing will see the updated path
            await _next.Invoke(context);

        }

        private async Task HandleVanityUrl(HttpContext context)
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
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.VanityUrl.Equals(path, StringComparison.CurrentCultureIgnoreCase));
            if (user == null) return;

            //If we got this far, the url matches a vanity url, which can be resolved to the profile details page of the user
            //Replace the request path so the next middleware (MVC) uses the resolved path
            context.Request.Path = String.Format(_resolvedProfileUrlFormat, user.Id);

            //Save the user into the HttpContext so we don't need to fetch it again from the DB in the controller action
            context.Items[VanityUrlConstants.ResolvedUserContextItem] = user;
        }

        private bool IsVanityUrl(string path)
        {
            return _vanityUrlRegex.IsMatch(path);
        }
    }
}
