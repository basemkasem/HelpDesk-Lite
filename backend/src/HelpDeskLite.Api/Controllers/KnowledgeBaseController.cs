using HelpDeskLite.Application.Common;
using HelpDeskLite.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KnowledgeBaseController(IKnowledgeBaseService knowledgeBaseService) : ControllerBase
{
    [HttpGet("suggestions")]
    public async Task<ActionResult<ApiResponse<object>>> GetSuggestions(
        [FromQuery] string description,
        CancellationToken cancellationToken)
    {
        var suggestions = await knowledgeBaseService.GetSuggestionsAsync(description ?? string.Empty, cancellationToken);
        return Ok(ApiResponse<object>.Ok(suggestions));
    }
}
