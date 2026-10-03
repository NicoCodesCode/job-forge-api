using JobForgeAPI.Models.Enums;

namespace JobForgeAPI.Models;

public class Job
{
    public int Id { get; set; }
    
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    public int OrganizationId  { get; set; }
    public Organization? Organization { get; set; }
    
    public string? IdempotencyKey { get; set; }
}