using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Elastic.Clients.Elasticsearch;
using FiapSrvGames.Application.DTOs;
using FiapSrvGames.Application.Exceptions;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Application.Services;
using FiapSrvGames.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class GameServiceTest
{
    private readonly Mock<IGameRepository> _gameRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IAuditEventRepository> _auditRepoMock;
    private readonly Mock<IAmazonSimpleNotificationService> _snsClientMock;
    private readonly Mock<ElasticsearchClient> _esClientMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<GameService>> _loggerMock;
    private readonly GameService _service;

    public GameServiceTest()
    {
        _gameRepoMock = new Mock<IGameRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _auditRepoMock = new Mock<IAuditEventRepository>();
        _snsClientMock = new Mock<IAmazonSimpleNotificationService>();
        _esClientMock = new Mock<ElasticsearchClient>();
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<GameService>>();

        // Setup da configuração para o Topic ARN
        _configMock.Setup(c => c["SnsTopics:GameEventsTopicArn"]).Returns("arn:aws:sns:us-east-2:123456789012:fake-topic");

        _service = new GameService(
            _gameRepoMock.Object,
            _userRepoMock.Object,
            _loggerMock.Object,
            _snsClientMock.Object,
            _esClientMock.Object,
            _configMock.Object,
            _auditRepoMock.Object
        );
    }

    // Testes para o método CreateAsync
    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldCreateGameAndLogAndPublish_WhenPublisherExists()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var dto = new CreateGameDto { Title = "Novo Jogo" };
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync(new Publisher());

        // Act
        await _service.CreateAsync(publisherId, dto);

        // Assert
        _gameRepoMock.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Once);
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditEvent>(e => e.EventType == "CreateGame")), Times.Once);
        _snsClientMock.Verify(s => s.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenPublisherDoesNotExist()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var dto = new CreateGameDto { Title = "Novo Jogo" };
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<FiapSrvGames.Application.Exceptions.NotFoundException>(() => _service.CreateAsync(publisherId, dto));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowModifyDatabaseException_WhenRepositoryFails()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var dto = new CreateGameDto { Title = "Novo Jogo" };
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync(new Publisher());
        _gameRepoMock.Setup(r => r.CreateAsync(It.IsAny<Game>())).ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<ModifyDatabaseException>(() => _service.CreateAsync(publisherId, dto));
    }

    [Fact]
    public async Task CreateAsync_ShouldLogAndContinue_WhenAuditRepositoryFails()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var dto = new CreateGameDto { Title = "Novo Jogo" };
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync(new Publisher());
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditEvent>())).ThrowsAsync(new Exception("Audit Error"));

        // Act
        await _service.CreateAsync(publisherId, dto);

        // Assert
        _gameRepoMock.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Once); // Garante que o jogo foi criado
        _snsClientMock.Verify(s => s.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()), Times.Once); // E que o evento SNS foi publicado
    }

    [Fact]
    public async Task CreateAsync_ShouldLogAndContinue_WhenSnsPublishFails()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var dto = new CreateGameDto { Title = "Novo Jogo" };
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync(new Publisher());
        _snsClientMock.Setup(s => s.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("SNS Error"));

        // Act
        await _service.CreateAsync(publisherId, dto);

        // Assert
        _gameRepoMock.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Once); // Garante que o jogo foi criado
        _auditRepoMock.Verify(r => r.CreateAsync(It.IsAny<AuditEvent>()), Times.Once); // E que o evento de auditoria foi logado
    }

    #endregion

    // Testes para os métodos de busca (Get)
    #region Get Methods Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnListOfGames()
    {
        // Arrange
        var games = new List<Game> { new Game(), new Game() };
        _gameRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(games);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnGame_WhenGameExists()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game { Id = gameId };
        _gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

        // Act
        var result = await _service.GetByIdAsync(gameId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gameId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game)null);

        // Act & Assert
        await Assert.ThrowsAsync<FiapSrvGames.Application.Exceptions.NotFoundException>(() => _service.GetByIdAsync(gameId));
    }

    [Fact]
    public async Task GetAllByPublisherAsync_ShouldReturnGames_WhenPublisherExists()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync(new Publisher());
        _gameRepoMock.Setup(r => r.GetAllByPublisherAsync(publisherId)).ReturnsAsync(new List<Game> { new Game() });

        // Act
        var result = await _service.GetAllByPublisherAsync(publisherId);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllByPublisherAsync_ShouldThrowNotFoundException_WhenPublisherDoesNotExist()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(publisherId)).ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<FiapSrvGames.Application.Exceptions.NotFoundException>(() => _service.GetAllByPublisherAsync(publisherId));
    }

    #endregion

    // Testes para o método UpdateAsync
    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldUpdateGameAndLog_WhenGameExists()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var dto = new UpdateGameDto { Title = "Título Atualizado" };
        var existingGame = new Game { Id = gameId, Title = "Título Antigo" };
        _gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(existingGame);

        // Act
        await _service.UpdateAsync(gameId, dto);

        // Assert
        _gameRepoMock.Verify(r => r.UpdateAsync(It.Is<Game>(g => g.Title == dto.Title)), Times.Once);
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditEvent>(e => e.EventType == "UpdateGame")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var dto = new UpdateGameDto();
        _gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game)null);

        // Act & Assert
        await Assert.ThrowsAsync<FiapSrvGames.Application.Exceptions.NotFoundException>(() => _service.UpdateAsync(gameId, dto));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowModifyDatabaseException_WhenRepositoryFails()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var dto = new UpdateGameDto();
        var existingGame = new Game { Id = gameId };
        _gameRepoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(existingGame);
        _gameRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Game>())).ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<ModifyDatabaseException>(() => _service.UpdateAsync(gameId, dto));
    }

    #endregion

    // Testes para os métodos de Métricas e Busca
    #region Metrics and Search Tests

    [Fact]
    public async Task GetMostPopularGamesAsync_ShouldReturnGamesFromRepository()
    {
        // Arrange
        var popularGames = new List<Game> { new Game(), new Game() };
        _gameRepoMock.Setup(r => r.GetTopByOwnershipCountAsync(5)).ReturnsAsync(popularGames);

        // Act
        var result = await _service.GetMostPopularGamesAsync(5);

        // Assert
        Assert.Equal(2, result.Count());
        _gameRepoMock.Verify(r => r.GetTopByOwnershipCountAsync(5), Times.Once);
    }

    #endregion
}