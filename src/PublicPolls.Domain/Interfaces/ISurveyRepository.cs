using PublicPolls.Domain.Entities;

namespace PublicPolls.Domain.Interfaces;

public interface ISurveyRepository
{
    Task<Survey?> GetByIdAsync(Guid id);
    Task<Survey?> GetByPublicUrlAsync(string publicUrl);
    Task<Survey?> GetByIdWithQuestionsAsync(Guid id);
    Task<IEnumerable<Survey>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Survey>> GetActiveAsync();
    Task<Survey> CreateAsync(Survey survey);
    Task<Survey> UpdateAsync(Survey survey);
    Task DeleteAsync(Guid id);
}
