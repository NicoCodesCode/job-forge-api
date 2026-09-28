namespace JobForgeAPI.Models;

public class User
{
    public int Id  { get; set; }
    
    public string Username  { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}