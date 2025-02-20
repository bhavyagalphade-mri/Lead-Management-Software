using System.ComponentModel.DataAnnotations;

namespace LMSWebAPI.Models
{
    public class LeadAssignment
    {
        [Key]
        public int assignment_id { get; set; }

        [Required]
        public int lead_id { get; set; }

        [Required]
        public int u_id { get; set; }

        public DateTime assignment_date { get; set; }
    }
}
