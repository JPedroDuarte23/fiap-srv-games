
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FiapSrvGames.Domain.Entities;

[ExcludeFromCodeCoverage]
public class Player : User
{
    public string Cpf { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public List<Guid> Library { get; set; } = new();
    public List<Guid> Cart { get; set; } = new();
    public List<Guid> Wishlist { get; set; } = new();
}
