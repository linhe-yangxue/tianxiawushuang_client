using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using System.IO;
using System.Text;
using DataTable;
//-------------------------------------------------------------------------
public class RoleData : ActiveData
{
    public int mIndex;

    public override void Init()
    {
        base.Init();
        teamPos = 0;
        mMaxLevelNum = CommonParam.characterLevelLimit;
    }

    public virtual void AddExp(int dExp, bool inPVE = false)
    {
        if (level >= mMaxLevelNum)
        {
            level = mMaxLevelNum;
            exp = 0;

            return;
        }

        exp += dExp;

        int iMaxExp = GetMaxExp();
        if (exp >= iMaxExp)
        {
            LevelUp();
            if (!inPVE)
            {
                OnLevelUp();
            }      
            int iDExp = exp - iMaxExp;

            exp = 0;
            AddExp(iDExp);
        }
    }
    public void OnLevelUp()
    {
        DataCenter.OpenWindow("UPGRADE_WINDOW");
        tWindow t = DataCenter.GetData("UPGRADE_WINDOW") as tWindow;

        if (t != null && t.mGameObjUI != null)
        {
            MonoBehaviour.Destroy(t.mGameObjUI, 2f);
        }
        GlobalModule.DoLater(()=>
        {
            if (NewFuncOpenWindow.mbNeedBreak) 
            {
                NewFuncOpenWindow.mbNeedBreak = false;
                return;                
            }
          NewFuncOpenWindow.ShowGoToNewFuncWin(level);
        },NewFuncOpenWindow.mWaitTime);
    }

    public virtual void LevelUp()
    {
        if (level >= mMaxLevelNum)
        {
            return;
        }
        RoleLogicData.Self.preStamina = RoleLogicData.Self.stamina;
        RoleLogicData.Self.preSpirit = RoleLogicData.Self.spirit;
		//update tili stamina
		RoleLogicData.Self.AddStamina (TableCommon.GetRoleLABOR (level));
        RoleLogicData.Self.AddSpirit(TableCommon.GetRoleEnergy(level));

        level += 1;

        ChangeSpeedWhenLevelEnough(level);
        if (level >= mMaxLevelNum)
        {
            exp = 0;
        }
        if (this == RoleLogicData.Self.character && RoleLogicData.mOnMainRoleLevelUp != null)
        {
            RoleLogicData.mOnMainRoleLevelUp(level);
        }

		if (GuideManager.IsGuideFinished())
		{
			DataCenter.OpenWindow("PLAYER_LEVEL_UP_SHOW_WINDOW");
		}
		
		//update tili 
		GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_VITAKITY", true);
        GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_FRIEND_POINT",true);
    }

    private void ChangeSpeedWhenLevelEnough(int level)
    {
        string mtempKey = CommonParam.mUId + "MAXBATTLESPEED";
        if (level >= CommonParam.speedX3OpenLevel)
        {
            if (!PlayerPrefs.HasKey(mtempKey) || (PlayerPrefs.GetString(mtempKey) != "USED3" && PlayerPrefs.GetString(mtempKey) != "3"))
            {
                PlayerPrefs.SetString(mtempKey, 3.ToString());
                PlayerPrefs.Save();
            }
        }
        else if (level >= CommonParam.speedX2OpenLevel)
        {
            if (!PlayerPrefs.HasKey(mtempKey))
            {
                PlayerPrefs.SetString(mtempKey, 2.ToString());
                PlayerPrefs.Save();
            }
        }           
    }

    public virtual int GetMaxExp()
    {
        RoleData roleData = RoleLogicData.GetMainRole();
        return TableCommon.GetRoleMaxExp(roleData.level);
    }
}

//-------------------------------------------------------------------------
public class RoleLogicData : NetLogicData
{
    static public RoleLogicData Self = null;
    public static Action<int> mOnMainRoleLevelUp = null;    // 当主角升级时的回调

    //public int guideProgress = 0; // beginner guide state 新手引导状态
    public int mDBID;
    //by chenliang
    //begin

//     public int gold; // 银币
//     public int diamond; // 元宝
//     public int stamina; // 体力
//     public int mMaxStamina = 100;
//     public int mStaminaTime;
//     public int mStaminaMaxTime = 600;
//     public int spirit; // 精力
//     public int soulPoint; // 符魂
//     public int reputation; // 声望
//     public int prestige; // 威名
//     public int battleAchv; // 战功
//     public int unionContr;
//     public int beatDemonCard; // 降魔令
//     public int power = 0; // 战斗力
//     public UInt64 lastLoginTime; // 最后登陆时间
//     public int newPlayer; // 是否需要创角
//     public int vipLevel; // vip等级
//     public int guildId; // 公会id
//     public int mMaxSpirit = 30; // 精力上限 
//     public int mSoulPoint = 9999999; // 
//     public int mReputation = 9999999;
//     public int mPrestige = 9999999;
//     public int mBattleAchv = 9999999;
//     public int mUnionContr = 8000;
//     public int mBeatDemonCard = 8000;
// 	public int mHonorPoint; // 荣誉点
//     public int mMaxPetNum = 60; // 已弃用
//     public int mMaxRoleEquipNum = 60; // 已弃用
//     public int mMaxPetEquipNum = 60; // 已弃用
//     public int mVIPExp;
//     public string name; // 名字
//     public string mStrID;
//     public int mInviteNum;
//     public int mMailNum;
// 	public int iconIndex; // 头像
// 	public bool mbHaveNewTuJian;
//     public bool mbHaveAwardTuJian;
// 	public int mSweepNum;
// 	public int mResetNum;
// 	public int mLockNum;
// 	public int mLuckyGuyMultipleIndex;
// 	public int chaLevel;        //已废弃，使用charactor.level
// 	public int chaExp;          //已废弃，使用charactor.exp
// 	public int mMaxPlayerLevel = 30;
// 	public int mChatTime;
//--------------
    //将一部分值改为属性
    private int _gold = 0;
    public int gold // 银币
    {
        set
        {
            if (CommonParam.IsVerifyNumberValue)
                GameValueVerify.Instance.Update(__GetVerifyKey("ROLE_DATA_GOLD"), value);
            _gold = value;
        }
        get
        {
            if (CommonParam.IsVerifyNumberValue && !GameValueVerify.Instance.Verify(__GetVerifyKey("ROLE_DATA_GOLD"), _gold))
            {
                if (!GameValueVerify.Instance.HasKey(__GetVerifyKey("ROLE_DATA_GOLD")))
                    return 0;
                GameCommon.DataVerifyFailedHandle();
                return 0;
            }
            return _gold;
        }
    }
    private int _diamond = 0;
    public int diamond // 元宝
    {
        set
        {
            if (CommonParam.IsVerifyNumberValue)
                GameValueVerify.Instance.Update(__GetVerifyKey("ROLE_DATA_DIAMOND"), value);
            _diamond = value;
        }
        get
        {
            if (CommonParam.IsVerifyNumberValue && !GameValueVerify.Instance.Verify(__GetVerifyKey("ROLE_DATA_DIAMOND"), _diamond))
            {
                if (!GameValueVerify.Instance.HasKey("ROLE_DATA_DIAMOND"))
                    return 0;
                GameCommon.DataVerifyFailedHandle();
                return 0;
            }
            return _diamond;
        }
    }
    public int preStamina = 0;  //> 升级前的体力
    private int _stamina = 0;
    public int stamina // 体力
    {
        set
        {
            if (CommonParam.IsVerifyNumberValue)
                GameValueVerify.Instance.Update(__GetVerifyKey("ROLE_DATA_STAMINA"), value);
            _stamina = value;
        }
        get
        {
            if (CommonParam.IsVerifyNumberValue && !GameValueVerify.Instance.Verify(__GetVerifyKey("ROLE_DATA_STAMINA"), _stamina))
            {
                if (!GameValueVerify.Instance.HasKey(__GetVerifyKey("ROLE_DATA_STAMINA")))
                    return 0;
                GameCommon.DataVerifyFailedHandle();
                return 0;
            }
            return _stamina;
        }
    }
    public int mMaxStamina = 100;
    public int mStaminaTime;
    public int mStaminaMaxTime = 600;

    public int preSpirit = 0;   //> 升级前的精力
    private int _spirit = 0;
    public int spirit // 精力
    {
        set
        {
            if (CommonParam.IsVerifyNumberValue)
                GameValueVerify.Instance.Update(__GetVerifyKey("ROLE_DATA_SPIRIT"), value);
            _spirit = value;
        }
        get
        {
            if (CommonParam.IsVerifyNumberValue && !GameValueVerify.Instance.Verify(__GetVerifyKey("ROLE_DATA_SPIRIT"), _spirit))
            {
                if (!GameValueVerify.Instance.HasKey(__GetVerifyKey("ROLE_DATA_SPIRIT")))
                    return 0;
                GameCommon.DataVerifyFailedHandle();
                return 0;
            }
            return _spirit;
        }
    }
    public int soulPoint; // 符魂
    public int reputation; // 声望
    public int prestige; // 威名
    public int battleAchv; // 战功
    public int unionContr;
    public int beatDemonCard; // 降魔令
    public int power = 0; // 战斗力
    public UInt64 lastLoginTime; // 最后登陆时间
    public int newPlayer; // 是否需要创角
    public string guildId; // 公会id
    public int mMaxSpirit = 30; // 精力上限 
    public int mSoulPoint = CommonParam.soulPointLimit; // 
    public int mReputation = CommonParam.reputationLimit;
    public int mPrestige = CommonParam.prestigeLimit;
    public int mBattleAchv = CommonParam.battleAchvLimit;
    public int mUnionContr = CommonParam.unionContrLimit;
    public int mBeatDemonCard = CommonParam.beatDemonCardLimit;
    public int mHonorPoint; // 荣誉点
    public int mMaxPetNum = 60; // 已弃用
    public int mMaxRoleEquipNum = 60; // 已弃用
    public int mMaxPetEquipNum = 60; // 已弃用
    public int mVIPExp;
    public string name; // 名字
    public string mStrID;
    public int mInviteNum;
    public int mMailNum;
    public int iconIndex; // 头像
    public bool mbHaveNewTuJian;
    public bool mbHaveAwardTuJian;
    public int mSweepNum;
    public int mResetNum;
    public int mLockNum;
    public int mLuckyGuyMultipleIndex;
    public int chaLevel;        //已废弃，使用charactor.level
    public int chaExp;          //已废弃，使用charactor.exp
    public int mMaxPlayerLevel = 30;
    public int mChatTime;

    //end

    public RoleData character; // 角色
    public RoleData[] mRoleList;
    public List<int> skin;// 皮肤
    //by chenliang
    //begin

    public long staminaStamp;           //体力恢复开始时间
    public long spiritStamp;             //精力恢复开始时间
    public long beatDemonCardStamp;     //降魔令恢复开始时间

    private int _vipExp = 0;         //VIP经验
    public int vipExp
    {
        set
        {
            if (CommonParam.IsVerifyNumberValue)
                GameValueVerify.Instance.Update(__GetVerifyKey("ROLE_DATA_VIP_EXP"), value);
            _vipExp = value;

            __VIPLevelUp();
        }
        get
        {
            if (CommonParam.IsVerifyNumberValue && !GameValueVerify.Instance.Verify(__GetVerifyKey("ROLE_DATA_VIP_EXP"), _vipExp))
            {
                if (!GameValueVerify.Instance.HasKey(__GetVerifyKey("ROLE_DATA_VIP_EXP")))
                    return 0;
                GameCommon.DataVerifyFailedHandle();
                return 0;
            }
            return _vipExp;
        }
    }
    /// <summary>
    /// VIP升级
    /// </summary>
    private void __VIPLevelUp()
    {
        if (vipLevel >= VIPHelper.GetMaxVIPLevel())
            return;

        int tmpCurrMaxVIPExp = __GetCurrentMaxPlayerVIPExp();
        if (vipExp >= tmpCurrMaxVIPExp)
        {
            vipLevel += 1;
            ChangeSpeedWhenLevelEnough(vipLevel);                                               
            __VIPLevelUp();
        }
    }

    private void ChangeSpeedWhenLevelEnough(int vipLevel)
    {
        string mtempKey = CommonParam.mUId + "MAXBATTLESPEED";
        if (vipLevel >= CommonParam.speedX3OpenVIP)
        {
            if (!PlayerPrefs.HasKey(mtempKey) || (PlayerPrefs.GetString(mtempKey) != "USED3" && PlayerPrefs.GetString(mtempKey) != "3"))
            {
                PlayerPrefs.SetString(mtempKey, 3.ToString());
                PlayerPrefs.Save();
            }
        }
        else if (vipLevel >= CommonParam.speedX2OpenVIP)
        {
            if (!PlayerPrefs.HasKey(mtempKey))
            {
                PlayerPrefs.SetString(mtempKey, 2.ToString());
                PlayerPrefs.Save();
            }
        }
    }

    private int __GetCurrentMaxPlayerVIPExp()
    {
        int tmpNextVIPLevel = Mathf.Min(vipLevel + 1, VIPHelper.GetMaxVIPLevel());
        DataRecord tmpVIPRecord = GameCommon.GetVIPConfig(tmpNextVIPLevel);
        return ((int)tmpVIPRecord.getObject("CASHPAID"));
    }
    private int _vipLevel = 0;  // vip等级
    public int vipLevel
    {
        set
        {
            if (CommonParam.IsVerifyNumberValue)
                GameValueVerify.Instance.Update(__GetVerifyKey("ROLE_DATA_VIP_LEVEL"), value);
            if (_vipLevel != value)
            {
                NewShopWindow tmpWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
                if (tmpWin != null)
                    tmpWin.set("ALREADY_OPEN_VIP_SHOP", null);
            }
            _vipLevel = value;
        }
        get
        {
            if (CommonParam.IsVerifyNumberValue && !GameValueVerify.Instance.Verify(__GetVerifyKey("ROLE_DATA_VIP_LEVEL"), _vipLevel))
            {
                if (!GameValueVerify.Instance.HasKey(__GetVerifyKey("ROLE_DATA_VIP_LEVEL")))
                    return 0;
                GameCommon.DataVerifyFailedHandle();
                return 0;
            }
            return _vipLevel;
        }
    }
    private int _money = 0;     //玩家已充值数量
    /// <summary>
    /// 充值
    /// </summary>
    public int money
    {
        set
        {
            if (CommonParam.IsVerifyNumberValue)
                GameValueVerify.Instance.Update(__GetVerifyKey("ROLE_DATA_MONEY"), value);
            _money = value;
        }
        get
        {
            if (CommonParam.IsVerifyNumberValue && !GameValueVerify.Instance.Verify(__GetVerifyKey("ROLE_DATA_MONEY"), _money))
            {
                if (!GameValueVerify.Instance.HasKey(__GetVerifyKey("ROLE_DATA_MONEY")))
                    return 0;
                GameCommon.DataVerifyFailedHandle();
                return 0;
            }
            return _money;
        }
    }

    //end

	//added by xuke
	//public int MAX_ATTRIBUTE_NUM = 999999999;
	//end

    //by chenliang
    //begin

//     public RoleLogicData()
//     {
//         Self = this;
//     }
    private string _verifyKey = "";      //用于作为验证key
    public RoleLogicData()
    {
        Self = this;

        _verifyKey = GameValueVerify.Instance.CreateNewKey();
    }

    private string __GetVerifyKey(string verifyField)
    {
        return _verifyKey + verifyField;
    }

    //end

    static public RoleData GetMainRole()
    {
		if(Self != null)
        	return Self.character;
       
		return null;
    }

    public RoleData GetRole(int index)
    {
        if (index >= 0 && index < mRoleList.Length)
        {
            return mRoleList[index];
        }
        return null;
    }

    public bool ChangeMainRole(int index)
    {
        if (index < 0 || index >= mRoleList.Length || mRoleList[index] == null || mRoleList[index] == character)
        {
            return false;
        }
        character = mRoleList[index];
        return true;
    }

    public int GetRoleCount()
    {
        int iCount = 0;
        for (int i = iCount; i < mRoleList.Length; i++)
        {
            if (GetRole(i) != null)
                iCount++;
        }

        return iCount;
    }

    public virtual void AddStamina(int dStamina)
    {
        // 当前拥有体力可以超过上限
        //if (stamina >= mMaxStamina)
        //    return;

        stamina += dStamina;

        if (stamina < 0) {
			stamina = 0;
		} 
		else if (stamina > CommonParam.staminaLimit) 
		{
            stamina = CommonParam.staminaLimit;
		}

        //by chenliang
        //begin

//        // TODO [9/30/2015 LC]
//        if (stamina < mMaxStamina)
//        {
//            GlobalModule.Instance.StartUpdateStaminaTime();
//        }
//        else
//        {
//            GlobalModule.Instance.StopUpdateStaminaTime();
//        }
//----------------------------
        RoleInfoTimerManager.Instance.CheckAndStartRecover(ROLE_INFO_TIMER_TYPE.STAMINA, true);

        //end

		GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_VITAKITY", true);
//        GameCommon.SetWindowData("ShopWindow", "UPDATE_VITAKITY", true);
    }

    public string GetUpdateStaminaString()
    {
        return GameCommon.CheckShowHadNumUI(stamina, mMaxStamina) + "/" + mMaxStamina.ToString();

        // TODO [9/30/2015 LC]
        string strStamina = "";
        string strTime = "";
        if (stamina < mMaxStamina)
        {
            strStamina = stamina.ToString() + "/" + mMaxStamina.ToString();
            int iMin = mStaminaTime / 60;
            int iSec = mStaminaTime % 60;
            string strMin = iMin.ToString();
            string strSec = iSec.ToString();
            if (iMin < 10)
            {
                strMin = "0" + strMin;
            }
            if (iSec < 10)
            {
                strSec = "0" + strSec;
            }
            strTime = strMin + ":" + strSec;
        }
        else if (stamina == mMaxStamina)
        {
            strStamina = mMaxStamina.ToString() + "/" + mMaxStamina.ToString();
            strTime = "MAX";
        }
        else
        {
            string strExtend = "+" + (stamina - mMaxStamina).ToString();
            strStamina = mMaxStamina.ToString() + "/" + mMaxStamina.ToString() + strExtend;
        }
        return strStamina + " " + strTime;
    }

    public virtual void UpdateStaminaTime()
    {
        mStaminaTime = (mStaminaMaxTime + mStaminaTime - 1) % mStaminaMaxTime;
        if (mStaminaTime == 0)
        {
            AddStamina(1);
        }
        else
        {
			GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_VITAKITY", true);
        }
    }

    public string GetSpiritString()
    {
        return GameCommon.CheckShowHadNumUI(spirit, mMaxSpirit) + "/" + mMaxSpirit.ToString();
    }

    public virtual void AddGold(int dGold)
    {
        gold += dGold;

        if (gold < 0) 
		{
			gold = 0;
		} 
		else if (gold > CommonParam.goldLimit) 
		{
            gold = CommonParam.goldLimit;
		}

		GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_GOLD", true);
    }

    public virtual void AddDiamond(int dDiamond)
    {
        diamond += dDiamond;

        if (diamond < 0)
		{
			diamond = 0;
		}
		else if (diamond > CommonParam.diamondLimit) 
		{
            diamond = CommonParam.diamondLimit;
		}
		GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND", true);
    }

	public virtual void AddHonorPoint(int iHonorPoint)
	{
		mHonorPoint += iHonorPoint;
		
		if (mHonorPoint < 0)
		{
			mHonorPoint = 0;
		}
	}

	public virtual void AddMailNum(int mailNum)
	{
		mMailNum += mailNum;
		
		if (mMailNum < 0)
		{
			mMailNum = 0;
		}  
		
//		GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_ROLE_SELECT_SCENE", true);
		GameCommon.SetWindowData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_MAIL_MARK", true);
	}

    public virtual void AddSpirit(int dSpirit)
    {
        // 当前拥有精力可以超过上限
        //if (spirit >= mMaxSpirit)
        //    return;

        spirit += dSpirit;

        if (spirit < 0) 
		{
			spirit = 0;
		}
		else if (spirit > CommonParam.spiritLimit) 
		{
            spirit = CommonParam.spiritLimit;
		}
        //by chenliang
        //begin

        RoleInfoTimerManager.Instance.CheckAndStartRecover(ROLE_INFO_TIMER_TYPE.SPIRIT, true);

        //end
        GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_FRIEND_POINT", true);
    }

    public virtual void AddSoulPoint(int dSoulPoint)
    {
        soulPoint += dSoulPoint;

        if (soulPoint < 0)
        {
            soulPoint = 0;
        }
        else if (soulPoint >= mSoulPoint)
        {
            soulPoint = mSoulPoint;
        }
        //GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_FRIEND_POINT", true);
    }

    public virtual void AddReputation(int dReputation)
    {
        reputation += dReputation;

        if (reputation < 0)
        {
            reputation = 0;
        }
        else if (reputation >= mReputation)
        {
            reputation = mReputation;
        }
        //GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_FRIEND_POINT", true);
    }

    public virtual void AddPrestige(int dPrestige)
    {
        prestige += dPrestige;

        if (prestige < 0)
        {
            prestige = 0;
        }
        else if (prestige >= mPrestige)
        {
            prestige = mPrestige;
        }
        //GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_FRIEND_POINT", true);
    }

    public virtual void AddBattleAchv(int dBattleAchv)
    {
        battleAchv += dBattleAchv;

        if (battleAchv < 0)
        {
            battleAchv = 0;
        }
        else if (battleAchv >= mBattleAchv)
        {
            battleAchv = mBattleAchv;
        }
        //GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_FRIEND_POINT", true);
    }

    public virtual void AddUnionContr(int dUnionContr)
    {
        unionContr += dUnionContr;

        if (unionContr < 0)
        {
            unionContr = 0;
        }
        else if (unionContr >= mUnionContr)
        {
            unionContr = mUnionContr;
        }
        //GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_FRIEND_POINT", true);
    }

    public virtual void AddBeatDemonCard(int dBeatDemonCard)
    {
        beatDemonCard += dBeatDemonCard;

        if (beatDemonCard < 0)
        {
            beatDemonCard = 0;
        }
        // 允许超过上限
        //else if (beatDemonCard >= mBeatDemonCard)
        //{
        //    beatDemonCard = mBeatDemonCard;
        //}
        //GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_FRIEND_POINT", true);
        //by chenliang
        //begin

        RoleInfoTimerManager.Instance.CheckAndStartRecover(ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD, true);

        //end
    }

	public virtual void AddFunctionalProp(ITEM_TYPE type, int count)
	{
		if(type == ITEM_TYPE.SAODANG_POINT)
		{
			mSweepNum += count;
			if(mSweepNum < 0)
				mSweepNum = 0;
		}
		else if(type == ITEM_TYPE.RESET_POINT)
		{
			mResetNum += count;
			if(mResetNum < 0)
				mResetNum = 0;
		}
		else if(type == ITEM_TYPE.LOCK_POINT)
		{
			mLockNum += count;
			if(mLockNum < 0)
				mLockNum = 0;
		}
	}

	public virtual void AddPlayerLevelExp(int iAddExp)
	{
		chaExp += iAddExp;
		PlayerLevelUp ();
	}

	void PlayerLevelUp()
	{
		if(character.level >= mMaxPlayerLevel)
		{
			chaLevel = mMaxPlayerLevel;
			chaExp = 0;
			return;
		}

		if(chaExp >= GetCurrentMaxPlayerLevelExp ())
		{
			chaExp -= GetCurrentMaxPlayerLevelExp ();

			//add stamina
			//stamina += TableCommon.GetRoleLABOR(chaLevel);

			chaLevel++;

            if (GuideManager.IsGuideFinished())
            {
                DataCenter.OpenWindow("PLAYER_LEVEL_UP_SHOW_WINDOW");
            }
			//update tili 
			GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_VITAKITY", true);
			PlayerLevelUp ();
		}
	}

	int GetCurrentMaxPlayerLevelExp()
	{
		return TableCommon.GetNumberFromPlayerLevelConfig (chaLevel, "EXP");
	}

	public int GetMaxPlayerLevel()
	{
		return DataCenter.mPlayerLevelConfig.GetRecordCount () - 1;
	}

	public int GetFreeSpaceInPetBag()
	{
		return mMaxPetNum - PetLogicData.Self.mDicPetData.Count;
	}

	public int GetFreeSpaceInRoleEquipBag()
	{
		RoleEquipLogicData logic = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		return mMaxRoleEquipNum - logic.GetRoleEquipCount ();
	}
}
//-------------------------------------------------------------------------