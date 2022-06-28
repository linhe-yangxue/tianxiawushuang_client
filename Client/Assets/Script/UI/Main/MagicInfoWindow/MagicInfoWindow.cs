using UnityEngine;
using System.Collections;
using DataTable;
using System.Collections.Generic;
using System;
using System.Linq;
using Logic;

public class MagicInfoWindow : tWindow
{
    private EquipData mCurItemData;
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_magic_strengthen_button", new DefineFactory<Button_magic_strengthen_button>());
        EventCenter.Self.RegisterEvent("Button_magic_refine_button", new DefineFactory<Button_magic_refine_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        mCurItemData = param as EquipData;

        GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "magic_strengthen_button")).set("EQUIP_DATA", mCurItemData);
        GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "magic_refine_button")).set("EQUIP_DATA", mCurItemData);
    }
    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "STRENGTHEN":
                GameCommon.ToggleTrue(GetSub("magic_strengthen_button"));
                DataCenter.CloseWindow("MAGIC_REFINE_INFO_WINDOW");
                DataCenter.OpenWindow("MAGIC_STRENGTHEN_INFO_WINDOW", mCurItemData);
                break;
            case "MAGIC_REFINE":
                GameCommon.ToggleTrue(GetSub("magic_refine_button"));
                DataCenter.CloseWindow("MAGIC_STRENGTHEN_INFO_WINDOW");
                DataCenter.OpenWindow("MAGIC_REFINE_INFO_WINDOW", mCurItemData);
                break;
        }
    }
}

class Button_magic_strengthen_button : CEvent
{
    public override bool _DoEvent()
    {
        object data = getObject("EQUIP_DATA");
        DataCenter.CloseWindow("MAGIC_REFINE_INFO_WINDOW");
        DataCenter.OpenWindow("MAGIC_STRENGTHEN_INFO_WINDOW", data);
        return true;
    }
}

class Button_magic_refine_button : CEvent
{
    public override bool _DoEvent()
    {
        object data = getObject("EQUIP_DATA");
        DataCenter.CloseWindow("MAGIC_STRENGTHEN_INFO_WINDOW");
        DataCenter.OpenWindow("MAGIC_REFINE_INFO_WINDOW", data);
        return true;
    }
}