using Lms.Models;
using LMSWebAPI.Data;
using LMSWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LMSWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public RegistrationController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            
        }

        //Register User
        [Authorize(Roles = "Superuser, Admin")]
        [HttpPost("register")] //add user
        public async Task<IActionResult> Register([FromBody] User user)
        {

            var currentUserIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }
            int currentUserId = int.Parse(currentUserIdClaim.Value);



            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (currentUserRole != UserRole.Superuser.ToString() && currentUserRole != UserRole.Admin.ToString())
            {
                return Unauthorized("Only Superuser or Admin can create new users.");
            }

            if (currentUserRole == UserRole.Admin.ToString() && user.role == UserRole.Superuser)
            {
                return BadRequest("Admin cannot create Superuser");
            }

            if (currentUserRole == UserRole.Admin.ToString() && user.role == UserRole.Admin)
            {
                return BadRequest("Admin cannot create another Admin");
            }


            if (await _context.users.AnyAsync(v => v.u_email == user.u_email && v.role == user.role))
            {
                return BadRequest("User with this email and role already exists.");
            }

            user.u_password = HashPassword(user.u_password);
            _context.users.Add(user);
            await _context.SaveChangesAsync();

            await new ActivityLogController(_context, _configuration).ActivityLog(currentUserId, ActionType.Add, null);

            return Ok("User registered successfully");

        }

        //Login User
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var existingUser = await _context.users.FirstOrDefaultAsync(v => v.u_email == loginRequest.u_email);
            if (existingUser == null || !VerifyPassword(loginRequest.u_password, existingUser.u_password))
            {
                return Unauthorized("Invalid Credentials");
            }

            //Generate JWT Token

            var token = GenerateJwtToken(existingUser);
            return Ok(new { token });
        }

        //Update User
        [Authorize(Roles = "Superuser, Admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUser updateUser)
        {
            var existingUser = await _context.users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            if (updateUser.u_id != id)
            {
                return BadRequest("User ID cannot be changed.");
            }


            existingUser.u_name = updateUser.u_name ?? existingUser.u_name;
            existingUser.u_email = updateUser.u_email ?? existingUser.u_email;
            //existingUser.role = updateUser.role ?? existingUser.role;
            existingUser.contact_no = updateUser.contact_no ?? existingUser.contact_no;


            //if (updateUser.role != null)
            //{
            //    existingUser.role = updateUser.role;
            //}

            //Hash password only if new one is provided
            if (!string.IsNullOrEmpty(updateUser.u_password))
            {
                existingUser.u_password = HashPassword(updateUser.u_password);
            }

            await _context.SaveChangesAsync();

            var currentUserId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            await new ActivityLogController(_context, _configuration).ActivityLog(currentUserId, ActionType.Update, null);

            return Ok(new { message = "User Updated successfully." });

        }

        //Delete User
        [Authorize(Roles = "Superuser, Admin")]
        [HttpDelete("delete/{id}")]

        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }

            //user.u_name ??= "Unknown";
            //user.u_email ??= "unknown@example.com";

            var adminUser = await _context.users.FirstOrDefaultAsync(u => u.role == UserRole.Admin);
            if(adminUser == null)
            {
                return BadRequest("Cannot delete user. No admin found to reassign leads.");
            }

            var userLeads = await _context.leads.Where(l => l.assigned_to == id).ToListAsync();
            foreach(var lead in userLeads)
            {
                lead.assigned_to = adminUser.u_id;
            }
            await _context.SaveChangesAsync();

            _context.users.Remove(user);
            await _context.SaveChangesAsync();

            var currentUserId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            await new ActivityLogController(_context, _configuration).ActivityLog(currentUserId, ActionType.Delete, null);

            return Ok("User Deleted Successfully");
        }


        [Authorize]
        // Get All User
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.users
                .Select(v => new
                {
                    Id = v.u_id,
                    Name = v.u_name ?? "N/A",
                    Email = v.u_email ?? "N/A",
                    Role = v.role,
                    contactNo = v.contact_no ?? "N/A",
                })
                .ToListAsync();
            return Ok(users);
        }



        //Generate JWT Token function
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.u_id.ToString()),
                new Claim(ClaimTypes.Email, user.u_email),
                new Claim(ClaimTypes.Role, user.role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Password Hashing Function
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedbytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedbytes);
        }


        //Password Verification function
        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            enteredPassword = HashPassword(enteredPassword); //try
            return enteredPassword == storedPassword;
        }

        //Method to add Logs in activity_log table
        //private async Task LogActivity(int userId, ActionType actionType, int? targetUserId)
        //{
        //    var log = new ActivityLog
        //    {
        //        u_id = userId,
        //        lead_id = targetUserId,
        //        action_type = actionType,// Using lead_id field for tracking user activities
        //        action_date = DateTime.UtcNow
        //    };

        //    _context.activity_log.Add(log);
        //    await _context.SaveChangesAsync();
        //}
    }
}
