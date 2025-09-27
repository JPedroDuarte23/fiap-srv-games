using System.Diagnostics.CodeAnalysis;

namespace FiapSrvGames.Infrastructure.Mappings;

[ExcludeFromCodeCoverage]
public static class MongoMappings
{
    public static void ConfigureMappings() 
    {
        UserMapping.Configure();
        GameMapping.Configure();
        AuditEventMapping.Configure();
    }
}