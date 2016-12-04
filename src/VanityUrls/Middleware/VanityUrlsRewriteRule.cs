using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VanityUrls.Configuration;
using VanityUrls.Models;

namespace VanityUrls.Middleware
{
    public class VanityUrlsRewriteRule : IRule
    {
        private readonly Regex _vanityUrlRegex = new Regex(VanityUrlConstants.VanityUrlRegex);
        private readonly string _resolvedProfileUrlFormat;
        private readonly UserManager<ApplicationUser> _userManager;

        public VanityUrlsRewriteRule(UserManager<ApplicationUser> userManager, IOptions<VanityUrlsMiddlewareOptions> options)
        {
            _userManager = userManager;
            _resolvedProfileUrlFormat = options.Value.ResolvedProfileUrlFormat;
        }

        public void ApplyRule(RewriteContext context)
        {
            var httpContext = context.HttpContext;

            //get path from request
            var path = httpContext.Request.Path.ToUriComponent();
            if (path[0] == '/')
            {
                path = path.Substring(1);
            }

            //Check if it matches the VanityUrl format (single segment, only lower case letters, dots and dashes)
            if (!_vanityUrlRegex.IsMatch(path))
            {
                return;
            }

            //Check if a user with this vanity url can be found
            var user = _userManager.Users.SingleOrDefault(u => u.VanityUrl.Equals(path, StringComparison.CurrentCultureIgnoreCase));
            if (user == null) return;

            //If we got this far, the url matches a vanity url, which can be resolved to the profile details page of the user
            //Replace the request path so the next middleware (MVC) uses the resolved path
            httpContext.Request.Path = String.Format(_resolvedProfileUrlFormat, user.Id);

            //Save the user into the HttpContext so we don't need to fetch it again from the DB in the controller action
            httpContext.Items[VanityUrlConstants.ResolvedUserContextItem] = user;

            //No need to set context.Result as the default value "RuleResult.ContinueRules" is ok
        }
    }
}
