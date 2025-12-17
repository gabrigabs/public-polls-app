namespace PublicPolls.Domain.Entities;

public class Response
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SurveyId { get; set; }
    public string? RespondentIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Survey? Survey { get; set; }
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
