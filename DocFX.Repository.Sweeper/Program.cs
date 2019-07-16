using CommandLine;
using DocFX.Repository.Sweeper.Core;
using DocFX.Repository.Sweeper.Extensions;
using System;
using System.Diagnostics;
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
            var parsedArgs = CLI.ParseArguments<Options>(args);
            if (parsedArgs.Tag == ParserResultType.Parsed)
            {
                await parsedArgs.MapResult(async options =>
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    await RepoSweeper.SweepAsync(options, stopwatch);

                    stopwatch.Stop();
                    Console.WriteLine($"Elapsed time: {stopwatch.Elapsed.ToHumanReadableString()}");
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
        }
    }
}