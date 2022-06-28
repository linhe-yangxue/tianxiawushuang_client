using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;

public class FeatsShopData {
	public int index;
	public int tabId;
	public string tabName;
	public int tid;
	public int itemNum;
	public int itemShowLevel;
	public int vipLevelDisplay;
	public int costType1;
	public int costNum1;
	public int costType2;
	public int costNum2;
	public int buyNum;
	public int refreshByDay;
	public int iLeftNum;
	public int iHaveBuyNum;
}

public class FeatsShopWindow : tWindow {

	public static SC_ResponseFeatsShopQuery rfsq;

	public class FeatsShopDeliverData {
		public List<FeatsShopData> feats;
		public object obj;
	}

	UIGridContainer btnGrid, contentGrid;

	public override void Init () {
		base.Init ();
		EventCenter.Self.RegisterEvent("Button_feat_shop_tab_page", new DefineFactory<Button_feat_shop_tab_page>());
		EventCenter.Self.RegisterEvent("Button_feats_close_button", new DefineFactory<Button_feats_close_button>());
		EventCenter.Self.RegisterEvent("Button_feats_buy_button", new DefineFactory<Button_feats_buy_button>());
		EventCenter.Self.RegisterEvent("Button_shop_feats_window_back_btn", new DefineFactory<Button_shop_feats_window_back_btn>());
	}


	public override void Open (object param) {
		base.Open (param);
		btnGrid = GetUIGridContainer("feats_title_buttons");
		contentGrid = GetUIGridContainer("feats_goods_grid");

		FeatsShopNet.RequestFeatsShopQuery(RequestFeatsShopQuerySuccess, ResponseFeatsShopQueryFail);
		//
	}

	void RequestFeatsShopQuerySuccess(string txt) {
		rfsq = JCode.Decode<SC_ResponseFeatsShopQuery> (txt);

		Refresh(rfsq);
		DEBUG.Log("RequestFeatsShopQuerySuccess:" + txt);
	}

	void ResponseFeatsShopQueryFail(string txt) {

		DEBUG.Log("ResponseFeatsShopQueryErr:" + txt);
	}


	public override bool Refresh (object param) {

		if(param is SC_ResponseFeatsShopQuery) {
			int maxTabs = 0;
			List<FeatsShopData> featsTabData = TableCommon.GetFeatsInfo(1, out maxTabs);

			FeatsShopDeliverData fsdd = new FeatsShopDeliverData();
			fsdd.feats = featsTabData;
			fsdd.obj = param;

			string[] tabNames = TableCommon.GetFeatsTabName(maxTabs);
			btnGrid.MaxCount = maxTabs;
			for(int i=0; i < maxTabs; i++) { 
				GameObject obj = btnGrid.controlList[i];
				if(i == 0) {
					obj.GetComponent<UIToggle>().startsActive = true;
					GameCommon.ToggleTrue(obj);
				}
				
				GameCommon.SetUIText(obj, "LabelBackground", tabNames[i]);
				GameCommon.SetUIText(obj, "LabelCheckmark", tabNames[i]);
				obj.name = "feat_shop_tab_page";
				obj.GetComponent<UIButtonEvent>().mData.set("TAB_ID", i + 1);
			}
			DataCenter.SetData("FEATS_SHOP_WINDOW", "FIRST_OPEN", fsdd);

			return true;
		}

		return false;
	}

    public void SortTabData(List<FeatsShopData> tabDataOld, SC_ResponseFeatsShopQuery rQuery, out List<FeatsShopData> tabDataNew)
    {
        List<FeatsShopData> tabDataTemp = new List<FeatsShopData>();
        foreach (var data in tabDataOld)
        {
            FeatsStruct[] cs = rQuery.buyState;
            bool isBuyed = false;
            if (data.buyNum != 0)
            {
                foreach (FeatsStruct cc in cs)
                {
                    if (cc.index == data.index && data.buyNum - cc.buyNum == 0)
                    {
                        isBuyed = true;
                        break;
                    }
                }
            }
            if (!isBuyed)
            {
                tabDataTemp.Add(data);
            }
        }

        foreach (var data in tabDataOld)
        {
            FeatsStruct[] cs = rQuery.buyState;
            foreach (FeatsStruct cc in cs)
            {
                if (cc.index == data.index && data.buyNum - cc.buyNum == 0)
                {
                    tabDataTemp.Add(data);
                    break;
                }
            }
        }

        tabDataNew = tabDataTemp;
    }

	public override void onChange (string keyIndex, object objVal) {
		base.onChange (keyIndex, objVal);

		List<FeatsShopData> tabData;
		SC_ResponseFeatsShopQuery rQuery;
		
		if(objVal.GetType() != new FeatsShopDeliverData().GetType()) return;
		
		if(objVal != null && objVal.GetType() == new FeatsShopDeliverData().GetType()) {
			FeatsShopDeliverData fsdd = (FeatsShopDeliverData) objVal;
			rQuery = fsdd.obj as SC_ResponseFeatsShopQuery;
            List<FeatsShopData> tabDataTemp = fsdd.feats;
            SortTabData(tabDataTemp, rQuery, out tabData); //买完的置底～
		} else {
			DEBUG.LogError("get prestige data null");
			return;
		}
		
		if(contentGrid.MaxCount > 0) contentGrid.MaxCount = 0;
		contentGrid.MaxCount = tabData.Count;


		switch(keyIndex) {
		case "FIRST_OPEN":
		{
			for( int i = 0; i < tabData.Count; i++) {
				GameObject go = contentGrid.controlList[i];
				FillUIItemDataContent(go, tabData, i);
				UpdateItemNetInfo(go, tabData, i, rQuery);
			}
			break;
		}
		case "REFRESH_PAGE":
		{
			for( int i = 0; i < tabData.Count; i++) {
				GameObject go = contentGrid.controlList[i];
				FillUIItemDataContent(go, tabData, i);
				UpdateItemNetInfo(go, tabData, i, rQuery);
			}
			break;
		}
		}

		RefreshHeadInfo();
		contentGrid.repositionNow = true;
	}

	void FillUIItemDataContent(GameObject item, List<FeatsShopData> data, int i) {
		string itemName = GameCommon.GetItemName(data[i].tid);
		GameCommon.SetUIText(item, "goods_name_label", itemName);

		//icon_1,icon_2
		GameObject _cost_icon_1 = GameCommon.FindObject (item,"title_feats");
		GameObject _cost_icon_2 = GameCommon.FindObject (item,"title_cost2");
		// 设置消耗货币类型1
		int _consumeTid_1 = data [i].costType1;
		if (_consumeTid_1 != 0) 
		{
			//GameCommon.SetOnlyItemIcon(_cost_icon_1,data[i].costType1);
            GameCommon.SetResIcon(item, "title_feats", data[i].costType1, false);
			GameCommon.SetUIText (item, "need_feats_num", data [i].costNum1.ToString ());
		}
		else 
		{
			GameCommon.SetUIVisiable(item,"need_feats",false);
		}
		// 设置消耗货币类型2
		int _consumeTid_2 = data [i].costType2;
		if (_consumeTid_2 != 0) 
		{
            //GameCommon.SetOnlyItemIcon(_cost_icon_2,data[i].costType2);
            GameCommon.SetResIcon(item, "title_cost2", data[i].costType2, false);
			GameCommon.SetUIText (item, "need_cost2_num", data [i].costNum2.ToString ());
		} 
		else 
		{
			GameCommon.SetUIVisiable(item,"need_cost_2",false);
		}
		GameCommon.SetUIText(item, "number_label", "x" + data[i].itemNum.ToString());
		// icon setting
		UISprite icon = GameCommon.FindComponent<UISprite>(item, "icon_goods");
		string altsName = TableCommon.GetStringFromRoleEquipConfig(data[i].tid, "ICON_ATLAS_NAME");
		string spriteName = TableCommon.GetStringFromRoleEquipConfig(data[i].tid, "ICON_SPRITE_NAME");
		// remove icon left top tag
		/*
		GameCommon.FindObject(item, "sprite").SetActive(false);
		GameCommon.SetIcon(icon, altsName, spriteName);
		*/
        GameCommon.SetOnlyItemIcon(GameCommon.FindObject(item,"icon_goods"), data[i].tid);
		AddButtonAction (GameCommon.FindObject (item,"icon_goods"), () => GameCommon.SetItemDetailsWindow (data[i].tid));
		// icon end
		// set item num
		GameCommon.SetUIText (item,"item_num_lbl","x" + data[i].itemNum.ToString());
		UIButtonEvent ube = GameCommon.FindComponent<UIButtonEvent>(item, "feats_buy_button");
		ube.mData.set("ITEM_DATA", data[i]);
	}

	void RefreshHeadInfo() {
		int usrFeatsNum = RoleLogicData.Self.battleAchv;
		GameCommon.SetUIText(mGameObjUI, "need_feats_head_title_num", GameCommon.ShowNumUI(usrFeatsNum));
	}

	void UpdateItemNetInfo(GameObject item, List<FeatsShopData> data, int i, SC_ResponseFeatsShopQuery rfsq) {
		int configIndex = data[i].index;
		FeatsStruct[] cs = rfsq.buyState;
		// 在这种情况下，找需要更新的条目信息
		if(cs != null && cs.Length > 0) {
			foreach(FeatsStruct cc in cs) {
				// 找到了需要次数限制的条目
				if(cc.index == configIndex) {
					//设置更新道具的次数
					int count = data[i].buyNum - cc.buyNum;
					data[i].iLeftNum = count;
					data[i].iHaveBuyNum = cc.buyNum;
					if(count > 0) {
						// GameCommon.SetUIText(item, "number_label", count.ToString());
						GameCommon.FindObject(item, "goods_limit_label02").SetActive(true);
						GameCommon.SetUIText(item, "goods_limit_label02", "可以购买" + count + "次");
						GameCommon.FindObject(item, "goods_limit_label03").SetActive(false);
						return;
					} else {
						GameCommon.FindObject(item, "goods_limit_label02").SetActive(false);
						GameCommon.FindObject(item, "goods_limit_label03").SetActive(true);
						
						UIImageButton ube = GameCommon.FindComponent<UIImageButton>(item, "feats_buy_button");
						ube.isEnabled = false;
						return;
					}
				}else {
					// 该物品有次数限制
					if(data[i].buyNum > 0) {
						// 采用默认的一次限制
						GameCommon.FindObject(item, "goods_limit_label02").SetActive(true);
						GameCommon.SetUIText(item, "goods_limit_label02", "可以购买" + data[i].buyNum + "次");
						GameCommon.FindObject(item, "goods_limit_label03").SetActive(false);
						data[i].iLeftNum = data[i].buyNum;
						data[i].iHaveBuyNum = 0;
					}else { //该物品为无限制次数的道具 
						GameCommon.FindObject(item, "goods_limit_label02").SetActive(false);
						GameCommon.FindObject(item, "goods_limit_label03").SetActive(false);
						data[i].iLeftNum = -1;
						data[i].iHaveBuyNum = 0;
					}
				}
			}
			//该物品默认使用配置表的信息
		} else  {
			// 该物品有次数限制
			if(data[i].buyNum > 0) {
				// 采用默认的一次限制
				GameCommon.FindObject(item, "goods_limit_label02").SetActive(true);
				GameCommon.SetUIText(item, "goods_limit_label02", "可以购买" + data[i].buyNum + "次");
				GameCommon.FindObject(item, "goods_limit_label03").SetActive(false);
			} else {//该物品为无限制次数的道具
				GameCommon.FindObject(item, "goods_limit_label02").SetActive(false);
				GameCommon.FindObject(item, "goods_limit_label03").SetActive(false);
				data[i].iLeftNum = -1;
				data[i].iHaveBuyNum = 0;
			}
		}
	}

}

class Button_feat_shop_tab_page:CEvent {

	public override bool _DoEvent () {
		int tabId = get("TAB_ID");
		int maxTabs = 0;
		List<FeatsShopData> currentTabData = TableCommon.GetFeatsInfo(tabId, out maxTabs);
		FeatsShopWindow.FeatsShopDeliverData fsdd = new FeatsShopWindow.FeatsShopDeliverData();
		fsdd.feats = currentTabData;
		fsdd.obj = FeatsShopWindow.rfsq;
		DataCenter.SetData("FEATS_SHOP_WINDOW", "REFRESH_PAGE", currentTabData);

		DEBUG.Log("tabId:" + tabId);
		return true;
	}
}

class Button_feats_close_button:CEvent {
	
	public override bool _DoEvent () {
		DataCenter.CloseWindow("FEATS_SHOP_WINDOW");

		return true;
	}
}
class Button_shop_feats_window_back_btn:CEvent {
	
	public override bool _DoEvent () 
	{
		DataCenter.CloseWindow("FEATS_SHOP_WINDOW");
		DataCenter.OpenWindow("BOSS_RAID_WINDOW");
		DataCenter.CloseWindow("SHOP_FEATS_WINDOW_BACK");
		return true;
	}
}

class Button_feats_buy_button:CEvent {
	
	FeatsShopData itemData;
	int buyNum = 1;
	
	public override bool _DoEvent () {
		itemData = get ("ITEM_DATA").mObj as FeatsShopData;
		
		int usrFeatsNum = RoleLogicData.Self.battleAchv;
		if(usrFeatsNum <= 0 || usrFeatsNum < itemData.costNum1) {
			//by chenliang
			//begin
			
			//			DataCenter.OpenMessageWindow("用户战功不足。");
			//-------------------
			//策划要求去除句号
			DataCenter.ErrorTipsLabelMessage("您的魔鳞不足");
			
			//end
			return true;
		}
		if (itemData.costNum2 > 0 && PackageManager.GetItemLeftCount (itemData.costType2) < itemData.costNum2) 
		{
			if (itemData.costType2 == (int)ITEM_TYPE.GOLD)
			{
				DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
			}
			else
			{
				DataCenter.ErrorTipsLabelMessage(GameCommon.GetItemName(itemData.costType2) + "不足!");
			}
			return true;
		}
		
		List<int> itemIdList = new List<int>();
		if (itemData.costType2 != 0)
			itemIdList.Add (GameCommon.GetItemId(itemData.costType2));

        //by chenliang
        //begin

//		MallBuyConsumeData tmpConsumeData = new MallBuyConsumeData();
//-----------------
        MallBuyConsumeData tmpConsumeData = new MallBuyConsumeData(ShopType.Boss);

        //end
		tmpConsumeData.Index = itemData.index;
		tmpConsumeData.Tid = itemData.tid;
		tmpConsumeData.HasBuyCount = itemData.iHaveBuyNum;
		tmpConsumeData.LeftBuyCount = itemData.iLeftNum;

		if(itemData.costNum2 == 0 || itemData.costType2 == 0)
		{
			tmpConsumeData.CostTid = itemData.costType1;
			tmpConsumeData.Price = itemData.costNum1;
		}else if(itemData.costNum1 == 0 || itemData.costType1 == 0)
		{
			tmpConsumeData.CostTid = itemData.costType2;
			tmpConsumeData.Price = itemData.costNum2;
		}

		tmpConsumeData.SureBuy = (int buyCount) =>
		{
			FeatsShopNet.RequestFeatsShopPurchase(itemData.index, buyCount, itemIdList.ToArray(), x => RequestFeatsShopPurchaseSuccess(buyCount, x), ResponseFeatsShopPurchaseFail);
		};
		tmpConsumeData.Complete = (string tmpRespText, int buyCount) =>
		{
			if (tmpRespText != "" && tmpRespText != "cancel")
			{
				set("BUY_COUNT", buyCount);
			}
		};
		DataCenter.OpenWindow("MALL_BUY_CONSUME_WINDOW", tmpConsumeData);			
		return true;
	}
	
	void RequestFeatsShopPurchaseSuccess(int buyCount, string txt) 
	{
		SC_ResponseFeatsShopPurchase rfsp = JCode.Decode<SC_ResponseFeatsShopPurchase>(txt);
		
		if(rfsp.isBuySuccess == 1) 
		{			
			List<ItemDataBase> _itemDataBaseList = PackageManager.UpdateItem(rfsp.buyItem);
			ItemDataBase[] _itemDataBase = (ItemDataBase[])_itemDataBaseList.ToArray ();

			DataCenter.CloseWindow("MALL_BUY_CONSUME_WINDOW");
//			DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", _itemDataBase);
			DataCenter.OnlyTipsLabelMessage("购买成功！");
			// 添加到背包
			//            PackageManager.UpdateItem(rfsp.buyItem);
			// 删除相应数据
			PackageManager.RemoveItem(itemData.costType1, 0, itemData.costNum1 * buyCount);
			if(itemData.costNum2 > 0)
				PackageManager.RemoveItem(itemData.costType2, GameCommon.GetItemId(itemData.costType2), itemData.costNum2 *buyCount);
			
			DataCenter.OpenWindow("FEATS_SHOP_WINDOW");
		}
		
		DEBUG.Log("RequestFeatsShopPurchaseSuccess:" + txt);
	}
	
	void ResponseFeatsShopPurchaseFail(string txt) 
	{		
		DEBUG.Log("ResponseFeatsShopPurchaseErr:" + txt);
		//by chenliang
		//begin		
		switch (txt)
		{
		case "1111": DataCenter.ErrorTipsLabelMessage("消耗品不足"); break;
		}		
		//end
	}	
}

