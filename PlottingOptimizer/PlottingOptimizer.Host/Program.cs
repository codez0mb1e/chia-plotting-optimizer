using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static PlottingOptimizer.RandomNumberGenerator;


namespace PlottingOptimizer
{
    class Program
    {
        private static readonly Configuration Config = new ();


        static async Task Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            IPlottingOptimizerStrategy optimizerStrategy = new PlottingOptimizerStrategy(Config);
            DirectoryInfo di = new DirectoryInfo(Config.PlotterLogsDir);


            CancellationToken token = source.Token;

            while (!token.IsCancellationRequested)
            {
                IEnumerable<FileInfo> files = di.GetFiles("*.log");

                IDictionary<string, int> phasesStats = files
                    .Where(f => f.LastWriteTimeUtc > DateTime.UtcNow.Subtract(TimeSpan.FromDays(5)))
                    .Select(f => new { f.Name, Count = GetPhasesNumberAsync(f.FullName).Result }).ToDictionary(s => s.Name, s => s.Count);

                int optimalProcessToStart = optimizerStrategy.CalculatePhases1OptimalCount(phasesStats);

                for (int i = 0; i < optimalProcessToStart; i++)
                    _ = Task.Run(async () => await RunPlottingScriptAsync(disks: GetDisks()));

                await Task.Delay(Config.PullingPeriod, token);
            }
        }



        private static (string TempDisk, string FinalDisk) GetDisks()
        {
            return (
                TempDisk: Config.TempDisks[RandomNumber(0, Config.TempDisks.Count)], 
                FinalDisk: Config.FinalDisks[RandomNumber(0, Config.FinalDisks.Count)]
                );
        }


        private static async Task<int> GetPhasesNumberAsync(string filePath)
        {
            const string phaseLabelPattern = @"(Starting phase \d{1}\/\d{1})|(Renamed final file from)";

            for (int i = 0; i < Config.PlottingLogReadingAttemptsN; ++i) {
                try
                {
                    await using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    
                    string content = await reader.ReadToEndAsync().ConfigureAwait(false);

                    MatchCollection matches = Regex.Matches(content, phaseLabelPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

                    return matches.Count;
                }
                catch (IOException)
                {
                    Console.WriteLine($"[ERROR] Attempt number {i + 1}");
                    Thread.Sleep(Config.PlottingLogReadingDelay);
                }
            }

            return 1;
        }


        private static async Task RunPlottingScriptAsync((string tempDir, string finalDir) disks)
        {
            if (disks.tempDir == null) throw new ArgumentException(nameof(disks));
            if (disks.finalDir == null) throw new ArgumentException(nameof(disks));
            if (disks.tempDir == disks.finalDir) throw new ArgumentException();

            await Task.Delay(TimeSpan.FromSeconds(RandomNumber(0, 10))); // avoid throttling

            DateTime currentTime = DateTime.UtcNow;
            Console.WriteLine($"{currentTime:G} > Starting plotting for {disks.tempDir} to {disks.finalDir}...");

            using (var ps = PowerShell.Create())
            {
                string script = await File.ReadAllTextAsync(Config.PlottingScriptPath).ConfigureAwait(false);

                script = script
                    .Replace("[string]$tempDir", $"[string]$tempDir = '{disks.tempDir}'")
                    .Replace("[string]$finalDir", $"[string]$finalDir = '{disks.finalDir}'")
                    .Replace("[string]$logDir", $"[string]$logDir = '{Config.PlotterLogsDir}'")
                    .Replace("[string]$chiaVersion", $"[string]$chiaVersion = '{Config.ChiaGuiVersion}'")
                    .Replace("[int]$threads", $"[int]$threads = {Config.Phase1ThreadsN}");

                ps.AddScript(script);

                await ps.InvokeAsync().ConfigureAwait(false);

                Console.WriteLine($"{currentTime:G} > End plotting");
            }
        }
    }
}
