# Chia Plotting Optimizer

___The optimizer of the Plotting process in the Chia Network Blockchain.___

## Motivation

Chia plotting process has the 4 stages. The first stage is actively used by the CPU and multi-threading (_CPU bound_), and the rest are more related to working on the hard disk workloads (_IO bound_).

An imbalance towards either CPU-bound loads or the other IO-bound loads leads to an inefficient Plotting process, or even throttling CPU- or IO-subsystems.

The `Chia Plotting Optimizer` project try to avoid imbalance between CPU-bound and IO-bound and in the same time maximize number of parallel plotting processes.

## Features

- Multi-platform: Ubuntu Server 18+ and Windows Server 2019, at least,
- Flexible [configuration](#Configuration),
- Ability to work in farmer-only mode,
- Support multiple directories for temporary files and for ready plots,
- Light-weight and fast C# library.

## Installation

Clone from GitHub

```bash
git clone git@github.com:codez0mb1e/chia-plotting-optimizer.git
```

Check that you have installed .NET 5 (if you haven't then it's [easy to install](https://github.com/codez0mb1e/cloud-rstudio-server/blob/master/scripts/install_dotnet_tools.sh)).

```bash
dotnet --version
```

[How to deploy Chia Farm from Zero?](deploy.md)

## Running

Update configuration (see [Configuration section](#Configuration) below), and run `run.ps` via:

a. Bash:

```bash
cd chia-plotting-optimizer/src
./run.ps
```

b. or Powershell:

```powershell
Set-Location chia-plotting-optimizer/src
./run.ps
```

## Configuration

- `ChiaGuiVersion`: installed Chia Software version
- `farmerKey`: you farmer public key for farm-only mode
- `computeResources` section:
  - `totalProcessorCount`: total number of CPUs
  - `OsDemandProcessorCount`: number of CPU threads allocated for OS
  - `ChiaDemandProcessorCount`: number of CPU threads allocated for non-plotting Chia processes
  - `phase1ProcessorCount`: number of CPU threads allocated for CPU-bound (phase 1) phases of   plotting
  - `phase1MaxCount`: maximum number of CPU threads allocated for all CPU-bound phases.
- `plottingDirectories` section:
  - `logDir`: plotting log directory
  - `tempPathList`: list of directories for temporary files
  - `finalPathList`: list of directories for ready plots.

Example of configuration for Ubuntu Server ([source](/src/PlottingOptimizer.Host/appsettings.u1804.json)):

```json
{
  "plottingSettings": {
    "ChiaGuiVersion": "1.1.6",

    "plottingDirectories": {
      "logDir": "/home/<usr_name>/chia-blockchain/logs",
      "tempPathList": [ "/plotdrive1", "/plotdrive2", "/plotdrive3" ],
      "finalPathList": [ "/harvestdrive1", "/harvestdrive2", "/harvestdrive3" ]
    },

    "computeResources": {
      "totalProcessorCount": 16,
      "OsDemandProcessorCount": 1,
      "ChiaDemandProcessorCount": 0,
      "phase1ProcessorCount": 2,
      "phase1MaxCount": 6
    },

    "plottingScriptPath": "scripts/run_plotting.u1804.ps1",
    "farmerKey": "<your_farmer_key>"
  } 
}
```

## References

1. [Official Chia Network](https://www.chia.net/).
1. [Chia Network GitHub](https://github.com/Chia-Network/chia-blockchain/).
1. [Chia Green Paper](https://www.chia.net/assets/ChiaGreenPaper.pdf).
1. [Deployment Chia Farm from Zero](deploy.md).
