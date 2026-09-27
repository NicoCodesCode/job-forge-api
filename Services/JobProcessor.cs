using JobForgeAPI.Data;
using JobForgeAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobForgeAPI.Services;

public class JobProcessor
{
    private readonly AppDbContext _context;

    public JobProcessor(AppDbContext context)
    {
        _context = context;
    }

    public async Task ProcessJobAsync(int jobId)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job is null) return;
        
        job.Status = JobStatus.Processing;
        await _context.SaveChangesAsync();

        await Task.Delay(3000); // pretend to do real work
        
        job.Status = JobStatus.Succeeded;
        job.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}