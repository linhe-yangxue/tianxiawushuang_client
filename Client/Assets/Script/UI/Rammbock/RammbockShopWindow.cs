
using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

public class RammbockShopData
{
    public int index;
    public int tabId;
    public string tabName;
    public int tid;
	public int itemNum;
    public int itemShowLevel;
    public int vipLevelDisplay;
    public List<int> costTid = new List<int>();
    public List<int> costNum = new List<int>();
    public int buyNum;			
    public int openStarNum;
    public int showLevel;
	public int iLeftNum;
	public int iHaveBuyNum;
}

public class RammbockShopWindow : tWindow {

	UIGridContainer tabPageGrid, renownGrid;
    //by chenliang
    //begin

    private bool mReposition = true;        //是否将内容滑动到顶端
    public bool Reposition
    {
        set { mReposition = value; }
    }

	private int mCurTabPageIndex = 1;	//> 当前标签页
    public int CurTabPageIndex
    {
        set { mCurTabPageIndex = value; }
        get { return mCurTabPageIndex; }
    }
    //end

	public static SC_ResponseClothShopQuery rcsq;

	public class RammbockDeliverData {
		public List<RammbockShopData> td;
		public object obj;
	}
  
	public override void Init ()
	{
		base.Init ();
		EventCenter.Self.RegisterEvent("Button_rammboc_shop_tab_page", new DefineFactory<Button_rammboc_shop_tab_page>());
		EventCenter.Self.RegisterEvent("Button_renown_close_button", new DefineFactory<Button_renown_close_button>());
		EventCenter.Self.RegisterEvent("Button_buy_button_renown_shop", new DefineFactory<Button_buy_rammbock_item_button>());

	}

	public override bool Refresh (object param)
	{
		base.Refresh (param);
		//ActiveData activeData = TeamPosInfoWindow.mCurActiveData;
		int playLevel = RoleLogicData.Self.character.level;
		int maxTabs = 0;
		List<RammbockShopData> tabData = TableCommon.GetRammbockInfo(CurTabPageIndex, out maxTabs);
		RammbockDeliverData rdd = new RammbockDeliverData();
		// 该信息为网络请求过来的
		if(param is SC_ResponseClothShopQuery) {
			rdd.td = tabData;
			rdd.obj = param;
		}

		string[] tabNames = TableCommon.GetRammbockTabName(maxTabs, playLevel);
		tabPageGrid.MaxCount = maxTabs;
		for(int i=0; i < maxTabs; i++) { 
			GameObject obj = tabPageGrid.controlList[i];
			if(i == CurTabPageIndex-1) {
				obj.GetComponent<UIToggle>().startsActive = true;
				GameCommon.ToggleTrue(obj);
			}
			
			GameCommon.SetUIText(obj, "LabelBackground", tabNames[i]);
			GameCommon.SetUIText(obj, "LabelCheckmark", tabNames[i]);
			obj.name = "rammboc_shop_tab_page";
			obj.GetComponent<UIButtonEvent>().mData.set("TAB_ID", i + 1);

			// DEBUG.Log("TableCount:" + maxTabs);
		}
        DataCenter.SetData("SHOP_RENOWN_WINDOW", "REFRESH_PAGE", rdd);

		return true;
	}

    public override void OnClose() {
        base.OnClose();
        DataCenter.CloseBackWindow();
    }

	public override void Open (object param)
	{
		base.Open (param);
        if (param is bool && param != null && (bool)param == false)
        {

        }
        else
        {
            DataCenter.OpenBackWindow("SHOP_RENOWN_WINDOW", "a_ui_lhsd_logo", () =>
            {
                if (param is string && (string)param == "BACK_RECOVER")
                {
                    DataCenter.OpenBackWindow("RECOVER_WINDOW", "a_ui_huishou_logo", () => MainUIScript.Self.ShowMainBGUI(), 120);
                }
                //            else
                //            {
                //                MainUIScript.Self.ShowMainBGUI();
                //            }
            });
        }
        

		tabPageGrid = GetUIGridContainer("renown_title_buttons");
		renownGrid = GetUIGridContainer("renown_goods_grid");

        //by chenliang
        //begin

//        RammbockShopNet.RequestClothShopInfo(RequestRammbockShopNetSuccess, ResponseRammbockShopNetFail);
//----------------
        if (param is bool)
            mReposition = (bool)param;

		if (mReposition)
			CurTabPageIndex = 1;

        RammbockShopNet.RequestClothShopInfo((string text) =>
        {
            //打开指定页签
            if (param is int)
                CurTabPageIndex = (int)param;

            RequestRammbockShopNetSuccess(text);
        }, ResponseRammbockShopNetFail);

        //end
	}

	void RequestRammbockShopNetSuccess(string txt) {
		rcsq = JCode.Decode<SC_ResponseClothShopQuery> (txt);
		Refresh(rcsq);

		DEBUG.Log("RequestRammbockShopNetSuccess:" + txt);
        //added by xuke 灵核商店红点相关
        RammbockNewMarkManager.Self.CheckReward_NewMark(rcsq);
        RammbockNewMarkManager.Self.RefreshRammbockNewMark();
        //end
	}

	void ResponseRammbockShopNetFail(string txt) {

		DEBUG.Log("ResponseRammbockShopNetErr:" + txt);
	}

    public void SortTabData(List<RammbockShopData> tabDataOld, SC_ResponseClothShopQuery rQuery, out List<RammbockShopData> tabDataNew)
    {
        List<RammbockShopData> tabDataTemp = new List<RammbockShopData>();
        foreach (var data in tabDataOld)
        {
            List<ClothStruct> cs = rQuery.cloth;
            bool isBuyed = false;
            if (data.buyNum != 0)
            {
                foreach (ClothStruct cc in cs)
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
            List<ClothStruct> cs = rQuery.cloth;
            foreach (ClothStruct cc in cs)
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

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_PAGE":
                {
                    // 条目数据
                    List<RammbockShopData> tabData = null;
                    // 网络请求数据
                    SC_ResponseClothShopQuery squery = null;

                    if (objVal != null && objVal.GetType() == new RammbockDeliverData().GetType())
                    {
                        List<RammbockShopData> tabDataTemp = ((RammbockDeliverData)objVal).td;
                        squery = ((RammbockDeliverData)objVal).obj as SC_ResponseClothShopQuery;
                        SortTabData(tabDataTemp, squery, out tabData);  //买完的置底
                    }
                    else
                    {
                        DEBUG.Log("get rammbock shop data is null.");
                        return;
                    }
                    renownGrid.MaxCount = tabData.Count;
                    //added by xuke
                    ResetGridTweenFlag();
                    //end
                    UIScrollView tmpScrollView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "renown_goods_scrollView");
                    //by chenliang
                    //begin

//                     if (tmpScrollView != null)
//                         tmpScrollView.ResetPosition();
//-----------------------------
                    if (tmpScrollView != null && mReposition)
                    {
                        mReposition = false;
                        tmpScrollView.ResetPosition();
                    }

                    //end

                    for (int i = 0; i < tabData.Count; i++)
                    {
                        GameObject go = renownGrid.controlList[i];
                        FillItemUIDataContent(go, tabData, i);
                        UpdateItemNetInfo(go, tabData, i, squery);
                    }
                    break;
                }
		case "RESET_POSITION":
			Reposition = true;
			onChange("REFRESH_PAGE",objVal);
			break;
        case "REFRESH_NEWMARK":
            RefreshNewMark();
            return;
            default:
                break;
        }

        RefreshHeadInfo();
        renownGrid.repositionNow = true;
    }

    private void RefreshNewMark()
    {
        if (mGameObjUI == null)
            return;
        UIGridContainer _gridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "renown_title_buttons");
        if (_gridContainer == null)
            return;
        if (_gridContainer.MaxCount < RammbockNewMarkManager.rewardIndex)
            return;
        GameObject _rewardTabObj = _gridContainer.controlList[RammbockNewMarkManager.rewardIndex - 1];
        GameCommon.SetNewMarkVisible(_rewardTabObj,RammbockNewMarkManager.Self.RewardVisible);
    }

    void FillItemUIDataContent(GameObject item, List<RammbockShopData> data, int i)
    {
        RammbockShopData tmpData = data[i];

        int tmpItemTid = tmpData.tid;
        string tmpItemName = GameCommon.GetItemName(tmpItemTid);
        ITEM_TYPE tmpItemType = PackageManager.GetItemTypeByTableID(tmpItemTid);
        bool tmpIsFragment = false;
        if (tmpItemType == ITEM_TYPE.PET_FRAGMENT || tmpItemType == ITEM_TYPE.EQUIP_FRAGMENT)
        {
            tmpIsFragment = true;
            DataRecord tmpFragConfig = DataCenter.mFragment.GetRecord(tmpItemTid);
            int tmpItemCount = PackageManager.GetItemLeftCount(tmpItemTid);
            int tmpFragCostNum = (int)tmpFragConfig.getObject("COST_NUM");
            //计算已有数量，合成所需数量
            tmpItemName += "（";
            if (tmpItemCount < tmpFragCostNum)
                tmpItemName += "[EA3030]";
            else
                tmpItemName += "[30EA30]";
            tmpItemName += tmpItemCount.ToString();
            tmpItemName += ("[FFFFFF]/" + (int)tmpFragConfig.getObject("COST_NUM") + "）");
        }
        GameCommon.SetUIText(item, "goods_name_label", tmpItemName);

        //消耗品
        UIGridContainer tmpNeedContainer = GameCommon.FindComponent<UIGridContainer>(item, "need_renown_grid");
        int tmpCostTypeCount = tmpData.costTid.Count;
        tmpNeedContainer.MaxCount = 2;
        for (int j = 0; j < 2; j++)
        {
            GameObject tmpGOItem = tmpNeedContainer.controlList[j];
            if (j >= tmpCostTypeCount)
            {
                tmpGOItem.SetActive(false);
                continue;
            }
            tmpGOItem.SetActive(true);
            GameCommon.SetOnlyItemIcon(tmpGOItem, "title_renown", tmpData.costTid[j]);
            GameCommon.SetUIText(tmpGOItem, "need_renown_num", GameCommon.ShowNumUI(tmpData.costNum[j]));
        }

		tmpNeedContainer.Reposition ();
        GameCommon.SetOnlyItemIcon(item, "icon_goods", tmpItemTid);
		AddButtonAction (GameCommon.FindObject (item, "icon_goods"), () => GameCommon.SetItemDetailsWindow(tmpItemTid));

		// item num;
		GameCommon.SetUIText (item,"item_num_lbl","x" + tmpData.itemNum.ToString());
        UIButtonEvent ube = GameCommon.FindComponent<UIButtonEvent>(item, "buy_button_renown_shop");
        {
            ube.mData.set("ITEM_DATA", data[i]);
        }
    }

    void RefreshHeadInfo()
    {
        ConsumeItemLogicData itemData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
        int usrPrestigeNum = itemData.GetDataByTid((int)ITEM_TYPE.PRESTIGE).itemNum;
        GameObject go = GameCommon.FindObject(mGameObjUI, "own_renown");
        GameCommon.SetUIText(go, "need_renown_num", GameCommon.ShowNumUI(RoleLogicData.Self.prestige));
    }

    void UpdateItemNetInfo(GameObject item, List<RammbockShopData> data, int i, SC_ResponseClothShopQuery rcq)
    {
        int configIndex = data[i].index;
        List<ClothStruct> cs = rcq.cloth;
        //查找指定数据，如果查不到，说明无购买限制
        ClothStruct tmpCS = cs.Find((ClothStruct tmpCSItem) => {
            return (tmpCSItem.index == configIndex);
        });
        //by chenliang
        //begin

        if (tmpCS == null)
        {
            //此时检查是否有购买限制，此时如果找到说明有购买次数，但是还没有购买过
            if (data[i].buyNum > 0)
                tmpCS = new ClothStruct() { index = configIndex, buyNum = 0 };
        }

        //end
        //剩余数量
        GameCommon.SetUIVisiable(item, "goods_number_label", tmpCS != null);
        if (tmpCS != null)
        {
            int tmpLeftCount = data[i].buyNum - tmpCS.buyNum;
			data[i].iLeftNum = tmpLeftCount;
			data[i].iHaveBuyNum = tmpCS.buyNum;
            GameCommon.SetUIText(GameCommon.FindObject(item, "goods_number_label"), "number_label", tmpLeftCount.ToString());
        }else 
		{
			data[i].iLeftNum = -1;
			data[i].iHaveBuyNum = 0;
		}
        //是否可以购买
        bool tmpCanBuy = false;
        if(tmpCS != null && tmpCS.buyNum < data[i].buyNum)
            tmpCanBuy = true;
        else if(tmpCS == null)
            tmpCanBuy = true;
        //星星限制
        bool tmpShowStarLimit = (data[i].openStarNum > 0 && data[i].openStarNum > rcq.star);
        GameCommon.SetUIVisiable(item, "limit_label", tmpShowStarLimit);
        if (tmpShowStarLimit)
            GameCommon.SetUIText(item, "limit_label", data[i].openStarNum.ToString() + "星可购买");
        //是否显示购买按钮
        UIImageButton tmpBtnImage = GameCommon.GetUIButton(item, "buy_button_renown_shop");
        if (tmpBtnImage != null)
            tmpBtnImage.isEnabled = !tmpShowStarLimit && tmpCanBuy;
    }
}

public class Button_rammboc_shop_tab_page : CEvent {

	public override bool _DoEvent () {
		int tabId = get("TAB_ID");
		RammbockShopWindow _rammbockShopWin = DataCenter.GetData ("SHOP_RENOWN_WINDOW") as RammbockShopWindow;
		if(_rammbockShopWin != null)
			_rammbockShopWin.CurTabPageIndex = tabId;


		int maxTabs = 0;
		List<RammbockShopData> tabData = TableCommon.GetRammbockInfo(tabId, out maxTabs);
		RammbockShopWindow.RammbockDeliverData rdd = new  RammbockShopWindow.RammbockDeliverData();
		rdd.td = tabData;
		rdd.obj = RammbockShopWindow.rcsq;
        //by chenliang
        //begin

//        RammbockShopWindow tmpWin = DataCenter.GetData("SHOW_RENOWN_WINDOW") as RammbockShopWindow;
//        if (tmpWin != null)
//            tmpWin.Reposition = true;

        //end

		DataCenter.SetData ("SHOP_RENOWN_WINDOW","RESET_POSITION",rdd);
		//DataCenter.SetData("SHOP_RENOWN_WINDOW", "REFRESH_PAGE", rdd);
		// DEBUG.Log("TabId:" + tabId);
		return true;
	}
}

public class Button_buy_rammbock_item_button : CEvent 
{
	RammbockShopData rsd;
	
	public override bool _DoEvent ()
	{
		rsd = get ("ITEM_DATA").mObj as RammbockShopData;
		if(rsd == null) {
			DEBUG.LogError("btn data is null. check codex");
			return true;
		}
		ConsumeItemLogicData itemData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
		
		for (int i = 0, count = rsd.costTid.Count; i < count; i++)
		{
			int tmpLeftCostNum = PackageManager.GetItemLeftCount(rsd.costTid[i]);
			if (tmpLeftCostNum < rsd.costNum[i])
			{
				string tmpCostName = GameCommon.GetItemName(rsd.costTid[i]);
				DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_MYSTERIOUS_NO_ENGOUGH_COST, tmpCostName);
				return true;
			}
		}
		List<int> itemIdArr = new List<int>();
		foreach (int costId in rsd.costTid) 
		{
			if(costId != 0)
				itemIdArr.Add(GameCommon.GetItemId(costId));
		}

        //by chenliang
        //begin

//		MallBuyConsumeData tmpConsumeData = new MallBuyConsumeData();
//-------------
        MallBuyConsumeData tmpConsumeData = new MallBuyConsumeData(ShopType.Tower);

        //end
		tmpConsumeData.Index = rsd.index;
		tmpConsumeData.Tid = rsd.tid;
		tmpConsumeData.HasBuyCount = rsd.iHaveBuyNum;
		tmpConsumeData.LeftBuyCount = rsd.iLeftNum;
		tmpConsumeData.CostTid = rsd.costTid[0];
		tmpConsumeData.Price = rsd.costNum[0];
		tmpConsumeData.SureBuy = (int buyCount) =>
		{
			RammbockShopNet.RequestClothShopPurchase(rsd.index, buyCount, itemIdArr.ToArray(), x => RequestRammbockShopPurchaseSuccess(buyCount, x), ResponseRammbockShopPurchaseFail);			
		};
		tmpConsumeData.Complete = (string tmpRespText, int buyCount) =>
		{
			if (tmpRespText != "" && tmpRespText != "cancel")
			{
				set("BUY_COUNT", buyCount);
				//				RequestShopPurchaseSuccess(tmpRespText);
			}
		};
		DataCenter.OpenWindow("MALL_BUY_CONSUME_WINDOW", tmpConsumeData);		
	
		//end
		
		DEBUG.Log("ButtonEvent: " + rsd.tid);
		return true;
	}
	
	// 物品购买成功
	//by chenliang
	//begin
	
	//	void RequestRammbockShopPurchaseSuccess(string text) {
	//-------------------
	void RequestRammbockShopPurchaseSuccess(int buyCount, string text) {
		
		//end
		SC_ClothShopPurchase csp = JCode.Decode<SC_ClothShopPurchase>(text);
		
		// 添加到背包
		//by chenliang
		//begin
		
		//		PackageManager.AddItem(csp.buyItem);
		//--------------
		List<ItemDataBase> _itemList = PackageManager.UpdateItem(csp.buyItem);
		
		//end
		// 删除相应数据
		for (int i = 0, count = rsd.costTid.Count; i < count; i++)
			PackageManager.RemoveItem(rsd.costTid[i], 0, rsd.costNum[i] * buyCount);

		DataCenter.CloseWindow("MALL_BUY_CONSUME_WINDOW");
		DataCenter.OnlyTipsLabelMessage("购买成功！");
        DataCenter.OpenWindow("SHOP_RENOWN_WINDOW", false);
        DataCenter.SetData("RAMMBOCK_WINDOW", "UPDATE_RENOWN_NUM", true);

		DEBUG.Log("RequestRammbockShopPurchaseSuccess:" + csp.buyItem[0].ToString());
	}
	
	void ResponseRammbockShopPurchaseFail(string text) {
		int errCode = 0;
		int.TryParse(text, out errCode);
		string txt = "";
		if(errCode == 1107) {
            txt = "/* 封灵符不足 */";
		}else {
			txt = "errCode:" + text;
		}
		DataCenter.ErrorTipsLabelMessage(txt);
		DEBUG.Log("ResponseRammbockShopPurchaseErr:" + text);
	}	
}

// 页面关闭
public class Button_renown_close_button : CEvent {

	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("SHOP_RENOWN_WINDOW");
		return true;
	}
}
