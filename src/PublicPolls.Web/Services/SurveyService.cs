using System.Net.Http.Json;
using System.Text.Json;

namespace PublicPolls.Web.Services;

public interface ISurveyService
{
    Task<List<SurveyListItem>> GetMySurveysAsync();
    Task<SurveyDetail?> GetByIdAsync(Guid id);
    Task<SurveyDetail?> GetPublicAsync(string publicUrl);
    Task<SurveyDetail> CreateAsync(CreateSurveyRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<SubmitResult> SubmitResponseAsync(Guid surveyId, List<AnswerRequest> answers);
    Task<SurveyResults?> GetResultsAsync(Guid id);
}

public class SurveyService : ISurveyService
{
    private readonly HttpClient _http;

    public SurveyService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<SurveyListItem>> GetMySurveysAsync()
    {
        var response = await _http.GetFromJsonAsync<List<SurveyListItem>>("api/surveys");
        return response ?? new List<SurveyListItem>();
    }

    public async Task<SurveyDetail?> GetByIdAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<SurveyDetail>($"api/surveys/{id}");
    }

    public async Task<SurveyDetail?> GetPublicAsync(string publicUrl)
    {
        try
        {
            return await _http.GetFromJsonAsync<SurveyDetail>($"api/surveys/{publicUrl}/public");
        }
        catch
        {
            return null;
        }
    }

    public async Task<SurveyDetail> CreateAsync(CreateSurveyRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/surveys", request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            string errorMessage = $"Erro ao criar pesquisa: {response.StatusCode}";
            
            try 
            {
                var problem = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (problem.TryGetProperty("detail", out var detail))
                    errorMessage = detail.GetString() ?? errorMessage;
                else if (problem.TryGetProperty("message", out var message))
                    errorMessage = message.GetString() ?? errorMessage;
                else if (problem.TryGetProperty("error", out var error))
                    errorMessage = error.GetString() ?? errorMessage;
            }
            catch {}
            
            throw new Exception(errorMessage);
        }

        return await response.Content.ReadFromJsonAsync<SurveyDetail>() 
            ?? throw new Exception("Erro ao criar pesquisa");
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/surveys/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<SubmitResult> SubmitResponseAsync(Guid surveyId, List<AnswerRequest> answers)
    {
        var response = await _http.PostAsJsonAsync($"api/surveys/{surveyId}/responses", new { answers });
        return await response.Content.ReadFromJsonAsync<SubmitResult>() 
            ?? new SubmitResult { Success = false, Error = "Erro de comunicação" };
    }

    public async Task<SurveyResults?> GetResultsAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<SurveyResults>($"api/surveys/{id}/results");
    }
}

public class SurveyListItem
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

public class SurveyDetail
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public List<QuestionItem> Questions { get; set; } = new();
}

public class QuestionItem
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public List<OptionItem> Options { get; set; } = new();
}

public class OptionItem
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class CreateSurveyRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CreateQuestionRequest> Questions { get; set; } = new();
}

public class CreateQuestionRequest
{
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public List<CreateOptionRequest> Options { get; set; } = new();
}

public class CreateOptionRequest
{
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class AnswerRequest
{
    public Guid QuestionId { get; set; }
    public Guid OptionId { get; set; }
}

public class SubmitResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Guid? ResponseId { get; set; }
}

public class SurveyResults
{
    public Guid SurveyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalResponses { get; set; }
    public List<QuestionResult> Questions { get; set; } = new();
}

public class QuestionResult
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TotalAnswers { get; set; }
    public List<OptionResult> Options { get; set; } = new();
}

public class OptionResult
{
    public Guid OptionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}
