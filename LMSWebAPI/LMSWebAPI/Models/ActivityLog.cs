using LMSWebAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Lms.Models;
public class ActivityLog
{
    [Key]
    public int activity_id { get; set; }

    public int u_id { get; set; }

    public int? lead_id { get; set; }

    public ActionType action_type { get; set; }

    public DateTime action_date { get; set; }

    //[JsonIgnore]
    //public virtual Lead Lead { get; set; } = null!;

    //[JsonIgnore]
    //public virtual User UIdNavigation { get; set; } = null!;
}