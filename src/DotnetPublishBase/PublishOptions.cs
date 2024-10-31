using System;
using System.Linq;

namespace DotnetPublishBase
{
    public sealed class PublishOptions
    {
        public string Host { get; set; }
        public int Port { get; set; } = 0;
        public string User { get; set; }
        public string? Password { get; set; }
        public string? KeyFile { get; set; }
        public string Path { get; set; }
        public string LocalPath { get; set; }
        public string? PreUploadCommand { get; set; }
        public string? PostUploadCommand { get; set; }
        public bool Diff { get; set; } = false;
        public string[] Args { get; set; } = Array.Empty<string>();
        public bool PrintHelp { get; set; }

        public static PublishOptions ParseArgs(string[] args)
        {
            var options = new PublishOptions();

            for (var idx = 0; idx < args.Length; idx++)
            {
                var arg = args[idx];
                switch (arg)
                {
                    case "--ssh-host":
                    case "--host":
                    {
                        options.Host = GetValue(ref args, ref idx);
                        break;
                    }
                    case "--ssh-port":
                    case "--port":
                    {
                        var value = GetValue(ref args, ref idx);
                        options.Port = Convert.ToInt32(value);
                        break;
                    }
                    case "--ssh-user":
                    case "--user":
                    {
                        options.User = GetValue(ref args, ref idx);
                        break;
                    }
                    case "--ssh-password":
                    case "--password":
                    {
                        options.Password = GetValue(ref args, ref idx);
                        break;
                    }
                    case "--ssh-keyfile":
                    case "--keyfile":
                    {
                        options.KeyFile = GetValue(ref args, ref idx);
                        break;
                    }
                    case "--ssh-path":
                    case "--path":
                    {
                        options.Path = GetValue(ref args, ref idx);
                        break;
                    }
                    case "--pre":
                    {
                        options.PreUploadCommand = GetValue(ref args, ref idx);
                        break;
                    }
                    case "--post":
                    {
                        options.PostUploadCommand = GetValue(ref args, ref idx);
                        break;
                    }
                    case "--diff":
                    {
                        SkipValue(ref args, ref idx);
                        options.Diff = true;
                        break;
                    }
                    case "-o":
                    {
                        options.LocalPath = GetValue(ref args, ref idx);
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

        private static void ValidateOptions(PublishOptions options)
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