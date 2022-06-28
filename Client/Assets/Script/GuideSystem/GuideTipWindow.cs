using UnityEngine;
using System.Collections;


public class GuideTipWindow : tWindow
{
    public override void Open(object param)
    {
        base.Open(param);

        if(param is Vector3)
        {
            mGameObjUI.transform.localPosition = (Vector3)param;
        }

        mGameObjUI.transform.localScale = Vector3.one * 0.001f;
        TweenScale.Begin(mGameObjUI, 0.2f, Vector3.one);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "TIP":
                SetText("label", (string)objVal);
                break;
        }
    }
}