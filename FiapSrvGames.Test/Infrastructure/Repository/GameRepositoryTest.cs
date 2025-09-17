using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiapSrvGames.Domain.Entities;
using FiapSrvGames.Infrastructure.Repository;
using MongoDB.Driver;
using Moq;
using Xunit;

public class GameRepositoryTest
{
    private readonly Mock<IMongoCollection<Game>> _collectionMock;
    private readonly Mock<IMongoDatabase> _databaseMock;
    private readonly GameRepository _repository;

    public GameRepositoryTest()
    {
        _collectionMock = new Mock<IMongoCollection<Game>>();
        _databaseMock = new Mock<IMongoDatabase>();
        _databaseMock.Setup(db => db.GetCollection<Game>("games", null)).Returns(_collectionMock.Object);
        _repository = new GameRepository(_databaseMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsInsertOneAsync()
    {
        var game = new Game { Id = Guid.NewGuid(), Title = "Test Game" };
        _collectionMock.Setup(c => c.InsertOneAsync(game, null, default)).Returns(Task.CompletedTask);

        await _repository.CreateAsync(game);

        _collectionMock.Verify(c => c.InsertOneAsync(game, null, default), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsGames()
    {
        // Arrange
        var games = new List<Game> { new Game { Id = Guid.NewGuid(), Title = "Game1" } };

        // Mock do cursor
        var cursorMock = new Mock<IAsyncCursor<Game>>();
        cursorMock.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        cursorMock.SetupGet(c => c.Current).Returns(games);

        // Mock do FindAsync
        _collectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Game>>(),
                It.IsAny<FindOptions<Game, Game>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Game1", result.First().Title);
    }



    [Fact]
    public async Task GetAllByPublisherAsync_ReturnsGames()
    {
        var publisherId = Guid.NewGuid();
        var games = new List<Game> { new Game { Id = Guid.NewGuid(), Publisher = publisherId } };

        // Mock do cursor
        var cursorMock = new Mock<IAsyncCursor<Game>>();
        cursorMock.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        cursorMock.SetupGet(c => c.Current).Returns(games);

        // Mock do FindAsync
        _collectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Game>>(),
                It.IsAny<FindOptions<Game, Game>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        var result = await _repository.GetAllByPublisherAsync(publisherId);

        Assert.Single(result);
        Assert.Equal(publisherId, result.First().Publisher);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsGame()
    {
        var gameId = Guid.NewGuid();
        var game = new Game { Id = gameId, Title = "Game" };
        var games = new List<Game> { game };

        // Mock do cursor
        var cursorMock = new Mock<IAsyncCursor<Game>>();
        cursorMock.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        cursorMock.SetupGet(c => c.Current).Returns(games);

        // Mock do FindAsync
        _collectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Game>>(),
                It.IsAny<FindOptions<Game, Game>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        var result = await _repository.GetByIdAsync(gameId);

        Assert.NotNull(result);
        Assert.Equal(gameId, result.Id);
    }


    [Fact]
    public async Task GetByIdsAsync_ReturnsGames()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var games = ids.Select(id => new Game { Id = id, Title = $"Game-{id}" }).ToList();

        var cursorMock = new Mock<IAsyncCursor<Game>>();
        cursorMock.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        cursorMock.SetupGet(c => c.Current).Returns(games);

        _collectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Game>>(),
                It.IsAny<FindOptions<Game, Game>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        var result = await _repository.GetByIdsAsync(ids);

        Assert.Equal(2, result.Count());
    }


    [Fact]
    public async Task UpdateAsync_CallsReplaceOneAsync()
    {
        var game = new Game { Id = Guid.NewGuid(), Title = "Game" };
        _collectionMock.Setup(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Game>>(),
            game,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ReplaceOneResult>());

        await _repository.UpdateAsync(game);

        _collectionMock.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Game>>(),
            game,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
