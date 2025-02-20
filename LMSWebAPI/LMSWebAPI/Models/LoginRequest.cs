using System.ComponentModel.DataAnnotations;

namespace LMSWebAPI.Models
{
    public class LoginRequest
    {
        [Required]
        public string u_email { get; set; }
        [Required]
        public string u_password { get; set; }

        [Required]
        public UserRole role { get; set; }
    }
}
