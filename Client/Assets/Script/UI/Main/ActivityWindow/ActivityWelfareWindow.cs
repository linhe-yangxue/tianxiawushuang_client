using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable ;
using System.Collections.Generic;
using System.Linq;

//VIP每日福利对象
public class Welfare
{
	public int preVipLevel = 0; // 刷新时间 用于领取每日福利每日刷新 
	public int vipLevel = 0; // 玩家当前的vip等级 
	public int[] array; // 福利是否已领取 
}
//VIP每周福利领取对象
public class WeekVipPackage
{
    public int index;      //> 索引ID
    public int buyNum;     //> 购买数量
}

public class ActivityWelfareWindow : tWindow 
{
	int iVipLevelMax = 0;
	public Welfare dalayWelfareItem;
	int iLeftTime;
	GameObject buyTipsWindowObj;
    //added by xuke
    private bool mHasCanGetReward = false;  //> 是否有能够领取的奖励

    private List<WeekVipPackage> mWeekVipPackageList = null;
    private UIGridContainer mWeekVipGridContainer = null;
    //end
	public override void Init()
	{
		EventCenter.Self.RegisterEvent ("Button_everyday_welfare_button", new DefineFactory<Button_everyday_welfare_button>());
		EventCenter.Self.RegisterEvent ("Button_weekly_welfare_button", new DefineFactory<Button_weekly_welfare_button>());
		EventCenter.Self.RegisterEvent ("Button_activity_welfare_get_btn", new DefineFactory<Button_activity_welfare_get_btn>());
		EventCenter.Self.RegisterEvent ("Button_activity_welfare_recharge_btn", new DefineFactory<Button_activity_welfare_recharge_btn>());

		EventCenter.Self.RegisterEvent ("Button_weekly_welfare_sure_buy_btn", new DefineFactory<Button_weekly_welfare_sure_buy_btn>());
		EventCenter.Self.RegisterEvent ("Button_weekly_welfare_cancel_buy_btn", new DefineFactory<Button_weekly_welfare_cancel_buy_btn>());
		EventCenter.Self.RegisterEvent ("Button_weekly_welfare_go_recharge_btn", new DefineFactory<Button_weekly_welfare_go_recharge_btn>());

		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mVipListConfig.GetAllRecord ())
		{
			if(v.Key != null)
			{
				iVipLevelMax ++;
			}
		}

	}

    protected override void OpenInit()
    {
        base.OpenInit();
        mWeekVipGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "vip_package_grid");
    }

	public override void Open(object param)
	{
		base.Open (param);
		buyTipsWindowObj = GameCommon.FindObject (mGameObjUI, "buy_tips_window").gameObject;
		buyTipsWindowObj.SetActive (false);

		NetManager.RequstVIPGiftQuery();
	}
	public void SetInfo()
	{
		DataCenter.SetData ("ACTIVITY_WELFARE_WINDOW", "EVERYDAY_WELFARE_BUTTON", true);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

		switch(keyIndex)
		{
		case "VIP_GIFT_QUERY":
			VipGiftQuery((string)objVal);
			break;
        case "VIP_WEEK_GIFT_QUERY":
            VipWeekGiftQuery((string)objVal);
            break;
		case "EVERYDAY_WELFARE_BUTTON":
			GameCommon.ToggleTrue(GameCommon.FindObject (mGameObjUI, "everyday_welfare_button"));
			GameCommon.FindObject (mGameObjUI, "everyday_welfare_window").SetActive (true);
			GameCommon.FindObject (mGameObjUI, "weekly_welfare_window").SetActive (false);
			UpdateEverydayWelfare();
            //RefreshNewMark();
			break;
		case "WEEKLY_WELFARE_BUTTON":
			GameCommon.ToggleTrue(GameCommon.FindObject (mGameObjUI, "weekly_welfare_button"));
			GameCommon.FindObject (mGameObjUI, "everyday_welfare_window").SetActive (false);
			GameCommon.FindObject (mGameObjUI, "weekly_welfare_window").SetActive (true);
			UpdateWeeklyWelfare();
			break;
		case "REQUST_WELFARE_GET":
			NetManager.RequstVIPDailyWelfare((int)objVal);
			break;
		case "WEEKLY_WELFARE_BUY_BUTTON":
			WeeklyWelfareBuy((int)objVal);
			break;
		case "VIP_DAILY_WELFARE":
			VipDailyWelfare((string)objVal);
            RefreshNewMark();
			break;
		case "VIP_WEEK_WELFARE":
			VipWeekWelfare((string)objVal);
			break;
		case "WEEKLY_WELFARE_BUY_MESSAGE":
			NetManager.RequstVIPWeekWelfare ((int)objVal);
			break;
        case "REFRESH_VIP_NEWMARK":
            RefreshNewMark();
            break;
		}
	}
    private int mCurBuyPackageIndex = 0;   //> 当前购买的礼包的Index
	void WeeklyWelfareBuy(int kIndex)
	{
		buyTipsWindowObj.SetActive (true);
		UILabel buyTipsLabel = buyTipsWindowObj.transform.Find("buy_tips_label").GetComponent<UILabel>();
		GameObject sureBuyBtn = buyTipsWindowObj.transform.Find ("weekly_welfare_sure_buy_btn").gameObject;
		sureBuyBtn.SetActive (true);
		GameObject goRechargeBtn = buyTipsWindowObj.transform.Find ("weekly_welfare_go_recharge_btn").gameObject;
		goRechargeBtn.SetActive (true);

        DataRecord _weekVipPackageRecord = DataCenter.mWeekGiftEventConfig.GetRecord(kIndex);
        if (_weekVipPackageRecord == null)
            return;
        int needMoney = _weekVipPackageRecord.getData("PRICE");

        //1.判断等级是否符合要求
        if (!IsInRange(_weekVipPackageRecord.getData("CONDITION")))
        {
            DEBUG.LogError("等级不在范围内");
            if (GameObject.Find("connect_error_tologin_window") == null)
                DataCenter.OpenWindow("CONNECT_ERROR_TOLOGIN_WINDOW");
            DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW", "WINDOW_CONTENT", STRING_INDEX.ERROR_NET_LOGIN_BY_OTHER);
            return;
            //___________________________________________________
        }
        //2.判断购买次数
        int _hasBuyNum = GetAlreadyBuyNumByIndex(_weekVipPackageRecord.getIndex());
        if (_hasBuyNum >= _weekVipPackageRecord.getData("BUY_NUM"))
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SHOP_NO_ENOUGH_BUY_NUM, () => 
            { 
                DEBUG.Log("刷新界面");
                NetManager.RequestVIPWeekGiftQuery(UpdateWeeklyWelfare);
            });
        }
        //3.判断元宝数量
        if (RoleLogicData.Self.diamond < needMoney)
        {
            buyTipsLabel.text = "元宝不足，是否前往充值？";
            sureBuyBtn.SetActive(false);
            goRechargeBtn.SetActive(true);
        }
        else 
        {
            sureBuyBtn.SetActive(true);
            goRechargeBtn.SetActive(false);
            buyTipsLabel.text = "是否确定购买？";

            mCurBuyPackageIndex = kIndex;
            GameCommon.GetButtonData(sureBuyBtn).set("BUY_VIP_INDEX", kIndex);
        }

		GameCommon.GetButtonData(buyTipsWindowObj.transform.Find ("weekly_welfare_cancel_buy_btn").gameObject).set("CLOSE_OBJ", (GameObject)buyTipsWindowObj);
		GameCommon.GetButtonData(sureBuyBtn).set("CLOSE_OBJ", (GameObject)buyTipsWindowObj);
        GameCommon.GetButtonData(goRechargeBtn).set("CLOSE_OBJ", (GameObject)buyTipsWindowObj);
	}
	void VipWeekWelfare(string text)
	{
        SC_VIPWeek item = JCode.Decode<SC_VIPWeek>(text);
        if (null == item)
            return;

        List<ItemDataBase> itemBaseList = PackageManager.UpdateItem(item.rewards);
        DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemBaseList);

        DataRecord _weekVipPackageRecord = DataCenter.mWeekGiftEventConfig.GetRecord(mCurBuyPackageIndex);
        if(_weekVipPackageRecord == null)
            return;
        int curNeedYuanbao = _weekVipPackageRecord.getData("PRICE");
        PackageManager.RemoveItem((int)ITEM_TYPE.YUANBAO, -1, curNeedYuanbao);

        bool alreadyHasBought = false;
        for(int i = 0,count = mWeekVipPackageList.Count;i < count;i++)
        {
            if (mWeekVipPackageList[i] == null)
                continue;
            if(mWeekVipPackageList[i].index == mCurBuyPackageIndex)
            {
                alreadyHasBought = true;
                mWeekVipPackageList[i].buyNum++;
            }
        }
        if (!alreadyHasBought) 
        {
            mWeekVipPackageList.Add(new WeekVipPackage() {index = mCurBuyPackageIndex,buyNum = 1});
        }
        UpdateWeeklyWelfare();
	}

	void VipDailyWelfare(string text)
	{
		SC_VIPDaily item = JCode.Decode<SC_VIPDaily>(text);
		if( null == item )
			return ;

		List<ItemDataBase> itemBaseList = PackageManager.UpdateItem (item.rewards);
		DataCenter.OpenWindow ("AWARDS_TIPS_WINDOW", itemBaseList);
		//dalayWelfareItem.isWelfare = true;
		NetManager.RequstVIPGiftQuery();
		//UpdateEverydayWelfare();
	}

	void VipGiftQuery(string text)
	{
		SC_VIPDailyInfoQuery item = JCode.Decode<SC_VIPDailyInfoQuery>(text);
		if( null == item )
			return ;

		dalayWelfareItem = item.dailyWelfare;

	    //mWeekVipPackageList = item.weekWelfare.ToList<WeekVipPackage>();

		//iLeftTime = item.leftTime;
		//SetCountdownTime ("label05",(Int64)(CommonParam.NowServerTime() +  iLeftTime ) ,new CallBack(this, "TimeIsOver", true));
		SetInfo();
	}

    private void VipWeekGiftQuery(string text) 
    {
        SC_VIPWeeklyInfoQuery item = JCode.Decode<SC_VIPWeeklyInfoQuery>(text);
        if (null == item)
            return;
        mWeekVipPackageList = item.weekWelfare.ToList<WeekVipPackage>();
    }

    private void RefreshNewMark() 
    {
        GameObject _dayWelfareBtnObj = GameCommon.FindObject(mGameObjUI, "everyday_welfare_button");   //> 每日福利按钮
        GameObject _weekWelfareBtnObj = GameCommon.FindObject(mGameObjUI, "weekly_welfare_button");    //> 每周礼包按钮

        bool _dayState = SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_WELFARE_DAY);
        bool _weekState = SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_WELFARE_WEEK);
        GameCommon.SetNewMarkVisible(_dayWelfareBtnObj,_dayState );
        GameCommon.SetNewMarkVisible(_weekWelfareBtnObj, _weekState);

        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_WELFARE);
    }
	void UpdateEverydayWelfare()
	{
        //已经领取每日礼包的数量
        int _hasGotRewardNum = 0;

		int curVipLevel = dalayWelfareItem.vipLevel;
		int preVip = dalayWelfareItem.preVipLevel;
		GameObject obj = GameCommon.FindObject (mGameObjUI, "activity_welfare_group").gameObject;
		UIGridContainer mGrid = obj.transform.Find ("ScrollView/grid").GetComponent<UIGridContainer>();
		if (preVip >= iVipLevelMax - 1) {
			mGrid.MaxCount = 1;
		} else if (preVip == iVipLevelMax - 2) {
			mGrid.MaxCount = 2;
		} else {
			if(curVipLevel < iVipLevelMax - 1){
				mGrid.MaxCount = 2 + curVipLevel - preVip;
			}
			else {
				mGrid.MaxCount = 1 + curVipLevel - preVip;
			}
		}
		for(int i = 0; i < mGrid.MaxCount; i++)
		{
			GameObject vipObj = mGrid.controlList[i];
			UILabel vipLevelLabel = vipObj.transform.Find ("item_icon_info/info_bg/Label/level").GetComponent<UILabel>();
			GameObject activityWelfareGetBtnObj = vipObj.transform.Find ("activity_welfare_get_btn").gameObject;
			GameObject activityWelfareRechargeBtnObj = vipObj.transform.Find ("activity_welfare_recharge_btn").gameObject;
			GameObject activityWelfareHaveGetBtnObj = vipObj.transform.Find ("activity_welfare_have_get_btn").gameObject;
			GameObject nextVipLevelTipsObj = vipObj.transform.Find ("item_icon_info/info_bg/next_vip_level_tips").gameObject;
			GameCommon.GetButtonData(activityWelfareGetBtnObj).set("GET_VIP_LEVEL",i+preVip);
			activityWelfareGetBtnObj.SetActive (true);
			activityWelfareRechargeBtnObj.SetActive (true);
			activityWelfareHaveGetBtnObj.SetActive (true);
			nextVipLevelTipsObj.SetActive (true);
			if(i < mGrid.MaxCount - 1)
			{
				activityWelfareGetBtnObj.SetActive (true);
				activityWelfareRechargeBtnObj.SetActive (false);
				activityWelfareHaveGetBtnObj.SetActive (false);
				nextVipLevelTipsObj.SetActive (false);
			}
			else 
			{
				if(curVipLevel == iVipLevelMax-1)
				{
					activityWelfareGetBtnObj.SetActive (true);
					activityWelfareRechargeBtnObj.SetActive (false);
					activityWelfareHaveGetBtnObj.SetActive (false);
					nextVipLevelTipsObj.SetActive (false);
				}
				else
				{
					activityWelfareGetBtnObj.SetActive (false);
					activityWelfareRechargeBtnObj.SetActive (true); 
					activityWelfareHaveGetBtnObj.SetActive (false);
					nextVipLevelTipsObj.SetActive (true);
				}
			}
			for(int x=0; x < dalayWelfareItem.array.Length; x++)
			{
				if(i+preVip == dalayWelfareItem.array[x])
				{
					activityWelfareGetBtnObj.SetActive (false);
					activityWelfareHaveGetBtnObj.SetActive (true);
                    _hasGotRewardNum++;
				}
			}

			if(CommonParam.isOpenMoveTips)
			{
				vipLevelLabel.text = "VIP " + (i + preVip).ToString () + "   ";
			}else
			{
				vipLevelLabel.text = (i + preVip).ToString () + "级至尊";
			}


			int iNeedNoney = TableCommon.GetNumberFromVipList (i + preVip, "CASHPAID");
			nextVipLevelTipsObj.GetComponent<UILabel>().text = "再充值" + iNeedNoney.ToString() + "元宝即可每日领取";

			int iGroupId = TableCommon.GetNumberFromVipList(i + preVip, "DAY_GROUP_ID");
			List<ItemDataBase> items = GameCommon.GetItemGroup(iGroupId,false);
			UIGridContainer welfareRewardGrid = vipObj.transform.Find ("item_icon_info/Grid").GetComponent<UIGridContainer>();
			welfareRewardGrid.MaxCount = items.Count;
			for(int j = 0; j < welfareRewardGrid.MaxCount; j++)
			{
				GameObject itemObj = welfareRewardGrid.controlList[j];
//				GameCommon.SetItemIcon(itemObj, "item_icon", items[j].tid);
				GameCommon.SetOnlyItemIcon (itemObj, "item_icon", items[j].tid);
				int _iTid = items[j].tid; 
				AddButtonAction (GameCommon.FindObject (itemObj, "item_icon"), () => GameCommon.SetItemDetailsWindow (_iTid));
				UILabel itemNumLabel = itemObj.transform.Find ("item_num").GetComponent<UILabel>();
				itemNumLabel.text = "X" + items[j].itemNum.ToString ();
			}
		}
        if(_hasGotRewardNum == curVipLevel + 1)
        {
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_WELFARE_DAY,false);
        }
        RefreshNewMark();
	}

    private bool mHasLeftPackage = true;
	void UpdateWeeklyWelfare()
	{
        mHasLeftPackage = false;

        List<DataRecord> _weekVipRecordList = new List<DataRecord>();
        //1. 根据玩家角色等级和VIP等级显示对应的礼包
        using (var record = DataCenter.mWeekGiftEventConfig.GetAllRecord().GetEnumerator()) 
        {
            while (record.MoveNext()) 
            {
                var _pair = record.Current;
                if (IsInRange(_pair.Value.getData("CONDITION"))) 
                {
                    _weekVipRecordList.Add(_pair.Value);
                }
            }
        }
        if (mWeekVipGridContainer == null) 
        {
            DEBUG.LogError("mWeekVipGridContainer 为空");
            return;
        }
        mWeekVipGridContainer.MaxCount = _weekVipRecordList.Count;
        for (int i = 0, count = _weekVipRecordList.Count; i < count; i++) 
        {
            if (_weekVipRecordList[i] == null)
                continue;
            UpdateWeekVipPackageItem(mWeekVipGridContainer.controlList[i], _weekVipRecordList[i]);
        }

        // 刷新红点
        //SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_WELFARE_WEEK, mHasLeftPackage);
        //RefershNewMark();
	}

    private bool IsInRange(string kStr) 
    {
        string[] _lvArr = kStr.Split(new char[2]{'#','|'});
        if (_lvArr.Length != 4) 
        {
            DEBUG.LogError("表格配置错误");
            return false;
        }
        int _lvMin;
        int _lvMax;
        int _vipMin;
        int _vipMax;
        int.TryParse(_lvArr[0], out _lvMin);
        int.TryParse(_lvArr[1],out _lvMax);
        int.TryParse(_lvArr[2],out _vipMin);
        int.TryParse(_lvArr[3],out _vipMax);

        int _roleLv = RoleLogicData.Self.character.level;
        int _vipLv = RoleLogicData.Self.vipLevel;
        if ((_lvMin <= _roleLv && _roleLv <= _lvMax) && (_vipMin <= _vipLv && _vipLv <= _vipMax))
            return true;
        return false;
    }
    private void UpdateWeekVipPackageItem(GameObject kVipItem,DataRecord kRecord) 
    {
        if (kVipItem == null || kRecord == null)
            return;
        //1.礼包名称
        GameCommon.SetUIText(kVipItem, "title_name", kRecord.getData("TITLE"));
        //2.折扣
        int _disCount = 0;
        int.TryParse(kRecord.getData("DISCOUNT").ToString(),out _disCount);
        DataRecord _disCountRecord = DataCenter.mTipIconConfig.GetRecord(_disCount);
        GameObject _disCountObj = GameCommon.FindObject(kVipItem, "discount_sprite");
        if (_disCountRecord != null)
        {
            if (_disCountObj != null)
                _disCountObj.SetActive(true);
            GameCommon.SetUISprite(kVipItem, "discount_sprite", _disCountRecord["TIPICON_ATLAS_NAME"], _disCountRecord["TIPICON_SPRITE_NAME"]);
        }
        else 
        {          
            if (_disCountObj != null)
                _disCountObj.SetActive(false);
        }
        //3.礼包物品
        UIGridContainer _buyItemGridContainer = GameCommon.FindComponent<UIGridContainer>(kVipItem, "rewards_grid");
        if (_buyItemGridContainer == null)
            return;
        List<ItemDataBase> _itemList = GameCommon.ParseItemList(kRecord["ITEM"]);
        //UI上最多能够同时显示3个物品
        _buyItemGridContainer.MaxCount = _itemList.Count;
        for (int i = 0, count = _buyItemGridContainer.MaxCount; i < count; i++) 
        {
            GameCommon.SetItemIconWithNum(_buyItemGridContainer.controlList[i], _itemList[i]);
            int _tid = _itemList[i].tid;
            AddButtonAction(_buyItemGridContainer.controlList[i], () => GameCommon.SetItemDetailsWindow(_tid));
        }
        //4.重置礼包物品位置
        //GameCommon.RePosScrollview(kVipItem,"vip_item_scrollview");
        //5.显示价格
        int _needDiamond = kRecord.getData("PRICE");
        GameCommon.SetUIText(kVipItem, "cost_num_label","x "+ _needDiamond);
        //6.设置购买状态
        int _leftBuyNum = kRecord.getData("BUY_NUM") - GetAlreadyBuyNumByIndex(kRecord.getIndex());
        //红点
        mHasLeftPackage = mHasLeftPackage || _leftBuyNum != 0;
        //end
        GameObject _leftBuyLbl = GameCommon.FindObject(kVipItem, "left_buy_num_label");
        if (_leftBuyLbl != null) 
        {
            _leftBuyLbl.SetActive(_leftBuyNum > 0);
            UILabel _label = _leftBuyLbl.GetComponent<UILabel>();
            if (_label != null) 
            {
                _label.text = "可购买[99ff66]" + _leftBuyNum + "[-]次";
            }
        }
        GameObject _buyBtnObj = GameCommon.FindObject(kVipItem,"vip_package_buy_btn");
        GameObject _hasBuyObj = GameCommon.FindObject(kVipItem, "rewards_have_buy_btn");

        if (_buyBtnObj != null) 
        {
            _buyBtnObj.SetActive(_leftBuyNum > 0);
        }
        if (_hasBuyObj != null) 
        {
            _hasBuyObj.SetActive(_leftBuyNum <= 0);
        }
        //7.设置购买按钮回调
        AddButtonAction(_buyBtnObj,
        ()=>
        {
            int _index = kRecord.getIndex();
            DataCenter.SetData("ACTIVITY_WELFARE_WINDOW", "WEEKLY_WELFARE_BUY_BUTTON", _index);
        });

    }

    //根据index得到购买该礼包的次数
    public int GetAlreadyBuyNumByIndex(int kIndex) 
    {
        if (mWeekVipPackageList == null)
            return 0;
        for (int i = 0, count = mWeekVipPackageList.Count; i < count; i++) 
        {
            if (mWeekVipPackageList[i] == null)
                continue;
            if (mWeekVipPackageList[i].index == kIndex)
                return mWeekVipPackageList[i].buyNum;
        }
        return 0;
    }
	public  void TimeIsOver()
	{
		Open(null);
	}

}

public class Button_everyday_welfare_button : CEvent
{
	public override bool _DoEvent()
	{

		DataCenter.SetData ("ACTIVITY_WELFARE_WINDOW", "EVERYDAY_WELFARE_BUTTON", true);
		return true;
	}
}
public class Button_weekly_welfare_button : CEvent
{
	public override bool _DoEvent()
	{
        NetManager.RequestVIPWeekGiftQuery(() => 
        {
            DataCenter.SetData("ACTIVITY_WELFARE_WINDOW", "WEEKLY_WELFARE_BUTTON", null);
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_WELFARE_WEEK, false);
            DataCenter.SetData("ACTIVITY_WELFARE_WINDOW", "REFRESH_VIP_NEWMARK", ACTIVITY_TYPE.ACTIVITY_WELFARE);
        });
		
		return true;
	}
}
public class Button_activity_welfare_get_btn : CEvent
{
	public override bool _DoEvent()
	{

		int curviplevel = get ("GET_VIP_LEVEL");
		DataCenter.SetData ("ACTIVITY_WELFARE_WINDOW", "REQUST_WELFARE_GET", curviplevel);
		return true;
	}
}
public class Button_activity_welfare_recharge_btn : CEvent
{
	public override bool _DoEvent()
	{
		GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, ()=>{NetManager.RequstVIPGiftQuery();}, CommonParam.rechageDepth);
		return true;
	}
}
public class Button_weekly_welfare_sure_buy_btn : CEvent
{
	public override bool _DoEvent()
	{
		GameObject closeObject = (GameObject)getObject ("CLOSE_OBJ");
		closeObject.SetActive (false);
		int iBuyVipIndex = get ("BUY_VIP_INDEX");
        DataCenter.SetData("ACTIVITY_WELFARE_WINDOW", "WEEKLY_WELFARE_BUY_MESSAGE", iBuyVipIndex);
		return true;
	}
}
public class Button_weekly_welfare_cancel_buy_btn : CEvent
{
	public override bool _DoEvent()
	{
		GameObject closeObject = (GameObject)getObject ("CLOSE_OBJ");
		closeObject.SetActive (false);
		return true;
	}
}
public class Button_weekly_welfare_go_recharge_btn : CEvent
{
	public override bool _DoEvent()
	{
        GameObject closeObject = (GameObject)getObject("CLOSE_OBJ");
        closeObject.SetActive(false);
        GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, CommonParam.rechageDepth);
		return true;
	}
}
