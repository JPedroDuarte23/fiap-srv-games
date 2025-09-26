// Garanta que todas estas diretivas using estão no topo do seu arquivo
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FiapSrvGames.Application.Interfaces;
using FiapSrvGames.Domain.Entities;
using Microsoft.Extensions.Logging;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.TermVectors;

namespace FiapSrvGames.Application.Services;

public class RecommendationService : IRecommendationService
{
    private const string GenresKeywordField = "genres.keyword";
    private const string TagsKeywordField = "tags.keyword";
    private readonly IUserRepository _userRepository;
    private readonly ElasticsearchClient _esClient;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(IUserRepository userRepository, ElasticsearchClient esClient, ILogger<RecommendationService> logger)
    {
        _userRepository = userRepository;
        _esClient = esClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Game>> GetRecommendationsForUser(Guid userId)
    {
        var player = await _userRepository.GetByIdAsync(userId) as Player;
        if (player is null || player.Library is null || !player.Library.Any())
        {
            return Array.Empty<Game>();
        }

        var userGameIds = player.Library;
        var elasticUserGameIds = new Ids(userGameIds.Select(id => new Id(id.ToString())));

        SearchResponse<Game> aggregationResponse;
        try
        {
            aggregationResponse = await _esClient.SearchAsync<Game>(s => s
               .Index("games")
               .Query(q => q.Ids(i => i.Values(elasticUserGameIds)))
               .Aggregations(aggs => aggs
                   .Add("top_genres", genreAgg => genreAgg
                       .Terms(t => t.Field(GenresKeywordField).Size(3))
                    )
                   .Add("top_tags", tagAgg => tagAgg
                       .Terms(t => t.Field(TagsKeywordField).Size(3))
                    )
                )
               .Size(0)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha na consulta de agregação para o usuário {UserId}", userId);
            return Array.Empty<Game>();
        }

        if (!aggregationResponse.IsValidResponse)
        {
            _logger.LogError("A resposta da consulta de agregação não foi válida. DebugInformation: {DebugInfo}", aggregationResponse.DebugInformation);
            if (aggregationResponse.ElasticsearchServerError is not null)
            {
                _logger.LogError("Erro do servidor Elasticsearch: {Error}", aggregationResponse.ElasticsearchServerError.ToString());
            }
            return Array.Empty<Game>();
        }

        var topGenres = aggregationResponse.Aggregations?
       .GetStringTerms("top_genres")?.Buckets
       .Select(b => b.Key.ToString()).ToList() ?? new List<string>();

        var topTags = aggregationResponse.Aggregations?
          .GetStringTerms("top_tags")?.Buckets
          .Select(b => b.Key.ToString()).ToList() ?? new List<string>();


        if (!topGenres.Any() && !topTags.Any())
        {
            _logger.LogInformation("Nenhum gênero ou tag significativo encontrado para o usuário {UserId}", userId);
            return Array.Empty<Game>();
        }

        SearchResponse<Game> recommendationsResponse;
        try
        {
            recommendationsResponse = await _esClient.SearchAsync<Game>(s => s
               .Index("games")
               .Query(q => q
                   .Bool(b => b
                       .MustNot(mn => mn.Ids(i => i.Values(elasticUserGameIds)))
                       .Should(sh => sh
                           .Terms(t => t
                               .Field(GenresKeywordField)
                               .Terms(new TermsQueryField(topGenres.Select(FieldValue.String).ToList()))
                               .Boost((float?)2.0)
                            )
                           .Terms(t => t
                               .Field(TagsKeywordField)
                               .Terms(new TermsQueryField(topTags.Select(FieldValue.String).ToList()))
                            )
                        )
                       .MinimumShouldMatch(1)
                    )
                )
               .Size(10)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha na consulta de recomendação para o usuário {UserId}", userId);
            return Array.Empty<Game>();
        }

        if (!recommendationsResponse.IsValidResponse)
        {
            _logger.LogError("A resposta da consulta de recomendação não foi válida. DebugInformation: {DebugInfo}", recommendationsResponse.DebugInformation);
            return Array.Empty<Game>();
        }

        return recommendationsResponse.Documents;
    }
}