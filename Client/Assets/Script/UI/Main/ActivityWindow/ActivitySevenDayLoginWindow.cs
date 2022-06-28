using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;
using System.Linq;

public class ActivitySevenDayLoginWindow : tWindow
{
	int iMaxDays = 0;
	int iLoginDay = 0;
	List<int> loginRewardsArrList = new List<int>();
    //added by xuke
    private bool mHasCanGetReward = false;  //> 是否有可以领取的奖励
    //end
    public override void Init()
	{
		EventCenter.Self.RegisterEvent ("Button_seven_day_login_get_button", new DefineFactory<Button_seven_day_login_get_button>());

		foreach(var v in DataCenter.mSevenDayLoginEvent.GetAllRecord())
		{
			if(v.Key != null)
				iMaxDays++;
		}
	}

	public override void Open(object param)
	{
		base.Open (param);
//		NetManager.RequstActivitySevenDayLoginQuery();
		UpdateUI(loginRewardsArrList);
	}
	public bool GetActivityIsOpen() 
	{
		bool isOpen = true;
		// 判断登录有礼是否开启
		if( loginRewardsArrList.Count >= iMaxDays)
			isOpen = false;
		return isOpen;
	}
	void SetIsClose()
	{
		if(loginRewardsArrList.Count >= iMaxDays)
		{
			ActivityWindow tmpWin = DataCenter.GetData("ACTIVITY_WINDOW") as ActivityWindow ;
			if(tmpWin != null)
			{
				tmpWin.Refresh(null);	
				DataCenter.CloseWindow ("ACTIVITY_SEVEN_DAY_LOGIN_WINDOW");
			}
		}
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case "ACTIVITY_SEVEN_DAY_LOGIN_QUERY":
			SC_SevenDayLoginQuery item = JCode.Decode<SC_SevenDayLoginQuery>((string)objVal);
			if(item == null)
				return;
			iLoginDay = item.loginDay;
			loginRewardsArrList = item.indexArr.ToList();
//			UpdateUI(loginRewardsArrList);
			SetIsClose();
			GetActivityIsOpen();
			break;
		case "ACTIVITY_SEVEN_DAY_LOGIN_REWARDS":
			GetSevenDayLoginRewards((string)objVal);
			break;
		case "ACTIVITY_SEVEN_DAY_LOGIN_INDEX":
			loginRewardsArrList.Add ((int)objVal);
			UpdateUI(loginRewardsArrList);
			SetIsClose();
			GetActivityIsOpen();
			break;
		}
	}
    private void RefreshNewMark() 
    {
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_SEVEN_DAY_LOGIN, mHasCanGetReward);
        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_SEVEN_DAY_LOGIN);
    }
	void GetSevenDayLoginRewards(string text)
	{
		SC_SevenDayLoginReward item = JCode.Decode<SC_SevenDayLoginReward>(text);
		if(item == null)
			return;
		
		List<ItemDataBase> itemDataList = PackageManager.UpdateItem (item.items);
		DataCenter.OpenWindow ("AWARDS_TIPS_WINDOW", itemDataList);
	}
	void UpdateUI(List<int> loginRewardList)
	{
        //added by xuke
        mHasCanGetReward = false;
        //end
		UIScrollView _ScrollView = GameCommon.FindObject (mGameObjUI, "ScrollView").GetComponent<UIScrollView>();
		UILabel loginNum = GameCommon.FindObject (mGameObjUI, "login_num").GetComponent<UILabel>();
		loginNum.text = iLoginDay + " / " + iMaxDays;

		UIGridContainer sevenDayLoginGrid = GameCommon.FindObject (mGameObjUI, "seven_day_login_grid").GetComponent<UIGridContainer>();
		sevenDayLoginGrid.MaxCount = iMaxDays;       

		for(int i = 0; i < sevenDayLoginGrid.MaxCount; i++)
		{
			GameObject obj = sevenDayLoginGrid.controlList[i];
			GameObject sevenDayLoginGetButton = GameCommon.FindObject (obj, "seven_day_login_get_button").gameObject;
			GameObject sevenDayLoginGetSprite = GameCommon.FindObject (obj, "seven_day_login_get_sprite").gameObject;
			UILabel titleName = GameCommon.FindObject (obj, "title_name").GetComponent<UILabel>();
			UILabel rewardsDes = GameCommon.FindObject (obj, "rewards_des").GetComponent<UILabel>();
			string strRewardsDes =  TableCommon.GetStringFromSevenDayLoginEvent (i + 1, "AWARD_DESCRIBE");
			if (strRewardsDes != "")
			{
				strRewardsDes = strRewardsDes.Replace("\\n", "");
				strRewardsDes = strRewardsDes.Replace("\n", "");
			}
			titleName.text = TableCommon.GetStringFromSevenDayLoginEvent (i + 1, "AWARD_NAME");
			rewardsDes.text = strRewardsDes;
			int loginDayResult = TableCommon.GetNumberFromSevenDayLoginEvent(i + 1, "LOGIN_DAY_REQUEST");

			bool isRewards = loginRewardList.IndexOf(i + 1) != -1;
			sevenDayLoginGetButton.SetActive (!isRewards);
			obj.SetActive (!isRewards);			//
			if(isRewards == true)
			{
				_ScrollView.ResetPosition ();
			}
			sevenDayLoginGetSprite.SetActive (isRewards);

			if(iLoginDay >= loginDayResult)
			{
				sevenDayLoginGetButton.GetComponent<UIImageButton>().isEnabled = true;
                //added by xuke
                if (!isRewards)
                    mHasCanGetReward = true;
                //end
			}else 
				sevenDayLoginGetButton.GetComponent<UIImageButton>().isEnabled = false;

			GameCommon.GetButtonData (sevenDayLoginGetButton).set ("DAY_INDEX", i + 1);

			string rewards = TableCommon.GetStringFromSevenDayLoginEvent (i + 1, "LOGIN_AWARD");
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

			for(int j = 0; j < Grid.MaxCount; j++)
			{
				string[] rewardIdAndNum = rewardsInfo[j].Split('#');
				int iRewardsTid = System.Convert.ToInt32(rewardIdAndNum[0]);
				string iRewardsNumStr = rewardIdAndNum[1];				
				GameObject itemObj = Grid.controlList[j];

				GameObject welfareIconTips = GameCommon.FindObject (itemObj, "welfare_icon_tips").gameObject;
				GameCommon.SetOnlyItemIcon(welfareIconTips, "item_icon", iRewardsTid);
				AddButtonAction (GameCommon.FindObject (welfareIconTips, "item_icon"), () => GameCommon.SetItemDetailsWindow(iRewardsTid));
			}
		}
        sevenDayLoginGrid.Reposition();
        sevenDayLoginGrid.repositionNow = false;
        //added by xuke 刷新红点逻辑
        RefreshNewMark();
        //end
	}
}
public class Button_seven_day_login_get_button : CEvent
{
	public override bool _DoEvent()
	{
		int iIndex = get ("DAY_INDEX");
		NetManager.RequstActivitySevenDayLoginReward (iIndex);
		return true;
	}
}