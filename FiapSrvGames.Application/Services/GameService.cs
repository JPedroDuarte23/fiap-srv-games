using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiapSrvGames.Application.DTOs;
using FiapSrvGames.Application.Exceptions;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FiapSrvGames.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GameService> _logger;
    public GameService(IGameRepository gameRepository, IUserRepository userRepository, ILogger<GameService> logger)
    {
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task CreateAsync(Guid publisherId, CreateGameDto dto)
    {
        _logger.LogInformation("Iniciando criação de jogo para publisher {PublisherId}", publisherId);

        var user = await _userRepository.GetByIdAsync(publisherId);
        if(user == null)
        {
            _logger.LogWarning("Publisher {PublisherId} não encontrada ao tentar criar jogo", publisherId);
            throw new NotFoundException("Publisher não encontrada");
        }
            
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Publisher = publisherId,
            Description = dto.Description,
            Price = dto.Price,
            ReleaseDate = dto.ReleaseDate
        };

        try
        {
            await _gameRepository.CreateAsync(game);
            _logger.LogInformation("Jogo {GameId} criado com sucesso para publisher {PublisherId}", game.Id, publisherId);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar jogo para publisher {PublisherId}", publisherId);
            throw new ModifyDatabaseException(ex.Message);
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
            throw new NotFoundException("Publisher não encontrada");
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
            throw new NotFoundException("Jogo não encontrado");
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
            throw new NotFoundException("Jogo não encontrado");
        }

        game.Title = dto.Title;
        game.Price = dto.Price;
        game.Description = dto.Description;
        game.ReleaseDate = dto.ReleaseDate;

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
}
