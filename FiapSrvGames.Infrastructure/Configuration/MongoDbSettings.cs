
using System.Diagnostics.CodeAnalysis;

namespace FiapSrvGames.Infrastructure.Configuration;

[ExcludeFromCodeCoverage]
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
