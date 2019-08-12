using DocFX.Repository.Services.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DocFX.Repository.Services
{
    public class GraphQLGitHubClient : IGraphQLGitHubClient
    {
        const string ProductID = "DocFX.Sweeper";
        const string ProductVersion = "1.0";

        readonly IConnection _connection;
        readonly string _owner;
        readonly string _repo;
        readonly GitHubOptions _config;
        readonly ILogger<GraphQLGitHubClient> _logger;

        public GraphQLGitHubClient(ILogger<GraphQLGitHubClient> logger, IOptions<GitHubOptions> config)
        {
            _logger = logger;
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _owner = _config.Owner;
            _repo = _config.Repo;

            _connection = new Connection(new ProductHeaderValue(ProductID, ProductVersion), _config.ApiToken);
        }

        public async Task<string> AddReactionAsync(string issueOrPullRequestId, ReactionContent reaction, string clientId)
        {
            var mutation =
                new Mutation()
                    .AddReaction(new AddReactionInput
                    {
                        ClientMutationId = clientId,
                        SubjectId = issueOrPullRequestId.ToGitHubId(),
                        Content = reaction
                    })
                    .Select(payload => new
                    {
                        payload.ClientMutationId
                    })
                    .Compile();

            var result = await _connection.Run(mutation);
            return result.ClientMutationId;
        }

        public async Task<string> RemoveReactionAsync(string issueOrPullRequestId, ReactionContent reaction, string clientId)
        {
            var mutation =
               new Mutation()
                   .RemoveReaction(new RemoveReactionInput
                   {
                       ClientMutationId = clientId,
                       SubjectId = issueOrPullRequestId.ToGitHubId(),
                       Content = reaction
                   })
                   .Select(payload => new
                   {
                       payload.ClientMutationId
                   })
                   .Compile();

            var result = await _connection.Run(mutation);
            return result.ClientMutationId;
        }
    }
}