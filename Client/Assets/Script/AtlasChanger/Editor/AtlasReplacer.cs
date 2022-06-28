using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// 替换Prefab的Atlas资源
/// </summary>
public class AtlasReplacerWindow : EditorWindow
{
    private float mFuncTitleSpace = 10.0f;
    private float mFuncSpace = 30.0f;

    private string mTargetFilePath = "";    //目标文件路径
    private bool mCanReplace = true;        //是否可以替换

    private string mSrcSpriteName = "";     //替换Sprite源名
    private string mDstSpriteName = "";     //替换Sprite目标名
    private UIAtlas mDstAtlas;      //目标Atlas

    private string mWarning = "";

    [MenuItem("Assets/Atlas Changer/Atlas Replacer/Show Atlas Replacer Window")]
    public static void ShowAtlasReplacer()
    {
        EditorWindow.GetWindow(typeof(AtlasReplacerWindow));
    }

    void OnGUI()
    {
        Color tmpBGColor = GUI.backgroundColor;

        GUILayout.BeginVertical();

        GUI.backgroundColor = Color.green;
        __ReplaceAtlasBySpriteNameGUI();

        GUILayout.Space(mFuncSpace);
        GUI.backgroundColor = Color.blue;
        __ReplaceSpriteNameGUI();

        GUILayout.EndVertical();

        GUI.backgroundColor = tmpBGColor;
    }

    private void __ReplaceAtlasBySpriteNameGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Replace UIAtlas by sprite name");
        GUILayout.Space(mFuncTitleSpace);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Target File Path");
        mTargetFilePath = GUILayout.TextField(mTargetFilePath);
        if (GUILayout.Button("Open"))
            mTargetFilePath = EditorUtility.OpenFilePanel("Open AtlasTraversal", Application.streamingAssetsPath + "/Atlas/", "csv");
        GUILayout.EndHorizontal();

        if (mCanReplace && GUILayout.Button("Replace"))
        {
            if (mTargetFilePath != "")
            {
                AtlasTraversalHelper tmpNewAltasHelper = new AtlasTraversalHelper();
                tmpNewAltasHelper.Load(mTargetFilePath);
                __Replace(tmpNewAltasHelper);
            }
            else
                mWarning = "File path is invalid.";
        }

        if (mWarning != "")
        {
            Color tmpOldColor = GUI.color;
            GUI.color = Color.red;
            GUILayout.Label("Warning : " + mWarning);
            GUI.color = tmpOldColor;
        }

        GUILayout.EndVertical();
    }
    private void __Replace(AtlasTraversalHelper newAtlasHelper)
    {
        if (!mCanReplace)
            return;

        object[] tmpSel = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
        if (tmpSel == null || tmpSel.Length <= 0)
        {
            EditorUtility.DisplayDialog("Warning", "You must select a prefab or folder at \"Project\" panel.", "Ok");
            return;
        }

        mCanReplace = false;

        for (int i = 0, count = tmpSel.Length; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("Replace atlas by sprite name", i + "/" + count, (float)i / (float)count);

            GameObject tmpGO = tmpSel[i] as GameObject;
            __Check(tmpGO, newAtlasHelper);
        }
        EditorUtility.ClearProgressBar();

        mCanReplace = true;
        EditorUtility.DisplayDialog("Congratulations", "Replace prefab success.", "Ok");
    }
    private void __Check(GameObject go, AtlasTraversalHelper newAtlasHelper)
    {
        if (go == null)
            return;

        UISprite tmpSprite = go.GetComponent<UISprite>();
        if (tmpSprite != null)
            __ReplacePrefabSprite(tmpSprite, newAtlasHelper);
        foreach (Transform tmpTrans in go.transform)
            __Check(tmpTrans.gameObject, newAtlasHelper);
    }

    /// <summary>
    /// 替换UISprite
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="oldAtlasHelper"></param>
    /// <param name="newAtlasHelper"></param>
    private void __ReplacePrefabSprite(UISprite sprite, AtlasTraversalHelper newAtlasHelper)
    {
        if (sprite == null || newAtlasHelper == null)
            return;
        //不考虑Atlas是否为null
//         if (sprite.atlas == null)
//             return;

        string tmpSpriteName = sprite.spriteName;
        AtlasData tmpNewAtlasData = newAtlasHelper.GetAtlasDataBySpriteName(tmpSpriteName);
        if (tmpNewAtlasData == null)
            return;
        GameObject tmpNewAtlas = AssetDatabase.LoadAssetAtPath(tmpNewAtlasData.AtlasPath, typeof(GameObject)) as GameObject;
        if (tmpNewAtlas == null)
            return;
        sprite.atlas = tmpNewAtlas.GetComponent<UIAtlas>();
    }


    private void __ReplaceSpriteNameGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Replace sprite name");
        GUILayout.Space(mFuncTitleSpace);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Source sprite name");
        mSrcSpriteName = GUILayout.TextField(mSrcSpriteName);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Destination sprite name");
        mDstSpriteName = GUILayout.TextField(mDstSpriteName);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Destination UIAtlas");
        mDstAtlas = EditorGUILayout.ObjectField(mDstAtlas, typeof(UIAtlas), false) as UIAtlas;
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Replace"))
        {
            if (mSrcSpriteName != "" && mDstSpriteName != "" && mDstAtlas != null)
                __ReplaceSpriteName();
            else
                EditorUtility.DisplayDialog("Warning", "Source name, destination name and destination UIAtlas required.", "Ok");
        }

        GUILayout.EndVertical();
    }

    private void __ReplaceSpriteName()
    {
        object[] tmpSel = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
        if (tmpSel == null || tmpSel.Length <= 0)
        {
            EditorUtility.DisplayDialog("Warning", "You must select a prefab or folder at \"Project\" panel.", "Ok");
            return;
        }

        for (int i = 0, count = tmpSel.Length; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("Replace sprite name progress", i + "/" + count, (float)i / (float)count);

            GameObject tmpGO = tmpSel[i] as GameObject;
            if (tmpGO == null)
                continue;
            __CheckSpriteName(tmpGO);
        }
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("Congratulations", "Replace sprite name from \"" + mSrcSpriteName + "\" to \"" + mDstSpriteName + "\" success", "Ok");
    }
    private void __CheckSpriteName(GameObject go)
    {
        if (go == null)
            return;

        UISprite tmpSprite = go.GetComponent<UISprite>();
        if (tmpSprite != null && tmpSprite.spriteName == mSrcSpriteName &&  
            mDstAtlas != null)
        {
            tmpSprite.atlas = mDstAtlas;
            tmpSprite.spriteName = mDstSpriteName;
        }
        foreach (Transform tmpTrans in go.transform)
            __CheckSpriteName(tmpTrans.gameObject);
    }
}
