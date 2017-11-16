using System;
using System.IO;

namespace CAPI.Common.Services
{
    public static class FileSystem
    {
        public static bool DirectoryExists(string directoryPath)
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
    }
}
