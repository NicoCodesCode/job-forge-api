using Hangfire.States;
using Hangfire.Storage;
using JobForgeAPI.Data;
using JobForgeAPI.Models.Enums;

namespace JobForgeAPI.Services;

public class MarkJobFailedFilter : IApplyStateFilter
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MarkJobFailedFilter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is not FailedState) return;

        var jobId = int.Parse(context.BackgroundJob.Job.Args[0].ToString()!);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var job = dbContext.Jobs.FirstOrDefault(j => j.Id == jobId);
        if (job is null) return;

        job.Status = JobStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;
        dbContext.SaveChanges();
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }
}