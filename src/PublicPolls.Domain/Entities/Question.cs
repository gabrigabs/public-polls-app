namespace PublicPolls.Domain.Entities;

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SurveyId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Survey? Survey { get; set; }
    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
