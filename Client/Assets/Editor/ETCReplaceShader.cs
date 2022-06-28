using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ETCReplaceShader : EditorWindow
{

    static Dictionary<string, string> ShaderDic;
    public static List<string> m_StringList = new List<string>();

    //[MenuItem("EffortForETC1/Change Shader")]
    public static void StartReplace()
    {
        ShaderDic = shaderDirectory.GetShaderDictionary();
        m_StringList.Clear();
        string[] matPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
        for (int i = 0; i < matPaths.Length; ++i)
        {
            string path = matPaths[i];
            
            string relativePath = GetRelativeAssetPath(path);
            Material mat = (Material)Resources.LoadAssetAtPath(relativePath, typeof(Material));
            if (null == mat) continue;

            bool gdeFlag = false;
            string _shaderName = mat.shader.name;
            if (mat.shader != null)
            {
                //兼容之前两个粒子shader
                if (_shaderName.Equals("GDE/Effect/Particles-Additive"))
                {
                    //Debug.Log("1" + mat.shader.name);
                    gdeFlag = true;
                    _shaderName = "Particles/Additive";
                    //Debug.Log("2" +mat.shader.name);
                }

                if (_shaderName.Equals("GDE/Effect/Particles-Alpha Blended"))
                {
                    gdeFlag = true;
                    _shaderName = "Particles/Alpha Blended";
                }


                if (_shaderName.Contains("GDE/"))
                {
                    Debug.Log("已经替换完成");
                    continue;
                }
                //Debug.Log("22222222222222222" + mat.shader.name);

                string _shaderPath = GetShaderPath(_shaderName);
                if (_shaderPath == null)
                    continue;
                string newshaderpath = GetRelativeAssetPath(_shaderPath);
                //string newshaderpath = "GDE/" + mat.shader.name;
               // Debug.Log("22222222222222222222334343434" + newshaderpath);
                Shader replacingShader = Resources.LoadAssetAtPath(newshaderpath,typeof(Shader)) as Shader;

               
                mat.shader = replacingShader;
                //内置particle shader
                if (gdeFlag == false && (_shaderName.Equals("Particles/Alpha Blended") || _shaderName.Equals("Particles/Additive")))
                {
                    if (!mat.HasProperty("_TintColor"))
                        Debug.Log("no _TintColor.");
                    else 
                    {
                        mat.SetFloat("_AlphaCtrl",2);
                        mat.SetFloat("_DiffuseCtrl",2);
                    }
                }
            }

            /*Object[] roots = new Object[] { mat };
            Object[] dependObjs = EditorUtility.CollectDependencies(roots);
            //foreach (Object obj in dependObjs)
            for(int index = 0 ; index < dependObjs.Length; index++)
            {
                if (dependObjs[index].GetType() == typeof(Shader))
                {
                    string shaderPath = AssetDatabase.GetAssetPath(dependObjs[index].GetInstanceID());
                    string newshaderPath = GetRelativeAssetPath(GetShaderPath(dependObjs[index].name));
					Shader replacingShader = Resources.LoadAssetAtPath<Shader>(newshaderPath);
                    dependObjs[index] = replacingShader;
                    //AddShaderPath(shaderPath + "<---->" + obj.name);
                }
            }*/
			float f = ((float)(i)) / ((float)matPaths.Length);
			EditorUtility.DisplayProgressBar("替换中,请稍候","进度："+(int)(f*100)+"%",f);
		}
		EditorUtility.DisplayProgressBar("替换中,请稍候","进度：100%",1f);
		EditorUtility.ClearProgressBar ();
	}
    //粒子只用单张纹理，ＲＧＢ灰度图任一通道用作Alpha
    //说明：必需先执行操作：生成Alpha贴图 , 以下是美术可能的3种操作
    //      |传统shader|             |新shader|　　　　　｜处理｜
    //        RGBA           ->       RGBA + A     　　　shader不改
    //        RGB + 选项     ->       RGB  + NULL        切换shader (Gray)
    //                                RGB  + RGB         切换shader (Gray)
    //前提条件：假设美术在_MainTex_Alpha上仅设置RGB灰度图 (可能会带_Alpha字串）
    public static void OptimizeParticleShaderWithGray()
    {
        ShaderDic = shaderDirectory.GetShaderDictionary();
        m_StringList.Clear();
        string[] matPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);

        UnityEngine.Debug.Log("** 开始优化粒子shader:执行前请确保　已执行过 \"生成Alpha纹理\" 和 \"修改材质\" 这两步操作 **");
        UnityEngine.Debug.Log("--------------------------------优化粒子shader 开始-----------------------------------");
        string[] particleList = {  "Particles/Additive",
                                   "Particles/Alpha Blended"
                                };
        int errNum = 0;
        //遍历所有.mat
        for (int i = 0; i < matPaths.Length; ++i)
        {
            string path = matPaths[i];
            string relativePath = GetRelativeAssetPath(path);
            Material mat = (Material)AssetDatabase.LoadAssetAtPath(relativePath, typeof(Material));
            if (null == mat) continue;

            if (mat.shader != null)
            {
                string _shaderName = mat.shader.name;
                bool grayFlag = false;
                Texture2D mainTex = null;
                Texture2D mainTex_Alpha = null;
                for (int j = 0; j < particleList.Length; j++)
                {
                    if (_shaderName.Equals(particleList[j]))
                    {
                        mainTex = mat.GetTexture("_MainTex") as Texture2D;
                        mainTex_Alpha = mat.GetTexture("_MainTex_Alpha") as Texture2D;
                        string mainTex_path = null;
                        string mainTexAlpha_path = null;

                        //先过滤两种情况，xx.png + xx_Alpha.png 和 xx1.png + xx2.png
                        if (mainTex_Alpha != null && mainTex != null && !mainTex_Alpha.name.Equals(mainTex.name))
                        {
                            break;
                        }
                        else if (mainTex_Alpha == null && mainTex == null) //null mat
                        {

                            errNum++;
                            UnityEngine.Debug.Log(errNum + ": " + "材质 " + mat.name + " 是空材质");
                            break;
                        }
                        else if (mainTex != null)  //xx.png + ?  已生成alpha，但alpha纹理可能未赋值
                        {
                            mainTex_path = AssetDatabase.GetAssetPath(mainTex.GetInstanceID());
                            int idx = mainTex_path.LastIndexOf('.');
                            mainTexAlpha_path = mainTex_path.Substring(0, idx) + "_Alpha.png"; //工具生成，后缀是"_Alpha.png"
                            //Debug.Log("--->" + mainTexAlpha_path);
                            if (!File.Exists(mainTexAlpha_path)) //不存在alpha纹理
                            {
                                _shaderName = particleList[j] + " (Gray)";
                                //mat.SetTexture("_MainTex", mainTex_Alpha);
                                grayFlag = true;
                            }
                            else
                            {
                                if (mainTex_Alpha == null)
                                {
                                    errNum++;
                                    UnityEngine.Debug.Log(errNum + ": " + "材质" + mat.name + "　Alpha纹理未赋值");
                                    break;
                                }

                            }
                        }


                        if (grayFlag == true)
                        {
                            string _shaderPath = GetShaderPath(_shaderName);
                            if (_shaderPath == null)
                            {
                                UnityEngine.Debug.Log("内置shader字典未初始化或" + _shaderName + "文件名拼写有误");
                                break;
                            }
                            string newshaderpath = GetRelativeAssetPath(_shaderPath);
                            Shader replacingShader = AssetDatabase.LoadAssetAtPath(newshaderpath,typeof(Shader)) as Shader;
                            mat.shader = replacingShader;
                            break;

                        }

                    }
                }//遍历两个粒子shader名
            }
			float f = ((float)i) / ((float)matPaths.Length);
			EditorUtility.DisplayProgressBar("优化中,请稍候","进度："+(int)(f*100)+"%",f);
        }//遍历所有mat
		EditorUtility.DisplayProgressBar("优化中,请稍候","进度：100%",1f);
		EditorUtility.ClearProgressBar ();
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        if (errNum > 0)
        {
            UnityEngine.Debug.LogError("** 优化粒子shader完成" + "请修正上面 " + errNum + " 处错误后，再执行一次！ **");
        }
        UnityEngine.Debug.Log("--------------------------------优化粒子shader 结束-----------------------------------");

    }

    /*
    //只需一张灰度图
    //算法：MainTex是灰度图，则替换；MainTex未指定，MainTex_Alpha是灰度图，则替换；　其它情况,打印提示信息
    //前提条件：假设美术在_MainTex_Alpha上仅设置RGB灰度图 (可能会带_Alpha字串）
    public static void OptimizeParticleShaderWithGray()
    {
        ShaderDic = shaderDirectory.GetShaderDictionary();
        m_StringList.Clear();
        string[] matPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);

        UnityEngine.Debug.Log("** 开始优化粒子shader:执行前请确保　已执行过 \"生成Alpha纹理\" 和 \"修改材质\" 这两步操作 **");
        UnityEngine.Debug.Log("--------------------------------优化粒子shader 开始-----------------------------------");
        string[] particleList = {  "Particles/Additive",
                                   "Particles/Alpha Blended"
                                };
        int errNum = 0;
        //遍历所有.mat
        for (int i = 0; i < matPaths.Length; ++i)
        {
            string path = matPaths[i];
            string relativePath = GetRelativeAssetPath(path);
            Material mat = (Material)AssetDatabase.LoadAssetAtPath(relativePath, typeof(Material));
            if (null == mat) continue;

            if (mat.shader != null)
            {
                string _shaderName = mat.shader.name;
                bool grayFlag = false;
                Texture2D mainTex = null;
                Texture2D mainTex_Alpha = null;
                for (int j = 0; j < particleList.Length; j++)
                {
                    if (_shaderName.Equals(particleList[j]))
                    {
                        mainTex = mat.GetTexture("_MainTex") as Texture2D;
                        mainTex_Alpha = mat.GetTexture("_MainTex_Alpha") as Texture2D;

                        string mainTex_path = null;
                        string mainTexAlpha_path = null;

                        //检查
                        if (mainTex != null)
                        {
                            if (mainTex.name.Contains("_Alpha"))
                            {
                                errNum++;
                                UnityEngine.Debug.Log(errNum + ": " + "请修正材质" + mat.name + " 用到的纹理可能不需要alpha通道");
                                break;
                            }
                            else   //主纹理不含"_Alpha"
                            {
                                if (mainTex_Alpha != null)
                                {
                                    if (!mainTex_Alpha.name.Contains("_Alpha"))
                                    {
                                        mainTex_path = AssetDatabase.GetAssetPath(mainTex_Alpha.GetInstanceID());
                                        int idx = mainTex_path.LastIndexOf('.');
                                        mainTexAlpha_path = mainTex_path.Substring(0, idx) + "_Alpha.png"; //工具生成，后缀是"_Alpha.png"
                                        Debug.Log("--->" + mainTexAlpha_path);
                                        if (!File.Exists(mainTexAlpha_path)) //不存在alpha纹理
                                        {
                                            _shaderName = particleList[j] + " (Gray)";
                                            grayFlag = true;
                                        }
                                        else
                                        {
                                            errNum++;
                                            UnityEngine.Debug.Log(errNum + ": " + "请修正材质" + mat.name + "　Alpha纹理 请附上灰度图");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        continue;  //xx.png + xx_Alpha.png
                                    }
                                }
                                else
                                {
                                    mainTex_path = AssetDatabase.GetAssetPath(mainTex.GetInstanceID());
                                    int idx = mainTex_path.LastIndexOf('.');
                                    mainTexAlpha_path = mainTex_path.Substring(0, idx) + "_Alpha.png"; //工具生成，后缀是"_Alpha.png"
                                    Debug.Log("--->" + mainTexAlpha_path);
                                    if (!File.Exists(mainTexAlpha_path)) //不存在alpha纹理
                                    {
                                        _shaderName = particleList[j] + " (Gray)";
                                        grayFlag = true;
                                    }
                                    else
                                    {
                                        errNum++;
                                        UnityEngine.Debug.Log(errNum + ": " + "请修正材质" + mat.name + "　Alpha纹理未赋值");
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (mainTex_Alpha != null)
                            {
                                if (mainTex_Alpha.name.Contains("_Alpha"))
                                {
                                    errNum++;
                                    UnityEngine.Debug.Log(errNum + ": " + "请修正材质" + mat.name + " 主纹理未赋值，用到的纹理可能不需要alpha通道");
                                    break;
                                }
                                else  //Alpha纹理不含"_Alpha"
                                {
                                    mainTex_path = AssetDatabase.GetAssetPath(mainTex_Alpha.GetInstanceID());
                                    int idx = mainTex_path.LastIndexOf('.');
                                    mainTexAlpha_path = mainTex_path.Substring(0, idx) + "_Alpha.png"; //工具生成，后缀是"_Alpha.png"
                                    Debug.Log("--->" + mainTexAlpha_path);
                                    if (!File.Exists(mainTexAlpha_path)) //不存在alpha纹理
                                    {
                                        _shaderName = particleList[j] + " (Gray)";
                                        mat.SetTexture("_MainTex", mainTex_Alpha);
                                        grayFlag = true;
                                    }
                                    else
                                    {
                                        errNum++;
                                        UnityEngine.Debug.Log(errNum + ": " + "请修正材质" + mat.name + "　Alpha纹理可能不需要alpha通道");
                                        break;
                                    }


                                }
                            }
                            else
                            {
                                errNum++;
                                UnityEngine.Debug.Log(errNum + ": " + "请修正材质" + mat.name + " 纹理均未设置!");
                                break;
                            }
                        }

                        if (grayFlag == true)
                        {
                            string _shaderPath = GetShaderPath(_shaderName);
                            if (_shaderPath == null)
                            {
                                UnityEngine.Debug.Log("内置shader字典未初始化或" + _shaderName + "文件名拼写有误");
                                break;
                            }
                            string newshaderpath = GetRelativeAssetPath(_shaderPath);
                            Shader replacingShader = AssetDatabase.LoadAssetAtPath<Shader>(newshaderpath);
                            mat.shader = replacingShader;
                            break;

                        }
                    }
                }//遍历两个粒子shader名
            }
        }//遍历所有mat
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        if (errNum > 0)
        {
            UnityEngine.Debug.LogError("** 优化粒子shader完成" + "请修正上面 " + errNum + " 处错误后，再执行一次！ **");
        }
        UnityEngine.Debug.Log("--------------------------------优化粒子shader 结束-----------------------------------");

    }
	*/
	
    static string GetShaderPath(string shadername)
    {

       // Debug.Log( "=------------------------>>> " + shadername);
        //string str = "GDE/" + shadername;
        string shaderPath;
        //foreach (KeyValuePair<string, string> kvp in ShaderDic)
        //{
        //    Debug.Log(kvp.Key + "!!!" + kvp.Value);
        //}
        //return null;
        if (ShaderDic.TryGetValue(shadername, out shaderPath))
            return shaderPath;
        else
        {

            // Debug.Log(shadername +":  no value!");
            return null;
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
        //Debug.Log("aaaaaaaaaaaaaaaaaaaabbbbbbbbb "  + _path);
        return _path.Replace("\\", "/");
    }


}
