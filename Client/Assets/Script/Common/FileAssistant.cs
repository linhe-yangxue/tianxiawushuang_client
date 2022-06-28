using System;
using System.IO;


namespace Utilities
{
    public class FileAssistant
    {
        public static void ForEachFile(string directory, string searchPattern, Action<string> actionOnPath)
        {
            if (actionOnPath == null || !Directory.Exists(directory))
            {
                return;
            }

            foreach (var subDir in Directory.GetDirectories(directory))
            {
                ForEachFile(subDir, searchPattern, actionOnPath);
            }

            foreach (var path in Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly))
            {
                actionOnPath(path);
            }
        }
    }
}