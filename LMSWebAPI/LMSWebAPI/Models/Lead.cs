using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LMSWebAPI.Models
{
    public class Lead
    {
        [Key]
        public int lead_id { get; set; }
        
        [Required]
        public string lead_name { get; set; } = null!;

        [Required]
        public string lead_email { get; set; } = null!;

        public string? lead_contact { get; set; }

        [Required] 
        public string lead_source { get; set; } = null!;

        public int assigned_to { get; set; }

        [Required]
        [Column(TypeName ="varchar(50)")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LeadStatus lead_status { get; set; }

        public DateTime created_at { get; set; }

        public DateTime? update_at { get; set; }

        //[JsonIgnore]
        //public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

        //[JsonIgnore]
        //public virtual User AssignedToNavigation { get; set; } = null!;

        //[JsonIgnore]
        //public virtual ICollection<LeadAssignment> LeadAssignments { get; set; } = new List<LeadAssignment>();

        //[JsonIgnore]
        //public virtual ICollection<LeadLog> LeadLogs { get; set; } = new List<LeadLog>();
    }
}
