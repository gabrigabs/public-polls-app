using System.ComponentModel.DataAnnotations;
using PublicPolls.Domain.Entities;
using PublicPolls.Domain.Interfaces;

namespace PublicPolls.Application.Services;

public interface ISurveyService
{
    Task<SurveyDto?> GetByIdAsync(Guid id, Guid userId);
    Task<SurveyDto?> GetByPublicUrlAsync(string publicUrl);
    Task<IEnumerable<SurveyListDto>> GetByUserIdAsync(Guid userId);
    Task<SurveyDto> CreateAsync(CreateSurveyDto dto, Guid userId);
    Task<SurveyDto?> UpdateAsync(Guid id, UpdateSurveyDto dto, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}

public class SurveyDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}

public class SurveyListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public int ResponseCount { get; set; }
}

public class QuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public List<OptionDto> Options { get; set; } = new();
}

public class OptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class CreateSurveyDto
{
    [Required(ErrorMessage = "O título é obrigatório")]
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    [MinLength(1, ErrorMessage = "A pesquisa deve ter pelo menos uma pergunta")]
    public List<CreateQuestionDto> Questions { get; set; } = new();
}

public class CreateQuestionDto
{
    [Required(ErrorMessage = "O texto da pergunta é obrigatório")]
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    
    [MinLength(2, ErrorMessage = "A pergunta deve ter pelo menos duas opções")]
    public List<CreateOptionDto> Options { get; set; } = new();
}

public class CreateOptionDto
{
    [Required(ErrorMessage = "O texto da opção é obrigatório")]
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class UpdateSurveyDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}

public class SurveyService : ISurveyService
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IResponseRepository _responseRepository;

    public SurveyService(ISurveyRepository surveyRepository, IResponseRepository responseRepository)
    {
        _surveyRepository = surveyRepository;
        _responseRepository = responseRepository;
    }

    // ... (Get methods omitted for brevity as they are unchanged) ...
    public async Task<SurveyDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var survey = await _surveyRepository.GetByIdWithQuestionsAsync(id);
        if (survey == null || survey.UserId != userId) return null;
        return MapToDto(survey);
    }

    public async Task<SurveyDto?> GetByPublicUrlAsync(string publicUrl)
    {
        var survey = await _surveyRepository.GetByPublicUrlAsync(publicUrl);
        if (survey == null || !survey.IsOpen) return null;
        return MapToDto(survey);
    }

    public async Task<IEnumerable<SurveyListDto>> GetByUserIdAsync(Guid userId)
    {
        var surveys = await _surveyRepository.GetByUserIdAsync(userId);
        var result = new List<SurveyListDto>();
        
        foreach (var survey in surveys)
        {
            var count = await _responseRepository.GetCountBySurveyIdAsync(survey.Id);
            result.Add(new SurveyListDto
            {
                Id = survey.Id,
                Title = survey.Title,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                IsActive = survey.IsActive,
                PublicUrl = survey.PublicUrl,
                IsOpen = survey.IsOpen,
                ResponseCount = count
            });
        }
        
        return result;
    }

    public async Task<SurveyDto> CreateAsync(CreateSurveyDto dto, Guid userId)
    {
        // Validation Defense Check
        if (dto.Questions == null || !dto.Questions.Any())
            throw new ArgumentException("A pesquisa deve ter perguntas.");

        foreach (var q in dto.Questions)
        {
            if (string.IsNullOrWhiteSpace(q.Text))
                throw new ArgumentException("O texto da pergunta é obrigatório.");
                
            if (q.Options == null || q.Options.Count < 2)
                throw new ArgumentException("Cada pergunta deve ter pelo menos duas opções.");
                
            if (q.Options.Any(o => string.IsNullOrWhiteSpace(o.Text)))
                throw new ArgumentException("O texto das opções é obrigatório.");
        }

        var survey = new Survey
        {
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            PublicUrl = GeneratePublicUrl(),
            Questions = dto.Questions.Select(q => new Question
            {
                Text = q.Text,
                Order = q.Order,
                IsRequired = q.IsRequired,
                Options = q.Options.Select(o => new Option
                {
                    Text = o.Text,
                    Order = o.Order
                }).ToList()
            }).ToList()
        };

        await _surveyRepository.CreateAsync(survey);
        return MapToDto(survey);
    }

    public async Task<SurveyDto?> UpdateAsync(Guid id, UpdateSurveyDto dto, Guid userId)
    {
        var survey = await _surveyRepository.GetByIdAsync(id);
        if (survey == null || survey.UserId != userId) return null;

        if (dto.Title != null) survey.Title = dto.Title;
        if (dto.Description != null) survey.Description = dto.Description;
        if (dto.StartDate.HasValue) survey.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) survey.EndDate = dto.EndDate.Value;
        if (dto.IsActive.HasValue) survey.IsActive = dto.IsActive.Value;

        await _surveyRepository.UpdateAsync(survey);
        
        var updated = await _surveyRepository.GetByIdWithQuestionsAsync(id);
        return MapToDto(updated!);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var survey = await _surveyRepository.GetByIdAsync(id);
        if (survey == null || survey.UserId != userId) return false;

        await _surveyRepository.DeleteAsync(id);
        return true;
    }

    private static SurveyDto MapToDto(Survey survey)
    {
        return new SurveyDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            StartDate = survey.StartDate,
            EndDate = survey.EndDate,
            IsActive = survey.IsActive,
            PublicUrl = survey.PublicUrl,
            IsOpen = survey.IsOpen,
            Questions = survey.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Order = q.Order,
                IsRequired = q.IsRequired,
                Options = q.Options.Select(o => new OptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    Order = o.Order
                }).ToList()
            }).ToList()
        };
    }

    private static string GeneratePublicUrl()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }
}
