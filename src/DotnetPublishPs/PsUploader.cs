using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using DotnetPublishBase;
using Microsoft.PowerShell.Commands;

namespace DotnetPublishSsh
{
    internal class PsUploader : IUploader
    {

        // короче, нужно по факту НЕ быть в удалённом ранспейсе, чтоб туда скопировать файлы.
        // может вообще не заморачиваться с WSManConnectionInfo, а просто вызвать руками создание сессии и использование
        // её в переменной?



        private const string ChecksumFileName = "checksum.hash";

        public char DirectorySeparator { get; set; } = '/';

        // наверное и не нужен, делать через локальную сессию + указание
        //private readonly Runspace remoteRunspace;

        private readonly PSObject remoteSession;

        private readonly Runspace localRunspace;

        private readonly HashSet<string> existingDirectories = new HashSet<string>();

        private bool disposedValue;

        private bool useDiff;

        public PsUploader(PublishOptions publishSshOptions)
        {
            this.localRunspace = RunspaceFactory.CreateRunspace();
            this.localRunspace.Open();
            this.useDiff = publishSshOptions.Diff;
            this.remoteSession = this.CreateRemoteSession(publishSshOptions);
        }

        private PSObject CreateRemoteSession(PublishOptions options)
        {
            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.Runspace = this.localRunspace;

                var sc = new SecureString();
                if (options.Password != null)
                {
                    foreach (var c in options.Password)
                    {
                        sc.AppendChar(c);
                    }
                }
                var creds = new PSCredential(options.User, sc);

                powershell.AddCommand("New-PSSession");
                powershell.AddParameter("ComputerName", options.Host);
                powershell.AddParameter("Credential", creds);
                var results = powershell.Invoke();
                var error = powershell.Streams.Error;

                if (error.Count > 0)
                {
                    throw new Exception(string.Concat(error.Select(e => e.ToString())));
                }

                if (results.Count < 1)
                {
                    throw new Exception("Unexpected ps output");
                }

                return results[0];
            }
        }

        public void UploadFiles(string path, ICollection<LocalFile> localFiles)
        {
            // Nomalize to Unix-style separators
            path = path.Replace(Path.DirectorySeparatorChar, this.DirectorySeparator);
            if (this.useDiff)
            {
                Console.WriteLine("Computing remote checksum...");
                this.CreateChecksumFile(path);
                Console.WriteLine("Remote checksum computing done!");

                Console.WriteLine($"Computing local checksum...");
                var localChecksum = new Checksum();
                foreach (var file in localFiles)
                {
                    localChecksum.AddFile(path, file);
                }
                Console.WriteLine("Local checksum computing done!");
                var diff = this.GetChecksumDiff(path, localChecksum);
                localFiles = localFiles.Where(lf => diff.Contains($"{path}{lf.RelativeName}")).ToList();

                Console.WriteLine($"{localFiles.Count} files changed or created");
            }

            using (PowerShell ps = PowerShell.Create())
            {
                ps.Runspace = this.localRunspace;
                foreach (var localFile in localFiles)
                {
                    this.UploadFileCommand(ps, localFile, path);
                }
                Console.WriteLine($"Begin Uploading... {localFiles.Count} files.");
                ps.Invoke();
            }
            
            Console.WriteLine($"Uploaded {localFiles.Count} files.");
        }

        public void Run(string command, bool silent = false)
        {
            this.RunCore(command, silent, false);
        }

        internal Collection<PSObject> RunCore(
            string command,
            bool silent = true,
            bool throwOnError = true)
        {
            using (var powershell = PowerShell.Create())
            {
                powershell.Runspace = this.localRunspace;
                powershell.AddCommand("Invoke-Command");
                powershell.AddParameter("ScriptBlock", ScriptBlock.Create(command));
                powershell.AddParameter("Session", this.remoteSession);

                if (!silent)
                {
                    // for getting text output instead of object
                    powershell.AddCommand("Out-String -Width 1024");
                }

                var results = powershell.Invoke();
                var errors = powershell.Streams.Error;

                if (throwOnError)
                {
                    if (errors.Count > 0)
                    {
                        throw new Exception(string.Concat(errors.Select(e => e.ToString())));
                    }
                }

                if (!silent)
                {
                    if (errors.Count > 0)
                    {
                        Console.WriteLine(string.Concat(errors.Select(e => e.ToString())));
                    }
                    Console.WriteLine($"{Environment.NewLine}Command output:{Environment.NewLine}" +
                        $"{string.Join("", results.Select(r => r.ToString()))}");
                }
                return results;
            }
        }

        public void CreateChecksumFile(string path)
        {
            // Get-ChildItem -File ./ -Recurse | Get-FileHash

            var checksumFilePath = Path.Combine(path, PsUploader.ChecksumFileName);
            var command = $"Get-ChildItem -File {path} -Recurse | " +
                $"Get-FileHash | Select -Property Hash, Path | " +
                $"Format-Table -AutoSize -HideTableHeaders | " +
                $"Out-String -Width 1024";
                //$"Out-Host {checksumFilePath} -Width 1024 -Force";
            //var command = $"Get-ChildItem -File {path} -Recurse | Get-FileHash | Select -Property Hash, Path |  Export-Csv -delimiter \" \" -path {checksumFilePath} -notype";


            var result = this.RunCore(command)[0].ToString();

            this.RunCore($"New-Item {checksumFilePath} -Force");
            this.RunCore($"Set-Content {checksumFilePath} \"{result}\"");
        }

        public List<string> GetChecksumDiff(string path, Checksum localChecksum)
        {
            try
            {
                var remoteChecksum = new Checksum();
                var checksumFilePath = $"{path}{ChecksumFileName}";

                Console.WriteLine("Getting remote checksum...");
                var dataString = this.RunCore($"Get-Content {checksumFilePath} | Out-String")[0]
                    .ToString()
                    .Replace("\\", "/");
                var splited = dataString.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in splited)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    remoteChecksum.AddEntry(line.TrimEnd(' '));
                }
                return localChecksum.Diff(remoteChecksum);
            }
            catch (Exception)
            {
                return localChecksum.Keys.ToList();
            }
        }

        private void UploadFileCommand(PowerShell ps, LocalFile localFile, string path)
        {
            //using (PowerShell ps = PowerShell.Create())
            //{
                ps.Runspace = this.localRunspace;
                Console.WriteLine($"Uploading {localFile.RelativeName}");

                // normalize to unix-style DirectorySeparator
                var filePath = localFile.RelativeName.Replace(Path.DirectorySeparatorChar, this.DirectorySeparator);

                var fullPath = path + filePath;

                this.EnsureDirExists(fullPath);

                //  -Path {localFile.FileName} -Destination {path}
                ps.AddCommand($"Copy-Item");
                ps.AddParameter("Path", localFile.FileName);
                ps.AddParameter("Destination", fullPath);
                ps.AddParameter("ToSession", this.remoteSession);

            ps.AddStatement();
            //ps.Invoke();

            var errors = ps.Streams.Error;
                if (errors.Count > 0)
                {
                    throw new Exception(string.Concat(errors.Select(e => e.ToString())));
                }
            //}
        }

        private void EnsureDirExists(string path)
        {
            var parts = path.Split(new[] { this.DirectorySeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            if (!path.EndsWith(this.DirectorySeparator.ToString()))
                parts = parts.Take(parts.Count - 1).ToList();

            this.CreateDir(parts);
        }

        private void CreateDir(ICollection<string> parts, bool noCheck = false)
        {
            if (parts.Any())
            {
                var path = this.Combine(parts);
                var parent = parts.Take(parts.Count - 1).ToList();

                if (noCheck || this.FolderExistsPs(path)) // Test-Path -Path $Folder
                {
                    this.CreateDir(parent, true);
                }
                else
                {
                    this.CreateDir(parent);
                    this.CreateDirectoryPs(path); // New-Item -ItemType Directory -Path C:\Path\To\New\Folder
                }

                this.existingDirectories.Add(path);
            }
        }

        private bool FolderExistsPs(string path)
        {
            return string.Equals(
                this.RunCore($"Test-Path -Path {path}")[0].ToString(), 
                bool.TrueString, 
                StringComparison.OrdinalIgnoreCase);
        }

        private void CreateDirectoryPs(string path)
        {
            this.RunCore($"New-Item -ItemType Directory -Path {path}");
        }

        private string Combine(ICollection<string> parts)
        {
            var path = string.Join(this.DirectorySeparator.ToString(), parts) +
                       (parts.Any() ? this.DirectorySeparator.ToString() : "");
            return path;
        }

        private void DisposeSession()
        {
            using (var powershell = PowerShell.Create())
            {
                powershell.Runspace = this.localRunspace;
                powershell.AddCommand("Remove-PSSession");
                powershell.AddParameter("Session", this.remoteSession);
                powershell.Invoke();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.DisposeSession();
                    this.localRunspace?.Close();
                    this.localRunspace?.Dispose();
                }
                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}