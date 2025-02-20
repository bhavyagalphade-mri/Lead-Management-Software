using Lms.Models;
using LMSWebAPI.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;


        public ActivityLogController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpGet("activitylog")]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> GetActivityLog()
        {
            return await _context.activity_log.ToListAsync();
        }

        [HttpGet("activitylog/{id}")]
        public async Task<ActionResult<ActivityLog>> GetActivityLog(int id)
        {
            var activity = await _context.activity_log.FindAsync(id);

            if (activity == null)
            {
                return NotFound();
            }

            return Ok(activity);
        }

        [NonAction]
        public async Task ActivityLog(int userId, Models.ActionType actionType, int? leadId)
        {
            if (leadId.HasValue)
            {
                var leadExists = await _context.leads.AnyAsync(l => l.lead_id == leadId);
                if (!leadExists)
                {
                    throw new Exception("Lead does not exist. Activity log failed.");
                }
            }

            var log = new ActivityLog
            {
                u_id = userId,
                action_type = actionType,
                lead_id = leadId,
                action_date = DateTime.Now
            };

            _context.activity_log.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
