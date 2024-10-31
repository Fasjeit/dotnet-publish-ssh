# dotnet-publish-ssh

Simple publish your .Net Core application to linux server via SSH.

# Usage

* Run `BuildAndInstall.ps1`
* Run `publish-ssh` with options:
```
Usage: publish-ssh [arguments] [options]
Arguments and options are the same as for `dotnet publish`
SSH specific options:
  --ssh-host *              Host address
  --ssh-port                Host port
  --ssh-user *              User name
  --ssh-password            Password
  --ssh-keyfile             Private OpenSSH key file
  --ssh-path *              Publish path on remote server
  --pre                     Run pre upload command
  --post                    Run post upload command
(*) required

All other options will be passed to dotnet publish
```

# Example

## copy files
```cmd
publish-ssh --ssh-host 10.0.0.1 --ssh-user root --ssh-password secret --ssh-path /var/www/site
```

## clear old files
```cmd
publish-ssh --ssh-host 10.0.0.1 --ssh-user root --ssh-password secret --ssh-path /var/www/site --pre "rm -rf /var/www/site/*"
```
