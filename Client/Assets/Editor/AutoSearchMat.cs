using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using MYEDITOR;
//[InitializeOnLoad]

public class AutoSearchMat : Editor
{
    static string old_TexName;
    static string current_TexName;

    //static AutoSearchMat()
    //{
    //    EditorApplication.update += Update;
    //}

    static IEnumerator Waits()
    {
        yield return new WaitForSeconds(3);
    }

  

    //[MenuItem("Assets/检查Alpha是否有意义(RGBA)")]
    static void CheckBW(Texture2D tex,int num)
    {
        
        //string selectPath = "Assets";
        //foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        //{
        //    selectPath = AssetDatabase.GetAssetPath(obj);
        //    if (!string.IsNullOrEmpty(selectPath) && File.Exists(selectPath))
        //    {
        //        Debug.Log("--->" + selectPath);
        //    }

        //    Texture2D tex = AssetDatabase.LoadAssetAtPath(selectPath, typeof(Texture2D)) as Texture2D;
        if(num == 0)
        {
            if (current_TexName == old_TexName)
            {
                bool isFrag = false;
                float cur = 0;
                float last = 0; //old

                for (int i = 0; i < tex.width - 1; i++)
                {

                    for (int j = 0; j < tex.height - 1; j++)
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
                                UnityEngine.Debug.Log("Alpha贴图有意义,是否需要删除Alpha通道？");
                                CreateFile(tex, "CutAlpha_Textures.txt");
                                current_TexName = old_TexName;
                                isFrag = true;
                                break;
                            }


                            else if (i == tex.width - 1 && j == tex.height - 1)
                            {
                                if (Mathf.Abs(last - 1) < 0.00001)
                                {
                                    UnityEngine.Debug.Log("Alpha贴图是白色的,是否需要删除Alpha通道？");
                                    CreateFile(tex, "CutAlpha_Textures.txt");
                                    current_TexName = old_TexName;
                                }
                                else
                                {
                                    UnityEngine.Debug.Log("Alpha贴图是黑色的,是否需要删除Alpha通道？");
                                    current_TexName = old_TexName;
                                    CreateFile(tex, "CutAlpha_Textures.txt");
                                }
                            }
                        }
                    }
                    if (isFrag == true)
                        break;
                }
            }
        }
        if (num == 1)
        {
            if (current_TexName == old_TexName)
            {
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
                                UnityEngine.Debug.Log("检测,Alpha贴图有意义！");
                                isFrag = true;
                                current_TexName = old_TexName;
                                break;
                            }


                            else if (i == tex.width - 1 && j == tex.height - 1)
                            {
                                if (Mathf.Abs(last - 1) < 0.00001)
                                {
                                    UnityEngine.Debug.Log("Alpha贴图是白色的,请确认是否出现贴图问题，或者使用了错误的shader！");
                                    current_TexName = old_TexName;
                                }
                                else
                                {
                                    UnityEngine.Debug.Log("Alpha贴图是黑色的,请确认是否出现贴图问题，或者使用了错误的shader！");
                                    current_TexName = old_TexName;
                                }
                            }
                        }
                    }
                    if (isFrag == true)
                        break;
                }
            }
        }
    }

    //[MenuItem("Assets/贴图自动检测(材质球)")]
    static void Update()
    {
        //DeleteFile("CutAlpha_Textures.txt");
        //ETC1MaterialTexture emt = new ETC1MaterialTexture();
        string selectPath = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            selectPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(selectPath) && File.Exists(selectPath))
            {
                Material thisMat = AssetDatabase.LoadAssetAtPath(selectPath, typeof(Material)) as Material;
                //Debug.Log(thisMat.name);
                string[] allTex =  FindAllTex(thisMat);
                Shader thisShader = thisMat.shader;     //记录下该物体当前shader
                string shaderPath = AssetDatabase.GetAssetPath(thisShader);
                string shaderName = Read(shaderPath);
                Texture2D mainTex = thisMat.GetTexture("_MainTex") as Texture2D;
                old_TexName = mainTex.name;
                current_TexName = mainTex.name;

                if (shaderName.Equals("Mobile/Diffuse") || shaderName.Equals("Legacy Shaders/Lightmapped/Diffuse")
                    || shaderName.Equals("Self-Illumin/Diffuse") || shaderName.Equals("Diffuse")
                    || shaderName.Equals("Diffuse Detail") || shaderName.Equals("Legacy Shaders/Diffuse Fast")
                    || shaderName.Equals("Reflective/Diffuse"))
                {
                    Debug.Log("该图不应该有Alpha通道");
                    CheckBW(mainTex,0);
                }
                else
                {
                    Debug.Log("该图应该有Alpha通道");
                    CheckBW(mainTex,1);
                }
                //foreach (string tex in allTex)
                //{
                //    Debug.Log(tex);
                    
                //}
            }

        }
    }

    static string[] FindAllTex(Material mat)
    {
        List<string> results = new List<string>();
        Object[] root = new Object [] { mat };
        Object[] dependObjs = EditorUtility.CollectDependencies(root);
        foreach (Object obj in dependObjs)
        {
            if (obj.GetType() == typeof(Texture2D))
            {
                string texpath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                results.Add(texpath);
            }
        }
        return results.ToArray();
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
    }

    static void CreateFile(Texture2D texture, string name)         ////CutAlpha_Textures.txt
    {
        Texture2D newTex = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, true);
        int w = texture.width;
        for (int i = 0; i < texture.height; i++)
        {
            for (int j = 0; j < texture.width - 1; j++)
            {
                //Color color = texture.GetPixel(i, j);
                //color.a = 1;
                //newTex.SetPixel(i, j, color);


                Color color = texture.GetPixel(i, j);
                Color rgbColor = Color.white;
                rgbColor.r = color.r;
                rgbColor.g = color.g;
                rgbColor.b = color.b;
                //Debug.Log(rgbColor);
            }
        }
        newTex.Apply();

        byte[] finalData = texture.EncodeToPNG();

        string _texPath = AssetDatabase.GetAssetPath(texture);
        int num = _texPath.LastIndexOf('.');                                                                        //#LOGO LJJ :选中一张图，然后对这张图生成alpha
        string mid_Path = _texPath.Substring(num, 4);

        File.WriteAllBytes(_texPath.Replace(mid_Path, "") + "_Test.png", finalData);
        AssetDatabase.Refresh(ImportAssetOptions.Default);
        //StreamWriter sw;
        //FileInfo fi = new FileInfo(Application.dataPath + "//" + name);
        //if (!fi.Exists)
        //{
        //    sw = fi.CreateText();
        //}
        //else
        //    sw = fi.AppendText();
        //sw.WriteLine(Application.dataPath.Remove(Application.dataPath.Length-6,6) + AssetDatabase.GetAssetPath(texture));
        //sw.Close();
        //sw.Dispose();
    }

  

    //static void DeleteFile(Texture2D tex,string name)
    //{
    //    File.Delete(AssetDatabase.GetAssetPath(tex) + "//" + name);
    //}
}
