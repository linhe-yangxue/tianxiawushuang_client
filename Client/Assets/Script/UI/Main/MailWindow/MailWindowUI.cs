using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logic;
using DataTable;

public class MailWindowUI : MonoBehaviour {

	MailWindow mMailWindow;
	void Start () 
	{
		mMailWindow = new MailWindow (gameObject) { mWinName = "MAIL_WINDOW" };
		DataCenter.Self.registerData ("MAIL_WINDOW", mMailWindow);

		EventCenter.Self.RegisterEvent ("Button_mail_back", new DefineFactory<Button_mail_back> ());
		EventCenter.Self.RegisterEvent ("Button_get_all_mail_button", new DefineFactory<Button_get_all_mail_button> ());
		EventCenter.Self.RegisterEvent ("Button_get_all_resources_mail_button", new DefineFactory<Button_get_all_resources_mail_button> ());
		EventCenter.Self.RegisterEvent ("Button_get_mail_button", new DefineFactory<Button_get_mail_button> ());
		EventCenter.Self.RegisterEvent ("Button_mail_message_background", new DefineFactory<Button_mail_message_background> ());
		EventCenter.Self.RegisterEvent ("Button_close_result_context_button", new DefineFactory<Button_close_result_context_button> ());
		EventCenter.Self.RegisterEvent ("Button_close_result_bag_full_button", new DefineFactory<Button_close_result_bag_full_button> ());
		EventCenter.Self.RegisterEvent ("Button_clean_bag_button", new DefineFactory<ButtonPackageBtn> ());
		EventCenter.Self.RegisterEvent ("Button_clean_pet_bag_button", new DefineFactory<Button_ChangeTeamBtn> ());

		DataCenter.OpenWindow ("MAIL_WINDOW", true);
	}

	void FixedUpdate()
	{
		mMailWindow.Update ();
	}

	void OnDestroy()
	{
		DataCenter.CloseWindow ("BACK_GROUP_MAIL_WINDOW");
		DataCenter.Remove("MAIL_WINDOW");
	}
}


public class MailWindow : tWindow
{
	UIScrollBar mScrollBar;

	int mWhichPage = 1;
	int mPageNum = 30;
	int mGridStartPage = 0;
    //by chenliang
    //begin

    private TweenPosition mTweenPosition;
    private EventDelegate mTweenDel;
    private GridsContainer mGridsContainer;

    //end
	
	public static List<MailData> mMailDataList = new List<MailData>();
	public MailWindow(GameObject obj)
	{
		mGameObjUI = obj;
	}  
	
	public override void Init() 
	{
		mScrollBar = GameCommon.FindObject (mGameObjUI, "scroll_bar").GetComponent<UIScrollBar>();
	}

	public override void Open(object param)
	{
		base.Open (param);
		DataCenter.OpenWindow ("BACK_GROUP_MAIL_WINDOW");

        setNoMailTipsIsVisible(false);
        //by chenliang
        //begin

// 		UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
// 		grid.MaxCount = 0;
//------------------
        if (mGridsContainer == null)
        {
            mGridsContainer = GameCommon.FindObject(mGameObjUI, "grid").GetComponent<GridsContainer>();
            mGridsContainer.FuncRefreshCell = __OnRefreshMailCell;
        }
        mGridsContainer.MaxCount = 0;
        //added by xuke
        ResetGridTweenFlag();
        //end
        //改为在动画播放完成之后，再请求数据
        if (mTweenPosition == null)
        {
            mTweenPosition = GameCommon.FindComponent<TweenPosition>(mGameObjUI, "npc");
            if (mTweenDel == null)
            {
                mTweenDel = new EventDelegate(__OnAnimComplete);
                mTweenPosition.onFinished.Add(mTweenDel);
            }
        }

        //end
		mWhichPage = 1;
		mGridStartPage = 0;

		ShowResultContext(null);
		ShowResultBagFullVisible (null);

		ShowNoMailTips ();

		/*
		tEvent evt = Net.StartEvent("CS_RequestMailList");
		evt.DoEvent();
		*/

        //by chenliang
        //begin

//		MailNetRequester.RequestMailList(RequestMailSuccess, RequestMailFail);
//--------------
        if(mTweenPosition != null)
        {
            mTweenPosition.ResetToBeginning();
            mTweenPosition.PlayForward();
        }

        //end

		// example email

		DataCenter.Remove ("MAIL_LIST_DATA");
		/*
		if (!respEvt.getData("MAIL_LIST", out maildata))
		{
			Log("Erro: No response maildata");
			DataCenter.SetData("MAIL_WINDOW", "GET_DATA_AND_UPDATA_MAIL_NUM", true);
			DataCenter.SetData("MAIL_WINDOW", "REFRESH", true);
			return;
		}
		*/

		/*
		//System.IO.FileStream fs = System.IO.File.Open(Application.dataPath + "/Config/mail.csv", System.IO.FileMode.Open);
		NiceTable mailTable = new NiceTable();
		mailTable = GameCommon.LoadTable("/Config/mail.csv", LOAD_MODE.ANIS);
		MailLogicData logicData = new MailLogicData();
		int mailNum = mailTable.GetAllRecord().Count;
		logicData.mMailList = new MailData[mailNum];
		DataCenter.RegisterData ("MAIL_LIST_DATA", logicData);
		foreach (KeyValuePair<int, DataRecord> r in mailTable.GetAllRecord())
		{ 
			DataRecord re = r.Value;
			MailData mailData = new MailData();
			mailData.mModelID = re.getData("RESOURCE_ID");
			mailData.mailId = re.getData("MAIL_ID");
			mailData.mTitleID = re.getData("MODEL_ID");
			mailData.mType = re.getData("RESOURCE_TYPE");
			mailData.mCount = re.getData("RESOURCE_COUNT");
			mailData.mTime = re.getData("EXPIRE_TIME");
			mailData.mRoleEquipElement = re.getData ("RESOURCE_ELEMENT");
			mailData.mAddTitle = re.getData ("TITLE");
			logicData.mMailList[count] = mailData;
			count ++;
		}

		DataCenter.SetData ("MAIL_WINDOW", "GET_DATA_AND_UPDATA_MAIL_NUM", true);
		DataCenter.SetData ("MAIL_WINDOW", "REFRESH", true);
		*/
	}

	public void ShowNoMailTips()
	{
		SetText ("Label_nomail_tips", DataCenter.mStringList.GetData(3100, "STRING_CN"));
	}

	public void setNoMailTipsIsVisible(bool visible)
	{
		SetVisible ("Label_nomail_tips", visible);
	}

    //by chenliang
    //begin

    private void __OnAnimComplete()
    {
        MailNetRequester.RequestMailList(RequestMailSuccess, RequestMailFail);
    }

    //end

	void RequestMailSuccess(string result) {
		MailLogicData logicData = new MailLogicData();
		logicData = JCode.Decode<MailLogicData>(result);
		DataCenter.RegisterData ("MAIL_LIST_DATA", logicData);

		DataCenter.SetData ("MAIL_WINDOW", "GET_DATA_AND_UPDATA_MAIL_NUM", true);
		DataCenter.SetData ("MAIL_WINDOW", "REFRESH", true);
	}

	void RequestMailFail(string result) {
		DEBUG.Log("mail data fail:" + result);
	}

    //by chenliang
    //begin

//	public override bool Refresh(object param)
//--------------------
    public bool RefreshOld(object param)

    //end
	{ 
		base.Refresh (param);
		SetMailCount ();
		int iMaxCount = mMailDataList.Count;

		GameObject npcObj = GameCommon.FindObject (mGameObjUI, "npc");
		float fDelay = (float)iMaxCount/(mPageNum * 2);
		if(fDelay > 0.5f) fDelay = 0.5f;
		npcObj.GetComponent<TweenPosition>().delay = fDelay;
		npcObj.GetComponent<TweenPosition>().enabled = true;

		SetText ("get_mail_num_label", iMaxCount.ToString () + "/" + DataCenter.mGlobalConfig.GetData ("MAX_MAIL_COUNT", "VALUE"));
//		GetLable("get_mail_num_label").gameObject.SetActive(false);
		if(iMaxCount >= mPageNum * mWhichPage) iMaxCount = mPageNum * mWhichPage;
        //by chenliang
        //begin

        //将所有邮件数据保存在一键领取按钮里
        NiceData tmpBtnGetAllData = GameCommon.GetButtonData(mGameObjUI, "get_all_mail_button");
        if (tmpBtnGetAllData != null)
            tmpBtnGetAllData.set("ALL_MAIL_DATA", mMailDataList);

        //end

		UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
		grid.MaxCount = iMaxCount;
		for(int i = 0; i < iMaxCount; i++)
		{
			GameObject subCell = grid.controlList[i];

			int currentMailsCount = mGridStartPage * mPageNum + mWhichPage * mPageNum;
			if(currentMailsCount > mMailDataList.Count) currentMailsCount = mMailDataList.Count;
			if(currentMailsCount < mWhichPage * mPageNum) currentMailsCount = mWhichPage * mPageNum;
			
			int iListCount = mMailDataList.Count;
			int iCurrentMailDataIndex = currentMailsCount - mWhichPage * mPageNum + i;
//			MailData mailData = mMailDataList[(iListCount - 1) - iCurrentMailDataIndex];
			MailData mailData = mMailDataList[iCurrentMailDataIndex];

			NiceData getMailButtonData = GameCommon.GetButtonData (subCell, "get_mail_button");
			if(mMailDataList != null) getMailButtonData.set ("MAIL_DATA", mailData);

			DataRecord mailTitleData = DataCenter.mMailBorad.GetRecord(mailData.mTitleID);
			string strMailDes = mailData.mailContent;
			//strMailTitle = string.Format (strMailTitle, "'" + mailData.mAddTitle + "'");
			GameCommon.SetUIText (subCell, "title_info_label", mailData.mailTitle);
			GameCommon.SetUIText(subCell, "info_label", strMailDes);

//            GameCommon.SetUIText(subCell, "info_label", "x" + mailData.items[0].itemNum);

			UIGridContainer itemIocnGroupGrid = subCell.transform.Find ("item_info_group/reward_scrollview/item_icon_grid").GetComponent<UIGridContainer>();
			itemIocnGroupGrid.MaxCount = mailData.items.Length;

			for(int j = 0; j < itemIocnGroupGrid.MaxCount; j++)
			{
				GameObject itemIconObj = itemIocnGroupGrid.controlList[j];
				UILabel itemNumLabel = itemIconObj.transform.Find ("item_icon/item_num").GetComponent<UILabel>();
				itemNumLabel.text = "x" + mailData.items[j].itemNum.ToString();
				GameCommon.SetItemIcon(itemIconObj, "item_icon", mailData.items[j].tid);
			}
//			itemIocnGroupGrid.MaxCount = 3;
//
//			if(mailData.items.Length <= 3)
//			{
//				for(int j = 0; j < mailData.items.Length; j++)
//				{
//					GameObject itemIconObj = itemIocnGroupGrid.controlList[j];
//					UILabel itemNumLabel = itemIconObj.transform.Find ("item_icon/item_num").GetComponent<UILabel>();
//					itemNumLabel.text = "x" + mailData.items[j].itemNum.ToString();
//					GameCommon.SetItemIcon(itemIconObj, "item_icon", mailData.items[j].tid);
//				}
//			}else if(mailData.items.Length > 3)
//			{
//
//				for(int j = 0; j < itemIocnGroupGrid.MaxCount; j++)
//				{
//					GameObject itemIconObj = itemIocnGroupGrid.controlList[j];
//					UILabel itemNumLabel = itemIconObj.transform.Find ("item_icon/item_num").GetComponent<UILabel>();
//					itemNumLabel.text = "x" + mailData.items[j].itemNum.ToString();
//					GameCommon.SetItemIcon(itemIconObj, "item_icon", mailData.items[j].tid);
//				}
//			}
			
			switch (PackageManager.GetItemTypeByTableID(mailData.tid)) {

				case ITEM_TYPE.PET:
				GameCommon.SetPetIconWithElementAndStar (subCell, "item_icon", "element", "star_level_label", int.Parse(mailData.mailContent));
				break;
			}
            //GameCommon.SetItemIcon(subCell, new ItemData() { mType = mailData.mType, mID = mailData.mModelID, mNumber = mailData.mCount, mEquipElement = mailData.mRoleEquipElement });
            /*
            GameCommon.SetUIVisiable(subCell, false, "element", "role_equip_element", "fragment_sprite");
            GameCommon.SetUIVisiable(subCell, true, "star_level_label");

            switch ((ITEM_TYPE)mailData.mType) 
            {
            case ITEM_TYPE.PET:
//				GameCommon.SetUIVisiable (subCell, "element", true);
                int iPetID = mailData.mModelID;
                GameCommon.SetPetIconWithElementAndStar (subCell, "item_icon", "element", "star_level_label", iPetID);
                break;
            case ITEM_TYPE.FRAGMENT:
                int fragmentID = mailData.mModelID;
                int fragmentPetID = TableCommon.GetNumberFromFragment(fragmentID,"ITEM_ID");
                GameCommon.SetPetIconWithElementAndStar (subCell, "item_icon", "element", "star_level_label", fragmentPetID);

                GameCommon.SetUIVisiable (subCell, "fragment_sprite", true);
                int elementIndex = TableCommon.GetNumberFromActiveCongfig(fragmentPetID, "ELEMENT_INDEX");
                GameCommon.SetElementFragmentIcon (subCell, "fragment_sprite", elementIndex);
                break;
            case ITEM_TYPE.ROLE_EQUIP:
                GameCommon.SetUIVisiable (subCell, "role_equip_element", true);
                GameCommon.SetUIVisiable (subCell, "element", true);
                GameCommon.SetEquipElementBgIcons(subCell, "element", "role_equip_element", mailData.mModelID, mailData.mRoleEquipElement);
//				GameCommon.SetEquipElementBgIcon(subCell, "role_equip_element", mailData.mRoleEquipElement);
                int iStarLevel = TableCommon.GetNumberFromRoleEquipConfig (mailData.mModelID, "STAR_LEVEL");
                GameCommon.SetUIText (subCell, "star_level_label", iStarLevel.ToString ());
                GameCommon.SetRoleEquipIcon (subCell, mailData.mModelID, "item_icon");
                break;
            default:
                GameCommon.SetUIVisiable (subCell, "star_level_label", false);
                UISprite itemSprite = GameCommon.FindObject (subCell, "item_icon").GetComponent <UISprite>();
                GameCommon.SetItemIcon (itemSprite, mailData.mType, mailData.mModelID);
                break;	
            }*/

            MailLogicData mailLogicData = DataCenter.GetData("MAIL_LIST_DATA") as MailLogicData;
            if (null == mailLogicData)
                return false;

            Int64 lastTime = (mailLogicData.mailSaveTime);
			if(lastTime > 0)
			{
				TimeSpan span = new TimeSpan ((long)lastTime * 10000000);
				int iDay = span.Days;
				int iHour = span.Hours;
				if(iDay != 0)
                {
                    if (iHour != 0) GameCommon.SetUIText(subCell, "off_time_label", iDay.ToString() + "天" + iHour.ToString() + "时");
					else GameCommon.SetUIText (subCell, "off_time_label", iDay.ToString () + "天");
				}
				else
				{
					MailBeyondLimitInfo mailBeyondLimitInfo = new MailBeyondLimitInfo(subCell, (int)mailData.mailId);
					SetCountdownTime (subCell, "off_time_label", mailData.mTime, new CallBack(this, "MailBeyondLimit", mailBeyondLimitInfo));
				}
			}
			else GameCommon.SetUIText (subCell, "off_time_label", "beyond the limit");
		}

        // 刷新邮件状态
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_MAIL, mMailDataList.Count > 0);

		return true;
	}
    //by chenliang
    //begin

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        SetMailCount();
        int iMaxCount = mMailDataList.Count;

        GameObject npcObj = GameCommon.FindObject(mGameObjUI, "npc");
        float fDelay = (float)iMaxCount / (mPageNum * 2);
        if (fDelay > 0.5f) fDelay = 0.5f;
        npcObj.GetComponent<TweenPosition>().delay = fDelay;
        npcObj.GetComponent<TweenPosition>().enabled = true;

        SetText("get_mail_num_label", iMaxCount.ToString() + "/" + DataCenter.mGlobalConfig.GetData("MAX_MAIL_COUNT", "VALUE"));

		//show no mail tips
		setNoMailTipsIsVisible (iMaxCount > 0 ? false : true);

        //将所有邮件数据保存在一键领取按钮里
        NiceData tmpBtnGetAllData = GameCommon.GetButtonData(mGameObjUI, "get_all_mail_button");
        if (tmpBtnGetAllData != null)
            tmpBtnGetAllData.set("ALL_MAIL_DATA", mMailDataList);

        mGridsContainer.MaxCount = iMaxCount;
        mGridsContainer.RefreshAllCell();

        // 刷新邮件状态
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_MAIL, mMailDataList.Count > 0);

        return true;
    }
    private void __OnRefreshMailCell(int index, GameObject goCell)
    {
        GameObject subCell = goCell;

        int iListCount = mMailDataList.Count;
        MailData mailData = mMailDataList[index];

        NiceData getMailButtonData = GameCommon.GetButtonData(subCell, "get_mail_button");
        if (mMailDataList != null) getMailButtonData.set("MAIL_DATA", mailData);

        DataRecord mailTitleData = DataCenter.mMailBorad.GetRecord(mailData.mTitleID);
        string strMailDes = mailData.mailContent;
        GameCommon.SetUIText(subCell, "title_info_label", mailData.mailTitle);
        GameCommon.SetUIText(subCell, "info_label", strMailDes);

        UIGridContainer itemIocnGroupGrid = subCell.transform.Find("item_info_group/reward_scrollview/item_icon_grid").GetComponent<UIGridContainer>();
        //改为最多显示3个
        itemIocnGroupGrid.MaxCount = Mathf.Min(mailData.items.Length, 3);

        for (int j = 0; j < itemIocnGroupGrid.MaxCount; j++)
        {
            GameObject itemIconObj = itemIocnGroupGrid.controlList[j];
            UILabel itemNumLabel = itemIconObj.transform.Find("item_icon/item_num").GetComponent<UILabel>();
            itemNumLabel.text = "x" + mailData.items[j].itemNum.ToString();
            GameCommon.SetOnlyItemIcon(itemIconObj, "item_icon", mailData.items[j].tid);
			int iTid = mailData.items[j].tid;
			AddButtonAction (GameCommon.FindObject(itemIconObj, "item_icon"), () => GameCommon.SetAccountItemDetailsWindow (iTid));
        }

        switch (PackageManager.GetItemTypeByTableID(mailData.tid))
        {

            case ITEM_TYPE.PET:
                GameCommon.SetPetIconWithElementAndStar(subCell, "item_icon", "element", "star_level_label", int.Parse(mailData.mailContent));
                break;
        }

        MailLogicData mailLogicData = DataCenter.GetData("MAIL_LIST_DATA") as MailLogicData;
        if (null == mailLogicData)
            return;

		Int64 startTime = mailData.mailTime;
		Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime ());
		Int64 closeTime = startTime + 24*60*60*2;
		Int64 lastTime = closeTime - nowSeconds;
		//if(closeTime -)
        if (lastTime > 0)
        {
            TimeSpan span = new TimeSpan((long)lastTime * 10000000);
            int iDay = span.Days;
            int iHour = span.Hours;
			int iMinute =span.Minutes;
			if (iDay != 0||iHour != 0||iMinute != 0)
			{
				if (iHour >= 0 && iDay ==1 && iMinute > 0) 
					GameCommon.SetUIText(subCell, "off_time_label", (iDay+1).ToString() + "天" );
                else 
					GameCommon.SetUIText(subCell, "off_time_label", (iDay+1).ToString() + "天");
            }
            else
            {
                MailBeyondLimitInfo mailBeyondLimitInfo = new MailBeyondLimitInfo(subCell, (int)mailData.mailId);
                SetCountdownTime(subCell, "off_time_label", mailData.mTime, new CallBack(this, "MailBeyondLimit", mailBeyondLimitInfo));
            }
        }
        else GameCommon.SetUIText(subCell, "off_time_label", "beyond the limit");
    }

    //end

	public void MailBeyondLimit(object param)
	{
		MailBeyondLimitInfo mailBeyondLimitInfo = param as MailBeyondLimitInfo;
		for(int i=0; i<mMailDataList.Count; i++)
		{
			if(mMailDataList[i].mailId == mailBeyondLimitInfo.mMailID) mMailDataList.RemoveAt (i);
		}
		mailBeyondLimitInfo.mParentObj.SetActive (false);

		UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
		grid.Reposition ();

		SetText ("get_mail_num_label", (--RoleLogicData.Self.mMailNum).ToString () + "/" + DataCenter.mGlobalConfig.GetData ("MAX_MAIL_COUNT", "VALUE"));
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch(keyIndex)
		{
		case"GET_DATA_AND_UPDATA_MAIL_NUM":
			MailLogicData mailLogicData = DataCenter.GetData ("MAIL_LIST_DATA") as MailLogicData;
			if(mailLogicData != null)
			{
				mMailDataList = mailLogicData.mails.ToList();

//				//Rule out beyond the limit of mail
//				for(int i = 0; i < mMailDataList.Count; i++)
//				{
//					Int64 lastTime = mMailDataList[i].mTime - CommonParam.NowServerTime ();
//					if(lastTime <= 0) 
//					{
//						mMailDataList.RemoveAt (i);
//						i--;
//					}
//				}
			
				RoleLogicData.Self.mMailNum = mMailDataList.Count;
			}
			else RoleLogicData.Self.mMailNum = 0;
			break;
		case "GET_MAIL_SUCCEND":
			//DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_SUCCESS, true);
			object obj;
			getData ("CURRENT_MAIL_DATA", out obj);
			NiceData currentMailData = obj as NiceData;

			GameObject buttonParent = currentMailData.getObject ("PARENT") as GameObject;
			buttonParent.SetActive (false);

			UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
			grid.Reposition();

			SetText ("get_mail_num_label", (--RoleLogicData.Self.mMailNum).ToString () + "/" + DataCenter.mGlobalConfig.GetData ("MAX_MAIL_COUNT", "VALUE"));

			MailData mailData = (MailData)currentMailData.getObject ("MAIL_DATA");
            ItemData item = new ItemData() { mType = mailData.mType, mID = mailData.mModelID, mNumber = mailData.mCount, mEquipElement = mailData.mRoleEquipElement };
            DataCenter.OpenWindow("GET_REWARDS_WINDOW", new ItemDataProvider(item));
			int iItemType = mailData.mType;
			int iItemCount = mailData.mCount;
			AddItem(iItemType, iItemCount);

			int iMailID = (int)mailData.mailId;
			for(int i=0; i<mMailDataList.Count; i++)
			{
				if(mMailDataList[i].mailId == iMailID) mMailDataList.RemoveAt (i);
			}
			SetMailCount ();
//			DataCenter.SetData ("INFO_GROUP_WINDOW", "UPDATE_ROLE_SELECT_SCENE", true);
			GameCommon.SetWindowData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_MAIL_MARK", true);
			break;
		case "GET_MAILS_SUCCESS":
			DataRecord dataRecord = (DataRecord)objVal;
			RoleLogicData mailsRoleLogicData = RoleLogicData.Self;
			mailsRoleLogicData.mMailNum --;

			int mailID = dataRecord.getData ("MAIL_ID");
			int iCount = 0;
			int iType = 0;

			for(int i=0; i<mMailDataList.Count; i++)
			{
				if(mMailDataList[i].mailId == mailID)
				{
					iType = mMailDataList[i].mType;
					iCount = mMailDataList[i].mCount;
					mMailDataList.RemoveAt (i);
				}
			}

			SetMailCount();
			AddItem(iType, iCount);

//			DataCenter.SetData ("INFO_GROUP_WINDOW", "UPDATE_ROLE_SELECT_SCENE", true);
			GameCommon.SetWindowData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_MAIL_MARK", true);
			break;
		case "SHOW_RESULT_CONTEXT":
			ShowResultContext (objVal);
			break;
		case "RESULT_BAG_FULL_CONTEXT":
			ShowResultBagFullVisible(objVal);
			break;
		}
	}

	public void Update()
	{
        //by chenliang
        //begin

        //退出，不计算分页
        return;

        //end
		if(mScrollBar.value == 1.0f  && mMailDataList.Count > (mWhichPage * mPageNum + mGridStartPage * mPageNum))
		{
			mWhichPage++;

			if(mWhichPage > 3)
			{
				mWhichPage = 3;
				mGridStartPage ++;	
			}

			int iMailCount = mMailDataList.Count;
			int iCurrent = mWhichPage * mPageNum + mGridStartPage * mPageNum;
			if(iMailCount > (iCurrent - mPageNum))
			{
				Refresh (null);
				if(mGridStartPage == 0) 
					mScrollBar.value = 1;
				else if(iMailCount < iCurrent)
					mScrollBar.value = 1 - (float)(iMailCount -(iCurrent - mPageNum)) / (mWhichPage * mPageNum);
				else 
					mScrollBar.value = 2.0f/3;
			}
		}
		if(mScrollBar.value == 0f && mGridStartPage > 0)
		{
			mGridStartPage--;
			Refresh (null);

			if(mMailDataList.Count < (mWhichPage * mPageNum + (mGridStartPage + 1) * mPageNum))
				mScrollBar.value = (float)(mMailDataList.Count % mPageNum) / (mWhichPage * mPageNum);
			else
				mScrollBar.value = 1.0f/3;
		}
	}

	void AddItem(int iItemType, int iItemCount)
	{
		GameCommon.RoleChangeNumericalAboutRole (iItemType, iItemCount);
	}
	
	void ShowResultContext(object resultObj)
	{
		List<DataRecord> resultList = resultObj as List<DataRecord>;

		if(resultList != null)
		{
			int count = resultList.Count;
			SetResultContextVisible(true);
			UIGridContainer grid = GetSub ("result_grid").GetComponent<UIGridContainer>();
            grid.MaxCount = 0;
			grid.MaxCount = count;
			for(int i = 0; i < count; i++)
			{
				GameObject subCell = grid.controlList[i];
				DataRecord re = resultList[i];
				int resourceID = re["RESOURCE_ID"];
				int resourceType = re["RESOURCE_TYPE"];
				int resourceCount = re["RESOURCE_COUNT"];
				int roleEquipElement = re["RESOURCE_ELEMENT"];

				GameCommon.SetUIText (subCell, "count_label", "x" + resourceCount);

				GameCommon.SetUIVisiable(subCell, false, "element", "role_equip_element", "fragment_sprite");
				GameCommon.SetUIVisiable(subCell, false, "star_level_label");

				switch ((ITEM_TYPE)resourceType) 
				{
				case ITEM_TYPE.PET:
					GameCommon.SetPetIconWithElementAndStar (subCell, "item_icon", "element", "star_level_label", resourceID);
					break;
				case ITEM_TYPE.PET_FRAGMENT:
					int iFragment = TableCommon.GetNumberFromFragment(resourceID,"ITEM_ID");
					GameCommon.SetPetIconWithElementAndStar (subCell, "item_icon", "element", "star_level_label", iFragment);
					
					GameCommon.SetUIVisiable (subCell, "fragment_sprite", true);
					int elementIndex = TableCommon.GetNumberFromActiveCongfig(resourceID, "ELEMENT_INDEX");
					GameCommon.SetElementFragmentIcon (subCell, "fragment_sprite", elementIndex);
					break;
				case ITEM_TYPE.EQUIP:
					GameCommon.SetUIVisiable (subCell, "role_equip_element", true);
					GameCommon.SetUIText (subCell, "count_label", "x1");
					int iStarLevel = TableCommon.GetNumberFromRoleEquipConfig (resourceID, "STAR_LEVEL");
					//GameCommon.SetUIText (subCell, "star_level_label", iStarLevel.ToString ());
                    GameCommon.SetStarLevelLabel(subCell, iStarLevel, "star_level_label");
					GameCommon.SetEquipIcon (subCell, resourceID, "item_icon");

					GameCommon.SetUIVisiable (subCell, "element", true);
					GameCommon.SetEquipElementBgIcons(subCell, "element", "role_equip_element", resourceID, roleEquipElement);
					break;
				default:
					GameCommon.SetUIVisiable (subCell, "star_level_label", false);
					UISprite itemSprite = GameCommon.FindObject (subCell, "item_icon").GetComponent <UISprite>();
					GameCommon.SetItemIcon (itemSprite, resourceType, resourceID);
					break;	
				}
			}
		}
		else
		{
			UIGridContainer grid = GetSub ("result_grid").GetComponent<UIGridContainer>();
			grid.MaxCount = 0;
			SetResultContextVisible(false);
		}
	}

	void ShowResultBagFullVisible(object obj)
	{
		if(obj != null)
		{
			SetResultBagFullVisible(true);
			string strWhickBag = obj.ToString ();
			if(strWhickBag == "PET_BAG")
			{
				SetPetBagFullLabelVisible(true);
				GameCommon.GetButtonData (GetSub ("clean_pet_bag_button")).set ("ACTION", "MAIL_ACTION");
			}
			else if(strWhickBag == "RESOURCE_BAG")
			{
				SetPetBagFullLabelVisible(false);
				GameCommon.GetButtonData (GetSub ("clean_bag_button")).set ("ACTION", "MAIL_ACTION");
			}
		}
		else 
		{
			SetResultBagFullVisible(false);
		}
	}

	void SetResultContextVisible(bool bVisible)
	{
		SetVisible ("get_mail_result_context", bVisible);
	}

	void SetResultBagFullVisible(bool bVisible)
	{
		SetVisible ("get_mail_result_bag_full", bVisible);
	}

	void SetPetBagFullLabelVisible(bool bVisible)
	{
		SetVisible ("other_bag_full_label", !bVisible);
		SetVisible ("pet_bag_full_label", bVisible);
	}

	void SetMailCount()
	{
		set ("MAIL_COUNT", mMailDataList.Count);
	}
}

public class MailBeyondLimitInfo
{
	public GameObject mParentObj;
	public int mMailID;
	public MailBeyondLimitInfo(GameObject obj, int iMailID)
	{
		mParentObj = obj;
		mMailID = iMailID;
	}
}

public class Button_mail_back : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
//		MainUIScript.Self.GoBack ();
		return true;
	}
}

public class Button_get_all_mail_button : CEvent
{
	public override bool _DoEvent()
	{
		if(GameCommon.bIsLogicDataExist ("MAIL_WINDOW"))
		{
			int iMailCount = DataCenter.GetData ("MAIL_WINDOW").get ("MAIL_COUNT");
			if(iMailCount != 0)
			{
                //by chenliang
                //begin

                //检查邮件里物品对应背包是否已满
                List<MailData> tmpAllMailData = getObject("ALL_MAIL_DATA") as List<MailData>;
                if (tmpAllMailData != null)
                {
                    for (int i = 0, count = tmpAllMailData.Count; i < count; i++)
                    {
                        MailData tmpMailData = tmpAllMailData[i];
                        List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(tmpMailData.items);
                        if (!CheckPackage.Instance.CanGetEmail(tmpPackageTypes))
                            return true;
                    }
                }

                //end
				MailNetRequester.RequestMailAllItem(GetMailAllItemSuccess, GetMailAllItemFail);
				/*
				tEvent evt = Net.StartEvent("CS_ReadAllMail");
				evt.set ("IS_RESOURCES", false);
				evt.DoEvent();
				*/
			}
			else DataCenter.ErrorTipsLabelMessage (STRING_INDEX.ERROR_MAIL_NO_MAIL);
		}
		return true;
	}

	void GetMailAllItemSuccess(string text){
		SC_RequestMailAllItem items = JCode.Decode<SC_RequestMailAllItem>(text);

		//AllMailDataStruct[] data = items.items;
		ItemDataBase [] basei = items.items;
		foreach(ItemDataBase idb in basei) {
			if(idb.itemId == -1) {
				PackageManager.AddItem(idb);
			} else {
				PackageManager.UpdateItem(idb);
			}
		}
		/*
		foreach(AllMailDataStruct amd in data) {
			PackageManager.up(amd.tid, amd.itemId, amd.itemNum);
		}
		*/

		MailWindow.mMailDataList.Clear();
		DataCenter.SetData ("MAIL_WINDOW", "REFRESH", true);
		DataCenter.OnlyTipsLabelMessage (STRING_INDEX.ERROR_MAIL_SUCCESS);

		string strMaxMail = DataCenter.mGlobalConfig.GetData ("MAX_MAIL_COUNT", "VALUE");
		int maxMailNum = Convert.ToInt32(strMaxMail);
		if(RoleLogicData.Self.mMailNum > maxMailNum - 1)
		{
			MailNetRequester.RequestMailList(RequestMailSuccess, RequestMailFail);
		}
	}
	void RequestMailSuccess(string result) {
		MailLogicData logicData = new MailLogicData();
		logicData = JCode.Decode<MailLogicData>(result);
		DataCenter.RegisterData ("MAIL_LIST_DATA", logicData);
		
		DataCenter.SetData ("MAIL_WINDOW", "GET_DATA_AND_UPDATA_MAIL_NUM", true);
		DataCenter.SetData ("MAIL_WINDOW", "REFRESH", true);
	}
	
	void RequestMailFail(string result) {
		DEBUG.Log("mail data fail:" + result);
	}
	void GetMailAllItemFail(string text) {
		DEBUG.Log("GetMailAllItemErr:" + text);
        MailWindow.mMailDataList.Clear();
        DataCenter.SetData("MAIL_WINDOW", "REFRESH", true);
	}
}

public class Button_get_all_resources_mail_button : CEvent
{
	public override bool _DoEvent()
	{
		if(GameCommon.bIsLogicDataExist ("MAIL_WINDOW"))
		{
			int iMailCount = DataCenter.GetData ("MAIL_WINDOW").get ("MAIL_COUNT");
			if(iMailCount != 0)
			{
				tEvent evt = Net.StartEvent("CS_ReadAllMail");
				evt.set ("IS_RESOURCES", true);
				evt.DoEvent();
			}
			else DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_NO_MAIL, true);
		}
		return true;
	}
}

public class Button_get_mail_button : CEvent
{
	int iMailID;
	public override bool _DoEvent()
	{
		GameObject obj = getObject ("BUTTON") as GameObject;

		NiceData buttonData = obj.GetComponent<UIButtonEvent>().mData as NiceData;
		MailData mailData = (MailData)getObject ("MAIL_DATA");
		if(mailData != null) iMailID = mailData.mailId;
        //by chenliang
        //begin

        //检查邮件里物品对应背包是否已满
        if (mailData != null)
        {
            List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(mailData.items);
            if (!CheckPackage.Instance.CanGetEmail(tmpPackageTypes))
                return true;
        }

        //end

		GameObject mailParent = obj.transform.parent.gameObject;
		set ("PARENT", mailParent);
		DataCenter.SetData ("MAIL_WINDOW", "CURRENT_MAIL_DATA", buttonData);

		MailNetRequester.RequestMailItem(iMailID, RequestGetMailItemSuccess, RequestGetMailItemFail);

		/*
		tEvent evt = Net.StartEvent("CS_ReadMail");
		evt.set ("MAIL_ID", iMailID);
		evt.DoEvent();
		*/
		return true;
	}

    void RequestGetMailItemSuccess(string text) {
		SC_RequestMailItem item = JCode.Decode<SC_RequestMailItem>(text);

		PackageManager.UpdateItem (item.items);

//		if(item.itemId == -1) {
//			PackageManager.AddItem(item.tid, item.itemId, item.itemNum);
//		} else {
//			PackageManager.UpdateItem(item.tid, item.itemId, item.itemNum);
//		}
//		DEBUG.Log("物品领取成功，tid:" + item.tid);
        List<MailData> ls = MailWindow.mMailDataList;

        for (int i = 0; i < ls.Count; i++)
        {
            if (ls[i].mailId == iMailID)
            {
                ls.RemoveAt(i);
                DataCenter.SetData("MAIL_WINDOW", "REFRESH", true);
				DataCenter.OnlyTipsLabelMessage (STRING_INDEX.ERROR_MAIL_SUCCESS);
//                DEBUG.Log("删除条目成功，tid:" + item.tid);
            }
        }
		string strMaxMail = DataCenter.mGlobalConfig.GetData ("MAX_MAIL_COUNT", "VALUE");
		int maxMailNum = Convert.ToInt32(strMaxMail);
		if(RoleLogicData.Self.mMailNum > maxMailNum - 1)
		{
			MailNetRequester.RequestMailList(RequestMailSuccess, RequestMailFail);
		}

	}
	void RequestMailSuccess(string result) {
		MailLogicData logicData = new MailLogicData();
		logicData = JCode.Decode<MailLogicData>(result);
		DataCenter.RegisterData ("MAIL_LIST_DATA", logicData);
		
		DataCenter.SetData ("MAIL_WINDOW", "GET_DATA_AND_UPDATA_MAIL_NUM", true);
		DataCenter.SetData ("MAIL_WINDOW", "REFRESH", true);
	}
	
	void RequestMailFail(string result) {
		DEBUG.Log("mail data fail:" + result);
	}

	void RequestGetMailItemFail(string text) {
		DEBUG.Log("物品领取失败，err:" + text);

        List<MailData> ls = MailWindow.mMailDataList;

        for (int i = 0; i < ls.Count; i++)
        {
            if (ls[i].mailId == iMailID)
            {
                ls.RemoveAt(i);
                DataCenter.SetData("MAIL_WINDOW", "REFRESH", true);
            }
        }
	}
}

public class Button_mail_message_background : CEvent
{
	public override bool _DoEvent()
	{
		GameObject obj = getObject ("BUTTON") as GameObject;
		obj.SetActive (false);
		return true;
	}
}

public class Button_close_result_context_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("MAIL_WINDOW", "SHOW_RESULT_CONTEXT", null);
		return true;
	}
}

class Button_close_result_bag_full_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("MAIL_WINDOW", "RESULT_BAG_FULL_CONTEXT", null);
		return true;
	}
}


