using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlottingOptimizer.Core;
using PlottingOptimizer.Core.Configurations;
using static PlottingOptimizer.RandomNumberGenerator;


namespace PlottingOptimizer.Host
{
    class Program
    {
        private const string Environment =
#if DEBUG
                "dev"
#else
                "u1804"
#endif
            ;

        private static readonly Lazy<PlottingSettings> PlottingSettings = new(() =>
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile($"appsettings.{Environment}.json").Build();

            IConfigurationSection section = config.GetSection(nameof(PlottingSettings));
            return section.Get<PlottingSettings>();
        });

        private static PlottingSettings Config => PlottingSettings.Value;


        static async Task Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            IPlottingOptimizerStrategy optimizerStrategy = new PlottingOptimizerStrategy(Config.ComputeResources);
            
            DirectoryInfo di = new DirectoryInfo(Config.PlottingDirectories.LogDir);
            CancellationToken cancellationToken = source.Token;

            while (!cancellationToken.IsCancellationRequested)
            {
                //
                IEnumerable<FileInfo> files = di.GetFiles("*.log");

                IDictionary<string, int> phasesStats = files
                    .Where(f => f.LastWriteTimeUtc > DateTime.UtcNow.Subtract(TimeSpan.FromDays(5)))
                    .Select(f => new { f.Name, Count = GetPhasesNumberAsync(f.FullName, cancellationToken).Result })
                    .ToDictionary(s => s.Name, s => s.Count);

                //
                IEnumerable<Task> tasks = Enumerable
                    .Range(0, optimizerStrategy.CalculatePhases1OptimalCount(phasesStats))
                    .Select(i => RunPlottingScriptAsync(disks: GetDisks(), cancellationToken));

                Task.WhenAll(tasks);

                //await Task
                //    .WhenAll(tasks)
                //    .ConfigureAwait(false);

                await Task.Delay(Config.PullingPeriod, cancellationToken);
            }
        }



        private static (string TempPath, string FinalPath) GetDisks()
        {
            return (
                TempPath: Config.PlottingDirectories.TempPathList[RandomNumber(0, Config.PlottingDirectories.TempPathList.Count)], 
                FinalPath: Config.PlottingDirectories.FinalPathList[RandomNumber(0, Config.PlottingDirectories.FinalPathList.Count)]
                );
        }


        private static async Task<int> GetPhasesNumberAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            const string phaseLabelPattern = @"(Starting phase \d{1}\/\d{1})|(Renamed final file from)";
            const RegexOptions regexOptions = RegexOptions.Multiline | RegexOptions.IgnoreCase;

            for (int i = 0; i < Config.PlottingLogReadingAttemptsN; ++i) 
                try
                {
                    await using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    
                    string content = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return Regex.Matches(content, phaseLabelPattern, regexOptions).Count;
                }
                catch (IOException)
                {
                    Console.WriteLine($"[ERROR] Attempt number {i + 1}");
                    Thread.Sleep(Config.PlottingLogReadingDelay);
                }


            return 1;
        }


        private static async Task RunPlottingScriptAsync((string TempPath, string FinalPath) disks, CancellationToken cancellationToken = default)
        {
            if (disks.TempPath == null) throw new ArgumentException(nameof(disks));
            if (disks.FinalPath == null) throw new ArgumentException(nameof(disks));
            if (disks.TempPath == disks.FinalPath) throw new ArgumentException();

            await Task.Delay(TimeSpan.FromSeconds(RandomNumber(0, 10)), cancellationToken); // avoid throttling

            DateTime currentTime = DateTime.UtcNow;
            Console.WriteLine($"{currentTime:G} > Starting plotting for {disks.TempPath} to {disks.FinalPath}...");

            using (var ps = PowerShell.Create())
            {
                string script = await File.ReadAllTextAsync(Config.PlottingScriptPath, cancellationToken).ConfigureAwait(false);

                script = script
                    .Replace("[string]$tempDir", $"[string]$tempDir = '{disks.TempPath}'")
                    .Replace("[string]$finalDir", $"[string]$finalDir = '{disks.FinalPath}'")
                    .Replace("[string]$logDir", $"[string]$logDir = '{Config.PlottingDirectories.LogDir}'")
                    .Replace("[string]$chiaVersion", $"[string]$chiaVersion = '{Config.ChiaGuiVersion}'")
                    .Replace("[int]$threads", $"[int]$threads = {Config.ComputeResources.Phase1ProcessorCount}");

                ps.AddScript(script);

                var r = await ps.InvokeAsync().ConfigureAwait(false);
                foreach (PSObject o in r.ReadAll())
                    Console.WriteLine(o);

                Console.WriteLine($"{currentTime:G} > End plotting");
            }
        }
    }
}
