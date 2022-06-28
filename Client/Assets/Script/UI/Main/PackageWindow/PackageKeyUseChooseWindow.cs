using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using DataTable;

public class PackageKeyUseChooseWindow : tWindow
{
	private int mUseCount = 0;
	ConsumeItemData data;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_key_use_choose_add_btn_left", new DefineFactoryLog<Button_key_use_choose_add_btn_left>());
		EventCenter.Self.RegisterEvent("Button_key_use_choose_add_btn_right", new DefineFactoryLog<Button_key_use_choose_add_btn_right>());
		EventCenter.Self.RegisterEvent("Button_key_use_choose_ten_btn_left", new DefineFactoryLog<Button_key_use_choose_ten_btn_left>());
		EventCenter.Self.RegisterEvent("Button_key_use_choose_ten_btn_right", new DefineFactoryLog<Button_key_use_choose_ten_btn_right>());

		EventCenter.Self.RegisterEvent("Button_key_use_choose_reward_close_button", new DefineFactory<Button_key_use_choose_reward_close_button>());
		EventCenter.Self.RegisterEvent("Button_key_use_choose_reward_ok_button", new DefineFactory<Button_key_use_choose_reward_ok_button>());
		EventCenter.Self.RegisterEvent("Button_choose_reward_icon_btn", new DefineFactory<Button_choose_reward_icon_btn>());
	}
	
	public override void Open(object param)
	{
		base.Open (param);
		mUseCount = 1;      //打开界面时使用数量为1
		data = (ConsumeItemData)param;
		DataRecord r = DataCenter.mConsumeConfig.GetRecord(data.tid);    
		UpdateUI(int.Parse(r["ITEM_GROUP_ID"]));		
	}
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		switch (keyIndex)
		{
		case "SEND_PET_TID":
			ItemDataBase itemDate = (ItemDataBase)objVal;
			GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "key_use_choose_reward_ok_button")).set("GET_PET_TID", itemDate);
			break;
		case "ADD_COUNT":
			_AddCount((int)objVal);
			break;
		}
	}
	/// <summary>
	/// 加物品数量
	/// </summary>
	/// <param name="count"></param>
	private void _AddCount(int count)
	{
		mUseCount += count;
		mUseCount = Mathf.Max(mUseCount, 1);
		if (data.itemNum != 0)
			mUseCount = Mathf.Min(mUseCount, data.itemNum);
		_RefreshUseInfo();
	}
	/// <summary>
	/// 刷新使用信息
	/// </summary>
	
	private void _RefreshUseInfo()
	{
		//使用数量
		GameObject tmpUseCount = GetSub("num_bg");
		GameCommon.SetUIText(tmpUseCount, "num", mUseCount.ToString());
		
		//设置使用按钮数据
		NiceData tmpBtnUseData = GameCommon.GetButtonData(mGameObjUI, "key_use_choose_reward_ok_button");
		if (tmpBtnUseData != null)
		{
			tmpBtnUseData.set("ITEM_COUNT", mUseCount);
			tmpBtnUseData.set("CONSUME_ITEM_DATA", data);
		}
	}
	void _SetHaveCount()
	{
		GameObject haveLabelObj = GameCommon.FindObject (mGameObjUI, "have_label");
		GameCommon.SetUIText (haveLabelObj, "num_label", data.itemNum.ToString ());
	}
	
	public void UpdateUI(int iGroupId)
	{
		_RefreshUseInfo();
		_SetHaveCount();
		List<ItemDataBase> items = GameCommon.GetItemGroup(iGroupId,false);

		UIGridContainer keyUseChooseRewardGrid = GameCommon.FindObject (mGameObjUI, "key_use_choose_reward_group").GetComponent<UIGridContainer>();
		if (keyUseChooseRewardGrid == null) return;

		keyUseChooseRewardGrid.MaxCount = items.Count;
		for(int i = 0; i < keyUseChooseRewardGrid.MaxCount; i++)
		{
			GameObject obj = keyUseChooseRewardGrid.controlList[i];
			UISprite sprite = obj.transform.Find ("choose_reward_icon_btn/icon_sprite").GetComponent<UISprite>();
			
			string strAtlasName = GameCommon.GetItemStringField(items[i].tid, GET_ITEM_FIELD_TYPE.ATLAS_NAME);
			string strSpriteName = GameCommon.GetItemStringField(items[i].tid, GET_ITEM_FIELD_TYPE.SPRITE_NAME);
			GameCommon.SetIcon (sprite, strAtlasName, strSpriteName);
			
			int iPetNum = items[i].itemNum;
			GameCommon.GetButtonData(GameCommon.FindObject(obj, "choose_reward_icon_btn")).set("SET_PET_TID", items[i]);
			
			GameCommon.SetUIText(obj, "name_label", GameCommon.GetItemName(items[i].tid));
			
			// 根据品质类型设置名字的颜色
			UILabel nameLabel = GameCommon.FindComponent<UILabel>(obj, "name_label");
			nameLabel.color = GameCommon.GetNameColor(items[i].tid);
			
			GameCommon.SetUIText(obj, "count_label", ("x" + items[i].itemNum));
			
			// 默认选中第一个宝箱
			if (i == 0) 
			{
				DataCenter.SetData("KEY_USE_CHOOSE_REWARDS_WINDOW", "SEND_PET_TID", items[0]);
				GameCommon.FindComponent<UIToggle>(keyUseChooseRewardGrid.controlList[0], "choose_reward_icon_btn").value = true;
			}
		}
	}
}
public class Button_key_use_choose_reward_close_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("KEY_USE_CHOOSE_REWARDS_WINDOW");
		return true;
	}
}
public class Button_key_use_choose_reward_ok_button : CEvent
{
	public override bool _DoEvent()
	{
		string callBy = (string)getObject("CALL_BY"); // 按钮标志位
		int iCount = (int)getObject ("ITEM_COUNT");
		if (callBy.Equals("PackageConsumeWindow"))
		{
			ItemDataBase selectedItem = (ItemDataBase)getObject("GET_PET_TID");
			selectedItem.itemNum = iCount;
			ConsumeItemData data = (ConsumeItemData)getObject ("CONSUME_ITEM_DATA");
			ItemDataBase __item = new ItemDataBase { itemId = data.itemId, tid = data.tid, itemNum = iCount };
			DataRecord r = DataCenter.mConsumeConfig.GetRecord(data.tid);    
			string notice = r["AWARD_NOTICE"];
			PackageConsumeWindow tmpWin = DataCenter.GetData("PACKAGE_CONSUME_WINDOW") as PackageConsumeWindow;
			if (selectedItem == null)
			{
				DataCenter.OpenMessageWindow("请选择物品");
			}
			else
			{
				if (tmpWin != null)
					SelectItemWindow.onCommit = x => GlobalModule.DoCoroutine(tmpWin.DoSendSelectResult(__item, x, notice));      

				if (SelectItemWindow.onCommit != null)
				{

					SelectItemWindow.onCommit(selectedItem);
					SelectItemWindow.onCommit = null;
				}
			}			
			set("CALL_BY", null); // 标志位初始化
			DataCenter.CloseWindow("KEY_USE_CHOOSE_REWARDS_WINDOW");
			return true;
		}
		return false;
	}
}
public class Button_choose_reward_icon_btn : CEvent
{
	public override bool _DoEvent()
	{
		ItemDataBase itemDate = (ItemDataBase)getObject ("SET_PET_TID");
		DataCenter.SetData("KEY_USE_CHOOSE_REWARDS_WINDOW", "SEND_PET_TID", itemDate);
		return true;
	}
}
/// <summary>
/// 物品减1
/// </summary>
class Button_key_use_choose_add_btn_left : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("KEY_USE_CHOOSE_REWARDS_WINDOW", "ADD_COUNT", -1);		
		return true;
	}
}
/// <summary>
/// 物品加1
/// </summary>
class Button_key_use_choose_add_btn_right : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("KEY_USE_CHOOSE_REWARDS_WINDOW", "ADD_COUNT", 1);		
		return true;
	}
}
/// <summary>
/// 物品减10
/// </summary>
class Button_key_use_choose_ten_btn_left : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("KEY_USE_CHOOSE_REWARDS_WINDOW", "ADD_COUNT", -10);		
		return true;
	}
}
/// <summary>
/// 物品加10
/// </summary>
class Button_key_use_choose_ten_btn_right : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("KEY_USE_CHOOSE_REWARDS_WINDOW", "ADD_COUNT", 10);		
		return true;
	}
}