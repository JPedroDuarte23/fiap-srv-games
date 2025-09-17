using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using FiapSrvGames.Application.DTOs;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiapSrvGames.API.Controllers;


[ApiController]
[Route("api/games")]
[ExcludeFromCodeCoverage]
public class GameController : ControllerBase
{
    private readonly IGameService _service;

    public GameController(IGameService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Game>>> GetAll()
    {
        var games = await _service.GetAllAsync();
        return Ok(games);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Game>> GetById(Guid id)
    {
        var games = await _service.GetByIdAsync(id);
        return Ok(games);
    }

    [HttpPost]
    [Authorize(Roles = "Publisher")]
    public async Task<IActionResult> Create([FromBody] CreateGameDto dto)
    {
        var publisherId = Guid.Parse(User.FindFirstValue(ClaimTypes.Name)!);
        await _service.CreateAsync(publisherId, dto);
        return Created();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Publisher")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return Ok();
    }

}
