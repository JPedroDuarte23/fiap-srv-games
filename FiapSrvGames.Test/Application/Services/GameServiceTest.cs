using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FiapSrvGames.Application.DTOs;
using FiapSrvGames.Application.Exceptions;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Application.Services;
using FiapSrvGames.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class GameServiceTest
{
    private readonly Mock<IGameRepository> _gameRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ILogger<GameService>> _loggerMock;
    private readonly GameService _service;

    public GameServiceTest()
    {
        _gameRepoMock = new Mock<IGameRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<GameService>>();
        _service = new GameService(_gameRepoMock.Object, _userRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesGame_WhenPublisherExists()
    {
        var publisherId = Guid.NewGuid();
        var dto = new CreateGameDto
        {
            Title = "Game",
            Description = "Desc",
            Price = 10,
            ReleaseDate = DateTime.UtcNow
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync(new Publisher());

        await _service.CreateAsync(publisherId, dto);

        _gameRepoMock.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundException_WhenPublisherNotFound()
    {
        var publisherId = Guid.NewGuid();
        var dto = new CreateGameDto
        {
            Title = "Game",
            Description = "Desc",
            Price = 10,
            ReleaseDate = DateTime.UtcNow
        };
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync((User)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(publisherId, dto));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsGames()
    {
        var games = new List<Game> { new Game { Id = Guid.NewGuid(), Title = "Game" } };
        _gameRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(games);

        var result = await _service.GetAllAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllByPublisherAsync_ReturnsGames_WhenPublisherExists()
    {
        var publisherId = Guid.NewGuid();
        var games = new List<Game> { new Game { Id = Guid.NewGuid(), Title = "Game" } };
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync(new Publisher());
        _gameRepoMock.Setup(r => r.GetAllByPublisherAsync(publisherId)).ReturnsAsync(games);

        var result = await _service.GetAllByPublisherAsync(publisherId);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllByPublisherAsync_ThrowsNotFoundException_WhenPublisherNotFound()
    {
        var publisherId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync((User)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAllByPublisherAsync(publisherId));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsGame_WhenGameExists()
    {
        var gameId = Guid.NewGuid();
        var game = new Game { Id = gameId, Title = "Game" };
        _gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

        var result = await _service.GetByIdAsync(gameId);

        Assert.NotNull(result);
        Assert.Equal(gameId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenGameNotFound()
    {
        var gameId = Guid.NewGuid();
        _gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(gameId));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesGame_WhenGameExists()
    {
        var gameId = Guid.NewGuid();
        var game = new Game { Id = gameId, Title = "Old", Description = "Old", Price = 1, ReleaseDate = DateTime.UtcNow };
        var dto = new UpdateGameDto { Title = "New", Description = "New", Price = 2, ReleaseDate = DateTime.UtcNow.AddDays(1) };
        _gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

        await _service.UpdateAsync(gameId, dto);

        _gameRepoMock.Verify(r => r.UpdateAsync(It.Is<Game>(g => g.Title == "New" && g.Description == "New" && g.Price == 2)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenGameNotFound()
    {
        var gameId = Guid.NewGuid();
        var dto = new UpdateGameDto { Title = "New", Description = "New", Price = 2, ReleaseDate = DateTime.UtcNow.AddDays(1) };
        _gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(gameId, dto));
    }
}
