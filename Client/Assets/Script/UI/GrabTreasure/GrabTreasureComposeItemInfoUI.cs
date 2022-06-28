using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class GrabTreasureComposeItemInfoWindow : tWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_grab_compose_item_info_ok_button", new DefineFactoryLog<Button_grab_compose_item_info_close_button>());
        EventCenter.Self.RegisterEvent("Button_grab_compose_item_info_close_button", new DefineFactoryLog<Button_grab_compose_item_info_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        int itemTid = (int)param;

        DataRecord tmpItemConfig = DataCenter.mRoleEquipConfig.GetRecord(itemTid);
        if (tmpItemConfig == null)
        {
            GrabTreasureWindow.LogError("找不到物品" + itemTid.ToString() + "配置");
            return true;
        }

        //图标
        GameCommon.SetItemIcon(GetSub("item_icon_parent"), new ItemData() { mID = itemTid, mType = (int)PackageManager.GetItemTypeByTableID(itemTid) });

        //描述
        GameCommon.SetUIText(mGameObjUI, "treasure_label", tmpItemConfig.getData("DESCRIPTION"));

        return true;
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_grab_compose_item_info_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GRABTREASURE_COMPOSE_ITEM_INFO_WINDOW");

        return true;
    }
}
