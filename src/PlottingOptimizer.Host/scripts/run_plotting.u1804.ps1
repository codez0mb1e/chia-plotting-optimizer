param(
  [int]$plotsPerQueue = 1,
  [int]$buffer = 4292, # 4 Gib
  [int]$threads,
  [string]$tempDir,
  [string]$finalDir,
  [string]$logPath,
  [string]$chiaVersion,
  [string]$farmerKey
)

Set-Alias -Name chia -Value \home\dictator\chia-blockchain\venv\bin\chia
Write-Host "Chia version:" (chia version)

chia plots create -n $plotsPerQueue -b $buffer -r $threads -t $tempDir -d $finalDir -f $farmerKey | Tee-Object -FilePath $logPath
