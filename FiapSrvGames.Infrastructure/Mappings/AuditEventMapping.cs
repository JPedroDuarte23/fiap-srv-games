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
public class AuditEventMapping
{
    public static void Configure()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(AuditEvent)))
        {
            BsonClassMap.RegisterClassMap<AuditEvent>(map =>
            {
                map.AutoMap();
                map.MapIdMember(e => e.Id)
                   .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                map.MapMember(e => e.EntityId).SetIsRequired(true);
                map.MapMember(map => map.EventType).SetIsRequired(true);
                map.MapMember(map => map.EventData).SetIsRequired(true);
                map.MapMember(map => map.Timestamp).SetIsRequired(true);    
            });
        }
    }
}
