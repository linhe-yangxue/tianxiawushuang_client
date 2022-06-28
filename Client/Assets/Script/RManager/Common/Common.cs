using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using Common;


public enum MessageInformType
{
    MSG_ERROR   = -1,
    MSG_INFO    = 0,
    MSG_WARN    = 1,
    MSG_LOG     = 2,
    MSG_VERBOSE = 3
}


public interface IU3dCroutineHelper
{
    Coroutine StartCoroutine(IEnumerator coroutine);
}

public static class StringExt
{
    public static string GetMD5(this string str)
    {
        var mem = new MemoryStream(Encoding.UTF8.GetBytes(str));
        return Helper.GetMD5OfStream(mem);
    }
}

public static class WWWExt
{
    public static int ForceLoadSync(this WWW www)
    {
        var len = www.size;//www.bytes.Length; //
        return len;
    }
}

public static class StreamExt
{
    public static string GetMD5(this System.IO.Stream stream)
    {
        return MD5.CalculateMD5(stream);
    }
}

namespace Common
{
    public class Helper
    {
        public static string GetMD5OfStream(System.IO.Stream stream)
        {
            return MD5.CalculateMD5(stream);
        }

        public static string CombinePath(string pathA, string pathB)
        {
            var result = string.Format("{0}&/{1}", pathA.Replace('\\', '/'), pathB.Replace('\\', '/'));

            if (result.IndexOf("/&/") > 0)
            {
                if (result.IndexOf("&//") > 0)
                    return result.Replace("/&//", "/");

                return result.Replace("/&/", "/");
            }

            if (result.IndexOf("&//") > 0)
                return result.Replace("&//", "/");

            return result.Replace("&/", "/");
        }

        public static void EnsureDirValid(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public static string GetResourcePath(string unityPath)
        {
            var i = unityPath.ToLower().IndexOf("assets/resources/");
            var result = unityPath;

            if (i >= 0)
                result = unityPath.Substring(i + "assets/resources/".Length);

            return result;
        }
    }

    public class CoroutineResult<T>
    {
        public IEnumerator Coroutine { get; set; }
        public bool HasError { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public T Reuslt { get; set; }

        public bool IsDone { get; set; }

        public CoroutineResult()
        {
            Coroutine = null;
            HasError = false;
            ErrorCode = 0;
            ErrorMessage = "";
            IsDone = false;
        }

        public void DoneByError(string errMsg, int errCode)
        {
            ErrorCode = errCode;
            ErrorMessage = errMsg;
            IsDone = true;
            HasError = true;
        }

        public void DoneByError(string errMsg)
        {
            ErrorMessage = errMsg;
            IsDone = true;
            HasError = true;
        }

        public void Completed(T reuslt)
        {
            Reuslt = reuslt;
            IsDone = true;
            HasError = false;
        }

        public void CompleteWithError(string errMsg, T reuslt, int errCode)
        {
            Completed(reuslt);
            DoneByError(errMsg, errCode);
        }


    }
}