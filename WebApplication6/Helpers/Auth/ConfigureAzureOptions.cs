﻿using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using WebApplication6.Models.Auth;

namespace WebApplication6.Helpers.Auth
{
    public class ConfigureAzureOptions : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly AzureAdOptions _azureOptions;
        private readonly IGraphAuthProvider _authProvider;

        public AzureAdOptions GetAzureAdOptions() => _azureOptions;

        public ConfigureAzureOptions(IOptions<AzureAdOptions> azureOptions, IGraphAuthProvider authProvider)
        {
            _azureOptions = azureOptions.Value;
            _authProvider = authProvider;
        }

        public void Configure(string name, OpenIdConnectOptions options)
        {
            options.ClientId = _azureOptions.ClientId;
            options.Authority = $"{_azureOptions.Instance}common/v2.0";
            options.UseTokenLifetime = true;
            options.CallbackPath = _azureOptions.CallbackPath;
            options.RequireHttpsMetadata = false;
            options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
            var allScopes = $"{_azureOptions.Scopes} {_azureOptions.GraphScopes}".Split(new[] { ' ' });
            foreach (var scope in allScopes) { options.Scope.Add(scope); }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Ensure that User.Identity.Name is set correctly after login
                NameClaimType = "name",

                // Instead of using the default validation (validating against a single issuer value, as we do in line of business apps),
                // we inject our own multitenant validation logic
                ValidateIssuer = false,

                // If the app is meant to be accessed by entire organizations, add your issuer validation logic here.
                //IssuerValidator = (issuer, securityToken, validationParameters) => {
                //    if (myIssuerValidationLogic(issuer)) return issuer;
                //}
            };

            options.Events = new OpenIdConnectEvents
            {
                OnTicketReceived = context =>
                {
                    // If your authentication logic is based on users then add your logic here
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    context.Response.Redirect("/Home/Error");
                    context.HandleResponse(); // Suppress the exception
                    return Task.CompletedTask;
                },
                OnAuthorizationCodeReceived = async (context) =>
                {
                    var code = context.ProtocolMessage.Code;
                    var identifier = context.Principal.FindFirst(Startup.ObjectIdentifierType).Value;

                    var result = await _authProvider.GetUserAccessTokenByAuthorizationCode(code);

                    // Check whether the login is from the MSA tenant. 
                    // The sample uses this attribute to disable UI buttons for unsupported operations when the user is logged in with an MSA account.
                    //var currentTenantId = context.Principal.FindFirst(Startup.TenantIdType).Value;

                    context.HandleCodeRedemption(result.AccessToken, result.IdToken);
                },
                // If your application needs to do authenticate single users, add your user validation below.
                //OnTokenValidated = context =>
                //{
                //    return myUserValidationLogic(context.Ticket.Principal);
                //}
            };
        }

        public void Configure(OpenIdConnectOptions options)
        {
            Configure(Options.DefaultName, options);
        }
    }
}
