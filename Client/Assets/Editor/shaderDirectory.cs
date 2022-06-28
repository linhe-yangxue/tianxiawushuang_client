/********************************************************************
	created:	2015/12/02
	created:	2:12:2015   13:48
	filename: 	D:\work\svn\jishuzhongxin\ProjectService\TextureForETC1\Assets\Editor\shaderDirectory.cs
	file path:	D:\work\svn\jishuzhongxin\ProjectService\TextureForETC1\Assets\Editor
	file base:	shaderDirectory
	file ext:	cs
	author:		LIJIANJUN
	
	purpose:	
*********************************************************************/
using System.IO;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


public class shaderDirectory : MonoBehaviour
{
    
    public static List<string> excludedList = new List<string>();  //排外路径
    public static List<string> includedList = new List<string>();  //包含路径(可能包含排外路径)
    public static List<string> validList = new List<string>();  //有效路径，已排外
    public static string ProjPath = System.Environment.CurrentDirectory;   //工程根目录
    public static string AssetPath = Application.dataPath;
    public static Dictionary<string, string> ShaderDictionary;
    public static List<string> file_List;
    public static List<string> DirFileList;

    static void startget()
    {
		char version = Application.unityVersion[0];
        string path;
        if (version > '4')
            path = Application.dataPath + "/builtin_shaders-5.1.1f1(ETC1)";
        else
            path = Application.dataPath + "/builtin_shaders-4.6.8(ETC1)";

        Dictionary<string, string> shaderDir = new Dictionary<string, string>();
        string[] allShaders = Directory.GetFileSystemEntries(path, "*");		//返回所有shader的名称 ，在这里改路径
        Recursion(allShaders, shaderDir);

        //foreach (KeyValuePair<string, string> kv in shaderDir)                            //遍历keys values     在这里添加函数就可以了
        //    Debug.Log("Keys: " + kv.Key);//+ " ;   " + "Value: " + kv.Value);

        ShaderDictionary = shaderDir;
    }

    static void Recursion(string[] path, Dictionary<string, string> shaderDir)
    {
        for (int i = 0; i < path.Length; i++)
        {
            if (path[i].Contains(".meta")) //如果是meta
            {
                //Debug.Log("meta: " + path[i]);
                continue;

            }
            else   //如果不是shader   success        
            {
                if (path[i].Contains(".cginc"))   //如果是cginc
                {
                    //Debug.Log("cginc: " + path[i]);
                    continue;

                }
                else if (path[i].Contains(".shader"))//如果是shader
                {
                    shaderDir.Add(Read(path[i]), path[i]);
                }
                else    //如果是文件夹  
                {
                    string[] patha = Directory.GetFileSystemEntries(path[i], "*");		//返回所有shader的名称 ，在这里改路径 。
                    Recursion(patha, shaderDir);
                }
            }
        }
    }

    static string Read(string path)		//返回shader内第一行中的路径的名称
    {
        FileStream fs = new FileStream(path, FileMode.Open);
        StreamReader sr = new StreamReader(fs, Encoding.Default);

        //string line = sr.ReadLine();

        //int i = line.IndexOf("\"");
        //int u = line.LastIndexOf("\"");
        string ss = sr.ReadLine();
        bool isFlag = false;
        while (ss != null)
        {
            if (ss.Contains("Shader \""))
            {
                int first = ss.IndexOf("\"") + 1;
                int last = ss.LastIndexOf("\"");
                ss = ss.Substring(first, last - first);
                isFlag = true;
                break;
            }
            ss = sr.ReadLine();
        }
        sr.Close();
        sr.Dispose();
        fs.Close();
        fs.Dispose();
        if (isFlag == true)
        {
            return ss;
        }
           
        return null;

        //while (line.Length < 7 || line.Substring(0, 7) != "Shader ")
        //{
        //    line = sr.ReadLine();
        //}

        //if (line.Substring(line.Length - 1, 1) == "{")
        //    {
        //        // Console.WriteLine(line.ToString());
        //        string re = line.Substring(8);
        //        string res = re.Substring(0, re.Length - 3);
        //        return res;
        //        //Console.WriteLine(res);
        //    }

        //    else
        //    {
        //        string re = line.Substring(8);
        //        string res = re.Substring(0, re.Length - 2);
        //        return res;
        //        //Console.WriteLine(res);
        //    }
    }

    

    static List<string> startgetFiles(string dir , string apart_txt = null)
    {

        //List<string> validList = new List<string>();
        ////get exclude paths
        //if (apart_txt == null)
        //{
        //    string[] _files =  Directory.GetDirectories(dir);

        //    DirectoryInfo di = new DirectoryInfo(dir);
        //    DirectoryInfo[] allfile = di.GetDirectories();
        //    for (int i = 0; i < allfile.Length; i++)
        //    {
        //        validList.Add(allfile[i].Name);
        //    }
        //    return validList;
        //}

        //get excludepath
       
        if(apart_txt != null)
        {
            string _path = apart_txt.Replace("\\", "/");
            //Debug.Log("--->>" + apart_txt+ "不为空");
        try
        {
            StreamReader sr = new StreamReader(_path, UTF8Encoding.UTF8);
            if (sr != null)
            {
                excludedList.Clear();
                  string strline = null;               
                  while (!sr.EndOfStream)
                      {
                        strline = sr.ReadLine().Trim();
                        //Debug.Log("<-------------------------------------->:" + strline);
                         if (strline.Length != 0)
                                excludedList.Add(strline);  //global var
                     }
     
                    sr.Dispose();
                    sr.Close();
             }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log(ex.Message);
            return null;
        }
    }
        
        string[] allfiles = Directory.GetDirectories(dir); //level 1

        //for (int i = 0; i < allfiles.Length; i++)
        //{
        //    UnityEngine.Debug.Log("-------------dir---------------");
        //    UnityEngine.Debug.Log(i + ":" + allfiles[i]);

        //}
        validList.Clear();
        RecursionFiles(allfiles, validList);
        return validList;
    }

    static List<string> startgetFilesEx2(string dir_txt, string apart_txt = null)
    {
        //set excluded path
        ReadFileLines(apart_txt, ref excludedList);
        ReadFileLines(dir_txt, ref includedList);

        //UnityEngine.Debug.Log("-------------------------- ");
        //for (int i = 0; i < excludedList.Count; i++)
        //    UnityEngine.Debug.Log("excluded "+ i + ":" + excludedList[i]);

        //for (int i = 0; i < includedList.Count; i++)
        //    UnityEngine.Debug.Log("excluded " + i + ":" + includedList[i]);

        string[] allfiles = null;
        bool RootPathFromTxt = false;
        if(includedList.Count > 0)
        {
            RootPathFromTxt = true;
            allfiles  = includedList.ToArray();
        }
        else  //取默认根目录
        {
            allfiles = Directory.GetDirectories(Application.dataPath);
        }

        validList.Clear();
        RecursionFiles(allfiles, validList);

        //补上根目录路径      
        string [] roots_path = { Application.dataPath};      
        string [] apart_path = excludedList.ToArray();
        if(RootPathFromTxt == false)
        {
            roots_path = includedList.ToArray();
        }

        IncludedPathPadding(roots_path,apart_path,validList);

        return validList;
        //for (int i = 0; i < allfiles.Length; i++)
        //{
        //    UnityEngine.Debug.Log("-------------dir---------------");
        //    UnityEngine.Debug.Log(i + ":" + allfiles[i]);

        //}
    }

    static  void IncludedPathPadding(string[] dir_path, string[] apart_path, List<string> append_List)
    {
        if (dir_path == null) return;
        for (int i = 0; i < dir_path.Length; i++)
        {
            string sub_str = GetRightFormatPath(dir_path[i]);
            bool isFind = false;
            for (int j = 0; j < apart_path.Length; j++)
            {
                string appart_str = GetRightFormatPath(apart_path[j]);
                if (sub_str.Equals(appart_str))
                {
                    isFind = true;
                    break;                 
                }
            }
            if(isFind == false)
            { 
                append_List.Add(sub_str);
            }
        }
    }

    //path: 相对一级路径
    //file_List 记录有效路径
    static void RecursionFiles(string[] path, List<string> file_List)
    {
        //Debug.Log("path : " + ExcludedPath);
        if (path == null)
        {
            return;
        }
        string subDir = null;
        string subExclude = null;
        for (int i = 0; i < path.Length; i++)
        {
            bool isFind = false;

            subDir = GetRightFormatPath(path[i]);
            for (int s = 0; s < excludedList.Count; s++)
            {             
                subExclude = GetRightFormatPath(excludedList[s]);
                //UnityEngine.Debug.Log("compare it: " + subDir + "<--->" + subExclude);
                if (subDir.Equals(subExclude))
                {
                    isFind = true;
                    //UnityEngine.Debug.Log("find it: "+  path[i]);
                    break;
                }
            }

            if (isFind == false)
            {
                UnityEngine.Debug.Log("find it: " + path[i]);
                file_List.Add(subDir);
            } 
            string[] patha = Directory.GetDirectories(subDir);		//返回所有shader的名称 ，在这里改路径 。
            RecursionFiles(patha, file_List);
        }
    }

    public static Dictionary<string, string> GetShaderDictionary()
    {
        startget();
        return ShaderDictionary;
    }

    //root_dir : 传入一个根目录路径， null,默认为项目资源目录 \Assets
    //apart_txt: null ，不排外处理
    public static List<string> GetDirListFromRoot(string root_dir = null, string apart_txt = null)
    {

        //init data
        ReadFileLines(apart_txt, ref excludedList);

        includedList.Clear();
        validList.Clear();

        if (string.IsNullOrEmpty(root_dir))
        {
            root_dir = AssetPath;
        }
        includedList.Add(root_dir);
        //else
        //{
        //    UnityEngine.Debug.Log("err:root dir is null");d
        //    return validList;
        //}

        string[] allfiles = Directory.GetDirectories(root_dir);
            
        RecursionFiles(allfiles, validList);

        ////补上根目录路径      
        //string[] roots_path = { root_dir };
        //string[] apart_path = excludedList.ToArray();

        //IncludedPathPadding(roots_path, apart_path, validList);

        return validList;
       
    }

    public static List<string> GetDirListFromFile(string dir_txt, string apart_txt = null)
    {

        //init data
        ReadFileLines(dir_txt, ref includedList,false);
        ReadFileLines(apart_txt, ref excludedList);

        //UnityEngine.Debug.Log("-------------------------- ");
        //for (int i = 0; i < excludedList.Count; i++)
        //    UnityEngine.Debug.Log("excluded " + i + ":" + excludedList[i]);

        //for (int i = 0; i < includedList.Count; i++)
        //    UnityEngine.Debug.Log("included " + i + ":" + includedList[i]);

        string[] allfiles = null;
        if (includedList.Count > 0)
        {
            allfiles = includedList.ToArray();
        }
        else  //取默认根目录
        {
            Console.WriteLine("包含文件列表不能为空");
            return null;
        }

        validList.Clear();
        RecursionFiles(allfiles, validList);

        //string log_path = Application.dataPath + "/validPath_log1.txt";
       // WriteLog(log_path, validList);
        return validList;
    }

    public static string GetRightFormatPath(string _path)
    {
        //Debug.Log("aaaaaaaaaaaaaaaaaaaabbbbbbbbb "  + _path);
        return _path.Replace("\\", "/");
    }

   static void ReadFileLines(string path_utf8, ref List<string> txtList,bool bIsExcludeFile = true)
    {
        txtList.Clear();
        if (path_utf8 != null)
        {
            string _path = GetRightFormatPath(path_utf8);
            //Debug.Log("--->>" + apart_txt+ "不为空");
            try
            {
                StreamReader sr = new StreamReader(_path, UTF8Encoding.UTF8);
                if (sr != null)
                {
                    string strline = null;
                    while (!sr.EndOfStream)
                    {
                        strline = sr.ReadLine().Trim();
                        //Debug.Log("<-------------------------------------->:" + strline);
                        if (strline.Length > 0)
                        {
                            if (!strline[0].Equals('\\') && !strline[0].Equals('/'))
                                strline = "/" + strline;

                            strline = ProjPath + strline; // 拼接 "d:\proj\" + "Assets\ABC"                          
                        }
                        txtList.Add(GetRightFormatPath(strline));  
                    }
                 if (!bIsExcludeFile && txtList.Count == 0 ) //文件本内容为空
                     txtList.Add(GetRightFormatPath(AssetPath));
                    sr.Dispose();
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
                return;
            }
        }
    }

    public static bool IsNullFile(string path_utf8)
   {
       includedList.Clear();
       ReadFileLines(path_utf8,ref includedList);
       if (includedList.Count == 0)
           return true;
       else
           return false;
   }

    public static void  WriteLog(string path, string[] context)
    {
        if (context == null || string.IsNullOrEmpty(path)) return;
        string _path = GetRightFormatPath(path);
        try
        {
            StreamWriter sw = new StreamWriter(_path, false, UTF8Encoding.UTF8);
            for (int i = 0; i < context.Length; i++)
                sw.Write(context[i] + "\r\n");
            sw.Dispose();
            sw.Close();
        }
        catch (IOException ex)
        {
            UnityEngine.Debug.Log(ex.Message);
        }
    }

   public static void  WriteLog(string path, List<string> infoList)
    {
        if (infoList == null) return;
        string[] strVec = infoList.ToArray();
        WriteLog(path, strVec);
    }


}
