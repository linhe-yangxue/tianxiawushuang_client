using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class AssetFileName
{
    public static string UniformResourceAssetPath(string path)
    {
        if(string.IsNullOrEmpty(path))
            return path;

        var result = path.Replace('\\', '/').ToLower();

        if(result.IndexOf("assets/resources/") >= 0)
            result = result.Substring(result.IndexOf("assets/resources/") + "assets/resources/".Length);

        var fileName = Path.GetFileNameWithoutExtension(result);

        result = Helper.CombinePath(Path.GetDirectoryName(result), fileName);
        return result;
    }
}