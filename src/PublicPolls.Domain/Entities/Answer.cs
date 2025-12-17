namespace PublicPolls.Domain.Entities;

public class Answer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ResponseId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid OptionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Response? Response { get; set; }
    public virtual Question? Question { get; set; }
    public virtual Option? Option { get; set; }
}
