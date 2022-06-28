using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class GrabTreasureComposeResultWindow : tWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_grab_compose_ok_button", new DefineFactoryLog<Button_grab_compose_close_button>());
        EventCenter.Self.RegisterEvent("Button_grab_compose_close_button", new DefineFactoryLog<Button_grab_compose_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        int itemTid = (int)param;

       /* DataRecord tmpItemConfig = DataCenter.mRoleEquipConfig.GetRecord(itemTid);
        if (tmpItemConfig == null)
        {
            GrabTreasureWindow.LogError("找不到物品" + itemTid.ToString() + "配置");
            return true;
        }
        */
        //图标
        GameCommon.SetOnlyItemIcon(GetSub("item_icon_parent"), "item_icon",itemTid);
        GameCommon.FindObject(mGameObjUI, "count_label").SetActive(false) ;
        //描述
        GameCommon.SetUIText(mGameObjUI, "treasure_label", GameCommon.GetItemDesc(itemTid));
		//物品名称
		GameCommon.SetUIText (mGameObjUI,"treasure_name_label",GameCommon.GetItemName(itemTid));


        return true;
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_grab_compose_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GRABTREASURE_COMPOSE_RESULT_WINDOW");
        DataCenter.SetData("GRABTREASURE_WINDOW", "REFRESH", null);

        return true;
    }
}
