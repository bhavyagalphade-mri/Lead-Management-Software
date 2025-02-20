using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace LMSWebAPI.Models
{
    public class LeadLog
    {
        [Key]
        public int status_id {  get; set; }  
        
        [Required]
        public int lead_id { get; set; }

        [Required]
        public string new_status { get; set; }

        [Required]
        public string old_status { get; set; }

        [Required]
        public int update_by { get; set; }

    }
}
