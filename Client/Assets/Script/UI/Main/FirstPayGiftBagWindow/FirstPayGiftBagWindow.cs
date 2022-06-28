using UnityEngine;
using System.Collections;
using DataTable;
using Logic;

public class FirstPayGiftBagWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Register ("Button_first_pay_gift_bag_back_button", new DefineFactory<Button_first_pay_gift_bag_back_button>());
		EventCenter.Register ("Button_immediately_pay_button", new DefineFactory<Button_immediately_pay_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		Refresh(param);
	}

	public override bool Refresh (object param)
	{
		int iPetID = DataCenter.mFirstPayGiftBagConfig.GetRecord (1001)["ITEM_ID"];
		set ("PET_ID", iPetID);

		int count = DataCenter.mFirstPayGiftBagConfig.GetRecordCount () - 2;
		UIGridContainer grid = GetSub ("first_pay_gift_bag_grid").GetComponent<UIGridContainer>();
		grid.MaxCount = count;
		for(int i = 0; i < count; i++)
		{
			int iIndex = 1002 + i;
			DataRecord record = DataCenter.mFirstPayGiftBagConfig.GetRecord (iIndex);
			if(record != null)
			{
				GameObject obj = grid.controlList[i];
				UISprite icon = GameCommon.FindObject (obj, "icon").GetComponent<UISprite>();
				int iType = record["TYPE"];
				int iItemID = record["ITEM_ID"];
				int iItemNum = record["NUMBER"];

				GameCommon.SetUIText (obj, "number", "X" + iItemNum.ToString());
				GameCommon.SetItemIcon (icon, iType, iItemID);

				GameCommon.SetIconData (obj, new ItemData {mType = iType, mID = iItemID});
				if(iType == (int)ITEM_TYPE.PET)
				{
//					GameCommon.SetUIVisiable (obj, "star_level", true);
//					GameCommon.SetUIText (obj, "star_level", TableCommon.GetStringFromActiveCongfig (iItemID, "STAR_LEVEL"));
					GameCommon.SetStarLevelLabel (obj, TableCommon.GetNumberFromActiveCongfig (iItemID, "STAR_LEVEL"));
				}
				else if(iType == (int)ITEM_TYPE.EQUIP)
				{
					GameCommon.SetStarLevelLabel (obj, TableCommon.GetNumberFromRoleEquipConfig (iItemID, "STAR_LEVEL"));
				}
				else
					GameCommon.SetUIVisiable (obj, "star_level", false);
			}
			else 
				DEBUG.Log ("DataRecord of  mFirstPayGiftBagConfig is null >>>>" + iIndex.ToString ());
		}
		return true;
	}

	public override void Close ()
	{
		MonoBehaviour.Destroy (mGameObjUI);
	}
}

class  Button_first_pay_gift_bag_back_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow ("FIRST_PAY_GIFT_BAG");
		return true;
	}
}

public class Button_immediately_pay_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("FIRST_PAY_GIFT_BAG");
	
		int i = System.Convert.ToInt32 (SHOP_PAGE_TYPE.DIAMOND);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
		
		GlobalModule.ClearAllWindow();
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
		return true;
	}
}
