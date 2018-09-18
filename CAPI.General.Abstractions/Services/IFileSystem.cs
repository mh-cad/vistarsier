using System.Collections.Generic;

namespace CAPI.General.Abstractions.Services
{
    public interface IFileSystem
    {
        bool DirectoryExistsIfNotCreate(string directoryPath);
        void CopyDirectory(string source, string target);
        /// <summary>
        /// Returns true if folder exists and contains at least one file
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        bool DirectoryIsValidAndNotEmpty(string folderPath);
        void FilesExist(IEnumerable<string> files);
    }
}
