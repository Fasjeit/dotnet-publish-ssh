using DotnetPublishBase;
using System;

namespace DotnetPublishSsh
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Func<PublishOptions, IUploader> uploaderConstructor = n => new SshUploader(n);
            Runner.Run(args, uploaderConstructor);
        }
    }
}
