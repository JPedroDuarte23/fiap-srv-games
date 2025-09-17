using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiapSrvGames.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace FiapSrvGames.Infrastructure.Mappings;

[ExcludeFromCodeCoverage]
public class GameMapping
{
    public static void Configure()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Game)))
        {
            BsonClassMap.RegisterClassMap<Game>(map =>
            {
                map.AutoMap();
                map.MapIdMember(g => g.Id)
                   .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                map.MapMember(g => g.Title).SetIsRequired(true);
                map.MapMember(g => g.Publisher).SetIsRequired(true);
                map.MapMember(g => g.Description).SetIsRequired(true);
                map.MapMember(g => g.Price).SetIsRequired(true);
                map.MapMember(g => g.ReleaseDate).SetIsRequired(true);
            });
        }
    }
}
