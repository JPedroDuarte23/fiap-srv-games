using System.Diagnostics.CodeAnalysis;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Domain.Entities;
using MongoDB.Driver;

namespace FiapSrvGames.Infrastructure.Repository;

[ExcludeFromCodeCoverage]
public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;

    public UserRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<User>("users");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<User>(indexKeys, indexOptions);

        _collection.Indexes.CreateOne(indexModel);
    }

    public async Task CreateAsync(User user)
    {
        await _collection.InsertOneAsync(user);
    }
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _collection.InsertOneAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _collection.ReplaceOneAsync(u => u.Id == user.Id, user);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _collection.DeleteOneAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<Player>> GetPlayersAsync()
    {
        return await _collection.OfType<Player>().Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Publisher>> GetPublishersAsync()
    {
        return await _collection.OfType<Publisher>().Find(_ => true).ToListAsync();
    }
}
