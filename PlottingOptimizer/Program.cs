using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PlottingOptimizer
{
    class Program
    {
        private const string PlotterLogsDir = @"C:\Users\dictator\.chia\mainnet\plotter";
        private const string Pattern = @"(Starting phase \d{1}\/\d{1})|(Renamed final file from)";
        private static readonly TimeSpan PullingPeriod = TimeSpan.FromMinutes(1);
        private const int MaxThreadsNumber = 16;

        private static readonly string[] tempDisks = new[] { "X:/", "Y:/" };
        private static readonly string[] finalDisks = new[] { "O:/", "Q:/" };


        static async Task Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            DirectoryInfo di = new DirectoryInfo(PlotterLogsDir);

            while (!token.IsCancellationRequested)
            {
                IEnumerable<FileInfo> files = di.GetFiles("*.log");

                IDictionary<string, int> phasesStats = files
                    .Where(f => f.LastWriteTimeUtc > DateTime.UtcNow.Subtract(TimeSpan.FromDays(5)))
                    .Select(f => new { Name = f.Name, Count = GetPhasesNumber(f.FullName) }).ToDictionary(s => s.Name, s => s.Count);

                int availableToStart = GetAvailableToStartPlotsCount(phasesStats);

                var plotTasks = new List<Task>();

                void StartPlottingProcess()
                {
                    var disks = GetDisks();
                    plotTasks.Add(RunPowerShellAsync(disks.tempDisk, disks.finalDisk));
                }

                for (int i = 0; i < availableToStart; i++)
                    Task.Run(StartPlottingProcess).ConfigureAwait(false);

                await Task.Delay(PullingPeriod, token);
            }
        }


        

        private static int GetAvailableToStartPlotsCount(IDictionary<string, int> phasesStats)
        {
            int n_phase1 = phasesStats.Count(s => s.Value < 2);
            int n_phase2 = phasesStats.Count(s => s.Value == 2);
            int n_phase3 = phasesStats.Count(s => s.Value == 3);
            int n_phase4 = phasesStats.Count(s => s.Value == 4);
            int n_completed = phasesStats.Count(s => s.Value == 5);

            int availableThreadsN = MaxThreadsNumber - (2 * n_phase1 + n_phase2 + n_phase3 + n_phase4) - 2; // 
            int availableToStart = (int)Math.Ceiling((decimal)(availableThreadsN / 2));

            if (n_phase1 >= 5) availableToStart = 0;
            if (availableToStart > 5) availableToStart = 5;

            Console.WriteLine($"{DateTime.UtcNow:G} > Active phase 1 plotters: {n_phase1}");
            Console.WriteLine($"{DateTime.UtcNow:G} > Active phase 2 plotters: {n_phase2}");
            Console.WriteLine($"{DateTime.UtcNow:G} > Active phase 3 plotters: {n_phase3}");
            Console.WriteLine($"{DateTime.UtcNow:G} > Active phase 4 plotters: {n_phase4}");

            Console.WriteLine($"{DateTime.UtcNow:G} > Available to start: {availableToStart}");

            return availableToStart;
        }

        private static (string tempDisk, string finalDisk) GetDisks()
        {
            return (
                tempDisk: tempDisks[RandomNumber(0, 2)], 
                finalDisk: finalDisks[RandomNumber(0, 2)]
                );
        }

        private static readonly Random _random = new Random();

        private static int RandomNumber(int min, int max) => _random.Next(min, max);


        private static int GetPhasesNumber(string filePath)
        {
            const int phaseNumberByNegativeScenario = 1;
            const int NumberOfRetries = 10;
            const int DelayOnRetry = 100;
            
            for (int i = 0; i < NumberOfRetries; ++i) {
                try
                {
                    using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    string content = reader.ReadToEnd();

                    MatchCollection matches =
                        Regex.Matches(content, Pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    return matches.Count;
                }
                catch (IOException)
                {
                    Console.WriteLine($"Attempt number {i + 1}");
                    Thread.Sleep(DelayOnRetry);
                }
            }

            return phaseNumberByNegativeScenario;
        }


        private static async Task RunPowerShellAsync(string tempDir, string finalDir)
        {
            await Task.Delay(TimeSpan.FromSeconds(RandomNumber(0, 10))); // avoid throttling

            DateTime currentTime = DateTime.UtcNow;
            Console.WriteLine($"{currentTime:G} > Starting plotting for {tempDir} to {finalDir}...");

            using (PowerShell ps = PowerShell.Create())
            {
                string script = File.ReadAllText("run_plotting.ps1")
                    .Replace("[string]$tempDir", $"[string]$tempDir = '{tempDir}'")
                    .Replace("[string]$finalDir", $"[string]$finalDir = '{finalDir}'");

                ps.AddScript(script);

                var results = await ps.InvokeAsync().ConfigureAwait(false);

                foreach (PSObject result in results)
                    Console.WriteLine($"{result}");

                Console.WriteLine($"{currentTime:G} > End plotting");
            }
        }
    }
}
