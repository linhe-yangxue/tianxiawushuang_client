using UnityEngine;
using System.Collections;

public class ScrollWorldMapLeft : tWindow
{
    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex) 
        {
            case "REFRESH_NEWMARK":
                RefreshNewMark();
                break;

        }
    }

    private void RefreshNewMark() 
    {
        if (mGameObjUI == null)
            return;
        GameCommon.SetNewMarkVisible(mGameObjUI, AdventureNewMarkManager.Self.LeftRewardVisible);
    }
}
