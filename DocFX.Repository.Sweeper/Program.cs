using CommandLine;
using DocFX.Repository.Sweeper.Core;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using static CommandLine.Parser;

namespace DocFX.Repository.Sweeper
{
    class Program
    {
        static readonly Parser CLI = Default;
        static readonly RepoSweeper RepoSweeper = new RepoSweeper();

        static async Task Main(string[] args)
        {
            // https://github.com/mayuki/Kurukuru#aware-non-unicode-codepage-on-windows-environment
            Console.OutputEncoding = Encoding.UTF8;

            var parsedArgs = CLI.ParseArguments<Options>(args);
            if (parsedArgs.Tag == ParserResultType.Parsed)
            {
                await parsedArgs.MapResult(async options =>
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var summary = await RepoSweeper.SweepAsync(options, stopwatch);

                    stopwatch.Stop();

                    ConsoleColor.Green.WriteLine(summary.ToString());
                    ConsoleColor.Green.WriteLine($"Elapsed time: {stopwatch.Elapsed.ToHumanReadableString()}");
                },
                errors => 
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error);
                    }

                    return Task.CompletedTask;
                });
            }

            FileTokenCacheUtility.PurgeCache();
        }
    }
}