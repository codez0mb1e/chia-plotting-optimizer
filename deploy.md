
# Deployment Chia Farm from Zero

In this document collected the most useful scripts and libraries related to Chia cluster deployment.

## Set up VM

Use _Storage optimized_ VM instances in Microsoft Azure. For this type of VM instances is available _NVMe SSD(s)_ on demand.

Create [Lsv2-series VM](https://docs.microsoft.com/en-us/azure/virtual-machines/lsv2-series) using [Azure Portal](https://portal.azure.com/#create/Canonical.UbuntuServer1804LTS-ARM) or Azure CLI.

Connect with VM and update: `sudo apt update && sudo apt upgrade -y`

Add non-root user (optional):

```bash
sudo adduser $user_name
sudo usermod -aG sudo $user_name
```

## Mount disks for Plotting and Harvesting

Mount volumes ([instruction](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/attach-disk-portal)):

```bash
# view available disks
lsblk

# for each NVMe drive
sudo parted /dev/nvme0n1 --script mklabel gpt mkpart xfspart xfs 0% 100%
sudo mkfs.xfs /dev/nvme0n1p1
sudo partprobe /dev/nvme0n1p1

sudo mkdir /plotdrive1
sudo mount /dev/nvme0n1p1 /plotdrive1
sudo chmod -R 777 /plotdrive1


# for each HDD drive
sudo parted /dev/sdc --script mklabel gpt mkpart xfspart xfs 0% 100%
sudo mkfs.xfs /dev/sdc1
sudo partprobe /dev/sdc1

sudo mkdir /harvestdrive1
sudo mount /dev/sdc1 /harvestdrive1
sudo chmod -R 777 /harvestdrive1

# check
df -h
```

## Install Chia Software

Install Chia Software and activate it ([instruction](https://github.com/Chia-Network/chia-blockchain/wiki/INSTALL#ubuntudebian)):

```bash
# install python 3.7 (if necessary)

sudo apt -y install python3.7
sudo apt install python3.7-venv python3.7-distutils python3.7-dev git lsb-release -y


# Checkout the source and install
git clone https://github.com/Chia-Network/chia-blockchain.git -b latest --recurse-submodules
cd chia-blockchain

sh install.sh

. ./activate
```

## Run Chia Software

Init Chia using [Chia CLI](https://github.com/Chia-Network/chia-blockchain/wiki/CLI-Commands-Reference):

```bash
chia version

chia keys add
chia keys show

chia init

chia start --help # check mode which you wish start Chia
chia start farmer-only
```

[Start plotting](https://github.com/Chia-Network/chia-blockchain/wiki/CLI-Commands-Reference#create):

```bash
chia keys show # see your farmer key

chia plots create -k 32 -n 1 -b 5000 -r 2 -t /plotdrive1 -d /harvestdrive1 2>&1 | tee ~/chia-blockchain/logs/my_1st_plot.log
```

## Tools

[Install **.NET 5** and **Powershell**](https://github.com/codez0mb1e/cloud-rstudio-server/blob/master/scripts/install_dotnet_tools.sh).

Git settings:

```bash
# 1.
# view ssh-key 
cat ~/.ssh/id_rsa.pub
# or create if it isn't exist
cd ~/.ssh && ssh-keygen

# 2. Register SSH keys in github
cat id_rsa.pub
# and register to https://github.com/settings/keys

# 3.
git config --global user.name $user_name # @codez0mb1e
git config --global user.email $user_email
```

## Monitoring

[Monitoring NVMe](https://github.com/linux-nvme/nvme-cli):

```bash
sudo apt -y install nvme-cli
sudo nvme list

sudo nvme smart-log /dev/nvme0n1 | grep percentage_used
```

Monitoring of Plotting via [PSChiaPlotter](https://github.com/MrPig91/PSChiaPlotter) (WARN: only for Windows):

```powershell
Install-Module -Repository PSGallery -Name PSChiaPlotter
Get-ChiaPlottingStatistic | sort Time_started -Descending | select -first 20
```

Monitoring of Plotting via [Chia Plot Graph](https://github.com/stolk/chiaplotgraph):

```bash
sudo apt install -y  build-essential

# clone and build tool
mkdir tools; cd tools
git clone https://github.com/stolk/chiaplotgraph.git
cd chiaplotgraph
make 
```

```powershell
# analyze logs
$top_n = 20
Set-Location ./tools/chiaplotgraph/

$logFiles = Get-ChildItem -Path ~/chia-blockchain/logs/ | Sort-Object -Property LastWriteTime -Descending | Select -expa FullName -first $top_n  
./chiaplotgraph $logFiles  
```

Monitoring of Harvesting via [Chia Harvest Graph](https://github.com/stolk/chiaharvestgraph):

```bash
# install
git clone https://github.com/stolk/chiaharvestgraph.git
cd chiaplotgraph
make 

# run
./chiaharvestgraph ~/chia-blockchain/farmer-logs
```

## Attempts to Boost (beta)

New [multi-thread Chia Plotter](https://github.com/madMAx43v3r/chia-plotter) (WARN: under active development).


## After VMs Reboot...

Script that I run after VM(s) reboot:

```bash
## Get updates
sudo apt update && sudo  apt upgrade -y


## Mount disks
sudo mount /dev/sdc1 /harvestdrive1

sudo parted /dev/nvme0n1 --script mklabel gpt mkpart xfspart xfs 0% 100%
sudo mkfs.xfs /dev/nvme0n1p1
sudo partprobe /dev/nvme0n1p1
sudo mount /dev/nvme0n1p1 /plotdrive1
sudo chmod -R 777 /plotdrive1
lsblk


## Start Chia farmer daemons
cd chia-blockchain
. ./activate
cd ~

chia version
chia start farmer-only
```

### Chia GUI (obsolete)

The GUI requires you have Ubuntu Desktop or a similar windowing system installed.
WARN: _You can not install and run the GUI as root._

```bash
# Install GUI ---- 
sudo apt -y install xfce4 # or ubuntu-desktop
sudo reboot

# Remote access [2] ----

sudo apt-get -y install xrdp
sudo systemctl enable xrdp

echo xfce4-session >~/.xsession

sudo service xrdp restart

az vm open-port --resource-group $resource_group_name --name $vm_name --port 3389

# Install Chia GUI
chmod +x ./install-gui.sh
./install-gui.sh

cd chia-blockchain-gui
npm run electron &
```
