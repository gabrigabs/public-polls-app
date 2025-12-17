using Microsoft.EntityFrameworkCore;
using PublicPolls.Domain.Entities;
using PublicPolls.Domain.Interfaces;
using PublicPolls.Infrastructure.Data;

namespace PublicPolls.Infrastructure.Repositories;

public class SurveyRepository : ISurveyRepository
{
    private readonly AppDbContext _context;

    public SurveyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Survey?> GetByIdAsync(Guid id)
    {
        return await _context.Surveys
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Survey?> GetByPublicUrlAsync(string publicUrl)
    {
        return await _context.Surveys
            .Include(s => s.Questions.OrderBy(q => q.Order))
                .ThenInclude(q => q.Options.OrderBy(o => o.Order))
            .FirstOrDefaultAsync(s => s.PublicUrl == publicUrl);
    }

    public async Task<Survey?> GetByIdWithQuestionsAsync(Guid id)
    {
        return await _context.Surveys
            .Include(s => s.Questions.OrderBy(q => q.Order))
                .ThenInclude(q => q.Options.OrderBy(o => o.Order))
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Survey>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Surveys
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Survey>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Surveys
            .Where(s => s.IsActive && s.StartDate <= now && s.EndDate >= now)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Survey> CreateAsync(Survey survey)
    {
        _context.Surveys.Add(survey);
        await _context.SaveChangesAsync();
        return survey;
    }

    public async Task<Survey> UpdateAsync(Survey survey)
    {
        survey.UpdatedAt = DateTime.UtcNow;
        _context.Surveys.Update(survey);
        await _context.SaveChangesAsync();
        return survey;
    }

    public async Task DeleteAsync(Guid id)
    {
        var survey = await _context.Surveys.FindAsync(id);
        if (survey != null)
        {
            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();
        }
    }
}
