using UnityEditor;
using UnityEngine;
using System.Collections;

public class ChangeFont : MonoBehaviour {

	static Font mTrueTypeFont;

    [MenuItem("Edit/ChangeFont")]
	public static void ChangeFonts()
	{
        mTrueTypeFont = Resources.Load<Font>("FontsRes/FangZhengCuYuan");
        Object[] objs = Selection.GetFiltered(typeof(object),SelectionMode.DeepAssets);
        foreach (Object o in objs)
        {
            GameObject obj = o as GameObject;
            if(obj != null)
                ChangeFonts(obj.transform);
        }

        //foreach (GameObject g in Selection.gameObjects)
        //{
        //    ChangeFonts(g.transform);
        //}

	}

    public static void ChangeFonts(Transform parentTran)
	{
		foreach(Transform tran in parentTran)
		{
			if(tran != null)
			{
				UILabel label = tran.GetComponent<UILabel>();
                if (label != null && label.trueTypeFont != null && label.trueTypeFont.name == "方正粗圆简体")
				{
					label.trueTypeFont = mTrueTypeFont;
				}
				
				ChangeFonts(tran);
			}
		}
	}
}
