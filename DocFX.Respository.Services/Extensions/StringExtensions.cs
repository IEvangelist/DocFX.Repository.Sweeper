using Octokit.GraphQL;

namespace DocFX.Repository.Services.Extensions
{
    public static class StringExtensions
    {
        public static ID ToGitHubId(this string value) => new ID(value);
    }
}