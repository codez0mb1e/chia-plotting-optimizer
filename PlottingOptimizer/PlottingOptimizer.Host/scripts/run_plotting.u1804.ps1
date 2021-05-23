param(
  [int]$plotsPerQueue = 1,
  [int]$buffer = 5000,
  [int]$threads,
  [string]$tempDir,
  [string]$finalDir,
  [string]$logDir,
  [string]$chiaVersion
)

Set-Location ~/chia-blockchain
sh ./activate

Write-Host "Chia version:" (chia version)


function Get-PlotterLogPath { 
  $date = Get-date -format yyyy-MM-ddThh-mm-ss
  $path = Join-Path $logDir $date
  $path += "_$($tempDir.Substring(0,1))2$($finalDir.Substring(0,1))_T$($threads)B$($buffer/1000).log"

  $path
}

$logPath = Get-PlotterLogPath

# chia plots create -k 32 -n 1 -b 5000 -r 2 -t /plotdrive1 -d /harvestdrive1 -f 81c717d2cd605185e43391db4c30a1f6929e8241629f89348c52c7df2670a4768de7b18895d7b5d2fdc181d055d67edb 2>&1 | tee ~/chia-blockchain/logs/q1t2_1.log
chia plots create -n $plotsPerQueue -b $buffer -r $threads -t $tempDir -d $finalDir | Tee-Object -FilePath $logpath
