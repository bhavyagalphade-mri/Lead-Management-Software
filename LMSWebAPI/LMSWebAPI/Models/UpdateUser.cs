using System.ComponentModel.DataAnnotations;
using System.Globalization;


namespace LMSWebAPI.Models
{
    public class UpdateUser
    {
        [Key]
        public int u_id { get; set; }

        public string u_name { get; set; }
        
        public string u_email { get; set; }

        public string u_password { get; set; }
        public UserRole role { get; set; }

        public string contact_no { get; set; }

    }
}
