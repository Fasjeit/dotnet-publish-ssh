using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace DotnetPublishSsh
{
    internal class Checksum : Dictionary<string, string>
    {
        public void AddEntry(string filePath, string hash)
        {
            this[filePath] = hash;
        }

        public void AddEntry(string? entry)
        {
            // format - "{sha265}  {filePath}"
            if (entry == null)
            {
                return;
            }
            var index = entry.IndexOf(' ');
            this.AddEntry(
                hash: entry.Substring(0, index),
                filePath: entry.Substring(index+2, entry.Length - index - 2));
        }

        public void AddFile(string path, LocalFile localFile)
        {
            using (FileStream fileStream = File.OpenRead(localFile.FileName))
            {
                using (var sha256 = SHA256.Create())
                {
                    var hash = sha256.ComputeHash(fileStream)
                        .ReverseEndianness()
                        .ToHexString()
                        .ToLowerInvariant();
                    this.AddEntry($"{Path.Combine(path, localFile.RelativeName)}", hash);
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
                        System.StringComparison.InvariantCultureIgnoreCase))
                {
                    diff.Add(file);
                }
            }
            return diff;
        }
    }
}
