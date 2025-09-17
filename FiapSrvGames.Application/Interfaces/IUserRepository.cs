using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiapSrvGames.Domain.Entities;

namespace FiapSrvGames.Application.Interfaces;


public interface IUserRepository
{
    Task CreateAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Player>> GetPlayersAsync();
    Task<IEnumerable<Publisher>> GetPublishersAsync();
}
