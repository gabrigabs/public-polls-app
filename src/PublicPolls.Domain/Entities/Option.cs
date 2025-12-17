namespace PublicPolls.Domain.Entities;

public class Option
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Question? Question { get; set; }
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
