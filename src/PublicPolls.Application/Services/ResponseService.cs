using PublicPolls.Domain.Entities;
using PublicPolls.Domain.Interfaces;

namespace PublicPolls.Application.Services;

public interface IResponseService
{
    Task<SubmitResponseResult> SubmitAsync(Guid surveyId, SubmitResponseDto dto, string? ip);
}

public class SubmitResponseDto
{
    public List<AnswerDto> Answers { get; set; } = new();
}

public class AnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid OptionId { get; set; }
}

public class SubmitResponseResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Guid? ResponseId { get; set; }
}

public class ResponseService : IResponseService
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IResponseRepository _responseRepository;

    public ResponseService(ISurveyRepository surveyRepository, IResponseRepository responseRepository)
    {
        _surveyRepository = surveyRepository;
        _responseRepository = responseRepository;
    }

    public async Task<SubmitResponseResult> SubmitAsync(Guid surveyId, SubmitResponseDto dto, string? ip)
    {
        var survey = await _surveyRepository.GetByIdWithQuestionsAsync(surveyId);
        if (survey == null)
        {
            return new SubmitResponseResult { Success = false, Error = "Pesquisa não encontrada" };
        }

        if (!survey.IsOpen)
        {
            return new SubmitResponseResult { Success = false, Error = "Pesquisa não está aberta" };
        }

        // Check if already responded from this IP
        if (await _responseRepository.HasRespondedAsync(surveyId, ip))
        {
            return new SubmitResponseResult { Success = false, Error = "Você já respondeu esta pesquisa" };
        }

        // Validate all required questions are answered
        var requiredQuestionIds = survey.Questions
            .Where(q => q.IsRequired)
            .Select(q => q.Id)
            .ToHashSet();
        
        var answeredQuestionIds = dto.Answers.Select(a => a.QuestionId).ToHashSet();
        
        if (!requiredQuestionIds.IsSubsetOf(answeredQuestionIds))
        {
            return new SubmitResponseResult { Success = false, Error = "Todas as perguntas obrigatórias devem ser respondidas" };
        }

        // Validate options belong to questions
        var validOptions = survey.Questions
            .SelectMany(q => q.Options.Select(o => new { q.Id, OptionId = o.Id }))
            .ToDictionary(x => x.OptionId, x => x.Id);

        foreach (var answer in dto.Answers)
        {
            if (!validOptions.TryGetValue(answer.OptionId, out var questionId) || questionId != answer.QuestionId)
            {
                return new SubmitResponseResult { Success = false, Error = "Resposta inválida" };
            }
        }

        var response = new Response
        {
            SurveyId = surveyId,
            RespondentIp = ip,
            Answers = dto.Answers.Select(a => new Answer
            {
                QuestionId = a.QuestionId,
                OptionId = a.OptionId
            }).ToList()
        };

        await _responseRepository.CreateAsync(response);

        return new SubmitResponseResult { Success = true, ResponseId = response.Id };
    }
}
