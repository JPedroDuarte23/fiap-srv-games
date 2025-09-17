using FiapSrvGames.Application.Exceptions;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FiapSrvGames.Application.Services;

public class LibraryService : ILibraryService
{
    private readonly IUserRepository _userRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<LibraryService> _logger;
    public LibraryService(IUserRepository userRepository, IGameRepository gameRepository, ILogger<LibraryService> logger)
    {
        _userRepository = userRepository;
        _gameRepository = gameRepository;
        _logger = logger;
    }
    public async Task<IEnumerable<Game>> GetPlayerGamesAsync(Guid userId)
    {
        _logger.LogInformation("Buscando jogos na biblioteca do usuário {UserId}", userId);

        var user = (Player)await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Usuário {UserId} não encontrado ao buscar jogos na biblioteca", userId);
            throw new NotFoundException("Usuário não encontrado");
        }

        if (!user.Library.Any())
        {
            _logger.LogInformation("Usuário {UserId} não possui jogos na biblioteca", userId);
            return new List<Game>();
        }

        var games = await _gameRepository.GetByIdsAsync(user.Library);

        _logger.LogInformation("Retornados {Count} jogos da biblioteca do usuário {UserId}", games.Count(), userId);
        return games;
    }

    public async Task AddToLibraryAsync(Guid userId, List<Guid> games)
    {
        _logger.LogInformation("Adicionando {Count} jogos à biblioteca do usuário {UserId}", games.Count, userId);

        var user = (Player)await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Usuário {UserId} não encontrado ao tentar adicionar jogos à biblioteca", userId);
            throw new NotFoundException("Usuário não encontrado");
        }

        user.Library.AddRange(games);

        try
        {
            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Jogos adicionados com sucesso à biblioteca do usuário {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar jogos à biblioteca do usuário {UserId}", userId);
            throw new ModifyDatabaseException(ex.Message);
        }
    }

    public async Task RemoveFromLibraryAsync(Guid userId, Guid gameId)
    {
        _logger.LogInformation("Removendo jogo {GameId} da biblioteca do usuário {UserId}", gameId, userId);

        var user = (Player)await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Usuário {UserId} não encontrado ao tentar remover jogo da biblioteca", userId);
            throw new NotFoundException("Usuário não encontrado");
        }

        if (!user.Library.Contains(gameId))
        {
            _logger.LogWarning("Jogo {GameId} não está na biblioteca do usuário {UserId}", gameId, userId);
            throw new NotFoundException("Jogo não está na biblioteca.");
        }

        user.Library.Remove(gameId);

        try
        {
            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Jogo {GameId} removido com sucesso da biblioteca do usuário {UserId}", gameId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover jogo {GameId} da biblioteca do usuário {UserId}", gameId, userId);
            throw new ModifyDatabaseException(ex.Message);
        }
    }
}
