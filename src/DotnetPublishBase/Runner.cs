using DotnetPublishBase;
using System.Diagnostics;
using System.Linq;

namespace DotnetPublishSsh
{
    public sealed class Runner
    {
        public static void Run(
            string[] args, 
            Func<PublishOptions, IUploader> uploaderConstructor)
        {
            //Process currentProcess = Process.GetCurrentProcess();
            //Console.WriteLine($"Current ProcessId is [{currentProcess.Id}]");
            //Console.WriteLine($"Waiting for debugger to attach...");
            //while (!Debugger.IsAttached)
            //{
            //    System.Threading.Thread.Sleep(100);
            //}
            //Console.WriteLine($"Debugger attached");

            var options = PublishOptions.ParseArgs(args);
            if (options.PrintHelp)
            {
                PrintHelp();
                return;
            }

            PrepareOptions(options);

            var arguments = string.Join(" ", options.Args);

            ///////////////////////
            // TEST ONLY - comment to skipp dotnet publish
            if (!PublishLocal(arguments))
            {
                return;
            }

            // TEST ONLY - uncomment to create test files to publish
            //File.WriteAllText(Path.Combine(options.LocalPath, "test"), "12");
            //Directory.CreateDirectory(Path.Combine(options.LocalPath, "folder"));
            //File.WriteAllText(Path.Combine(options.LocalPath, "folder", "test_f"), "2f2");
            ////////////////
            ///

            var path = options.Path;
            var localPath = options.LocalPath;

            try
            {
                using (var uploader = uploaderConstructor(options))
                {
                    if (!path.EndsWith(uploader.DirectorySeparator))
                    {
                        path += uploader.DirectorySeparator;
                    }

                    localPath = Path.GetFullPath(localPath) + Path.DirectorySeparatorChar;

                    var localFiles = GetLocalFiles(localPath);

                    Console.WriteLine();
                    Console.WriteLine($"Uploading {localFiles.Count} files to {options.User}@{options.Host}:{options.Port} Path: [{options.Path}]");

                    if (options.PreUploadCommand != null)
                    {
                        uploader.Run(options.PreUploadCommand);
                    }
                    uploader.UploadFiles(path, localFiles);

                    if (options.PostUploadCommand != null)
                    {
                        uploader.Run(options.PostUploadCommand);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading files to server: {ex.Message}");
            }

            Directory.Delete(localPath, true);
        }

        private static void PrepareOptions(PublishOptions options)
        {
            if (string.IsNullOrEmpty(options.LocalPath))
            {
                var tempPath = Path.Combine(Path.GetTempPath(), $"publish.{Guid.NewGuid()}");
                Directory.CreateDirectory(tempPath);
                options.LocalPath = tempPath;
            }

            options.Args = options.Args.Concat(new[] {"-o", options.LocalPath}).ToArray();
        }

        private static bool PublishLocal(string arguments)
        {
            Console.WriteLine($"Starting `dotnet {arguments}`");

            var info = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "publish " + arguments
            };

            var process = Process.Start(info);
            if (process == null)
            {
                Console.WriteLine($"Cannot start dotnet publish process");
                return false;
            }
            process.WaitForExit();
            var exitCode = process.ExitCode;

            Console.WriteLine($"dotnet publish exited with code {exitCode}");

            return exitCode == 0;
        }

        private static List<LocalFile> GetLocalFiles(string localPath)
        {
            var localFiles = Directory
                .EnumerateFiles(localPath, "*.*", SearchOption.AllDirectories)
                .Select(f => new LocalFile(localPath, f))
                .ToList();
            return localFiles;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Publish to remote server via SSH");
            Console.WriteLine();
            Console.WriteLine("Usage: publish-ssh [arguments] [options]");
            Console.WriteLine();
            Console.WriteLine("Arguments and options are the same as for `dotnet publish`");
            Console.WriteLine();
            Console.WriteLine("SSH specific options:");
            Console.WriteLine("  --ssh-host *    <host>    Host address");
            Console.WriteLine("  --ssh-port      <port>    Host port");
            Console.WriteLine("  --ssh-user *    <user>    User name");
            Console.WriteLine("  --ssh-password  <pswd>    Password");
            Console.WriteLine("  --ssh-keyfile   <key>     Private OpenSSH key file");
            Console.WriteLine("  --ssh-path *    <path>    Publish path on remote server");
            Console.WriteLine();
            Console.WriteLine("Extra options:");
            Console.WriteLine("  --pre           <script>  Run pre upload command");
            Console.WriteLine("  --post          <script>  Run post upload command");
            Console.WriteLine("  --diff                    Upload only new of modified files");
            Console.WriteLine("(*) required");
            Console.WriteLine();
            Console.WriteLine("All other options will be passed to dotnet publish");
            Console.WriteLine();
        }
    }
}