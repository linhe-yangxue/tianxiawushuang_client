using UnityEngine;
using System.Collections;
using Logic;
using DataTable;




public class MainUI : MonoBehaviour 
{
	static public MainUI Self;
	// Character
	public UIProgressBar mCharacterBloodBar;
	public UIProgressBar mCharacterEnergyBar;
	public UIProgressBar mCharacterExpBar;

	public UILabel mCharacterBloodCurLabel;
	public UILabel mCharacterBloodMaxLabel;
	public UILabel mCharacterLevelLabel;
	public UILabel mCharacterEnergyCurLabel;
	public UILabel mCharacterEnergyMaxLabel;
	public UILabel mCharacterExpCurLabel;
	public UILabel mCharacterExpMaxLabel;
	public UILabel mCharacterGoldLabel;
    public GameObject mCharacterBufferGroup;
	
	public UISprite mDoSkillCD_1;
	public UISprite mDoSkillCD_2;
	public UISprite mDoSkillCD_3;
	public UISprite mDoSkillCD_4;

	public UILabel mDoSkillCDTime_1;
	public UILabel mDoSkillCDTime_2;
	public UILabel mDoSkillCDTime_3;
	public UILabel mDoSkillCDTime_4;
    
	// Friend
    public GameObject mFriendInfoGroup;
    [HideInInspector]
    public UIGridContainer mFriendInfoGrid;
    public UIProgressBar[] mVecFriendBloodBar;

	// Boss
	public UIProgressBar mBossBloodBar;
	public GameObject mBossInfoUI;
	public UILabel mBossBloodCurLabel;
	public UILabel mBossBloodMaxLabel;

	//Auto_fight
	public GameObject mAuto_fightON;
	public GameObject mAuto_fightOFF;
    //public GameObject mAuto_fight_Effect;

	public UISprite mRoleIcon;



	// Use this for initialization
	void Awake(){
		mDoSkillCD_1.fillAmount = 0.0f;
		mDoSkillCD_2.fillAmount = 0.0f;
		mDoSkillCD_3.fillAmount = 0.0f;
		mDoSkillCD_4.fillAmount = 0.0f;
	}
	void Start () {
		Self = this;

        //mUICamera = GetComponentInChildren<Camera>();

        //CharInfo characterInfo = new CharInfo();
        //characterInfo.mBloodCurLabel = mCharacterBloodCurLabel;
        //characterInfo.mBloodMaxLabel = mCharacterBloodMaxLabel;
        //characterInfo.mEnergyCurLabel = mCharacterEnergyCurLabel;
        //characterInfo.mEnergyMaxLabel = mCharacterEnergyMaxLabel;
        //characterInfo.mExpCurLabel = mCharacterExpCurLabel;
        //characterInfo.mExpMaxLabel = mCharacterExpMaxLabel;
        //characterInfo.mLevelLabel = mCharacterLevelLabel;
        //characterInfo.mGoldLabel = mCharacterGoldLabel;
        //characterInfo.mBufferGroup = mCharacterBufferGroup;

        //??? No use this, use new battle_player_Info_window DataCenter.Self.registerData("BATTLE_PLAYER_WINDOW", characterInfo);

        InitPetInfoUI();

        mBossInfoUI.SetActive(false);

        if (MainProcess.mStage != null)
            MainProcess.mStage.Start();

		SetRoleIcon();
	}	

    //static void ReadySkillButtonData(tLogicData uiData, UISprite cdSprite, UILabel cdLable, int petUseIndex)
    //{
    //    SkillButtonData skill_1 = new SkillButtonData();
    //    skill_1.mCDSprite = cdSprite;
    //    skill_1.mCDLabel = cdLable;

    //    PetLogicData petData = DataCenter.GetData("PET_DATA") as PetLogicData;

    //    PetData pet = petData.GetPetDataByPos(petUseIndex);
    //    if (pet != null)
    //    {
    //        DataRecord config = DataCenter.mActiveConfigTable.GetRecord(pet.mModelIndex);
    //        if (config != null)
    //        {
    //            int petSkill = (int)config["PET_SKILL_1"];
                
    //            skill_1.set("SKILL_INDEX", petSkill);
    //            if (petSkill > 0)
    //            {
    //                cdSprite.fillAmount = 0;
    //                Self.SetSkillIcon (cdSprite, petSkill);
    //            }
    //            else
    //                cdSprite.fillAmount = 1;
    //        }
    //        else
    //            EventCenter.Log(LOG_LEVEL.ERROR, "No exist pet config >" + pet.mModelIndex.ToString());
    //    }
    //    else
    //    {
    //        skill_1.set("SKILL_INDEX", 0);
    //        cdSprite.fillAmount = 1;
    //    }

    //    string dataKey = "DoSkill_" + petUseIndex.ToString();
    //    uiData.registerData(dataKey, skill_1);
    //}

	void SetSkillIcon(UISprite cd_sprite, int skill_id)
	{
		DataRecord config = DataCenter.mSkillConfigTable.GetRecord(skill_id);
		if(config != null)
		{
			string icon = config["ICON"];
			GameObject parent = cd_sprite.transform.parent.gameObject;
			GameCommon.SetUISprite (parent, "Background", icon);
		}
		else 
		{
			DEBUG.LogError ("SkillConfig->Icon is null");
		}
	}

	void SetRoleIcon()
	{
		//RoleLogicData role_data = RoleLogicData.Self;
		int role_id = RoleLogicData.GetMainRole ().tid;
		DataRecord data = DataCenter.mActiveConfigTable.GetRecord (role_id);
        GameCommon.SetIcon(mRoleIcon, (string)data["HEAD_ATLAS_NAME"], (string)data["HEAD_SPRITE_NAME"]);
	}


    void OnDestroy()
    {
        OnChangeSceneBefore();
    }

    public void OnChangeSceneBefore()
    {
        DataCenter.Remove("SKILL_UI");
        //DataCenter.Remove("BATTLE_UI");
		DataCenter.Remove("BATTLE_PLAYER_WINDOW");
		DataCenter.Remove("BATTLE_PLAYER_EXP_WINDOW");
		//DataCenter.Remove("BOSS_HP");
		DataCenter.Remove("MINI_MAP");
    }



    public void InitPetInfoUI()
    {
        PetLogicData logic = DataCenter.GetData("PET_DATA") as PetLogicData;
        int iCount = 0;
        for (int i = 0; i < Character.msFriendsCount; i++)
        {
            if (logic.GetPetDataByPos(i+1) != null)
            {
                iCount++;
            }
        }

        mFriendInfoGrid = mFriendInfoGroup.transform.Find("Grid").GetComponent<UIGridContainer>();

        iCount = 0;
        for (int i = 0; i < mFriendInfoGrid.MaxCount; i++)
        {
            PetData petData = logic.GetPetDataByPos(i+1);
            GameObject curGridFirendObj = mFriendInfoGrid.controlList[i];
            curGridFirendObj.SetActive(false);
            if (petData != null)
            {
				FriendInfo friendInfo = new FriendInfo();
				friendInfo.mFriendBloodBar = mVecFriendBloodBar[iCount];
				friendInfo.mFriendInfoGrid = mFriendInfoGrid;
				friendInfo.mCurGridFirendObj = mFriendInfoGrid.controlList[iCount];
				Transform result = friendInfo.mCurGridFirendObj.transform.Find("BufferGroup");
				if (result!=null)
                	friendInfo.mBufferGroup = result.gameObject;
                DataCenter.RegisterData("FRIEND_INFO_" + iCount.ToString(), friendInfo);

				friendInfo.InitContextUI(iCount, petData);
                iCount++;
            }
        }
    }
}

public class RoleInfo : tLogicData
{
    public int m_iPosX0 = 0;
    public int m_iPosY0 = 0;
    public int m_iDX = 5;
    public int m_iDY = 5;
    public int m_iHeight = 40;
    public int m_iWidth = 40;
    public int m_iMaxColumn = 1;
	public int m_iMaxCount = 0;

    public GameObject mBufferGroup = null;
	public ActiveObject mOwner = null;
	public string mStrDataName = "";

    // buffer icon
    public void SetBufferIcon(ActiveObject owner, string strDataName, int iAffectConfig)
    {
		mOwner = owner;
		mStrDataName = strDataName;

		int iAllBufferCount = owner.mAffectList.Count;

        PetLogicData logic = DataCenter.GetData("PET_DATA") as PetLogicData;

        RoleBufferData roleBufferData = new RoleBufferData();

		// buffer icon
		GameObject bufferObj = LoadUI();
		if(bufferObj != null)
		{
			// buffer CD
			roleBufferData.mBufferUIObj = bufferObj;
			roleBufferData.mCDSprite = bufferObj.transform.Find("BufferCD").gameObject.GetComponent<UISprite>();
			roleBufferData.mCDSprite.fillAmount = 0;
			roleBufferData.mOwner = mOwner;
			
			registerData(mStrDataName + iAllBufferCount.ToString(), roleBufferData);
			
			// set buffer icon
			SetBufferIconUI(iAffectConfig);
			
			setData(mStrDataName + iAllBufferCount.ToString(), "BUFFER_INDEX", iAffectConfig);
			
			
			float affectTime = (float)TableCommon.GetObjectFromAffectBuffer(iAffectConfig, "TIME");
			if (affectTime > 0.0001f)
				roleBufferData.set("START_CD", affectTime);
		}        
    }

    public GameObject LoadUI()
    {
		int iUnfinishBufferCount = GetUnfinishBufferNum();
        GameObject obj = GameCommon.LoadUIPrefabs("buffer", mBufferGroup);
		SetBufferUIPos(iUnfinishBufferCount, obj);
        return obj;
    }

	public void SetBufferUIPos(int iIndex, GameObject obj)
	{
		if(obj != null)
		{
			if(iIndex <= m_iMaxCount)
				obj.transform.localPosition = GameCommon.GetPostion(m_iPosX0, m_iPosY0, m_iDX, m_iDY, m_iWidth, m_iHeight, iIndex - 1, m_iMaxColumn);
			else
				obj.transform.localPosition = new Vector3(3000, 1000, 0);
			obj.name = "Buffer_" + iIndex.ToString();
		}
	}

	public void RefreshAllBufferUIPos()
	{
		int iCount = 0;
		for (int i = 0; i < mOwner.mAffectList.Count; i++)
		{
			RoleBufferData roleBufferData = getData(mStrDataName + (i + 1).ToString()) as RoleBufferData;
			if(!roleBufferData.mbIsCDFinish)
			{
				iCount++;
				SetBufferUIPos(iCount, roleBufferData.mBufferUIObj);
			}
		}
	}

	public void HideAllBuffer()
	{
		if(mOwner == null)
			return;

		for (int i = 0; i < mOwner.mAffectList.Count; i++)
		{
			RoleBufferData roleBufferData = getData(mStrDataName + (i + 1).ToString()) as RoleBufferData;
			if(!roleBufferData.mbIsCDFinish)
			{
				roleBufferData.HideBuffer();
			}
		}
	}

	public void SetBufferIconUI(int iAffectConfig)
	{
		int iAllBufferCount = mOwner.mAffectList.Count;

		RoleBufferData roleBufferData = getData(mStrDataName + iAllBufferCount.ToString()) as RoleBufferData;
		GameObject bufferObj = roleBufferData.mBufferUIObj;
		bufferObj.SetActive(true);
		UISprite bufferIcon = bufferObj.transform.Find("Background").GetComponent<UISprite>();
		
		string strAtlasName = TableCommon.GetStringFromAffectBuffer(iAffectConfig, "BUFFER_ATLAS_NAME");
		string strSpriteName = TableCommon.GetStringFromAffectBuffer(iAffectConfig, "BUFFER_SPRITE_NAME");
		
		UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
		bufferIcon.atlas = tu;
		bufferIcon.spriteName = strSpriteName;
	}

	public int GetUnfinishBufferNum()
	{
		int iCount = 0;
		for (int i = 0; i < mOwner.mAffectList.Count; i++)
		{
			if (mOwner.mAffectList[i].GetFinished())
				continue;
			
			iCount++;
		}
		
		return iCount;
	}
}


public class CharInfo : RoleInfo
{
	public UILabel mBloodCurLabel = null;
	public UILabel mBloodMaxLabel = null;
	public UILabel mEnergyCurLabel = null;
	public UILabel mEnergyMaxLabel = null;
	public UILabel mExpCurLabel = null;
	public UILabel mExpMaxLabel = null;
	public UILabel mLevelLabel = null;
	public UILabel mGoldLabel = null;

	RoleData mRoleData = null;
	public CharInfo()
	{
		mRoleData = RoleLogicData.GetMainRole();
        m_iMaxColumn = 8;
		m_iMaxCount = 8;
	}


	public override void onChange(string keyIndex, object v) 
	{
		if (keyIndex=="HP")
		{
			int curHp = get("HP");
			int maxHp = get("MAX_HP");
			float percentage =  curHp / (float)maxHp;
			MainUI.Self.mCharacterBloodBar.value = percentage;
			mBloodCurLabel.text = curHp.ToString();
			mBloodMaxLabel.text = maxHp.ToString();
		}
		else if (keyIndex=="MP")
		{
			int curMp = get("MP");
			int maxMp = get("MAX_MP");
			float percentage =  curMp / (float)maxMp;
			MainUI.Self.mCharacterEnergyBar.value = percentage;
			mEnergyCurLabel.text = curMp.ToString();
			mEnergyMaxLabel.text = maxMp.ToString();
		}
		else if (keyIndex=="EXP")
		{
			int curExp = get("EXP");
			int maxExp = get("MAX_EXP");
			float percentage =  curExp / (float)maxExp;
			MainUI.Self.mCharacterExpBar.value = percentage;
			mExpCurLabel.text = curExp.ToString();
			mExpMaxLabel.text = maxExp.ToString();
		}
		else if (keyIndex=="GOLD")
		{
			int iGoldNum = get("GOLD");
			mGoldLabel.text = iGoldNum.ToString();
		}
		else if (keyIndex=="LEVEL")
		{
			mLevelLabel.text = get("LEVEL").ToString();
		}
	}
}

public class FriendInfo : RoleInfo
{
    public UILabel mBloodCurLabel = null;
    public UILabel mBloodMaxLabel = null;
    public UIProgressBar mFriendBloodBar = null;
	public UIGridContainer mFriendInfoGrid = null;
	public GameObject mCurGridFirendObj = null;

    public FriendInfo()
    {
        m_iMaxColumn = 3;
		m_iMaxCount = 6;
    }
    public override void onChange(string keyIndex, object v)
    {
        if (keyIndex == "HP")
        {
            int curHp = get("HP");
            int maxHp = get("MAX_HP");
            float percentage = curHp / (float)maxHp;
            mFriendBloodBar.value = percentage;
            mBloodCurLabel.text = curHp.ToString();
            mBloodMaxLabel.text = maxHp.ToString();
        }
        else if (keyIndex == "ADD_BUFFER")
        {

        }
    }

	public void InitContextUI(int iIndex, PetData petData)
	{
		if (iIndex < mFriendInfoGrid.MaxCount && petData != null)
		{
			mCurGridFirendObj.SetActive(true);

			// set pet icon
			SetFriendIcon(petData);
			
			// cur blood
			mBloodCurLabel = mCurGridFirendObj.transform.Find("BloodBar/BloodCurLabel").gameObject.GetComponent<UILabel>();
			mBloodCurLabel.text = "[ffc579]" + "1000";
			
			// max blood
			mBloodMaxLabel = mCurGridFirendObj.transform.Find("BloodBar/BloodMaxLabel").gameObject.GetComponent<UILabel>();
			mBloodMaxLabel.text = "[ffc579]" + "2000";

			set ("HP", 1000);
			set ("MAX_HP", 2000);
			onChange("HP", true);

		}
		
	}
	
	public void SetFriendIcon(PetData petData)
	{
		if(petData != null)
		{
			string strAtlasName = TableCommon.GetStringFromActiveCongfig(petData.tid, "HEAD_ATLAS_NAME");
			string strSpriteName = TableCommon.GetStringFromActiveCongfig(petData.tid, "HEAD_SPRITE_NAME");
			
			UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
			UISprite sprite = mCurGridFirendObj.GetComponent<UISprite>();
			sprite.atlas = tu;
			sprite.spriteName = strSpriteName;
//			sprite.MakePixelPerfect();
		}
	}
}

public class BossBar : tLogicData
{
	public UILabel mBloodCurLabel = null;
	public UILabel mBloodMaxLabel = null;

	
	public override void onChange(string keyIndex, object v) 
	{
		if (keyIndex=="HP")
        {
			int curHp = (int)v;

            int maxHp = get("MAX_HP");
            float percentage = curHp / (float)maxHp;
            MainUI.Self.mBossBloodBar.value = percentage;
            mBloodCurLabel.text = curHp.ToString();
            mBloodMaxLabel.text = maxHp.ToString();
		}
	}

}

public enum eSkillButtonEffect
{
    EleFair,
    EleWater,
    EleWood,
    EleGold,
    EleShadow,

    EleCDBegin,
    EleCDEnd,

    EleMax,
}

public class SkillButtonData : tLogicData
{
    public static readonly float SUMMON_CD = 10f;
    public static readonly float PET_LIFE_TIME = 10f;
    public static readonly float FRIEND_LIFE_TIME = 30f;

    public bool mbIsDoSkill = false;
    public GameObject mSummonObj = null;
    public GameObject mSkillObj = null;
    public UISprite mSummonCDSprite = null;
    public UISprite mCDSprite = null;
	public UILabel mCDLabel = null;
    public int mPetUsePos = 0;
    public DataRecord mSkillConfig;
    public SkillInfo mInfo;
    public tEvent mCDEvent;
    public tEvent mSummonCDEvent;
    public tEvent mPetLifeTimeEvent;
    public bool mActive = true;

    public tEvent mUpdateEvent;

    public ELEMENT_TYPE mSkillElement;

    public ParticleSystem[] mEffect = new ParticleSystem[(int)eSkillButtonEffect.EleMax];
    public GameObject mPetDieStateSprite;
    public UILabel mNeedMpText;
    public int mNeedMp = 0;
	
	public bool mbPetIsDead = false;
    public float mSummonCD = SUMMON_CD;
    public float mPetLifeTime = PET_LIFE_TIME;
    public float mPunishCD = 0f;

    public SkillButtonData()
    {
        set("CD_FINISH", true);

    }

    public void SetQuality(bool highQuality)
    {
        GameCommon.SetUIVisiable(mSkillObj, "low_quality_round", !highQuality);
        GameCommon.SetUIVisiable(mSkillObj, "high_quality_round", highQuality);
    }

    public void InitStart(GameObject mEffectMainObject)
    {
        mInfo = SkillGlobal.GetInfo(mSkillConfig);
        mNeedMp = mInfo.needMP;//mSkillConfig["NEED_MP"];
        mUpdateEvent = EventCenter.WaitUpdate(this, "RefreshCheckCanUse", null, 0.5f);

        mPetDieStateSprite = GameCommon.FindObject(mEffectMainObject, "pet_die_state_sprite");
        if (mPetDieStateSprite != null)
            mPetDieStateSprite.SetActive(false);

        PlayCanUseSkillEffect(null);
    }

    public void InitEffect(GameObject mEffectMainObject)
    {
        // 初始特效
        mEffect[(int)eSkillButtonEffect.EleFair] = GameCommon.FindComponent<ParticleSystem>(mEffectMainObject, "skillfiretime2");
        mEffect[(int)eSkillButtonEffect.EleGold] = GameCommon.FindComponent<ParticleSystem>(mEffectMainObject, "skillgoldtime2");
        mEffect[(int)eSkillButtonEffect.EleShadow] = GameCommon.FindComponent<ParticleSystem>(mEffectMainObject, "skillshadowtime2");
        mEffect[(int)eSkillButtonEffect.EleWater] = GameCommon.FindComponent<ParticleSystem>(mEffectMainObject, "skillwatertime2");
        mEffect[(int)eSkillButtonEffect.EleWood] = GameCommon.FindComponent<ParticleSystem>(mEffectMainObject, "skillwoodtime1");

        mEffect[(int)eSkillButtonEffect.EleCDBegin] = GameCommon.FindComponent<ParticleSystem>(mEffectMainObject, "skillhitf");
        mEffect[(int)eSkillButtonEffect.EleCDEnd] = GameCommon.FindComponent<ParticleSystem>(mEffectMainObject, "skill_cooldown");

        PlayEffect(eSkillButtonEffect.EleMax);
    }

    public void InitState(bool active)
    {
        if (active)
        {
            //if (mPetUsePos > 3)
            //{
            //    mbIsDoSkill = false;
            //    mSkillObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            //    mSkillObj.transform.localPosition = new Vector3(30, 30, 0);
            //    mSummonCDSprite.spriteName = "ui_mapmark1";
            //    mSummonCDSprite.fillAmount = 1f;
            //    set("SUMMON_CD_FINISH", true);
            //    set("CD_FINISH", true);

            //    if (mPetDieStateSprite != null)
            //        mPetDieStateSprite.SetActive(false);

            //    mCDSprite.fillAmount = 0f;
            //    mCDLabel.text = "";
            //    mNeedMpText.gameObject.SetActive(false);
            //}
            //else
            //{
                mbIsDoSkill = true;
                mSkillObj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                mSkillObj.transform.localPosition = new Vector3(0, 0, 0);
                set("CD_FINISH", true);
                mCDSprite.fillAmount = 0f;
                mCDLabel.text = "";
                mNeedMpText.gameObject.SetActive(true);
            //}

            mActive = true;
        }
        else
        {
            mSkillObj.transform.localScale = Vector3.one;
            mSkillObj.transform.localPosition = Vector3.zero;
            set("CD_FINISH", false);
            set("SKILL_INDEX", 0);

            if (mPetDieStateSprite != null)
                mPetDieStateSprite.SetActive(false);

            mCDSprite.fillAmount = 0f;
            mCDLabel.text = "";
            mNeedMpText.gameObject.SetActive(false);
            mActive = false;
        }
    }

    public void SetSummonCDRate(float rate)
    {
        if (mSummonCDSprite != null)
        {
            mSummonCDSprite.fillAmount = rate;
        }
    }

    public override void onChange(string keyIndex, object value)
    {
        if (keyIndex == "ON_CLICK_BUTTON")
        {
            if (mbIsDoSkill)
            {
                DoSkill();
            }
            else
            {
                SummonPet();
            }
        }
        if (mCDSprite != null && keyIndex == "START_CD")
        {
            StartSkillCD();
        }
        else if (mSummonCDSprite != null && keyIndex == "START_SUMMON_CD")
        {
            if (mActive)
            {
                StartSummonCD(value is float ? (float)value : 1f, false);
            }
        }
        else if (keyIndex == "SKILL_INDEX")
        {
            mSkillConfig = DataCenter.GetSkillTable().GetRecord((int)value);
            // change button icon
        }
        else if (keyIndex == "PET_DEAD")
        {
            if (value is bool && !(bool)value)
                return;

            mbPetIsDead = true;
            return; // 宠物死后仍可释放技能
            
            if (mCDSprite != null)
            {
                mCDSprite.fillAmount = 1;
                mCDLabel.text = "";
            }
            if (mCDEvent != null)
                mCDEvent.Finish();

            if (mPetLifeTimeEvent != null)
                mPetLifeTimeEvent.Finish();
            
            PlayEffect(eSkillButtonEffect.EleMax);
            set("CD_FINISH", false);

            //if (mPetDieStateSprite != null)
            //    mPetDieStateSprite.SetActive(true);

            if (mPetUsePos >= 0 && mPetUsePos <= 3)
            {
                //if (get("PET_LIFE_TIME_OVER"))
                //{
                //    StartSummonCD(1f, false);
                //}
                //else
                //{
                //    StartSummonCD(1f, true);
                //}
            }
            else
            {
                mSummonObj.transform.parent.gameObject.SetActive(false);
            }
        }
        else if (keyIndex == "DO_SKILL")
        {
            DoSkill();
        }
        else if (keyIndex == "SUMMON_PET")
        {
            SummonPet();
        }
        else if (keyIndex == "HIDE_BUTTON")
        {
            mSummonObj.transform.parent.gameObject.SetActive(false);
        }
        else if (keyIndex == "REMOVE_SKILL_CD")
        {
            if (mCDEvent != null && !mCDEvent.GetFinished())
            {
                (mCDEvent as TM_SkillButtonData).mTime.mLastTime = 0;
            }
        }
        else if (keyIndex == "REMOVE_SUMMON_CD")
        {
            if (mSummonCDEvent != null && !mSummonCDEvent.GetFinished())
            {
                (mSummonCDEvent as TM_SummonCD).mTime.mLastTime = 0;
            }
        }
    }

    private void StartSkillCD()
    {
        PlayEffect(eSkillButtonEffect.EleCDBegin);

        if (mCDEvent != null)
            mCDEvent.Finish();
        TM_SkillButtonData cdEvt = EventCenter.Start("TM_SkillButtonData") as TM_SkillButtonData;
        cdEvt.mSkillData = this;

        float cdTime = 1;
        if (mSkillConfig != null)
            cdTime = mInfo.skillCD;//mSkillConfig.getData("CD_TIME");

        if (Character.Self != null)
            cdTime = Character.Self.GetSkillCD(cdTime);

        cdEvt.mTime.Start(1, 0, (float)cdTime);
        cdEvt.DoEvent();
        mCDEvent = cdEvt;
    }

    private void StartSummonCD(float rate, bool withPunish)
    {
        set("SUMMON_CD_FINISH", false);
        set("CD_FINISH", true);
   
        if (mSummonCDEvent != null)
            mSummonCDEvent.Finish();

        TM_SummonCD cdEvt = EventCenter.Start("TM_SummonCD") as TM_SummonCD;
        cdEvt.mSkillData = this;
        float cdTime = withPunish ? mSummonCD + mPunishCD : mSummonCD;

        if (Character.Self != null)
            cdTime = Character.Self.GetSummonCD(cdTime);

        mSummonCDSprite.spriteName = withPunish ? "ui_mapmark2" : "ui_mapmark1";

        cdEvt.mTotalTime = cdTime;
        cdEvt.mRate = rate;     
        cdEvt.DoEvent();
        mSummonCDEvent = cdEvt;

        mbIsDoSkill = false;
        OnChangeToSummonPet();
    }

    private void DoSkill()
    {
        if (!get("CD_FINISH"))
            return;

        //int skillIndex = get("SKILL_INDEX");
        if (MainProcess.Self.mController != null)
        {
            MainProcess.Self.mController.OnClickSkill(this);
        }
        //OnBattleCustomerActionObserver.Instance.Notify("ClickSkillButton", this);

        //BaseObject enemy = Character.Self.GetCampEnemy();
        //
        //if (enemy == null)
        //    enemy = Character.Self.FindNearestEnemy();
        //
        //if (enemy == null && Character.Self.mAutoBattleAI != null)
        //    enemy = Character.Self.mAutoBattleAI.GetNextAttackMonster();
        //
        //if (enemy == null)
        //    return;
        //
        //Character.Self.SkillAttack(skillIndex, enemy, this);
    }

    private void SummonPet()
    {
        if (!get("SUMMON_CD_FINISH"))
            return;

        float cdTime = 0f;

        if (mPetUsePos >= 0 && mPetUsePos <= 3)
        {
            ++MainProcess.mStage.mPetSummonCount;
            Friend f = Character.Self.InitFriendPet(mPetUsePos - 1);
            cdTime = mPetLifeTime;
            //
            //if(f != null)
            //    cdTime = f.GetLifeTime(mPetLifeTime);
        }
        else if (PetLogicData.mFreindPetData != null)
        {
            Friend f = Character.Self.CreateFriendPet(PetLogicData.mFreindPetData);
            cdTime = FRIEND_LIFE_TIME;
            //
            //if (f != null)
            //    cdTime = f.GetLifeTime(FRIEND_LIFE_TIME);
        }
        else
        {
            return;
        }

        mbPetIsDead = false;
        
        if (mPetLifeTimeEvent != null)
            mPetLifeTimeEvent.Finish();

        TM_PetLifeTime evt = EventCenter.Start("TM_PetLifeTime") as TM_PetLifeTime;
        evt.mSkillData = this;
        evt.mPetUsePos = mPetUsePos;
        
        if (Character.Self != null)
            cdTime = Character.Self.GetSummonTime(cdTime);

        evt.mTotalTime = cdTime;
        evt.mTime.Start(1, 0, cdTime);
        evt.DoEvent();
        mPetLifeTimeEvent = evt;
        set("PET_DEAD", false);
        DataCenter.SetData("BATTLE_PLAYER_WINDOW", "PET_ALIVE", mPetUsePos);

        mbIsDoSkill = true;
        OnChangeToDoSkill();
    }

    private void OnChangeToSummonPet()
    {
        TweenPosition.Begin(mSkillObj, 0.2f, new Vector3(30, 30, 0));
        TweenScale.Begin(mSkillObj, 0.2f, new Vector3(0.5f, 0.5f, 0.5f));

        mSummonCDSprite.fillAmount = 1f;
        
        if (mPetDieStateSprite != null)
            mPetDieStateSprite.SetActive(false);

        mCDSprite.fillAmount = 0f;
        mCDLabel.text = "";
        mNeedMpText.gameObject.SetActive(false);
        PlayEffect(eSkillButtonEffect.EleMax);
    }

    private void OnChangeToDoSkill()
    {
        TweenPosition.Begin(mSkillObj, 0.2f, Vector3.zero);
        TweenScale.Begin(mSkillObj, 0.2f, new Vector3(0.9f, 0.9f, 0.9f));

        mNeedMpText.gameObject.SetActive(true);
        PlayCanUseSkillEffect(null);
    }

    public void PlayEffect(eSkillButtonEffect effectIndex)
    {
        for (int i=0; i<(int)eSkillButtonEffect.EleMax; ++i)
        {
            if (mEffect[i]!=null)
            {
                mEffect[i].gameObject.SetActive(i == (int)effectIndex);
                if (i==(int)effectIndex)
                {
                    mEffect[i].Stop(true);
                    mEffect[i].Play(true);
                }
            }
        }
    }

    public void PlayCanUseSkillEffect(object param)
    {
        if (mbIsDoSkill)
        {
            PlayEffect((eSkillButtonEffect)mSkillElement);
        }
    }

    public bool RefreshCheckCanUse(object param)
    {
        if (mNeedMpText != null && Character.Self!=null)
        {
            if (Character.Self.GetMp() > mNeedMp)
			{
                mNeedMpText.color = Color.white;
			}
            else
			{
                mNeedMpText.color = Color.red;
			}

            if (mCDLabel.text == "")// && !mbPetIsDead)
            {
                mCDSprite.fillAmount = Character.Self.CanDoSkill(mSkillConfig) ? 0f : 1f;
            }
        }

		return true;
    }

    public override void onRemove()
    {
        if (mCDEvent != null)
            mCDEvent.Finish();

        if (mUpdateEvent != null)
            mUpdateEvent.Finish();

        if (mSummonCDEvent != null)
            mSummonCDEvent.Finish();

        if (mPetLifeTimeEvent != null)
            mPetLifeTimeEvent.Finish();
    }
}

public class TM_SkillButtonData : CEvent
{
    public ValueAnimation mTime = new ValueAnimation();
    public SkillButtonData mSkillData;

	public TM_SkillButtonData()
	{

	}

    public override bool _DoEvent()
    {
		mSkillData.set("CD_FINISH", false);
        StartUpdate();
		return true;
    }

    public override bool Update(float dt)
    {        
        float nowTime;
        bool bEnd = mTime.Update(dt, out nowTime);
        if (bEnd)
        {
            mSkillData.set("CD_FINISH", true);

            mSkillData.PlayEffect(eSkillButtonEffect.EleCDEnd);
            EventCenter.WaitAction(mSkillData, "PlayCanUseSkillEffect", null, 0.5f);

            Finish();
        }
        
        if (mSkillData.mCDSprite!=null)
            mSkillData.mCDSprite.fillAmount = nowTime;
		if(mSkillData.mCDLabel != null)
			//mSkillData.mCDLabel.text = ((int)(nowTime * mSkillData.mSkillConfig.getData ("CD_TIME")) + 1).ToString ();
            mSkillData.mCDLabel.text = ((int)(nowTime * mSkillData.mInfo.skillCD) + 1).ToString();
		if(nowTime <= 0)
			mSkillData.mCDLabel.text = "";
        return !bEnd;
    }

}

public class TM_SummonCD : CEvent
{
    public float mTotalTime = 10f;
    public float mRate = 1f;
    public ValueAnimation mTime = new ValueAnimation();
    public SkillButtonData mSkillData;

    public override bool _DoEvent()
    {
        mTime.Start(mRate, 0, mTotalTime * mRate);
        mSkillData.set("SUMMON_CD_FINISH", false);
        StartUpdate();
        return true;
    }

    public override bool Update(float dt)
    {
        float nowTime;
        bool bEnd = mTime.Update(dt, out nowTime);

        if (bEnd)
        {
            mSkillData.set("SUMMON_CD_FINISH", true);
            mSkillData.mSummonCDSprite.fillAmount = 0f;
            mSkillData.mCDLabel.text = "";
            Finish();
            return false;
        }
        else
        {
            mSkillData.mSummonCDSprite.fillAmount = nowTime;
            mSkillData.mCDLabel.text = Mathf.CeilToInt(nowTime * mTotalTime).ToString();
            return true;
        }
    }
}

public class TM_PetLifeTime : CEvent
{
    public ValueAnimation mTime = new ValueAnimation();
    public SkillButtonData mSkillData;
    public int mPetUsePos = 1;
    public float mTotalTime = 1f;
    private tLogicData mBattlePlayerWindow;
    private string key = "";

    public override bool _DoEvent()
    {
        mBattlePlayerWindow = DataCenter.GetData("BATTLE_PLAYER_WINDOW");
        key = "SET_PET_LIFETIME_" + mPetUsePos;
        StartUpdate();
        return true;
    }

    public override bool Update(float dt)
    {
        Friend friend = null;

        if (Character.Self.mFriends[mPetUsePos - 1] != null)
        {
            friend = Character.Self.mFriends[mPetUsePos - 1] as Friend;
        }

        if (friend != null)
        {
            dt = dt * friend.GetLifeTimeSpeed(mTotalTime);
        }

        float nowTime;
        bool bEnd = mTime.Update(dt, out nowTime);
        
        if (MainProcess.mStage == null || MainProcess.mStage.mbBattleFinish)
        {
            Finish();
            return false;
        }

        if (bEnd)
        {
            if (friend != null)
            {
                friend.mbLifeTimeOver = true;
                friend.ChangeHp(-100000000);

                if (mPetUsePos > 3)
                {
                    mSkillData.set("HIDE_BUTTON", true);
                }
            }

            Finish();
        }
        else
        {
            mBattlePlayerWindow.set(key, nowTime);
        }

        return !bEnd;
    }
}

public class Vector3Data
{
    public Vector3 v;

    public Vector3Data(Vector3 vec)
    {
        v = vec;
    }
}

//public class UI_BattleData : tLogicData
//{
//    Win_WarResult mWarResultWindow = new Win_WarResult();

//    public UI_BattleData()
//    {
//        mWarResultWindow.Init();
//    }

//    public override void onChange(string keyIndex, object objVal)
//    {
//        switch (keyIndex)
//        {
//            case "OPEN":
//                {
//                    bool win = (bool)objVal;
//                    mWarResultWindow.Open(win);
//                }
//                break;

//            case "CLOSE":
//                {
//                    mWarResultWindow.Close();
//                }
//                break;

//            case "VISIBLE":
//                {
//                    mWarResultWindow.SetVisible();
//                }
//                break;
//            //case "AUTO_BATTLE":
//            //    {
//            //        bool bAuto = (bool)objVal;
//            //        if (bAuto)
//            //        {
//            //            MainUI.Self.mAuto_fightOFF.SetActive(false);
//            //            MainUI.Self.mAuto_fightON.SetActive(true);
//            //            //MainUI.Self.mAuto_fight_Effect.SetActive(true);
//            //        }
//            //        else
//            //        {
//            //            MainUI.Self.mAuto_fightOFF.SetActive(true);
//            //            MainUI.Self.mAuto_fightON.SetActive(false);
//            //            //MainUI.Self.mAuto_fight_Effect.SetActive(false);
//            //        }
//            //    }
//            //    break;

//        }
//    }
//}


public class Win_WarResult : tWindow
{
    public GameObject mWinObject;
    public GameObject mWinResult;
    public GameObject mLostResult;

    public GameObject mAgainAndExit;

    public override void Init()
    {
        mWinObject = GameObject.Find("WarResult");
        mWinResult = GameObject.Find("Victory");
        mLostResult = GameObject.Find("Failed");
        mAgainAndExit = GameObject.Find("AgainAndExit");

        Close();
    }
	public override bool IsOpen(){ return mWinObject.activeSelf; }
    public override void Open(object param)
    {
        mWinObject.SetActive(true);
        bool bWin = ((bool)param);
        mWinResult.SetActive(bWin);
        mLostResult.SetActive(!bWin);
        mAgainAndExit.SetActive(true);
    }

    public override void Close()
    {
		if (mWinObject!=null)
        	mWinObject.SetActive(false);

		if (mAgainAndExit!=null)
        	mAgainAndExit.SetActive(false);
    }

	public void SetVisible()
	{
		if(mWinObject.activeSelf)
		{
			Close();
		}
		else
		{
			Open (true);
		}
	}

}


public class RoleBufferData : tLogicData
{
    public GameObject mBufferUIObj = null;
    public UISprite mCDSprite = null;
    public DataRecord mBufferConfig;
    public tEvent mCDEvent;
    public ActiveObject mOwner = null;
    public bool mbIsCDFinish = false;

    public override void onChange(string keyIndex, object value)
    {
        if (mCDSprite != null && keyIndex == "START_CD")
        {
            if (mbIsCDFinish)
                return;

            if (mCDEvent != null)
                mCDEvent.Finish();
            TM_RoleBufferData cdEvt = EventCenter.Start("TM_RoleBufferData") as TM_RoleBufferData;
            cdEvt.mRoleBufferData = this;

            float cdTime = (float)value;
            if (mBufferConfig != null)
                mBufferConfig.getData("TIME");

            cdEvt.mTime.Start(0, 1, cdTime);
            cdEvt.DoEvent();
            mCDEvent = cdEvt;
        }
        else if (keyIndex == "BUFFER_INDEX")
        {
            mBufferConfig = DataCenter.GetBufferTable().GetRecord((int)value);
        }
        else if (keyIndex == "CD_FINISH")
        {
            mbIsCDFinish = (bool)value;
            if (mbIsCDFinish)
            {
                HideBuffer();
                Refresh();
            }
        }
        else if (keyIndex == "HIDE_BUFFER")
        {
            if (!mbIsCDFinish)
                mBufferUIObj.SetActive(false);
        }
    }

    public void Refresh()
    {
        mOwner.RefreshAllBufferUIPos();
    }

    public void HideBuffer()
    {
        mBufferUIObj.SetActive(false);
    }

    public override void onRemove()
    {
        if (mCDEvent != null)
            mCDEvent.Finish();

        if (mBufferUIObj != null)
        {
            Object.Destroy(mBufferUIObj);
        }
    }
}

public class TM_RoleBufferData : CEvent
{
    public ValueAnimation mTime = new ValueAnimation();
    public RoleBufferData mRoleBufferData;

    public override bool _DoEvent()
    {
        mRoleBufferData.set("CD_FINISH", false);
        StartUpdate();
        return true;
    }

    public override bool Update(float dt)
    {
        float nowTime;
        bool bEnd = mTime.Update(dt, out nowTime);
        if (bEnd)
        {
            Finish();
        }

        if (mRoleBufferData.mCDSprite != null)
            mRoleBufferData.mCDSprite.fillAmount = 1 - nowTime;

        return !bEnd;
    }

    public override void Finish()
    {
        base.Finish();
        mRoleBufferData.set("CD_FINISH", true);
    }
}