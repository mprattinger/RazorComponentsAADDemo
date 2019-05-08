using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication6.Extensions.Auth;
using WebApplication6.Helpers.Auth;

namespace WebApplication6
{
    public class TestClaimsTransformation : IClaimsTransformation
    {
        private readonly IGraphSdkHelper _graphSdkHelper;
        private readonly ILogger<TestClaimsTransformation> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TestClaimsTransformation(IGraphSdkHelper graphSdkHelper, ILogger<TestClaimsTransformation> logger, IHttpContextAccessor httpContextAccessor)
        {
            _graphSdkHelper = graphSdkHelper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var email = string.Empty;
            try
            {
                var config = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var ret = new Dictionary<string, string>();
                config.GetSection("UserPermissions").GetChildren().ToList().ForEach(x => ret.Add(x.Key, x.Value));
                var claims = ret.Select(x => new Claim("UserPremission", x.Key)).ToList();

                var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)principal.Identity);
                email = principal.GetEmailAddress();

                var groups = await graphClient.Users[email].MemberOf.Request().GetAsync();
                var groupList = new List<string>();
                foreach (var g in groups.CurrentPage)
                {
                    if (g.GetType() == typeof(Group))
                    {
                        var role = g as Group;
                        groupList.Add(role.DisplayName);
                    }
                    else if (g.GetType() == typeof(DirectoryRole))
                    {
                        var role = g as DirectoryRole;
                        groupList.Add(role.DisplayName);
                    }
                }

                if (groupList.Any(x => x == "TMSMaster" || x == "TMSAdmin"))
                {
                    _logger.LogInformation($"Adding claims to user: {string.Join("; ", claims.Select(x => x.Value).ToArray())}");
                    ((ClaimsIdentity)principal.Identity).AddClaims(claims);
                }
            }
            catch (ServiceException e)
            {
                switch (e.Error.Code)
                {
                    case "Request_ResourceNotFound":
                    case "ResourceNotFound":
                    case "ErrorItemNotFound":
                    case "itemNotFound":
                        _logger.LogError($"Error: {e.Error.Code}, {JsonConvert.SerializeObject(new { Message = $"User '{email}' was not found." }, Formatting.Indented)}");
                        break;
                    case "ErrorInvalidUser":
                        _logger.LogError($"Error: {e.Error.Code}, {JsonConvert.SerializeObject(new { Message = $"The requested user '{email}' is invalid." }, Formatting.Indented)}");
                        break;
                    case "AuthenticationFailure":
                        _logger.LogError($"Error: {e.Error.Code}, {JsonConvert.SerializeObject(new { e.Error.Message }, Formatting.Indented)}");
                        break;
                    case "TokenNotFound":
                        await _httpContextAccessor.HttpContext.ChallengeAsync();
                        _logger.LogError($"Error: {e.Error.Code}, {JsonConvert.SerializeObject(new { e.Error.Message }, Formatting.Indented)}"); ;
                        break;
                    default:
                        _logger.LogError($"Error: {e.Error.Code}, {JsonConvert.SerializeObject(new { Message = "An unknown error has occurred." }, Formatting.Indented)}");
                        break;
                }
            }
            return principal;
        }
    }
}
