using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using NoSQLProject.Repositories;
using NoSQLProject.Models;

namespace NoSQLProject.Other.Security
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "BasicAuthentication";

        private readonly IEmployeeRepository _employees;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IEmployeeRepository employees) : base(options, logger, encoder, clock)
        {
            _employees = employees;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();

            try
            {
                if (!TryParseBasicHeader(Request.Headers["Authorization"].ToString(), out var email, out var password))
                {
                    return AuthenticateResult.NoResult();
                }

                var emp = await ValidateEmployeeAsync(email!, password!);
                if (emp is null)
                {
                    return AuthenticateResult.Fail("Invalid username or password");
                }

                var role = emp is ServiceDeskEmployee ? "ServiceDesk" : "Employee";

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, emp.Id),
                    new(ClaimTypes.Name, emp.FullName),
                    new(ClaimTypes.Email, emp.Email),
                    new(ClaimTypes.Role, role)
                };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }

        private static bool TryParseBasicHeader(string authorizationHeader, out string? email, out string? password)
        {
            email = null;
            password = null;

            if (string.IsNullOrWhiteSpace(authorizationHeader) ||
                !authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var token = authorizationHeader[6..].Trim();
            string credentialString;
            try
            {
                credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            }
            catch
            {
                return false;
            }

            var parts = credentialString.Split(':', 2);
            if (parts.Length != 2) return false;

            email = parts[0];
            password = parts[1];
            return !(string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password));
        }

        private async Task<Employee?> ValidateEmployeeAsync(string email, string password)
        {
            var hashed = Hasher.GetHashedString(password);
            var emp = await _employees.GetByCredentialsAsync(email, hashed);
            return emp is { Status: Employee_Status.Active } ? emp : null;
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"API\"";
            return base.HandleChallengeAsync(properties);
        }
    }
}
