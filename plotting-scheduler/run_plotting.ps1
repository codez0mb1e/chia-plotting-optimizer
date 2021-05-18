﻿param(
  [int]$plotsPerQueue = 1,
  [int]$buffer = 5000,
  [int]$threads = 2,
  [string]$tempDir,
  [string]$finalDir,
  [string]$logDir = "~/.chia/mainnet/plotter/",
  [string]$chiaVersion = "1.1.4"
)

Set-Alias -Name chia -Value $ENV:LOCALAPPDATA\chia-blockchain\app-$chiaVersion\resources\app.asar.unpacked\daemon\chia.exe
Write-Host "Chia version:" (chia version)


function Get-PlotterLogPath { 
  $date = Get-date -format yyyy-MM-ddThh-mm-ss
  $path = Join-Path $logDir $date
  $path += "_$($tempDir.Substring(0,1))2$($finalDir.Substring(0,1))_T$($threads)B$($buffer/1000).log"

  $path
}

$logPath = Get-PlotterLogPath

chia plots create -n $plotsPerQueue -b $buffer -r $threads -t $tempDir -d $finalDir | Tee-Object -FilePath $logpath

#New-Item -Path $logPath -ItemType "file" -Value "Starting phase 1/1"
#Start-Sleep -Seconds 30

Write-Host "Completed."