using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace DotnetPublishSsh
{
    internal class Uploader : IDisposable
    {
        private const string ChecksumFileName = "checksum.hash";

        public char DirectorySeparator { get; set; } = '/';

        private SftpClient? ftp;
        private SftpClient Ftp
        {
            get
            {
                if (this.ftp == null)
                {
                    this.ftp = new SftpClient(this.connectionInfo);
                }
                return this.ftp;
            }
        }

        private SshClient? ssh;
        private SshClient Ssh
        {
            get
            {
                if (this.ssh == null)
                {
                    this.ssh = new SshClient(this.connectionInfo);
                    this.ssh.Connect();
                }
                return this.ssh;
            }
        }

        private readonly ConnectionInfo connectionInfo;
        private readonly HashSet<string> existingDirectories = new HashSet<string>();

        private bool disposedValue;

        private bool UseDiff;

        public Uploader(PublishSshOptions publishSshOptions)
        {
            this.connectionInfo = CreateConnectionInfo(publishSshOptions);
            this.UseDiff = publishSshOptions.Diff;
        }

        private static ConnectionInfo CreateConnectionInfo(PublishSshOptions options)
        {
            var authenticationMethods = new List<AuthenticationMethod>();

            if (options.Password != null)
            {
                authenticationMethods.Add(
                    new PasswordAuthenticationMethod(options.User, options.Password));
            }

            if (options.KeyFile != null)
            {
                authenticationMethods.Add(
                    new PrivateKeyAuthenticationMethod(options.User, new PrivateKeyFile(options.KeyFile)));
            }

            var connectionInfo = new ConnectionInfo(
                options.Host,
                options.Port,
                options.User,
                authenticationMethods.ToArray());

            return connectionInfo;
        }

        public void UploadFiles(string path, ICollection<LocalFile> localFiles)
        {
            this.Ftp.Connect();

            if (this.UseDiff)
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
                localFiles = localFiles.Where(lf => diff.Contains($"{Path.Combine(path, lf.RelativeName)}")).ToList();

                Console.WriteLine($"{localFiles.Count} files changed or created");
            }

            foreach (var localFile in localFiles)
            {
                this.UploadFile(localFile, this.Ftp, path);
            }
            Console.WriteLine($"Uploaded {localFiles.Count} files.");
        }

        public void Run(string command, bool silent = false)
        {
            var sshCommand = this.Ssh.RunCommand(command);
            if (!silent)
            {
                Console.WriteLine($"{Environment.NewLine}Command output:{Environment.NewLine}{sshCommand.Result}");
            }
        }

        public void CreateChecksumFile(string path)
        {
            // find path -maxdepth 1 -type f -exec cmd params {} \; > results.out

            var checksumFilePath = Path.Combine(path, ChecksumFileName);
            var command = $"find {path} -type f -exec sha256sum {{}} \\; > {checksumFilePath}";
            this.Run(command, true);
        }

        public List<string> GetChecksumDiff(string path, Checksum localChecksum)
        {
            try
            {
                var remoteChecksum = new Checksum();
                var checksumFilePath = Path.Combine(path, "checksum.hash");

                using (var stream = new MemoryStream())
                {
                    Console.WriteLine("Getting remote checksum...");
                    this.Ftp.DownloadFile(checksumFilePath, stream);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        var line = reader.ReadLine();
                        remoteChecksum.AddEntry(line);
                        while (line != null)
                        {
                            line = reader.ReadLine();
                            remoteChecksum.AddEntry(line);
                        }
                    }
                }
                return localChecksum.Diff(remoteChecksum);
            }
            catch (SftpPathNotFoundException)
            {
                return localChecksum.Keys.ToList();
            }
        }

        private void UploadFile(LocalFile localFile, SftpClient ftp, string path)
        {
            Console.WriteLine($"Uploading {localFile.RelativeName}");
            using (var stream = File.OpenRead(localFile.FileName))
            {
                var filePath = localFile.RelativeName.Replace(Path.DirectorySeparatorChar, this.DirectorySeparator);

                var fullPath = path + filePath;

                this.EnsureDirExists(ftp, fullPath);

                ftp.UploadFile(stream, fullPath, true);
            }
        }

        private void EnsureDirExists(SftpClient ftp, string path)
        {
            var parts = path.Split(new[] { this.DirectorySeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            if (!path.EndsWith(this.DirectorySeparator.ToString()))
                parts = parts.Take(parts.Count - 1).ToList();

            this.CreateDir(ftp, parts);
        }

        private void CreateDir(SftpClient ftp, ICollection<string> parts, bool noCheck = false)
        {
            if (parts.Any())
            {
                var path = this.Combine(parts);
                var parent = parts.Take(parts.Count - 1).ToList();

                if (noCheck || ftp.Exists(path))
                {
                    this.CreateDir(ftp, parent, true);
                }
                else
                {
                    this.CreateDir(ftp, parent);
                    ftp.CreateDirectory(path);
                }

                this.existingDirectories.Add(path);
            }
        }

        private string Combine(ICollection<string> parts)
        {
            var path = this.DirectorySeparator +
                       string.Join(this.DirectorySeparator.ToString(), parts) +
                       (parts.Any() ? this.DirectorySeparator.ToString() : "");
            return path;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.ftp?.Dispose();
                    this.ssh?.Dispose();
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