using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FiapSrvGames.Application.Interfaces;

namespace FiapSrvGames.API.Controllers;

[ApiController]
[Route("api/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("user")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> GetRecommendations()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.Name)!);
        var recommendedGames = await _recommendationService.GetRecommendationsForUser(userId);
        return Ok(recommendedGames);
    }
}
