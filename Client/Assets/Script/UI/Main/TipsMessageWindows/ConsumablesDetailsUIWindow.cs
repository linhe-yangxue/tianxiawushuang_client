using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class ConsumablesDetailsUIWindow : tWindow
{
	public override void Init()
	{
		base.Init();

		EventCenter.Self.RegisterEvent("Button_consumables_details_close_button", new DefineFactoryLog<Button_consumables_details_close_button>());
        EventCenter.Self.RegisterEvent("Button_consumables_details_window", new DefineFactoryLog<Button_consumables_details_close_button>());
    }
	
	public override void Open(object param)
	{
		base.Open(param);
		
		Refresh(param);
	}
	
	public override bool Refresh(object param)
	{
		int itemTid = (int)param;
		GameObject obj = GameCommon.FindObject (mGameObjUI, "item_icon_infos").gameObject;

		//图标
		GameCommon.SetOnlyItemIcon(obj, "item_icon",itemTid);
		GameCommon.FindObject(obj, "count_label").SetActive(false) ;
		GameCommon.FindObject (obj, "have_num").SetActive (false);
		//描述
		GameCommon.SetUIText(obj, "consumables_detail_label", GameCommon.GetItemDesc(itemTid));
		//物品名称
		GameCommon.SetUIText (obj,"consumables_name_label",GameCommon.GetItemName(itemTid));
		GameCommon.FindObject (obj, "consumables_name_label").GetComponent<UILabel>().color = GameCommon.GetNameColor (itemTid);
		
		return true;
	}
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_consumables_details_close_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("CONSUMBLES_DETAILS_WINDOW");		
		return true;
	}
}