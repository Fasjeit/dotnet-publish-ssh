using System;

namespace DotnetPublishSsh
{
    internal class LocalFile
    {
        /// <summary>
        /// Full file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Relative file name.
        /// </summary>
        public string RelativeName { get; set; }

        public LocalFile(string localPath, string fileName)
        {
            this.FileName = fileName;
            this.RelativeName = new Uri(localPath).MakeRelativeUri(new Uri(fileName)).OriginalString;
        }
    }
}