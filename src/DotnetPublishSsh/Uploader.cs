using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Renci.SshNet;

namespace DotnetPublishSsh
{
    internal class Uploader : IDisposable
    {
        public char DirectorySeparator { get; set; } = '/';

        private SftpClient ftp;
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

        private SshClient ssh;
        private SshClient Ssh
        {
            get
            {
                if (this.ssh == null)
                {
                    this.ssh = new SshClient(this.connectionInfo);
                }
                return this.ssh;
            }
        }

        private readonly ConnectionInfo connectionInfo;
        private readonly HashSet<string> existingDirectories = new HashSet<string>();

        private bool disposedValue;

        public Uploader(PublishSshOptions publishSshOptions)
        {
            this.connectionInfo = CreateConnectionInfo(publishSshOptions);
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
                //client.Connect();
                this.Ftp.Connect();

                foreach (var localFile in localFiles)
                {
                    this.UploadFile(localFile, ftp, path);
                }
            Console.WriteLine($"Uploaded {localFiles.Count} files.");
        }

        public void Run(string command)
        {
            this.ssh.Connect();
            var sshCommand = this.ssh.RunCommand(command);
            Console.WriteLine($"{Environment.NewLine}Command output:{Environment.NewLine}{sshCommand.Result}");

            //Console.WriteLine($"Uploaded {localFiles.Count} files.");
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
                    this.ftp.Dispose();
                    this.ssh.Dispose();
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