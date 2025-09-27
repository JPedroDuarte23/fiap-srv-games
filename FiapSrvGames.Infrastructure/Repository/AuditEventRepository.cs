using System.Diagnostics.CodeAnalysis;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Domain.Entities;
using MongoDB.Driver;

namespace FiapSrvGames.Infrastructure.Repository;

[ExcludeFromCodeCoverage]
public class AuditEventRepository : IAuditEventRepository
{
    private readonly IMongoCollection<AuditEvent> _collection;

    public AuditEventRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<AuditEvent>("game_events");
    }

    public async Task CreateAsync(AuditEvent auditEvent)
    {
        await _collection.InsertOneAsync(auditEvent);
    }
}
