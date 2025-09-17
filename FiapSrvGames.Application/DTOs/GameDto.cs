using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapSrvGames.Application.DTOs;

[ExcludeFromCodeCoverage]
public class GameDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Publisher { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
    public DateTime ReleaseDate { get; set; }
}
