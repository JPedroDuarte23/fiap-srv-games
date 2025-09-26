using FiapSrvGames.Application.DTOs;
using FiapSrvGames.Domain.Entities;

namespace FiapSrvGames.Application.Interfaces
{
    public interface IGameService
    {
        Task<IEnumerable<Game>> GetAllAsync();
        Task<IEnumerable<Game>> GetAllByPublisherAsync(Guid publisherId);
        Task<Game> GetByIdAsync(Guid id);
        Task CreateAsync(Guid publisherID, CreateGameDto dto);
        Task UpdateAsync(Guid id, UpdateGameDto dto);
        Task<IReadOnlyCollection<Game>> SearchAsync(string queryText);

        Task<IEnumerable<Game>> GetMostPopularGamesAsync(int count);
    }
}
