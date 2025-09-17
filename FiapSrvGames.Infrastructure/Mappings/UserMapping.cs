using System.Diagnostics.CodeAnalysis;
using FiapSrvGames.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace FiapSrvGames.Infrastructure.Mappings;

[ExcludeFromCodeCoverage]
public static class UserMapping
{
    public static void Configure()
    {

        if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
        {
            BsonClassMap.RegisterClassMap<User>(map =>
            {
                map.AutoMap();
                map.MapIdMember(u => u.Id)
                   .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                map.MapMember(u => u.Name).SetIsRequired(true);
                map.MapMember(u => u.Username).SetIsRequired(true);
                map.MapMember(u => u.Email).SetIsRequired(true);
                map.MapMember(u => u.PasswordHash).SetIsRequired(true);
                map.MapMember(u => u.BornDate).SetIsRequired(true);
                map.MapMember(u => u.CreatedAt).SetIsRequired(true);
                map.MapMember(u => u.Role).SetIsRequired(true);


            });
        }
        if (!BsonClassMap.IsClassMapRegistered(typeof(Player)))
        {
            BsonClassMap.RegisterClassMap<Player>(map =>
            {
                map.AutoMap();

                map.MapMember(p => p.Cpf).SetIsRequired(true);
                map.MapMember(p => p.Library);
                map.MapMember(p => p.Cart);
                map.MapMember(p => p.Wishlist);

                map.SetDiscriminator("Player"); 
            });
        }
        if (!BsonClassMap.IsClassMapRegistered(typeof(Publisher)))
        {
            BsonClassMap.RegisterClassMap<Publisher>(map =>
            {
                map.AutoMap();

                map.MapMember(p => p.PublisherName).SetIsRequired(true);
                map.MapMember(p => p.Cnpj).SetIsRequired(true);
                map.MapMember(p => p.CompanyName).SetIsRequired(true);
                map.MapMember(p => p.GamesPublished);
                map.MapMember(p => p.TeamMembers);

                map.SetDiscriminator("Publisher");
            });
        }
    }
}
