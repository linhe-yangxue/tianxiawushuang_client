using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using DataTable;
using System;

/// <summary>
/// 精灵统计数据
/// </summary>
public class SpriteStatisticsData
{
    public string OldPath { set; get; }
    public string OldAtlasName { set; get; }
    public string OldSpriteName { set; get; }
    public string NewPath { set; get; }
    public string NewAtlasName { set; get; }
    public string NewSpriteName { set; get; }

    public void GrabDataFromUISpriteData(UIAtlas atlas, UISpriteData spriteData)
    {
        if (spriteData == null)
            return;

        OldPath = AssetDatabase.GetAssetPath(atlas.gameObject);
        OldAtlasName = atlas.name;
        OldSpriteName = spriteData.name;
    }
}
/// <summary>
/// 精灵重名统计
/// </summary>
public class DuplicationSpriteStatisticsHelper
{
    private Dictionary<string, List<SpriteStatisticsData>> mDicDupStat = new Dictionary<string, List<SpriteStatisticsData>>();

    public void Stat(object[] objs)
    {
        if (objs == null || objs.Length <= 0)
            return;

        for (int i = 0, count = objs.Length; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("Duplication name sprite statistics", i + "/" + count, (float)i / (float)count);

            GameObject tmpGO = objs[i] as GameObject;
            if (tmpGO == null)
                continue;
            UIAtlas tmpAtlas = tmpGO.GetComponent<UIAtlas>();
            if (tmpAtlas == null)
                continue;
            __StatAtlas(tmpAtlas);
        }
        EditorUtility.ClearProgressBar();

        string tmpSavePath = EditorUtility.SaveFilePanel("Save stat file", Application.streamingAssetsPath + "/Atlas/", "DuplicationSpriteStat", "csv");
        if (tmpSavePath != "")
            __Save(tmpSavePath);
    }

    private void __StatAtlas(UIAtlas atlas)
    {
        if (atlas == null || atlas.spriteList == null || atlas.spriteList.Count <= 0)
            return;

        for (int i = 0, count = atlas.spriteList.Count; i < count; i++)
        {
            UISpriteData tmpSpriteData = atlas.spriteList[i];
            List<SpriteStatisticsData> tmpList = __GetStatList(tmpSpriteData.name);
            SpriteStatisticsData tmpSpriteStatData = new SpriteStatisticsData();
            tmpSpriteStatData.GrabDataFromUISpriteData(atlas, tmpSpriteData);
            tmpList.Add(tmpSpriteStatData);
        }
    }
    /// <summary>
    /// 获取精灵列表
    /// </summary>
    /// <param name="spriteName"></param>
    /// <returns></returns>
    private List<SpriteStatisticsData> __GetStatList(string spriteName)
    {
        if (mDicDupStat == null)
            return null;

        List<SpriteStatisticsData> tmpValue = null;
        if (!mDicDupStat.TryGetValue(spriteName, out tmpValue))
        {
            tmpValue = new List<SpriteStatisticsData>();
            mDicDupStat[spriteName] = tmpValue;
        }
        return tmpValue;
    }

    private void __Save(string path)
    {
        if (path == "")
        {
            EditorUtility.DisplayDialog("Warning", "Save path can't be empty.", "Ok");
            return;
        }

        NiceTable tmpTable = new NiceTable();

        tmpTable.SetField("SPRITE_NAME", FIELD_TYPE.FIELD_STRING, 0);
        tmpTable.SetField("COUNT", FIELD_TYPE.FIELD_INT, 1);
        tmpTable.SetField("ATLAS_NAME_0", FIELD_TYPE.FIELD_STRING, 2);
        tmpTable.SetField("ATLAS_NAME_1", FIELD_TYPE.FIELD_STRING, 3);
        tmpTable.SetField("ATLAS_NAME_2", FIELD_TYPE.FIELD_STRING, 4);
        tmpTable.SetField("ATLAS_NAME_3", FIELD_TYPE.FIELD_STRING, 5);
        tmpTable.SetField("ATLAS_NAME_4", FIELD_TYPE.FIELD_STRING, 6);
        tmpTable.SetField("ATLAS_NAME_5", FIELD_TYPE.FIELD_STRING, 7);

        foreach (KeyValuePair<string, List<SpriteStatisticsData>> tmpPair in mDicDupStat)
        {
            List<SpriteStatisticsData> tmpList = tmpPair.Value;
            int tmpCount = tmpList.Count;
            if (tmpCount <= 1)
                continue;
            DataRecord tmpRecord = tmpTable.CreateRecord(tmpPair.Key);
            tmpRecord.set("COUNT", tmpCount);
            for (int i = 0; i < 5; i++)
            {
                if (i < tmpCount)
                    tmpRecord.set("ATLAS_NAME_" + i, tmpList[i].OldPath);
            }
        }

        tmpTable.SaveTable(path, System.Text.Encoding.ASCII);
        EditorUtility.DisplayDialog("Save success", "FilePath : " + path, "Ok");
    }
}

/// <summary>
/// 指向空UIAtlas的UISprite统计
/// </summary>
public class NullAtlasRefHelper
{
    private Dictionary<int, string> mDicAtlas = new Dictionary<int, string>();      //Key：递增序号，Value：Atlas路径

    /// <summary>
    /// 
    /// </summary>
    /// <param name="objs"></param>
    /// <param name="onlyPrefab">是否只统计Prefab</param>
    public void Stat(object[] objs, bool onlyPrefab)
    {
        if (objs == null || objs.Length <= 0)
            return;

        for (int i = 0, count = objs.Length; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("Null atlas reference file statistics", i + "/" + count, (float)i / (float)count);

            GameObject tmpGO = objs[i] as GameObject;
            if (tmpGO == null)
                continue;
            string tmpCurrPath = AssetDatabase.GetAssetPath(tmpGO.GetInstanceID()) + "#@#";
            __Stat(tmpGO, onlyPrefab, tmpCurrPath);
        }
        EditorUtility.ClearProgressBar();

        string tmpDefaultFileName = onlyPrefab ? "NullAtlasReferencePrefab" : "NullAtlasReference";
        string tmpSavePath = EditorUtility.SaveFilePanel("Save stat file", Application.streamingAssetsPath + "/Atlas/", tmpDefaultFileName, "csv");
        if (tmpSavePath != "")
            __Save(tmpSavePath);
    }

    /// <summary>
    /// 统计
    /// </summary>
    /// <param name="go"></param>
    private void __Stat(GameObject go, bool onlyPrefab, string path)
    {
        if(go == null)
            return;

        if (!__SpriteStat(go, onlyPrefab, path))
            return;
        foreach (Transform tmpTrans in go.transform)
            __Stat(tmpTrans.gameObject, onlyPrefab, path + "/" + tmpTrans.gameObject.name);
    }
    /// <summary>
    /// 统计UISprite
    /// </summary>
    /// <param name="go"></param>
    /// <param name="onlyPrefab"></param>
    /// <param name="path"></param>
    /// <returns>是否继续遍历子节点</returns>
    private bool __SpriteStat(GameObject go, bool onlyPrefab, string path)
    {
        if (go == null)
            return false;

        UISprite tmpSprite = go.GetComponent<UISprite>();
        if (tmpSprite == null || (tmpSprite.atlas != null && tmpSprite.spriteName != "" && tmpSprite.spriteName != null))
            return true;

        if (onlyPrefab)
        {
            string tmpPrefabPath = "";
            int tmpRemoveIndex = path.IndexOf("#@#");
            tmpPrefabPath = (tmpRemoveIndex >= path.Length) ? path : path.Remove(tmpRemoveIndex);
            //获取Prefab名称
            string[] tmpSplitPath = tmpPrefabPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            tmpPrefabPath = tmpSplitPath[tmpSplitPath.Length - 1];
            //去除后缀
            tmpPrefabPath = tmpPrefabPath.Remove(tmpPrefabPath.IndexOf('.'));
            if (!mDicAtlas.ContainsValue(tmpPrefabPath))
                mDicAtlas[mDicAtlas.Count] = tmpPrefabPath;
            //直接返回，没必要继续向子物体统计
            return false;
        }
        else
            mDicAtlas[mDicAtlas.Count] = path + "#@#UISprite";

        return true;
    }

    private void __Save(string path)
    {
        if (path == "")
        {
            EditorUtility.DisplayDialog("Warning", "Save path can't be empty.", "Ok");
            return;
        }

        NiceTable tmpTable = new NiceTable();

        tmpTable.SetField("INDEX", FIELD_TYPE.FIELD_INT, 0);
        tmpTable.SetField("PATH", FIELD_TYPE.FIELD_STRING, 1);

        foreach (KeyValuePair<int, string> tmpPair in mDicAtlas)
        {
            DataRecord tmpRecord = tmpTable.CreateRecord(tmpPair.Key);
            tmpRecord.set("PATH", tmpPair.Value);
        }

        tmpTable.SaveTable(path, System.Text.Encoding.ASCII);
        EditorUtility.DisplayDialog("Save success", "FilePath : " + path, "Ok");
    }
}

public class AtlasSpriteStatData
{
    public string AtlasName { set; get; }
    public string SpriteName { set; get; }
}
/// <summary>
/// 统计每一个Atlas中所有的Sprite
/// </summary>
public class AtlasSpriteStatHelper
{
    private Dictionary<int, AtlasSpriteStatData> mDicAtlasSprite = new Dictionary<int, AtlasSpriteStatData>();

    public Dictionary<int, AtlasSpriteStatData> atlasSprite
    {
        get{return mDicAtlasSprite;}
    }

    public void Stat(UIAtlas atlas, string saveFolderPath, bool isShowCompleteDialog)
    {
        if (atlas == null)
            return;

        string tmpAtlasName = atlas.gameObject.name;
        for (int i = 0, count = atlas.spriteList.Count; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("Atlas - sprite statistics", i + "/" + count, (float)i / (float)count);

            mDicAtlasSprite[mDicAtlasSprite.Count] = new AtlasSpriteStatData() { AtlasName = tmpAtlasName, SpriteName = atlas.spriteList[i].name };
        }
        EditorUtility.ClearProgressBar();

        __Save(saveFolderPath + "/" + tmpAtlasName + ".csv", isShowCompleteDialog);
    }

    private void __Save(string path, bool isShowCompleteDialog)
    {
        if (path == "")
        {
            EditorUtility.DisplayDialog("Warning", "Save path can't be empty.", "Ok");
            return;
        }

        NiceTable tmpTable = new NiceTable();

        tmpTable.SetField("INDEX", FIELD_TYPE.FIELD_INT, 0);
        tmpTable.SetField("ATLAS_NAME", FIELD_TYPE.FIELD_STRING, 1);
        tmpTable.SetField("SPRITE_NAME", FIELD_TYPE.FIELD_STRING, 2);

        foreach (KeyValuePair<int, AtlasSpriteStatData> tmpPair in mDicAtlasSprite)
        {
            DataRecord tmpRecord = tmpTable.CreateRecord(tmpPair.Key);

            tmpRecord.set("ATLAS_NAME", tmpPair.Value.AtlasName);
            tmpRecord.set("SPRITE_NAME", tmpPair.Value.SpriteName);
        }

        tmpTable.SaveTable(path, System.Text.Encoding.ASCII);
        if(isShowCompleteDialog)
            EditorUtility.DisplayDialog("Save success", "FilePath : " + path, "Ok");
    }

    public void Load(string path)
    {
        if (path == "")
        {
            EditorUtility.DisplayDialog("Warning", "Load path can't be empty.", "Ok");
            return;
        }

        NiceTable tmpTable = new NiceTable();
        tmpTable.LoadTable(path, System.Text.Encoding.ASCII, LOAD_MODE.ANIS);

        foreach (KeyValuePair<int, DataRecord> tmpPair in tmpTable.GetAllRecord())
        {
            DataRecord tmpRecord = tmpPair.Value;
            mDicAtlasSprite[tmpPair.Key] = new AtlasSpriteStatData() { AtlasName = tmpRecord.getObject("ATLAS_NAME").ToString(), SpriteName = tmpRecord.getObject("SPRITE_NAME").ToString() };
        }

        EditorUtility.DisplayDialog("Load success", "FilePath : " + path, "Ok");
    }
}

/// <summary>
/// Sprite引用数统计
/// </summary>
public class SpriteRefCountHelper
{
    private Dictionary<string, int> mDicSprite = new Dictionary<string, int>();
    private AtlasSpriteStatHelper mAtlasSprite = new AtlasSpriteStatHelper();

    public void Stat(object[] objs, string dataFilePath)
    {
        if (objs == null || objs.Length <= 0)
            return;

        mAtlasSprite.Load(dataFilePath);
        foreach (KeyValuePair<int, AtlasSpriteStatData> tmpPair in mAtlasSprite.atlasSprite)
            mDicSprite[tmpPair.Value.SpriteName] = 0;
        for (int i = 0, count = objs.Length; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("Sprite reference count statistics", i + "/" + count, (float)i / (float)count);

            GameObject tmpGO = objs[i] as GameObject;
            if (tmpGO == null)
                continue;
            __Stat(tmpGO);
        }
        EditorUtility.ClearProgressBar();

        string tmpPath = EditorUtility.SaveFilePanel("Save stat file", Application.streamingAssetsPath + "/Atlas/", "SpriteRefCount", "csv");
        if(tmpPath != "")
            __Save(tmpPath);
    }

    private void __Stat(GameObject go)
    {
        if (go == null)
            return;

        UISprite tmpSprite = go.GetComponent<UISprite>();
        if (tmpSprite != null && tmpSprite.spriteName != "" && tmpSprite.spriteName != null)
        {
            foreach (KeyValuePair<int, AtlasSpriteStatData> tmpPair in mAtlasSprite.atlasSprite)
            {
                if (((tmpSprite.atlas != null && tmpSprite.atlas.name == tmpPair.Value.AtlasName) ||      //Atlas不为null时，UISprite中Atlas、Sprite要完全匹配
                    tmpSprite.atlas == null) &&        //Atlas为null时统计
                    tmpSprite.spriteName == tmpPair.Value.SpriteName)
                {
                    __Add(tmpSprite.spriteName, 1);
                    break;
                }
            }
        }
        foreach (Transform tmpTrans in go.transform)
            __Stat(tmpTrans.gameObject);
    }

    private void __Add(string key, int count)
    {
        if (!mDicSprite.ContainsKey(key))
            mDicSprite[key] = count;
        else
            mDicSprite[key] += count;
    }

    private void __Save(string path)
    {
        if (path == "")
        {
            EditorUtility.DisplayDialog("Warning", "Save path can't be empty.", "Ok");
            return;
        }

        NiceTable tmpTable = new NiceTable();

        tmpTable.SetField("SPRITE_NAME", FIELD_TYPE.FIELD_STRING, 0);
        tmpTable.SetField("REF_COUNT", FIELD_TYPE.FIELD_INT, 1);

        foreach (KeyValuePair<string, int> tmpPair in mDicSprite)
        {
            DataRecord tmpRecord = tmpTable.CreateRecord(tmpPair.Key);

            tmpRecord.set("REF_COUNT", tmpPair.Value);
        }

        tmpTable.SaveTable(path, System.Text.Encoding.ASCII);
        EditorUtility.DisplayDialog("Save success", "FilePath : " + path, "Ok");
    }
}

public class SpriteRefData
{
    public string AtlasName { set; get; }
    public string SpriteName { set; get; }
    public string RefPrefabName { set; get; }
    public string RefPath { set; get; }
}
/// <summary>
/// Sprite引用统计
/// </summary>
public class SpriteRefHelper
{
    private Dictionary<int, SpriteRefData> mDicSpriteRef = new Dictionary<int, SpriteRefData>();
    private AtlasSpriteStatHelper mAtlasSprite = new AtlasSpriteStatHelper();

    public void Stat(object[] objs, string dataFilePath)
    {
        if (objs == null || objs.Length <= 0)
            return;

        mAtlasSprite.Load(dataFilePath);
        for (int i = 0, count = objs.Length; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("Sprite reference count statistics", i + "/" + count, (float)i / (float)count);

            GameObject tmpGO = objs[i] as GameObject;
            if (tmpGO == null)
                continue;
            string tmpGOPath = AssetDatabase.GetAssetPath(tmpGO.GetInstanceID()) + "#@#";
            __Stat(tmpGO, tmpGOPath);
        }
        EditorUtility.ClearProgressBar();

        string tmpPath = EditorUtility.SaveFilePanel("Save stat file", Application.streamingAssetsPath + "/Atlas/", "SpriteRef", "csv");
        if (tmpPath != "")
            __Save(tmpPath);
    }

    private void __Stat(GameObject go, string path)
    {
        if (go == null)
            return;

        UISprite tmpSprite = go.GetComponent<UISprite>();
        if (tmpSprite != null && tmpSprite.spriteName != "" && tmpSprite.spriteName != null)
        {
            foreach (KeyValuePair<int, AtlasSpriteStatData> tmpPair in mAtlasSprite.atlasSprite)
            {
                if (((tmpSprite.atlas != null && tmpSprite.atlas.name == tmpPair.Value.AtlasName) ||      //Atlas不为null时，UISprite中Atlas、Sprite要完全匹配
                    tmpSprite.atlas == null)    &&        //Atlas为null时统计
                    tmpSprite.spriteName == tmpPair.Value.SpriteName)
                {
                    int tmpIdx = path.IndexOf("#@#");
                    mDicSpriteRef[mDicSpriteRef.Count] = new SpriteRefData()
                    {
                        AtlasName = tmpPair.Value.AtlasName,
                        SpriteName = tmpPair.Value.SpriteName,
                        RefPrefabName = path.Remove(tmpIdx),
                        RefPath = path.Substring(tmpIdx)
                    };
                    break;
                }
            }
        }
        foreach (Transform tmpTrans in go.transform)
            __Stat(tmpTrans.gameObject, path + "/" + tmpTrans.name);
    }

    private void __Save(string path)
    {
        if (path == "")
        {
            EditorUtility.DisplayDialog("Warning", "Save path can't be empty.", "Ok");
            return;
        }

        NiceTable tmpTable = new NiceTable();

        tmpTable.SetField("INDEX", FIELD_TYPE.FIELD_INT, 0);
        tmpTable.SetField("ATLAS_NAME", FIELD_TYPE.FIELD_STRING, 1);
        tmpTable.SetField("SPRITE_NAME", FIELD_TYPE.FIELD_STRING, 2);
        tmpTable.SetField("REF_PREFAB_NAME", FIELD_TYPE.FIELD_STRING, 3);
        tmpTable.SetField("REF_PATH", FIELD_TYPE.FIELD_STRING, 4);

        foreach (KeyValuePair<int, SpriteRefData> tmpPair in mDicSpriteRef)
        {
            DataRecord tmpRecord = tmpTable.CreateRecord(tmpPair.Key);

            tmpRecord.set("ATLAS_NAME", tmpPair.Value.AtlasName);
            tmpRecord.set("SPRITE_NAME", tmpPair.Value.SpriteName);
            tmpRecord.set("REF_PREFAB_NAME", tmpPair.Value.RefPrefabName);
            tmpRecord.set("REF_PATH", tmpPair.Value.RefPath);
        }

        tmpTable.SaveTable(path, System.Text.Encoding.ASCII);
        EditorUtility.DisplayDialog("Save success", "FilePath : " + path, "Ok");
    }
}

public class AtlasStatistics
{
    /// <summary>
    /// 菜单项：统计Atlas中Sprite重名
    /// </summary>
    [MenuItem("Assets/Atlas Changer/Atlas Statistics/Duplication Sprite Name")]
    public static void MenuDuplicationSpriteName()
    {
        object[] tmpSel = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
        if (tmpSel == null || tmpSel.Length <= 0)
        {
            EditorUtility.DisplayDialog("Warning", "No object selected.", "Ok");
            return;
        }

        DuplicationSpriteStatisticsHelper tmpStat = new DuplicationSpriteStatisticsHelper();
        tmpStat.Stat(tmpSel);
    }

    /// <summary>
    /// 菜单项：统计UI Prefab中的Atlas空引用项
    /// </summary>
    [MenuItem("Assets/Atlas Changer/Atlas Statistics/Null Atlas Reference(Only Prefab)")]
    public static void MenuNullAtlasRefPrefab()
    {
        object[] tmpSel = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
        if (tmpSel == null || tmpSel.Length <= 0)
        {
            EditorUtility.DisplayDialog("Warning", "No object selected.", "Ok");
            return;
        }

        NullAtlasRefHelper tmpStat = new NullAtlasRefHelper();
        tmpStat.Stat(tmpSel, true);
    }
    [MenuItem("Assets/Atlas Changer/Atlas Statistics/Null Atlas Reference")]
    public static void MenuNullAtlasRef()
    {
        object[] tmpSel = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
        if (tmpSel == null || tmpSel.Length <= 0)
        {
            EditorUtility.DisplayDialog("Warning", "No object selected.", "Ok");
            return;
        }

        NullAtlasRefHelper tmpStat = new NullAtlasRefHelper();
        tmpStat.Stat(tmpSel, false);
    }

    [MenuItem("Assets/Atlas Changer/Atlas Statistics/Atlas - Sprite")]
    public static void MenuAtlasSpriteStatistics()
    {
        object[] tmpSel = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
        if (tmpSel == null || tmpSel.Length <= 0)
        {
            EditorUtility.DisplayDialog("Warning", "No object selected.", "Ok");
            return;
        }

        string tmpFolderPath = EditorUtility.SaveFolderPanel("Save folder", Application.streamingAssetsPath + "/Atlas/", "Atlas-Sprite");
        if (tmpFolderPath == "")
            return;

        for (int i = 0, count = tmpSel.Length; i < count; i++)
        {
            GameObject tmpGO = tmpSel[i] as GameObject;
            if (tmpGO == null)
                continue;
            UIAtlas tmpAtlas = tmpGO.GetComponent<UIAtlas>();
            AtlasSpriteStatHelper tmpStat = new AtlasSpriteStatHelper();
            tmpStat.Stat(tmpAtlas, tmpFolderPath, false);
        }
    }

    [MenuItem("Assets/Atlas Changer/Atlas Statistics/Sprite Reference Count")]
    public static void MenuSpriteRefCountStatistics()
    {
        object[] tmpSel = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
        if (tmpSel == null || tmpSel.Length <= 0)
        {
            EditorUtility.DisplayDialog("Warning", "No object selected.", "Ok");
            return;
        }

        SpriteRefCountHelper tmpStat = new SpriteRefCountHelper();
        string tmpDataFilePath = EditorUtility.OpenFilePanel("Open Atlas-Sprite file", Application.streamingAssetsPath + "/Atlas/", "csv");
        tmpStat.Stat(tmpSel, tmpDataFilePath);
    }

    [MenuItem("Assets/Atlas Changer/Atlas Statistics/Sprite Reference")]
    public static void MenuSpriteRefStatistics()
    {
        object[] tmpSel = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
        if (tmpSel == null || tmpSel.Length <= 0)
        {
            EditorUtility.DisplayDialog("Warning", "No object selected.", "Ok");
            return;
        }

        SpriteRefHelper tmpStat = new SpriteRefHelper();
        string tmpDataFilePath = EditorUtility.OpenFilePanel("Open Atlas-Sprite file", Application.streamingAssetsPath + "/Atlas/", "csv");
        tmpStat.Stat(tmpSel, tmpDataFilePath);
    }
}
