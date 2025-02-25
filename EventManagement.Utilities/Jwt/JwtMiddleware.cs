using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace EventManagement.Utilities.Jwt
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var jwtToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            try
            {
                if (jwtToken != null)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(jwtToken);

                    // Extracting user ID from claims
                    var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "id");
                    long userId;
                    if (long.TryParse(userIdClaim.Value, out userId))
                    {
                        context.Items["UserId"] = userId;
                    }

                    // Extracting user Role from claims
                    var userRole = token.Claims.FirstOrDefault(c => c.Type == "role");
                    string role;
                    if (userRole != null)
                    {
                        role = userRole.Value.ToLower();
                        context.Items["UserRole"] = role;
                    }

                    // Validate token format (JWT should have 3 parts)
                    if (!IsValidJwtFormat(jwtToken))
                    {
                        context.Response.StatusCode = 401; // Unauthorized
                        await context.Response.WriteAsync("Invalid token format.");
                        return;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                // Catch invalid token format exceptions and return 401 Unauthorized
                _logger.LogError("Invalid token: {Error}", ex.Message);
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Invalid token.");
                return;
            }

            // Retrieve the OrganizationId from the header
            var organizationIdHeader = context.Request.Headers["OrganizationId"].FirstOrDefault();
            if (!string.IsNullOrEmpty(organizationIdHeader) && long.TryParse(organizationIdHeader, out long organizationId))
            {
                // Set the OrganizationId to the context items
                context.Items["OrganizationId"] = organizationId;
            }
            else
            {
                // Set a default value of 0 if the header is missing or invalid
                context.Items["OrganizationId"] = 0;
            }

            await _next(context);
        }

        // Helper method to validate if a token has a valid JWT structure (3 parts separated by '.')
        private bool IsValidJwtFormat(string token)
        {
            // A valid JWT should have 3 parts (separated by dots)
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }
            return true;
        }
        }
    }
