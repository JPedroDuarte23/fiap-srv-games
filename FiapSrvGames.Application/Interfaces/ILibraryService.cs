using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiapSrvGames.Application.DTOs;
using FiapSrvGames.Domain.Entities;

namespace FiapSrvGames.Application.Interfaces
{
    public interface ILibraryService
    {
        Task<IEnumerable<Game>> GetPlayerGamesAsync(Guid userId);
        Task AddToLibraryAsync(Guid userId, List<Guid> games);
        Task RemoveFromLibraryAsync(Guid userId, Guid gameId);
    }
}
