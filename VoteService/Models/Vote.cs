namespace VoteService.Models;

public class Vote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PollCode { get; set; } = string.Empty;
    public int OptionIndex { get; set; }
    public string VoterToken { get; set; } = string.Empty;
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
}