using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

public class BattlePlayerInfoWindow : tWindow
{
    private static readonly float PLACE_DURATION = 0.3f;
    private static readonly Vector3 PLACE_OFFSET = new Vector3(0, -80, 0);

    public RoleData mRoleData;
    private PetData[] mPetDatas;

    private UISlider mHpSlider;
    private UISlider mMpSlider;
    private UISlider[] mPetSliders;
    private UISlider[] mPetLifeTimes;
    //private UISlider mFriendSlider;

    public UILabel mHpMaxLabel;
	public UILabel mHpCurLabel;
	public UILabel mMpMaxLabel;
	public UILabel mMpCurLabel;
	public UILabel mLevelLabel;

    private BuffGroup mCharBuffs;
    private BuffGroup[] mPetBuffs;
    //private BuffGroup mFriendBuffs;

	//private UISlider mFriendHelpCoundownSlider;
    private List<GameObject> mActiveInfos = new List<GameObject>();

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public void SetDaoJiShi(long limitTime)
    {
        SetCountdownTime(GetSub("boss_refresh_time_title_label"), "boss_refresh_time_label", limitTime, new CallBack(this, "BattleRefresh", true));
    }

    public void BattleRefresh(object obj)
    {
        //时间到
        DEBUG.Log("time is over!");
    }

    public override bool Refresh(object param)
    {
        InitInfo();

        return true;
    }

    public override void Close()
    {
        Release();
        base.Close();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

		if (!IsOpen())
			return;

        switch (keyIndex)
        {
            case "HP":
                RefreshHpBar();
                break;
            case "MP":
                RefreshMpBar();
                break;
            case "LEVEL":
                RefreshLevel();
                break;
            case "PET_HP":
                RefreshPetHp(objVal);
                break;
            case "CHAR_DEAD":
                OnCharDead();               
                break;
            case "PET_DEAD":                
                OnPetDead((int)objVal);
                break;
            case "PET_ALIVE":
                OnPetAlive((int)objVal);
                break;
            //case "FRIEND_DEAD":
            //    OnFriendDead();
            //    break;
            case "START_CHAR_BUFF":
                mCharBuffs.StartBuff((int)objVal);
                break;
            case "START_PET_BUFF_1":
                mPetBuffs[0].StartBuff((int)objVal);
                break;
            case "START_PET_BUFF_2":
                mPetBuffs[1].StartBuff((int)objVal);
                break;
            case "START_PET_BUFF_3":
                mPetBuffs[2].StartBuff((int)objVal);
                break;
            case "START_PET_BUFF_4":
                //mFriendBuffs.StartBuff((int)objVal);
                mPetBuffs[3].StartBuff((int)objVal);
                break;

            case "REMOVE_CHAR_BUFF":
                mCharBuffs.RemoveBuff((int)objVal);
                break;
            case "REMOVE_PET_BUFF_1":
                mPetBuffs[0].RemoveBuff((int)objVal);
                break;
            case "REMOVE_PET_BUFF_2":
                mPetBuffs[1].RemoveBuff((int)objVal);
                break;
            case "REMOVE_PET_BUFF_3":
                mPetBuffs[2].RemoveBuff((int)objVal);
                break;
            case "REMOVE_PET_BUFF_4":
                //mFriendBuffs.RemoveBuff((int)objVal);
                mPetBuffs[3].RemoveBuff((int)objVal);
                break;

            case "SET_PET_LIFETIME_1":
                mPetLifeTimes[0].value = (float)objVal;
                break;
            case "SET_PET_LIFETIME_2":
                mPetLifeTimes[1].value = (float)objVal;
                break;
            case "SET_PET_LIFETIME_3":
                mPetLifeTimes[2].value = (float)objVal;
                break;
            case "SET_PET_LIFETIME_4":
                //mFriendHelpCoundownSlider.value = (float)objVal;
                mPetLifeTimes[3].value = (float)objVal;
                break;
            case "CLEAR_ALL_BUFF":
                ClearAllBuff();
                break;
			//case "FRIEND_HELP":
			//	FriendHelpPetIcon();
			//	break;
			//case "FRIEND_HELP_END":
			//	SetVisible ("pet_blood_info(Clone)_3", false);
			//	break;
            //case "FRIEND_HELP_COUNTDOWN":
            //	mFriendHelpCoundownSlider.value = (float)objVal;
            //	break;

        }
    }

    //private void FriendHelpPetIcon()
    //{
    //    mFriendHelpCoundownSlider = GameCommon.FindObject (mGameObjUI, "pet_disappear_bar").GetComponent<UISlider>();
    //    mFriendHelpCoundownSlider.value = 1f;

    //    if(PetLogicData.mFreindPetData != null)
    //    {
    //        int iCurrentFriendHelpPetID = PetLogicData.mFreindPetData.tid;
    //        DataRecord petConfig = DataCenter.mActiveConfigTable.GetRecord (iCurrentFriendHelpPetID);
    //        if(petConfig != null)
    //        {
    //            GameObject friendHelpPetIcon = GameCommon.FindObject (mGameObjUI, "pet_blood_info(Clone)_3");
    //            InitPetIcon(friendHelpPetIcon, iCurrentFriendHelpPetID, petConfig["STAR_LEVEL"]);

    //            GetComponent<UIGridContainer>("grid").Reposition();
    //        }
    //        else 
    //            DEBUG.Log ("No exist pet config >" + iCurrentFriendHelpPetID.ToString());
    //    }
    //}

    private void InitInfo()
    {
        InitPlayerInfo();
        InitPetInfo();
    }

    private void InitPlayerInfo()
    {
		SetRoleData ();

        GameObject playerInfoObj = GameCommon.FindObject(mGameObjUI, "ui_blood_magic_back_ground");

        GameObject hpObj = GameCommon.FindObject(playerInfoObj, "blood_bar");
        mHpSlider = hpObj.GetComponent<UISlider>();
        mHpMaxLabel = GameCommon.FindObject(hpObj, "blood_max_label").GetComponent<UILabel>();
        mHpCurLabel = GameCommon.FindObject(hpObj, "blood_cur_label").GetComponent<UILabel>();
        mHpSlider.value = 1f;

        GameObject mpObj = GameCommon.FindObject(playerInfoObj, "energy_bar");
        mMpSlider = mpObj.GetComponent<UISlider>();
        mMpMaxLabel = GameCommon.FindObject(mpObj, "energy_max_label").GetComponent<UILabel>();
        mMpCurLabel = GameCommon.FindObject(mpObj, "energy_cur_label").GetComponent<UILabel>();
        mMpSlider.value = 1f;

        mLevelLabel = GameCommon.FindObject(playerInfoObj, "level_label").GetComponent<UILabel>();

        UIGridContainer container = GameCommon.FindComponent<UIGridContainer>(playerInfoObj, "buffer_group");
        mCharBuffs = new BuffGroup(container, 10);
      
		SetPlayerInfo(playerInfoObj);
    }

	private void SetPlayerInfo(GameObject parentObj)
	{
		mLevelLabel.text = mRoleData.level.ToString();
        mHpMaxLabel.text = get("MAX_HP"); //GameCommon.GetBaseMaxHP(mRoleData.tid, mRoleData.level, 0).ToString();
		mHpCurLabel.text = mHpMaxLabel.text;
        mMpMaxLabel.text = get("MAX_MP"); //GameCommon.GetBaseMaxMP(mRoleData.tid, mRoleData.level, 0).ToString();
		mMpCurLabel.text = mMpMaxLabel.text;

		if (mRoleData.tid > 0)
		{
            DataRecord data = DataCenter.mActiveConfigTable.GetRecord(mRoleData.tid);
            string atlas = data["HEAD_ATLAS_NAME"];//TableCommon.GetStringFromRoleSkinConfig(mRoleData.tid, "ICON_ATLAS");
            string sprite = data["HEAD_SPRITE_NAME"];// TableCommon.GetStringFromRoleSkinConfig(mRoleData.tid, "ICON_SPRITE");
			//GameCommon.SetIcon(parentObj, "role_icon", sprite, atlas);

			UISprite _roleIcon = GameCommon.FindObject (parentObj,"role_icon").GetComponent<UISprite>();
			GameCommon.SetRoleIcon (_roleIcon,mRoleData.tid,GameCommon.ROLE_ICON_TYPE.BATTLE);
		}
	}

    private void InitPetInfo()
    {
//		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        mPetDatas = new PetData[4];
        mPetSliders = new UISlider[4];
        mPetLifeTimes = new UISlider[4];
        mPetBuffs = new BuffGroup[4];

        mActiveInfos.Clear();

        for (int i = 0; i < 4; ++i)
        {
//			if(MainProcess.mStage is PVP4Battle)
//				mPetDatas[i] = petLogicData.GetFourVsFourPetDataByPos(i + 1); 
//			else
//            	mPetDatas[i] = petLogicData.GetPetDataByPos(i + 1);
			mPetDatas[i] = GetPetData (i);
            string petIconName = "pet_blood_info(Clone)_" + i.ToString();
            GameObject petIconObj = GameCommon.FindObject(mGameObjUI, petIconName);            
            mPetSliders[i] = GameCommon.FindObject(petIconObj, "blood_bar").GetComponent<UISlider>();
            mPetSliders[i].value = 1f;
            GameObject mlifetime_bar=GameCommon.FindObject(petIconObj, "lifetime_bar");
            mPetLifeTimes[i] = mlifetime_bar.GetComponent<UISlider>();
            mPetLifeTimes[i].value = 1f;
            mlifetime_bar.SetActive(false);
            InitPetIcon(petIconObj, mPetDatas[i]);

            UIGridContainer container = GameCommon.FindComponent<UIGridContainer>(petIconObj, "buffer_group");
            mPetBuffs[i] = new BuffGroup(container, 4);
        }

        //GameObject friendIconObj = GameCommon.FindObject(mGameObjUI, "pet_blood_info(Clone)_3");
        //mFriendSlider = GameCommon.FindObject(friendIconObj, "blood_bar").GetComponent<UISlider>();
        //mFriendSlider.value = 1f;
        //friendIconObj.SetActive(false);
        //UIGridContainer friendContainer = GameCommon.FindComponent<UIGridContainer>(friendIconObj, "buffer_group");
        //mFriendBuffs = new BuffGroup(friendContainer, 4);

    }

	public virtual void SetRoleData()
	{
		mRoleData = RoleLogicData.GetMainRole();
	}

	public virtual PetData GetPetData(int pos)
    {
    //    PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;

    //    //if (MainProcess.mStage is PVP4Battle)
    //    //    return petLogicData.GetFourVsFourPetDataByPos(pos + 1); 
    //    //else
    //    return petLogicData.GetPetDataByPos(pos + 1);           
        return MainProcess.mStage.GetPetDataAtTeamPos(pos + 1);
	}

    private void RefreshHpBar()
    {
        int maxHp = get("MAX_HP");
        int curHp = get("HP");
        float percentage = curHp / (float)maxHp;
        mHpSlider.value = percentage;
        mHpMaxLabel.text = maxHp.ToString();
        mHpCurLabel.text = curHp.ToString();
    }

    private void RefreshMpBar()
    {
        int curMp = get("MP");
        int maxMp = get("MAX_MP");
        float percentage = curMp / (float)maxMp;
        mMpSlider.value = percentage;
        mMpMaxLabel.text = maxMp.ToString();
        mMpCurLabel.text = curMp.ToString();
    }

    private void RefreshLevel()
    {
        mLevelLabel.text = get("LEVEL").ToString();
    }

    virtual public void RefreshPetHp(object obj)
    {
		Friend friend = obj as Friend;
        if(friend == null)
            return;

        //PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        //int iPos = petLogicData.GetPosInCurTeam(friend.mPetData.mDBID);
        //
        //if (iPos < 1 || iPos > 3)
        //    return;
        //
        int iPos = friend.mUsePos;
        int curHp = friend.GetHp();
        int maxHp = friend.GetMaxHp();
        float percentage = curHp / (float)maxHp;

		SetSlider (iPos, percentage);
    }

	public void SetSlider(int iPos, float percentage)
	{
		if (iPos >= 1 && iPos <= 4)
			mPetSliders[iPos - 1].value = percentage;
        //else if(iPos == 4)
        //    mFriendSlider.value = percentage;
	}

    private void OnCharDead()
    {
        mCharBuffs.ClearBuff();
    }

    private void OnPetDead(int pos)
    {
        float toGrayTime=1f;
        if (pos >= 1 && pos <= 4)
        {            
            //GameCommon.SetUIVisiable(mPetSliders[pos - 1].transform.parent.gameObject, "shade", true);
            mPetBuffs[pos - 1].ClearBuff();
            //mPetSliders[pos - 1].transform.parent.gameObject.SetActive(false);
            TweenColor.Begin(mPetSliders[pos - 1].transform.parent.gameObject, toGrayTime, Color.grey);
            mActiveInfos.Remove(mPetSliders[pos - 1].transform.parent.gameObject);
          // Arrangement.RemoveThenPlaceSmoothly(mActiveInfos, mPetSliders[pos - 1].transform.parent.gameObject, PLACE_OFFSET, PLACE_DURATION);
        }
        //else if (pos == 4)
        //{         
        //    //GameCommon.SetUIVisiable(mFriendSlider.transform.parent.gameObject, "shade", true);
        //    mFriendBuffs.ClearBuff();
        //   // mFriendSlider.transform.parent.gameObject.SetActive(false);
        //    TweenColor.Begin(mFriendSlider.transform.parent.gameObject, toGrayTime, Color.grey);
        //    mActiveInfos.Remove(mFriendSlider.transform.parent.gameObject);
        //   // Arrangement.RemoveThenPlaceSmoothly(mActiveInfos, mFriendSlider.transform.parent.gameObject, PLACE_OFFSET, PLACE_DURATION);
        //}

        //GetComponent<UIGridContainer>("grid").Reposition();
    }


    private void OnPetAlive(int pos)
    {
        if (pos >= 1 && pos <= 4)
        {
            mPetSliders[pos - 1].transform.parent.gameObject.SetActive(true);
            mPetSliders[pos - 1].value = 1f;
            mPetLifeTimes[pos - 1].value = 1f;
            Arrangement.AddThenPlaceSmoothly(mActiveInfos, mPetSliders[pos - 1].transform.parent.gameObject, PLACE_OFFSET, PLACE_DURATION);
        }
        //else
        //{
        //    FriendHelpPetIcon();
        //    Arrangement.AddThenPlaceSmoothly(mActiveInfos, mFriendSlider.transform.parent.gameObject, PLACE_OFFSET, PLACE_DURATION);
        //}

        //GetComponent<UIGridContainer>("grid").Reposition();
    }

    //private void OnFriendDead()
    //{
    //    GameCommon.SetUIVisiable(mFriendSlider.transform.parent.gameObject, "shade", true);
    //    mFriendBuffs.ClearBuff();
    //}

    private void ClearAllBuff()
    {
        mCharBuffs.ClearBuff();
        //mFriendBuffs.ClearBuff();

        for (int i = 0; i < mPetBuffs.Length; ++i)
            mPetBuffs[i].ClearBuff();
    }

    private void InitPetIcon(GameObject petIconObj, PetData petData)
    {
        if(petData != null)
        {
            InitPetIcon(petIconObj, petData.tid, petData.starLevel);
        }
        petIconObj.SetActive(false);
    }

    private void InitPetIcon(GameObject petIconObj, int modelIndex, int starLevel)
    {
        if (modelIndex == 0)
        {
            petIconObj.SetActive(false);
        }
        else
        {
            petIconObj.SetActive(true);
            UISprite petSprite = petIconObj.GetComponent<UISprite>();
            GameCommon.SetPetIcon(petSprite, modelIndex);
            GameCommon.SetUIVisiable(petIconObj, "shade", false);
//            GameCommon.SetUIText(petIconObj, "star_level_label", starLevel.ToString());
			GameCommon.SetStarLevelLabel (petIconObj, starLevel, "star_level_label" );
        }
    }

    private void Release()
    {
        mRoleData = null;
        mPetDatas = null;
        mHpSlider = null;
        mMpSlider = null;
        mPetSliders = null;
        //mFriendSlider = null;
        mHpMaxLabel = null;
        mHpCurLabel = null;
        mMpMaxLabel = null;
        mMpCurLabel = null;
        mLevelLabel = null;
        mCharBuffs = null;
        mPetBuffs = null;
        //mFriendBuffs = null;
        //mFriendHelpCoundownSlider = null;
    }
}

public class BattlePlayerExpWindow : tWindow
{
	private RoleData mRoleData;
	private UISlider mExpSlider;
	private UILabel mTimeLabel;
    private UILabel mRammbockTimeLabel;

	
	public override void Open(object param)
	{
		base.Open(param);
        GameCommon.FindObject(mGameObjUI, "RammbockTimeLabel").SetActive(false);
		Refresh(param);
	}
	
	public override bool Refresh(object param)
	{
		InitInfo();
		
		return true;
	}
	
	public override void Close()
	{
		Release();
		base.Close();
	}
	
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		if (!IsOpen())
			return;

        switch (keyIndex)
        {
            case "EXP":
                RefreshExpBar();
                break;
            case "TIME":
                RefreshTime((float)objVal);
                break;
        }
	}
	
	private void InitInfo()
	{
		mRoleData = RoleLogicData.GetMainRole();

//		UISprite sprite = GameCommon.FindObject(mGameObjUI, "foreground").GetComponent<UISprite>();
//		if(sprite != null)
//		{
//			sprite.width = Screen.width;
//		}

		mExpSlider = GameCommon.FindObject(mGameObjUI, "experience_bar").GetComponent<UISlider>();
		mExpSlider.value = mRoleData.exp / (float)TableCommon.GetRoleMaxExp(mRoleData.level);

		mTimeLabel = GameCommon.FindObject(mGameObjUI, "time_num").GetComponent<UILabel>();
		mTimeLabel.text = GetRemainedTime((float)GetStageTime());

        mRammbockTimeLabel = GameCommon.FindObject(mGameObjUI, "RammbockTimeLabel").GetComponent<UILabel>();
        mRammbockTimeLabel.text = "";
	}
	
	private void RefreshExpBar()
	{
		int curExp = get("EXP");
		int maxExp = get("MAX_EXP");
		float percentage = curExp / (float)maxExp;
		mExpSlider.value = percentage;
	}

	public static string GetRemainedTime(float remainedTime)
	{
		string ret = "";
		if(GetStageTime() !=0)
		{
			if(remainedTime <= 0)
			{
				remainedTime = 0;
			}
			ret = GameCommon.FormatTime(remainedTime);
		}
		else
		{
			ret = TableCommon.getStringFromStringList(STRING_INDEX.BATTLE_REMAINED_TIME_TIPS); //"无限制";
		}
		return ret;
	}

	public static int GetStageTime()
	{
		int stageId = DataCenter.Get("CURRENT_STAGE");
		int stageTime = TableCommon.GetNumberFromConfig (stageId, "STAGE_TIME", DataCenter.mStageTable);
		return stageTime;
	}

    private void RefreshTime(float fTime)
    {
        if (mTimeLabel != null)
        {
			mTimeLabel.text = GetRemainedTime(GetStageTime() - fTime);
			if(GetStageTime() !=0 && GetStageTime() -fTime < 11)
			{
				mTimeLabel.color = Color.red;
			}
        }
        //新增战斗内胜利条件显示
        //by chenliang
        //begin

//        if (DataCenter.Self.get("I_AM_IN_RAMMBOCK"))
//-------------------
        //防止打印Warning
        if (RammbockBattle.IsInRammbockBattle)

        //end
        {
            if ((object)DataCenter.Self.get("NOW_STAGE_WIN_CONDITION") != null)
            {
                GameCommon.FindObject(mGameObjUI, "RammbockTimeLabel").SetActive(true);
                int condition = int.Parse(DataCenter.Self.get("NOW_STAGE_WIN_CONDITION"));
                DataRecord r = DataCenter.mStageStar.GetRecord(condition);
                int condition_type = int.Parse(r.getData("STARTYPE"));
                float condition_num = 0;
                float need_condition_num = 0;
                string mstate = "";
                switch (condition_type)
                {
                    case 0: mstate = fTime.ToString("f0");
                        condition_num = fTime;
                        need_condition_num = float.Parse(r.getData("STARVAR"));
                        if (condition_num > need_condition_num)
                        {
                            mRammbockTimeLabel.color = Color.red;
                        }
                        else
                        {
                            mRammbockTimeLabel.color = Color.white;
                        }
                        break;
                    case 4: mstate = (MainProcess.GetTeamHpRate() * 100).ToString("f1") + "%";
                        condition_num = MainProcess.GetTeamHpRate();
                        need_condition_num = float.Parse(r.getData("STARVAR")) / 100;
                        if (condition_num < need_condition_num)
                        {
                            mRammbockTimeLabel.color = Color.red;
                        }
                        else
                        {
                            mRammbockTimeLabel.color = Color.white;
                        }
                        break;
                    case 1: mstate = MainProcess.GetTeamDeadCount().ToString();
                        condition_num = MainProcess.GetTeamDeadCount();
                        need_condition_num = float.Parse(r.getData("STARVAR"));
                        if (condition_num > need_condition_num)
                        {
                            mRammbockTimeLabel.color = Color.red;
                        }
                        else
                        {
                            mRammbockTimeLabel.color = Color.white;
                        }
                        break;
                }

                string introduce = r.getData("STARNAME_STATE");
                introduce = introduce.Replace("{0}", mstate);
                introduce = introduce.Replace("{1}", r.getData("STARVAR").ToString());
                mRammbockTimeLabel.text = introduce;

            }
            else
            { GameCommon.FindObject(mGameObjUI, "RammbockTimeLabel").SetActive(false); }
        }
        else
        {
            GameCommon.FindObject(mGameObjUI, "RammbockTimeLabel").SetActive(false);
        }
    }
	
	private void Release()
	{
		mRoleData = null;
		mExpSlider = null;
	}
}

public class BattleBossHpWindow : tWindow
{
    private static readonly Color COUNT_NORMAL_COLOR = new Color(1f, 0.8f, 0f);
    private static readonly Color COUNT_WARN_COLOR = new Color(1f, 0f, 0f);

    public int mMaxHp = 1;
    private UILabel mCountdown;

    public override void OnOpen()
    {
        mCountdown = GetComponent<UILabel>("battle_countdown");
        mCountdown.text = "";
        mCountdown.color = COUNT_NORMAL_COLOR;
        SetVisible("no_time_limit_sprite", false);

        bool numberVisible = MainProcess.mStage != null;
        SetVisible("boss_cur_label", numberVisible);
        SetVisible("boss_label", numberVisible);
        SetVisible("boss_max_label", numberVisible);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        if (keyIndex == "MAX_HP")
        {
            mMaxHp = (int)objVal;
            if (mMaxHp <= 0)
                mMaxHp = 1;

            SetText("boss_max_label", mMaxHp.ToString());
        }
        else if (keyIndex=="HP")
        {
            int hp = (int)objVal;
            SetText("boss_cur_label", hp.ToString());
            GameObject obj = GetSub("ui_bossblood");
            if (obj != null)
            {
                UISlider slider = obj.GetComponent<UISlider>();
                if (slider != null)
                {
                    
                    slider.value = (float)hp / mMaxHp;
                }
            }
        }
        else if (keyIndex == "COUNTDOWN")
        {
            SetCountDown((float)objVal);
        }

        base.onChange(keyIndex, objVal);
    }

    private void SetCountDown(float time)
    {
        if (mCountdown == null)
            return;

        SetVisible("no_time_limit_sprite", time == Mathf.Infinity);

        if (time == Mathf.Infinity)
        {         
            mCountdown.text = "";
        }
        else if (time <= 0f)
        {
            mCountdown.text = "0.00";
        }
        else
        {
            TimeSpan span = new TimeSpan((long)(time * 10000000));
            int centiseconds = span.Milliseconds / 10;

            if (span.Minutes == 0)
            {
                mCountdown.text = span.Seconds + "." + string.Format("{0:D2}", centiseconds);

                if (span.Seconds < 10)
                {
                    mCountdown.color = COUNT_WARN_COLOR;
                }
            }
            else
            {
                mCountdown.text = span.Minutes + " : " + span.Seconds + "." + string.Format("{0:D2}", centiseconds);
            }
        }
    }
}

public class PveMiniMapWindow : tWindow
{
    private UILabel mGoldLabel;
    private UILabel mItemLabel;
    //private UILabel mTimeLabel;

    public override void Open(object param)
    {
        base.Open(param);
        GameCommon.SetUIButtonEnabled(mGameObjUI, "battle_settings", true);
        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        InitPlayerInfo();

        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "GOLD":
                RefreshGold((int)objVal);
                break;
            case "ITEM":
                RefreshItem((int)objVal);
                break;
            case "ENABLE_SETTING_BTN":
                bool bIsEnabled = (bool)objVal;
                GameCommon.SetUIButtonEnabled(mGameObjUI, "battle_settings", bIsEnabled);

                if (!bIsEnabled)
                    DataCenter.CloseWindow("BATTLE_SETTINGS_WINDOW");
                break;
            case "SHOW_STAGE_NAME":
                {
                    if (objVal is STAGE_TYPE)
                    {
                        ShowGuankName((STAGE_TYPE)objVal);
                    }
                }
                break;
        }
    }

    public void ShowGuankName(STAGE_TYPE type)
    {
        int stageIndex = DataCenter.Get("CURRENT_STAGE");
        string stageNumber = TableCommon.GetStringFromStageConfig(stageIndex, "STAGENUMBER");
        SetText("name", stageNumber);
    }

    private void InitPlayerInfo()
    {
        mGoldLabel = GameCommon.FindObject(mGameObjUI, "gold_num").GetComponent<UILabel>();
		mGoldLabel.text = "0";
        mItemLabel = GameCommon.FindObject(mGameObjUI, "item_num").GetComponent<UILabel>();
        mItemLabel.text = "0";
//        mTimeLabel = GameCommon.FindObject(mGameObjUI, "time_num").GetComponent<UILabel>();
//        mTimeLabel.text = GameCommon.FormatTime(0f);
    }

    private void RefreshGold(int iGold)
    {
		if (mGoldLabel!=null)
		{
            mGoldLabel.text = iGold.ToString();
		}
    }

    private void RefreshItem(int iItem)
    {
        if (mItemLabel != null)
        {
            mItemLabel.text = iItem.ToString();
        }
    }

    //private void RefreshTime(float fTime)
    //{
    //    if (mTimeLabel != null)
    //    {
    //        mTimeLabel.text = GameCommon.FormatTime(fTime);
    //    }
    //}
}


public class BattleAutoBattleWindow : tWindow
{
    public override void OnOpen()
    {
        //SetVisible("auto_fight", true);
        GameCommon.SetUIButtonEnabled(mGameObjUI, "auto_fight", true);
        BATTLE_CONTROL control = MainProcess.mStage.GetBattleControl();//MainProcess.mStage.IsBOSS() ? BossBattle.mBattleControl : PVEStageBattle.mBattleControl;
        SetPattern(control);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        if (!IsOpen())
            return;

        if (keyIndex == "AUTO_BATTLE")
        {
            BATTLE_CONTROL control = (BATTLE_CONTROL)(int)objVal;
            SetPattern(control);
        }
        //else if(keyIndex == "AUTO_BATTLE_INVALID")
        //{
        //    GetSub ("auto_fight").GetComponent<BoxCollider>().enabled = (bool)objVal;
        //}
    }

    private void SetPattern(BATTLE_CONTROL control)
    {
        SetVisible("will_auto_fight", control == BATTLE_CONTROL.MANUAL);
        SetVisible("will_auto_skill", control == BATTLE_CONTROL.AUTO_ATTACK);
        SetVisible("will_manual_fight", control == BATTLE_CONTROL.AUTO_SKILL);
    }
}

public class PetAttackBossWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_pet_attack_boss", new DefineFactory<Button_pet_attack_boss>());
        EventCenter.Self.RegisterEvent("Button_battle_speed", new DefineFactory<Button_battle_speed>());
        EventCenter.Self.RegisterEvent("Button_auto_fight", new DefineFactory<Button_auto_fight>());
        EventCenter.Self.RegisterEvent("Button_update_shop_button", new DefineFactory<Button_update_shop_button>());       //战斗失败界面 3种提升战力的选择
        EventCenter.Self.RegisterEvent("Button_update_equip_button", new DefineFactory<Button_update_equip_button>());
        EventCenter.Self.RegisterEvent("Button_update_active_stage_button", new DefineFactory<Button_update_active_stage_button>());
    }

    //public virtual bool Refresh(object param) 
    //{
    //    if (MainProcess.mStage != null)
    //    {
    //        MainProcess.mStage.ForcePetFollow(!MainProcess.mStage.mbForcePetFollow);
    //        SetVisible("pet_allow_off", !MainProcess.mStage.mbForcePetFollow);
    //        SetVisible("pet_allow_on", MainProcess.mStage.mbForcePetFollow);
    //    }

    //    return true; 
    //}

    public override void OnOpen()
    {
        int level = RoleLogicData.GetMainRole().level;

        SetVisible("auto_fight", level >= CommonParam.btnAutoBattleOpenLevel);
        SetVisible("battle_speed", level >= CommonParam.btnSpeedUpOpenLevel);
        SetVisible("pet_attack_boss", level >= CommonParam.btnFollowOpenLevel);

        if (MainProcess.mStage != null)
        {
            MainProcess.mStage.ForcePetFollow(MainProcess.mStage.mbForcePetFollow);
            SetFollowUI(MainProcess.mStage.mbForcePetFollow);

            GameCommon.SetUIButtonEnabled(mGameObjUI, "auto_fight", true);
            BATTLE_CONTROL control = MainProcess.mStage.GetBattleControl();//MainProcess.mStage.IsBOSS() ? BossBattle.mBattleControl : PVEStageBattle.mBattleControl;
            SetControlUI(control);
            BATTLE_SPEED speed;

            if (MainProcess.mStage.IsBOSS())
            {
                speed = BossBattle.mBattleSpeed;
            }
            else if (MainProcess.mStage.IsGuildBOSS())
            {
                speed = GuildBossBattle.mBattleSpeed;
            }
            else
            {
                speed = PVEStageBattle.mBattleSpeed;
            }

            SetSpeedUI(speed);        
            Button_battle_speed.lastTipRealTime = -99999f;
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if (objVal.GetType()==typeof(string))
        {
           if(objVal.ToString()=="OpenGrabAutoFight")
           {
               GameCommon.FindObject(mGameObjUI,"pet_attack_boss").SetActive(false);
               GameCommon.FindObject(mGameObjUI,"battle_speed").SetActive(false);
           }
        }

        if (!IsOpen())
            return;

        if (keyIndex == "AUTO_BATTLE")
        {
            BATTLE_CONTROL control = (BATTLE_CONTROL)(int)objVal;
            SetControlUI(control);
        }
        else if (keyIndex == "PET_FOLLOW")
        {
            bool follow = (bool)objVal;
            MainProcess.mStage.ForcePetFollow(follow);
            SetFollowUI(follow);
        }
        else if (keyIndex == "BATTLE_SPEED")
        {
            BATTLE_SPEED speed = (BATTLE_SPEED)(int)objVal;
            SetSpeedUI(speed);
        }
    }

    private void SetControlUI(BATTLE_CONTROL control)
    {
        SetVisible("will_auto_fight", control == BATTLE_CONTROL.MANUAL);
        SetVisible("will_auto_skill", control == BATTLE_CONTROL.AUTO_ATTACK);
        SetVisible("will_manual_fight", control == BATTLE_CONTROL.AUTO_SKILL);
    }

    private void SetFollowUI(bool follow)
    {
        SetVisible("pet_allow_off", !follow);
        SetVisible("pet_allow_on", follow);
    }

    private void SetSpeedUI(BATTLE_SPEED speed)
    {
        SetVisible("speed_x1", speed == BATTLE_SPEED.X1);
        SetVisible("speed_x2", speed == BATTLE_SPEED.X2);
        SetVisible("speed_x3", speed == BATTLE_SPEED.X3);
    }
}

public class Button_pet_attack_boss : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PET_ATTACK_BOSS_WINDOW", "PET_FOLLOW", !MainProcess.mStage.mbForcePetFollow);
        return true;
    }
}


public class Button_auto_fight : CEvent
{
    public override bool _DoEvent()
    {
        //if (MainProcess.mStage.IsBOSS())
        //{
        //    switch (BossBattle.mBattleControl)
        //    {
        //        case BATTLE_CONTROL.MANUAL:
        //            BossBattle.mBattleControl = BATTLE_CONTROL.AUTO_ATTACK;
        //            break;

        //        case BATTLE_CONTROL.AUTO_ATTACK:
        //            BossBattle.mBattleControl = BATTLE_CONTROL.AUTO_SKILL;
        //            break;

        //        case BATTLE_CONTROL.AUTO_SKILL:
        //            BossBattle.mBattleControl = BATTLE_CONTROL.MANUAL;
        //            break;
        //    }

        //    DataCenter.SetData("PET_ATTACK_BOSS_WINDOW", "AUTO_BATTLE", (int)BossBattle.mBattleControl);

        //    if (MainProcess.mStage.mbBattleActive
        //        && !MainProcess.mStage.mbBattleFinish)
        //    {
        //        //Time.timeScale = BossBattle.mBattleControl == BATTLE_CONTROL.MANUAL ? 1f : AIParams.autoBattleTimeScale;

        //        if (BossBattle.mBattleControl != BATTLE_CONTROL.MANUAL)
        //        {
        //            MainProcess.mStage.mHasAutoFighted = true;
        //        }
        //    }
        //}
        //else 
        //{
        //    switch (PVEStageBattle.mBattleControl)
        //    {
        //        case BATTLE_CONTROL.MANUAL:
        //            PVEStageBattle.mBattleControl = BATTLE_CONTROL.AUTO_ATTACK;
        //            break;

        //        case BATTLE_CONTROL.AUTO_ATTACK:
        //            PVEStageBattle.mBattleControl = BATTLE_CONTROL.AUTO_SKILL;
        //            break;

        //        case BATTLE_CONTROL.AUTO_SKILL:
        //            PVEStageBattle.mBattleControl = BATTLE_CONTROL.MANUAL;
        //            break;
        //    }

        //    DataCenter.SetData("PET_ATTACK_BOSS_WINDOW", "AUTO_BATTLE", (int)PVEStageBattle.mBattleControl);

        //    if (MainProcess.mStage.mbBattleActive
        //        && !MainProcess.mStage.mbBattleFinish)
        //    {
        //        //Time.timeScale = PVEStageBattle.mBattleControl == BATTLE_CONTROL.MANUAL ? 1f : AIParams.autoBattleTimeScale;

        //        if (PVEStageBattle.mBattleControl != BATTLE_CONTROL.MANUAL)
        //        {
        //            MainProcess.mStage.mHasAutoFighted = true;
        //        }
        //    }
        //}

        if (MainProcess.mStage != null)
        {
            switch (MainProcess.mStage.GetBattleControl())
            {
                case BATTLE_CONTROL.MANUAL:
                    MainProcess.mStage.SetBattleControl(BATTLE_CONTROL.AUTO_ATTACK);
                    break;

                case BATTLE_CONTROL.AUTO_ATTACK:
                    MainProcess.mStage.SetBattleControl(BATTLE_CONTROL.AUTO_SKILL);
                    break;

                case BATTLE_CONTROL.AUTO_SKILL:
                    MainProcess.mStage.SetBattleControl(BATTLE_CONTROL.MANUAL);
                    break;
            }

            DataCenter.SetData("PET_ATTACK_BOSS_WINDOW", "AUTO_BATTLE", (int)MainProcess.mStage.GetBattleControl());

            if (MainProcess.mStage.mbBattleActive && !MainProcess.mStage.mbBattleFinish)
            {
                if (MainProcess.mStage.GetBattleControl() != BATTLE_CONTROL.MANUAL)
                {
                    MainProcess.mStage.mHasAutoFighted = true;
                }
            }
        }

        return true;
    }
}


public class Button_battle_speed : CEvent
{
    private static readonly float realDuration = 60f;
    public static float lastTipRealTime = -99999f;

    public override bool _DoEvent()
    {
        if (MainProcess.mStage.IsBOSS())
        {
            BATTLE_SPEED nextSpeed = GetNextSpeed(BossBattle.mBattleSpeed);

            if (TrySwitchSpeed(nextSpeed, ref BossBattle.mBattleSpeed))
            {
                if (MainProcess.mStage.mbBattleActive && !MainProcess.mStage.mbBattleFinish)
                {
                    Time.timeScale = StageBattle.GetTimeScale(BossBattle.mBattleSpeed);
                }

                DataCenter.SetData("PET_ATTACK_BOSS_WINDOW", "BATTLE_SPEED", (int)BossBattle.mBattleSpeed);
                PlayerPrefs.SetString(CommonParam.mUId + "_MY_AUTO_BATTLE_SPEED", ((int)BossBattle.mBattleSpeed).ToString());
                PlayerPrefs.Save();
            }
        }
        else if (MainProcess.mStage.IsGuildBOSS())
        {
            BATTLE_SPEED nextSpeed = GetNextSpeed(GuildBossBattle.mBattleSpeed);

            if (TrySwitchSpeed(nextSpeed, ref GuildBossBattle.mBattleSpeed))
            {
                if (MainProcess.mStage.mbBattleActive && !MainProcess.mStage.mbBattleFinish)
                {
                    Time.timeScale = StageBattle.GetTimeScale(GuildBossBattle.mBattleSpeed);
                }

                DataCenter.SetData("PET_ATTACK_BOSS_WINDOW", "BATTLE_SPEED", (int)GuildBossBattle.mBattleSpeed);
                PlayerPrefs.SetString(CommonParam.mUId + "_MY_AUTO_BATTLE_SPEED", ((int)GuildBossBattle.mBattleSpeed).ToString());
                PlayerPrefs.Save();
            }
        }
        else
        {
            BATTLE_SPEED nextSpeed = GetNextSpeed(PVEStageBattle.mBattleSpeed);

            if (TrySwitchSpeed(nextSpeed, ref PVEStageBattle.mBattleSpeed))
            {
                if (MainProcess.mStage.mbBattleActive && !MainProcess.mStage.mbBattleFinish)
                {
                    Time.timeScale = StageBattle.GetTimeScale(PVEStageBattle.mBattleSpeed);
                }

                DataCenter.SetData("PET_ATTACK_BOSS_WINDOW", "BATTLE_SPEED", (int)PVEStageBattle.mBattleSpeed);
                PlayerPrefs.SetString(CommonParam.mUId + "_MY_AUTO_BATTLE_SPEED", ((int)PVEStageBattle.mBattleSpeed).ToString());
                PlayerPrefs.Save();
            }
        }

        return true;
    }

    private static BATTLE_SPEED GetNextSpeed(BATTLE_SPEED currentSpeed)
    {
        switch (currentSpeed)
        {
            case BATTLE_SPEED.X1:
                return BATTLE_SPEED.X2;
            case BATTLE_SPEED.X2:
                return BATTLE_SPEED.X3;
            default:
                return BATTLE_SPEED.X1;
        }
    }

    private static bool TrySwitchSpeed(BATTLE_SPEED battleSpeed, ref BATTLE_SPEED finalSpeed)
    {
        if (CheckCanApply(battleSpeed))
        {
            finalSpeed = battleSpeed;
            return true;
        }
        else if (Time.realtimeSinceStartup - lastTipRealTime < realDuration && battleSpeed != BATTLE_SPEED.X2)
        {
            finalSpeed = BATTLE_SPEED.X1;
            lastTipRealTime = -99999f;
            return true;
        }
        else 
        {
            ShowSwitchFailTip(battleSpeed);
            lastTipRealTime = Time.realtimeSinceStartup;
            return false;
        }
    }

    public static bool CheckCanApply(BATTLE_SPEED battleSpeed)
    {
        int level = RoleLogicData.GetMainRole().level;
        int vip = RoleLogicData.Self.vipLevel;

        switch (battleSpeed)
        {
            case BATTLE_SPEED.X2:
                return level >= CommonParam.speedX2OpenLevel || vip >= CommonParam.speedX2OpenVIP;
            case BATTLE_SPEED.X3:
                return level >= CommonParam.speedX3OpenLevel || vip >= CommonParam.speedX3OpenVIP;
            default:
                return true;
        }
    }

    private static void ShowSwitchFailTip(BATTLE_SPEED battleSpeed)
    {
        switch (battleSpeed)
        {
            case BATTLE_SPEED.X2:
                DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_SPEED_X2);
                break;

            case BATTLE_SPEED.X3:
                DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_SPEED_X3);
                break;
        }      
    }
}


public class Button_get_friend_help_button : CEvent
{
	float iCountdown = 0;
	int mFriendHelpCoundownTime = 30;
	public override bool _DoEvent()
	{
        if (MainProcess.mStage == null || MainProcess.mStage.mbBattleFinish)
        {
            return false;
        }

		DataCenter.SetData ("BATTLE_SKILL_WINDOW", "FRIEND_HELP", true);
		DataCenter.SetData("BATTLE_PLAYER_WINDOW", "FRIEND_HELP", true);

		BaseObject friendPetObj = Character.Self.CreateFriendPet (PetLogicData.mFreindPetData);

		set ("FRIEND_PET_OBJ", friendPetObj);
		WaitTime (mFriendHelpCoundownTime);
		StartUpdate ();

		return true;
	}

	public override bool Update (float secondTime)
	{
		iCountdown += secondTime;
		if(iCountdown > mFriendHelpCoundownTime)
		{
			iCountdown = mFriendHelpCoundownTime;
			return false;
		}
		else 
			DataCenter.SetData("BATTLE_PLAYER_WINDOW", "FRIEND_HELP_COUNTDOWN", 1 - iCountdown/mFriendHelpCoundownTime);
		
		return true; 
	}

	public override void DoOverTime()
	{
		base.DoOverTime ();

        if (MainProcess.mStage == null || MainProcess.mStage.mbBattleFinish)
        {
            return;
        }

		if(PetLogicData.mFreindPetData != null) PetLogicData.mFreindPetData = null;

		DataCenter.SetData("BATTLE_PLAYER_WINDOW", "FRIEND_HELP_END", true);
		
		GameObject obj = (GameObject) getObject("BUTTON");
		obj.SetActive (false);
		BaseObject  friendPetObj = (BaseObject)getObject ("FRIEND_PET_OBJ");
		friendPetObj.ChangeHp (-100000000);
	}
}

public class BattleSkillWindow : tWindow
{
	tLogicData uiData ;

    public static List<int> mSkillIdList = new List<int>();  //技能id列表

    public static List<int> GetmSkillIdList()
    {
        return mSkillIdList;
    }

    public static void InitSkillIdList()
    {
        mSkillIdList.Clear();
    }

	public override void Init ()
	{
		//EventCenter.Self.RegisterEvent ("Button_get_friend_help_button", new DefineFactory<Button_get_friend_help_button>());

		uiData = new tLogicData();
	}

    //public override void onChange (string keyIndex, object objVal)
    //{
    //	base.onChange (keyIndex, objVal);
    //	if(keyIndex == "FRIEND_HELP")
    //	{
    //		GameObject skillEffectObj = GetSub ("get_friend_help_button");
    //
    //		GetComponent<BoxCollider>("get_friend_help_button").enabled = false;
    //		SetVisible ("do_skill_4", true);
    //		SetVisible ("friend_pet_or_skill_icon", false);
    //		UISprite friendHelpRound = GetSprite ("friend_help_round");
    //		if(friendHelpRound != null) 
    //		{
    //			friendHelpRound.spriteName = "ui_skill_ring";
    //			friendHelpRound.MakePixelPerfect ();
    //		}
    //
    //		UISprite cdSprite = GetSprite("do_skill_cd_4");
    //		UILabel cdLable = GetLable("do_skill_cd_time_4");
    //		if(cdSprite != null && cdLable != null)
    //		{
    //			SkillButtonData skill_1 = new SkillButtonData();
    //			skill_1.mCDSprite = cdSprite;
    //			skill_1.mCDLabel = cdLable;
    //            skill_1.mPetUsePos = 4;
    //			cdLable.text = "";
    //
    //			skill_1.InitEffect(skillEffectObj);
    //
    //			if(PetLogicData.mFreindPetData != null)
    //			{
    //				int iCurrentFriendHelpPetID = PetLogicData.mFreindPetData.mModelIndex;
    //				DataRecord config = DataCenter.mActiveConfigTable.GetRecord(iCurrentFriendHelpPetID);
    //				if (config != null)
    //				{
    //					int iPetSkillID = (int)config["PET_SKILL_1"];
    //					
    //					skill_1.set("SKILL_INDEX", iPetSkillID);
    //					if (iPetSkillID > 0)
    //					{
    //						skill_1.mSkillElement = (ELEMENT_TYPE)(int)config["ELEMENT_INDEX"];
    //
    //						cdSprite.fillAmount = 0;
    //						SetSkillIcon (cdSprite, iPetSkillID);
    //
    //						DataRecord skillConfig = DataCenter.mSkillConfigTable.GetRecord(iPetSkillID);
    //						int need_mp = skillConfig["NEED_MP"];
    //						if(need_mp != 0) SetText ("mana_4", need_mp.ToString ());
    //
    //						skill_1.mNeedMpText = GameCommon.FindComponent<UILabel>(skillEffectObj.transform.parent.gameObject, "mana_4");
    //						skill_1.InitStart(skillEffectObj);
    //					}
    //					else
    //						cdSprite.fillAmount = 1;
    //				}
    //				else 
    //					DEBUG.LogError("No exist pet config >" + iCurrentFriendHelpPetID.ToString());
    //			}
    //			else 
    //				LOG.log ("PetLogicData mFriendPetData is null");
    //
    //			uiData.registerData("do_skill_4", skill_1);
    //		}
    //	}
    //
    //	if(keyIndex == "BATTLE_END")
    //		SetVisible ("get_friend_help_button", false);
    //}

    public override void Open(object param)
    {
		base.Open(param);

        InitSkillIdList();
		Refresh(param);
	}

    /// <summary>
    /// 根据技能位置设置技能开放描述
    /// </summary>
    /// <param name="kSkillPos"></param>
    private void SetSkillBtnOpenDescription(int kSkillPos)
    {
        if (kSkillPos < 1 || kSkillPos > (int)TEAM_POS.PET_3)
            return;

        string skillObjName = "do_skill_father_" + kSkillPos;
        GameObject _openLevelRoot = GameCommon.FindObject(GetSub(skillObjName), "skill_open_description_root");
        UILabel _skillOpenLevelDescription = GameCommon.FindComponent<UILabel>(_openLevelRoot, "skill_open_label");
        UILabel _openDescription = GameCommon.FindComponent<UILabel>(_openLevelRoot, "open_description");
        // 有助阵符灵时则不显示提示文字
        if (MainProcess.mStage.GetPetDataAtTeamPos(kSkillPos) != null)
        {
            _openLevelRoot.SetActive(false);
            return;
        }
        
        //1.判断等级是否开放

        TeamPosData _teamPosData = TeamManager.GetTeamPosData(kSkillPos);
        int _openLevel = _teamPosData.openLevel;
        if (_openLevel > RoleLogicData.GetMainRole().level && !Guide.inPrologue)
        {
            if (_openLevelRoot != null)
            {
                _openLevelRoot.SetActive(true);
                if (_skillOpenLevelDescription != null)
                {
                    _skillOpenLevelDescription.text = "等级" + _openLevel;
                }
                if (_openDescription != null)
                {
                    _openDescription.text = "开启";
                }
            }
        }
        else 
        {
            //判断符灵是否上阵
            if (_teamPosData.bodyId <= 0)
            {
                if (_openLevelRoot != null)
                {
                    _openLevelRoot.SetActive(true);
                    if (_skillOpenLevelDescription != null)
                    {
                        _skillOpenLevelDescription.text = "上阵灵将" + kSkillPos;
                    }
                    if (_openDescription != null)
                    {
                        _openDescription.text = "可用";
                    }
                }
            }
            else 
            {
                _openLevelRoot.SetActive(false);
            }
        }
    }

	public override bool Refresh(object param)
	{
        for (int i = 0; i <= 4; ++i)
        {
            string btn = "do_skill_" + i;
            GameCommon.SetUIButtonEnabled(mGameObjUI, btn, true);
        }

        //        tLogicData uiData = new tLogicData();
        {
            GameObject btn = GetSub("do_skill_0");
            btn.SetActive(false);
            GameObject summon_1 = GameCommon.FindObject(btn, "summon_pet");
            GameObject skill_1 = GameCommon.FindObject(btn, "do_skill");
            UISprite cdObj_1 = GameCommon.FindComponent<UISprite>(btn, "do_skill_cd_0");
            UILabel cdLable_1 = GameCommon.FindComponent<UILabel>(btn, "cd");
            UILabel mana = GameCommon.FindComponent<UILabel>(btn, "mana");
            if (cdObj_1 != null && cdLable_1 != null)
            {
                UISprite sprite = cdObj_1.GetComponent<UISprite>();
                ReadySkillButtonData(uiData, summon_1, skill_1, cdObj_1, cdLable_1, mana, 0, GetSub("do_skill_father_0"));
            }
        }

        {
            GameObject btn = GetSub("do_skill_1");
            GameObject summon_1 = GameCommon.FindObject(btn, "summon_pet");
            GameObject skill_1 = GameCommon.FindObject(btn, "do_skill");
            UISprite cdObj_1 = GameCommon.FindComponent<UISprite>(btn, "do_skill_cd_1");
            UILabel cdLable_1 = GameCommon.FindComponent<UILabel>(btn, "cd");
            UILabel mana = GameCommon.FindComponent<UILabel>(btn, "mana");
            if (cdObj_1 != null && cdLable_1 != null)
            {
                UISprite sprite = cdObj_1.GetComponent<UISprite>();
                ReadySkillButtonData(uiData, summon_1, skill_1, cdObj_1, cdLable_1, mana, 1, GetSub("do_skill_father_1"));
            }
            SetSkillBtnOpenDescription(1);
        }

        {
            GameObject btn = GetSub("do_skill_2");
            GameObject summon_1 = GameCommon.FindObject(btn, "summon_pet");
            GameObject skill_1 = GameCommon.FindObject(btn, "do_skill");
            UISprite cdObj_1 = GameCommon.FindComponent<UISprite>(btn, "do_skill_cd_2");
            UILabel cdLable_1 = GameCommon.FindComponent<UILabel>(btn, "cd");
            UILabel mana = GameCommon.FindComponent<UILabel>(btn, "mana");
            if (cdObj_1 != null && cdLable_1 != null)
            {
                UISprite sprite = cdObj_1.GetComponent<UISprite>();
                ReadySkillButtonData(uiData, summon_1, skill_1, cdObj_1, cdLable_1, mana, 2, GetSub("do_skill_father_2"));
            }
            SetSkillBtnOpenDescription(2);
        }

        {
            GameObject btn = GetSub("do_skill_3");
            GameObject summon_1 = GameCommon.FindObject(btn, "summon_pet");
            GameObject skill_1 = GameCommon.FindObject(btn, "do_skill");
            UISprite cdObj_1 = GameCommon.FindComponent<UISprite>(btn, "do_skill_cd_3");
            UILabel cdLable_1 = GameCommon.FindComponent<UILabel>(btn, "cd");
            UILabel mana = GameCommon.FindComponent<UILabel>(btn, "mana");
            if (cdObj_1 != null && cdLable_1 != null)
            {
                UISprite sprite = cdObj_1.GetComponent<UISprite>();
                ReadySkillButtonData(uiData, summon_1, skill_1, cdObj_1, cdLable_1, mana, 3, GetSub("do_skill_father_3"));
            }
            SetSkillBtnOpenDescription(3);
        }

		//friend help battle UI
        //if (PetLogicData.mFreindPetData != null)
        if(MainProcess.mStage.GetPetDataAtTeamPos(4) != null)
        {           
            GameObject btn = GetSub("do_skill_4");
            btn.SetActive(true);
            GameObject summon_1 = GameCommon.FindObject(btn, "summon_pet");
            GameObject skill_1 = GameCommon.FindObject(btn, "do_skill");
            UISprite cdObj_1 = GameCommon.FindComponent<UISprite>(btn, "do_skill_cd_4");
            UILabel cdLable_1 = GameCommon.FindComponent<UILabel>(btn, "cd");
            UILabel mana = GameCommon.FindComponent<UILabel>(btn, "mana");
            if (cdObj_1 != null && cdLable_1 != null)
            {
                UISprite sprite = cdObj_1.GetComponent<UISprite>();
                ReadySkillButtonData(uiData, summon_1, skill_1, cdObj_1, cdLable_1, mana, 4, GetSub("do_skill_father_4"));
            }
            //GetComponent<BoxCollider>("get_friend_help_button").enabled = true;
            //SetVisible ("get_friend_help_button", true);
            //SetVisible ("friend_pet_or_skill_icon", true);
            //SetVisible ("do_skill_4", false);
            //UISprite friendHelpRound = GetSprite ("friend_help_round");
            //if(friendHelpRound != null) 
            //{
            //	friendHelpRound.spriteName = "ui_fr";
            //	friendHelpRound.MakePixelPerfect ();
            //}
            //UISprite icon =  GetSprite ("friend_pet_or_skill_icon");
            //GameCommon.SetPetIcon (icon, PetLogicData.mFreindPetData.mModelIndex);
        }
        else
        {
            SetVisible("do_skill_4", false);
            //SetVisible("get_friend_help_button", false);
        }


        DataCenter.Self.registerData("SKILL_UI", uiData);

		return true;
    }

    void ReadySkillButtonData(tLogicData uiData, GameObject summonObj, GameObject skillObj, UISprite cdSprite, UILabel cdLable, UILabel manaLable, int petUseIndex, GameObject skillEffectObj)
    {
        SkillButtonData skill_1 = new SkillButtonData();
        skill_1.mSummonObj = summonObj;
        skill_1.mSkillObj = skillObj;
        skill_1.mSummonCDSprite = GameCommon.FindComponent<UISprite>(summonObj, "summon_cd");
        skill_1.mCDSprite = cdSprite;
        skill_1.mCDLabel = cdLable;
        skill_1.mPetUsePos = petUseIndex;
        skill_1.mNeedMpText = manaLable;
		cdLable.text = "";

		skill_1.InitEffect(skillEffectObj);

        //PetLogicData petData = DataCenter.GetData("PET_DATA") as PetLogicData;
        PetData pet = null;
        int roleSkillIndex = 0;

        if (petUseIndex > 0)
        {
            pet = MainProcess.mStage.GetPetDataAtTeamPos(petUseIndex);
            //if (petUseIndex <= 3)
            //{
            //    //if (MainProcess.mStage is PVP4Battle)
            //    //    pet = petData.GetFourVsFourPetDataByPos(petUseIndex);
            //    //else
            //    pet = petData.GetPetDataByPos(petUseIndex);
            //}
            //else
            //    pet = PetLogicData.mFreindPetData;
        }
        else
        {
            roleSkillIndex = TableCommon.GetNumberFromActiveCongfig(RoleLogicData.Self.character.tid, "PET_SKILL_1");
        }

        if (pet != null)
        {
            //UISprite icon = GameCommon.FindComponent<UISprite>(summonObj, "icon");
            SetPetIcon(summonObj, pet.tid);
            //GameCommon.SetPetIcon(icon, pet.mModelIndex);
            skill_1.InitState(true);
            skill_1.SetQuality(pet.starLevel >= (int)PET_QUALITY.ORANGE);

            //if (petUseIndex <= 3)
            //    skill_1.SetSummonCDRate(0.2f);
            //else
                skill_1.SetSummonCDRate(0f);

            DataRecord config = DataCenter.mActiveConfigTable.GetRecord(pet.tid);
            if (config != null)
            {
                int summonCD = (int)config["SUMMON_TIME"];
                int lifeTime = (int)config["ACTIVE_TIME"];
                int petSkill = (int)config["PET_SKILL_1"];
                int punishCD = (int)config["PUNISH_TIME"];

                if (summonCD > 0)
                    skill_1.mSummonCD = summonCD / 1000f;
                else
                    skill_1.mSummonCD = SkillButtonData.SUMMON_CD;

                if (lifeTime > 0)
                    skill_1.mPetLifeTime = lifeTime / 1000f;
                else
                    skill_1.mPetLifeTime = SkillButtonData.PET_LIFE_TIME;

                skill_1.mPunishCD = punishCD / 1000f;

                skill_1.set("SKILL_INDEX", petSkill);
                if (petSkill > 0)
                {
                    skill_1.mSkillElement = (ELEMENT_TYPE)(int)config["ELEMENT_INDEX"];

                    cdSprite.fillAmount = 0;
                    SetSkillIcon(cdSprite, petSkill);

                    //string manaName = "mana_" + petUseIndex.ToString();
                    int need_mp = SkillGlobal.GetInfo(petSkill).needMP;//TableCommon.GetNumberFromSkillConfig(petSkill, "NEED_MP");

                    if (need_mp != 0) manaLable.text = need_mp.ToString();

                    //skill_1.mNeedMpText = GameCommon.FindComponent<UILabel>(skillEffectObj.transform.parent.gameObject, manaName);
                    skill_1.InitStart(skillEffectObj);

                    //					else SetText (manaName, "");
                }
                else
                {
                    cdSprite.fillAmount = 1;
                    manaLable.text = "";
                }

                string offName = "ui_skilloff_" + petUseIndex.ToString();
                GameCommon.SetUIVisiable(mGameObjUI, offName, false);
            }
            else
                EventCenter.Log(LOG_LEVEL.ERROR, "No exist pet config >" + pet.tid.ToString());
        }
        else if (roleSkillIndex > 0)
        {
            skill_1.InitState(true);

            skill_1.set("SKILL_INDEX", roleSkillIndex);

            if (roleSkillIndex > 0)
            {
                skill_1.mSkillElement = ELEMENT_TYPE.MAX;

                cdSprite.fillAmount = 0;
                SetSkillIcon(cdSprite, roleSkillIndex);

                //string manaName = "mana_" + petUseIndex.ToString();
                int need_mp = SkillGlobal.GetInfo(roleSkillIndex).needMP;//TableCommon.GetNumberFromSkillConfig(roleSkillIndex, "NEED_MP");

                if (need_mp != 0) manaLable.text = need_mp.ToString();

                //skill_1.mNeedMpText = GameCommon.FindComponent<UILabel>(skillEffectObj.transform.parent.gameObject, manaName);
                skill_1.InitStart(skillEffectObj);

                //					else SetText (manaName, "");
            }
            else
            {
                cdSprite.fillAmount = 1;
                manaLable.text = "";
            }

            string offName = "ui_skilloff_" + petUseIndex.ToString();

            GameCommon.SetUIVisiable(mGameObjUI, offName, false);
            skillObj.transform.parent.parent.localScale = Vector3.one;


        }
        else
        {
            skill_1.InitState(false);
            string offName = "ui_skilloff_" + petUseIndex.ToString();
            GameCommon.SetUIVisiable(mGameObjUI, offName, true);
            skillObj.transform.parent.parent.localScale = new Vector3(0.9f, 0.9f, 0);
            string manaName = "mana_" + petUseIndex.ToString();
            SetText(manaName, "");
        }

        string dataKey = "do_skill_" + petUseIndex.ToString();
        uiData.registerData(dataKey, skill_1);

        //计算列表
        int skillId = skill_1.get("SKILL_INDEX");
        mSkillIdList.Add(skillId);
    }

    public void SetSkillIcon(UISprite cd_sprite, int skill_id)
    {
        DataRecord config = DataCenter.mSkillConfigTable.GetRecord(skill_id);
        if (config != null)
        {
            string iconAtlas = config["SKILL_ATLAS_NAME"];
            string iconName = config["SKILL_SPRITE_NAME"];
            GameObject parent = cd_sprite.transform.parent.gameObject;

			GameObject niceSpriteObj = GameCommon.FindObject (parent, "nice_sprite");
			if(niceSpriteObj != null) MonoBehaviour.Destroy (niceSpriteObj);

            CircularSprite mySprite = new CircularSprite(true);
            NiceSprite mnicesprite=mySprite.Init(parent, 1.0f);
            mySprite.SetAtlasTexture(iconAtlas, iconName);
            mnicesprite.SetSize(100, 100);
            //GameCommon.SetUISprite(parent, "Background", icon);
        }
        else
        {
            DEBUG.LogError("SkillConfig->DataRecord is null>>>" + skill_id.ToString ());
        }
    }

    public void SetPetIcon(GameObject summonBtn, int activeIndex)
    {
        DataRecord config = DataCenter.mActiveConfigTable.GetRecord(activeIndex);

        if (config != null)
        {
            string iconAtlas = config["HEAD_ATLAS_NAME"];
            string iconName = config["HEAD_SPRITE_NAME"];

            GameObject niceSpriteObj = GameCommon.FindObject(summonBtn, "nice_sprite");
            if (niceSpriteObj != null) MonoBehaviour.Destroy(niceSpriteObj);

            CircularSprite mySprite = new CircularSprite(true);
            mySprite.Init(summonBtn, 0.88f);
            mySprite.SetAtlasTexture(iconAtlas, iconName);
        }
        else
        {
            DEBUG.LogError("ActiveConfig->Icon is null");
        }
    }
}


public class BattlePveInfoWindow : tWindow
{

}

public class PveBattleWindow : tWindow
{
    public override void Open(object param)
    {
        GameObject obj = GameCommon.FindUI("back_ground");
        if (obj != null)
            obj.SetActive(false);

		DataCenter.OpenWindow("BATTLE_PLAYER_WINDOW");
		DataCenter.OpenWindow("BATTLE_PLAYER_EXP_WINDOW");       
		//DataCenter.OpenWindow("BATTLE_BOSS_HP_WINDOW");
		//DataCenter.OpenWindow("BATTLE_AUTO_FIGHT_BUTTON");
		DataCenter.OpenWindow("BATTLE_SKILL_WINDOW");
		DataCenter.OpenWindow("PVE_TOP_RIGHT_WINDOW");

        DataCenter.OpenWindow("MASK_OPERATE_WINDOW");

        DataCenter.OpenWindow("PET_ATTACK_BOSS_WINDOW"); // 宠物强制跟随窗口

        if (MiniMap.Self!=null)
		{
			MiniMap.Self.InitData();
		}
    }
}


public class BuffGroup
{
    private UIGridContainer mContainer;
    private List<BuffData> mBuffs = new List<BuffData>();
    private int mMaxCount = 10;

    public BuffGroup(UIGridContainer container, int maxCount)
    {
        mContainer = container;
        mMaxCount = maxCount;
    }

    public void StartBuff(int iAffectConfig)
    {
        BuffData buff = GetBuffDataByIndex(iAffectConfig);
        if (buff != null && buff.IsVisible())
        {
            buff.Restart();
        }
        else if (mBuffs.Count < mMaxCount)
        {
            buff = new BuffData(this, iAffectConfig);

            if(buff.IsVisible())
                buff.Start();
        }
    }

    public void ClearBuff()
    {
        foreach (BuffData buff in mBuffs)
        {
            buff.FinishCD();
            MonoBehaviour.DestroyImmediate(buff.mObj);
        }
        mBuffs.Clear();
        mContainer.Reposition();
    }

    public void AddBuff(BuffData buff)
    {
        mBuffs.Add(buff);
        buff.Attach(CreateBuffObject(buff));
        mContainer.Reposition();
    }

    public void RemoveBuff(BuffData buff)
    {
        mBuffs.Remove(buff);
        MonoBehaviour.DestroyImmediate(buff.mObj);
        mContainer.Reposition();
    }

    public void RemoveBuff(int configIndex)
    {
        BuffData data = GetBuffDataByIndex(configIndex);

        if (data != null)
            RemoveBuff(data);
    }

    private BuffData GetBuffDataByIndex(int configIndex)
    {
        foreach (BuffData buffData in mBuffs)
        {
            if (buffData.mConfig == configIndex)
            {
                return buffData;
            }
        }
        return null;
    }

    private GameObject CreateBuffObject(BuffData buff)
    {
        float affectTime = (float)TableCommon.GetObjectFromAffectBuffer(buff.mConfig, "TIME");
        GameObject c = MonoBehaviour.Instantiate(mContainer.controlTemplate) as GameObject;
        c.name += "_" + (affectTime > 0.0001f ? "1" : "0");
        c.transform.parent = mContainer.transform;
        c.transform.localScale = Vector3.one;
        c.transform.localPosition = Vector3.zero;
        c.SetActive(true);
        return c;
    }
}

public class BuffData
{
    private BuffGroup mGroup;   
    private TM_BuffCD mBuffCD;
    private UISprite mCD;

    public GameObject mObj { get; private set; }
    public int mConfig { get; private set; }

    public BuffData(BuffGroup group, int config)
    {
        mGroup = group;
        mConfig = config;
    }

    public void Attach(GameObject obj)
    {
        mObj = obj;
        mCD = GameCommon.FindComponent<UISprite>(obj, "BufferCD");
        SetIcon();
    }

    public void Start()
    {
        mGroup.AddBuff(this);

        if (mBuffCD != null)
            mBuffCD.Finish();

        StartCD();
    }

    public void Restart()
    {
        if (mBuffCD != null && !mBuffCD.GetFinished())
        {
            mBuffCD.Restart();
        }
    }

    public void Remove()
    {
        mGroup.RemoveBuff(this);
    }

    public void Finish()
    {
        FinishCD();
        Remove();
    }

    public void SetFillAmount(float amount)
    {
        mCD.fillAmount = amount;
    }

    public void FinishCD()
    {
        if (mBuffCD != null && !mBuffCD.GetFinished())
        {
            mBuffCD.Finish();
        }
    }

    public bool IsVisible()
    {
        string atlasName = TableCommon.GetStringFromAffectBuffer(mConfig, "BUFFER_ATLAS_NAME");
        string spriteName = TableCommon.GetStringFromAffectBuffer(mConfig, "BUFFER_SPRITE_NAME");
        return atlasName != "" && spriteName != "";
    }

    private void StartCD()
    {
        BuffInfo info = BuffGlobal.GetInfo(mConfig);

        if(info != null && info.time > 0.001f)
        {
            TM_BuffCD cdEvt = EventCenter.Start("TM_BuffCD") as TM_BuffCD;
            cdEvt.mBuff = this;
            cdEvt.mAffectTime = info.time;
            cdEvt.mHideCD = info.hideCD;
            cdEvt.DoEvent();
            mBuffCD = cdEvt;
        }
        else 
        {
            SetFillAmount(0f);
        }
    }

    private void SetIcon()
    {
        UISprite icon = GameCommon.FindComponent<UISprite>(mObj, "Background");
        GameCommon.SetBufferIcon(icon, mConfig);
    }
}

public class TM_BuffCD : CEvent
{
    public ValueAnimation mValueAnim = new ValueAnimation();
    public BuffData mBuff;
    public float mAffectTime = 1f;
    public bool mHideCD = false;

    public override bool _DoEvent()
    {
        mValueAnim.Start(0f, 1f, mAffectTime);
        mBuff.SetFillAmount(mHideCD ? 0f : 1f);
        StartUpdate();
        return true;
    }

    public override bool Update(float dt)
    {
        float nowTime;
        bool bEnd = mValueAnim.Update(dt, out nowTime);
        if (bEnd)
        {
            Finish();
            mBuff.Remove();
        }

        mBuff.SetFillAmount(mHideCD ? 0f : 1f - nowTime);
        return !bEnd;
    }

    public void Restart()
    {
        mValueAnim.Start(0f, 1f, mAffectTime);
    }
}


public class Button_skip_battle : CEvent
{
    public override bool _DoEvent()
    {
        if (MainProcess.mStage != null)
        {
            MainProcess.mStage.SkipBattle();
        }

        return true;
    }
}