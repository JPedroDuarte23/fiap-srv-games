using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapSrvGames.Domain.Entities;

[ExcludeFromCodeCoverage]
public class Publisher : User
{
    public string PublisherName { get; set; }
    public string Cnpj { get; set; }
    public string CompanyName { get; set; }
    public List<Guid> GamesPublished { get; set; } = new();
    public List<Guid> TeamMembers { get; set; } = new();
}
