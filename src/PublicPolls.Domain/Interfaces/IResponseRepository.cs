using PublicPolls.Domain.Entities;

namespace PublicPolls.Domain.Interfaces;

public interface IResponseRepository
{
    Task<Response?> GetByIdAsync(Guid id);
    Task<IEnumerable<Response>> GetBySurveyIdAsync(Guid surveyId);
    Task<Response> CreateAsync(Response response);
    Task<int> GetCountBySurveyIdAsync(Guid surveyId);
    Task<bool> HasRespondedAsync(Guid surveyId, string? ip);
    Task<IEnumerable<AnswerCount>> GetAnswerCountsAsync(Guid surveyId);
}

public class AnswerCount
{
    public Guid QuestionId { get; set; }
    public Guid OptionId { get; set; }
    public int Count { get; set; }
}
