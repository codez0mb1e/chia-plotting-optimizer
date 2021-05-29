param(
  [int]$plotsPerQueue = 1,
  [int]$buffer = 5000,
  [int]$threads,
  [string]$tempDir,
  [string]$finalDir,
  [string]$logDir,
  [string]$chiaVersion,
  [string]$farmerKey
)


function Get-PlotterLogPath { 
  $date = Get-date -format yyyy-MM-ddThh-mm-ss
  $path = Join-Path $logDir $date
  $path += "_T$($threads)B$($buffer/1000).log"

  $path
}

Set-Alias -Name chia -Value \home\dictator\chia-blockchain\venv\bin\chia
Write-Host "Chia version:" (chia version)

chia plots create -n $plotsPerQueue -b $buffer -r $threads -t $tempDir -d $finalDir -f $farmerKey | Tee-Object -FilePath (Get-PlotterLogPath)
