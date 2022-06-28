using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using DataTable;

public class AstrologyRewardWindow : tWindow
{
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_astrology_reward_close_button", new DefineFactory<Button_astrology_reward_close_button>());
		EventCenter.Self.RegisterEvent("Button_astrology_reward_ok_button", new DefineFactory<Button_astrology_reward_ok_button>());
		EventCenter.Self.RegisterEvent ("Button_reward_icon_btn", new DefineFactory<Button_reward_icon_btn>());
	}

	public override void Open(object param)
	{
		base.Open (param);
		UpdateUI((int)param);
        
	}
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		switch (keyIndex)
		{
		case "SEND_MESSAGE_PET_TID":
			ItemDataBase items = (ItemDataBase)objVal;
			NetManager.RequestPointStarRewards(items);
			AstrologyUIWindow _tWindow = DataCenter.GetData("ASTROLOGY_UI_WINDOW") as AstrologyUIWindow;
			UIImageButton btn = GameCommon.FindObject(_tWindow.mGameObjUI, "lighten_Button").GetComponent<UIImageButton>();
			if(btn != null)
				btn.isEnabled = false;
			break;
		case "SEND_PET_TID":
			ItemDataBase itemDate = (ItemDataBase)objVal;
			GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "astrology_reward_ok_button")).set("GET_PET_TID", itemDate);
            break;
		}
	}
	
	public void UpdateUI(int iGroupId)
	{
		List<ItemDataBase> items = GameCommon.GetItemGroup(iGroupId,false);

        // 判空大法好  ——Said by Chen Liang
		UIGridContainer astrologyRewardGrid = GameCommon.FindObject (mGameObjUI, "astrology_reward_group").GetComponent<UIGridContainer>();
        if (astrologyRewardGrid == null) return;

        UISprite astrologyRewardUpBg = GameCommon.FindComponent<UISprite>(mGameObjUI, "astrology_reward_up_bg");
        if (astrologyRewardUpBg == null) return;

        UISprite astrologyRewardUpBgSprite1 = GameCommon.FindComponent<UISprite>(astrologyRewardUpBg.gameObject, "sprite_1");
        if (astrologyRewardUpBgSprite1 == null) return;
        
        UISprite astrologyRewardUpBgSprite2 = GameCommon.FindComponent<UISprite>(astrologyRewardUpBg.gameObject, "sprite_2");
        if (astrologyRewardUpBgSprite2 == null) return;
        
        Transform closeBtn = GameCommon.FindComponent<Transform>(mGameObjUI, "astrology_reward_close_button");
        if (closeBtn == null) return;

        // 根据MaxCount的数量改变astrology_reward_window的宽度，同时改变closeButton的位置
        astrologyRewardGrid.MaxCount = items.Count;
        if (items.Count == 1 || items.Count == 2 || items.Count == 3)
        {
            astrologyRewardUpBg.width = 409;
            astrologyRewardUpBgSprite1.width = 377;
            astrologyRewardUpBgSprite2.width = 356;
            closeBtn.transform.localPosition = new Vector3(191f, 165f, 0);
        }
        else
        {
            astrologyRewardUpBg.width = 509;
            astrologyRewardUpBgSprite1.width = 477;
            astrologyRewardUpBgSprite2.width = 456;
            closeBtn.transform.localPosition = new Vector3(243f, 165f, 0);
        }

        // 调整astrology_reward_group中UIGridContainer的ControlList每个元素的间隔
        float cellWidth = 88;
        if (items.Count == 2) // 对两个元素时的间隔进行单独调整
        {
            cellWidth = astrologyRewardUpBgSprite2.width / (items.Count + 0.5f);
        }
        else
        {
            cellWidth = astrologyRewardUpBgSprite2.width / items.Count;
        }
        float x = cellWidth * (items.Count - 1) / 2;
        astrologyRewardGrid.CellWidth = cellWidth;
        astrologyRewardGrid.transform.localPosition = new Vector3(-x, 0, 0);

        //if (items.Count == 1)  
        //{
        //    astrologyRewardGrid.transform.localPosition = new Vector3(0, 20, 0);
        //}
        //else if (items.Count == 2)
        //{
        //    astrologyRewardGrid.transform.localPosition = new Vector3(-70, 20, 0);
        //}
        //else
        //{
        //    astrologyRewardGrid.transform.localPosition = new Vector3(-210, 20, 0);
        //}
		for(int i = 0; i < astrologyRewardGrid.MaxCount; i++)
		{
			GameObject obj = astrologyRewardGrid.controlList[i];
			UISprite sprite = obj.transform.Find ("reward_icon_btn/icon_sprite").GetComponent<UISprite>();

			string strAtlasName = GameCommon.GetItemStringField(items[i].tid, GET_ITEM_FIELD_TYPE.ATLAS_NAME);
            string strSpriteName = GameCommon.GetItemStringField(items[i].tid, GET_ITEM_FIELD_TYPE.SPRITE_NAME);
			GameCommon.SetIcon (sprite, strAtlasName, strSpriteName);

			int iPetNum = items[i].itemNum;
			GameCommon.GetButtonData(GameCommon.FindObject(obj, "reward_icon_btn")).set("SET_PET_TID", items[i]);

            GameCommon.SetUIText(obj, "name_label", GameCommon.GetItemName(items[i].tid));

            // 根据品质类型设置名字的颜色
            UILabel nameLabel = GameCommon.FindComponent<UILabel>(obj, "name_label");
            nameLabel.color = GameCommon.GetNameColor(items[i].tid);
            
            GameCommon.SetUIText(obj, "count_label", ("x" + items[i].itemNum));

            // 默认选中第一个宝箱
            if (i == 0) 
            {
                DataCenter.SetData("ASTROLOGY_REWARD_WINDOW", "SEND_PET_TID", items[0]);
                GameCommon.FindComponent<UIToggle>(astrologyRewardGrid.controlList[0], "reward_icon_btn").value = true;
            }
		}
	}
}
public class Button_astrology_reward_close_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("ASTROLOGY_REWARD_WINDOW");
		return true;
	}
}
public class Button_astrology_reward_ok_button : CEvent
{
	public override bool _DoEvent()
	{
        string callBy = (string)getObject("CALL_BY"); // 按钮标志位
        if (callBy.Equals("AstrologyUIWindow"))
        {
            ItemDataBase itemDate = (ItemDataBase)getObject("GET_PET_TID");
            DataCenter.SetData("ASTROLOGY_REWARD_WINDOW", "SEND_MESSAGE_PET_TID", itemDate);

            set("CALL_BY", null); // 标志位初始化
            DataCenter.CloseWindow("ASTROLOGY_REWARD_WINDOW");
            return true;
        }
        if (callBy.Equals("PackageConsumeWindow"))
        {
            ItemDataBase selectedItem = (ItemDataBase)getObject("GET_PET_TID");

            if (selectedItem == null)
            {
                DataCenter.OpenMessageWindow("请选择物品");
            }
            else
            {
                if (SelectItemWindow.onCommit != null)
                {
                    SelectItemWindow.onCommit(selectedItem);
                    SelectItemWindow.onCommit = null;
                }
            }
            
            set("CALL_BY", null); // 标志位初始化
            DataCenter.CloseWindow("ASTROLOGY_REWARD_WINDOW");
            return true;
        }
        return false;
	}
}
public class Button_reward_icon_btn : CEvent
{
	public override bool _DoEvent()
	{
		ItemDataBase itemDate = (ItemDataBase)getObject ("SET_PET_TID");
		DataCenter.SetData("ASTROLOGY_REWARD_WINDOW", "SEND_PET_TID", itemDate);
		return true;
	}
}