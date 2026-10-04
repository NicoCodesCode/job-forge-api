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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get()
    {
        var organizationId = GetCurrentOrganizationId();
        return Ok(await _context.Jobs.Where(j => j.OrganizationId == organizationId).ToListAsync());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Post([FromBody] CreateJobRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        var organizationId = GetCurrentOrganizationId();
        
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existingJob = await _context.Jobs.FirstOrDefaultAsync(
                j => j.OrganizationId == organizationId && j.IdempotencyKey == idempotencyKey
            );

            if (existingJob is not null) return Ok(existingJob);
        }
        
        var newJob = new Job
        {
            Type = request.Type,
            Payload = request.Payload,
            Status = JobStatus.Pending,
            OrganizationId = organizationId,
            IdempotencyKey = idempotencyKey,
        };
        
        await _context.Jobs.AddAsync(newJob);
        await _context.SaveChangesAsync();

        _backgroundJobClient.Enqueue<JobProcessor>(processor => processor.ProcessJobAsync(newJob.Id));
        
        return StatusCode(201, newJob);
    }
}