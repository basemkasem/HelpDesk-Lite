using HelpDeskLite.Application.Common;
using HelpDeskLite.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await categoryService.GetCategoriesAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(categories));
    }
}
