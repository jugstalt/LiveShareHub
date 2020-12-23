using LiveShareHub.Core.Reflection;
using LiveShareHub.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveShareHub.Core.Extensions;
using Org.BouncyCastle.Asn1.X509;
using System.Net.Mime;

namespace LiveShareHub.Middleware
{
    public class AuthorizeAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizeAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context,
                                 RoutingEndPointReflectionService endpointReflection,
                                 IConfiguration config)
        {
            if (endpointReflection != null && endpointReflection.Apply)
            {
                var clients = config.GetSection("Clients");

                if (clients.GetChildren().Count() > 0)
                {
                    var authAccessAttribute = endpointReflection.GetCustomAttribute<AuthorizeAccessAttribute>();

                    if (authAccessAttribute != null)
                    {
                        bool grantAccess = false;
                        if (authAccessAttribute.AuthorizationType.HasFlag(AuthorizationType.Basic))
                        {
                            string authHeaderValue = context.Request.Headers["Authorization"];

                            if (authHeaderValue != null &&
                                authHeaderValue.StartsWith("Basic ", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var usernamePassword = authHeaderValue.BasicAuthUsernamePassword();

                                var client = clients
                                    .GetChildren()
                                    .Where(c => "basic".Equals(c["type"], StringComparison.InvariantCultureIgnoreCase) &&
                                                c["name"] == usernamePassword.username)
                                    .FirstOrDefault();

                                if (client != null && client["password"] == usernamePassword.password)
                                {
                                    grantAccess = true;
                                }
                            }
                        }
                        if (!grantAccess)
                        {
                            if (clients
                                .GetChildren()
                                .Where(c => "basic".Equals(c["type"], StringComparison.InvariantCultureIgnoreCase))
                                .Count() > 0)
                            {
                                context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"RealmName\"";
                            }
                            context.Response.StatusCode = 401;
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
