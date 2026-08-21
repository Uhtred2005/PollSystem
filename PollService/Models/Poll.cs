namespace PollService.Models;

public enum PollStatus
{
    Open,
    Closed
}

public class Poll
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public PollStatus Status { get; set; } = PollStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}