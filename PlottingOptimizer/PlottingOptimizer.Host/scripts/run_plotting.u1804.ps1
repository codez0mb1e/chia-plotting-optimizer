param(
  [int]$plotsPerQueue = 1,
  [int]$buffer = 5000,
  [int]$threads,
  [string]$tempDir,
  [string]$finalDir,
  [string]$logDir,
  [string]$chiaVersion
)


function Get-PlotterLogPath { 
  $date = Get-date -format yyyy-MM-ddThh-mm-ss
  $path = Join-Path $logDir $date
  $path += "_T$($threads)B$($buffer/1000).log"

  $path
}

Set-Alias -Name chia -Value \home\dictator\chia-blockchain\venv\bin\chia
Write-Host "Chia version:" (chia version)

chia plots create -k 32 -n 1 -b 5000 -r 2 -t /plotdrive1 -d /harvestdrive1 2>&1 | tee ~/chia-blockchain/logs/2021-05-24_q1t2_1.log
# chia plots create -k 32 -n 1 -b 5000 -r 2 -t /plotdrive1 -d /harvestdrive1 -f <public_framer_key> 2>&1 | tee ~/chia-blockchain/logs/q1t2_1.log
# chia plots create -n $plotsPerQueue -b $buffer -r $threads -t $tempDir -d $finalDir | Tee-Object -FilePath (Get-PlotterLogPath)
