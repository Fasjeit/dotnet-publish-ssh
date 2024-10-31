# dotnet-publish-ssh

Simple publish your .Net Core application to linux server via SSH.

Based on original [project](https://github.com/albekov/dotnet-publish-ssh) by [albekov](https://github.com/albekov).

# Usage

* Run `BuildAndInstall.ps1`
* Run `publish-ssh` in project folder with options:
```
Usage: publish-ssh [arguments] [options]

Arguments and options are the same as for `dotnet publish`

SSH specific options:
  --ssh-host *    <host>    Host address
  --ssh-port      <port>    Host port
  --ssh-user *    <user>    User name
  --ssh-password  <pswd>    Password
  --ssh-keyfile   <key>     Private OpenSSH key file
  --ssh-path *    <path>    Publish path on remote server

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
publish-ssh --ssh-host 10.0.0.1 --ssh-user root --ssh-password secret --ssh-path /var/www/site
```

## clear old files before publish
```cmd
publish-ssh --ssh-host 10.0.0.1 --ssh-user root --ssh-password secret --ssh-path /var/www/site --pre "rm -rf /var/www/site/*"
```
