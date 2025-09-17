using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using FiapSrvGames.Application.DTOs;
using FiapSrvGames.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapSrvGames.API.Controllers;


[ApiController]
[Route("api/library")]
[ExcludeFromCodeCoverage]
public class LibraryController : ControllerBase
{
    private readonly ILibraryService _service;
    public LibraryController (ILibraryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Player")]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetPlayerGames()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.Name)!);
        var library = await _service.GetPlayerGamesAsync(userId);
        return Ok(library);
    }

    [HttpPost]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> AddToLibrary([FromBody] List<Guid> games)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.Name)!);
        await _service.AddToLibraryAsync(userId, games);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Player")]
    public async Task<IActionResult> RemoveGame(string id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.Name)!);
        var gameId = Guid.Parse(id);
        await _service.RemoveFromLibraryAsync(userId, gameId);
        return NoContent();
    }
}
