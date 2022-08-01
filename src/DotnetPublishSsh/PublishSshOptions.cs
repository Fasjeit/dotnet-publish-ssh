using System;
using System.Linq;

namespace DotnetPublishSsh
{
    internal sealed class PublishSshOptions
    {
        public string Host { get; set; }
        public int Port { get; set; } = 22;
        public string User { get; set; }
        public string Password { get; set; }
        public string KeyFile { get; set; }
        public string Path { get; set; }
        public string LocalPath { get; set; }
        public string PreUploadCommand { get; set; }
        public string PostUploadCommand { get; set; }
        public string[] Args { get; set; }
        public bool PrintHelp { get; set; }

        public static PublishSshOptions ParseArgs(string[] args)
        {
            var options = new PublishSshOptions();

            for (var idx = 0; idx < args.Length; idx++)
            {
                var arg = args[idx];
                switch (arg)
                {
                    case "--ssh-host":
                    {
                        options.Host = PublishSshOptions.GetValue(ref args, ref idx);
                        break;
                    }
                    case "--ssh-port":
                    {
                        var value = PublishSshOptions.GetValue(ref args, ref idx);
                        options.Port = Convert.ToInt32(value);
                        break;
                    }
                    case "--ssh-user":
                    {
                        options.User = PublishSshOptions.GetValue(ref args, ref idx);
                        break;
                    }
                    case "--ssh-password":
                    {
                        options.Password = PublishSshOptions.GetValue(ref args, ref idx);
                        break;
                    }
                    case "--ssh-keyfile":
                    {
                        options.KeyFile = PublishSshOptions. GetValue(ref args, ref idx);
                        break;
                    }
                    case "--ssh-path":
                    {
                        options.Path = PublishSshOptions.GetValue(ref args, ref idx);
                        break;
                    }
                    case "--pre":
                    {
                        options.PreUploadCommand = PublishSshOptions.GetValue(ref args, ref idx);
                        break;
                    }
                    case "--post":
                    {
                        options.PostUploadCommand = PublishSshOptions.GetValue(ref args, ref idx);
                        break;
                    }
                    case "-o":
                    {
                        options.LocalPath = PublishSshOptions.GetValue(ref args, ref idx);
                        break;
                    }
                    case "-?":
                    case "-h":
                    case "--help":
                    {
                        options.PrintHelp = true;
                        break;
                    }
                }
            }

            ValidateOptions(options);

            options.Args = args;

            return options;
        }

        private static void ValidateOptions(PublishSshOptions options)
        {
            if (string.IsNullOrEmpty(options.Host) ||
                string.IsNullOrEmpty(options.User) ||
                string.IsNullOrEmpty(options.Path))
                options.PrintHelp = true;
        }

        private static string GetValue(ref string[] args, ref int idx)
        {
            if (args.Length <= idx + 1)
                throw new ArgumentException($"Missing value for option {args[idx]}");

            var value = args[idx + 1];

            args = args.Take(idx).Concat(args.Skip(idx + 2)).ToArray();
            idx--;

            return value;
        }

        private static void SkipValue(ref string[] args, ref int idx)
        {
            args = args.Take(idx).Concat(args.Skip(idx + 1)).ToArray();
            idx--;
        }
    }
}