using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using MYEDITOR;


public class MaterialTextureEditor : EditorWindow
{

    //图片总数
    public static int picnum = 0;
    //已处理图片数
    public static int dealedpicnum = 0;
    string strForSingleTexture = "";
    string strForAlpha = "";
    string strForShader = "";
    float m_Percent = 0f;

    static ETC1MaterialTexture m_ETC1TexOperator = null;
    static ETCReplaceShader m_ETCReplacer = null;

    //若IncludedPath.txt包含目录，则以此为准,否则，使用默认RootPath
    public static string RootPath = Application.dataPath;  //+ "/file03";
    public static string DirPath = Application.dataPath + "/Editor/IncludedPath.txt"; 
    public static string ExcludedPath = Application.dataPath + "/Editor/ExcludedPath.txt";

    Vector2 m_Vec2ForScroll = new Vector2(80, 200);

    public MaterialTextureEditor()
    {
//        minSize = new UnityEngine.Vector2(620f, 550f);
//        maxSize = new UnityEngine.Vector2(621f, 551f);
        strForSingleTexture = "输入";
        strForAlpha = "输入";
        strForShader = "输入";
        m_ETC1TexOperator = new ETC1MaterialTexture();
        m_ETCReplacer = new ETCReplaceShader();
    }


    [MenuItem("EffortForETC1/Open Window")]
    static void OpenWindow()
    {
        MaterialTextureEditor window = (MaterialTextureEditor) EditorWindow.GetWindow(typeof(MaterialTextureEditor));
//        window.position = new Rect(300,300,860,550);
        
    }
    
    void OnGUI() 
    {

//        GUILayout.BeginArea(new Rect(10, 10, 600, 520));
        EditorGUILayout.BeginVertical();

        // 按钮第一排 ////////////////////////////
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("打印"))
        {
            EditorCoroutine.Start(m_ETC1TexOperator.FindAllShadersEx(new MYEDITOR.DelegateTextureTool(SetPercent)));
        }

        if (GUILayout.Button("1.生成Alpha纹理") )
        {

            Debug.Log("&& 开始分离纹理 &&");
            //EditorCoroutine.Start(m_ETC1TexOperator.GenAlphaTextureForAllFromRoot(new MYEDITOR.DelegateTextureTool(SetPercent),ExcludedPath));
            EditorCoroutine.Start(m_ETC1TexOperator.GenAlphaTextureForAllFromFile(new MYEDITOR.DelegateTextureTool(SetPercent), DirPath,ExcludedPath));
        }

        if (GUILayout.Button("2.替换内置Shader"))
        {
            //m_ETCReplacer.StartReplace();
            Debug.Log("开始替换shader");
            ETCReplaceShader.StartReplace();
            Debug.Log("替换完了，不信你再打印一次试试");
        }
		

        if (GUILayout.Button("3.修改材质"))
        {
            Debug.Log("&& 开始修改材质 &&");
            EditorCoroutine.Start(m_ETC1TexOperator.ModifyMaterialWhichHasFlag(new MYEDITOR.DelegateTextureTool(SetPercent)));
        }

        if (GUILayout.Button("[优化粒子Shader]"))
        {
            Debug.Log("[优化粒子shader]");
            ETCReplaceShader.OptimizeParticleShaderWithGray();
            Debug.Log("优化完成");
        }


        EditorGUILayout.EndHorizontal();

        // 按钮第二排 ////////////////////////////
        EditorGUILayout.BeginHorizontal();

        //if (GUILayout.Button("转换纹理ETC1:", GUILayout.Width(100)))
        //{

        //}
        //if (GUILayout.Button("Normal+ToNearest", GUILayout.Width(120))) //120
        //{
        //    Debug.Log("&& 开始修改纹理为ETC1格式(Normal+ToNearest) &&");
        //    m_ETC1TexOperator.ModifyAllTexture2ETC1InAndroidPlatformEx(TextureCompressionQuality.Normal,TextureImporterNPOTScale.ToNearest, DirPath, ExcludedPath);
        //    Debug.Log("&& 修改纹理为ETC1格式(Normal+ToNearest)结束 &&");
        //}

        if (GUILayout.Button("转换ETC1(Normal)")) //120
        {
            Debug.Log("&& 开始修改纹理为ETC1格式(Normal+ToLarger) &&");
			EditorCoroutine.Start(m_ETC1TexOperator.ModifyTexture2ETC1InAndroidPlatformFromFile(TextureCompressionQuality.Normal, TextureImporterNPOTScale.ToLarger, DirPath, ExcludedPath));
//            m_ETC1TexOperator.ModifyTexture2ETC1InAndroidPlatformFromFile(TextureCompressionQuality.Normal, TextureImporterNPOTScale.ToLarger, DirPath, ExcludedPath);
            Debug.Log("&& 修改纹理为ETC1格式(Normal+ToLarger)结束 &&");
        }
        //if (GUILayout.Button("Best+ToNearest", GUILayout.Width(120))) //120
        //{
        //    Debug.Log("&& 开始修改纹理为ETC1格式(Best+ToNearest) &&");
        //    m_ETC1TexOperator.ModifyAllTexture2ETC1InAndroidPlatformEx(TextureCompressionQuality.Best, TextureImporterNPOTScale.ToNearest, DirPath, ExcludedPath);
        //    Debug.Log("&& 修改纹理为ETC1格式(Best+ToNearest)结束 &&");
        //}
        if (GUILayout.Button("转换ETC1(Best)")) //120
        {
            Debug.Log("&& 开始修改纹理为ETC1格式(Best+ToLarger) &&");
			EditorCoroutine.Start(m_ETC1TexOperator.ModifyTexture2ETC1InAndroidPlatformFromFile(TextureCompressionQuality.Best, TextureImporterNPOTScale.ToLarger, DirPath, ExcludedPath));
//            m_ETC1TexOperator.ModifyTexture2ETC1InAndroidPlatformFromFile(TextureCompressionQuality.Best, TextureImporterNPOTScale.ToLarger, DirPath, ExcludedPath);
            Debug.Log("&& 修改纹理为ETC1格式(Best+ToLarger)结束 &&");
        }

        //if (GUILayout.Button("Test01", GUILayout.Width(120)))
        //{
        //    m_ETC1TexOperator.Test01();
        //}

        if (GUILayout.Button("....."))
        {

        }

        if (GUILayout.Button("*还原材质并删除alpha图"))
        {
            EditorCoroutine.Start(m_ETC1TexOperator.RestoreMaterialAndRemoveAlphaFromRoot(new MYEDITOR.DelegateTextureTool(SetPercent), RootPath));
        }

        if (GUILayout.Button("拷贝列表内容"))
        {
            TextEditor te = new TextEditor();
            string finalStr = "";
            List<string> strList = m_ETC1TexOperator.m_StringList;
            finalStr = "数量: " + strList.Count + "\n";
            foreach (string str in strList)
            {
                finalStr += str;
                finalStr += "\n";
            }
            te.content.text = finalStr;
            te.OnFocus();
            te.Copy();
        }

        EditorGUILayout.EndHorizontal();
		
        //第三排：yzm
        EditorGUILayout.BeginHorizontal();
        GUILayout.TextArea("动画压缩->", GUIStyle.none);
        if (GUILayout.Button("FBX动画压缩:精简", GUILayout.Width(120)))
        {
            Debug.Log("FBX动画压缩 - KeyframeReduction 开始");
            Modelhandle.FindAllFBXAnimFiles(ModelImporterAnimationCompression.KeyframeReduction);
            Debug.Log("FBX动画压缩 - 完成");
        }

        if (GUILayout.Button("FBX动画压缩:精简并压缩", GUILayout.Width(120)))
        {
            Debug.Log("FBX动画压缩 - KeyframeReductionAndCompression 开始");
            Modelhandle.FindAllFBXAnimFiles(ModelImporterAnimationCompression.KeyframeReductionAndCompression);
            Debug.Log("FBX动画压缩 - 完成");
        }
        GUILayout.TextArea("", GUIStyle.none);

        EditorGUILayout.EndHorizontal();
				
        EditorGUILayout.BeginHorizontal();

        //================================
        strForSingleTexture = GUILayout.TextArea(strForSingleTexture, GUIStyle.none);
        //Debug.Log("&& 图片名字:"+ str);
		if (GUILayout.Button("粘贴",GUILayout.Width(100)))
        {
            TextEditor te = new TextEditor();
            te.Paste();
            strForSingleTexture = te.content.text;
        }
		if (GUILayout.Button("平铺rgb+alpha",GUILayout.Width(100)) && strForSingleTexture.Length > 0)
        {
            m_ETC1TexOperator.SeperateRGBAandMerge(strForSingleTexture);
        }
        EditorGUILayout.EndHorizontal();

        //================================
        EditorGUILayout.BeginHorizontal();
        strForAlpha = GUILayout.TextArea(strForAlpha, GUIStyle.none);
        //Debug.Log("&& 图片名字:"+ str);
		if (GUILayout.Button("粘贴",GUILayout.Width(100)))
        {
            TextEditor te = new TextEditor();
            te.Paste();
            strForAlpha = te.content.text;
        }
		if (GUILayout.Button("给图片加alpha通道",GUILayout.Width(100)) && strForAlpha.Length > 0)
        {
            m_ETC1TexOperator.AddAlpha2Picture(strForAlpha);
        }
        EditorGUILayout.EndHorizontal();

        //================================
        EditorGUILayout.BeginHorizontal();
        strForShader = GUILayout.TextArea(strForShader, GUIStyle.none);
        //Debug.Log("&& 图片名字:"+ str);
		if (GUILayout.Button("粘贴",GUILayout.Width(100)))
        {
            TextEditor te = new TextEditor();
            te.Paste();
            strForShader = te.content.text;
        }
		if (GUILayout.Button("查找shader引用",GUILayout.Width(100)) && strForShader.Length > 0)
        {
            m_ETC1TexOperator.FindMaterialsByShaderName(strForShader);
        }
        EditorGUILayout.EndHorizontal();

        //================================
        //显示所有路径
        EditorGUILayout.BeginVertical();
        m_Vec2ForScroll = EditorGUILayout.BeginScrollView(m_Vec2ForScroll);

        List<string> strList2 = m_ETC1TexOperator.m_StringList;
        GUILayout.Label("数量: "+ strList2.Count);
        foreach (string str in strList2)
        {
            GUILayout.Label(str);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
//        string info = "" + (int)(m_Percent * 100f) + "/100";
//        EditorGUILayout.LabelField(info);

        //GUILayout.HorizontalScrollbar(1f, m_Percent, 1f, 0f, GUI.skin.GetStyle("horizontalscrollbar"));
        //EditorGUILayout.LabelField("此工具只能将项目切换到pc平台时使用, 其它状态下不保证正确性");
        EditorGUILayout.EndVertical();
//        GUILayout.EndArea();
    }

    public void SetPercent(float f)
    {
        m_Percent = f;
    }

    void OnDestroy()
    {
        dealedpicnum = 0;
    }


    //已处理图片数加1
    public static void DealOne()
    {
        dealedpicnum++;
    }
    //已搜索图片数加1
    public static void SearchOne()
    {
        
    }

    public void PrintOne(string s)
    {
        
    }

}
