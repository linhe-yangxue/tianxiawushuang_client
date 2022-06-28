using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using DataTable;

public class SpriteLayoutData
{
    public int x = 0;
    public int y = 0;
    public int width = 0;
    public int height = 0;

    public int borderLeft = 0;
    public int borderRight = 0;
    public int borderTop = 0;
    public int borderBottom = 0;

    public int paddingLeft = 0;
    public int paddingRight = 0;
    public int paddingTop = 0;
    public int paddingBottom = 0;

    public void GrabDataFromUISpriteData(UISpriteData spriteData)
    {
        x = spriteData.x;
        y = spriteData.y;
        width = spriteData.width;
        height = spriteData.height;

        borderLeft = spriteData.borderLeft;
        borderRight = spriteData.borderRight;
        borderTop = spriteData.borderTop;
        borderBottom = spriteData.borderBottom;

        paddingLeft = spriteData.paddingLeft;
        paddingRight = spriteData.paddingRight;
        paddingTop = spriteData.paddingTop;
        paddingBottom = spriteData.paddingBottom;
    }
}
public class SpriteData
{
    private SpriteLayoutData mLayoutData;

    public string SpriteName { set; get; }
    public SpriteLayoutData LayoutData
    {
        set { mLayoutData = value; }
        get { return mLayoutData; }
    }
}
/// <summary>
/// 图集数据
/// </summary>
public class AtlasData
{
    private List<SpriteData> mListSprite = new List<SpriteData>();

    public string AtlasPath { set; get; }
    public string AtlasName { set; get; }
    public List<SpriteData> ListSprite
    {
        get { return mListSprite; }
    }

    public void GrabData(UIAtlas atlas, bool grabSpriteLayoutData)
    {
        if (mListSprite == null || atlas == null)
            return;

        AtlasName = atlas.name;
        AtlasPath = AssetDatabase.GetAssetPath(atlas.GetInstanceID());
        for (int i = 0, count = atlas.spriteList.Count; i < count; i++)
        {
            SpriteData tmpSpriteData = new SpriteData();
            tmpSpriteData.SpriteName = atlas.spriteList[i].name;
            if (grabSpriteLayoutData)
            {
                tmpSpriteData.LayoutData = new SpriteLayoutData();
                tmpSpriteData.LayoutData.GrabDataFromUISpriteData(atlas.spriteList[i]);
            }
            mListSprite.Add(tmpSpriteData);
        }
    }
}
/// <summary>
/// 遍历图集
/// </summary>
public class AtlasTraversalHelper
{
    private Dictionary<string, AtlasData> mDicAtlas = new Dictionary<string, AtlasData>();                  //以图集名称为键值
    private Dictionary<string, AtlasData> mDicAtlasBySpriteName = new Dictionary<string, AtlasData>();      //以精灵名称为键值
    private Dictionary<string, SpriteData> mDicSpriteBySpriteName = new Dictionary<string, SpriteData>();   //以精灵名称为键值

    /// <summary>
    /// 遍历
    /// </summary>
    /// <param name="objs"></param>
    public void Traversal(object[] objs, bool grabSpriteLayoutData)
    {
        if (objs == null || objs.Length <= 0)
            return;

        for (int i = 0, count = objs.Length; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("Atlas traversal", i + "/" + count, (float)i / (float)count);

            GameObject tmpGO = objs[i] as GameObject;
            if (tmpGO == null)
                continue;
            UIAtlas tmpAtlas = tmpGO.GetComponent<UIAtlas>();
            if (tmpAtlas == null)
                continue;
            __AtlasTraversal(tmpAtlas, grabSpriteLayoutData);
        }
        EditorUtility.ClearProgressBar();

        string tmpFilePath = EditorUtility.SaveFilePanel("Save UIAtlas travesal", Application.streamingAssetsPath + "/Atlas/", "AtlasTraversal", "csv");
        if (tmpFilePath != "")
            __Save(tmpFilePath, grabSpriteLayoutData);
    }

    /// <summary>
    /// 获取spriteName对应的AtlasData
    /// </summary>
    /// <param name="spriteName"></param>
    /// <returns></returns>
    public AtlasData GetAtlasDataBySpriteName(string spriteName)
    {
        if (mDicAtlasBySpriteName == null || mDicAtlasBySpriteName.Count <= 0)
            return null;

        AtlasData tmpAtlasData = null;
        if (!mDicAtlasBySpriteName.TryGetValue(spriteName, out tmpAtlasData))
            return null;
        return tmpAtlasData;
    }
    /// <summary>
    /// 获取spriteName对应的SpriteData
    /// </summary>
    /// <param name="spriteName"></param>
    /// <returns></returns>
    public SpriteData GetSpriteDataBySpriteName(string spriteName)
    {
        if (mDicSpriteBySpriteName == null || mDicSpriteBySpriteName.Count <= 0)
            return null;

        SpriteData tmpSpriteData = null;
        if (!mDicSpriteBySpriteName.TryGetValue(spriteName, out tmpSpriteData))
            return null;
        return tmpSpriteData;
    }

    private void __AtlasTraversal(UIAtlas atlas, bool grabSpriteLayoutData)
    {
        if (mDicAtlas == null)
            return;

        AtlasData tmpAtlasData = new AtlasData();
        tmpAtlasData.GrabData(atlas, grabSpriteLayoutData);
        mDicAtlas[tmpAtlasData.AtlasName] = tmpAtlasData;
        for (int i = 0, count = tmpAtlasData.ListSprite.Count; i < count; i++)
        {
            SpriteData tmpSpriteData = tmpAtlasData.ListSprite[i];
            mDicAtlasBySpriteName[tmpSpriteData.SpriteName] = tmpAtlasData;
            mDicSpriteBySpriteName[tmpSpriteData.SpriteName] = tmpSpriteData;
        }
    }

    private void __Save(string path, bool grabSpriteLayoutData)
    {
        if (path == "")
        {
            EditorUtility.DisplayDialog("Warning", "Save path can't be empty.", "Ok");
            return;
        }

        NiceTable tmpTable = new NiceTable();

        tmpTable.SetField("INDEX", FIELD_TYPE.FIELD_INT, 0);
        tmpTable.SetField("ATLAS_NAME", FIELD_TYPE.FIELD_STRING, 1);
        tmpTable.SetField("ATLAS_PATH", FIELD_TYPE.FIELD_STRING, 2);
        tmpTable.SetField("SPRITE_NAME", FIELD_TYPE.FIELD_STRING, 3);
        if (grabSpriteLayoutData)
            tmpTable.SetField("LAYOUT_DATA", FIELD_TYPE.FIELD_STRING, 4);

        int tmpIndex = 0;
        foreach (KeyValuePair<string, AtlasData> tmpPair in mDicAtlas)
        {
            List<SpriteData> tmpListSprite = tmpPair.Value.ListSprite;
            for (int i = 0, count = tmpListSprite.Count; i < count; i++)
            {
                DataRecord tmpRecord = tmpTable.CreateRecord(tmpIndex);
                SpriteData tmpSpriteData = tmpListSprite[i];
                tmpRecord.set("ATLAS_NAME", tmpPair.Key);
                tmpRecord.set("ATLAS_PATH", tmpPair.Value.AtlasPath);
                tmpRecord.set("SPRITE_NAME", tmpSpriteData.SpriteName);
                if (grabSpriteLayoutData)
                {
                    string tmpStrLayoutData = JCode.Encode(tmpSpriteData.LayoutData);
                    tmpStrLayoutData = tmpStrLayoutData.Replace(",", "#$#");
                    tmpRecord.set("LAYOUT_DATA", tmpStrLayoutData);
                }
                tmpIndex += 1;
            }
        }

        tmpTable.SaveTable(path, System.Text.Encoding.ASCII);
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
            AtlasData tmpAtlasData = null;
            string tmpAtlasName = tmpRecord.getObject("ATLAS_NAME").ToString();
            if (!mDicAtlas.TryGetValue(tmpAtlasName, out tmpAtlasData))
            {
                tmpAtlasData = new AtlasData();
                tmpAtlasData.AtlasName = tmpAtlasName;
                tmpAtlasData.AtlasPath = tmpRecord.getObject("ATLAS_PATH").ToString();
                mDicAtlas[tmpAtlasName] = tmpAtlasData;
            }
            SpriteData tmpSpriteData = new SpriteData();
            tmpSpriteData.SpriteName = tmpRecord.getObject("SPRITE_NAME").ToString();
            object tmpObjLayoutData = tmpRecord.getObject("LAYOUT_DATA");
            if(tmpObjLayoutData != null)
            {
                string tmpStrLayoutData = tmpObjLayoutData.ToString();
                tmpStrLayoutData = tmpStrLayoutData.Replace("#$#", ",");
                tmpSpriteData.LayoutData = JCode.Decode<SpriteLayoutData>(tmpStrLayoutData);
            }
            tmpAtlasData.ListSprite.Add(tmpSpriteData);
            mDicAtlasBySpriteName[tmpSpriteData.SpriteName] = tmpAtlasData;
            mDicSpriteBySpriteName[tmpSpriteData.SpriteName] = tmpSpriteData;
        }

        EditorUtility.DisplayDialog("Load success", "FilePath : " + path, "Ok");
    }
}

public class AtlasTraversal
{
    [MenuItem("Assets/Atlas Changer/Atlas Traversal/Atlas Traversal(Grab Sprite Layout Data)")]
    public static void MenuAtlasTraversalGrab()
    {
        MenuAtlasTraversal(true);
    }
    [MenuItem("Assets/Atlas Changer/Atlas Traversal/Atlas Traversal(No Grab Sprite Layout Data)")]
    public static void MenuAtlasTraversalNoGrab()
    {
        MenuAtlasTraversal(false);
    }
    /// <summary>
    /// 遍历图集
    /// </summary>
    /// <param name="grabSpritePosData">是否捕获精灵布局数据</param>
    public static void MenuAtlasTraversal(bool grabSpriteLayoutData)
    {
        object[] tmpSel = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
        if (tmpSel == null || tmpSel.Length <= 0)
        {
            EditorUtility.DisplayDialog("Warning", "No object selected.", "Ok");
            return;
        }

        AtlasTraversalHelper tmpTraversal = new AtlasTraversalHelper();
        tmpTraversal.Traversal(tmpSel, grabSpriteLayoutData);
    }

    /// <summary>
    /// 根据文件，设置图集中精灵的数据
    /// </summary>
    [MenuItem("Assets/Atlas Changer/Atlas Traversal/Set Sprite Layout Data")]
    public static void MenuSetSpriteLayoutData()
    {
        object[] tmpSel = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
        if (tmpSel == null || tmpSel.Length <= 0)
        {
            EditorUtility.DisplayDialog("Warning", "No object selected.", "Ok");
            return;
        }

        string tmpAtlasDataPath = EditorUtility.OpenFilePanel("Open Atlas Traversal", Application.streamingAssetsPath + "/Atlas/", "csv");
        if (tmpAtlasDataPath == "")
            return;

        AtlasTraversalHelper tmpTraversalHelper = new AtlasTraversalHelper();
        tmpTraversalHelper.Load(tmpAtlasDataPath);
        for (int i = 0, count = tmpSel.Length; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("Set sprite layout data", i + "/" + count, (float)i / (float)count);

            GameObject tmpGO = tmpSel[i] as GameObject;
            if (tmpGO == null)
                continue;
            UIAtlas tmpAtlas = tmpGO.GetComponent<UIAtlas>();
            if (tmpAtlas == null)
                continue;

            for (int j = 0, jCount = tmpAtlas.spriteList.Count; j < jCount; j++)
            {
                UISpriteData tmpUISpriteData = tmpAtlas.spriteList[j];
                SpriteData tmpSpriteData = tmpTraversalHelper.GetSpriteDataBySpriteName(tmpUISpriteData.name);

                //九宫格
                tmpUISpriteData.SetBorder(
                    tmpSpriteData.LayoutData.borderLeft, tmpSpriteData.LayoutData.borderBottom,
                    tmpSpriteData.LayoutData.borderRight, tmpSpriteData.LayoutData.borderTop);
            }
        }
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("Congratulations", "Set sprites layout data success.", "Ok");
    }
}
