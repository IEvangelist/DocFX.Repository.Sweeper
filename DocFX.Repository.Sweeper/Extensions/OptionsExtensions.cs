using DocFX.Repository.Extensions;
using DocFX.Repository.Sweeper.OpenPublishing;
using System.Threading.Tasks;

namespace DocFX.Repository.Sweeper
{
    static class OptionsExtensions
    {
        static Task<DocFxConfig> _configTask;
        static Task<RedirectConfig> _redirectConfigTask;

        internal static Task<DocFxConfig> GetConfigAsync(this Options options) 
            => _configTask ?? (_configTask = options.SourceDirectory.FindJsonFileAsync<DocFxConfig>(DocFxConfig.FileName));

        internal static Task<RedirectConfig> GetRedirectConfigAsync(this Options options)
            => _redirectConfigTask ?? (_redirectConfigTask = options.SourceDirectory.FindJsonFileAsync<RedirectConfig>(RedirectConfig.FileName));
    }
}