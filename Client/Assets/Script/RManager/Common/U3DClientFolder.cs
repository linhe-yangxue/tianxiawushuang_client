using UnityEngine;
using System.Collections;
using System.IO;

public class U3DFileIO
{
#if UNITY_IPHONE || UNITY_ANDROID
    static string WritableBaseFolder = Application.persistentDataPath;
#else
    static string WritableBaseFolder = Application.streamingAssetsPath;
#endif
    //static string WritableBaseFolder = Application.streamingAssetsPath; //Application.persistentDataPath;
    static U3DFileIO()
    {
        //WritableBaseFolder = Application.streamingAssetsPath; //Application.persistentDataPath;
    }

    public static string Combine(string pathA, string pathB)
    {
        return Common.Helper.CombinePath(pathA, pathB);
    }

    public static string UniformPath(string path)
    {
        if(path[0] == '/')
            return path.Replace('\\', '/');

        return string.Format("/{0}", path).Replace('\\', '/');
    }

    public static string GetFileWirtableFullPath(string filePath)
    {
        return Combine(WritableBaseFolder, filePath);
    }

    public static string GetFileU3dPath(string fileFullPath)
    {
        var index = fileFullPath.IndexOf(WritableBaseFolder);
        if(index >= 0)
            return fileFullPath.Substring(index + WritableBaseFolder.Length);

        return "";
    }

    public static string[] GetFiles(string path, string pattern, SearchOption searchOpetion)
    {
        path = Combine(WritableBaseFolder, path);
        return Directory.GetFiles(path, pattern, searchOpetion);
    }
    
    public static bool Exists(string filePath)
    {
        var path = Combine(WritableBaseFolder, filePath);
        return Directory.Exists(path) || File.Exists(path);
    }

    public static bool CreateFolder(string folderName)
    {
        var dir = Combine(WritableBaseFolder, folderName);

        if(Directory.Exists(dir))
            return true;

        try
        {
            var dirInfo = Directory.CreateDirectory(dir);
            return dirInfo != null && dirInfo.Attributes == FileAttributes.Directory;
        }
        catch
        {
            return false;
        }
    }

    public static bool DeleteFolder(string folderName)
    {
        var dir = Combine(WritableBaseFolder, folderName);

        if(!Directory.Exists(dir))
            return true;

        try
        {
            Directory.Delete(dir);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static FileStream CreateFile(string fileName)
    {
        var file = Combine(WritableBaseFolder, fileName);

        try
        {
            if(File.Exists(file))
                return File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

            var dir = Path.GetDirectoryName(file);
            if(!Directory.Exists(dir))
            {
                var dirInfo = Directory.CreateDirectory(dir);
                if(dirInfo == null)
                    return null;
            }

            return File.Create(file);
        }
        catch
        {
            return null;
        }
    }

    public static FileStream OpenFileForRead(string fileName)
    {
        var file = Combine(WritableBaseFolder, fileName);

        try
        {
            if(File.Exists(file))
                return File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            else
                return null;
        }
        catch
        {
            return null;
        }
    }

    public static FileStream OpenFileForReadWrite(string fileName)
    {
        var file = Combine(WritableBaseFolder, fileName);

        try
        {
            if(File.Exists(file))
                return File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            else
                return null;
        }
        catch
        {
            return null;
        }
    }

    public static bool DeleteFile(string fileName)
    {
        var file = Combine(WritableBaseFolder, fileName);

        if(!File.Exists(file))
            return true;

        try
        {
            File.Delete(file);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool MoveFile(string source, string target)
    {
        var sFile = Combine(WritableBaseFolder, source);
        var tFile = Combine(WritableBaseFolder, target);

        try
        {
            if (File.Exists(tFile))
                File.Delete(tFile);

            File.Move(sFile, tFile);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
