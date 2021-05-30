Write-Host ".NET version:" (dotnet --version)

dotnet restore
dotnet publish -c Release -o out

Set-Location out
dotnet PlottingOptimizer.Host.dll
