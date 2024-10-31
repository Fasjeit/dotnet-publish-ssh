using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace DotnetPublishBase
{
    public class Checksum : Dictionary<string, string>
    {
        public Checksum()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        public void AddEntry(string filePath, string hash)
        {
            this[filePath] = hash;
        }

        public void AddEntry(string? entry)
        {
            // format - "{sha265}  {filePath}" unix
            // format - "{sha265} {filePath}" win (ps)
            if (entry == null)
            {
                return;
            }
            if (entry.Contains("  "))
            {
                var index = entry.IndexOf(' ');
                this.AddEntry(
                    hash: entry.Substring(0, index),
                    filePath: entry.Substring(index + 2, entry.Length - index - 2));
            }
            else if (entry.Contains(" "))
            {
                var index = entry.IndexOf(" ");
                this.AddEntry(
                    hash: entry.Substring(0, index),
                    filePath: entry.Substring(index + 1, entry.Length - index - 1));
            }
        }

        public void AddFile(string path, LocalFile localFile)
        {
            using (FileStream fileStream = File.OpenRead(localFile.FileName))
            {
                using (var sha256 = HashAlgorithm.Create("SHA256"))
                {
                    if (sha256 == null)
                    {
                        throw new Exception("Cannot create hash algorithm");
                    }
                    var hash = sha256.ComputeHash(fileStream)
                        .ReverseEndianness()
                        .ToHexString()
                        .ToLowerInvariant();
                    this.AddEntry($"{path}{localFile.RelativeName}", hash);
                }
            }
        }

        public List<string> Diff(Checksum other)
        {
            var diff = new List<string>();
            foreach (var file in this.Keys)
            {
                if (!other.ContainsKey(file) ||
                    !string.Equals(
                        other[file],
                        this[file],
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    diff.Add(file);
                }
            }
            return diff;
        }
    }
}
