using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json.Serialization;

namespace LMSWebAPI.Models
{
    public class User
    {
        [Key]
        public int u_id { get; set; }

        [Required]
        public string u_name { get; set; }
        [Required]
        public string u_email { get; set; }

        [Required]
        public string u_password { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole role { get; set; }

        public string contact_no { get; set; }
    }
}
