using System;

namespace DotnetPublishSsh
{
    internal class LocalFile
    {
        public string FileName { get; set; }

        public string RelativeName { get; set; }

        public LocalFile(string localPath, string fileName)
        {
            this.FileName = fileName;
            this.RelativeName = new Uri(localPath).MakeRelativeUri(new Uri(fileName)).OriginalString;
        }
    }
}