using Microsoft.EntityFrameworkCore;
using PublicPolls.Domain.Entities;
using PublicPolls.Domain.Interfaces;
using PublicPolls.Infrastructure.Data;

namespace PublicPolls.Infrastructure.Repositories;

public class ResponseRepository : IResponseRepository
{
    private readonly AppDbContext _context;

    public ResponseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Response?> GetByIdAsync(Guid id)
    {
        return await _context.Responses
            .Include(r => r.Answers)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Response>> GetBySurveyIdAsync(Guid surveyId)
    {
        return await _context.Responses
            .Where(r => r.SurveyId == surveyId)
            .Include(r => r.Answers)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();
    }

    public async Task<Response> CreateAsync(Response response)
    {
        _context.Responses.Add(response);
        await _context.SaveChangesAsync();
        return response;
    }

    public async Task<int> GetCountBySurveyIdAsync(Guid surveyId)
    {
        return await _context.Responses.CountAsync(r => r.SurveyId == surveyId);
    }

    public async Task<bool> HasRespondedAsync(Guid surveyId, string? ip)
    {
        if (string.IsNullOrEmpty(ip)) return false;
        return await _context.Responses.AnyAsync(r => r.SurveyId == surveyId && r.RespondentIp == ip);
    }

    public async Task<IEnumerable<AnswerCount>> GetAnswerCountsAsync(Guid surveyId)
    {
        return await _context.Answers
            .Where(a => a.Response!.SurveyId == surveyId)
            .GroupBy(a => new { a.QuestionId, a.OptionId })
            .Select(g => new AnswerCount
            {
                QuestionId = g.Key.QuestionId,
                OptionId = g.Key.OptionId,
                Count = g.Count()
            })
            .ToListAsync();
    }
}
