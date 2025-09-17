using System.Diagnostics.CodeAnalysis;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Domain.Entities;
using MongoDB.Driver;

namespace FiapSrvGames.Infrastructure.Repository;

[ExcludeFromCodeCoverage]
public class GameRepository : IGameRepository
{
    private readonly IMongoCollection<Game> _collection;

    public GameRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Game>("games");
    }

    public async Task CreateAsync(Game game)
    {
        await _collection.InsertOneAsync(game);
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetAllByPublisherAsync(Guid publisherId)
    {
        return await _collection.Find(g => g.Publisher == publisherId).ToListAsync();
    }

    public async Task<Game> GetByIdAsync(Guid id)
    {
        return await _collection.Find(g => g.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Game>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _collection
            .Find(g => ids.Contains(g.Id))
            .ToListAsync();
    }

    public async Task UpdateAsync(Game game)
    {
        await _collection.ReplaceOneAsync(g => g.Id == game.Id, game);
    }
}
