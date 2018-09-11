﻿using CAPI.General.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace CAPI.General.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FileSystem : IFileSystem
    {
        public bool DirectoryExistsIfNotCreate(string directoryPath)
        {
            var pathSections = directoryPath.Split('\\');
            if (pathSections.Length < 1) return false;
            if (pathSections.Length == 1) return Directory.Exists(directoryPath);
            try
            {
                var path = pathSections[0] + '\\';
                for (var i = 1; i < pathSections.Length; i++)
                {
                    var pathToCheck = Path.Combine(path, pathSections[i]);
                    if (!Directory.Exists(pathToCheck)) Directory.CreateDirectory(pathToCheck);
                    path = Path.Combine(path, pathSections[i]);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public void CopyDirectory(string source, string target)
        {

            if (Directory.Exists(target))
                throw new Exception($"Directory {target} exists already. Unable to copy to destination.");

            Directory.CreateDirectory(target);

            foreach (var dirPath in Directory.GetDirectories(source))
            {
                var dirName = Path.GetFileName(dirPath);

                CopyDirectory(dirPath, $"{target}\\{dirName}");
            }

            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        }

        public bool DirectoryIsValidAndNotEmpty(string folderPath)
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

        public void FilesExist(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                if (!File.Exists(file))
                    throw new FileNotFoundException($"Unable to locate the following file: {file}");
            }
        }
    }
}