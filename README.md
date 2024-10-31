# dotnet-publish-ssh

Simple publish your .Net Core application to linux server via SSH or WinRM.

# Usage

* Run `BuildAndInstall_ALL.ps1`
* Run `publish-ssh` or `publish-ps` in project folder with options:
```
Usage: publish-ssh [arguments] [options]
Arguments and options are the same as for `dotnet publish`
Connection specific options:
  --host *              Host address
  --port                Host port
  --user *              User name
  --password            Password
  --keyfile             Private OpenSSH key file
  --path *              Publish path on remote server

Extra options:
  --pre           <script>  Run pre upload command
  --post          <script>  Run post upload command
  --diff                    Upload only new of modified files
(*) required

All other options will be passed to dotnet publish
```

# Example

## publish project and copy files
```cmd
publish-ssh --host 10.0.0.1 --user root --password secret --path /var/www/site
```

## clear old files before publish
```cmd
publish-ssh --host 10.0.0.1 --user root --password secret --path /var/www/site --pre "rm -rf /var/www/site/*"
```

# WinRM setup

## server

setup :

```powershell
winrm quickconfig 
````

test :  

```powershell
WinRM enumerate winrm/config/listener
````

## client (using server ip)

setup : 

```powershell
Set-Item WSMan:\localhost\Client\TrustedHosts -Value IP_OF_SERVER_MACHINE
````

test :  
```powershell
Test-WsMan IP_OF_SERVER_MACHINE
```

# SSH setup

## server

setup: 

```bash
sudo apt install openssh-server -y
sudo systemctl enable ssh
```
