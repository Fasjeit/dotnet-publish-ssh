using DotnetPublishBase;
using DotnetPublishSsh;
using System;

namespace DotnetPublishPs
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Func<PublishOptions, IUploader> uploaderConstructor = n => new PsUploader(n);
            Runner.Run(args, uploaderConstructor);

            //PsUploader u = new PsUploader(new PublishOptions() { Host = "192.168.20.52", User = "tdss.local\\makarov", Password = "1qaz@WSX" });
            //u.Run("Test-Path c:\\distr", silent: false);
            //u.CreateChecksumFile("c:\\distr\\tmp");

            // publish-ps --self-contained -c Debug -r win-x64 --host "192.168.20.52" --user "tdss.local\makarov" --password "1qaz@WSX" --path "c:\distr\tmp\"
        }
    }
}