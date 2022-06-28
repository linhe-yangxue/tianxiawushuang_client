using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using MYEDITOR;

public class RightMouse : Editor
{
    [MenuItem("Assets/Alpha + ETC1(必须为RGBA贴图)")]
    static void BuildScenes()
    {
        ETC1MaterialTexture emt = new ETC1MaterialTexture();
        string selectPath = "";
        string allPath = Application.dataPath;
		string assets = "Assets";

		string multiPaths = "";
		int all = 0;
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Texture2D), SelectionMode.Assets))
        {
            selectPath = AssetDatabase.GetAssetPath(obj);

			int idx = selectPath.LastIndexOf('.');
			if (selectPath.Substring(idx - 6, 6).Equals("_Alpha")) 
				continue;
			all++;
			string from = allPath + selectPath.Replace(assets,"");//"D:\\UnityProjects\\ETC1Tools\\Assets\\zhiwu_toutie01.png"; //
			string to = from.Insert(from.LastIndexOf("."),"_Alpha");//"D:\\UnityProjects\\ETC1Tools\\Assets\\zhiwu_toutie01_abc.png";////
//			emt.SetTextureETC1FormatCompleteEx(selectPath, TextureCompressionQuality.Normal,TextureImporterNPOTScale.ToLarger);
			multiPaths+=from+","+to+";";
		}
		if (multiPaths.Length < 1)
			return;
		multiPaths = multiPaths.Remove (multiPaths.LastIndexOf (";"));
		Debug.Log (multiPaths);
		emt.CreateMultiAlpha (multiPaths);
		AssetDatabase.Refresh ();

		int curIndex = 1;
		EditorUtility.DisplayProgressBar("转换ETC1","进度：" + "0%", 0);
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Texture2D), SelectionMode.Assets))
		{
			selectPath = AssetDatabase.GetAssetPath(obj);
			
			int idx = selectPath.LastIndexOf('.');
			if (selectPath.Substring(idx - 6, 6).Equals("_Alpha")) 
				continue;
			selectPath = AssetDatabase.GetAssetPath(obj);
			emt.SetTextureETC1FormatCompleteEx(selectPath, TextureCompressionQuality.Normal,TextureImporterNPOTScale.ToLarger);
			float f = ((float)(curIndex++)) / ((float)all);
			EditorUtility.DisplayProgressBar("转换ETC1","进度：" + (int)(f * 100) + "%", f);
		}
		EditorUtility.ClearProgressBar ();
    }

    [MenuItem("Assets/检查Alpha是否有意义(RGBA)")]
    static void CheckBW()
    {

        string selectPath = "Assets";
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            selectPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(selectPath) && File.Exists(selectPath))
            {
                Debug.Log("--->" + selectPath);
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath(selectPath, typeof(Texture2D)) as Texture2D;
            bool isFrag = false;
            float cur = 0;
            float last = 0; //old

            for (int i = 0; i < tex.width; i++)
            {

                for (int j = 0; j < tex.height; j++)
                {
                    isFrag = false;
                    if (i == 0 && j == 0)
                    {
                        last = cur = tex.GetPixel(i, j).a;
                    }
                    else
                    {
                        cur = tex.GetPixel(i, j).a; //var 1/0

                        if (Mathf.Abs(cur - last) > 0.001)
                        {
                            //UnityEngine.Debug.Log("(" + i + "," + j + ")" + " --> "+ "last = " + last + " :  " + "cur =" + cur);
                            UnityEngine.Debug.Log("Alpha贴图正常");
                            isFrag = true;
                            break;
                        }


                        else if (i == tex.width - 1 && j == tex.height - 1)
                        {
                            if (Mathf.Abs(last - 1) < 0.00001)
                            {
                                UnityEngine.Debug.LogWarning("Alpha贴图是白色的,没有意义，是否需要删除？");
                                
                            }
                            else
                                UnityEngine.Debug.LogWarning("Alpha贴图是黑色的,没有意义，是否需要删除？");

                        }
                    }
                }
                if (isFrag == true)
                    break;
            }
        }
    }

    static string GetRelativeAssetPath(string _fullPath)
    {
        _fullPath = GetRightFormatPath(_fullPath);
        int idx = _fullPath.IndexOf("Assets");
        string assetRelativePath = _fullPath.Substring(idx);
        return assetRelativePath;
    }
    static string GetRightFormatPath(string _path)
    {
        return _path.Replace("\\", "/");
    }
    static void SetTextureReadable(string _relativeAssetPath)
    {
        try
        {
            string postfix = GetFilePostfix(_relativeAssetPath);
            if (postfix == ".dds")    // no need to set .dds file.  Using TextureImporter to .dds file would get casting type error.
                return;
            TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(_relativeAssetPath);
            if (null == ti) return;
            ti.isReadable = true;
            AssetDatabase.ImportAsset(_relativeAssetPath);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError(_relativeAssetPath + "==========" + ex.Message);
        }
    }
    static string GetFilePostfix(string _filepath)   //including '.' eg ".tga", ".dds"
    {
        string postfix = "";
        int idx = _filepath.LastIndexOf('.');
        if (idx > 0 && idx < _filepath.Length)
            postfix = _filepath.Substring(idx, _filepath.Length - idx);
        return postfix;
    }
}
