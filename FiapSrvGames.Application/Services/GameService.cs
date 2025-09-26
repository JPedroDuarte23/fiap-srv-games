using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using FiapSrvGames.Application.DTOs;
using FiapSrvGames.Application.Exceptions;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Elastic.Clients.Elasticsearch;

namespace FiapSrvGames.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GameService> _logger;
    private readonly string _topicArn;
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly ElasticsearchClient _esClient;
    public GameService(IGameRepository gameRepository, IUserRepository userRepository, ILogger<GameService> logger, IAmazonSimpleNotificationService snsClient, IConfiguration configuration)
    {
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _logger = logger;
        _topicArn = configuration["SnsTopics:GameEventsTopicArn"];
        _snsClient = snsClient;
    }

    public async Task CreateAsync(Guid publisherId, CreateGameDto dto)
    {
        _logger.LogInformation("Iniciando criação de jogo para publisher {PublisherId}", publisherId);

        var user = await _userRepository.GetByIdAsync(publisherId);
        if(user == null)
        {
            _logger.LogWarning("Publisher {PublisherId} não encontrada ao tentar criar jogo", publisherId);
            throw new Exceptions.NotFoundException("Publisher não encontrada");
        }

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Publisher = publisherId,
            Description = dto.Description,
            Price = dto.Price,
            ReleaseDate = dto.ReleaseDate,
            Genres = dto.Genres,
            Tags = dto.Tags ?? new List<Domain.Enums.GameTag>(),
            OwnershipCount = 0
        };

        try
        {
            await _gameRepository.CreateAsync(game);
            _logger.LogInformation("Jogo {Title} ({GameId}) criado com sucesso para publisher {PublisherId}", game.Title, game.Id, publisherId);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar jogo para publisher {PublisherId}", publisherId);
            throw new ModifyDatabaseException(ex.Message);  
        }

        try
        {
            var message = JsonSerializer.Serialize(game);
            var publishRequest = new PublishRequest
            {
                TopicArn = _topicArn,
                Message = message
            };  

            await _snsClient.PublishAsync(publishRequest);
            _logger.LogInformation("Evento de criação para o jogo {GameName} publicado no SNS.", game.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao publicar evento de criação do jogo no SNS.");
        }

    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        _logger.LogInformation("Buscando todos os jogos");
        
        var games = await _gameRepository.GetAllAsync();

        _logger.LogInformation("Retornados {Count} jogos", games.Count());

        return games;
    }

    public async Task<IEnumerable<Game>> GetAllByPublisherAsync(Guid publisherId)
    {
        _logger.LogInformation("Buscando jogos da publisher {PublisherId}", publisherId);

        var user = await _userRepository.GetByIdAsync(publisherId);
        if (user == null)
        {
            _logger.LogWarning("Publisher {PublisherId} não encontrada ao buscar jogos", publisherId);
            throw new Exceptions.NotFoundException("Publisher não encontrada");
        }


        var games = await _gameRepository.GetAllByPublisherAsync(publisherId);

        _logger.LogInformation("Retornados {Count} jogos para publisher {PublisherId}", games.Count(), publisherId);

        return games;
    }
    public async Task<Game> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Buscando jogo {GameId}", id);

        var game = await _gameRepository.GetByIdAsync(id);

        if (game == null)
        {
            _logger.LogWarning("Jogo {GameId} não encontrado", id);
            throw new Exceptions.NotFoundException("Jogo não encontrado");
        }

        _logger.LogInformation("Jogo {GameId} encontrado", id);

        return game;
    }
    public async Task UpdateAsync(Guid id, UpdateGameDto dto)
    {
        _logger.LogInformation("Iniciando atualização do jogo {GameId}", id);

        var game = await _gameRepository.GetByIdAsync(id);

        if (game == null)
        {
            _logger.LogWarning("Jogo {GameId} não encontrado para atualização", id);
            throw new Exceptions.NotFoundException("Jogo não encontrado");
        }

        game.Title = dto.Title;
        game.Price = dto.Price;
        game.Description = dto.Description;
        game.ReleaseDate = dto.ReleaseDate;
        game.Genres = dto.Genres;
        game.Tags = dto.Tags;

        try
        {
            await _gameRepository.UpdateAsync(game);
            _logger.LogInformation("Jogo {GameId} atualizado com sucesso", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar jogo {GameId}", id);
            throw new ModifyDatabaseException(ex.Message);
        }
    }

    public async Task<IReadOnlyCollection<Game>> SearchAsync(string queryText)
    {
        var response = await _esClient.SearchAsync<Game>(s => s
            .Index("games")
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(queryText)
                    .Fields("title,description")
                    .Fuzziness(new Fuzziness("AUTO"))
                )
            )
            .Size(20)
        );

        if (!response.IsSuccess())
        {
            _logger.LogError("Falha na busca no Elasticsearch: {Reason}", response.DebugInformation);
            return Array.Empty<Game>();
        }

        return response.Documents;
    }

    public async Task<IEnumerable<Game>> GetMostPopularGamesAsync(int count)
    {
        _logger.LogInformation("Buscando os {Count} jogos mais populares.", count);

        var popularGames = await _gameRepository.GetTopByOwnershipCountAsync(count);

        return popularGames;
    }
}

