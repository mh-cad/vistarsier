using System;
using System.Collections.Generic;
using System.IO;

namespace VisTarsier.Common
{
    public static class FileSystem
    {
        public static bool DirectoryExistsIfNotCreate(string directoryPath)
        {
            if (Directory.Exists(directoryPath)) { return true; }

            try
            {
                Directory.CreateDirectory(directoryPath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static void CopyDirectory(string source, string target)
        {

            if (Directory.Exists(target))
            { 
                throw new Exception($"Directory {target} exists already. Unable to copy to destination."); 
            }

            Directory.CreateDirectory(target);

            foreach (var dirPath in Directory.GetDirectories(source))
            {
                var dirName = Path.GetFileName(dirPath);

                CopyDirectory(dirPath, $"{target}\\{dirName}");
            }

            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns true if folder exists and contains at least one file
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static bool DirectoryIsValidAndNotEmpty(string folderPath)
        {
            try
            {
                return
                    !string.IsNullOrEmpty(folderPath) &&
                    Directory.Exists(folderPath) &&
                    Directory.GetFiles(folderPath).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public static void FilesExist(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                if (!File.Exists(file))
                    throw new FileNotFoundException($"Unable to locate the following file: {file}");
            }
        }
    }
}
