param(
  [int]$plotsPerQueue = 1,
  [int]$buffer = 5000,
  [int]$threads,
  [string]$tempDir,
  [string]$finalDir,
  [string]$logPath,
  [string]$chiaVersion,
  [string]$farmerKey
)


Set-Alias -Name chia -Value $ENV:LOCALAPPDATA\chia-blockchain\app-$chiaVersion\resources\app.asar.unpacked\daemon\chia.exe
Write-Host "Chia version:" (chia version)


New-Item -Path $logPath -ItemType "file" -Value "Starting phase 1/1"
Start-Sleep -Seconds 60

Write-Host "Completed."
