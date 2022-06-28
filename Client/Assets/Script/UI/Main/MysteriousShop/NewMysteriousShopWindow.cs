using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class MysteriousShopData {
	public int index;
	public int tid;
	public int itemNum;
	public int itemShowLevel;
	public int costType1;
	public int costNum1;
	public int firstShow;
	public int petId;
	public int refreshRate;
}

// NEW_MYSTERIOUS_SHOP_WINDOW
public class NewMysteriousShopWindow : tWindow {

	UIGridContainer mystery_goods_grid;
    SC_MysteryShopQuery msq;
    private CountdownUI mDownCountUI;

    const int maxLeftTime = 7200;      //2小时
	public override void Init ()
	{
		base.Init ();
		EventCenter.Self.RegisterEvent("Button_mystery_window_back", new DefineFactory<Mystery_Window_Back_Button>());
		EventCenter.Self.RegisterEvent("Button_black_market_goods_buy_button", new DefineFactory<Button_black_market_goods_buy>());
		EventCenter.Self.RegisterEvent("Button_black_market_update_button", new DefineFactory<Button_black_market_update>());
		EventCenter.Self.RegisterEvent("Button_black_market_get_button", new DefineFactory<Button_black_market_get_button>());

	}

	public override bool Refresh (object param)
	{
		List<MysteriousShopData> msd;
		// 网络请求成功
		if(param is SC_MysteryShopQuery) {
			msq = (SC_MysteryShopQuery)param;
            // 刷新显示
            RefreshHeadInfo(msq);

			if(mystery_goods_grid.MaxCount > 0) mystery_goods_grid.MaxCount = 0;
            msd = TableCommon.GetMysteryNeedInfo(msq.indexArr);
            if (msd != null)
            {
                mystery_goods_grid.MaxCount = msd.Count;

                for (int i = 0; i < msd.Count; i++)
                {
                    GameObject item = mystery_goods_grid.controlList[i];
                    FillUIItemDataContent(item, msd, msq, i);
                }
            }
		}
		return true;
	}

    public static int GetMaxFreeRefreshCount()
    {
        int tmpCount = 10;
        return tmpCount;
    }

    void FillUIItemDataContent(GameObject item, List<MysteriousShopData> data, SC_MysteryShopQuery resp, int i) {
		// base setting
		string itemName = GameCommon.GetItemName (data[i].tid); 
		GameCommon.SetUIText(item, "goods_name_label", itemName);
        GameCommon.FindObject(item, "goods_name_label").GetComponent<UILabel>().width = 160;
		GameCommon.SetUIText(item, "buy_need_num", "x" + data[i].costNum1.ToString());
		GameCommon.SetUIText(item, "item_number", data[i].itemNum.ToString());
		// base end

		// flag setting
        GameCommon.FindObject(item, "Sprite").SetActive(data[i].firstShow == 1);
		GameObject onReadySprite = GameCommon.FindObject(item, "battle_sprite");
		bool alreadyRedayOn = TeamManager.IsPetInTeamByTid(data[i].petId);
		if(alreadyRedayOn)
			onReadySprite.SetActive(true);
		else
			onReadySprite.SetActive(false);
        UISprite _sprite = GameCommon.FindComponent<UISprite>(item,"item_icon");
        if(_sprite != null)
            GameCommon.SetOnlyItemIcon(_sprite.gameObject,data[i].tid);

        AddButtonAction(GameCommon.FindObject (item, "item_icon").gameObject, () => GameCommon.SetItemDetailsWindow (data[i].tid));
		// icon end
		// consume coin icon
		GameCommon.SetResIcon (item,"soul_icon",data[i].costType1,false,true);

		// 判断是否需要显示碎片图标
		GameCommon.SetUIVisiable(item,"suipian_icon",GameCommon.CheckIsFragmentByTid(data[i].tid));

		UIButtonEvent ube = GameCommon.FindComponent<UIButtonEvent>(item, "black_market_goods_buy_button");
		ube.mData.set("ITEM_DATA", data[i]);
		ube.mData.set("ITEM_GRID", item);

		// buy button state update
		UIImageButton uib = GameCommon.FindComponent<UIImageButton>(item, "black_market_goods_buy_button");
        uib.isEnabled = resp.mysteryArr == null ? false : (resp.mysteryArr.IndexOf(data[i].index) == -1);
	}

	void RefreshHeadInfo(SC_MysteryShopQuery info) {
		int usrFHNum = RoleLogicData.Self.soulPoint;
		GameCommon.SetUIText(mGameObjUI, "usr_have_need_num", GameCommon.ShowNumUI(usrFHNum));
        //如果剩余免费刷新次数大于等于最大次数，隐藏免费次数倒计时
        int tmpMaxFreeCount = GetMaxFreeRefreshCount();
        GameCommon.SetUIVisiable(mGameObjUI, "free_time_fresh", info.freeNum < tmpMaxFreeCount);
        // 免费次数刷新完成计时
        if (info.leftTime > 0)
            mDownCountUI = SetCountdown(mGameObjUI, "free_time_fresh_label", info.leftTime, new CallBack(this, "BlackShopCountdownCallback", null), true);
        // 推送免费次数恢复满的消息
        if (info.freeNum < tmpMaxFreeCount && mDownCountUI)
            __AddLocalPush(PUSH_TYPE.BLACK_MARKET_UPDATE, info.freeNum, (long)mDownCountUI.svrRelativelyTime);

        GameCommon.SetUIVisiable(mGameObjUI, "free_time_full_label", info.freeNum >= tmpMaxFreeCount);
        GameCommon.SetUIText(mGameObjUI, "label_refresh_time", info.freeNum + "/" + tmpMaxFreeCount.ToString());

		ConsumeItemLogicData itemData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
		int usrRefreshTokenNum = itemData.GetDataByTid((int)ITEM_TYPE.REFRESH_TOKEN).itemNum;
		GameCommon.SetUIText(mGameObjUI, "refresh_cmd_need_num", usrRefreshTokenNum.ToString());
		int usrVipLevel = RoleLogicData.Self.vipLevel;
		int usrRefreshCount = TableCommon.GetNumberFromVipList(usrVipLevel, "PART_NUM");
		string infoLabel = "使用刷新令或[99ff66]20[ffffff]元宝可以刷新商店   今日剩余{0}次";
		GameCommon.SetUIText(mGameObjUI, "label_usr_all_left_info", string.Format(infoLabel, info.leftNum));

		UIButtonEvent ube = GameCommon.FindComponent<UIButtonEvent>(mGameObjUI, "black_market_update_button");
		ube.mData.set("NET_DATA", info);
	}

    /// <summary>
    /// 添加推送
    /// </summary>
    /// <param name="pushiType">推送类型</param>
    /// <param name="freeNum">剩余免费次数</param>
    /// <param name="leftTime">剩余时间</param>
    /// <returns>是否添加成功</returns>
    private bool __AddLocalPush(PUSH_TYPE pushiType, int freeNum, long leftTime)
    {
        return PushMessageManager.Self.AddLocalPush(pushiType, __GetFullFreeRefreshNumTime(freeNum, leftTime));
    }

    /// <summary>
    /// 获得恢复满免费刷新次数的时间
    /// </summary>
    /// <param name="freeNum">剩余免费次数</param>
    /// <param name="leftTime">剩余时间</param>
    /// <returns></returns>
    private long __GetFullFreeRefreshNumTime(int freeNum, long leftTime)
    {
        int dNum = GetMaxFreeRefreshCount() - freeNum - 1;
        return CommonParam.NowServerTime() + dNum * maxLeftTime + leftTime;
    }

	// 免费刷新倒计时完成
	public void BlackShopCountdownCallback(GameObject go) {

		DEBUG.Log("BlackShopCountdownCallback...");

		SC_MysteryShopQuery msqNative = new SC_MysteryShopQuery();
		msq.freeNum += 1;
		msqNative.freeNum = msq.freeNum;
        //by chenliang
        //begin

//		msqNative.leftTime = -1;
//------------------
        //如果倒计时结束，免费次数应该要增加1，直到免费次数满
        int tmpMaxFreeCount = GetMaxFreeRefreshCount();
        if (msqNative.freeNum >= tmpMaxFreeCount)
            msqNative.leftTime = -1;
        else
            msqNative.leftTime = maxLeftTime;      //2小时

        //end
		msqNative.leftNum = msq.leftNum;
		msqNative.indexArr = msq.indexArr;
        msqNative.mysteryArr = msq.mysteryArr;

        DataCenter.SetData("NEW_MYSTERIOUS_SHOP_WINDOW", "REFRESH", msqNative);  
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex) {
		case "UPDATE_SOUL_POINT_ITEM_DATA":
			if(objVal is LateUpdataUIDataStruct) {
				LateUpdataUIDataStruct ludds = (LateUpdataUIDataStruct)objVal;
				// update base info
				int usrFHNum = RoleLogicData.Self.soulPoint;
				GameCommon.SetUIText(mGameObjUI, "usr_have_need_num", GameCommon.ShowNumUI(usrFHNum));

				// update button state
				UIImageButton uib = GameCommon.FindComponent<UIImageButton>(ludds.item, "black_market_goods_buy_button");
				uib.isEnabled = false;
			}
			break;
		}
	}

	public override void Open (object param)
	{
		base.Open (param);
		mystery_goods_grid = GetUIGridContainer("mystery_goods_grid");

		//DataCenter.OpenWindow ("BACK_GROUP_FRIEND_VISIT_WINDOW");
		//DataCenter.OpenWindow ("INFO_GROUP_WINDOW");

		NewMysteriousShopNet.RequestMysteriousShopQuery(RequestMysteriousSuccess, ResponseMysteriousFail);
	}

	public override void Close ()
	{
		base.Close ();
		DataCenter.CloseBackWindow ();
	}

	void RequestMysteriousSuccess(string txt) {
		msq = JCode.Decode<SC_MysteryShopQuery> (txt);
        if (msq.ret == (int)STRING_INDEX.ERROR_NONE)
        {
            Refresh(msq);
            DEBUG.Log("RequestMysteriousSuccess:" + txt);
        }
        else
        {
            DEBUG.LogError("RequestMysteriousFailed -- " + txt);
        }
	}

	void ResponseMysteriousFail(string txt) {
		DEBUG.Log("RequestMysteriousErr:" + txt);
	}
}

class Mystery_Window_Back_Button:CEvent {
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("NEW_MYSTERIOUS_SHOP_WINDOW");
		MainUIScript.Self.ShowMainBGUI ();
		return true;
	}
}

struct LateUpdataUIDataStruct {
	public MysteriousShopData msd;
	public GameObject item;
}

// buy goods
class Button_black_market_goods_buy:CEvent {
	MysteriousShopData msd;
	GameObject item;
	public override bool _DoEvent ()
	{
		msd = get("ITEM_DATA").mObj as MysteriousShopData;
		item = get("ITEM_GRID").mObj as GameObject;
        //by chenliang
        //begin

//		int usrFHNum = RoleLogicData.Self.soulPoint;
//		if(usrFHNum < msd.costNum1) {
//			DataCenter.OpenMessageWindow("用户符魂不足。");
//			return true;
//		}
//---------------
		int tmpLeftCostNum = PackageManager.GetItemLeftCount (msd.costType1);
		if (tmpLeftCostNum < msd.costNum1)
		{
			string diamon ="元宝";
			string tmpCostName = GameCommon.GetItemName(msd.costType1);
			if(tmpCostName == diamon){				
				GameCommon.ToGetDiamond();
			}else {
                if (msd.costType1 == (int)ITEM_TYPE.PET_SOUL)
                {
                    DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.PET_SOUL);
                }
                else
                {
                    DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_MYSTERIOUS_NO_ENGOUGH_COST, tmpCostName);
                }
			}
			return true;
		}

        //检查相应背包是否已满
        List<PACKAGE_TYPE> tmpPackageTypes = new List<PACKAGE_TYPE>()
        {
            PackageManager.GetPackageTypeByItemTid(msd.tid)
        };
        if (!CheckPackage.Instance.CanAddItems(tmpPackageTypes))
            return true;

        //end
		NewMysteriousShopNet.RequestMysterousShopPurchase(msd.index, 1, RequestMysteriousShopPurchaseSuccess, ResponseMysteriousShopPurchaseFail);

		return true;
	}

	void RequestMysteriousShopPurchaseSuccess(string txt) {
		SC_MysteryShopPurchase msp = JCode.Decode<SC_MysteryShopPurchase>(txt);
		if(msp.ret == (int)STRING_INDEX.ERROR_NONE) {
			if(msp.isBuySuccess != 1) {
				DEBUG.LogError("物品购买失败。");
                //by chenliang
                //begin

//				DataCenter.OpenMessageWindow("物品购买失败。");
//-------------------
                //策划要求去除句号
                DataCenter.OpenMessageWindow("物品购买失败");

                //end
				return;
			}
			ItemDataBase[] buyItems = msp.buyItem;
			List<ItemDataBase> _itemList = PackageManager.UpdateItem(msp.buyItem);
			// 添加包背包
			//PackageManager.AddItem(buyItems);
			// 减少购买得消耗
			//RoleLogicData.Self.soulPoint -= msd.costNum1;
			GameCommon.ConsumeCoinByTid(msd.costType1,msd.costNum1);
			// update ui
			LateUpdataUIDataStruct luuds = new LateUpdataUIDataStruct();
			luuds.item = item;
			luuds.msd = msd;
			DataCenter.SetData("NEW_MYSTERIOUS_SHOP_WINDOW", "UPDATE_SOUL_POINT_ITEM_DATA", luuds);
			// 通知用户
			DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", _itemList.ToArray());
            //test
            if (PetGainWindow.CheckPetGainQuality(msd.tid)) 
            {
                DataCenter.OpenWindow("PET_GAIN_WINDOW",msd.tid);
            }
		}
		DEBUG.Log("RequestMysteriousShopPurchaseSuccess:" + txt);
	}

	void ResponseMysteriousShopPurchaseFail(string txt) {

		DEBUG.Log("ResponseMysteriousShopPurchaseErr:" + txt);
	}
}

/*
 * 
点击刷新，进行以下逻辑判断
1. 判断当前是否有免费次数，有则刷新，没有则进入2
2. 判断当前是否有剩余刷新次数，有则进入3，没有则进入4
3. 判断当前是否有刷新令数量，有则刷新，没有则进入4
4. 判断当前是否有元宝，有则刷新，没有则弹出充值提示
 */
class Button_black_market_update:CEvent {
	SC_MysteryShopQuery net;
	int useWhat = 0;//1,free 2,cmd 3,yb
	public override bool _DoEvent ()
	{
		net = get("NET_DATA").mObj as SC_MysteryShopQuery;
		ConsumeItemLogicData itemData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
		ConsumeItemData refreshTokenCid = itemData.GetDataByTid((int)ITEM_TYPE.REFRESH_TOKEN);
		int usrYB = RoleLogicData.Self.diamond;

        //by chenliang
        //begin

//		if(net.freeNum > 0) useWhat = 1;
//---------------
        int tmpMaxFreeCount = NewMysteriousShopWindow.GetMaxFreeRefreshCount();
        if(net.freeNum > 0)
        {
            if (net.freeNum == tmpMaxFreeCount)
                useWhat = 1;
            else
                useWhat = 2;
        }

        //end
		else if(net.leftNum > 0) {
			if(refreshTokenCid.itemNum > 0) {
				useWhat = (int)ITEM_TYPE.REFRESH_TOKEN;
			} else {
				if(usrYB >= 20) {
					useWhat = (int)ITEM_TYPE.YUANBAO;
				} else {
					//DataCenter.OpenMessageWindow("用户元宝不够，请去充值。");
					GameCommon.ToGetDiamond();
					return true;
				}
			}
		} else {
			DataCenter.OpenMessageWindow("今天刷新次数已经用完。");
			return true;
			// useWhat = (int)ITEM_TYPE.YUANBAO;
		}

		/*
		// 没剩余次数或有计时
		if(net.freeNum <= 0 || net.leftTime > 0) {
			// 没剩余次数
			if(net.leftNum <= 0) {
				if(usrYB < 20) {
					DataCenter.OpenMessageWindow("用户元宝不够，请去充值。");
					return true;
				} else {
					useWhat = (int)ITEM_TYPE.YUANBAO;
				}
			} else if(refreshTokenCid.itemNum > 0) {
				useWhat = (int)ITEM_TYPE.REFRESH_TOKEN;
			} else if(usrYB < 20){
				DataCenter.OpenMessageWindow("用户元宝不够，请去充值。");
				return true;
			} else {
				useWhat = (int)ITEM_TYPE.YUANBAO;
			}
		} else if(net.leftTime <= 0) {
			useWhat = 1;
		}
		*/
        tWindow _newMysteryWin = DataCenter.GetData("NEW_MYSTERIOUS_SHOP_WINDOW") as tWindow;
        if (_newMysteryWin != null) 
        {
            _newMysteryWin.ResetGridTweenFlag();
        }
		ItemDataBase idb = new ItemDataBase();
		List<MysteriousShopData> ls = TableCommon.GetMysteryInfo();
		idb.tid = useWhat;
		idb.itemNum = useWhat == (int)ITEM_TYPE.REFRESH_TOKEN ? 1 : useWhat == (int)ITEM_TYPE.YUANBAO ? 20 : -1;
		idb.itemId = itemData.GetItemIdByTid(useWhat);
		NewMysteriousShopNet.RequestMysteriousShopRefresh(idb, RequestRefreshShopSuccess, RequestRefreshShopFail);
		return true;
	}
	/*
	 * 
	public int[] indexArr;
	public int freeNum;
	public long leftTime;
	public int leftNum;
	 */
	void RequestRefreshShopSuccess(string txt) {
		SC_MysteryShopRefresh msr = JCode.Decode<SC_MysteryShopRefresh>(txt);
		if(msr.ret == (int)STRING_INDEX.ERROR_NONE) {
			SC_MysteryShopQuery msq = new SC_MysteryShopQuery();
			int freeNumNative = net.freeNum;
			int leftNumNative = net.leftNum;
			int[] indexArrNative = msr.indexArr;
			long leftTimeNative = net.leftTime;
			switch(useWhat) {
			case 1: // free
			{
				freeNumNative -= 1;
				leftTimeNative = 120 * 60;
				break;
			}
            //by chenliang
            //begin

            case 2:
            {
                freeNumNative -= 1;
                //除了免费次数满时，用刷新时间不需要从头开始
                leftTimeNative = -1;
            }break;

            //end
			case (int)ITEM_TYPE.REFRESH_TOKEN: // 刷新令牌
			{
				leftNumNative -= 1;
				leftTimeNative = -1;
				PackageManager.RemoveItem((int)ITEM_TYPE.REFRESH_TOKEN, -1, 1);
				break;
			}
			case (int)ITEM_TYPE.YUANBAO: //元宝
			{
				leftNumNative -= 1;
				leftTimeNative = -1;
				PackageManager.RemoveItem((int)ITEM_TYPE.YUANBAO, -1, 20);
				break;
			}
			}
			msq.freeNum = freeNumNative;
			msq.leftNum = leftNumNative;
			msq.indexArr = indexArrNative;
			msq.leftTime = leftTimeNative;
            //by chenliang
            //begin

            msq.mysteryArr = new List<int>();

            //end

			DataCenter.SetData("NEW_MYSTERIOUS_SHOP_WINDOW", "REFRESH", msq);
		}
		DEBUG.Log("RequestRefreshShopSuccess:" + txt);
	}

	void RequestRefreshShopFail(string txt) {

		DEBUG.Log("RequestRefreshShopErr:" + txt);
	}
}

class Button_black_market_get_button:CEvent {

	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("NEW_MYSTERIOUS_SHOP_WINDOW");
		DataCenter.OpenWindow("RECOVER_WINDOW");
		return true;
	}
}