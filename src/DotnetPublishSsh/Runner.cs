using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetPublishSsh
{
    internal static class Runner
    {
        public static void Run(ConnectionInfo _connectionInfo)
        {
            using (var client = new SshClient(_connectionInfo))
            {
                client.Connect();
                var command = client.RunCommand("ls");
                Console.WriteLine(command.Result);

            }
            //Console.WriteLine($"Uploaded {localFiles.Count} files.");
        }
    }
}
