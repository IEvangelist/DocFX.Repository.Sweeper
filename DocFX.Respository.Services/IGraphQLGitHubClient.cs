using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using System.Threading.Tasks;

namespace DocFX.Repository.Services
{
    public interface IGraphQLGitHubClient
    {
        Task<string> AddReactionAsync(string issueOrPullRequestId, ReactionContent reaction, string clientId);

        Task<string> RemoveReactionAsync(string issueOrPullRequestId, ReactionContent reaction, string clientId);
    }
}