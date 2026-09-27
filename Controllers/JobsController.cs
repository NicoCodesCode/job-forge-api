using JobForgeAPI.Data;
using JobForgeAPI.Models;
using JobForgeAPI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobForgeAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _context;

    public JobsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _context.Jobs.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateJobRequest request)
    {
        var newJob = new Job
        {
            Type = request.Type,
            Payload = request.Payload,
            Status = JobStatus.Pending
        };
        
        await _context.Jobs.AddAsync(newJob);
        await _context.SaveChangesAsync();
        
        return Ok(newJob);
    }
}