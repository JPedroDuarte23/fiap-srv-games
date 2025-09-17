using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FiapSrvGames.Application.Exceptions;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Application.Services;
using FiapSrvGames.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class LibraryServiceTest
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IGameRepository> _gameRepoMock;
    private readonly Mock<ILogger<LibraryService>> _loggerMock;
    private readonly LibraryService _service;

    public LibraryServiceTest()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _gameRepoMock = new Mock<IGameRepository>();
        _loggerMock = new Mock<ILogger<LibraryService>>();
        _service = new LibraryService(_userRepoMock.Object, _gameRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPlayerGamesAsync_ReturnsGames_WhenUserAndLibraryExist()
    {
        var userId = Guid.NewGuid();
        var player = new Player { Id = userId, Library = new List<Guid> { Guid.NewGuid() } };
        var games = new List<Game> { new Game { Id = player.Library[0], Title = "Game" } };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(player);
        _gameRepoMock.Setup(r => r.GetByIdsAsync(player.Library)).ReturnsAsync(games);

        var result = await _service.GetPlayerGamesAsync(userId);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetPlayerGamesAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((Player)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetPlayerGamesAsync(userId));
    }

    [Fact]
    public async Task GetPlayerGamesAsync_ReturnsEmpty_WhenLibraryIsEmpty()
    {
        var userId = Guid.NewGuid();
        var player = new Player { Id = userId, Library = new List<Guid>() };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(player);

        var result = await _service.GetPlayerGamesAsync(userId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task AddToLibraryAsync_AddsGames_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var player = new Player { Id = userId, Library = new List<Guid>() };
        var gamesToAdd = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(player);

        await _service.AddToLibraryAsync(userId, gamesToAdd);

        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<Player>(p => p.Library.Count == 2)), Times.Once);
    }

    [Fact]
    public async Task AddToLibraryAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((Player)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AddToLibraryAsync(userId, new List<Guid> { Guid.NewGuid() }));
    }

    [Fact]
    public async Task RemoveFromLibraryAsync_RemovesGame_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var player = new Player { Id = userId, Library = new List<Guid> { gameId } };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(player);

        await _service.RemoveFromLibraryAsync(userId, gameId);

        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<Player>(p => !p.Library.Contains(gameId))), Times.Once);
    }

    [Fact]
    public async Task RemoveFromLibraryAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((Player)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RemoveFromLibraryAsync(userId, gameId));
    }
}
