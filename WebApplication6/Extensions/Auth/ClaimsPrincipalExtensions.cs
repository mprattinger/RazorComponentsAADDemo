using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication6.Extensions.Auth
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetObjectId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        }

        public static string GetEmailAddress(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return principal.FindFirstValue("preferred_username");
        }
    }
}
