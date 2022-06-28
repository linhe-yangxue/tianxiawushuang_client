using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

public class PrestigeShopData {
	public int index;
	public int tabId;
	public string tabName;
	public int tid;
	public int itemNum;
	public int itemShow;
	public int costType1;
	public int costNum1;
	public int costType2;
	public int costNum2;
	public int buyNum;
	public int refreshByDay;
	public int openRank;
    public int firstShow;
    public int curRank;
	public int iLeftNum;
	public int iHaveBuyNum;
}

public class PVPPrestigeShopWindow : tWindow {

	public class PrestigeShopDeliverData {
		public List<PrestigeShopData> psd;
		public object obj;
		public int whichPage;
	}

	UIGridContainer prestigeBtnGrid;
	UIGridContainer prestigeGoodsGrid;

	public static SC_ResponsePrestigeShopQuery rpsq;
	
	public int isWhichPage;

	void RequestPrestigeShopQuerySuccess(string txt) {
		rpsq = JCode.Decode<SC_ResponsePrestigeShopQuery> (txt);
		Refresh(rpsq);

		DEBUG.Log("RequestPrestigeShopQuerySuccess:" + txt);
	    //added by xuke 声望商店红点
        PVPNewMarkManager.Self.CheckReward_NewMark(rpsq);
        PVPNewMarkManager.Self.RefreshPrestigeNewMark();
        //end
    }

	void ReponsePrestigeShopQueryFail(string txt) {

		DEBUG.Log("ReponsePrestigeShopQueryErr:" + txt);
	}

	public override void Open (object param)
	{
		base.Open (param);
        DataCenter.Set("PVP_PRESTIGE_RETURN_RECOVER", false); //init

		prestigeBtnGrid = GetUIGridContainer("prestige_title_buttons_grid");
		prestigeGoodsGrid = GetUIGridContainer("prestige_goods_grid");

        //by chenliang
        //begin

//		PVPPrestigeShopNet.RequestPrestigeShopQuery(RequestPrestigeShopQuerySuccess, ReponsePrestigeShopQueryFail);
//---------------
        //增加打开页签操作
        PVPPrestigeShopNet.RequestPrestigeShopQuery(
            (string text) =>
            {
                if (param is int)
                    isWhichPage = (int)param;
                RequestPrestigeShopQuerySuccess(text);
            }, ReponsePrestigeShopQueryFail);

        //end
	}

	public override void Init ()
	{
		base.Init ();
		EventCenter.Self.RegisterEvent("Button_prestige_shop_tab_page", new DefineFactory<Button_prestige_shop_tab_page>());
		EventCenter.Self.RegisterEvent("Button_prestige_close_button", new DefineFactory<Button_prestige_close_button>());
		EventCenter.Self.RegisterEvent("Button_prestige_buy_button", new DefineFactory<Button_prestige_buy_button>());

		isWhichPage = 1;
	}

	public override bool Refresh (object param)
	{
		if(param is SC_ResponsePrestigeShopQuery) {
			int maxTabs = 0;
			List<PrestigeShopData> prestiges = TableCommon.GetPrestigeInfo(isWhichPage, out maxTabs);
			PVPPrestigeShopWindow.PrestigeShopDeliverData psdd = new PVPPrestigeShopWindow.PrestigeShopDeliverData();
			psdd.psd = prestiges;
			psdd.obj = param;
			psdd.whichPage = isWhichPage;

			string[] tabNames = TableCommon.GetPrestigeTabName(maxTabs);
           
            //begin

//			prestigeBtnGrid.MaxCount = maxTabs;
//----------------
            //之前代码暂时废弃

            //end
			for(int i=0; i < maxTabs; i++) { 
				GameObject obj = prestigeBtnGrid.controlList[i];
				if(i + 1 == isWhichPage) {
					obj.GetComponent<UIToggle>().startsActive = true;
					GameCommon.ToggleTrue(obj);
				}
				else
				{
					obj.GetComponent<UIToggle>().startsActive = false;
					GameCommon.ToggleFalse(obj);
				}
				
				GameCommon.SetUIText(obj, "LabelBackground", tabNames[i]);
				GameCommon.SetUIText(obj, "LabelCheckmark", tabNames[i]);
				obj.name = "prestige_shop_tab_page";
				obj.GetComponent<UIButtonEvent>().mData.set("TAB_ID", i + 1);
			}
			DataCenter.SetData("PVP_PRESTIGE_SHOP_WINDOW", "FIRST_OPEN", psdd);

		}
       
		return true;

	}

    public void SortTabData(List<PrestigeShopData> tabDataOld, SC_ResponsePrestigeShopQuery rQuery, out List<PrestigeShopData> tabDataNew)
    {
        List<PrestigeShopData> tabDataTemp = new List<PrestigeShopData>();
        foreach (var data in tabDataOld)
        {
            PrestigeStruce[] cs = rQuery.commodity;
            bool isBuyed = false;
            if (data.buyNum != 0)
            {
                foreach (PrestigeStruce cc in cs)
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
            PrestigeStruce[] cs = rQuery.commodity;
            foreach (PrestigeStruce cc in cs)
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

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

		//特别的
		if (keyIndex == "REFRESH_PAGE") {
			PVPPrestigeShopNet.RequestPrestigeShopQuery(RequestPrestigeShopQuerySuccess, ReponsePrestigeShopQueryFail);
            return;
        }
        else if (keyIndex == "REFRESH_NEWMARK") 
        {
            RefreshNewMark();
            return;
        }

		List<PrestigeShopData> tabData;
		SC_ResponsePrestigeShopQuery rQuery;

		if(objVal.GetType() != new PrestigeShopDeliverData().GetType()) return;

		if(objVal != null && objVal.GetType() == new PrestigeShopDeliverData().GetType()) {
			PrestigeShopDeliverData psdd = (PrestigeShopDeliverData) objVal;
			List<PrestigeShopData> tabDataTemp = psdd.psd;
			rQuery = psdd.obj as SC_ResponsePrestigeShopQuery;

            SortTabData(tabDataTemp, rQuery, out tabData);    //买完的置底
			//设置页码
			isWhichPage = psdd.whichPage;
		} else {
			DEBUG.LogError("get prestige data null");
			return;
		}

		if(prestigeGoodsGrid.MaxCount > 0) prestigeGoodsGrid.MaxCount = 0;
		prestigeGoodsGrid.MaxCount = tabData.Count;
        //added by xuke
        ResetGridTweenFlag();
        //end
		switch(keyIndex){
		case "REFRESH_INIT":
		{
			for(int i=0; i < tabData.Count; i ++) {
				GameObject go = prestigeGoodsGrid.controlList[i];
				FillUIItemDataContent(go, tabData, i);
				UpdateItemNetInfo(go, tabData, i, rQuery);
			}
			break;
		}
		case "FIRST_OPEN":
		{
			for(int i=0; i < tabData.Count; i++) {
				GameObject go = prestigeGoodsGrid.controlList[i];
				FillUIItemDataContent(go, tabData, i);
				UpdateItemNetInfo(go, tabData, i, rQuery);
			}
			break;
		}
		}
		RefreshHeadInfo();
		prestigeGoodsGrid.repositionNow = true;

	}

    private void RefreshNewMark() 
    {
        if (mGameObjUI == null)
            return;
        UIGridContainer _gridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "prestige_title_buttons_grid");
        if (_gridContainer == null)
            return;
        if(_gridContainer.MaxCount < PVPNewMarkManager.rewardTabIndex)
            return;
        GameObject _rewardTabObj = _gridContainer.controlList[PVPNewMarkManager.rewardTabIndex - 1];
        GameCommon.SetNewMarkVisible(_rewardTabObj,PVPNewMarkManager.Self.RewardVisible);
    }

	void  FillUIItemDataContent(GameObject item, List<PrestigeShopData> data, int i) {
		string itemName = GameCommon.GetItemName(data[i].tid);

		GameCommon.SetUIText(item, "goods_name_label", itemName);
		// 设置消耗货币1
		//GameCommon.SetOnlyItemIcon (GameCommon.FindObject(item,"cost_Icon_1"),data[i].costType1);
		GameCommon.SetResIcon (item,"cost_Icon_1",data[i].costType1,false,true);
		GameCommon.SetUIText(item, "need_prestige_num", data[i].costNum1.ToString());

		// 设置消耗货币2
		if (data [i].costType2 != 0) 
		{
			GameCommon.SetUIVisiable(item,"need_cost_2",true);
			//GameCommon.SetOnlyItemIcon (GameCommon.FindObject (item, "cost_Icon_2"), data [i].costType2);
			GameCommon.SetResIcon (item,"cost_Icon_2",data[i].costType2,false,true);
			GameCommon.SetUIText (item, "need_cost_num_2", data [i].costNum2.ToString ());
		} 
		else 
		{
			GameCommon.SetUIVisiable(item,"need_cost_2",false);
		}
		/*
		// icon setting
		UISprite icon = GameCommon.FindComponent<UISprite>(item, "icon_goods");
		string altsName = TableCommon.GetStringFromRoleEquipConfig(data[i].tid, "ICON_ATLAS_NAME");
		string spriteName = TableCommon.GetStringFromRoleEquipConfig(data[i].tid, "ICON_SPRITE_NAME");
		// remove icon left top tag
		GameCommon.FindObject(item, "sprite").SetActive(false);
		GameCommon.SetIcon(icon, altsName, spriteName);
		// icon end
		*/
        //by chenliang
        //begin

//		GameCommon.SetItemIcon(item, "icon_goods", data[i].tid);
//--------------
        //added by xuke
        //-----  GameCommon.SetItemIconEx(item, "icon_goods", data[i].tid, data[i].costNum1);
        GameCommon.SetOnlyItemIcon(GameCommon.FindObject(item, "icon_goods"), data[i].tid);
		AddButtonAction (GameCommon.FindObject(item, "icon_goods"), () => GameCommon.SetItemDetailsWindow(data[i].tid));
        //end

		// item num 
		GameCommon.SetUIText (item,"item_num_lbl","x" + data[i].itemNum.ToString());
        //end
		UIButtonEvent ube = GameCommon.FindComponent<UIButtonEvent>(item, "prestige_buy_button");
		ube.mData.set("ITEM_DATA", data[i]);

        // 设置折扣
        GameObject _saleObj = GameCommon.FindObject(item, "sale_sprite");
        if(_saleObj != null)
        {
            UISprite _saleSprite = _saleObj.GetComponent<UISprite>();
            if (data[i].firstShow == 0)
            {
                _saleObj.SetActive(false);
            }
            else
            {
                if (_saleSprite != null)
                {
                    _saleSprite.gameObject.SetActive(true);
                    if (DataCenter.mTipIconConfig == null)
                        return;
                    DataRecord _record = DataCenter.mTipIconConfig.GetRecord(data[i].firstShow);
                    if (_record == null)
                        return;
                    _saleSprite.atlas = GameCommon.LoadUIAtlas(_record["TIPICON_ATLAS_NAME"]);
                    _saleSprite.spriteName = _record["TIPICON_SPRITE_NAME"];
                }
            }
        }      
	}

	void RefreshHeadInfo() {
		int usrPrestigeNum = RoleLogicData.Self.reputation;
		GameCommon.SetUIText(mGameObjUI, "need_prestige_head_title_num", GameCommon.ShowNumUI(usrPrestigeNum));
	}

	void UpdateItemNetInfo(GameObject item, List<PrestigeShopData> data, int i, SC_ResponsePrestigeShopQuery rcq) {
		setButtonEnable (item, true);

		int configIndex = data[i].index;
		PrestigeStruce[] cs = rcq.commodity;
		// 在这种情况下，找需要更新的条目信息
		if(cs != null && cs.Length > 0) 
		{
			showLimitBuyTips(item, data, i, rcq);
			//该物品默认使用配置表的信息
		} 
		else 
		{
			// 该物品有次数限制
			if(data[i].buyNum > 0) {
				GameCommon.FindObject(item, "goods_limit_label01").SetActive(false);
				// 采用默认的一次限制
                //modified by xuke
                //GameCommon.SetUIVisiable(item, "goods_number_label", false);
                //---
                GameCommon.SetUIVisiable(item, "goods_number_label", true);
                GameCommon.SetUIText(GameCommon.FindObject(item, "goods_number_label"), "number_label", data[i].buyNum.ToString());
				data[i].iLeftNum = data[i].buyNum;
				data[i].iHaveBuyNum = 0;
                //end
			} else {//该物品为无限制次数的道具
				GameCommon.FindObject(item, "goods_limit_label01").SetActive(false);
                GameCommon.SetUIVisiable(item, "goods_number_label", false);
				data[i].iLeftNum = -1;
				data[i].iHaveBuyNum = 0;
			}
		}

		//rankTips
		int openRankNum = data[i].openRank;
        int curRankNum = data[i].curRank;
        showRankTips(openRankNum, curRankNum, item, rcq);
	
	}

	public void showLimitBuyTips(GameObject item, List<PrestigeShopData> data, int i, SC_ResponsePrestigeShopQuery rcq)
	{
		int configIndex = data[i].index;
		PrestigeStruce[] cs = rcq.commodity;
		bool isHasBuyed = false;  
		foreach(PrestigeStruce cc in cs) 
		{
			// 找到了需要次数限制的条目
			if(cc.index == configIndex) 
			{
				//设置买过了
				isHasBuyed = true;
				//设置更新道具的次数
				int count = data[i].buyNum - cc.buyNum;
				data[i].iLeftNum = count;
				data[i].iHaveBuyNum = cc.buyNum;

                if (data[i].buyNum == 0)
                {
                    GameCommon.FindObject(item, "goods_limit_label01").SetActive(false);
                    GameCommon.SetUIVisiable(item, "goods_number_label", false);
					data[i].iLeftNum = -1;
					data[i].iHaveBuyNum = 0;
                }
                else
                {
                    if (count > 0)
                    {
                        GameCommon.FindObject(item, "goods_limit_label01").SetActive(false);

                        GameCommon.SetUIVisiable(item, "goods_number_label", true);
                        GameCommon.SetUIText(GameCommon.FindObject(item, "goods_number_label"), "number_label", count.ToString());

                    }
                    else
                    {
                        GameCommon.FindObject(item, "goods_limit_label01").SetActive(false);

                        GameCommon.SetUIVisiable(item, "goods_number_label", true);
                        GameCommon.SetUIText(GameCommon.FindObject(item, "goods_number_label"), "number_label", "0");

                        setButtonEnable(item, false);
                    }
                }
			}
		}
		if(!isHasBuyed)
		{
			// 该物品有次数限制
			if(data[i].buyNum > 0) {
				GameCommon.FindObject(item, "goods_limit_label01").SetActive(false);
				// 采用默认的一次限制

                GameCommon.SetUIVisiable(item, "goods_number_label", true);
                GameCommon.SetUIText(GameCommon.FindObject(item, "goods_number_label"), "number_label", data[i].buyNum.ToString());
				data[i].iLeftNum = data[i].buyNum;
				data[i].iHaveBuyNum = 0;
				//end
			}else { //该物品为无限制次数的道具 
				GameCommon.FindObject(item, "goods_limit_label01").SetActive(false);
                GameCommon.SetUIVisiable(item, "goods_number_label", false);
				data[i].iLeftNum = -1;
				data[i].iHaveBuyNum = 0;
			}
		}
	}

	public void setButtonEnable(GameObject item, bool enable)
	{
		UIImageButton tmpUbe = GameCommon.FindComponent<UIImageButton>(item, "prestige_buy_button");
		tmpUbe.isEnabled = enable;
	}

	public void showRankTips(int rankNum,int curRank, GameObject item, SC_ResponsePrestigeShopQuery rcq)
	{
        GameObject go = GameCommon.FindObject(item, "goods_limit_label01");
        UILabel rankNumlbl;
        if(go == null)
            return;

        rankNumlbl = go.GetComponent<UILabel>();
        if(rankNum == null)
            return;

		// 控制玩家排行榜，玩家有rank排名限制
		if(rankNum > 0) 
		{
            go.SetActive(true);
            rankNumlbl.fontSize = 18;
			rankNumlbl.text = "排名" + rankNum + "可购买";
			if(rankNum >= rcq.rank) 
			{
				go.SetActive(false);
			} 
			else
			{
				setButtonEnable(item, false);
			}
		}
        else if (curRank > 0)
		{ 
            // 当前排名
            go.SetActive(true);
            int _myCurRank = rcq.curRank + 1;
            rankNumlbl.fontSize = 15;
            rankNumlbl.text = "当前排名" + curRank + "可购买";
            if (_myCurRank <= curRank)
            {
                go.SetActive(false);
            }
            else 
            {
                setButtonEnable(item, false);
            }
		}
	}
}

public class Button_prestige_shop_tab_page : CEvent {

	public override bool _DoEvent ()
	{
		int tabId = get("TAB_ID");

		int tabMax = 0;
		List<PrestigeShopData> npsd = TableCommon.GetPrestigeInfo(tabId, out tabMax);
		PVPPrestigeShopWindow.PrestigeShopDeliverData psdd = new PVPPrestigeShopWindow.PrestigeShopDeliverData();
		psdd.psd = npsd;
		psdd.obj = PVPPrestigeShopWindow.rpsq;
		psdd.whichPage = tabId;

		DataCenter.SetData("PVP_PRESTIGE_SHOP_WINDOW", "REFRESH_INIT", psdd);
		DEBUG.Log("TABID:" + tabId);
        GameObject.Find("prestige_goods_scrollView").GetComponent<UIScrollView>().ResetPosition();
         //  transform.localPosition = new Vector3(0,0,0);
		return true;
	}
}

public class Button_prestige_close_button:CEvent {

	public override bool _DoEvent ()
	{
        if(DataCenter.Get("PVP_PRESTIGE_RETURN_RECOVER"))
        {
            DataCenter.Set("PVP_PRESTIGE_RETURN_RECOVER", false);
            DataCenter.CloseWindow("PVP_PRESTIGE_SHOP_WINDOW");

            //返回回收界面
            DataCenter.SetData("RECOVER_WINDOW", "OPEN", 1);
            DataCenter.SetData("RECOVER_WINDOW", "SHOW_WINDOW", 1);
            MainUIScript.Self.HideMainBGUI();
        }
        else
        {
            ArenaMainWindow tmpWin = DataCenter.GetData("ARENA_MAIN_WINDOW") as ArenaMainWindow;
            if (tmpWin != null && tmpWin.IsOpen())
            {
                tmpWin.set("REFRESH_MY_REPUTATION", null);
            }
            
            //DataCenter.SetData("ARENA_MAIN_WINDOW", "REFRESH_MY_REPUTATION", null);

		    DataCenter.CloseWindow("PVP_PRESTIGE_SHOP_WINDOW");
        }
		return true;
	}
}

/// <summary>
/// 玩家购买道具信息
/// </summary>
public class Button_prestige_buy_button:CEvent {
	PrestigeShopData psd;
	
	public override bool _DoEvent ()
	{
		psd = get ("ITEM_DATA").mObj as PrestigeShopData;
		
		int usrPrestigeNum = RoleLogicData.Self.reputation;
		// 当用户没有声望点数，以及声望点数不够时，主动拦截
		if(usrPrestigeNum <= 0 || usrPrestigeNum < psd.costNum1) {
			//by chenliang
			//begin
			
			//			DataCenter.OpenMessageWindow("用户声望不足。");
			//--------------------
			//策划要求去除句号
			DataCenter.ErrorTipsLabelMessage("用户声望不足");
			
			//end
			return true;
		}
		if (psd.costNum2 > 0 && PackageManager.GetItemLeftCount (psd.costType2) < psd.costNum2) 
		{
			if (psd.costType2 == (int)ITEM_TYPE.GOLD)
			{
				DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
			}
			else
			{
				DataCenter.ErrorTipsLabelMessage(GameCommon.GetItemName(psd.costType2) + "不足!");
			}
			return true;
		}
		
		List<int> itemIdArr = new List<int>();
		if (psd.costType2 != 0)
			itemIdArr.Add (GameCommon.GetItemId( psd.costType2));

        //by chenliang
        //begin

//		MallBuyConsumeData tmpConsumeData = new MallBuyConsumeData();
//-------------
        MallBuyConsumeData tmpConsumeData = new MallBuyConsumeData(ShopType.PVP);

        //end
		tmpConsumeData.Index = psd.index;
		tmpConsumeData.Tid = psd.tid;
		tmpConsumeData.HasBuyCount = psd.iHaveBuyNum;
		tmpConsumeData.LeftBuyCount = psd.iLeftNum;
		if(psd.costNum2 == 0 || psd.costType2 == 0)
		{
			tmpConsumeData.CostTid = psd.costType1;
			tmpConsumeData.Price = psd.costNum1;
		}else if(psd.costNum1 == 0 || psd.costType1 == 0)
		{
			tmpConsumeData.CostTid = psd.costType2;
			tmpConsumeData.Price = psd.costNum2;
		}

		tmpConsumeData.SureBuy = (int buyCount) =>
		{
//			RammbockShopNet.RequestClothShopPurchase(psd.index, buyCount, itemIdArr.ToArray(), x => RequestRammbockShopPurchaseSuccess(buyCount, x), ResponseRammbockShopPurchaseFail);	
			PVPPrestigeShopNet.RequestPrestigeShopPurchase(psd.index, buyCount, itemIdArr.ToArray(), x => RequestPrestigeShopPurchaseSuccess(buyCount, x) , ResponsePrestigeShopPurchaseFail);
		};
		tmpConsumeData.Complete = (string tmpRespText, int buyCount) =>
		{
			if (tmpRespText != "" && tmpRespText != "cancel")
			{
				set("BUY_COUNT", buyCount);
			}
		};
		DataCenter.OpenWindow("MALL_BUY_CONSUME_WINDOW", tmpConsumeData);	

		DEBUG.Log("buy sth." + psd.index);
		return true;
	}
	
	void RequestPrestigeShopPurchaseSuccess(int buyCount, string txt) {
		SC_ResponsePrestigeShopPurchase rpsp = JCode.Decode<SC_ResponsePrestigeShopPurchase>(txt);
		if(rpsp.isBuySuccess == 1) {
			// 添加到背包
			List<ItemDataBase> itemDataList = PackageManager.UpdateItem(rpsp.buyItem);
			//PackageManager.AddItem(rpsp.buyItem);
			// 删除相应数据
			// PackageManager.RemoveItem(psd.costType1, 0, psd.costNum1);
			RoleLogicData.Self.reputation -= psd.costNum1 * buyCount;
			if(psd.costNum2 > 0)
				PackageManager.RemoveItem(psd.costType2, GameCommon.GetItemId(psd.costType2), psd.costNum2 * buyCount);
			
			//把相同的item跌起来
			List<ItemDataBase> itemDataListNew;
			GameCommon.MergeItemDataBase(itemDataList, out itemDataListNew);
			DataCenter.CloseWindow("MALL_BUY_CONSUME_WINDOW");
//			DataCenter.OpenWindow("GET_REWARDS_WINDOW", itemDataListNew.ToArray());
			DataCenter.OnlyTipsLabelMessage("购买成功！");
			
			//购买完成以后刷新声望商店
			DataCenter.SetData("PVP_PRESTIGE_SHOP_WINDOW", "REFRESH_PAGE", null);
			
			
		} 
		DEBUG.Log("RequestPrestigeShopPurchaseSuccess:" + txt);
	}
	
	void ResponsePrestigeShopPurchaseFail(string txt) {
		
		DEBUG.Log("RequestPrestigeShopPurchaseErr:" + txt);
		//by chenliang
		//begin
		
		switch(txt)
		{
		case "1111": DataCenter.OpenMessageWindow("消耗品不足"); break;
		case "1951":
			DataCenter.OpenWindow("ARENA_MAIN_WINDOW");
			DataCenter.SetData("PVP_PRESTIGE_SHOP_WINDOW", "REFRESH_PAGE", null);
			break;
		}
		
		//end
	}
}

