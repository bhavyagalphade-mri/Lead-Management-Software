using Lms.Models;
using LMSWebAPI.Data;
using LMSWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace LMSWebAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]


    public class LeadController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public LeadController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("add-lead")] //add leads
        public async Task<IActionResult> AddLead([FromBody] Lead lead)
        {

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) 
            {
                return Unauthorized("User Id not found in token");
            }

            int userId = int.Parse(userIdClaim.Value);

            if (await _context.leads.AnyAsync(v => v.lead_email == lead.lead_email))
            {
                return BadRequest("Lead with this email already exists.");
            }

            _context.leads.Add(lead);
            await _context.SaveChangesAsync();


            var leadAssignment = new LeadAssignment
            {
                //assignment_id = 0,
                lead_id = lead.lead_id,
                u_id = userId,
                assignment_date = DateTime.UtcNow
            };

            _context.lead_assignment.Add(leadAssignment);
               await _context.SaveChangesAsync();

            await new ActivityLogController(_context, _configuration).ActivityLog(userId, ActionType.Add, lead.lead_id);

            return Ok("Lead added and assigned successfully");
        }


        [HttpPut("updateStatus/{id}")] //update status
        [Authorize]
        public async Task<IActionResult> UpdateLeadStatus(int id, [FromBody] string status)
        {
            var lead = await _context.leads.FirstOrDefaultAsync(l=> l.lead_id == id);
            if (lead == null) 
            {
                return NotFound(new { message = "Lead not found" });
            }

            var validStatuses = Enum.GetNames(typeof(LeadStatus));

            //var validStatuses = new List<string> {"New", "Contacted", "Follow-up", "Converted", "Lost"};

            //if (!Enum.TryParse<LeadStatus>(status, true, out var leadStatus))
            //{
            //    return BadRequest("Invalid lead status");
            //}
            if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Invalid lead status" });
            }


            var userIdclaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if(userIdclaim == null)
            {
                return Unauthorized("User ID not found in token");
            }


            int userId = int.Parse(userIdclaim.Value);


            string oldStatus = lead.lead_status.ToString();


            //It will update the lead status
            lead.lead_status = Enum.Parse<LeadStatus>(status, true);
            

            var leadLog = new LeadLog
            {
                lead_id = id,
                old_status = oldStatus,
                new_status = status,
                update_by = userId
            };

            _context.lead_log.Add(leadLog);

            await new ActivityLogController(_context, _configuration).ActivityLog(userId, ActionType.Update, lead.lead_id);

            return Ok(new { message = "Lead status updated successfully", lead });

            //await new ActivityLogController(_context, _configuration).ActivityLog(user.UId, ActionType.Add, null);         
        }

        //private async Task LogActivity(int userId, ActionType actionType, int? leadId)
        //{
        //    if (leadId.HasValue)
        //    {
        //        var leadExists = await _context.leads.AnyAsync(l => l.lead_id == leadId);
        //        if (!leadExists)
        //        {
        //            throw new Exception("Lead does not exist. Activity log failed.");
        //        }
        //    }

        //    var log = new ActivityLog
        //    {
        //        u_id = userId,
        //        lead_id = leadId,
        //        action_type = actionType,
        //        action_date = DateTime.UtcNow
        //    };

        //    _context.activity_log.Add(log);
        //    await _context.SaveChangesAsync();
        //}
    }
}