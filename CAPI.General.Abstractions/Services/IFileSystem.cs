using System.Collections.Generic;

namespace CAPI.General.Abstractions.Services
{
    public interface IFileSystem
    {
        bool DirectoryExistsIfNotCreate(string directoryPath);
        void CopyDirectory(string source, string target);
        bool DirectoryIsValidAndNotEmpty(string folderPath);
        void FilesExist(IEnumerable<string> files);
    }
}
