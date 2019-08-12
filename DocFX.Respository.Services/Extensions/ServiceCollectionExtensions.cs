using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DocFX.Repository.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSweeperServices(this IServiceCollection services)
        {
            var configuration =
                new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();

            services.AddLogging(logging => logging.AddFilter(level => true))
                    .AddOptions()
                    .Configure<GitHubOptions>(configuration.GetSection(nameof(GitHubOptions)))
                    .AddTransient<IGraphQLGitHubClient, GraphQLGitHubClient>();

            return services;
        }
    }
}