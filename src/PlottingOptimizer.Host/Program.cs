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


        static async Task Main(string[] args)
        {
            // read settings
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile($"appsettings.{Environment}.json").Build();

            IConfigurationSection section = config.GetSection(nameof(PlottingSettings));
            PlottingSettings plotSettings = section.Get<PlottingSettings>();

            
            // get plotting optimizer
            IPlottingOptimizerStrategy optimizer = new PlottingOptimizerStrategy(plotSettings.ComputeResources);
            

            // get cancellation token
            CancellationTokenSource source = new();
            CancellationToken cancellationToken = source.Token;


            // run
            while (!cancellationToken.IsCancellationRequested)
            {
                // get current phase stats
                IEnumerable<FileInfo> files = GetLogsFiles(plotSettings);

                IDictionary<string, int> phasesStats = files
                    .Where(f => f.LastWriteTimeUtc > DateTime.UtcNow.Subtract(TimeSpan.FromDays(5)))
                    .Select(f => new { f.Name, Count = GetPhasesNumberAsync(f.FullName, plotSettings, cancellationToken).Result })
                    .ToDictionary(s => s.Name, s => s.Count);


                // run new plot tasks
                IEnumerable<Task> tasks = Enumerable
                    .Range(0, optimizer.CalculatePhases1OptimalCount(phasesStats))
                    .Select(i => RunPlottingScriptAsync(disks: GetDisks(plotSettings), plotSettings, cancellationToken));

                await Task.WhenAll(tasks);


                // wait next pulling
                await Task.Delay(plotSettings.PullingPeriod, cancellationToken);
            }
        }


        #region Log parser

        private static IEnumerable<FileInfo> GetLogsFiles(PlottingSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            DirectoryInfo di = new(settings.PlottingDirectories.LogDir);
            return di.GetFiles("*.log");
        }

        private static async Task<int> GetPhasesNumberAsync(string filePath, PlottingSettings settings, CancellationToken cancellationToken = default)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            const string phaseLabelPattern = @"(Starting phase \d{1}\/\d{1})|(Renamed final file from)";
            const RegexOptions regexOptions = RegexOptions.Multiline | RegexOptions.IgnoreCase;

            for (int i = 0; i < settings.PlottingLogReadingAttemptsN; ++i)
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
                    Thread.Sleep(settings.PlottingLogReadingDelay);
                }


            return 1;
        }

        #endregion


        #region Plotting script

        private static (string TempPath, string FinalPath) GetDisks(PlottingSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            return (
                TempPath: settings.PlottingDirectories.TempPathList[RandomNumber(0, settings.PlottingDirectories.TempPathList.Count)], 
                FinalPath: settings.PlottingDirectories.FinalPathList[RandomNumber(0, settings.PlottingDirectories.FinalPathList.Count)]
                );
        }


        private static async Task RunPlottingScriptAsync((string TempPath, string FinalPath) disks, PlottingSettings settings, CancellationToken cancellationToken = default)
        {
            if (disks.TempPath == null) throw new ArgumentException(nameof(disks));
            if (disks.FinalPath == null) throw new ArgumentException(nameof(disks));
            if (disks.TempPath == disks.FinalPath) throw new ArgumentException();
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            Guid processId = Guid.NewGuid();

            // Avoid throttling while start new plots
            #if !DEBUG
            await Task.Delay(
                TimeSpan.FromSeconds(RandomNumber(1, (int)settings.PullingPeriod.TotalSeconds/2)), 
                cancellationToken);
            #endif

            // Run plotting script
            Console.WriteLine($"{DateTime.UtcNow:G} > Starting plotting {processId:D} for {disks.TempPath} to {disks.FinalPath}...");

            using (var ps = PowerShell.Create())
            {
                string script = await File.ReadAllTextAsync(settings.PlottingScriptPath, cancellationToken).ConfigureAwait(false);
                string logPath = Path.Join(settings.PlottingDirectories.LogDir, $"plot-k32-{DateTime.UtcNow:yy-MM-dd-HH-mm}-{processId:N}.log"); ;

                script = script
                    .Replace("[string]$tempDir", $"[string]$tempDir = '{disks.TempPath}'")
                    .Replace("[string]$finalDir", $"[string]$finalDir = '{disks.FinalPath}'")
                    .Replace("[string]$logPath", $"[string]$logPath = '{logPath}'")
                    .Replace("[string]$chiaVersion", $"[string]$chiaVersion = '{settings.ChiaGuiVersion}'")
                    .Replace("[int]$threads", $"[int]$threads = {settings.ComputeResources.Phase1ProcessorCount}")
                    .Replace("[string]$farmerKey", $"[string]$farmerKey = '{settings.FarmerKey}'");

                ps.AddScript(script);

                var r = await ps.InvokeAsync().ConfigureAwait(false);

                foreach (PSObject o in r.ReadAll())
                    Console.WriteLine(o);
            }

            Console.WriteLine($"{DateTime.UtcNow:G} > Complete plotting {processId:D}");
        }
        #endregion

    }
}
