using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DataTable;


public class NetLogicData : tLogicData
{
    public string vs;
    public string pv;
    public int pi;
    public int ret;
}

public class tLogicData : NiceData
{
    public virtual void Init() { }
    public virtual void Init(string strName) { }
    public override void set(string indexName, object objVal)
    {
        base.set(indexName, objVal);

        onChange(indexName, objVal);
    }
    
    public virtual void RemoveAll()
    {
        string[] removeList = new string[mDataMap.Count];
        int x = 0;
#if NICEDATA_USE_INDEX_ID
            foreach (KeyValuePair<int, object> v in mDataMap)
#else
        foreach (KeyValuePair<string, object> v in mDataMap)
#endif
        {
            tLogicData logicData = v.Value as tLogicData;
            if (logicData != null)
            {
                removeList[x++] = v.Key;
            }
        }

        foreach (string key in removeList)
        {
            if (key != null && key != "")
                remove(key);
        }

        mDataMap.Clear();
    }

    public virtual void onChange(string keyIndex, object objVal) { }
    public virtual void onRemove() { }

    public virtual void registerData(string keyIndex, tLogicData data) 
    {
        data.Init();
        data.Init(keyIndex);
        set(keyIndex, data);
    }

    public override bool remove(string keyIndex)
    {
        tLogicData d = getData(keyIndex);
        if (d != null)
        {
            d.onRemove();

            d.RemoveAll();

            return base.remove(keyIndex);
        }
        return false;
    }

    public virtual tLogicData getData(string index) 
    {
        object obj;
        if (getData(index, out obj))
        {
            return obj as tLogicData;
        }
        Logic.EventCenter.Log(LOG_LEVEL.WARN, "No exist logic data > " + index);
        return null; 
    }

    public virtual bool setData(string dataIndex, string keyIndex, object val)
    {
        tLogicData d = getData(dataIndex);
        if (d != null)
        {
            d.set(keyIndex, val);
            return true;
        }

        return false;
    }
}


public class DataCenter : tLogicData
{
	static public DataCenter Self = new DataCenter();

	static public NiceTable mActiveConfigTable;
	static public NiceTable mMonsterObject;
    static public NiceTable mSkillConfigTable;
	static public NiceTable mMotionSoundTable;
    static public NiceTable mStageTable;
	static public NiceTable mCharacterLevelExpTable;
	static public NiceTable mPetLevelExpTable;
	static public NiceTable mElementTable;
    static public NiceTable mAffectBuffer;
	static public NiceTable mStoneTypeIcon;
	static public NiceTable mStrengthenConsume;
	static public NiceTable mEvolutionConsume;
	static public NiceTable mGroupIDConfig;
    static public NiceTable mIndianaDraw;  //mGroupIDConfig的按等级分段表
    static public NiceTable mModelTable;
	static public NiceTable mAttackState;
    static public NiceTable mPetOfPredestined;
    static public NiceTable mTaskConfig;
    static public NiceTable mWindowConfig;
    static public NiceTable mItemIcon;
    // shop
    static public NiceTable mShopSlotBase;

	static public NiceTable mItemConfig;
    static public NiceTable mBossConfig;

	static public NiceTable mMailBorad;
    static public int[] mPVPBattleList;

	static public NiceTable mPumpingConfig;
	static public NiceTable mMallShopConfig;

    // role equip
    static public NiceTable mRoleEquipConfig;
    static public NiceTable mEquipAttachAttributeConfig;
	static public NiceTable mEquipAttributeIconConfig;
	static public NiceTable mSetEquipConfig;
	static public NiceTable mEquipStrengthCostConfig;
	static public NiceTable mFateConfig;
    static public NiceTable mGlobalConfig;
    static public NiceTable mStageBirth;
    static public NiceTable mBoard;
	static public NiceTable mStringList;
    static public NiceTable mRoleStarLevel;
    static public NiceTable mPowerUp;
    static public NiceTable mRoleUIConfig;
	static public NiceTable mSelPetUIConfig;
    static public NiceTable mDialog;
	static public NiceTable mDailySignEvent;
	static public NiceTable mVipListConfig;
    static public NiceTable mAnnouncementConfig;
    static public NiceTable mStagePoint;
    static public NiceTable mStageFather;               //> 章节表
    static public NiceTable mStageBonus;
	static public NiceTable mPetType;
    static public NiceTable mEventList;
    static public NiceTable mEventConfig;
	static public NiceTable mPvpAttributeConfig;
	static public NiceTable mPvpRankAwardsConfig;
	static public NiceTable mFirstName;
	static public NiceTable mEffectSound;
	static public NiceTable mTacticalFormation;
	static public NiceTable mGuildShopConfig;
	static public NiceTable mGuildDonationConfig;
	static public NiceTable mGuildTechConfig;
	static public NiceTable mBeStrongerConfig;
	static public NiceTable mHelpTipsConfig;
    static public NiceTable mEventShow;
	static public NiceTable mFirstPayGiftBagConfig;
	static public NiceTable mEndlessDifficult;
//	static public NiceTable mBoardVersionConfig;
	static public NiceTable mActiveLevelListConfig;
	static public NiceTable mActiveTakeBreakConfig;
	static public NiceTable mActiveLuckyGuyConfig;
	static public NiceTable mConsumeConfig;
	static public NiceTable mPlayerLevelConfig;
	static public NiceTable mFragment;
    static public NiceTable mStageStar;
	static public NiceTable mRoleSkinConfig;
	static public NiceTable mBanedTextConfig;
    static public NiceTable mMaterialConfig;
    static public NiceTable mMaterialFragment;
    static public NiceTable mMaterialPrompt;
    static public NiceTable mMaterialSynthesis;
    static public NiceTable mScriptBase;
    static public NiceTable mTips;
    static public NiceTable mStaticConfig;
	static public NiceTable mPetDecompose;
	static public NiceTable mOperateEventConfig;
	static public NiceTable mAwardConfig;
	static public NiceTable mActiveRankConfig;
	static public NiceTable mActiveTaskConfig;
    static public NiceTable mRelateConfig;
	static public NiceTable mBreakLevelConfig;
	static public NiceTable mBreakBuffConfig;
	static public NiceTable mBreakAttribute;
	static public NiceTable mSkillCost;

    static public NiceTable mPetRecoverConfig;
    static public NiceTable mEquipRecoverConfig;
    static public NiceTable mEquipRefineLvConfig;
    static public NiceTable mEquipRefineStoneConfig;
    static public NiceTable mMagicEquipLvConfig;
    static public NiceTable mMagicEquipRefineConfig;
	static public NiceTable mPointStarConfig;

	static public NiceTable mFeatEventConfig;       //天魔活动
    static public NiceTable mFeatAwardConfig;       //天魔奖励

    static public NiceTable mClimbingTowerConfig;       //群魔乱舞
    static public NiceTable mClimbingConsumeConfig;     //群魔乱舞重置消耗

    static public NiceTable mFragmentAdminConfig;       //碎片管理
    static public NiceTable mEquipComposeConfig;        //法宝碎片合成

    static public NiceTable mFairylandConfig;           //符灵探险
    static public NiceTable mFairylandCostConfig;       //符灵探险花费
    static public NiceTable mFairylandDescConfig;       //符灵探险描述

    static public NiceTable mRobotConfig;           //机器人配置

    static public NiceTable mGuildCreated;
    static public NiceTable mWorshipConfig;
    static public NiceTable mWorshipSchedule;


    static public NiceTable mPvpRankAwardConfig;
    static public NiceTable mPvpLoot;

    static public NiceTable mLanguageConfig;
    static public NiceTable mAchieveConfig;
    static public NiceTable mScoreConfig;
    static public NiceTable mPetPostionLevel;
    static public NiceTable mStageLootGroupIDConfig;

	static public NiceTable mEnergyEvent;
    static public NiceTable mBeginnerConfig;
    static public NiceTable mCamera;
	static public NiceTable mResourceGainConfig;
	static public NiceTable mGainFunctionConfig;
	static public NiceTable mFunctionConfig;
    static public NiceTable mShooterParkConfig;
    static public NiceTable mLuckCardConfig;
    static public NiceTable mHelpListConfig;
    //static public NiceTable mRechargeEventConfig;   //> 充值送礼配表
    //static public NiceTable mCostEventConfig;       //> 累计消费配表
    static public NiceTable mFirstRechargeConfig;     //> 首充礼包配表
    static public NiceTable mWeekGiftEventConfig;     //> VIP周礼包配表 

    static public NiceTable mLocalPushConfig;   // 本地推送表
	static public NiceTable mHDlogin;
	static public NiceTable mHdrevelry;

    static public NiceTable mSceneBuff;
    static public NiceTable mEnergyCost;
    static public NiceTable mResetCost;

    static public NiceTable mQualityConfig; // 品质配表
    static public NiceTable mRetCode;   // server 错误码
    static public NiceTable mGuildBoss; //公会副本
    static public NiceTable mGuildBossPrice;  //公会副本
    static public NiceTable mDailyStageConfig;

	static public NiceTable mFundEvent;
	static public NiceTable mSevenDayLoginEvent;
    static public NiceTable mRankActivity;   //排名活动奖励
    static public NiceTable mLimitTimeSale;  //限时抢购
    static public NiceTable mOpenTime;      //活动时间
    static public NiceTable mPowerEnergy;   //> 金币和经验公式的系数表
    static public NiceTable mTipIconConfig; //> 折扣配表
    static public NiceTable mSingleRechargeEvent;   //单充福利
    static public NiceTable mFlashSaleEvent; //限时抢购new
	static public NiceTable mKingdomDescribe; 

    static public NiceTable mSpiritSendConfig;    // 精力赠送

    //by chenliang
    //begin

    public static NiceTable mChargeConfig;
    public static NiceTable mConduit;
    public static Dictionary<int, DataRecord> mDicSuitEquipTid = new Dictionary<int, DataRecord>();     //套装数据，以装备Tid为键值
    public static NiceTable mSufferingTriggerConfig;        //历练快捷入口

    //end

    static public NiceTable mBattleProve;   // 战斗验证表
    static public NiceTable mFailGuide;     // 战斗失败引导配置

    static public NiceTable mIosCheck;   //Ios审核表 
    static public IEnumerable<DataTable.DataRecord> allIosCheck;

	static public NiceTable mAgreementTable;
	static public NiceTable mEquipPostion;
    static public NiceTable mStageTask;
	static public NiceTable mStrengMaster;

    static public NiceTable mDefaultRole;//GM创建账号工具 默认角色信息配置
    static public NiceTable mWindowPoint; //界面识别点

	static public NiceTable mMainUiNotice;
	static public NiceTable mFightUiNotice;
    static public NiceTable mFightUi;
    static public NiceTable mEffect;

    static public NiceTable mSoundControl;

    public void LoadConfigDataFromFile()
    {
        //by chenliang
        //begin

        if (mTips != null)
            return;

        //end
#if UNITY_EDITOR || UNITY_STANDALONE
        TableManager.Self.LoadResouceConfig("Tips", "Config/StaticConfig/Tips.csv", LOAD_MODE.ANIS);
        TableManager.Self.LoadResouceConfig("StaticConfig", "Config/StaticConfig/StaticConfig.csv", LOAD_MODE.ANIS);
#else        
        TableManager.Self.LoadResouceConfig("Tips", "Config/StaticConfig/Tips.csv", LOAD_MODE.BYTES_RES);
        TableManager.Self.LoadResouceConfig("StaticConfig", "Config/StaticConfig/StaticConfig.csv", LOAD_MODE.BYTES_RES);        
#endif
        mTips = TableManager.GetTable("Tips");
        mStaticConfig = TableManager.GetTable("StaticConfig");
		UnityEngine.Debug.Log ("jiazaibiaoge99999=========" + mTips);
		DEBUG.Log ("jiazaibiaoge9999=========" + mTips);
    }

	public void LoadConfigData()
	{
		UnityEngine.Debug.Log ("jiazaibiaoge22222=========");
		DEBUG.Log ("jiazaibiaoge222222=========");
		//TableManager.Self.LoadConfig("Resources/Config/ConfigTable.csv", LOAD_MODE.ANIS);
        mGlobalConfig = TableManager.GetTable("Global");
		mActiveConfigTable = TableManager.GetTable("ActiveObject"); 
		mMonsterObject = TableManager.GetTable("MonsterObject");
        mSkillConfigTable = TableManager.GetTable("Skill");
        mMotionSoundTable = TableManager.GetTable("MotionSound");
        mStageTable = TableManager.GetTable("Stage");
		mCharacterLevelExpTable = TableManager.GetTable("CharacterLevelExp");
		mPetLevelExpTable = TableManager.GetTable("PetLevelExp");
		mElementTable = TableManager.GetTable("Element");
        mAffectBuffer = TableManager.GetTable("AffectBuffer");
		mStoneTypeIcon = TableManager.GetTable("StoneTypeIcon");
		mStrengthenConsume = TableManager.GetTable("StrengthenConsume");
		mEvolutionConsume = TableManager.GetTable("EvolutionConsume");
		mGroupIDConfig = TableManager.GetTable("GroupIDConfig");
        mIndianaDraw = TableManager.GetTable("IndianaDraw");
        mModelTable = TableManager.GetTable("ModelConfig");
        mAttackState = TableManager.GetTable("AttackState");
        mPetOfPredestined = TableManager.GetTable("PetOfPredestined");
        mTaskConfig = TableManager.GetTable("TaskConfig");
		mBreakAttribute = TableManager.GetTable ("BreakAttribute");
        mWindowConfig = TableManager.GetTable("WindowConfig");

        mItemIcon = TableManager.GetTable("ItemIcon");

        // shop
        mShopSlotBase = TableManager.GetTable("ShopSlotBase");

        mItemConfig = TableManager.GetTable("ItemConfig");
        mBossConfig = TableManager.GetTable("BossConfig");	

		mMailBorad = TableManager.GetTable ("Mailborad");

        // role equip
        mRoleEquipConfig = TableManager.GetTable("RoleEquipConfig");
        mEquipAttachAttributeConfig = TableManager.GetTable("EquipAttachAttributeConfig");
		mEquipAttributeIconConfig = TableManager.GetTable("EquipAttributeIconConfig");
		mEquipStrengthCostConfig = TableManager.GetTable("EquipStrengthCostConfig");
		mFateConfig = TableManager.GetTable("FateConfig");
		mSetEquipConfig = TableManager.GetTable("SetEquipConfig");

        mStageBirth = TableManager.GetTable("StageBirth");
        mBoard = TableManager.GetTable("Board");

		mStringList = TableManager.GetTable("StringList");
        mRoleStarLevel = TableManager.GetTable("RoleStarLevel");
        mPowerUp = TableManager.GetTable("PowerUp");
        mRoleUIConfig = TableManager.GetTable("RoleUIConfig");
		mSelPetUIConfig = TableManager.GetTable("SelPetUIConfig");
        mDialog = TableManager.GetTable("Dialog");
		mDailySignEvent = TableManager.GetTable ("DailySignEvent");
		mVipListConfig = TableManager.GetTable ("VipListConfig");
        mAnnouncementConfig = TableManager.GetTable("AnnouncementConfig");
        mStagePoint = TableManager.GetTable("StagePoint");
        mStageFather = TableManager.GetTable("StageFather");
        mStageBonus = TableManager.GetTable("StageBonus");
		mPetType = TableManager.GetTable("PetType");
        mEventList = TableManager.GetTable("EventList");
        mEventConfig = TableManager.GetTable("EventConfig");
		mPvpAttributeConfig = TableManager.GetTable("PvpAttributeConfig");
		mPvpRankAwardsConfig = TableManager.GetTable("PvpRankAwardsConfig");
		mFirstName = TableManager.GetTable("FirstName");
		mEffectSound = TableManager.GetTable("EffectSound");
		mTacticalFormation = TableManager.GetTable("TacticalFormation");
		mGuildShopConfig = TableManager.GetTable ("GuildShopConfig");
		mGuildDonationConfig = TableManager.GetTable ("GuildDonation");
		mGuildTechConfig = TableManager.GetTable ("GuildTech");
		mBeStrongerConfig = TableManager.GetTable ("BeStronger");
		mHelpTipsConfig = TableManager.GetTable ("HelpTipsConfig");
        mEventShow = TableManager.GetTable("EventShow");
		mFirstPayGiftBagConfig = TableManager.GetTable ("FirstPayGiftConfig");
		//mBoardVersionConfig = TableManager.GetTable ("BoardVersionConfig");
		mEndlessDifficult = TableManager.GetTable ("EndlessDifficultConfig");
		mActiveLevelListConfig = TableManager.GetTable ("ActiveLevelListConfig");
		mActiveTakeBreakConfig = TableManager.GetTable ("ActiveTakeBreakConfig");
		mActiveLuckyGuyConfig = TableManager.GetTable ("ActiveLuckyGuyConfig");
		mConsumeConfig = TableManager.GetTable ("ConsumeConfig");
		mPlayerLevelConfig = TableManager.GetTable ("PlayerLevelConfig");
        mFragment = TableManager.GetTable("Fragment");
        mStageStar = TableManager.GetTable("StageStar");
        mRoleSkinConfig = TableManager.GetTable("RoleSkinConfig");
		mBanedTextConfig = TableManager.GetTable("BanedTextConfig");
        mMaterialConfig = TableManager.GetTable("MaterialConfig");
        mMaterialFragment = TableManager.GetTable("MaterialFragment");
        mMaterialPrompt = TableManager.GetTable("MaterialPrompt");
        mMaterialSynthesis = TableManager.GetTable("MaterialSynthesis");
        mScriptBase = TableManager.GetTable("ScriptBase");
		mPetDecompose = TableManager.GetTable("Decompose");
		mOperateEventConfig = TableManager.GetTable ("OperateEvent");
		mAwardConfig = TableManager.GetTable ("AwardConfig");
		mActiveRankConfig = TableManager.GetTable ("RankEventConfig");
		mActiveTaskConfig = TableManager.GetTable ("TaskEventConfig");
		mBreakLevelConfig = TableManager.GetTable ("BreakLevelConfig");
		mBreakBuffConfig = TableManager.GetTable ("BreakBuff");
        mRelateConfig = TableManager.GetTable("RelateConfig");
		mSkillCost = TableManager.GetTable("SkillCost");
		mMallShopConfig = TableManager.GetTable("MallShopConfig");

        mPetRecoverConfig = TableManager.GetTable("PetRecoverConfig");

        mEquipRecoverConfig = TableManager.GetTable("EquipRecoverConfig");
        mEquipRefineLvConfig = TableManager.GetTable("EquipRefineLvConfig");
        mEquipRefineStoneConfig = TableManager.GetTable("EquipRefineStoneConfig");

		mFeatEventConfig = TableManager.GetTable("FeatEventConfig");
        mFeatAwardConfig = TableManager.GetTable("FeatAwardConfig");

        mClimbingTowerConfig = TableManager.GetTable("ClimbingTower");
        mClimbingConsumeConfig = TableManager.GetTable("ClimbingConsume");

        mFragmentAdminConfig = TableManager.GetTable("FragmentAdmin");
        mEquipComposeConfig = TableManager.GetTable("EquipCompose");

        mFairylandConfig = TableManager.GetTable("Fairylad");
        mFairylandCostConfig = TableManager.GetTable("FairyladCost");
        mFairylandDescConfig = TableManager.GetTable("FairyladDesc");

        mRobotConfig = TableManager.GetTable("RobotConfig");

        mMagicEquipLvConfig = TableManager.GetTable("MagicEquipLvConfig");
        mMagicEquipRefineConfig = TableManager.GetTable("MagicEquipRefineConfig");
		mPointStarConfig = TableManager.GetTable("PointStarConfig");

        mGuildCreated = TableManager.GetTable("GuildCreated");
        mWorshipConfig = TableManager.GetTable("WorshipConfig");
        mWorshipSchedule = TableManager.GetTable("WorshipSchedule");

        mPvpRankAwardConfig = TableManager.GetTable("PvpRankAwardConfig");
        mPvpLoot = TableManager.GetTable("PvpLoot");

        mAchieveConfig = TableManager.GetTable("AchieveConfig");
        mScoreConfig = TableManager.GetTable("ScoreConfig");
        mPetPostionLevel = TableManager.GetTable("PetPostionLevel");
        mStageLootGroupIDConfig = TableManager.GetTable("StageLootGroupIDConfig");
		mEnergyEvent = TableManager.GetTable("EnergyEvent");

        mBeginnerConfig = TableManager.GetTable("BeginnerConfig");
        mCamera = TableManager.GetTable("Camera");

		mResourceGainConfig = TableManager.GetTable ("ResourceGainConfig");
		mGainFunctionConfig = TableManager.GetTable ("GainFunctionConfig");
		mFunctionConfig = TableManager.GetTable ("FunctionConfig");

		mHDlogin = TableManager.GetTable ("HDlogin");
		mHdrevelry =TableManager.GetTable ("Hdrevelry");

        mLocalPushConfig = TableManager.GetTable("LocalPushConfig");

        mSpiritSendConfig = TableManager.GetTable("PowerReword");
        mSoundControl = TableManager.GetTable("SoundManage");

        //by chenliang
        //begin

        mChargeConfig = TableManager.GetTable("ChargeConfig");
        mConduit = TableManager.GetTable("Conduit");

        mPumpingConfig = TableManager.GetTable("PumpingConfig");
        foreach (KeyValuePair<int, DataRecord> tmpPair in mSetEquipConfig.GetAllRecord())
        {
            DataRecord tmpRecord = tmpPair.Value;
            for (int i = 0, count = 4; i < count; i++)
            {
                object tmpObjEquipTid = tmpRecord.getObject("SET_EQUIP_" + i.ToString());
                if (tmpObjEquipTid == null)
                    continue;
                int tmpEquipTid = (int)tmpObjEquipTid;
                mDicSuitEquipTid[tmpEquipTid] = tmpRecord;
            }
        }
        mSufferingTriggerConfig = TableManager.GetTable("SufferingTrigger");

        //end

        //added by xuke
        mShooterParkConfig = TableManager.GetTable("Hdshooter");
        mLuckCardConfig = TableManager.GetTable("LuckCard");
        mHelpListConfig = TableManager.GetTable("HelpList");
        mResetCost = TableManager.GetTable("ResetCost");
        //mRechargeEventConfig = TableManager.GetTable("RechargeEvent");
        //mCostEventConfig = TableManager.GetTable("CostEvent");
        mFirstRechargeConfig = TableManager.GetTable("FirstRechargeEvent");
        mPowerEnergy = TableManager.GetTable("PowerEnergy");
        mTipIconConfig = TableManager.GetTable("TipiconConfig");
        mWeekGiftEventConfig = TableManager.GetTable("WeekGiftEvent");
        //end

        mSceneBuff = TableManager.GetTable("SceneBuff");
        mEnergyCost = TableManager.GetTable("EnergyCost");

        mQualityConfig = TableManager.GetTable("QualityConfig");
        mRetCode = TableManager.GetTable("RetCode");
		mFundEvent = TableManager.GetTable ("FundEvent");
		mSevenDayLoginEvent = TableManager.GetTable ("SevenDayLoginEvent");
        mRankActivity = TableManager.GetTable("RankActivity");
        mGuildBoss = TableManager.GetTable("GuildBoss");
        mGuildBossPrice = TableManager.GetTable("GuildBossPrice");
        mDailyStageConfig = TableManager.GetTable("DailyStageConfig");
        mLimitTimeSale = TableManager.GetTable("LimitTimeSale");
        mOpenTime = TableManager.GetTable("OpenTime");
        mBattleProve = TableManager.GetTable("BattleProve");
        mFailGuide = TableManager.GetTable("FailGuide");
        mSingleRechargeEvent = TableManager.GetTable("SingleRechargeEvent");
        mFlashSaleEvent = TableManager.GetTable("FlashSaleEvent");
		mKingdomDescribe = TableManager.GetTable("KingdomDescribe");

        mIosCheck = TableManager.GetTable("IosCheck");
		mAgreementTable = TableManager.GetTable ("Agreement");
		mEquipPostion = TableManager.GetTable ("EquipPostion");
        mStageTask = TableManager.GetTable("StageTask");
		mStrengMaster = TableManager.GetTable ("StrengMaster");

        mDefaultRole = TableManager.GetTable("DefaultRole");
		mMainUiNotice = TableManager.GetTable ("MainUiNotice");
        mWindowPoint = TableManager.GetTable("WindowPoint");
		mFightUiNotice = TableManager.GetTable ("FightUiNotice");
        mFightUi = TableManager.GetTable("FightUi");
        mEffect = TableManager.GetTable("Effect");
        CommonParam.InitConfig();

        allIosCheck = DataCenter.mIosCheck.Records();

        int x = 0;
		foreach (KeyValuePair<int, DataRecord> v in mStageTable.GetAllRecord())
		{
            if (v.Value.get("TYPE") == "PVP6")
            {
                ++x;
            }
		}
        if (x <= 0)
        {
            Logic.EventCenter.Log(LOG_LEVEL.ERROR, "Not config any PVP6 battle stage");
        }
        else
        {
            mPVPBattleList = new int[x];
            int i = 0;
            foreach (KeyValuePair<int, DataRecord> v in mStageTable.GetAllRecord())
            {
                if (v.Value.get("TYPE") == "PVP6")
                {
                    mPVPBattleList[i++] = v.Key;
                }
            }
        }
		
	}

	public void InitResetGameData()
	{
        //by chenliang
        //begin

//		CommonParam.ClientVer = DataCenter.mGlobalConfig.GetData ("CLIENT_VERSION_NUMBER", "VALUE");
//----------------
        HotUpdateLog.IsTempLog = (string)DataCenter.mGlobalConfig.GetData("HOT_UPDATE_LOG_TEMP", "VALUE") == "YES";
        CommonParam.IsUseMVSkipTip = ((string)DataCenter.mGlobalConfig.GetData("IS_USE_MV_SKIP_TIP", "VALUE")) == "YES";

        //end
        bool bLog = (string)DataCenter.mGlobalConfig.GetData("LOG", "VALUE") == "YES";
        if (!bLog)
            DEBUG.LogWarning("Now close write log, may open GlobalConfig.csv > LOG > YES");

        CommonParam.NeedLog = bLog;
        CommonParam.LogOnScreen = (string)DataCenter.mGlobalConfig.GetData("LOG_ON_SCREEN", "VALUE") == "YES";
        CommonParam.LogOnConsole = (string)DataCenter.mGlobalConfig.GetData("LOG_ON_CONSOLE", "VALUE") == "YES";
        CommonParam.PingLogOnScreen = (string)DataCenter.mGlobalConfig.GetData("PING_LOG_ON_SCREEN", "VALUE") == "YES";
        ScrollWorldMapWindow.MAP_COUNT = (int)DataCenter.mGlobalConfig.GetData("WORLD_MAP_NUMBER", "VALUE");
#if UNITY_IPHONE || UNITY_ANDROID
        CommonParam.UseSDK = (string)DataCenter.mGlobalConfig.GetData("USE_SDK", "VALUE") == "YES";
#else
        CommonParam.UseSDK = false;
#endif

        string strSpeedX2 = DataCenter.mGlobalConfig.GetData("SPEED_X2", "VALUE");

        if (string.IsNullOrEmpty(strSpeedX2) || !float.TryParse(strSpeedX2, out CommonParam.speedX2))
        {
            CommonParam.speedX2 = 1.5f;
        }

        string strSpeedX3 = DataCenter.mGlobalConfig.GetData("SPEED_X3", "VALUE");

        if (string.IsNullOrEmpty(strSpeedX3) || !float.TryParse(strSpeedX3, out CommonParam.speedX3))
        {
            CommonParam.speedX3 = 2f;
        }

        string strSpeedX2OpenLevel = DataCenter.mGlobalConfig.GetData("SPEED_X2_LEVEL", "VALUE");

        if (string.IsNullOrEmpty(strSpeedX2OpenLevel) || !int.TryParse(strSpeedX2OpenLevel, out CommonParam.speedX2OpenLevel))
        {
            CommonParam.speedX2OpenLevel = 5;
        }

        string strSpeedX3OpenLevel = DataCenter.mGlobalConfig.GetData("SPEED_X3_LEVEL", "VALUE");

        if (string.IsNullOrEmpty(strSpeedX3OpenLevel) || !int.TryParse(strSpeedX3OpenLevel, out CommonParam.speedX3OpenLevel))
        {
            CommonParam.speedX3OpenLevel = 25;
        }

        string strSpeedX2OpenVIP = DataCenter.mGlobalConfig.GetData("SPEED_X2_VIP", "VALUE");

        if (string.IsNullOrEmpty(strSpeedX2OpenVIP) || !int.TryParse(strSpeedX2OpenVIP, out CommonParam.speedX2OpenVIP))
        {
            CommonParam.speedX2OpenVIP = 1;
        }

        string strSpeedX3OpenVIP = DataCenter.mGlobalConfig.GetData("SPEED_X3_VIP", "VALUE");

        if (string.IsNullOrEmpty(strSpeedX3OpenVIP) || !int.TryParse(strSpeedX3OpenVIP, out CommonParam.speedX3OpenVIP))
        {
            CommonParam.speedX3OpenVIP = 1;
        }

        DataRecord record;

        record = TableCommon.FindRecord(mFightUi, x => x["NAME"] == "AUTO_BUTTON");
        CommonParam.btnAutoBattleOpenLevel = record == null ? 2 : record["LEVEL"];

        record = TableCommon.FindRecord(mFightUi, x => x["NAME"] == "SPEED_BUTTON");
        CommonParam.btnSpeedUpOpenLevel = record == null ? 3 : record["LEVEL"];

        record = TableCommon.FindRecord(mFightUi, x => x["NAME"] == "FOLLOW_BUTTON");
        CommonParam.btnFollowOpenLevel = record == null ? 4 : record["LEVEL"];

		set ( "BATTLE", new tLogicData() );

        foreach (KeyValuePair<int, DataRecord> reVal in mWindowConfig.GetAllRecord())
        {
            DataRecord re = reVal.Value;
            string winName = re.get("WINDOW_NAME");
            string fabName = re.get("WIN_PREFAB"
			                        );
            string scriptName = re.get("SCRIPT");

            if (scriptName != "")
            {
                tWindow win = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(scriptName) as tWindow;
                if (win != null)
                {
                    win.mWinName = winName;
                    win.mWinPrefabName = fabName;
                    win.mAnchor = re.get("ANCHOR");
                    RegisterData(winName, win);
                }
                else
                {
                    Logic.EventCenter.Log(LOG_LEVEL.ERROR, "No exist script>" + scriptName + " of " + winName);
                }
            }
        }

        LuaInstance.Preload();
        BuffGlobal.Init();
        SkillGlobal.Init();
	}

    static public void Set(string key, object value)
    {
        Self.set(key, value);
    }
    
    
    static public Data Get(string key)
    {
        return Self.get(key);
    }

    static public bool SetData(string dataIndex, string keyIndex, object val)
    {
        return Self.setData(dataIndex, keyIndex, val);
    }

    static public tLogicData GetData(string dataIndex)
    {
        return Self.getData(dataIndex);
    }

    static public Data GetData(string dataIndex, string valueKey)
    {
        tLogicData data = Self.getData(dataIndex);
        if (data != null)
            return data.get(valueKey);
        Logic.EventCenter.Log(LOG_LEVEL.WARN, "no exist logic data >" + dataIndex);
        return new Data();
    }

    static public void RegisterData(string key, tLogicData data)
    {
        Self.registerData(key, data);
    }

    static public bool Remove(string dataIndex)
    {
        return Self.remove(dataIndex);
    }


    static public void CloseBackWindow() {
        CloseWindow(UIWindowString.common_back);
    }
    static public void OpenBackWindow(string selfWindowName,string backSpriteName,Action action=null,int width=184) {
        tLogicData backWindow=Self.getData("COMMON_BACK");
        tWindow backWinData=GetData(UIWindowString.common_back) as tWindow;
        if(backWinData.mGameObjUI==null) {
            DataRecord backRecord=mWindowConfig.GetRecord(UIWindowString.common_back);
            string backPrefabName=backRecord.getData("WIN_PREFAB");
            var backBtn=GameCommon.LoadAndIntanciateUIPrefabs(backPrefabName,"TopAnchor");
            backBtn.name=backPrefabName;
            backBtn.SendMessage("CreateUI",UIWindowString.common_back,SendMessageOptions.DontRequireReceiver);
            //tWindow backWinData=GetData(UIWindowString.common_back) as tWindow;
            //if(backWinData!=null) {
                backWinData.mWinName=UIWindowString.common_back;
                backWinData.mGameObjUI=backBtn;
                backWinData.Close();
            //}
        }
        if(backWindow==null){
            //BackWindowState.isFirst=false;
        }
        //if(BackWindowState.backBtn!=null) GameObject.DontDestroyOnLoad(BackWindowState.backBtn);
        //tWindow _backWinData=GetData(UIWindowString.common_back) as tWindow;
        /*之后设定sprite*/
        //tWindow _backWinData=GetData(UIWindowString.common_back) as tWindow;

        GameCommon.FindComponent<UISprite>(backWinData.mGameObjUI,"Sprite").spriteName=backSpriteName;
        GameCommon.FindComponent<UISprite>(backWinData.mGameObjUI,"Sprite").width=width;
        GameCommon.FindComponent<UIButtonEvent>(backWinData.mGameObjUI,"backBtn").AddAction(() => {
            DEBUG.Log(selfWindowName);
            CloseWindow(selfWindowName);
			//MainUIScript.Self.ShowMainBGUI();
            if(action!=null) action();
        });

        SetData(UIWindowString.common_back,"OPEN",true);
        //OpenWindow(UIWindowString.common_back);
    }


	static public bool OpenWindow(string windowName)
	{
		return OpenWindow(windowName, true);
	}

    static public bool OpenWindow(string windowName, object param)
    {
		DEBUG.Log ("OpenWindow ==== 111---" + windowName);
        tLogicData win = Self.getData(windowName);
		DEBUG.Log ("OpenWindow ==== 222---" + win);
        if (win == null)
        {
            DataRecord r = mWindowConfig.GetRecord(windowName);
			DEBUG.Log ("OpenWindow ==== 333---" + r);
            if (r != null)
            {
                string fabName = r.getData("WIN_PREFAB");
				DEBUG.Log ("OpenWindow ==== 444---" + fabName);
                GameObject uiObj = GameCommon.LoadAndIntanciateUIPrefabs(fabName, "CenterAnchor");
                if (uiObj!=null)
				{
                    uiObj.name = fabName;
					DEBUG.Log ("OpenWindow ==== 555---" + uiObj.name);
                    uiObj.SendMessage("CreateUI", windowName, SendMessageOptions.DontRequireReceiver);
					tWindow winData = GetData (windowName) as tWindow;
                    if (winData != null)
                    {
						DEBUG.Log ("OpenWindow ==== 666---" + windowName);
                        winData.mWinName = windowName;
                        winData.mGameObjUI = uiObj;
                        winData.Close();
                    }
				}
            }
			else
				DEBUG.LogError("Window no exist config > "+windowName);
        }
		DEBUG.Log ("OpenWindow ==== 777---");
        return SetData(windowName, "OPEN", param);
    }

    static public bool CloseWindow(string windowName)
    {
		return SetData(windowName, "CLOSE", true);
    }

    static public bool LogicActive(string logicDataName, string activeFunctionName, object param)
    {
        tLogicData win = Self.getData(logicDataName);
        if (win != null)
        {
            MethodInfo info = win.GetType().GetMethod(activeFunctionName);
            if (info != null)
            {
                object[] p = new object[1];
                p[0] = param;
				info.Invoke(win, p);

                return true;
            }
        }
        return false;
    }

	static public NiceTable GetActiveTable() { return mActiveConfigTable; }
    static public NiceTable GetSkillTable() { return mSkillConfigTable;  }
	static public NiceTable GetBufferTable() { return mAffectBuffer;  }
   //	static public NiceTable GetRoleLevelAttribute() { return Self.mRoleLevelAttribute;  }

 
    static public void OpenHelpMessageWindow(int iIndxe)
    {
		string showText = "";
		string titleName = "";

		showText = DataCenter.mHelpTipsConfig.GetRecord (iIndxe)["DESC"];
		titleName = DataCenter.mHelpTipsConfig.GetRecord (iIndxe)["NAME"];

		OpenWindow("MESSAGE_HELP_WINDOW", showText);
		SetData ("MESSAGE_HELP_WINDOW", "TITLE", titleName);
    }

	static public void CloseHelpMessageWindow()
	{
		SetData("MESSAGE_HELP_WINDOW", "CLOSE", true);
	}

	static public void CloseMessageWindow()
	{
		SetData("MESSAGE", "CLOSE", true);
	}
	
	static public void OpenMessageWindow(string showText)
	{
		OpenWindow("MESSAGE", showText);
	}

	static public void OpenMessageWindow(STRING_INDEX stringIndex)
	{
		int index = (int)stringIndex;
		string showText = "";
		DataRecord listInfo = mStringList.GetRecord (index);
		if(listInfo != null)
			showText = listInfo.get ("STRING_CN");
 
		OpenWindow("MESSAGE", showText);
	}

	static public void OpenMessageTipsWindow(STRING_INDEX stringIndex)
	{
		int index = (int)stringIndex;
		string showText = "";
		DataRecord listInfo = mStringList.GetRecord (index);
		if(listInfo != null)
			showText = listInfo.get ("STRING_CN");

		OpenWindow ("MESSAGE_TIPS_WINDOW", showText) ;
	}

	/// <summary>
	/// Opens the message window.
	/// </summary>
	/// <param name="stringIndex">String index.</param>

	//	only tips window
	static public void OnlyTipsLabelMessage(string showText)
	{
		GameObject objWindow = GameCommon.LoadAndIntanciateEffectPrefabs("Prefabs/UI/label_tips_window", "CenterAnchor");
		UILabel tipsLabel = GameCommon.FindObject (objWindow, "tips_label").GetComponent<UILabel>();

		tipsLabel.text = showText;
		
		GlobalModule.DoLater (() => GameObject.DestroyImmediate(objWindow), 1.3f);
	}
	static public void OnlyTipsLabelMessage(STRING_INDEX stringIndex)
	{
		GameObject objWindow = GameCommon.LoadAndIntanciateEffectPrefabs("Prefabs/UI/label_tips_window", "CenterAnchor");
		UILabel tipsLabel = GameCommon.FindObject (objWindow, "tips_label").GetComponent<UILabel>();
		int index = (int)stringIndex;
		DataRecord listInfo = mStringList.GetRecord (index);
		if(listInfo != null)
			tipsLabel.text = listInfo.get ("STRING_CN");
		
		GlobalModule.DoLater (() => GameObject.DestroyImmediate(objWindow), 1.3f);
	}
	static public void OnlyTipsLabelMessage(STRING_INDEX stringIndex, string addInfo)
	{
		GameObject objWindow = GameCommon.LoadAndIntanciateEffectPrefabs("Prefabs/UI/label_tips_window", "CenterAnchor");
		UILabel tipsLabel = GameCommon.FindObject (objWindow, "tips_label").GetComponent<UILabel>();
		int index = (int)stringIndex;
		DataRecord listInfo = mStringList.GetRecord (index);
		if(listInfo != null)
		{
			tipsLabel.text = listInfo.get ("STRING_CN");
			tipsLabel.text  = string.Format (tipsLabel.text, addInfo);
		}			
		
		GlobalModule.DoLater (() => GameObject.DestroyImmediate(objWindow), 1.3f);
	}

	//	error tips window
	static public void ErrorTipsLabelMessage(string showText)
	{
		GameObject objWindow = GameCommon.LoadAndIntanciateEffectPrefabs("Prefabs/UI/label_small_tips_window", "CenterAnchor");
		UILabel tipsLabel = GameCommon.FindObject (objWindow, "tips_label").GetComponent<UILabel>();
		tipsLabel.text = showText;		
		GlobalModule.DoLater (() => GameObject.DestroyImmediate(objWindow), 1.3f);
	}
	static public void ErrorTipsLabelMessage(STRING_INDEX stringIndex)
	{
		GameObject objWindow = GameCommon.LoadAndIntanciateEffectPrefabs("Prefabs/UI/label_small_tips_window", "CenterAnchor");
		UILabel tipsLabel = GameCommon.FindObject (objWindow, "tips_label").GetComponent<UILabel>();
		int index = (int)stringIndex;
		DataRecord listInfo = mStringList.GetRecord (index);
		if(listInfo != null)
			tipsLabel.text = listInfo.get ("STRING_CN");
		
		GlobalModule.DoLater (() => GameObject.DestroyImmediate(objWindow), 1.3f);
	}
	static public void ErrorTipsLabelMessage(STRING_INDEX stringIndex, string addInfo)
	{
		GameObject objWindow = GameCommon.LoadAndIntanciateEffectPrefabs("Prefabs/UI/label_small_tips_window", "CenterAnchor");
		UILabel tipsLabel = GameCommon.FindObject (objWindow, "tips_label").GetComponent<UILabel>();
		int index = (int)stringIndex;
		DataRecord listInfo = mStringList.GetRecord (index);
		if(listInfo != null)
		{
			tipsLabel.text = listInfo.get ("STRING_CN");
			tipsLabel.text  = string.Format (tipsLabel.text, addInfo);
		}
		
		GlobalModule.DoLater (() => GameObject.DestroyImmediate(objWindow), 1.3f);
	}
	//	show icon 1~8?
	static public void OpenAwardsTipsMessage(STRING_INDEX stringIndex)
	{
		GameObject objWindow = GameCommon.LoadAndIntanciateEffectPrefabs("Prefabs/UI/awards_tips_window", "CenterAnchor");
		UILabel tipsLabel = GameCommon.FindObject (objWindow, "tips_label").GetComponent<UILabel>();
		int index = (int)stringIndex;
		DataRecord listInfo = mStringList.GetRecord (index);
		if(listInfo != null)
			tipsLabel.text = listInfo.get ("STRING_CN");
		
		GlobalModule.DoLater (() => GameObject.DestroyImmediate(objWindow), 1.3f);
	}


	/// <summary>
	/// end
	/// </summary>


    static public void OpenMessageWindow(string showText, Action onClick)
    {
        OpenMessageWindow(showText);
        ObserverCenter.Add("MESSAGE_CLICK", onClick);
    }

    static public void OpenMessageWindow(STRING_INDEX stringIndex, Action onClick)
    {
        OpenMessageWindow(stringIndex);
        ObserverCenter.Add("MESSAGE_CLICK", onClick);
    }

    static public void OpenMessageWindow(STRING_INDEX stringIndex,int breakNeedLevel)
    {
        int index = (int)stringIndex;
        string showText = "";
        DataRecord listInfo = mStringList.GetRecord(index);
        if (listInfo != null)
        {
            showText = listInfo.get("STRING_CN");
           showText=string.Format(showText, breakNeedLevel); 
        }

        OpenWindow("MESSAGE", showText);
    }
	static public void OpenMessageWindow(STRING_INDEX stringIndex, string addInfo)
	{
		int index = (int)stringIndex;
		string showText = "";
		DataRecord listInfo = mStringList.GetRecord (index);
		if(listInfo != null)
		{
			showText = listInfo.get ("STRING_CN");
            //by chenliang
            //begin

//			addInfo = "'" + addInfo + "'";
//------------------
            //策划要求去除引号

            //end
			showText = string.Format (showText, addInfo);
		}
		
		OpenWindow("MESSAGE", showText);
	}

	static public void OpenMessageWindow(STRING_INDEX stringIndex, string addInfo, string addInfo1, bool bFadeOut)
	{
		int index = (int)stringIndex;
        string showText = "";
        DataRecord listInfo = mStringList.GetRecord(index);
		if(listInfo != null)
		{
			showText = listInfo.get ("STRING_CN");
			showText = string.Format (showText, addInfo, addInfo1);

			if(showText.Contains ("\\n"))
				showText = showText.Replace ("\\n", "\n");
		}
		
		OpenWindow("MESSAGE", showText);
		if(bFadeOut)
			SetData ("MESSAGE", "FADE_OUT_TIME", true);
	}

	static public void OpenMessageWindow(STRING_INDEX stringIndex, bool bFadeOut)
	{
		OpenMessageWindow(stringIndex);
		if(bFadeOut)
			SetData ("MESSAGE", "FADE_OUT_TIME", true);
	}

    static public void OpenMessageWindow(string showText, bool bFadeOut)
    {
        OpenMessageWindow(showText);
        if (bFadeOut)
            SetData("MESSAGE", "FADE_OUT_TIME", true);
    }

    static public void OpenTipPictureWindow(Tuple<ItemDataBase[],Action> tuple) {
        OpenWindow("TIPS_PICTURE_WINDOW",tuple);
        
    }

    static public string GetDescByStringIndex(STRING_INDEX strIndex)
    {
        int index = (int)strIndex;
        DataRecord listInfo = mStringList.GetRecord(index);
        if (listInfo != null) return listInfo.get("STRING_CN");
        return "";
    }

    /* 刷新双按钮确认框的按钮文字
     * leftText 左边按钮上的文字，初始值为"取消"
     * rightText 左边按钮上的文字，初始值为"确定"
     * */
    static public void RefreshMessageOkText(string leftText, string rightText)
    {
        string strText = leftText + "|" + rightText;
        DataCenter.SetData(UIWindowString.message_window, "WINDOW_BUTTON_TEXT_UPDATE", strText);
    }

    static public void RefreshMessageOkText(STRING_INDEX leftText, STRING_INDEX rightText)
    {
        string strText = TableCommon.getStringFromStringList(leftText) + "|" + TableCommon.getStringFromStringList(rightText);
        DataCenter.SetData(UIWindowString.message_window, "WINDOW_BUTTON_TEXT_UPDATE", strText);
    }
    
	static public void OpenMessageOkWindow(STRING_INDEX stringIndex, string addInfo, string windowName, bool isShowState = false)
	{
		int index = (int)stringIndex;
		string showText = "";
		DataRecord listInfo = mStringList.GetRecord (index);
		if(listInfo != null)
		{
			showText = listInfo.get ("STRING_CN");
            //by chenliang
            //begin

//			addInfo = "'" + addInfo + "'";
//---------------------
            //策划要求去除引号

            //end
			showText = string.Format (showText, addInfo);
		}
		DataCenter.Set("IS_SHOW_STATE", isShowState);
		OpenWindow("MESSAGE_WINDOW", showText);
		SetData("MESSAGE_WINDOW", "WINDOW_SEND", windowName);
		SetData("MESSAGE_WINDOW", "WINDOW_CLICK_CLOSE", "");
	}
	static public void OpenMessageOkWindow(STRING_INDEX stringIndex, string addInfo, Action onClickOk, bool isShowState = false)
    {
		OpenMessageOkWindow(stringIndex, addInfo, "", isShowState);
        ObserverCenter.Add("MESSAGE_OK", onClickOk);
    }
    static public void OpenMessageOkWindow(string shoText, Action onClickOk, bool notClose = false, bool isShowState = false) {
		DataCenter.Set("IS_SHOW_STATE", isShowState);
        OpenWindow("MESSAGE_WINDOW", shoText);
        SetData("MESSAGE_WINDOW", "WINDOW_SEND", "");
        ObserverCenter.Add("MESSAGE_OK", onClickOk);

        //点击不关闭
        SetData("MESSAGE_WINDOW", "WINDOW_NOT_CLOSE", notClose);
		SetData("MESSAGE_WINDOW", "WINDOW_CLICK_CLOSE", "");
    }
    static public void OpenMessageOkWindow(STRING_INDEX kStringIndex, Action onClick, bool notClose = false, bool isShowState = false) 
    {
        string _info = TableCommon.getStringFromStringList(STRING_INDEX.ERROR_SHOP_VIP_LEVEL_LOW);
        OpenMessageOkWindow(_info,onClick,notClose,isShowState);
    }

	static public void OpenMessageOkWindow(string shoText, Action onClickOk, Action onClickClose, bool notClose = false, bool isShowState = false) 
	{
		DataCenter.Set("IS_SHOW_STATE", isShowState);
		OpenWindow("MESSAGE_WINDOW", shoText);
		SetData("MESSAGE_WINDOW", "WINDOW_SEND", "");
		ObserverCenter.Add("MESSAGE_OK", onClickOk);
		ObserverCenter.Add("MESSAGE_CANCEL", onClickClose);
		SetData("MESSAGE_WINDOW", "WINDOW_NOT_CLOSE", notClose);
		SetData("MESSAGE_WINDOW", "WINDOW_CLICK_CLOSE", "");
	}

    static public void RefreshMessageOK(string showText, Action onClickOk)
    {
        DataCenter.SetData("MESSAGE_WINDOW", "WINDOW_NOT_CLOSE", false);
        DataCenter.SetData("MESSAGE_WINDOW", "WINDOW_TEXT_UPDATE", showText);
        DataCenter.RemoveKeyFromObserver();
        DataCenter.MessageOKaddAction(onClickOk);
    }
    static public void RemoveKeyFromObserver()
    {
        ObserverCenter.Clear("MESSAGE_OK");
    }

    static public void MessageOKaddAction(Action onClickOk)
    {
        ObserverCenter.Add("MESSAGE_OK", onClickOk);
    }

    static public void OpenMessageOkWindow(STRING_INDEX stringIndex, string addInfo, Action onClickOk, Action onClickCancel)
    {
        OpenMessageOkWindow(stringIndex, addInfo, "");
        ObserverCenter.Add("MESSAGE_OK", onClickOk);
        ObserverCenter.Add("MESSAGE_CANCEL", onClickCancel);
    }

	static public void CloseMessageOkWindow()
	{
		SetData("MESSAGE_WINDOW", "CLOSE", true);
	}
}