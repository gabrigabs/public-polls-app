using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicPolls.Application.Services;

namespace PublicPolls.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SurveysController : ControllerBase
{
    private readonly ISurveyService _surveyService;
    private readonly IResponseService _responseService;
    private readonly IResultsService _resultsService;

    public SurveysController(
        ISurveyService surveyService,
        IResponseService responseService,
        IResultsService resultsService)
    {
        _surveyService = surveyService;
        _responseService = responseService;
        _resultsService = resultsService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
    }

    /// <summary>
    /// Lista todas as pesquisas do usuário autenticado
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<SurveyListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var surveys = await _surveyService.GetByUserIdAsync(GetUserId());
        return Ok(surveys);
    }

    /// <summary>
    /// Obtém detalhes de uma pesquisa específica
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var survey = await _surveyService.GetByIdAsync(id, GetUserId());
        if (survey == null) return NotFound();
        return Ok(survey);
    }

    /// <summary>
    /// Obtém pesquisa pública pelo código da URL (para respondentes)
    /// </summary>
    [HttpGet("{publicUrl}/public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublic(string publicUrl)
    {
        var survey = await _surveyService.GetByPublicUrlAsync(publicUrl);
        if (survey == null) return NotFound(new { error = "Pesquisa não encontrada ou não está aberta" });
        return Ok(survey);
    }

    /// <summary>
    /// Cria uma nova pesquisa
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSurveyDto dto)
    {
        var survey = await _surveyService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = survey.Id }, survey);
    }

    /// <summary>
    /// Atualiza uma pesquisa existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSurveyDto dto)
    {
        var survey = await _surveyService.UpdateAsync(id, dto, GetUserId());
        if (survey == null) return NotFound();
        return Ok(survey);
    }

    /// <summary>
    /// Remove uma pesquisa
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _surveyService.DeleteAsync(id, GetUserId());
        if (!deleted) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Submete resposta a uma pesquisa (endpoint público)
    /// </summary>
    [HttpPost("{id:guid}/responses")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SubmitResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SubmitResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitResponse(Guid id, [FromBody] SubmitResponseDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _responseService.SubmitAsync(id, dto, ip);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Obtém resultados sumarizados de uma pesquisa
    /// </summary>
    [HttpGet("{id:guid}/results")]
    [Authorize]
    [ProducesResponseType(typeof(SurveyResultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResults(Guid id)
    {
        var results = await _resultsService.GetResultsAsync(id, GetUserId());
        if (results == null) return NotFound();
        return Ok(results);
    }
}
