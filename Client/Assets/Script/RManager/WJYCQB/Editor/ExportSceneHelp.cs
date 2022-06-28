using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using Object = UnityEngine.Object;
using Asset;
using System.IO;
using System.Reflection;
using Common;
/*
 * 资源名称中不能包含 * & 关键符号
 */
public class ExportSceneHelp : EditorWindow
{
    void OnHierarchyChange()
    {
        writeExportSceneSetting();
    }
    void OnDestroy()
    {
        writeExportSceneSetting();
    }
    public static bool IsDebug = false, RemoveNotExistsFile = false;
    static float width = 800, height = 800;
    public static int ForceExport = -1;
    static void showWindow()
    {
        window = EditorWindow.GetWindow(typeof(ExportSceneHelp), false, "Scene Setting", true) as ExportSceneHelp;
        window.position = new Rect((Screen.currentResolution.width - width) / 2, (Screen.currentResolution.height - height) / 2, width, height);
    }
    void Update()
    {
        if (exportSceneSettingList == null)
        {
            readExportSceneSetting();
            showWindow();
        }
        if (testIndex > -1)
        {
            if (test)
            {
                GameObject g = GameObject.Find("LoadScene");
                if (g != null)
                {
                    string sceneName = exportSceneSettingList[testIndex].SceneName;
                    sceneName = sceneName.Substring(sceneName.LastIndexOf("/") + 1);
                    sceneName = sceneName.Substring(0, sceneName.IndexOf("."));
                    g.SendMessage("ChangeScene", sceneName);
                }
            }
            testIndex = -1;
        }
        if (ForceExport > -1)
        {
            exportList.Clear();
            string sceneName = exportSceneSettingList[ForceExport].SceneName;
            exportSceneSettingList[ForceExport].State = 0;
            try
            {
                ExportSceneByName(sceneName);
                exportSceneSettingList[ForceExport].State = 2;
            }
            catch (Exception ex)
            {
                exportSceneSettingList[ForceExport].State = 1;
                DEBUG.LogError(ex.ToString());
            }
            ForceExport = -1;
            WriteExportFileList();
        }
        if (exportPress)
        {
            exportList.Clear();
            exportPress = false;
            readMd5ListState = true;
            string sceneName = "";
            for (int i = 0; i < exportSceneSettingList.Length - 1; i++)
            {
                sceneName = exportSceneSettingList[i].SceneName;
                exportSceneSettingList[i].State = 0;
                if (exportSceneSettingList[i].Enable)
                {
                    try
                    {
                        ExportSceneByName(sceneName);
                        exportSceneSettingList[i].State = 2;
                    }
                    catch (Exception ex)
                    {
                        exportSceneSettingList[i].State = 1;
                        DEBUG.LogError(ex.ToString());
                        UnityEditor.EditorUtility.ClearProgressBar();
                    }
                }
            }
            WriteExportFileList();
        }
    }
    void OnGUI()
    {
        if (exportSceneSettingList == null || exportSceneSettingList.Length < 1) return;
        scroll = EditorGUILayout.BeginScrollView(scroll, false, false, GUILayout.ExpandWidth(true));
        for (int i = 0; i < exportSceneSettingList.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Scene " + i + ":", GUILayout.Width(80));
            exportSceneSettingList[i].SceneName = GUILayout.TextField(exportSceneSettingList[i].SceneName, GUILayout.Width(window.position.width - 310));
            exportSceneSettingList[i].Enable = GUILayout.Toggle(exportSceneSettingList[i].Enable, "", GUILayout.Width(15));
            switch (exportSceneSettingList[i].State)
            {
                case 0:
                    {
                        GUILayout.Label("就绪", GUILayout.Width(25));
                        break;
                    }
                case 1:
                    {
                        GUILayout.Label("失败", GUILayout.Width(25));
                        break;
                    }
                case 2:
                    {
                        GUILayout.Label("成功", GUILayout.Width(25));
                        break;
                    }
            }
            if (i == exportSceneSettingList.Length - 1)
            {
                if (GUILayout.Button("添加", GUILayout.Width(40)))
                {
                    if (!string.IsNullOrEmpty(exportSceneSettingList[i].SceneName))
                    {
                        List<AssetItemState> tmpList = new List<AssetItemState>();
                        foreach (AssetItemState m in exportSceneSettingList)
                        {
                            tmpList.Add(m);
                        }
                        tmpList.Add(new AssetItemState("", "1", "0"));
                        exportSceneSettingList = tmpList.ToArray();
                        break;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("删除", GUILayout.Width(40)))
                {
                    List<AssetItemState> tmpList = new List<AssetItemState>();
                    foreach (AssetItemState m in exportSceneSettingList)
                    {
                        tmpList.Add(m);
                    }
                    tmpList.RemoveAt(i);
                    exportSceneSettingList = tmpList.ToArray();
                    break;
                }
                if (GUILayout.Button("强制导出", GUILayout.Width(60)))
                {
                    defaultState();
                    ForceExport = i;
                }
                if (GUILayout.Button(test ? "测试" : "--", GUILayout.Width(40)))
                {
                    if (test) testIndex = i;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        findPath = EditorGUILayout.TextField("检索路径:", findPath);
        if (GUILayout.Button("智能检索"))
        {
            defaultState();
            string findFolder = System.Environment.CurrentDirectory + Path.DirectorySeparatorChar;
            DirectoryInfo di = new DirectoryInfo(findFolder + "Assets" + Path.DirectorySeparatorChar + findPath);
            List<string> flist = new List<string>();
            FileInfo[] fi = di.GetFiles("*.unity", SearchOption.AllDirectories);
            string tmpName = "";
            foreach (FileInfo guid in fi)
            {
                tmpName = guid.FullName.Replace(findFolder, "");
                tmpName = tmpName.Replace(Path.DirectorySeparatorChar, '/');
                bool find = false;
                foreach (AssetItemState c in exportSceneSettingList)
                {
                    if (string.Compare(c.SceneName, tmpName) == 0)
                    {
                        find = true;
                        break;
                    }
                }
                if (!find) flist.Add(tmpName);
            }
            List<AssetItemState> tmpList = new List<AssetItemState>();
            foreach (AssetItemState m in exportSceneSettingList)
            {
                if (!string.IsNullOrEmpty(m.SceneName)) tmpList.Add(m);
            }
            foreach (string m in flist)
            {
                tmpList.Add(new AssetItemState(m, "1", "0"));
            }
            tmpList.Add(new AssetItemState("", "1", "0"));
            exportSceneSettingList = tmpList.ToArray();
        }
        if (GUILayout.Button("全选(是/否)"))
        {
            if (exportSceneSettingList.Length > 1)
            {
                bool check = !exportSceneSettingList[0].Enable;
                for (int i = 0; i < exportSceneSettingList.Length; i++)
                {
                    if (i != exportSceneSettingList.Length - 1) exportSceneSettingList[i].Enable = check;
                }
            }
        }
        if (GUILayout.Button("批量导出"))
        {
            defaultState();
            exportPress = true;
        }
        if (GUILayout.Button(test ? "退出测试" : "进入测试"))
        {
            if (test)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            else
            {
                GameObject g = GameObject.Find("TestLoadScene");
                if (!UnityEditor.EditorApplication.isPlaying || g == null)
                {
                    UnityEditor.EditorApplication.OpenScene("Assets/TestLoadScene.unity");
                    UnityEditor.EditorApplication.isPlaying = true;
                }
            }
            test = !test;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        samePath = EditorGUILayout.TextField("同步标签:", samePath);
        if (GUILayout.Button("设置同步资源(资源项目)") && !string.IsNullOrEmpty(samePath))
        {
            string storePath = setStorePath(samePath);
            copyFile(unityPath2PhysicsPath("Assets/Resources/Shader"), string.Concat(storePath, Path.DirectorySeparatorChar, "Shader"), null);
            copyFile(unityPath2PhysicsPath("Assets/Resources/Material"), string.Concat(storePath, Path.DirectorySeparatorChar, "Material"), null);
        }
        if (GUILayout.Button("获取同步资源(主项目)") && !string.IsNullOrEmpty(samePath))
        {
            List<string> shaderList = new List<string>();
            string storePath = setStorePath(samePath);
            copyFile(string.Concat(storePath, Path.DirectorySeparatorChar, "Shader"), unityPath2PhysicsPath("Assets/Resources/Shader"), shaderList);
            copyFile(string.Concat(storePath, Path.DirectorySeparatorChar, "Material"), unityPath2PhysicsPath("Assets/Resources/Material"), null);
            setShader(shaderList);
        }
        EditorGUILayout.EndHorizontal();
    }
    string setStorePath(string tmpSamePath)
    {
        string result = string.Concat(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.DirectorySeparatorChar, "May");
        if (!Directory.Exists(result)) Directory.CreateDirectory(result);
        result = string.Concat(result, Path.DirectorySeparatorChar, tmpSamePath);
        if (!Directory.Exists(result)) Directory.CreateDirectory(result);
        return result;
    }
    void setShader(List<string> copyList)
    {
        AssetDatabase.Refresh();
        FileInfo tmp = null;
        string objName = "";
        for (int i = 0; i < copyList.Count; i++)
        {
            tmp = new FileInfo(copyList[i]);
            objName = tmp.Name.Substring(0, tmp.Name.IndexOf("."));
            Shader sh1 = Resources.Load(string.Concat("Shader/", objName)) as Shader;
            Material mat1 = Resources.Load(string.Concat("Material/", objName)) as Material;
            if (sh1 != null && mat1 != null) mat1.shader = sh1;
        }
    }
    void copyFile(string from, string to, List<string> copyFileList)
    {
        if (!Directory.Exists(from)) return;
        if (!Directory.Exists(to)) Directory.CreateDirectory(to);
        string newFileName = "";
        DirectoryInfo di = new DirectoryInfo(from);
        FileInfo[] fis = di.GetFiles();
        FileInfo tmp = null;
        for (int i = 0; i < fis.Length; i++)
        {
            tmp = fis[i];
            if (tmp.Name.EndsWith(".meta")) continue;
            newFileName = string.Concat(to, Path.DirectorySeparatorChar, tmp.Name);
            if (tmp.Name.EndsWith(".txt"))
            {
                if (File.Exists(newFileName))
                {
                    copyList(tmp.FullName, newFileName);
                }
                else tmp.CopyTo(newFileName);
            }
            else
            {
                if (!File.Exists(newFileName)) tmp.CopyTo(newFileName);
                if (copyFileList != null) copyFileList.Add(newFileName);
            }
        }
    }
    void copyList(string from, string to)
    {
        List<string> sList = new List<string>();
        Dictionary<string, byte> chkList = new Dictionary<string, byte>();
        List<string> tList = new List<string>();
        using (StreamReader sr = new StreamReader(from, System.Text.Encoding.UTF8))
        {
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                sList.Add(line);
            }
        }
        using (StreamReader sr = new StreamReader(to, System.Text.Encoding.UTF8))
        {
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                chkList[line] = 0;
            }
        }
        string tmp = null;
        for (int i = 0; i < sList.Count; i++)
        {
            tmp = sList[i];
            if (!chkList.ContainsKey(tmp))
            {
                chkList[tmp] = 0;
            }
        }
        using (StreamWriter sr = new StreamWriter(to, false, System.Text.Encoding.UTF8))
        {
            foreach (KeyValuePair<string, byte> m in chkList)
            {
                sr.WriteLine(m.Key);
            }
        }
    }
    void defaultState()
    {
        if (UnityEditor.EditorApplication.isPlaying)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        test = false;
    }
    bool test = false;
    Vector2 scroll = Vector2.zero;
    int testIndex = -1;
    static string findPath = "", samePath = "";
    static bool exportPress = false;
    static ExportSceneHelp window = null;
    [MenuItem("AssetBundle/Scene/Export Panel")]
    static void ExportScene()
    {
        showWindow();
        readExportSceneSetting();
        for (int i = 0; i < exportSceneSettingList.Length; i++)
        {
            exportSceneSettingList[i].State = 0;
        }

    }
    [MenuItem("AssetBundle/Scene/Export Current")]
    static void ExportCurrentScene()
    {
        readMd5ListState = true;
        ForceExport = 0;
        ExportSceneByName(UnityEditor.EditorApplication.currentScene);
        ForceExport = -1;
    }
    static bool createExportFolderState = true;
    public static string CreateExportFolder()
    {
        string folder = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Export", Path.DirectorySeparatorChar);
        if (createExportFolderState)
        {
            createExportFolderState = false;
            if (!System.IO.Directory.Exists(folder)) AssetDatabase.CreateFolder("Assets", "Export");
            string streamFolder = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "StreamingAssets", Path.DirectorySeparatorChar);
            if (!System.IO.Directory.Exists(streamFolder)) AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            string tmpFolder = string.Concat(streamFolder, "Animations");
            if (!System.IO.Directory.Exists(tmpFolder)) AssetDatabase.CreateFolder("Assets/StreamingAssets", "Animations");
            tmpFolder = string.Concat(streamFolder, "Config");
            if (!System.IO.Directory.Exists(tmpFolder)) AssetDatabase.CreateFolder("Assets/StreamingAssets", "Config");
            tmpFolder = string.Concat(streamFolder, "Images");
            if (!System.IO.Directory.Exists(tmpFolder)) AssetDatabase.CreateFolder("Assets/StreamingAssets", "Images");
            tmpFolder = string.Concat(streamFolder, "Meshs");
            if (!System.IO.Directory.Exists(tmpFolder)) AssetDatabase.CreateFolder("Assets/StreamingAssets", "Meshs");
            tmpFolder = string.Concat(streamFolder, "Prefabs");
            if (!System.IO.Directory.Exists(tmpFolder)) AssetDatabase.CreateFolder("Assets/StreamingAssets", "Prefabs");
            tmpFolder = string.Concat(streamFolder, "Scenes");
            if (!System.IO.Directory.Exists(tmpFolder)) AssetDatabase.CreateFolder("Assets/StreamingAssets", "Scenes");
            tmpFolder = string.Concat(streamFolder, "Sounds");
            if (!System.IO.Directory.Exists(tmpFolder)) AssetDatabase.CreateFolder("Assets/StreamingAssets", "Sounds");
        }
        return folder;
    }
    public static void ExportSceneByName(string exportSceneName)
    {
        string currentScene = UnityEditor.EditorApplication.currentScene;
        if (!UnityEditor.EditorApplication.OpenScene(exportSceneName)) return;
        UnityEditor.EditorApplication.SaveAssets();
        lastScene = exportSceneName;
        if (lastScene.IndexOf("_export.unity") > -1) return;
        string sceneName = lastScene;
        sceneName = sceneName.Substring(sceneName.LastIndexOf("/") + 1);
        sceneName = sceneName.Substring(0, sceneName.IndexOf("."));
        currentSceneName = sceneName + "_export";
        CreateExportFolder();
        //exportDataPath = folder + Path.DirectorySeparatorChar + sceneName + "_export_data";
        //if (!System.IO.Directory.Exists(exportDataPath)) AssetDatabase.CreateFolder("Assets/Export", sceneName + "_export_data");
        //dataPath = "Assets/Export/" + sceneName + "_export_data";
        sceneName = "Assets/Export/" + sceneName + "_export.unity";
        UnityEditor.EditorApplication.SaveScene(sceneName, false);
        UnityEditor.EditorApplication.OpenScene(sceneName);

        ReadMd5List();
        exportCurrentScene();
        UnityEditor.EditorApplication.OpenScene(lastScene);
        WriteMd5List();
        ExportSceneHelp.ClearTmpFile();

        UnityEditor.EditorApplication.OpenScene(currentScene);
    }
    public const string ExportPath = "Assets/StreamingAssets";
    static void exportCurrentScene()
    {
        ExportItem sceneItem = new ExportItem(lastScene, "Scenes", lastScene);
        GameObject skybox = new GameObject(sceneItem.ObjName + "_export_skybox");
        SkinnedMeshRenderer sk = skybox.AddComponent<SkinnedMeshRenderer>();
        sk.sharedMaterials = new Material[] { RenderSettings.skybox };
        sk.enabled = false;
        if (RenderSettings.skybox != null) RenderSettings.skybox = null;
        AssetRecordRoot root = skybox.AddComponent<AssetRecordRoot>();

        GameObject[] objs = GetAllGameObject();//GameObject.FindObjectsOfType(typeof(GameObject));
        UnityEditor.EditorUtility.DisplayProgressBar("导出 " + lastScene + " 步骤 1/3", "正在分析场景 0/" + objs.Length, 0);
        int index = 0;
        Dictionary<string, ExportItem> olist = new Dictionary<string, ExportItem>();
        List<GameObject> gList = new List<GameObject>();
        List<AssetRecord> rList = new List<AssetRecord>();
        List<MonoBehaviour> sList = new List<MonoBehaviour>();
        ExportItem.CollectStaticAssets(true);
        foreach (GameObject o in objs)
        {
            index++;
            ExportItem.ExportObject2Item(o, olist, gList, rList, sList);
            UnityEditor.EditorUtility.DisplayProgressBar("导出 " + lastScene + " 步骤 1/3", "正在分析场景 " + index + "/" + objs.Length, 0);
        }
        root.Type = "s";
        root.SetVersion();
        root.Objects = gList.ToArray();
        root.Records = rList.ToArray();
        root.DeferredExecScripts = sList.ToArray();
        ExportItem.CollectStaticAssets(false);

        UnityEditor.EditorUtility.DisplayProgressBar("导出 " + lastScene + " 步骤 2/3", "正在导出物件 0/" + olist.Count, 0);
        index = 0;
        foreach (KeyValuePair<string, ExportItem> v in olist)
        {
            index++;
            UnityEditor.EditorUtility.DisplayProgressBar("导出 " + lastScene + " 步骤 2/3", "正在导出物件 " + index + "/" + olist.Count, 0);

            try
            {
                if (CheckMd5State(v.Value) || ForceExport > -1)
                    BuildPipeline.BuildAssetBundle(v.Value.Obj, null, ExportPath + "/" + v.Value.ExportPath, BuildAssetBundleOptions.CollectDependencies, EditorUserBuildSettings.activeBuildTarget);
            }
            catch
            {
                DEBUG.LogError(" >>> " + v.Value.Obj.name);
            }
        }

        UnityEditor.EditorApplication.SaveScene(UnityEditor.EditorApplication.currentScene);
        UnityEditor.EditorUtility.DisplayProgressBar("导出 " + lastScene + " 步骤 3/3", "正在导出场景", 0);
        if (CheckMd5State(sceneItem) || ForceExport > -1) BuildPipeline.BuildStreamedSceneAssetBundle(new string[] { UnityEditor.EditorApplication.currentScene }, ExportPath + "/" + sceneItem.ExportPath, EditorUserBuildSettings.activeBuildTarget);
        if (!IsDebug) AssetDatabase.DeleteAsset(UnityEditor.EditorApplication.currentScene);
        UnityEditor.EditorUtility.ClearProgressBar();
    }
    public static Dictionary<string, string> Md5List = new Dictionary<string, string>();
    public static Dictionary<string, string> FileList = new Dictionary<string, string>();
    static List<string> exportList = new List<string>();
    static AssetItemState[] exportSceneSettingList = null;
    static string dataPath = "", lastScene = "", currentSceneName = "", exportDataPath = "", tmpMd5 = "";
    private static bool readMd5ListState = true;
    public static bool CheckMd5State(ExportItem item)
    {
        bool needUpdate = true;
        //DEBUG.Log(item.FileName + "->-------------------------");
        var selfMD5 = "";

        if (!DefaultAsset(item.Key))
        {
            var physicsPath = unityPath2PhysicsPath(item.RealAssetUnityPath);
            
            try
            {
                selfMD5 = MD5Help.GetMD5(physicsPath);
            } catch
            {
                DEBUG.LogError("Asset export: error to find asset at " + physicsPath);
                return true;
            }

            tmpMd5 = selfMD5;

            if (item.DependedMaterial != null && item.DependedMaterial.mainTexture != null)
            {
                string texturePath = AssetDatabase.GetAssetPath(item.DependedMaterial.mainTexture);
                if (!DefaultAsset(texturePath))
                {
                    texturePath = unityPath2PhysicsPath(texturePath);
                    string textureMD5 = "";
                    try
                    {
                        textureMD5 = MD5Help.GetMD5(texturePath);
                    } catch
                    {
                        DEBUG.LogError("Asset export: error to find materal texture asset at " + texturePath);
                        return true;
                    }

                    tmpMd5 = string.Concat(selfMD5, "_", textureMD5);
                }
            
            /*
            if (string.Compare(item.Type, "Prefabs") == 0 && item.Obj != null)
            {
                if (item.Obj.GetType() == typeof(Material))
                {
                    Material mat = item.Obj as Material;
                    if (mat != null && mat.mainTexture != null)
                    {
                        string texturePath = AssetDatabase.GetAssetPath(mat.mainTexture);
                        if (!DefaultAsset(texturePath))
                        {
                            texturePath = unityPath2PhysicsPath(texturePath);
                            tmpMd5 = string.Concat(selfMD5, "_", MD5Help.GetMD5(texturePath));
                        }
                    }
                    //DEBUG.Log(item.FileName + "->item.Obj.GetType() == typeof(Material)->" + tmpMd5);
                }
                if (item.Obj.GetType() == typeof(UIAtlas))
                {
                    UIAtlas atlas = item.Obj as UIAtlas;
                    //DEBUG.Log((atlas != null) + "," + (atlas.spriteMaterial != null) + "," + (atlas.spriteMaterial.mainTexture != null));
                    if (atlas != null && atlas.spriteMaterial != null && atlas.spriteMaterial.mainTexture != null)
                    {
                        string texturePath = AssetDatabase.GetAssetPath(atlas.spriteMaterial.mainTexture);
                        if (!DefaultAsset(texturePath))
                        {
                            texturePath = unityPath2PhysicsPath(texturePath);
                            tmpMd5 = string.Concat(selfMD5, "_", MD5Help.GetMD5(texturePath));
                        }
                    }
                    //DEBUG.Log(item.FileName + "->item.Obj.GetType() == typeof(UIAtlas)->" + tmpMd5);
                }
                if (item.Obj.GetType() == typeof(UIFont))
                {
                    UIFont uiFont = item.Obj as UIFont;
                    if (uiFont != null && uiFont.material != null && uiFont.material.mainTexture != null)
                    {
                        string texturePath = AssetDatabase.GetAssetPath(uiFont.material.mainTexture);
                        if (!DefaultAsset(texturePath))
                        {
                            texturePath = unityPath2PhysicsPath(texturePath);
                            tmpMd5 = string.Concat(selfMD5, "_", MD5Help.GetMD5(texturePath));
                        }
                    }
                    //DEBUG.Log(item.FileName + "->item.Obj.GetType() == typeof(UIFont)->" + tmpMd5);
                }
             */
            }
            if (!File.Exists(physicsPath))
            {
                DEBUG.LogError("error path:" + item.Key);
            }
        }
        else
        {
            tmpMd5 = "default_resources";
        }
        if (Md5List.ContainsKey(item.Key))
        {
            needUpdate = (string.Compare(tmpMd5, Md5List[item.Key]) != 0);
            if (needUpdate)
            {
                Md5List[item.Key] = tmpMd5;
                exportList.Add(item.ExportPath);
                DEBUG.Log("Update item:" + item.Key + "->" + tmpMd5);
                addUpdateItem(item, 1);
            }
        }
        else
        {
            Md5List[item.Key] = tmpMd5;
            exportList.Add(item.ExportPath);
            DEBUG.Log("Added item:" + item.Key + "->" + tmpMd5);
            addUpdateItem(item, 2);
        }
        addFileList(item);
        /*
        if (ForceExport > -1)
        {
            needUpdate = true;
        }
         * */
        //return true;
        return needUpdate;
    }
    public static void AddTmpFile(Object o)
    {
        if (o == null) return;
        string assetPath = AssetDatabase.GetAssetPath(o);
        if (!DefaultAsset(assetPath)) tmpFileList[AssetDatabase.GetAssetPath(o)] = o;
    }
    public static void ClearTmpFile()
    {
        List<string> fList = new List<string>();
        foreach (KeyValuePair<string, Object> k in tmpFileList)
        {
            fList.Add(k.Key);
        }
        string tmp = null;
        for (int i = 0; i < fList.Count; i++)
        {
            tmp = fList[i];
            if (tmp != null) AssetDatabase.DeleteAsset(tmp);
        }
        ExportItem.ClearTempFile();
        AssetDatabase.Refresh();
    }

    public static void ResetUpdateState()
    {
        tmpUpdateList.Clear();
    }
    static void addUpdateItem(ExportItem item, int updateState)
    {
        if(!tmpUpdateList.ContainsKey(updateState))
            tmpUpdateList[updateState] = new List<ExportItem>();

        tmpUpdateList[updateState].Add(item);
    }

    public static Dictionary<int, List<ExportItem>> UpdateList {get { return tmpUpdateList; }}

    static Dictionary<string, Object> tmpFileList = new Dictionary<string, Object>();
    static Dictionary<int, List<ExportItem>> tmpUpdateList = new Dictionary<int,List<ExportItem>>();
    static IEnumerable<Object> SceneRoots()
    {
        HierarchyProperty hp = new HierarchyProperty(HierarchyType.GameObjects);
        int[] expanded = new int[0];
        while (hp.Next(expanded))
        {
            yield return hp.pptrValue;
        }
    }
    public static GameObject[] GetAllGameObject()
    {
        List<GameObject> objs = new List<GameObject>();
        foreach (Object o in SceneRoots())
        {
            GetChild(o as GameObject, objs);
        }
        return objs.ToArray();
    }
    static void GetChild(GameObject root, List<GameObject> objs)
    {
        foreach (Transform child in root.transform)
        {
            GetChild(child.gameObject, objs);
        }
        objs.Add(root);
    }
    static void addFileList(ExportItem item)
    {
        string fileName = Helper.CombinePath(item.RealAssetUnityPath, item.FileName);
        if (!string.IsNullOrEmpty(fileName))
        {
            if (FileList.ContainsKey(fileName))
            {
                string path = FileList[fileName];
                if (File.Exists(path))
                {
                    if (string.Compare(item.RealAssetUnityPath, path) != 0)
                    {
                        string errorMessage = "发现文件 " + item.RealAssetUnityPath + " 与之前的文件 " + path + " 文件名重复，请修改文件名后再试!";
                        UnityEditor.EditorUtility.DisplayDialog("导出场景 " + lastScene, errorMessage, "嗯,我改!");
                        //throw (new Exception(errorMessage));
                        DEBUG.LogError(errorMessage);
                    }
                }
                else
                {
                    FileList.Remove(fileName);
                }
            }
            else
            {
                FileList[fileName] = item.RealAssetUnityPath;
            }
        }
    }
    public static string unityPath2PhysicsPath(string unityPath)
    {
        StringBuilder sb = new StringBuilder();
        string[] part = unityPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < part.Length; i++)
        {
            if (i == 0)
            {
                sb.Append(System.Environment.CurrentDirectory);
                sb.Append(Path.DirectorySeparatorChar);
                sb.Append(part[i]);
            }
            else
            {
                sb.Append(Path.DirectorySeparatorChar);
                sb.Append(part[i]);
            }
        }
        return sb.ToString();
    }
    public static void ReadMd5List()
    {
        if (!readMd5ListState) return;
        readMd5ListState = false;
        string key = "", md5 = "", tmpStr = "";
        Md5List.Clear();
        string md5FilePath = getMd5File();
        if (File.Exists(md5FilePath))
        {
            List<string> tmpList = new List<string>();
            using (StreamReader sr = new StreamReader(md5FilePath, System.Text.Encoding.UTF8))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    tmpList.Add(line);
                }
            }
            for (int i = 0; i < tmpList.Count; i++)
            {
                key = tmpList[i];
                md5 = tmpList[i + 1];
                if (RemoveNotExistsFile)
                {
                    if (key.IndexOf("default_resources") > -1)
                    {
                        Md5List[key] = md5;
                    }
                    else
                    {
                        tmpStr = key.Substring(0, key.IndexOf("&"));
                        if (File.Exists(tmpStr))
                        {
                            Md5List[key] = md5;
                        }
                    }
                }
                else
                {
                    Md5List[key] = md5;
                }
                i++;
            }
        }
        FileList.Clear();
        string fileListFileName = getAllFileList();
        if (File.Exists(fileListFileName))
        {
            List<string> tmpList = new List<string>();
            using (StreamReader sr = new StreamReader(fileListFileName, System.Text.Encoding.UTF8))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    tmpList.Add(line);
                }
            }
            for (int i = 0; i < tmpList.Count; i++)
            {
                FileList[tmpList[i]] = tmpList[i + 1];
                i++;
            }
        }
    }
    public static void WriteMd5List()
    {
        if (Md5List.Count != 0)
        {
			// 写配置文件
            using (StreamWriter sr = new StreamWriter(getMd5FileCSV(), false, System.Text.Encoding.UTF8))
            {
				sr.WriteLine("INT,STRING,STRING");
				sr.WriteLine("INDEX,NAME,MD5");
				sr.WriteLine("0,filename desc,file md5 desc");
				int index = 1000;
                foreach (KeyValuePair<string, string> m in Md5List)
                {
					string srcFileName = m.Key;
					if(!srcFileName.Contains("unity default resources")) {
						sr.WriteLine(index + "," + srcFileName + "," + m.Value);
						index ++;
					} else {
						// ingore unity default file
					}

                }
				
            }

			// 写log文件
			using (StreamWriter sr = new StreamWriter(getMd5File(), false, System.Text.Encoding.UTF8))
			{
				foreach (KeyValuePair<string, string> m in Md5List)
				{
					sr.WriteLine(m.Key);
					sr.WriteLine(m.Value);
				}
			}
        }
        if (FileList.Count != 0)
        {
            using (StreamWriter sr = new StreamWriter(getAllFileList(), false, System.Text.Encoding.UTF8))
            {
                foreach (KeyValuePair<string, string> m in FileList)
                {
                    sr.WriteLine(m.Key);
                    sr.WriteLine(m.Value);
                }
            }
        }
    }
    static void readExportSceneSetting()
    {
        string path = getExportScene();
        if (File.Exists(path))
        {
            List<string> tmpList = new List<string>();
            using (StreamReader sr = new StreamReader(path, System.Text.Encoding.UTF8))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    tmpList.Add(line);
                }
            }
            findPath = tmpList[0];
            samePath = tmpList[1];
            tmpList.RemoveAt(0);
            tmpList.RemoveAt(0);
            exportSceneSettingList = new AssetItemState[(tmpList.Count / 3) + 1];
            int sceneCount = tmpList.Count / 3;
            for (int i = 0; i < sceneCount; i++)
            {
                exportSceneSettingList[i] = new AssetItemState(tmpList[i * 3], tmpList[i * 3 + 1], tmpList[i * 3 + 2]);
            }
            exportSceneSettingList[exportSceneSettingList.Length - 1] = new AssetItemState("", "1", "0");
        }
        else
        {
            exportSceneSettingList = new AssetItemState[1];
            exportSceneSettingList[0] = new AssetItemState("", "1", "0");
        }
    }
    static void writeExportSceneSetting()
    {
        if (exportSceneSettingList != null && exportSceneSettingList.Length != 0)
        {
            using (StreamWriter sr = new StreamWriter(getExportScene(), false, System.Text.Encoding.UTF8))
            {
                sr.WriteLine(findPath);
                sr.WriteLine(samePath);
                foreach (AssetItemState m in exportSceneSettingList)
                {
                    if (!string.IsNullOrEmpty(m.SceneName))
                    {
                        sr.WriteLine(m.SceneName);
                        sr.WriteLine(m.Enable ? "1" : "0");
                        sr.WriteLine(m.State.ToString());
                    }
                }
            }
        }
    }
    public static void WriteExportFileList()
    {
        if (exportList.Count > 0)
        {
            using (StreamWriter sr = new StreamWriter(getExportFileList(), false, System.Text.Encoding.UTF8))
            {
                foreach (string m in exportList)
                {
                    if (!string.IsNullOrEmpty(m))
                    {
                        sr.WriteLine(m);
                    }
                }
            }
        }
    }
    static string getMd5File()
    {
        return "Assets/Export/SourceFileMd5_" + EditorUserBuildSettings.activeBuildTarget + ".dat";
    }
	static string getMd5FileCSV()
	{
		return "Assets/Export/SourceFileMd5.csv";
	}
    static string getExportScene()
    {
        return "Assets/Export/ExportSceneSetting.dat";
    }
    static string getAllFileList()
    {
        return "Assets/Export/FileList.dat";
    }
    static string getExportFileList()
    {
        return "Assets/Export/ExportFileList_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".dat";
    }
    public static bool DefaultAsset(string path)
    {
        return path.IndexOf("default resources") > -1;
    }
}
