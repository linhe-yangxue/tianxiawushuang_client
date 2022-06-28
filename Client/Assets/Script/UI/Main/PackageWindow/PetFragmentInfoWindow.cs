using UnityEngine;
using System.Collections;

public class PetFragmentInfoWindow : FragmentInfoWindow
{
    protected override void UpdateInfoUI()
    {
        base.UpdateInfoUI();

        GameObject obj = GetSub("fragment_info");
        if (mCurItemData != null && obj != null)
        {
            // set element icon
            int iElementIndex = TableCommon.GetNumberFromActiveCongfig(mCurItemData.mComposeItemTid, "ELEMENT_INDEX");
            GameCommon.SetElementIcon(obj, iElementIndex);

            // set element fragment icon
            GameCommon.SetElementFragmentIcon(obj, iElementIndex);

            // 设置宠物类型名称
            string strPetTypeName = GameCommon.GetAttackType(mCurItemData.mComposeItemTid);
            GameCommon.SetUIText(obj, "defence_type", strPetTypeName);
        }        
    }
	
}
