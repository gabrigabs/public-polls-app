using PublicPolls.Domain.Interfaces;

namespace PublicPolls.Application.Services;

public interface IResultsService
{
    Task<SurveyResultsDto?> GetResultsAsync(Guid surveyId, Guid userId);
}

public class SurveyResultsDto
{
    public Guid SurveyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalResponses { get; set; }
    public List<QuestionResultDto> Questions { get; set; } = new();
}

public class QuestionResultDto
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TotalAnswers { get; set; }
    public List<OptionResultDto> Options { get; set; } = new();
}

public class OptionResultDto
{
    public Guid OptionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ResultsService : IResultsService
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IResponseRepository _responseRepository;

    public ResultsService(
        ISurveyRepository surveyRepository,
        IResponseRepository responseRepository)
    {
        _surveyRepository = surveyRepository;
        _responseRepository = responseRepository;
    }

    public async Task<SurveyResultsDto?> GetResultsAsync(Guid surveyId, Guid userId)
    {
        var survey = await _surveyRepository.GetByIdWithQuestionsAsync(surveyId);
        if (survey == null || survey.UserId != userId) return null;

        var totalResponses = await _responseRepository.GetCountBySurveyIdAsync(surveyId);
        var answerCounts = await _responseRepository.GetAnswerCountsAsync(surveyId);
        var answerCountsList = answerCounts.ToList();

        var questionResults = survey.Questions.Select(q =>
        {
            var questionAnswers = answerCountsList.Where(a => a.QuestionId == q.Id).ToList();
            var totalQuestionAnswers = questionAnswers.Sum(a => a.Count);

            return new QuestionResultDto
            {
                QuestionId = q.Id,
                Text = q.Text,
                TotalAnswers = totalQuestionAnswers,
                Options = q.Options.Select(o =>
                {
                    var count = questionAnswers.FirstOrDefault(a => a.OptionId == o.Id)?.Count ?? 0;
                    return new OptionResultDto
                    {
                        OptionId = o.Id,
                        Text = o.Text,
                        Count = count,
                        Percentage = totalQuestionAnswers > 0 
                            ? Math.Round((double)count / totalQuestionAnswers * 100, 2) 
                            : 0
                    };
                }).ToList()
            };
        }).ToList();

        return new SurveyResultsDto
        {
            SurveyId = survey.Id,
            Title = survey.Title,
            TotalResponses = totalResponses,
            Questions = questionResults
        };
    }
}
