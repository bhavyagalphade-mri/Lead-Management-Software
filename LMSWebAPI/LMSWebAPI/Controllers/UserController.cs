using LMSWebAPI.Data;
using LMSWebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LMSWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetHirarchyUsers()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (String.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer"))
            {
                return Unauthorized(new { message = "Token is missing or invalid" });
            }

            var token = authHeader.Substring("Bearer" .Length).Trim();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (roleClaim == null)
            {
                return Unauthorized(new { message = "Role not found in token" });
            }


            //if(!Enum.TryParse(roleClaim, out UserRole userRole))
            //{
            //    return BadRequest("Invalid role in token");
            //}

            IQueryable<User> usersQuery;

            if (roleClaim == "Admin")
            {
                usersQuery = _context.users.Where(v => v.role == UserRole.Manager);
            }

            else if (roleClaim == "Manager")
            {
                usersQuery = _context.users.Where(u => u.role == UserRole.SalesRepresentative);
            }
            else
            {
                return Forbid();
            }

            var users = await usersQuery.Select( u => new
            {
                u.u_id,
                u.u_name,
                u.u_email,
                u.role
            }).ToListAsync();

            return Ok(users);
        }
    }
}
