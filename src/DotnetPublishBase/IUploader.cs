namespace DotnetPublishBase
{
    public interface IUploader : IDisposable
    {
        char DirectorySeparator { get; set; }

        void CreateChecksumFile(string path);
        List<string> GetChecksumDiff(string path, Checksum localChecksum);
        void Run(string command, bool silent = false);
        void UploadFiles(string path, ICollection<LocalFile> localFiles);
    }
}