using UnityEngine;
using System.Collections;
using DataTable;
using System.Collections.Generic;
using System;
using System.Linq;
using Logic;

public class EquipInfoWindow : tWindow
{
    private EquipData mCurEquipData;

	public override void Init()
	{
        EventCenter.Self.RegisterEvent("Button_equip_strengthen_button", new DefineFactory<Button_equip_strengthen_button>());
        EventCenter.Self.RegisterEvent("Button_equip_refine_button", new DefineFactory<Button_equip_refine_button>());
	}

	public override void Open(object param)
	{
		base.Open(param);

        mCurEquipData = param as EquipData;

        GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "equip_strengthen_button")).set("EQUIP_DATA", mCurEquipData);
        GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "equip_refine_button")).set("EQUIP_DATA", mCurEquipData);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);

		switch (keyIndex)
		{
		case "STRENGTHEN":
            GameCommon.ToggleTrue(GetSub("equip_strengthen_button"));
            DataCenter.CloseWindow("EQUIP_REFINE_INFO_WINDOW");
            DataCenter.OpenWindow("EQUIP_STRENGTHEN_INFO_WINDOW", mCurEquipData);			
			break;

		case "EQUIP_REFINE":
			GameCommon.ToggleTrue(GetSub("equip_refine_button"));
            DataCenter.CloseWindow("EQUIP_STRENGTHEN_INFO_WINDOW");
            DataCenter.OpenWindow("EQUIP_REFINE_INFO_WINDOW", mCurEquipData);     
			break;
		}
	}
}

class Button_equip_strengthen_button : CEvent
{
	public override bool _DoEvent ()
	{
		object data = getObject("EQUIP_DATA");
        DataCenter.CloseWindow("EQUIP_REFINE_INFO_WINDOW");
        DataCenter.OpenWindow("EQUIP_STRENGTHEN_INFO_WINDOW", data);
		return true;
	}
}

class Button_equip_refine_button : CEvent
{
	public override bool _DoEvent ()
	{
        object data = getObject("EQUIP_DATA");
		DataCenter.CloseWindow("EQUIP_STRENGTHEN_INFO_WINDOW");
        DataCenter.OpenWindow("EQUIP_REFINE_INFO_WINDOW", data);
		return true;
	}
}