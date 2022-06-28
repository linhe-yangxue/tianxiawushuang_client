using UnityEngine;
using System.Collections;
using DataTable;
using Logic;
using System;
using System.Collections.Generic;

public class MorrowLandGiftWindow : tWindow
{
	public int iMaxDay = 0;
	Int64 closeOneDay = 0;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_morrow_land_rewards_get_btn", new DefineFactory<Button_morrow_land_rewards_get_btn>());
		EventCenter.Self.RegisterEvent ("Button_morrow_land_rewards_get_btn_gray", new DefineFactory<Button_morrow_land_rewards_get_btn_gray>());
		EventCenter.Self.RegisterEvent ("Button_morrow_land_window_close_btn", new DefineFactory<Button_morrow_land_window_close_btn>());
		foreach(KeyValuePair<int, DataRecord> v in DataCenter.mHDlogin.GetAllRecord())
		{
			if(v.Key != null)
			{
				iMaxDay++;
			}
		}
	}
	public override void Open(object param)
	{
		base.Open (param);

		DateTime startData =  DateTime.Parse (CommonParam.mCreateDate);
		Int64 startSeconds = GameCommon.DateTime2TotalSeconds(startData);
		closeOneDay = startSeconds + 24*60*60;

		Refresh(null);
	}

	public override bool Refresh (object param)
	{
		base.Refresh (param);
		UpdaeRewardsUI();
		UpdateButtons();
		return true;
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

		switch (keyIndex)
		{
		case "GET_REWARDS":
			GetMorrowLandRewards ((string)objVal);		
			break;
		}
	}

	void UpdateButtons()
	{
		GameObject rewardsGetBtnObj = GameCommon.FindObject (mGameObjUI, "morrow_land_rewards_get_btn").gameObject;
		GameObject rewardsGetBtnGrayObj = GameCommon.FindObject (mGameObjUI, "morrow_land_rewards_get_btn_gray").gameObject;
		GameObject haveGetObj = GameCommon.FindObject (mGameObjUI, "morrow_land_rewards_have_get_btn").gameObject;

		if(closeOneDay > GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime ()))
		{
			rewardsGetBtnObj.SetActive (false);
			rewardsGetBtnGrayObj.SetActive (true);
			haveGetObj.SetActive (false);
		}else if(MorrowLandLogicData.Self.dayRewardItem[0].isReward == false)
		{
			rewardsGetBtnObj.SetActive (true);
			rewardsGetBtnGrayObj.SetActive (false);
			haveGetObj.SetActive (false);
		}else
		{
			rewardsGetBtnObj.SetActive (false);
			rewardsGetBtnGrayObj.SetActive (false);
			haveGetObj.SetActive (true);
		}
	}

	void GetMorrowLandRewards(string text)
	{
		SC_LoginReward item = JCode.Decode<SC_LoginReward>(text);
        List<ItemDataBase> itemBaseList = PackageManager.UpdateItem(item.loginReward);
		DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemBaseList);
		MorrowLandLogicData.Self.dayRewardItem[0].isReward = true;
		PackageManager.UpdateItem (item.loginReward);
		GameCommon.FindObject (mGameObjUI, "morrow_land_rewards_get_btn").SetActive (false);
		GameCommon.FindObject (mGameObjUI, "morrow_land_rewards_have_get_btn").SetActive (true);
		PlayerPrefs.SetString ("GET_MORROW_REWARDS", "OK");
		UpdateButtons();
	}

	public  void TimeIsOver(object param)
	{
//		MorrowLandLogicData.Self.iLeftTime = 0;
		GlobalModule.DoCoroutine(_DoAction());	

//		GameCommon.FindObject (mGameObjUI, "count_down_num").SetActive (false);
//		GameCommon.FindObject (mGameObjUI, "can_get_rewards_label").SetActive (true);
		Open(null);
	}
	private IEnumerator _DoAction()
	{
		yield return NetManager.StartWaitMorrowLandQuery();
	}

	void UpdaeRewardsUI()
	{
		if(closeOneDay > GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime ()))
		{
			GameCommon.FindObject (mGameObjUI, "count_down_num").SetActive (true);
			GameCommon.FindObject (mGameObjUI, "can_get_rewards_label").SetActive (false);
			SetCountdownTime ("count_down_num",closeOneDay, null);
		}else 
		{
			GameCommon.FindObject (mGameObjUI, "count_down_num").SetActive (false);
			GameCommon.FindObject (mGameObjUI, "can_get_rewards_label").SetActive (true);
		}

		string strRewardsInfos = TableCommon.GetStringFromHDlogin (1, "ITEM");
		string[] rewardInfos = strRewardsInfos.Split('|');
		int rewardsTypeNum = rewardInfos.Length;
		for(int i = 0; i < rewardsTypeNum ; i++)
		{
			if(rewardInfos[i] == "0" || rewardInfos[i] == "")
			{
				rewardsTypeNum--;
			}
		}

		UIGridContainer rewardsGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "rewards_grid");
		rewardsGrid.MaxCount = rewardsTypeNum;
		if(rewardsGrid != null)
		{
			for(int i = 0; i < rewardsGrid.MaxCount; i++)
			{
				string[] rewardIdAndNum = rewardInfos[i].Split('#');
				int iRewardsTid = System.Convert.ToInt32(rewardIdAndNum[0]);
				string iRewardsNumStr = rewardIdAndNum[1];
				GameObject obj = rewardsGrid.controlList[i];

				GameCommon.SetOnlyItemIcon(obj, "item_icon", iRewardsTid);
				UILabel itemNumLabel = obj.transform.Find("item_icon/item_num").GetComponent<UILabel>();
				itemNumLabel.text = "x" + iRewardsNumStr;
			}
		}
	}

}

public class Button_morrow_land_rewards_get_btn : CEvent
{
	public override bool _DoEvent()
	{
        //预判背包是否已满
        List<ItemDataBase> tmpAwardItems = GameCommon.ParseItemList(TableCommon.GetStringFromHDlogin(1, "ITEM"));
        List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(tmpAwardItems);
        if (!CheckPackage.Instance.CanAddItems(tmpPackageTypes))
        {
            DataCenter.OpenMessageWindow("背包已满");
            return true;
        }


		NetManager.RequestMorrowLandRewards();
		return true;
	}
}
public class Button_morrow_land_rewards_get_btn_gray : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenMessageWindow("明日登陆游戏可领取这些奖励哦！");
		return true;
	}
}
public class Button_morrow_land_window_close_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("MORROW_LAND_WINDOW");
		MainUIScript.Self.OpenMainWindowByIndex (MAIN_WINDOW_INDEX.RoleSelWindow);
		return true;
	}
}