/********************************************************************
	created:	2015/11/03
	created:	3:11:2015   18:37
	filename: 	D:\work\svn\jishuzhongxin\ProjectService\TextureForETC1\Assets\Editor\ETC1MaterialTexture.cs
	file path:	D:\work\svn\jishuzhongxin\ProjectService\TextureForETC1\Assets\Editor
	file base:	ETC1MaterialTexture
	file ext:	cs
	author:		PIYEJUN
	
	purpose:	
*********************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace MYEDITOR
{
    public delegate void DelegateTextureTool(float fPercent);
    public delegate void DelegateShowName(string str);

    //yzm:FromRoot,FromFile系列函数不能复合使用
    public class ETC1MaterialTexture : MonoBehaviour
    {
        public string m_AlphaPostfix = "_Alpha.png";
        public List<string> m_StringList = new List<string>();
        public List<string> m_usedList = new List<string>();
        public int m_buildIn = 0;
        public int m_NGUI = 0;

        #region 辅助函数
        public bool IsPowerOfTwo(int i)
        {
            return ((i > 0) && ((i & (i - 1)) == 0));
        }

        string GetRightFormatPath(string _path)
        {
            return _path.Replace("\\", "/");
        }

        string GetRelativeAssetPath(string _fullPath)
        {
            _fullPath = GetRightFormatPath(_fullPath);
            int idx = _fullPath.IndexOf("Assets");
            string assetRelativePath = _fullPath.Substring(idx);
            return assetRelativePath;
        }

        bool IsTextureFile(string _path)
        {
            string path = _path.ToLower();
            bool b = path.EndsWith(".psd")
                || path.EndsWith(".tga")
                || path.EndsWith(".png")
                || path.EndsWith(".jpg")
                || path.EndsWith(".dds")
                || path.EndsWith(".bmp")
                || path.EndsWith(".tif")
                || path.EndsWith(".gif");
            return b;
        }

        string GetAlphaTexNameByMainTexName(string path)
        {
            string alphaTexName = null;
            int idx = path.LastIndexOf('.');
            // xx_Alpha.tga -> xx  + _Alpha.png
            if (!path.Substring(idx - 6, 6).Equals("_Alpha"))  //assert path
            {
                alphaTexName = path.Substring(0, idx) + m_AlphaPostfix;
                //UnityEngine.Debug.Log("-------texture path-----");
                //UnityEngine.Debug.Log(path + ">>>" + idx + "<<<" + path.Substring(0, idx));
            }
            return alphaTexName;


        }

        string GetFilePostfix(string _filepath)   //including '.' eg ".tga", ".dds"
        {
            string postfix = "";
            int idx = _filepath.LastIndexOf('.');
            if (idx > 0 && idx < _filepath.Length)
                postfix = _filepath.Substring(idx, _filepath.Length - idx);
            return postfix;
        }



        void SetTextureReadable(string _relativeAssetPath)
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

        void SetTextureETC1Format(string relativePath)
        {
            try
            {
                TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(relativePath);
                if (null == ti) return;
                ti.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC_RGB4, (int)TextureCompressionQuality.Best);
            }
            catch (System.InvalidCastException)
            {
                UnityEngine.Debug.LogError("SetTextureETC1Format() exception, file name:" + relativePath);
            }
        }

        void SetTextureRGBA32Format(string relativePath)
        {
            try
            {
                TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(relativePath);
                if (null == ti) return;
                ti.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.RGBA32);
            }
            catch (System.InvalidCastException)
            {
                UnityEngine.Debug.LogError("SetTextureETC1Format() exception, file name:" + relativePath);
            }
        }

        void SetTextureETC1FormatComplete(string relativePath)
        {
            try
            {
                TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(relativePath);
                if (null == ti) return;
                ti.textureType = TextureImporterType.Advanced;
                ti.npotScale = TextureImporterNPOTScale.ToNearest;
                ti.compressionQuality = (int)TextureCompressionQuality.Best;
                //ti.wrapMode = TextureWrapMode.Repeat;
                //ti.filterMode = FilterMode.Bilinear;
                //ti.generateMipsInLinearSpace = false;
                //ti.mipmapEnabled = false;
                ti.isReadable = false;
                ti.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC_RGB4, (int)TextureCompressionQuality.Best);

                char version = Application.unityVersion[0];
                if (version > '4')
                    AssetDatabase.WriteImportSettingsIfDirty(relativePath);
                else
                    AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);


            }
            catch (System.InvalidCastException)
            {
                UnityEngine.Debug.LogError("SetTextureETC1FormatComplete() exception, file name:" + relativePath);
            }

        }

        void AddShaderPath(string path)
        {
            string str = m_StringList.Find(s => s.Equals(path));
            if (null == str)
            {
                m_StringList.Add(path);
            }
        }

        public string GetAssetPath()
        {
            string str = UnityEngine.Application.dataPath;
            return str.Substring(0, str.Length - 6);
        }
        #endregion

        // 调用外部工具来生成 alpha 图  ////////////////
        void CreateAlpha(string from, string to)
        {
            string caName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar , Path.DirectorySeparatorChar, "ca.exe");
            if (File.Exists(caName))
            {
                Process p = new Process();
                p.StartInfo.FileName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar , Path.DirectorySeparatorChar, "ca.exe");
                p.StartInfo.Arguments = string.Concat("\"0\" ", "\"", from, "\" \"", to, "\"");
                p.Start();
                p.WaitForExit();
                //UnityEngine.Debug.Log("Succeed to Seperate RGB and Alpha channel for texture : " + from);
            }
        }
		// 调用外部工具来生成多个alpha 图  ////////////////
		public void CreateMultiAlpha(string paths)
		{
			string caName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar , Path.DirectorySeparatorChar, "ca.exe");
			if (File.Exists(caName))
			{
				Process p = new Process();
				p.StartInfo.FileName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar , Path.DirectorySeparatorChar, "ca.exe");
				p.StartInfo.Arguments = string.Concat("\"3\" ", "\"", paths,"\"");
				UnityEngine.Debug.Log(p.StartInfo.Arguments );
				p.Start();
				p.WaitForExit();
				//UnityEngine.Debug.Log("Succeed to Seperate RGB and Alpha channel for texture : " + from);
			}
			
		}


        // 查找所有材质引用到的shader /////////////////////////
        public IEnumerator FindAllShaders(DelegateTextureTool func)
        {
            m_StringList.Clear();
            string[] matPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
            for (int i = 0; i < matPaths.Length; ++i)
            {
                string path = matPaths[i];
                string relativePath = GetRelativeAssetPath(path);
                Material mat = (Material)AssetDatabase.LoadAssetAtPath(relativePath, typeof(Material));
                if (null == mat) continue;

                Object[] roots = new Object[] { mat };
                Object[] dependObjs = EditorUtility.CollectDependencies(roots);
                foreach (Object obj in dependObjs)
                {
                    if (obj.GetType() == typeof(Shader))
                    {
                        string shaderPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                        AddShaderPath(shaderPath + "<---->" + obj.name);
                    }
                }
                float f = ((float)i) / ((float)matPaths.Length);
                func(f);
                yield return 0;
            }
            func(1f);
        }


        // 给项目内所有有alpha 通道的图片生成 alpha 图片 /////////////////////////
        public IEnumerator GenAlphaTextureForAll(DelegateTextureTool func,string dir,string excluded_txt)
        {
            string caName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar , Path.DirectorySeparatorChar, "ca.exe");
            if (File.Exists(caName))
            {
                Process p = new Process();
                p.StartInfo.FileName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, "ca.exe");
           
                p.StartInfo.Arguments = string.Concat("\"1\" \"", dir,excluded_txt, "\"");        //更改主路径，只要是这个路径下的所有的路径都会走
                p.Start();
                p.WaitForExit();

                //List<string> box_List = shaderDirectory.GetFilesList();
        
                //foreach (string box in box_List)
                //{
                //    //UnityEngine.Debug.Log(box + "aaa");
                //    p.StartInfo.Arguments = string.Concat("\"1\" \"", box , "\"");        //更改主路径，只要是这个路径下的所有的路径都会走
                //    p.Start();
                //    p.WaitForExit();
                    
                //}
            }
            func(1f);
            yield return 0;
            AssetDatabase.Refresh();
        }

        // 给项目内所有有alpha 通道的图片生成 alpha 图片 /////////////////////////
        public IEnumerator GenAlphaTextureForAllFromRoot(DelegateTextureTool func , string excluded_path = null)
        {

            UnityEngine.Debug.Log("-----> in GenAlphaTextureForAllFromRoot ");
            string caName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar , Path.DirectorySeparatorChar, "ca.exe");
            if (File.Exists(caName))
            {
                Process p = new Process();
                p.StartInfo.FileName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, "ca.exe");

                string post_str = "\""; 

                if (excluded_path != null)
                    post_str = "\" \"" + excluded_path + post_str;

                //string ss = string.Concat("\"1\" \"", MaterialTextureEditor.DirPath, post_str);
                //UnityEngine.Debug.Log("----->" + ss);

                p.StartInfo.Arguments = string.Concat("\"1\" \"", MaterialTextureEditor.RootPath, post_str);
                p.Start();
                p.WaitForExit();
            }
            func(1f);
            yield return 0;
            AssetDatabase.Refresh();
        }


        //传给ca.exe 参数必需有效
        public IEnumerator GenAlphaTextureForAllFromFile(DelegateTextureTool func, string includedPath_txt, string excludedPath_txt = null)
        {
            //如果included_path文件为空，设置指定目录当做根结点

            bool bFromRoot = false;
            if (shaderDirectory.IsNullFile(includedPath_txt))
            {
                includedPath_txt = MaterialTextureEditor.RootPath;
                bFromRoot = true;
            }
        

                string caName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, "ca.exe");

                if (File.Exists(caName))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, "ca.exe");

                    string post_str = "\"";

                    if (excludedPath_txt != null)
                        post_str = "\" \"" + excludedPath_txt + post_str;


                    if (bFromRoot == true)
                    {
                        p.StartInfo.Arguments = string.Concat("\"1\" \"", includedPath_txt, post_str);
                    }
                    else
                    {
                        p.StartInfo.Arguments = string.Concat("\"2\" \"", includedPath_txt, post_str);
                    }
                    
                   //UnityEngine.Debug.Log("----->" + p.StartInfo.Arguments);

                    p.Start();
                    p.WaitForExit();
                
                func(1f);
                yield return 0;
                AssetDatabase.Refresh();
            }
        }


        // 修改项目内所有有特殊属性的材质 /////////////////////////
        public IEnumerator ModifyMaterialWhichHasFlag(DelegateTextureTool func)
        {
            string[] matPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
            for (int i = 0; i < matPaths.Length; ++i)
            {
                // 1. 查看材质是否有 _MainTex_Alpha 属性 //////////////////
                string path = matPaths[i];
                string relativePath = GetRelativeAssetPath(path);
                Material mat = (Material)AssetDatabase.LoadAssetAtPath(relativePath, typeof(Material));
                if (null == mat) continue;
                //if (!mat.HasProperty("_MainTex")) continue;
                //if (!mat.HasProperty("_MainTex_Alpha")) continue;
                if (!mat.HasProperty("_UseSecondAlpha")) continue;

                Texture2D mainTex = mat.GetTexture("_MainTex") as Texture2D;
                if (null == mainTex)
                {
                    UnityEngine.Debug.LogError("&& 主纹理没有设置, 材质名:" + path);
                    continue;
                }


                // 2. 找出alpha 纹理名字  //////////////
                string alphaTexName = null;
                string mainTexPath = null;
                Object[] roots = new Object[] { mat };
                Object[] dependObjs = EditorUtility.CollectDependencies(roots);
                foreach (Object obj in dependObjs)
                {
                    if (obj.GetType() == typeof(Texture2D))
                    {
                        mainTexPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                        if (null != mainTexPath && mainTexPath.Contains(mainTex.name))
                        {
                            alphaTexName = GetAlphaTexNameByMainTexName(mainTexPath);
                            break;
                        }
                    }
                }
	 			roots = null;
                dependObjs = null;
                if (null == alphaTexName) continue;


                //处理自发光图
                if (mat.HasProperty("_Illum_Alpha"))
                {
                    Texture2D illumTex = mat.GetTexture("_Illum") as Texture2D;
                    if (null == illumTex)
                    {
                        UnityEngine.Debug.LogError("&& 自发光纹理没有设置, 材质名:" + path);
                        continue;
                    }


                    string alphaTexName2 = null;
                    string illumTexPath = null;
                    Object[] roots2 = new Object[] { mat };
                    Object[] dependObjs2 = EditorUtility.CollectDependencies(roots2);
                    foreach (Object obj in dependObjs2)
                    {
                        if (obj.GetType() == typeof(Texture2D))
                        {
                            illumTexPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                            if (null != mainTexPath && illumTexPath.Contains(illumTex.name))
                            {
                                alphaTexName2 = GetAlphaTexNameByMainTexName(illumTexPath);
                                break;
                            }
                        }
                    }
					 roots2 = null;
                    dependObjs2 = null;
                    if (null == alphaTexName2) continue;

                    Texture2D alphaIlumTex = AssetDatabase.LoadAssetAtPath(alphaTexName2, typeof(Texture2D)) as Texture2D;
                    if (null == alphaIlumTex)
                    {
                        UnityEngine.Debug.LogError("&& Alpha纹理不存在:" + path);
                        UnityEngine.Debug.Log(alphaTexName2);
                        continue;
                    }
                    mat.SetTexture("_Illum_Alpha", alphaIlumTex);
				
					//处理高度图
                    if (mat.HasProperty("_ParallaxMap_Alpha"))
                    {
                        Texture2D HeightTex = mat.GetTexture("_ParallaxMap") as Texture2D;
                        if (null == HeightTex)
                        {
                            UnityEngine.Debug.LogError("&& 高光纹理没有设置, 材质名:" + path);
                            continue;
                        }


                        string alphaTexName3 = null;
                        string HeightTexPath = null;
                        Object[] roots3 = new Object[] { mat };
                        Object[] dependObjs3 = EditorUtility.CollectDependencies(roots3);
                        foreach (Object obj in dependObjs3)
                        {
                            if (obj.GetType() == typeof(Texture2D))
                            {
                                HeightTexPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                                if (null != mainTexPath && HeightTexPath.Contains(HeightTex.name))
                                {
                                    alphaTexName3 = GetAlphaTexNameByMainTexName(HeightTexPath);
                                    break;
                                }
                            }
                        }

                        roots3 = null;
                        dependObjs3 = null;
                        if (null == alphaTexName3) continue;

                        Texture2D alphaHeightTex = AssetDatabase.LoadAssetAtPath(alphaTexName3, typeof(Texture2D)) as Texture2D;
                        if (null == alphaHeightTex)
                        {
                            UnityEngine.Debug.LogError("&& Alpha纹理不存在:" + path);
                            UnityEngine.Debug.Log(alphaTexName3);
                            continue;
                        }
                        mat.SetTexture("_ParallaxMap_Alpha", alphaHeightTex);
                    }
                }


                // 3. 修改材质  //////////////
                //if (alphaTexName.Contains("_Alpha_Alpha"))
                //{
                //    continue;
                //}
                // else 
                //{
                Texture2D alphaTex = AssetDatabase.LoadAssetAtPath(alphaTexName, typeof(Texture2D)) as Texture2D;
                if (null == alphaTex)
                {
                    UnityEngine.Debug.LogError("&& Alpha纹理不存在:" + path);
                    UnityEngine.Debug.Log(alphaTexName);
                    continue;
                }
                mat.SetTexture("_MainTex_Alpha", alphaTex);
                mat.SetInt("_UseSecondAlpha", 1);


                // }


                // 4. 修改纹理格式 //////////
                //SetTextureETC1Format(mainTexPath);
                //SetTextureETC1FormatComplete(alphaTexName);

                float f = ((float)i) / ((float)matPaths.Length);
                func(f);
				EditorUtility.DisplayProgressBar("材质修改中,请稍候","进度："+(int)(f*100)+"%",f);
                yield return new WaitForSeconds(0.01f);
            }
            func(1f);
			EditorUtility.DisplayProgressBar("材质修改中,请稍候","进度：100%",1f);
			EditorUtility.ClearProgressBar ();
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        // 删除所有 alpha 纹理 并还原材质  /////////////////////////
        public IEnumerator RestoreMaterialAndRemoveAlpha(DelegateTextureTool func,string dirPath,string excludedPath_txt)
        {
            RemoveAllAlphaTexRoot(dirPath,excludedPath_txt);
            func(0.5f);
            yield return new WaitForSeconds(0.5f);
            RestoreAllMaterials();
            func(1f);
            yield return new WaitForSeconds(0.5f);
        }

        public IEnumerator RestoreMaterialAndRemoveAlphaFromRoot(DelegateTextureTool func, string dirPath)
        {
            RemoveAllAlphaTexRoot(dirPath, null);
            func(0.5f);
            yield return new WaitForSeconds(0.5f);
            RestoreAllMaterials();
            func(1f);
            yield return new WaitForSeconds(0.5f);
        }

        public IEnumerator RestoreMaterialAndRemoveAlphaFromFile(DelegateTextureTool func, string dirPath, string excludedPath)
        {
            RemoveAllAlphaTexFromFile(dirPath, excludedPath);
            func(0.5f);
            yield return new WaitForSeconds(0.5f);
            RestoreAllMaterials();
            func(1f);
            yield return new WaitForSeconds(0.5f);
        }

        // 删除所有 alpha 纹理  //////////////////////////////////
        public void RemoveAllAlphaTexRoot(string dirPath, string excludedPath)
        {
            UnityEngine.Debug.Log("&& 开始删除纹理 &&");
            string[] matPaths = shaderDirectory.GetDirListFromRoot(dirPath,excludedPath).ToArray();
            for (int m = 0; m < matPaths.Length; m++)
            {
                string[] subPaths = Directory.GetFiles(matPaths[m], "*.*", SearchOption.TopDirectoryOnly);

                //UnityEngine.Debug.Log("-->" + matPaths[m]);
                for (int i = 0; i < subPaths.Length; ++i)
                {
                    string path = subPaths[i];
                    if (path.EndsWith(m_AlphaPostfix))
                    {
                        File.Delete(path);
                    }
                }
            }
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("&& 删除纹理结束 &&");
        }

        public void RemoveAllAlphaTexFromFile(string includedPath_txt, string excludedPath)
        {
            //UnityEngine.Debug.Log("&& 开始删除纹理 &&");

            //如果included_path文件为空，设置指定目录当做根结点
            //if (shaderDirectory.IsNullFile(includedPath_txt))
            //    includedPath_txt = MaterialTextureEditor.RootPath;

            string[] matPaths = shaderDirectory.GetDirListFromFile(includedPath_txt, excludedPath).ToArray();

            for (int m = 0; m < matPaths.Length; m++)
            {
                string[] subPaths = Directory.GetFiles(matPaths[m], "*.*", SearchOption.TopDirectoryOnly);

                //UnityEngine.Debug.Log("-->" + matPaths[m]);
                for (int i = 0; i < subPaths.Length; ++i)
                {
                    string path = subPaths[i];
                    if (path.EndsWith(m_AlphaPostfix))
                    {
                        File.Delete(path);
                    }
                }
				float f = ((float)m) / ((float)matPaths.Length);
				EditorUtility.DisplayProgressBar("删除alpha","进度："+(int)(f*100)+"%",f);
            }
			EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
           // UnityEngine.Debug.Log("&& 删除纹理结束 &&");
        }

        // 还原所有材质  //////////////////////////////
        public void RestoreAllMaterials()
        {
            UnityEngine.Debug.Log("&& 开始还原材质 &&");
            string[] matPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
			int curIndex = 0;
            foreach (string path in matPaths)
            {
                string properMatPath = GetRelativeAssetPath(path);
                Material mat = (Material)AssetDatabase.LoadAssetAtPath(properMatPath, typeof(Material));
				if (mat)
				{
					//if (!mat.HasProperty("_MainTex_Alpha")) continue;
					if (!mat.HasProperty("_UseSecondAlpha")) continue;
					
					mat.SetTexture("_MainTex_Alpha", null);
					if(mat.HasProperty("_Illum_Alpha"))
						mat.SetTexture("_Illum_Alpha", null);
					if (mat.HasProperty("_ParallaxMap_Alpha"))
                        mat.SetTexture("_ParallaxMap_Alpha", null);
					mat.SetInt("_UseSecondAlpha", 0);
				}
				float f = ((float)curIndex++) / ((float)matPaths.Length);
				EditorUtility.DisplayProgressBar("还原材质","进度："+(int)(f*100)+"%",f);
			}
			EditorUtility.DisplayProgressBar("还原材质","进度：100%",1f);
			EditorUtility.ClearProgressBar ();
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("&& 还原材质完成 &&");
        }



        // 拆分 RGB 和 alpha 数据 //////////////////
        public void SeperateRGBAandMerge(string _texPath)
        {
            string assetRelativePath = GetRelativeAssetPath(_texPath);
            SetTextureReadable(assetRelativePath);
            Texture2D sourcetex = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(Texture2D)) as Texture2D;  //not just the textures under Resources file
            
            if (!sourcetex)
            {
                UnityEngine.Debug.LogError("Load Texture Failed : " + assetRelativePath);
                return;
            }

            if ((!IsPowerOfTwo(sourcetex.width)) || (!IsPowerOfTwo(sourcetex.height)))
            {
                UnityEngine.Debug.LogError("Texture width or height not the power of 2: " + assetRelativePath);
                //return;
            }
            Texture2D finalTex = new Texture2D(sourcetex.width, sourcetex.height, TextureFormat.RGB24, true);
            int w = sourcetex.width;
            for (int i = 0; i < sourcetex.width; ++i)
            {
                for (int j = 0; j < sourcetex.height; ++j)
                {
                    Color color = sourcetex.GetPixel(i, j);
                    Color rgbColor = color;
                    Color alphaColor = color;
                    alphaColor.r = color.a;
                    alphaColor.g = color.a;
                    alphaColor.b = color.a;
                    finalTex.SetPixel(i, j, rgbColor);
                    finalTex.SetPixel(i , j, alphaColor);
                }
            }

            finalTex.Apply();
            byte[] finalData = finalTex.EncodeToPNG();
            int dot = _texPath.IndexOf(".");
            string a_path = _texPath.Substring(0,dot) + "_Alpha.png";
            //string _path1 = GetAlphaTexNameByMainTexName(_texPath);
            File.WriteAllBytes(a_path, finalData);    //&& _path1  _texPath
            UnityEngine.Debug.Log("Succeed to merge texture:" + _texPath);
        }

		
		// 拆分 RGB 和 alpha 数据, 然后平铺数据   //////////////////
        public void SeperateRGBAandMerge_back(string _texPath)
        {
            string assetRelativePath = GetRelativeAssetPath(_texPath);
            SetTextureReadable(assetRelativePath);
            Texture2D sourcetex = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(Texture2D)) as Texture2D;  //not just the textures under Resources file
            if (!sourcetex)
            {
                UnityEngine.Debug.LogError("Load Texture Failed : " + assetRelativePath);
                return;
            }

            if ((!IsPowerOfTwo(sourcetex.width)) || (!IsPowerOfTwo(sourcetex.height)))
            {
                UnityEngine.Debug.LogError("Texture width or height not the power of 2: " + assetRelativePath);
                //return;
            }
            Texture2D finalTex = new Texture2D(sourcetex.width * 2, sourcetex.height, TextureFormat.RGB24, true);
            int w = sourcetex.width;
            for (int i = 0; i < sourcetex.width; ++i)
            {
                for (int j = 0; j < sourcetex.height; ++j)
                {
                    Color color = sourcetex.GetPixel(i, j);
                    Color rgbColor = color;
                    Color alphaColor = color;
                    alphaColor.r = color.a;
                    alphaColor.g = color.a;
                    alphaColor.b = color.a;
                    finalTex.SetPixel(i, j, rgbColor);
                    finalTex.SetPixel(i + w, j, alphaColor);
                }
            }

            finalTex.Apply();
            byte[] finalData = finalTex.EncodeToPNG();
            //string _path1 = GetAlphaTexNameByMainTexName(_texPath);
            File.WriteAllBytes(_texPath, finalData);    //&& _path1  _texPath
            UnityEngine.Debug.Log("Succeed to merge texture:" + _texPath);
        }

        public void AddAlpha2Picture(string _path)
        {
            string assetRelativePath = GetRelativeAssetPath(_path);
            SetTextureReadable(assetRelativePath);
            Texture2D sourcetex = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(Texture2D)) as Texture2D;  //not just the textures under Resources file
            if (!sourcetex)
            {
                UnityEngine.Debug.LogError("Load Texture Failed : " + assetRelativePath);
                return;
            }

            Texture2D finalTex = new Texture2D(sourcetex.width, sourcetex.height, TextureFormat.ARGB32, true);

            int w = finalTex.width;
            for (int i = 0; i < finalTex.width; ++i)
                for (int j = 0; j < finalTex.height; ++j)
                {
                    Color rgbColor = sourcetex.GetPixel(i, j);
                    Color finalColor = rgbColor;
                    finalColor.a = 0.99f;
                    finalTex.SetPixel(i, j, finalColor);
                }

            finalTex.Apply();
            byte[] finalData = finalTex.EncodeToPNG();
            File.WriteAllBytes(_path, finalData);
            UnityEngine.Debug.Log("Succeed to merge texture:" + _path);
        }

        //通过shader名查找所有引用它的材质  ////////////////////
        public void FindMaterialsByShaderName(string shaderName)
        {
            m_StringList.Clear();
            // 1. 查找所有材质 /////////////////////////
            string[] matPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
            for (int i = 0; i < matPaths.Length; ++i)
            {
                string path = matPaths[i];
                string relativePath = GetRelativeAssetPath(path);
                Material mat = (Material)AssetDatabase.LoadAssetAtPath(relativePath, typeof(Material));
                if (null == mat) continue;

                Object[] roots = new Object[] { mat };
                Object[] dependObjs = EditorUtility.CollectDependencies(roots);
                foreach (Object obj in dependObjs)
                {
                    if (obj.GetType() == typeof(Shader)
                        && obj.name.Equals(shaderName))
                    {
                        m_StringList.Add(relativePath);
                        break;
                    }
                }
				float f = ((float)i) / ((float)matPaths.Length);
				EditorUtility.DisplayProgressBar("查找中,请稍候","进度："+(int)(f*100)+"%",f);
            }
			EditorUtility.ClearProgressBar();
        }

		public void From2ETC1(string _texPath)//#LOGO LJJ :选中一张图，然后对这张图生成alpha
		{
			//int num = _texPath.LastIndexOf('.');                                                                        
			//string mid_Path = _texPath.Substring(num, 4);
			//string alp_Path = _texPath.Replace(mid_Path, "") + "_Alpha.png";
			//string a_mid_Path = GetRelativeAssetPath(alp_Path);
            string tex_pa = GetRelativeAssetPath(_texPath);
			//SetTextureReadable(a_mid_Path);
			//Texture2D alp = AssetDatabase.LoadAssetAtPath(a_mid_Path, typeof(Texture2D)) as Texture2D;
            Texture2D texPath = AssetDatabase.LoadAssetAtPath(tex_pa, typeof(Texture2D)) as Texture2D;
            OnPostporcessTexture(texPath);
			//OnPostporcessTexture(alp);

		}

		
		public void From2ETC1_back(string _texPath)
		{
			int num = _texPath.LastIndexOf('.');                                                                        
			string mid_Path = _texPath.Substring(num, 4);
			
			string alp_Path = _texPath.Replace(mid_Path, "") + "_Alpha.png";
			string a_mid_Path = GetRelativeAssetPath(alp_Path);
			SetTextureReadable(a_mid_Path);
			Texture2D alp = AssetDatabase.LoadAssetAtPath(a_mid_Path, typeof(Texture2D)) as Texture2D;
			
			OnPostporcessTexture(alp);
		}
		public void OnPostporcessTexture(Texture2D sourcetex)
		{
			UnityEngine.Debug.Log("in : " + sourcetex);
			try
			{
				TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(sourcetex));
				if (null == ti) return;
				ti.textureType = TextureImporterType.Advanced;
				ti.npotScale = TextureImporterNPOTScale.ToLarger;
				ti.compressionQuality = (int)TextureCompressionQuality.Best;
				//ti.wrapMode = TextureWrapMode.Repeat;
				//ti.filterMode = FilterMode.Bilinear;
				//ti.generateMipsInLinearSpace = false;
				//ti.mipmapEnabled = false;
				//ti.isReadable = false;
				ti.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC_RGB4, (int)TextureCompressionQuality.Best);
				
				char version = Application.unityVersion[0];
				if (version > '4')
					AssetDatabase.WriteImportSettingsIfDirty(AssetDatabase.GetAssetPath(sourcetex));
				else
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(sourcetex), ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
			}
			catch (System.InvalidCastException)
			{
				UnityEngine.Debug.LogError("SetTextureETC1FormatComplete() exception, file name:" + AssetDatabase.GetAssetPath(sourcetex));
			}
		}

        // 修改所有纹理格式: 在安卓平台为ETC1 ////////////////////
        public void ModifyAllTexture2ETC1InAndroidPlatform()
        {
            m_StringList.Clear();
            string[] allPaths = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            foreach (string path in allPaths)
            {
                if (!IsTextureFile(path)) continue;

                string relativePath = GetRelativeAssetPath(path);
                m_StringList.Add(relativePath);
                SetTextureETC1FormatComplete(relativePath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        public void Test01()
        {
            /*
            string matPath = "Assets/Assets/Cha/Materials/de_bird01.mat";
            string sdPath = "Assets\\Resources\\DefaultResourcesExtra\\Mobile\\Mobile-Diffuse.shader";
            Material mat = (Material)Resources.LoadAssetAtPath(matPath, typeof(Material));
            if (null == mat) return;
            Shader sd = (Shader)Resources.LoadAssetAtPath(sdPath, typeof(Shader));
            if (null == sd) return;
            mat.shader = sd;
             */
            string srcPath = "D:\\work\\svn\\jishuzhongxin\\ProjectService\\TextureForETC1\\Assets\\Resources\\Atlas\\NormalUI.png";
            string targetPath = "D:\\work\\svn\\jishuzhongxin\\ProjectService\\TextureForETC1\\Assets\\Resources\\Atlas\\NormalUI2.png";
            CreateScaleTexture(srcPath, targetPath, 0.4f);
            AssetDatabase.Refresh();
        }

        // 调用外部工具来缩小的纹理  ////////////////
        static void CreateScaleTexture(string from, string to, float fRate = 0.5f)
        {
            string caName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, "ca.exe");
            if (File.Exists(caName))
            {
                Process p = new Process();
                p.StartInfo.FileName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar , Path.DirectorySeparatorChar, "ca.exe");
               
                p.StartInfo.Arguments = string.Concat("\"2\" ", "\"", from, "\" \"", to, "\" ", "\"", fRate, "\"");
                
                p.Start();
                p.WaitForExit();
                UnityEngine.Debug.Log("Succeed to create scale texture for texture : " + from);
            }
        }


        #region 改善性需求
        // 修改所有纹理格式: 在安卓平台为ETC1 ////////////////////
        public IEnumerator ModifyTexture2ETC1InAndroidPlatformFromRoot(TextureCompressionQuality quality, TextureImporterNPOTScale POTStyle, string dir, string aparts_txt)
        {
            m_StringList.Clear();
            //string[] allPaths = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            List<string> myList = shaderDirectory.GetDirListFromRoot(dir, aparts_txt);
            FilterNguiFile( myList);
            string[] allPaths = myList.ToArray();

            int curIndex = 0;
            int allPicCnt = GetAllPicCnt(allPaths);

            foreach (string path in allPaths)
            {
                string[] allChildPaths = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                foreach (string childs in allChildPaths)
                {
                    if (!IsTextureFile(childs)) continue;
                   // UnityEngine.Debug.Log("-etc1->"+ childs);
                    string relativePath = GetRelativeAssetPath(childs);
                    m_StringList.Add(relativePath);
                    m_usedList.Add("\t/" + relativePath);
                    SetTextureETC1FormatCompleteEx(relativePath, quality, POTStyle);
                    float f = ((float)(curIndex++)) / ((float)allPicCnt);
                    EditorUtility.DisplayProgressBar("转换中,请稍候", "进度：" + (int)(f * 100) + "%", f);
                }
            }
            
            string _item1 = "数量: " + m_usedList.Count;
            string _item2 = "ModifyTexture2ETC1InAndroidPlatformFromRoot\r\n";
            m_usedList.Insert(0, _item1);
            WriteExFileInfoLog_Insert();
             m_usedList.Insert(0, _item2);
            string log_path = System.Environment.CurrentDirectory + "/ModifyMat_log.txt";
            WriteLog(log_path,m_usedList);


            EditorUtility.DisplayProgressBar("转换中,请稍候", "进度：" + "100%", 1f);
            for (int j = 0; j < 50; j++)
                yield return 0;
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        public IEnumerator ModifyTexture2ETC1InAndroidPlatformFromFile(TextureCompressionQuality quality, TextureImporterNPOTScale POTStyle, string dir_txt, string aparts_txt)
        {
            m_StringList.Clear();
            m_usedList.Clear();
            //string[] allPaths = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            
            List<string> myList = shaderDirectory.GetDirListFromFile(dir_txt, aparts_txt);
            FilterNguiFile(myList);
            string[] allPaths = myList.ToArray();

			int curIndex = 0;
            int allPicCnt = GetAllPicCnt(allPaths);

            foreach (string path in allPaths)
            {
                string[] allChildPaths = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                foreach (string childs in allChildPaths)
                {
                    if (!IsTextureFile(childs)) continue;
                    //UnityEngine.Debug.Log("-etc1->"+ childs);
                    string relativePath = GetRelativeAssetPath(childs);
                    m_StringList.Add(relativePath);
                    m_usedList.Add("\t/" + relativePath);
                    SetTextureETC1FormatCompleteEx(relativePath, quality, POTStyle);
					float f = ((float)(curIndex++)) / ((float)allPicCnt);
					EditorUtility.DisplayProgressBar("转换中,请稍候","进度："+(int)(f*100)+"%",f);
                }
			}
            string _item1 = "数量: " + m_usedList.Count;
            string _item2 = "ModifyTexture2ETC1InAndroidPlatformFromFile\r\n";
            
            m_usedList.Insert(0, _item1);
            WriteExFileInfoLog_Insert();
            m_usedList.Insert(0, _item2);
            string log_path = System.Environment.CurrentDirectory + "/ModifyMat_log.txt";
            WriteLog(log_path, m_usedList);

			EditorUtility.DisplayProgressBar("转换中,请稍候","进度："+"100%",1f);
			for(int j=0;j<50;j++)
				yield return 0;
			EditorUtility.ClearProgressBar ();
			AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);          
        }

        void FilterMyFile(string[] filter, List<string> myList)
        {
            if (filter == null || string.IsNullOrEmpty(filter[0]) || myList == null)
                return ;
            for (int i = 0; i < filter.Length; i++)
            {
                for (int j = myList.Count - 1; j >= 0; j--)
                {
                    if (myList[j].Contains(filter[i]))
                        myList.RemoveAt(j);
                }
            }
        }

        void FilterNguiFile( List<string> myList)
        {
            if (myList == null) return;
            string[] filter = { "NGUI" };
            FilterMyFile(filter, myList);
        }


		int GetAllPicCnt(string[] allPaths)
		{
			int picCnt = 0;
			foreach (string path in allPaths)
			{
				string[] allChildPaths = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
				foreach (string childs in allChildPaths)
				{
					if (!IsTextureFile(childs)) continue;
					picCnt++;
				}
			}
			return picCnt;
		}
       public void SetTextureETC1FormatCompleteEx(string relativePath, TextureCompressionQuality quality,TextureImporterNPOTScale pot_style)
        {
            try
            {

                int idx = relativePath.LastIndexOf('.');
                if (!relativePath.Substring(idx - 6, 6).Equals("_Alpha"))  //处理原图
                {

                    TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(relativePath);
                    if (null == ti) return;
                    ti.textureType = TextureImporterType.Advanced;
                    ti.npotScale = pot_style;
                    ti.compressionQuality = (int)quality;
                    //ti.isReadable = false;
                    ti.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC_RGB4, (int)quality);

                    char version = Application.unityVersion[0];
                    if (version > '4')
                        AssetDatabase.WriteImportSettingsIfDirty(relativePath);
                    else
                        AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                    //处理可能存在的alpha图
                    string relativePath_alpha = relativePath.Substring(0, idx) + m_AlphaPostfix;
                    TextureImporter ti_alpha = (TextureImporter)TextureImporter.GetAtPath(relativePath_alpha);
                    if (null == ti_alpha) return;
                    //string assetpath_alpha = ti_alpha.assetPath;
                    //ti_alpha.textureType = ti.textureType;
                    //ti_alpha.npotScale = ti.npotScale;
                    //ti_alpha.isReadable = ti.isReadable;
                    //ti_alpha.mipmapEnabled = ti.mipmapEnabled;
                    //ti_alpha.wrapMode = ti.wrapMode;
                    //ti_alpha.filterMode = ti.filterMode;
                    //ti_alpha.compressionQuality = ti.compressionQuality;
                    TextureImporterSettings st = new TextureImporterSettings();
                    ti.ReadTextureSettings(st);
                    ti_alpha.SetTextureSettings(st);
                   // ti_alpha.mipmapEnabled = false;
                    ti_alpha.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC_RGB4, (int)quality);

                    if (version > '4')
                        AssetDatabase.WriteImportSettingsIfDirty(relativePath_alpha);
                    else
                        AssetDatabase.ImportAsset(relativePath_alpha, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                }

            }
            catch (System.InvalidCastException)
            {
                UnityEngine.Debug.LogError("SetTextureETC1FormatComplete() exception, file name:" + relativePath);
            }

        }

        // 查找所有材质引用到的shader /////////////////////////
        public IEnumerator FindAllShadersEx(DelegateTextureTool func)
        {
            m_StringList.Clear();
            m_buildIn = 0;
            m_NGUI = 0;
            string[] matPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
			EditorUtility.DisplayProgressBar("打印中,请稍候","进度：0%",0f);
            for (int i = 0; i < matPaths.Length; ++i)
            {
                string path = matPaths[i];
                string relativePath = GetRelativeAssetPath(path);
                Material mat = (Material)AssetDatabase.LoadAssetAtPath(relativePath, typeof(Material));
                if (null == mat) 
					continue;
                Object[] roots = new Object[] { mat };
                Object[] dependObjs = EditorUtility.CollectDependencies(roots);
                foreach (Object obj in dependObjs)
                {
                    if (obj.GetType() == typeof(Shader))
                    {
                        string shaderPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                        AddShaderPathEx(shaderPath + "<---->" + obj.name);
                    }
                }
				float f = ((float)i) / ((float)matPaths.Length);
                func(f);
				EditorUtility.DisplayProgressBar("打印中,请稍候","进度："+(int)(f*100)+"%",f);
                yield return 0;
            }
            func(1f);
			EditorUtility.DisplayProgressBar("打印中,请稍候","进度：100%",1f);
			yield return 0;
			EditorUtility.ClearProgressBar();
        }

        void AddShaderPathEx(string path)
        {
            string str = m_StringList.Find(s => s.Equals(path));
            if (null == str)
            {
                if (path.Contains("unity_builtin_extra"))
                {
                    m_StringList.Insert(0, path);
                    m_buildIn ++;
                }
                else if(path.Contains("builtin_shaders-"))
                {
                    m_StringList.Insert(m_buildIn, path);
                    m_NGUI ++;
                }
                else if(path.Contains("NGUI/"))
                {
                    m_StringList.Insert(m_buildIn + m_NGUI, path);
                }
                else
                {
                    m_StringList.Add(path);
                }
            }
        }

        void WriteLog(string path,string[] context)
        {
            if (context == null || string.IsNullOrEmpty(path)) return;
            string _path = GetRightFormatPath(path);
            try
            {
                StreamWriter sw = new StreamWriter(_path, false, UTF8Encoding.UTF8);
                for (int i = 0; i < context.Length; i++)
                    sw.Write(context[i]+"\r\n");
                sw.Dispose();
                sw.Close();
            }
            catch(IOException ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }
        }

        void WriteLog(string path,List<string> infoList)
        {
            if (infoList == null) return;
            string [] strVec = infoList.ToArray();
            WriteLog(path, strVec);
        }

        void WriteExFileInfoLog_Insert()
        {
            List<string> inList = shaderDirectory.includedList;
            List<string> exList = shaderDirectory.excludedList; 
         
            for(int i = exList.Count - 1; i >= 0; i--)
            {
                m_usedList.Insert(0, "\t" + exList[i] );
            }
            m_usedList.Insert(0,"excluded File:");

            for (int i = inList.Count - 1; i >= 0; i--)
            {
                m_usedList.Insert(0, "\t" + inList[i] );
            }
            m_usedList.Insert(0, "included File:");
          
        }

        #endregion

    } // end class

} // end namespace
