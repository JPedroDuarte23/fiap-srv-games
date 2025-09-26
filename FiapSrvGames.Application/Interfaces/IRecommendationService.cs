using FiapSrvGames.Domain.Entities;

namespace FiapSrvGames.Application.Interfaces;

public interface IRecommendationService
{
    Task<IReadOnlyCollection<Game>> GetRecommendationsForUser(Guid userId);
}
