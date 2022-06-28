using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;
using System.Linq;

public class ActivityFundWindow : tWindow 
{
	int iHaveBuyNum = 0;
	int iFundTaskNum = 0;
	int iWelfareTaskNum = 0;
	List<int> fundArrList;
	bool isHaveBuy = false;
	GameObject fundBuyTipsWindowObj;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent ("Button_title_fund_button", new DefineFactory<Button_title_fund_button>());
		EventCenter.Self.RegisterEvent ("Button_title_welfare_button", new DefineFactory<Button_title_welfare_button>());
		EventCenter.Self.RegisterEvent ("Button_activity_fund_buy_button", new DefineFactory<Button_activity_fund_buy_button>());
		EventCenter.Self.RegisterEvent ("Button_activity_welfare_buy_button", new DefineFactory<Button_activity_welfare_buy_button>());
		EventCenter.Self.RegisterEvent ("Button_activity_fund_get_button", new DefineFactory<Button_activity_fund_get_button>());
		EventCenter.Self.RegisterEvent ("Button_activity_welfare_get_button", new DefineFactory<Button_activity_welfare_get_button>());
		EventCenter.Self.RegisterEvent ("Button_fund_sure_buy_btn", new DefineFactory<Button_fund_sure_buy_btn>());
		EventCenter.Self.RegisterEvent ("Button_fund_cancel_buy_btn", new DefineFactory<Button_fund_cancel_buy_btn>());
		EventCenter.Self.RegisterEvent ("Button_fund_go_recharge_btn", new DefineFactory<Button_fund_go_recharge_btn>());

		foreach(var v in DataCenter.mFundEvent.GetAllRecord ())
		{
			if(v.Value["TYPE"] == 1)
			{
				iFundTaskNum++;
			}else if(v.Value["TYPE"] == 2)
			{
				iWelfareTaskNum ++;
			}
		}
	}

	public override void Open(object param)
	{
		base.Open (param);
		fundBuyTipsWindowObj = GameCommon.FindObject (mGameObjUI, "fund_buy_tips_window").gameObject;
		fundBuyTipsWindowObj.SetActive (false);
		NetManager.RequstActivityFundQuery();
		GameCommon.ToggleTrue(GameCommon.FindObject (mGameObjUI, "title_fund_button"));
		GameCommon.FindObject (mGameObjUI, "fund_info_window").SetActive (true);
		GameCommon.FindObject (mGameObjUI, "welfare_info_window").SetActive (false);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		
		switch(keyIndex)
		{
		case "FUND_TITLE":
			GameCommon.ToggleTrue(GameCommon.FindObject (mGameObjUI, "title_fund_button"));
			GameCommon.FindObject (mGameObjUI, "fund_info_window").SetActive (true);
			GameCommon.FindObject (mGameObjUI, "welfare_info_window").SetActive (false);
			UpdateFundUI(fundArrList);
			break;
		case "WELFARE_TITLE":
			GameCommon.ToggleTrue(GameCommon.FindObject (mGameObjUI, "title_welfare_button"));
			GameCommon.FindObject (mGameObjUI, "fund_info_window").SetActive (false);
			GameCommon.FindObject (mGameObjUI, "welfare_info_window").SetActive (true);
			UpdateWelfareUI();
			break;		
		case "ACTIVITY_FUND_QUERY":
			SC_FundQuery item = JCode.Decode<SC_FundQuery>((string)objVal);
			if( null == item )
				return ;
			iHaveBuyNum = item.buyNum;
			fundArrList = item.fundArr.ToList();
			isHaveBuy = item.isBuy;
			UpdateFundUI(fundArrList);
            //RefreshNewMark();
			break;
		case "FUND_GO_BUY_MESSAGE":
			FundBuy();
			break;
		case "ACTIVITY_FUND_PURCHASE":
			isHaveBuy = (bool)objVal;
			iHaveBuyNum++;
			PackageManager.RemoveItem((int)ITEM_TYPE.YUANBAO, -1, 1000);
			UpdateFundUI(fundArrList);
			break;
		case "ACTIVITY_FUND_REWARDS":
			fundArrList.Add ((int)objVal);
			ItemDataBase _itemDate = new ItemDataBase();
			_itemDate.tid = (int)ITEM_TYPE.YUANBAO;
			_itemDate.itemNum = TableCommon.GetNumberFromFundEvent ((int)objVal, "NUM");
			PackageManager.AddItem (_itemDate);
			ItemDataBase[] itemDatas = new ItemDataBase[1];
			itemDatas[0] = _itemDate;
			DataCenter.OpenWindow ("AWARDS_TIPS_WINDOW", itemDatas);
			UpdateFundUI(fundArrList);
            //RefreshNewMark();
			break;
		case "ACTIVITY_WELFARE_REWARDS":
			GetWelfareRewards((string)objVal);
			break;
		case "ACTIVITY_WELFARE_INDEX":
			fundArrList.Add ((int)objVal);
			UpdateWelfareUI();
            RefreshNewMark();
			break;
		}
	}

	void GetWelfareRewards(string text)
	{
		SC_WelfareReward item = JCode.Decode<SC_WelfareReward>(text);
		if(item == null)
			return;

		List<ItemDataBase> itemDataList = PackageManager.UpdateItem (item.items);
		DataCenter.OpenWindow ("AWARDS_TIPS_WINDOW", itemDataList);
	}
	void FundBuy()
	{
		fundBuyTipsWindowObj.SetActive (true);
		UILabel buyTipsLabel = fundBuyTipsWindowObj.transform.Find("buy_tips_label").GetComponent<UILabel>();
		GameObject sureBuyBtn = fundBuyTipsWindowObj.transform.Find ("fund_sure_buy_btn").gameObject;
		sureBuyBtn.SetActive (true);
		GameObject goRechargeBtn = fundBuyTipsWindowObj.transform.Find ("fund_go_recharge_btn").gameObject;
		goRechargeBtn.SetActive (true);
		
		if(RoleLogicData.Self.vipLevel < 2)
		{
			sureBuyBtn.SetActive (false);
			goRechargeBtn.SetActive (true);
            buyTipsLabel.text = TableCommon.getStringFromStringList(STRING_INDEX.ERROR_SHOP_VIP_LEVEL_LOW);//"VIP等级不足，是否前往充值？";		
		}
        else 
		{
			if(RoleLogicData.Self.diamond < 1000)
			{
                buyTipsLabel.text = TableCommon.getStringFromStringList(STRING_INDEX.RECHAGE_MAIN_TIPS);//"元宝不足，是否前往充值？";
				sureBuyBtn.SetActive (false);
				goRechargeBtn.SetActive (true);
			}
            else
			{
				sureBuyBtn.SetActive (true);
				goRechargeBtn.SetActive (false);
				buyTipsLabel.text = "是否确定购买？";
			}			
		}
		GameCommon.GetButtonData(fundBuyTipsWindowObj.transform.Find ("fund_cancel_buy_btn").gameObject).set("CLOSE_OBJ", (GameObject)fundBuyTipsWindowObj);
		GameCommon.GetButtonData(fundBuyTipsWindowObj.transform.Find ("fund_sure_buy_btn").gameObject).set("CLOSE_OBJ", (GameObject)fundBuyTipsWindowObj);
        GameCommon.GetButtonData(fundBuyTipsWindowObj.transform.Find("fund_go_recharge_btn").gameObject).set("CLOSE_OBJ", (GameObject)fundBuyTipsWindowObj);
        
	}

    private void RefreshNewMark() 
    {
        GameObject _serverFund = GameCommon.FindObject(mGameObjUI, "title_fund_button");
        GameObject _universalFund = GameCommon.FindObject(mGameObjUI, "title_welfare_button");

        GameCommon.SetNewMarkVisible(_serverFund,SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FUND_DIAMOND));
        GameCommon.SetNewMarkVisible(_universalFund,SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FUND_WELFARE));

        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_FUND);
    }

	public void UpdateFundUI(List<int> fundArrList)
	{
		GameObject fundObj = GameCommon.FindObject (mGameObjUI, "fund_info_window").gameObject;
		SetBuyNum(fundObj);
		GameCommon.FindObject (fundObj, "activity_fund_buy_button").GetComponent<UIImageButton>().isEnabled = !isHaveBuy;
		GameCommon.FindObject (fundObj, "have_get_label").gameObject.SetActive (isHaveBuy);
		GameCommon.FindObject (fundObj, "vip_label").gameObject.SetActive (!isHaveBuy);

		if(!isHaveBuy)
		{
			if(CommonParam.isOnLineVersion)
			{
				GameCommon.SetUIText (fundObj, "vip_label", "VIP 2");
			}else
			{
				GameCommon.SetUIText (fundObj, "vip_label", "2级至尊");
			}
		}

		UIGridContainer fundGrid = GameCommon.FindObject (fundObj, "fund_grid").GetComponent<UIGridContainer>();
		fundGrid.MaxCount = iFundTaskNum;
		int iRecordIndex = 1001;
		int iHaveGetGold = 0;
        //added by xuke 红点相关
        int _canGetCount = 0;
        //end
		for(int i = 0; i < fundGrid.MaxCount; i++)
		{
			GameObject obj = fundGrid.controlList[i];
			GameObject activityFundGetButton = GameCommon.FindObject (obj, "activity_fund_get_button").gameObject;
			GameObject activityFundGetSprite = GameCommon.FindObject (obj, "activity_fund_get_sprite").gameObject;
			UILabel limitLabel = GameCommon.FindObject (obj, "limit_label").GetComponent<UILabel>();
			limitLabel.text = TableCommon.GetStringFromFundEvent (i + iRecordIndex, "DESC");
			GameObject fundIconTipsBtn = GameCommon.FindObject (obj, "fund_icon_tips_btn").gameObject;
			UILabel itemNumLabel = GameCommon.FindObject(fundIconTipsBtn, "item_num").GetComponent<UILabel>();
			itemNumLabel.text = "x" + TableCommon.GetNumberFromFundEvent (i + iRecordIndex, "NUM");
			int _iTid = TableCommon.GetNumberFromFundEvent(i + iRecordIndex, "ITEM_ID");
			GameCommon.SetOnlyItemIcon(fundIconTipsBtn, "item_icon", _iTid);
			AddButtonAction (fundIconTipsBtn, () => GameCommon.SetItemDetailsWindow (_iTid));
			int limitNum = TableCommon.GetNumberFromFundEvent (i + iRecordIndex, "LEVEL");

			bool isRewards = fundArrList.IndexOf(i + iRecordIndex) != -1;
			activityFundGetButton.SetActive (!isRewards);
			activityFundGetSprite.SetActive (isRewards);
			if(isRewards == true)
			{
				iHaveGetGold += TableCommon.GetNumberFromFundEvent (i + iRecordIndex, "NUM");
			}
			if(activityFundGetButton)
			{
                if (limitNum <= RoleLogicData.GetMainRole().level && isHaveBuy)
                {
                    activityFundGetButton.GetComponent<UIImageButton>().isEnabled = true;
                    //added by xuke 红点相关
                    if(!isRewards)
                    {
                        _canGetCount++;
                    }
                    //end
                }
                else 
                {
                    activityFundGetButton.GetComponent<UIImageButton>().isEnabled = false;
                }					
			}
			GameCommon.GetButtonData(activityFundGetButton).set("FUND_GET_INDEX", i + iRecordIndex);
		}
        //added by xuke 红点相关
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FUND_DIAMOND, (_canGetCount > 0) || (!isHaveBuy && RoleLogicData.Self.vipLevel >= 2 && RoleLogicData.Self.diamond >= 1000));
        RefreshNewMark();
        //end
		GameCommon.FindObject (fundObj, "have_get_num").GetComponent<UILabel>().text = iHaveGetGold.ToString ();
		GameCommon.FindObject (fundObj, "can_get_num").GetComponent<UILabel>().text = (10000 - iHaveGetGold).ToString ();

	}

	public void UpdateWelfareUI()
	{
		GameObject welfareObj = GameCommon.FindObject (mGameObjUI, "welfare_info_window").gameObject;
		SetBuyNum(welfareObj);
		UIGridContainer welfareGrid = GameCommon.FindObject (welfareObj, "welfare_grid").GetComponent<UIGridContainer>();
		welfareGrid.MaxCount = iWelfareTaskNum;
		int iRecordIndex = 2001;
        //added by xuke 红点相关
        int _canGetCount = 0;
        //end
		for(int i = 0; i < welfareGrid.MaxCount; i++)
		{
			GameObject obj = welfareGrid.controlList[i];
			GameObject activityWelfareGetButton = GameCommon.FindObject (obj, "activity_welfare_get_button").gameObject;
			GameObject activityWelfareGetSprite = GameCommon.FindObject (obj, "activity_welfare_get_sprite").gameObject;
			UILabel limitLabel = GameCommon.FindObject (obj, "limit_label").GetComponent<UILabel>();
			limitLabel.text = TableCommon.GetStringFromFundEvent (i + iRecordIndex, "DESC");
			int limitNum = TableCommon.GetNumberFromFundEvent (i + iRecordIndex, "COUNT");

			bool isRewards = fundArrList.IndexOf(i + iRecordIndex) != -1;
			activityWelfareGetButton.SetActive (!isRewards);
			activityWelfareGetSprite.SetActive (isRewards);
			if(activityWelfareGetButton)
			{
                if (limitNum <= iHaveBuyNum)
                {
                    activityWelfareGetButton.GetComponent<UIImageButton>().isEnabled = true;
                    if (!isRewards)
                    {
                        _canGetCount++;
                    }
                }
                else 
                {
                    activityWelfareGetButton.GetComponent<UIImageButton>().isEnabled = false;                
                }
			}

			string rewards = TableCommon.GetStringFromFundEvent (i + iRecordIndex, "REWARD");
			string[] rewardsInfo = rewards.Split('|');
			int rewardsTypeNum = rewardsInfo.Length;
			for(int j = 0; j < rewardsTypeNum ; j++)
			{
				if(rewardsInfo[j] == "0" || rewardsInfo[j] == "")
				{
					rewardsTypeNum--;
				}
			}
			UIGridContainer Grid = GameCommon.FindObject (obj, "Grid").GetComponent<UIGridContainer>();
			Grid.MaxCount = rewardsTypeNum;
			for (int j = 0; j < Grid.MaxCount; j++)
			{
				string[] rewardIdAndNum = rewardsInfo[j].Split('#');
				int iRewardsTid = System.Convert.ToInt32(rewardIdAndNum[0]);
				string iRewardsNumStr = rewardIdAndNum[1];				
				GameObject itemObj = Grid.controlList[j];
				GameObject welfareIconTips = GameCommon.FindObject (itemObj, "welfare_icon_tips").gameObject;
				UILabel itemNumLabel = GameCommon.FindObject(welfareIconTips, "item_num").GetComponent<UILabel>();
				itemNumLabel.text = "x" + iRewardsNumStr;
				GameCommon.SetOnlyItemIcon(welfareIconTips, "item_icon", iRewardsTid);
				AddButtonAction (welfareIconTips, () => GameCommon.SetItemDetailsWindow (iRewardsTid));
			}
			GameCommon.GetButtonData(activityWelfareGetButton).set("WELFARE_GET_INDEX", i + iRecordIndex);
		}
        //added by xuke
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FUND_WELFARE, _canGetCount > 0);
        //end
	}

	void SetBuyNum(GameObject obj)
	{
		GameObject curVipObj = obj.transform.Find ("title_info/cur_vip_label").gameObject;
		GameObject curVipLabelObj = obj.transform.Find ("title_info/cur_vip_level_label").gameObject;
		curVipObj.SetActive (false);
		curVipLabelObj.SetActive (false);
		if(CommonParam.isOnLineVersion)
		{
			curVipLabelObj.SetActive (true);
			GameCommon.SetUIText(curVipLabelObj, "cur_vip_level", "VIP " + RoleLogicData.Self.vipLevel);
		}else
		{
			curVipObj.SetActive (true);
			GameCommon.SetUIText(curVipObj, "cur_vip_level", RoleLogicData.Self.vipLevel.ToString ());
		}
//		UILabel curVipLevel = GameCommon.FindObject (obj, "cur_vip_level").GetComponent<UILabel>();
		UILabel buyNumSmall = GameCommon.FindObject (obj, "buy_num_label_small").GetComponent<UILabel>();
		UILabel buyNumBig = GameCommon.FindObject (obj, "buy_num_label_big").GetComponent<UILabel>();
//		curVipLevel.text = RoleLogicData.Self.vipLevel.ToString ();
		buyNumSmall.text = iHaveBuyNum.ToString ();
		buyNumBig.text = iHaveBuyNum.ToString ();
	}
}

public class Button_title_fund_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ACTIVITY_FUND_WINDOW", "FUND_TITLE", true);
		return true;
	}
}
public class Button_title_welfare_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ACTIVITY_FUND_WINDOW", "WELFARE_TITLE", true);
		return true;
	}
}
public class Button_activity_fund_buy_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ACTIVITY_FUND_WINDOW", "FUND_GO_BUY_MESSAGE", true);
		return true;
	}
}

public class Button_activity_welfare_buy_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ACTIVITY_FUND_WINDOW", "FUND_TITLE", true);
		return true;
	}
}
public class Button_activity_fund_get_button : CEvent
{
	public override bool _DoEvent()
	{
		int iIndex = get ("FUND_GET_INDEX");
		NetManager.RequstActivityFundReward(iIndex);
		return true;
	}
}
public class Button_fund_sure_buy_btn : CEvent
{
	public override bool _DoEvent()
	{
		GameObject closeObject = (GameObject)getObject ("CLOSE_OBJ");
		closeObject.SetActive (false);
		NetManager.RequstActivityFundPurchase();
		return true;
	}
}
public class Button_fund_cancel_buy_btn : CEvent
{
	public override bool _DoEvent()
	{
		GameObject closeObject = (GameObject)getObject ("CLOSE_OBJ");
		closeObject.SetActive (false);
		return true;
	}
}
public class Button_fund_go_recharge_btn : CEvent
{
	public override bool _DoEvent()
	{
        GameObject closeObject = (GameObject)getObject("CLOSE_OBJ");
        closeObject.SetActive(false);
        GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, CommonParam.rechageDepth);
		return true;
	}
}
public class Button_activity_welfare_get_button : CEvent
{
	public override bool _DoEvent()
	{
		int iIndex = get ("WELFARE_GET_INDEX");
		NetManager.RequstActivityWelfareReward(iIndex);
		return true;
	}
}