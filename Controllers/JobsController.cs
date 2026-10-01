using Hangfire;
using JobForgeAPI.Data;
using JobForgeAPI.Models;
using JobForgeAPI.Models.Enums;
using JobForgeAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobForgeAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public JobsController(AppDbContext context, IBackgroundJobClient backgroundJobClient)
    {
        _context = context;
        _backgroundJobClient = backgroundJobClient;
    }

    private int GetCurrentOrganizationId() => int.Parse(User.FindFirst("organizationId")?.Value!);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var organizationId = GetCurrentOrganizationId();
        return Ok(await _context.Jobs.Where(j => j.OrganizationId == organizationId).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateJobRequest request)
    {
        var newJob = new Job
        {
            Type = request.Type,
            Payload = request.Payload,
            Status = JobStatus.Pending,
            OrganizationId = GetCurrentOrganizationId()
        };
        
        await _context.Jobs.AddAsync(newJob);
        await _context.SaveChangesAsync();

        _backgroundJobClient.Enqueue<JobProcessor>(processor => processor.ProcessJobAsync(newJob.Id));
        
        return Ok(newJob);
    }
}