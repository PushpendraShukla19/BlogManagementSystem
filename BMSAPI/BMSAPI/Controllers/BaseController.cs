using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace BMSAPI.Controllers
{
    // BaseController for common functionality like token parsing
    public class BaseController : ControllerBase
    {
        // Constructor for BaseController
        public BaseController()
        {
        }

        // Method to extract and validate token data (like username, role, etc.)
        protected ActionResult<string> GetTokenDtails()
        {
            // Extract the token from the Authorization header
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing.");
            }

            try
            {
                // Initialize JwtSecurityTokenHandler
                var handler = new JwtSecurityTokenHandler();

                // Read and parse the JWT token
                var jwtToken = handler.ReadJwtToken(token);

                // Extract claims from the token
                var claims = jwtToken.Claims;

                // Extract specific claims like username, role, etc.
                var username = claims.FirstOrDefault(c => c.Type == "username")?.Value;
                var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                // You can perform any action with the extracted data (e.g., authorization checks, logging, etc.)
                if (!string.IsNullOrEmpty(username))
                {
                    // For example, log the username or use it for further checks
                    return Ok($"Data accessed successfully! Username: {username}, Role: {role}");
                }
                else
                {
                    return Unauthorized("Invalid token.");
                }
            }
            catch (Exception ex)
            {
                // If token parsing fails, return Unauthorized with the error message
                return Unauthorized($"Invalid token: {ex.Message}");
            }
        }
    }
}
