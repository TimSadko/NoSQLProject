using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using NoSQLProject.Repositories;
using NoSQLProject.Other;
using NoSQLProject.Models;

namespace NoSQLProject.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IEmployeeRepository _employees;
        public AuthController(IEmployeeRepository employees) => _employees = employees;

        public class LoginRequest
        {
            public string? Email { get; set; }
            public string Password { get; set; } = string.Empty;
        }

        // POST: /api/auth/basic
        // Validates credentials against Employees collection (email + password)
        // and returns a ready-to-use Authorization header value.
        [HttpPost("basic")]
        [AllowAnonymous]
        public async Task<IActionResult> Basic([FromBody] LoginRequest? req)
        {
            var login = req?.Email;
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(req?.Password))
            {
                return BadRequest(new { error = "Email (or username) and password are required" });
            }

            try
            {
                var hashed = Hasher.GetHashedString(req.Password);
                var emp = await _employees.GetByCredentialsAsync(login, hashed);
                if (emp is not { Status: Employee_Status.Active })
                {
                    return Unauthorized(new { error = "Invalid email or password" });
                }

                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{req.Password}"));
                return Ok(new
                {
                    scheme = "Basic",
                    authorizationHeader = $"Basic {token}",
                    note = "Send this value as the Authorization header when calling protected endpoints (e.g., /api/tickets)."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
