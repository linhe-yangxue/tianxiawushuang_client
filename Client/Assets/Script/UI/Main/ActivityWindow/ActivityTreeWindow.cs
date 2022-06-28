using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable ;
using System.Collections.Generic;

public class ActivityTreeWindow : tWindow
{
	public const int SHAKETIMES = 6;
	public int iAllNum = 0;
	public int iTodayNum = 0;
	public int iTotalGold = 0;
	public int iLeftTime = 0;
    private GameObject mMask = null;
	GameObject shakeTreeEffectObj;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent ("Button_activity_tree_shake_button", new DefineFactory<Button_activity_tree_shake_button>());
		EventCenter.Self.RegisterEvent ("Button_decorate_tree", new DefineFactory<Button_decorate_tree>());
	}

    protected override void OpenInit()
    {
        base.OpenInit();
        mMask = GetCurUIGameObject("mask");
    }

	public override void Open(object param)
	{
		base.Open (param);
		NetManager.StartWaitShakeTreeQuery ();
		shakeTreeEffectObj = GameCommon.FindObject (mGameObjUI, "decorate_tree_effect").gameObject;
		shakeTreeEffectObj.SetActive (false);
        mMask.SetActive(false);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		
		switch(keyIndex)
		{
		case "SHAKE_TREE_QUERY":
			ShakeTreeQuery((string)objVal);
			break ;
		case "SHAKE_TREE_MESSAGE":
            mMask.SetActive(true);
			NetManager.RequstShakeTreeShake ();
			break;
		case "SHAKE_MONEY":
			ShakeMoney((string)objVal);
			break;
		}
	}

    private void RefreshNewMark() 
    {
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_SHAKE_TREE, false);
        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_TREE);
    }

	void ShakeMoney(string text)
	{
		SC_ShakeTree item = JCode.Decode<SC_ShakeTree>(text);
		if( null == item )
			return ;

		shakeTreeEffectObj.SetActive (true);
		GlobalModule.DoLater (() => shakeTreeEffectObj.SetActive (false), 1.0f);

        //added by xuke 刷新红点
        RefreshNewMark();
        //end

		iAllNum += 1;
		iTodayNum += 1;

		int iShakeMoney = item.goldNum;

		ItemDataBase itemGold= new ItemDataBase();
		itemGold.itemNum = item.goldNum;
		itemGold.tid = (int)ITEM_TYPE.GOLD;
		itemGold.itemId = -1;


		GameObject tipsObj = GameCommon.FindObject (mGameObjUI, "tips_label").gameObject;
		UILabel goldNumLabel = tipsObj.transform.Find ("num").GetComponent<UILabel>();
		UILabel iLeftTimeLabel = tipsObj.transform.Find ("left_shake_times").GetComponent<UILabel>();

		if(item.extraGoldNum <= 0)
		{
			tipsObj.SetActive (false);
		}else 
		{
			tipsObj.SetActive (true);
			goldNumLabel.text = item.extraGoldNum.ToString ();
			if(SHAKETIMES - iAllNum <= 0)
			{
				iLeftTimeLabel.text = "0";
			}else
			{
				iLeftTimeLabel.text = (SHAKETIMES - iAllNum).ToString ();
			}

		}

		GameObject countDownObj = GameCommon.FindObject (mGameObjUI, "shake_count_down").gameObject;
		GameObject shakeTimesObj = GameCommon.FindObject (mGameObjUI, "shake_label").gameObject;
		UILabel shakeTimeslabel = shakeTimesObj.transform.Find ("num").GetComponent<UILabel>();
		int iLimitTime = TableCommon.GetNumberFromCharacterLevelExp (RoleLogicData.Self.character.level , "MONEY_TREE_TIME") * 60;

		if(iTodayNum < 3)
		{
			countDownObj.SetActive (true);
            SetCountdownTime("shake_count_down", (Int64)(CommonParam.NowServerTime() + iLimitTime), new CallBack(this, "TimeIsOver", true));
		}else 
		{
			countDownObj.SetActive (false);
		}

		GameCommon.FindComponent<UIImageButton >(mGameObjUI, "activity_tree_shake_button").isEnabled = false;


		shakeTimeslabel.text = iTodayNum.ToString () + "/3次";
//		DataCenter.OpenWindow("AWARD_WINDOW", itemGold);	

		ItemDataBase[] itemDataArr = new ItemDataBase[1];
		ItemDataBase itemDatas = PackageManager.UpdateItem (itemGold);
		itemDataArr[0] = itemDatas;

		//ItemDataBase
//		DataCenter.OpenWindow ("GET_REWARDS_WINDOW", new ItemDataProvider(itemDatas));
        GlobalModule.DoLater(() => 
        {
			DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemDataArr);
            mMask.SetActive(false); 
        }, 1.0f);
	}

	void ShakeTreeQuery(string text)
	{
		SC_TreeOfGold item = JCode.Decode<SC_TreeOfGold>(text);
		if( null == item )
			return ;

		iAllNum = item.allNum;
		iTodayNum = item.dayNum;
		iTotalGold = item.extraGold;
		iLeftTime = item.leftTime;

		GameObject tipsObj = GameCommon.FindObject (mGameObjUI, "tips_label").gameObject;
		UILabel goldNumLabel = tipsObj.transform.Find ("num").GetComponent<UILabel>();
		UILabel iLeftTimeLabel = tipsObj.transform.Find ("left_shake_times").GetComponent<UILabel>();

		GameObject countDownObj = GameCommon.FindObject (mGameObjUI, "shake_count_down").gameObject;
		GameObject shakeTimesObj = GameCommon.FindObject (mGameObjUI, "shake_label").gameObject;
		UILabel shakeTimeslabel = shakeTimesObj.transform.Find ("num").GetComponent<UILabel>();

		if(iTotalGold <= 0)
		{
			tipsObj.SetActive (false);
		}else 
		{
			tipsObj.SetActive (true);
			goldNumLabel.text = iTotalGold.ToString ();
			iLeftTimeLabel.text = (SHAKETIMES - iAllNum).ToString ();
		}
		if(iLeftTime <= 0)
		{
			countDownObj.SetActive (false);
			GameCommon.FindComponent<UIImageButton >(mGameObjUI, "activity_tree_shake_button").isEnabled = true;
		}else 
		{
			countDownObj.SetActive (true);
			GameCommon.FindComponent<UIImageButton >(mGameObjUI, "activity_tree_shake_button").isEnabled = false;

			SetCountdownTime ("shake_count_down",(Int64)(CommonParam.NowServerTime() + iLeftTime ) ,new CallBack(this, "TimeIsOver", true));
		}
		if(iTodayNum >= 3 )
		{
			GameCommon.FindComponent<UIImageButton >(mGameObjUI, "activity_tree_shake_button").isEnabled = false;
		}
		if(iAllNum == 6)
		{
			GameCommon.FindComponent<UIImageButton >(mGameObjUI, "activity_tree_shake_button").isEnabled = false;
		}

		shakeTimeslabel.text = iTodayNum.ToString () + "/3次";
	}

	public  void TimeIsOver()
	{
		Open(null);
	}
}

public class Button_activity_tree_shake_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ACTIVITY_TREE_WINDOW", "SHAKE_TREE_MESSAGE", true);
		//GameObject .Find ("active_list_window_back_group").GetComponent <UIPanel > ().depth = 0;
		return true;
	}
}
public class Button_decorate_tree : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("ACTIVITY_TREE_UP_WINDOW");
		return true;
	}
}

//摇钱树已摇信息
public class ActivityTreeUpWindow : tWindow
{
	public const int SHAKETIMES = 6;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent ("Button_activity_tree_up_close_button", new DefineFactory<Button_activity_tree_up_close_button>());
		EventCenter.Self.RegisterEvent ("Button_activity_tree_up_ok_button", new DefineFactory<Button_activity_tree_up_ok_button>());
		EventCenter.Self.RegisterEvent ("Button_activity_tree_up_get_rewards_button", new DefineFactory<Button_activity_tree_up_get_rewards_button>());
	}
	
	public override void Open(object param)
	{
		base.Open (param);
		NetManager.ShakeTreeInfos();
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		
		switch(keyIndex)
		{
		case "SET_TREE_MESSAGE":
			UpdateUI ((string)objVal);
			break ;
		case "REQUST_MONEY_MESSAGE":
			NetManager.RequstShakeTreeRewards();
			break;
		case "GET_MONEY_MESSAGE":
			GetRewards((string)objVal);
			break ;
		}
	}

	void GetRewards(string text)
	{
		SC_TreeExtraGold item = JCode.Decode<SC_TreeExtraGold>(text);
		if( null == item )
			return ;

		ItemDataBase itemGold= new ItemDataBase();
		itemGold.itemNum = item.extraGold;
		itemGold.tid = (int)ITEM_TYPE.GOLD;
		itemGold.itemId = -1;
		PackageManager.AddItem (itemGold);
		DataCenter.CloseWindow ("ACTIVITY_TREE_UP_WINDOW");
		NetManager.StartWaitShakeTreeQuery ();
	}

	public void UpdateUI(string text)
	{
		SC_TreeOfGold item = JCode.Decode<SC_TreeOfGold>(text);
		if( null == item )
			return ;
		int goldNum = item.extraGold;
		int shakeLeftTimes =SHAKETIMES - item.allNum;
		GameObject obj = GameCommon.FindObject (mGameObjUI, "title_sprite").gameObject;
		UILabel label = obj.transform.Find ("Label").GetComponent<UILabel>();
		if(shakeLeftTimes <= 0)
		{
			shakeLeftTimes = 0;
		}
		label.text = "[e8d6b7]当前已收集到[99ff66]" + goldNum.ToString () + "[e8d6b7]银币啦，再[ea3030]连续[e8d6b7]摇" + shakeLeftTimes.ToString () + "次就可以获得奖励哦亲~！";

		GameObject goldObj = GameCommon.FindObject (mGameObjUI, "item_icon_btn").gameObject;
		UILabel goldObjLabel = goldObj.transform.Find ("label").GetComponent<UILabel>();
		goldObjLabel.text = "X" + goldNum.ToString ();
		AddButtonAction (goldObj, () => GameCommon.SetAccountItemDetailsWindow ((int)ITEM_TYPE.GOLD));

		if(item.allNum < SHAKETIMES)
		{
			GameCommon.FindObject (mGameObjUI, "activity_tree_up_get_rewards_button" ).SetActive (false);
			GameCommon.FindObject (mGameObjUI, "activity_tree_up_ok_button" ).SetActive (true);
		}else 
		{
			GameCommon.FindObject (mGameObjUI, "activity_tree_up_get_rewards_button" ).SetActive (true);
			GameCommon.FindObject (mGameObjUI, "activity_tree_up_ok_button" ).SetActive (false);
		}
	}

}

public class Button_activity_tree_up_close_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("ACTIVITY_TREE_UP_WINDOW");
		return true;
	}
}

public class Button_activity_tree_up_ok_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("ACTIVITY_TREE_UP_WINDOW");
		return true;
	}
}
public class Button_activity_tree_up_get_rewards_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ACTIVITY_TREE_UP_WINDOW", "REQUST_MONEY_MESSAGE", true);
		return true;
	}
}