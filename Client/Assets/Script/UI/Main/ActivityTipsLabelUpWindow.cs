using UnityEngine;
using System.Collections;

/// <summary>
/// 获得奖励的渐隐弹窗
/// </summary>
public class ActivityTipsLabelUpWindow : tWindow
{
    public System.Action mCallback = null;
    public override void Open(object param)
    {
        base.Open(param);
        if (param != null && param is ItemData) 
        {
            ItemData _item = (ItemData)param;
            GameCommon.SetResIcon(mGameObjUI,"item_icon",_item.mID,false);
            GameCommon.FindComponent<UILabel>(mGameObjUI, "item_num_label").text = "x " + _item.mNumber.ToString();
            RoleLogicData.Self.AddDiamond(_item.mNumber);
            DataCenter.SetData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND", null);
            GlobalModule.DoLater(() => { CloseAndDoAction(); }, 1.5f);
        }
    }

    private void CloseAndDoAction() 
    {
        Close();
        if (mCallback != null) 
        {
            mCallback();
            mCallback = null;
        }
    }
}
