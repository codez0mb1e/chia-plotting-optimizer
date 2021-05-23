#!/bin/bash


# ----
#Lxxx
# https://azure.microsoft.com/en-us/pricing/details/managed-disks/

# ----

sudo apt update && apt upgrade -y

sudo adduser dp
sudo usermod -aG sudo dp


# ---- 
sudo apt -y install xfce4 # or ubuntu-desktop
sudo reboot


# [4] ----

lsblk

sudo parted /dev/nvme0n1
sudo parted /dev/nvme1n1 --script mklabel gpt mkpart xfspart xfs 0% 100%

sudo mkfs.xfs /dev/nvme0n1p1
sudo mkfs.xfs /dev/nvme1n1p1

sudo partprobe /dev/nvme0n1p1
sudo partprobe /dev/nvme1n1p1

sudo mkdir /datadrive0
sudo mkdir /datadrive1

sudo mount /dev/nvme0n1p1 /datadrive0
sudo mount /dev/nvme1n1p1 /datadrive1

# Set auto mount after reboot [4]


# Remote access [2] ----

sudo apt-get -y install xrdp
sudo systemctl enable xrdp

echo xfce4-session >~/.xsession

sudo service xrdp restart

az vm open-port --resource-group blchn-rg --name xch-u2004-vm --port 3389


# ! Connect via remote desktop

sudo apt-get install python3.7
sudo apt-get -y install python3-venv python3-distutils python3-dev
sudo apt-get -y install libcanberra-gtk-module libgconf-2-4 npm



# [1] ----

mkdir chia; cd chia

# Checkout the source and install
git clone https://github.com/Chia-Network/chia-blockchain.git -b latest --recurse-submodules
cd chia-blockchain

sh install.sh

. ./activate

# The GUI requires you have Ubuntu Desktop or a similar windowing system installed.
# You can not install and run the GUI as root

chmod +x ./install-gui.sh
./install-gui.sh

cd chia-blockchain-gui
npm run electron &


# Tools [6, 7] ----

# Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned
Import-Module PSChiaPlotter
Get-ChiaPlottingStatistic | sort Time_started -Descending | select -first 20


sudo apt install -y wget apt-transport-https software-properties-common
# Download and register  the Microsoft repository GPG keys
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb


sudo apt update
sudo add-apt-repository universe

sudo apt install -y powershell

pwsh # Start PowerShell



# Chia CLI [8] -----

Set-Alias -Name chia -Value $ENV:LOCALAPPDATA\chia-blockchain\app-1.1.4\resources\app.asar.unpacked\daemon\chia.exe

# Init
chia init

# Checks
chia plots check -n 30
chia show -s 


# References
#
# 0. https://wilhard.ru/bitcoin/chia-install-guide-ubuntu-linux/

# 1. https://github.com/Chia-Network/chia-blockchain/wiki/INSTALL#ubuntudebian
# 2. https://docs.microsoft.com/en-us/azure/virtual-machines/linux/use-remote-desktop
# 3. https://docs.microsoft.com/en-us/azure/virtual-machines/lsv2-series
# 4. https://docs.microsoft.com/en-us/azure/virtual-machines/linux/attach-disk-portal
# 5. https://download.chia.net/?prefix=install/
# 6. https://github.com/MrPig91/PSChiaPlotter
# 7. https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux?view=powershell-7.1
# 8. https://github.com/Chia-Network/chia-blockchain/wiki/CLI-Commands-Reference
# 9. https://www.chia.net/
# 10. https://github.com/Chia-Network/chia-blockchain/