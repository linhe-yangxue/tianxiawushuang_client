using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logic;
using DataTable;
//-------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class NetEvent
{
    static public void RegisterNetEvent()
    {
		EventCenter center = Net.gNetEventCenter;


        //center.RegisterEvent("CS_HeartBeat", new DefineFactoryLog<CS_Heartbeat>());
        center.RegisterEvent("TM_TickTestNet", new DefineFactoryLog<TM_TickTestNet>());

		center.RegisterEvent("UDP_RequestGameServerInfo", new DefineFactoryLog<UDP_RequestGameServerInfo>());

		center.RegisterEvent("Net_CloseEvent", new DefineFactoryLog<Net_CloseEvent>());
		center.RegisterEvent("Net_ConnectFailEvent", new DefineFactoryLog<Net_ConnectFailEvent>());
        center.RegisterEvent("Net_ConnectGSFailEvent", new DefineFactoryLog<Net_ConnectGSFailEvent>());
        center.RegisterEvent("LOGIN_TryLoadAccountThenEnterGame", new DefineFactoryLog<LOGIN_TryLoadAccountThenEnterGame>());        
		
		center.RegisterEvent("CS_RequestCreateAccount", new DefineFactoryLog<CS_RequestCreateAccount>());
		center.RegisterEvent("CS_BindGuestUser", new DefineFactoryLog<CS_BindGuestUser>());

		center.RegisterEvent("TestServerEvent", new DefineFactoryLog<TestServerEvent>());
		
        center.RegisterEvent("SC_NotifyLoginAgain", new DefineFactoryLog<SC_NotifyLoginAgain>());

		center.RegisterEvent("CS_QuestEnterGame", new DefineFactoryLog<CS_QuestEnterGame>());
		center.RegisterEvent("CS_RequestRoleData", new DefineFactoryLog<CS_RequestRoleData>());
		center.RegisterEvent("CS_CreateGuestUser", new DefineFactoryLog<CS_CreateGuestUser>());
		
		//center.RegisterEvent("CS_RequestCreateNewRole", new DefineFactoryLog<CS_RequestCreateNewRole>());
		center.RegisterEvent("CS_RequestChangeMainChar", new DefineFactoryLog<CS_RequestChangeMainChar>());

        center.RegisterEvent("CS_BattleStart", new DefineFactoryLog<CS_BattleStart>());
        //center.RegisterEvent("CS_BattleResult", new DefineFactoryLog<CS_BattleResult>());
		
		center.RegisterEvent("CS_RequestChangePetUsePos", new DefineFactoryLog<CS_RequestChangePetUsePos>());
		center.RegisterEvent("CS_RequestPetUpgrade", new DefineFactoryLog<CS_RequestPetUpgrade>());
		center.RegisterEvent("CS_RequestPetStrengthen", new DefineFactoryLog<CS_RequestPetStrengthen>());
		center.RegisterEvent("CS_RequestPetEvolution", new DefineFactoryLog<CS_RequestPetEvolution>());
		center.RegisterEvent("CS_RequestSalePet", new DefineFactoryLog<CS_RequestSalePet>());

		center.RegisterEvent("CS_SetTeam", new DefineFactoryLog<CS_SetTeam>());

        // for boss battle
        BossBattle.RegisterNetEvent();
        PVP6Battle.RegisterNetEvent();
		PVP4Battle.RegisterNetEvent();
        GuildBossBattle.RegisterNetEvent();
        
        // friend
        center.RegisterEvent("CS_SearchFriend", new DefineFactoryLog<CS_SearchFriend>());
		center.RegisterEvent("CS_RequestFriendList", new DefineFactoryLog<CS_RequestFriendList>());
		center.RegisterEvent("CS_RequestInviteList", new DefineFactoryLog<CS_RequestInviteList>());
		center.RegisterEvent("CS_AddFriend", new DefineFactoryLog<CS_AddFriend>());
        //center.RegisterEvent("CS_AddFriendByString", new DefineFactoryLog<CS_AddFriendByString>());
		center.RegisterEvent("CS_AcceptFriend", new DefineFactoryLog<CS_AcceptFriend>());
        //by chenliang
        //begin

//		center.RegisterEvent("CS_RefuseFriend", new DefineFactoryLog<CS_RefuseFriend>());
//--------------------

        //end
		center.RegisterEvent("CS_DeleteFriend", new DefineFactoryLog<CS_DeleteFriend>());
		center.RegisterEvent("CS_SendSpirit", new DefineFactoryLog<CS_SendSpirit>());
        center.RegisterEvent("CS_VisitFriend", new DefineFactoryLog<CS_VisitFriend>());
		center.RegisterEvent("CS_ZanResult", new DefineFactoryLog<CS_ZanResult>());

        // mail
        center.RegisterEvent("CS_RequestMailList", new DefineFactoryLog<CS_RequestMailList>());
        center.RegisterEvent("CS_ReadMail", new DefineFactoryLog<CS_ReadMail>());
        center.RegisterEvent("CS_ReadAllMail", new DefineFactoryLog<CS_ReadAllMail>());

        // shop
        center.RegisterEvent("CS_BuyItemResult", new DefineFactoryLog<CS_BuyItemResult>());

        center.RegisterEvent("CS_RequestMissionData", new DefineFactoryLog<CS_RequestMissionData>());
        //center.RegisterEvent("CS_TaskAcceptAwardResult", new DefineFactoryLog<CS_TaskAcceptAwardResult>());
        //center.RegisterEvent("GC_MissionFinish", new DefineFactoryLog<GC_MissionFinish>());
		center.RegisterEvent("GC_MailNum", new DefineFactoryLog<GC_MailNum>());
		center.RegisterEvent("GC_NewTujian", new DefineFactoryLog<GC_NewTujian>());
        center.RegisterEvent("GC_AwardTujian", new DefineFactoryLog<GC_AwardTujian>());
		center.RegisterEvent("GC_BossNum", new DefineFactoryLog<GC_BossNum>());
		center.RegisterEvent("GC_DailySign", new DefineFactoryLog<GC_DailySign>());
        center.RegisterEvent("GC_NotifyNotice", new DefineFactoryLog<GC_NotifyNotice>());
		center.RegisterEvent("GC_Exit", new DefineFactoryLog<GC_Exit>());
		center.RegisterEvent("CS_DiamondDailySign", new DefineFactoryLog<CS_DiamondDailySign>());

        // bag
        center.RegisterEvent("CS_RequestPetList", new DefineFactoryLog<CS_RequestPetList>());
		center.RegisterEvent("CS_RequestGem", new DefineFactoryLog<CS_RequestGem>());
        center.RegisterEvent("CS_RequestRoleEquip", new DefineFactoryLog<CS_RequestRoleEquip>());
        center.RegisterEvent("CS_RequestPetEquip", new DefineFactoryLog<CS_RequestPetEquip>());
		center.RegisterEvent("CS_RequestConsumData", new DefineFactoryLog<CS_RequestConsumData>());
        center.RegisterEvent("CS_RequestMaterial", new DefineFactoryLog<CS_RequestMaterial>());
        center.RegisterEvent("CS_RequestMaterialFragment", new DefineFactoryLog<CS_RequestMaterialFragment>());
        
		center.RegisterEvent("CS_ComposePet", new DefineFactoryLog<CS_ComposePet>());
        center.RegisterEvent("CS_ComposeMaterial", new DefineFactoryLog<CS_ComposeMaterial>());
        // role equip
        center.RegisterEvent("CS_RequestRoleEquipStrengthen", new DefineFactoryLog<CS_RequestRoleEquipStrengthen>());
        center.RegisterEvent("CS_RequestRoleEquipEvolution", new DefineFactoryLog<CS_RequestRoleEquipEvolution>());
        center.RegisterEvent("CS_RequestRoleEquipReset", new DefineFactoryLog<CS_RequestRoleEquipReset>());

		center.RegisterEvent("CS_RequestRoleEquipUse", new DefineFactoryLog<CS_RequestRoleEquipUse>());
		center.RegisterEvent("CS_RequestRoleEquipSale", new DefineFactoryLog<CS_RequestRoleEquipSale>());

        center.RegisterEvent("CS_ComposeFabao", new DefineFactoryLog<CS_ComposeFabao>());

		// tujian
		center.RegisterEvent("CS_RequestTujian", new DefineFactoryLog<CS_RequestTujian>());
		center.RegisterEvent("CS_TujianReward", new DefineFactoryLog<CS_TujianReward>());

        center.RegisterEvent("CS_AcceptStageBonus", new DefineFactoryLog<CS_AcceptStageBonus>());

		// on hook
		center.RegisterEvent("CS_RequestIdleBottingStatus", new DefineFactoryLog<CS_RequestIdleBottingStatus>());
		center.RegisterEvent("CS_RequestIdleBottingBegin", new DefineFactoryLog<CS_RequestIdleBottingBegin>());
		center.RegisterEvent("CS_RequestIdleBottingSpeedUp", new DefineFactoryLog<CS_RequestIdleBottingSpeedUp>());
		center.RegisterEvent("CS_RequestIdleBottingAward", new DefineFactoryLog<CS_RequestIdleBottingAward>());

        // board
        center.RegisterEvent("CS_RequestAnouncement", new DefineFactoryLog<CS_RequestAnouncement>());

        center.RegisterEvent("CS_RequestSaoDang", new DefineFactoryLog<CS_RequestSaoDang>());

        center.RegisterEvent("SC_RequestUpdateNotice", new DefineFactoryLog<SC_RequestUpdateNotice>());

		center.RegisterEvent("CS_UpdateHDData", new DefineFactoryLog<CS_UpdateHDData>());
		center.RegisterEvent("CS_RequestHDData", new DefineFactoryLog<CS_RequestHDData>());

		center.RegisterEvent("CS_RequestFragmentData", new DefineFactoryLog<CS_RequestFragmentData>());

        // rank        
        center.RegisterEvent("CS_RequestFightPowerRank", new DefineFactoryLog<CS_RequestFightPowerRank>());
        center.RegisterEvent("CS_RequestMyFightPowerRank", new DefineFactoryLog<CS_RequestMyFightPowerRank>());

        // guide
        center.RegisterEvent("CS_RequestGuideProcess", new DefineFactoryLog<CS_RequestGuideProcess>());
        center.RegisterEvent("CS_SaveGuideProcess", new DefineFactoryLog<CS_SaveGuideProcess>());
	}
}


public abstract class tNetEvent : tServerEvent
{
    public override bool DoEvent()
    {
        DataCenter.CloseWindow("TOP_MESSAGE_WINDOW");
        Net.StartWaitEffect();
        bool result = base.DoEvent();

        if (GetFinished())
            Net.StopWaitEffect();

        return result;
    }

    public override bool _OnEvent(tEvent evt)
    {
        Net.StopWaitEffect();
        return base._OnEvent(evt);
    }

    public override void Finish()
    {
        if (!GetFinished())
            Net.StopWaitEffect();

        base.Finish();
    }

    public override void OnSendFail(SEND_RESULT errorResult)
    {
        Net.StopWaitEffect();
        if (errorResult == SEND_RESULT.eNetDisconnect)
        {
            //DEBUG.LogWarning("Send event net disconnect over time, then restart game >"+GetEventName());
            //GlobalModule.RestartGame();

            DataCenter.OpenWindow("TOP_MESSAGE_WINDOW", "网络不给力，正在尝试连接");
            DEBUG.LogWarning("Net event [" + GetEventName() + "] disconnect fialed to send, try to reconnect game server...");
            LoginNet.ReconnectGameServer(this);
        }
    }

    public override void DoOverTime()
    {
        Net.StopWaitEffect();
        base.DoOverTime();
        //DataCenter.OpenWindow("TOP_MESSAGE_WINDOW", "网络不给力，正在尝试连接");

        DEBUG.LogWarning("Net evet[" + GetEventName() + "] time over, try to reconnect to game server...");
        LoginNet.ReconnectGameServer(this);
        /*
        DEBUG.LogWarning("Net evet over time, then restart game");
        // NOTE: Now restart to login game when net event over time or send fail
        GlobalModule.RestartGame();
        */
    }
}

public abstract class BaseNetEvent : tNetEvent
{
    public override bool DoEvent()
    {
        
        Net.mLastSendEvent = this;
        return base.DoEvent();
    }

    public override bool _OnEvent(tEvent evt)
    {
        Net.mLastSendEvent = null;
        return base._OnEvent(evt);
    }

    public override void DoOverTime()
    {
        Net.mLastSendEvent = null;

        // over time may be net close, then try once connect game server
        if (!LoginNet.IsWaitSendEvent(this))
        {
            if ((int)get("RESEND_DBID") > 0)
            {
                DataCenter.OpenWindow("LOGIN_WINDOW");
                return;
            }

            SetFinished(false);

            RoleLogicData roleLogic = RoleLogicData.Self;
            if (roleLogic == null)
            {
                DataCenter.OpenWindow("LOGIN_WINDOW");
                return;
            }
            set("RESEND_DBID", RoleLogicData.Self.mDBID);
            //DataCenter.OpenWindow("TOP_MESSAGE_WINDOW", "网络重连, 尝试重新请求...");
            LoginNet.ReconnectGameServer(this);

        }
        else
        {
            base.DoOverTime();
        }
    }

    protected override void AlloctRespID()
    {
        UInt64 id = 0;
        if (!get(ResponsionEvent.RESP_ID_KEY, out id))       
            base.AlloctRespID();
    }
}


//-------------------------------------------------------------------------
public class ItemDataBase
{
    public int itemId = -1;// 唯一ID
    public int tid; // table id
    public int itemNum; // 数量

	public override string ToString ()
	{
		return "[itemId:" + itemId + "][tid:" + tid + "][itemNum:" + itemNum + "]";
	}
}

public class TeamPosItemDataBase : ItemDataBase
{
    public int teamPos = -1; // 装备绑定阵Id
    public bool IsInTeam()
    {
        return teamPos >= 0;
    }
}

public class GemData
{
    public int mType;
    public int mCount;
}

public class MapData
{
    public int battleId = -1;
    public int successCount = -1;       // 已成功次数
    public int fightCount = -1;         // 已挑战次数
    public int bestRate = -1;           // 历史最高评价
    public int todaySuccess = -1;       // 今日通关次数
    public int todayReset = -1;         // 今日重置次数
}

public class TujianData
{
    public int mID;
    public int mStatus;
}

public class FriendData:NetLogicData
{
	public string friendId;
	public string name;
	public int level;
	public long lastLoginTime;
	public int icon;
	public bool enableSendSpirit;

    public int mailID;
    public string mID;
    public int mLevel;
	public int mStrengthenLevel;
    public int mModel;
    public int mTime;
	public int mDonateTime;
	public int mCombatTime;
	public int mLoginTime;
	public bool mIsFriend;

    public int power;
    public string guildName;
}
public class GetFriendData
{
	public FriendData[] arr;
}
public class TaskData
{
    public int m_iTaskID = 0;
	public int m_iTaskAimCount = 0;
	public TASK_STATE m_TaskState = TASK_STATE.Can_Not_Accept;
}

public class MailData:NetLogicData
{
	public int mailId;
	public string mailTitle;
	public string mailContent;
	public int mailType;
	public long mailTime;
	public long mailSaveTime;
	public int tid;
	public int itemNum;
	public ItemDataBase[] items;

	public int mModelID;
	public int mType;
	public int mCount;
	public Int64 mTime;
    public int mTitleID;
	public int mRoleEquipElement;
	public string mAddTitle;
}

public class DailySignData
{
	public int mID;
	public int mStatus;
//	public int mCanReceiveMaxNum;
}

public class ShopData
{
	public int mIndex;
	public int mUsedCount;
	public Int64 mGetFreeCardLastTime;
}

public class EquipAttribute
{
    public AFFECT_TYPE mType = AFFECT_TYPE.NONE;
    public float mValue = 0;
    public bool mIsRate = false;

    public virtual bool ApplyAffect(){return true;}

    public virtual float Affect(AFFECT_TYPE affectType)
    {
        if (affectType == mType)
        {
            if (!mIsRate)
                return mValue;
        }
        return 0;
    }

    public virtual float AffectRate(AFFECT_TYPE affectType)
    {
        if (affectType == mType)
        {
            if (mIsRate)
                return mValue;
        }
        return 0;
    }
}

public class EquipAttachAttribute : EquipAttribute
{
	public LOCK_STATE mLockState = LOCK_STATE.UNLOCK;
    public override bool ApplyAffect()
    {
        string strType = GameCommon.ToAffectTypeString(mType);
        mIsRate = strType.LastIndexOf("_RATE") > 0;

        return true;
    }
}

public class EquipBaseAttribute : EquipAttribute
{
    EquipData mEquipData = null;
    public int mIndex = 0;
    public void SetEquipData(EquipData equipData)
    {
        mEquipData = equipData;
    }

    public override bool ApplyAffect()
    {
        if (mEquipData != null)
        {
            mValue = RoleEquipData.GetBaseAttributeValue(mIndex, mEquipData.tid, mEquipData.strengthenLevel);

            mType = (AFFECT_TYPE)TableCommon.GetNumberFromRoleEquipConfig(mEquipData.tid, "ATTRIBUTE_TYPE_" + mIndex.ToString());
            string strType = GameCommon.ToAffectTypeString(mType);
            mIsRate = strType.LastIndexOf("_RATE") > 0;
        }
        return true;
    }
}

public enum ON_HOOK_STATE
{
	LOCKED,
	UN_START,
	DOING,
	UN_GET_AWARD,
}

public class OnHookData
{
	public ON_HOOK_STATE mState = ON_HOOK_STATE.LOCKED;
	public int mLevel = -1;
	public Int64 mRemainingTime = 0;
	public int mRateNum = 100;
	public int mRemainSpeedupToday = 0;
}

public class ConsumeItemData : ItemDataBase
{
	public Int64 mCountdownTime;
	public int mGridIndex;
    public int needLevel;
    public int mSpecifyUseNum = -1; //> 自定义使用道具的数量

    public ConsumeItemData()
    {

    }

    public ConsumeItemData(ItemDataBase itemData)
    {
        if (null == itemData)
            return;

        itemId = itemData.itemId;
        itemNum = itemData.itemNum;
        tid = itemData.tid;
        mCountdownTime = CommonParam.NowServerTime();
    }
}

public class ConsumeItemStatus
{
	public int mIndex;
	public Int64 mCountdownTime;
}


public class MaterialData
{
    public int mIndex;
    public int mCount;
    public int mGridIndex;
}

public class MaterialFragmentData
{
    public int mFragmentIndex;
    public int mMaterialIndex;
    public int mCount;
    public int mGridIndex;
}
//-------------------------------------------------------------------------



public class GemLogicData : tLogicData
{
    public GemData[] mGemList;
    public Dictionary<string, GemData> itemList = new Dictionary<string, GemData>();

    public GemData GetGemDataByIndex(int iIndex)
    {
        if (iIndex < mGemList.Length)
        {
            return mGemList[iIndex];
        }

        Logic.EventCenter.Log(LOG_LEVEL.WARN, "iIndex >= mGemList.Length");
        return null;
    }

	public GemData GetGemDataByType(int iType)
	{
		for(int i = 0; i < mGemList.Length; i++)
		{
			if(mGemList[i].mType == iType)
				return  mGemList[i];
		}

		return null;
	}

	public void SetGetDataByType(int iType, int iCount)
	{
		for(int i = 0; i < mGemList.Length; i++)
		{
			if(mGemList[i].mType == iType)
				mGemList[i].mCount += iCount;
		}
	}

	public int GetCountByID(int ID)
	{
		int count = 0;
		for(int i = 0; i < mGemList.Length; i++)
		{
			if(mGemList[i].mType == ID)
				count += mGemList[i].mCount;
		}
		return count;
	}
}

public class MapLogicData : SingletonData<MapLogicData>
{
    //public static MapLogicData Self { get; private set; }
    public static Action<int> mOnFirstClearStage = null; // 第一次通关某个索引的关卡时调用的回调

    private Dictionary<int, MapData> mapDict = new Dictionary<int, MapData>();

    //public MapLogicData() { Self = this; }

    //public void Clear()
    //{
    //    mapDict.Clear();
    //}
    //
    public void Update(IEnumerable<MapData> mapDatas)
    {
        foreach (var d in mapDatas)
        {
            if (mapDict.ContainsKey(d.battleId))
            {
                mapDict[d.battleId] = d;
            }
            else 
            {
                mapDict.Add(d.battleId, d);
            }
        }
    }

    public MapData GetMapDataByStageIndex(int stageIndex)
    {
        MapData data = null;

        if (!mapDict.TryGetValue(stageIndex, out data))
        {
            data = new MapData();
            data.battleId = stageIndex;
            data.bestRate = 0;
            data.fightCount = 0;
            data.successCount = 0;
            data.todaySuccess = 0;
            data.todayReset = 0;
            mapDict.Add(stageIndex, data);
        }

        return data;
    }

    public MapData UpdateData(int stageIndex, bool iswin, int starRate)
    {
        var d = GetMapDataByStageIndex(stageIndex);
        d.fightCount++;

        if (iswin)
        {
            d.successCount++;
        }

        if (starRate > d.bestRate)
        {
            d.bestRate = starRate;
        }

        if (iswin && d.successCount == 1 && mOnFirstClearStage != null)
        {
            mOnFirstClearStage(stageIndex);
        }

        return d;
    }
}

public class FriendLogicData : NetLogicData
{
    public FriendData[] arr;
	static public FriendData mSelectPlayerData;
}

public class TujianLogicData : tLogicData
{
    public TujianData[] mTujianList;

	public TujianData GetTujianDataByModelIndex(int iModelIndex)
	{
		foreach(TujianData tujianData in mTujianList)
		{
			if(tujianData.mID == iModelIndex)
			{
				return tujianData;
			}
		}
		return null;
	}
}

public class MailLogicData : NetLogicData
{
	public MailData[] mails;
    public int mailSaveTime = 2 * 24 * 3600;
}

public class DailySignLogicData : tLogicData
{
	public DailySignData[] mDailSignDataList;
}

public class ShopLogicData : tLogicData
{
	public ShopData[] mShopDataList;

	public ShopData GetShopDataByIndex(int index)
	{
		if(mShopDataList == null)
			return null;

		for(int i = 0; i < mShopDataList.Length; i++)
		{
			if(mShopDataList[i].mIndex == index)
				return mShopDataList[i];
		}

		return null;
	}

	public void SetShopDataByIndex(int index, int addCount, Int64 lastTime)
	{
		if(mShopDataList == null)
			return ;

		for(int i = 0; i < mShopDataList.Length; i++)
		{
			if(mShopDataList[i].mIndex == index)
			{
				mShopDataList[i].mUsedCount += addCount;
				mShopDataList[i].mGetFreeCardLastTime = lastTime;
			}
		}
	}

	public bool GetFree()
	{
		bool isFree = false;
		Dictionary<int, DataRecord> dicRecord = DataCenter.mShopSlotBase.GetAllRecord();
		foreach (KeyValuePair<int, DataRecord> iter in dicRecord)
		{
			if (iter.Value.get ("IS_FREE"))
			{
				isFree = IsFreeByIndex (iter.Key);
				if(isFree) return isFree;
			}
		}
		
		return isFree;
	}

	public bool GetFreeByPageType(SHOP_PAGE_TYPE type)
	{
		bool isFree = false;
		Dictionary<int, DataRecord> dicRecord = DataCenter.mShopSlotBase.GetAllRecord();
		foreach (KeyValuePair<int, DataRecord> iter in dicRecord)
		{
			if (iter.Value.get ("IS_FREE") && (int)iter.Value.get("PAGE_TYPE") == (int)type)
			{
				isFree = IsFreeByIndex (iter.Key);
				if(isFree) return isFree;
			}
		}

		return isFree;
	}

	public bool IsFreeByIndex(int index)
	{
		if(mShopDataList == null)
			return false;

		bool isFree = false;
		int iMaxFreeCount = TableCommon.GetNumberFromShopSlotBase (index, "FREE_COUNT");
		int iFreeIntervalTime = TableCommon.GetNumberFromShopSlotBase (index, "FREE_INTERVAL");
		bool isDailyFreeCard = Convert.ToBoolean(TableCommon.GetNumberFromShopSlotBase(index, "IS_DAILY_REFRESH"));

		for(int i = 0; i < mShopDataList.Length; i++)
		{
			if(mShopDataList[i].mIndex == index)
			{
				ShopData shopData = mShopDataList[i];
				if(isDailyFreeCard)
				{
					if(shopData.mUsedCount < iMaxFreeCount && (CommonParam.NowServerTime() - shopData.mGetFreeCardLastTime) > iFreeIntervalTime)
						isFree = true;
					else
						isFree = false;
				}
				else
				{
					if((CommonParam.NowServerTime() - shopData.mGetFreeCardLastTime) > iFreeIntervalTime) 
						isFree = true;
					else 
						isFree = false;
				}
			}
		}
		return isFree;
	}

}

public class TaskLogicData : SingletonData<TaskLogicData>
{
    public int curScore { get; private set; }
    public List<int> acceptedScoreAwards { get; private set; }
    public List<TaskObject> dailyTasks { get; private set; }
    public List<TaskObject> achievements { get; private set; }

    public TaskLogicData()
    {
        curScore = 0;
        acceptedScoreAwards = new List<int>();
        dailyTasks = new List<TaskObject>();
        achievements = new List<TaskObject>();
    }

    /// <summary>
    /// 更新日常任务数据
    /// </summary>
    /// <param name="taskDatas"> 任务数据 </param>
    /// <param name="unacceptedScoreAwards"> 已领取的积分奖励 </param>
    /// <param name="curScore"> 当前积分 </param>
    public void UpdateDailyTasks(IEnumerable<TaskObject> taskDatas, IEnumerable<int> acceptedScoreAwards, int curScore)
    {
        this.curScore = curScore;
        this.acceptedScoreAwards = new List<int>(acceptedScoreAwards);
        this.dailyTasks = new List<TaskObject>(taskDatas);
    }

    /// <summary>
    /// 更新成就数据
    /// </summary>
    /// <param name="taskDatas"> 成就数据 </param>
    public void UpdateAchievements(IEnumerable<TaskObject> taskDatas)
    {
        this.achievements = new List<TaskObject>(taskDatas);
    }

    /// <summary>
    /// 获取日常任务数据对象
    /// </summary>
    /// <param name="taskId"> 任务id </param>
    /// <returns> 任务数据对象 </returns>
    public TaskObject GetDailyTaskObject(int taskId)
    {
        return dailyTasks.Find(x => x.taskId == taskId);

        //if (obj == null)
        //{
        //    obj = new TaskObject { taskId = taskId, progress = 0, accepted = 0 };
        //    dailyTasks.Add(obj);
        //}

        //return obj;
    }

    /// <summary>
    /// 获取成就数据对象
    /// </summary>
    /// <param name="taskId"> 成就id </param>
    /// <returns> 成就数据对象 </returns>
    public TaskObject GetAchievementTaskObject(int taskId)
    {
        return achievements.Find(x => x.taskId == taskId);

        //if (obj == null)
        //{
        //    obj = new TaskObject { taskId = taskId, progress = 0, accepted = 0 };
        //    achievements.Add(obj);
        //}

        //return obj;
    }

    /// <summary>
    /// 添加或更新任务对象
    /// </summary>
    /// <param name="obj"> 被添加或更新的对象 </param>
    //public void AddOrUpdateAchievementTaskObject(TaskObject obj)
    //{
    //    TaskObject result = achievements.Find(x => x.taskId == obj.taskId);

    //    if (result == null)
    //    {
    //        result = new TaskObject() { taskId = obj.taskId, progress = obj.progress, accepted = obj.accepted };
    //        achievements.Add(result);
    //    }
    //    else 
    //    {
    //        result.progress = obj.progress;
    //        result.accepted = obj.accepted;
    //    }
    //}

    /// <summary>
    /// 获取任务奖励
    /// </summary>
    /// <param name="taskId"> 任务id </param>
    public List<ItemDataBase> AcceptDailyTaskAward(int taskId, IEnumerable<ItemDataBase> awardList)
    {
        List<ItemDataBase> addItems;

        // 增加积分
        DataRecord r = DataCenter.mTaskConfig.GetRecord(taskId);
        curScore += r["SCORE_NUM"];

        // 获取物品奖励
        addItems = PackageManager.UpdateItem(awardList);

        // 设置为已领取
        TaskObject obj = dailyTasks.Find(x => x.taskId == taskId);
        obj.accepted = 1;

        return addItems;
    }

    /// <summary>
    /// 获取成就奖励
    /// </summary>
    /// <param name="taskId"> 成就id </param>
    public List<ItemDataBase> AcceptAchievementAward(int taskId, IEnumerable<ItemDataBase> awardList)
    {
        List<ItemDataBase> addItems;

        // 获取物品奖励
        addItems = PackageManager.UpdateItem(awardList);

        // 设置为已领取
        TaskObject obj = achievements.Find(x => x.taskId == taskId);
        obj.accepted = 1;

        // 添加新解锁任务数据
        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mAchieveConfig.GetAllRecord())
        {
            int preTaskId = pair.Value["EX_TASK"];

            if (preTaskId == taskId && !achievements.Exists(x => x.taskId == pair.Key))
            {
                DataRecord r = DataCenter.mAchieveConfig.GetRecord(preTaskId);
                int minLevel = r["TASK_SHOW_LVMIN"];
                int maxLevel = r["TASK_SHOW_LVMAX"];
                int curLevel = RoleLogicData.GetMainRole().level;

                if (minLevel <= curLevel && curLevel <= maxLevel)
                {
                    achievements.Add(new TaskObject() { taskId = pair.Key, progress = obj.progress, accepted = 0 });
                }
            }
        }

        return addItems;
    }

    /// <summary>
    /// 获取积分奖励
    /// </summary>
    /// <param name="awardId"> 积分奖励id </param>
    public List<ItemDataBase> AcceptTaskScoreAward(int awardId, IEnumerable<ItemDataBase> awardList)
    {
        List<ItemDataBase> addItems;

        // 获取物品奖励
        addItems = PackageManager.UpdateItem(awardList);

        // 添加至已领取的积分奖励列表
        acceptedScoreAwards.Add(awardId);

        return addItems;
    }

    /// <summary>
    /// 积分奖励是否已领取
    /// </summary>
    /// <param name="awardId"> 积分奖励id </param>
    /// <returns></returns>
    public bool IsScoreAwardAccepted(int awardId)
    {
        return acceptedScoreAwards.Contains(awardId);
    }

    public Dictionary<TASK_PAGE_TYPE, Dictionary<int, TaskData>> m_dicTask = new Dictionary<TASK_PAGE_TYPE, Dictionary<int, TaskData>>();

    public Dictionary<int, TaskData> m_dicDailyTask = new Dictionary<int, TaskData>();
    public Dictionary<int, TaskData> m_dicAchivementTask = new Dictionary<int, TaskData>();
	public Dictionary<int, TaskData> m_dicAchivementDeliverTask = new Dictionary<int, TaskData>();
    public Dictionary<int, TaskData> m_dicActivityTask = new Dictionary<int, TaskData>();
    public Dictionary<int, TaskData> m_dicWeeklyTask = new Dictionary<int, TaskData>();

    //public TaskLogicData()
    //{
    //    m_dicTask.Add(TASK_PAGE_TYPE.DAILY, m_dicDailyTask);
    //    m_dicTask.Add(TASK_PAGE_TYPE.ACHIEVEMENT, m_dicAchivementTask);
    //	m_dicTask.Add(TASK_PAGE_TYPE.ACHIEVEMENT_DILIVER, m_dicAchivementDeliverTask);
    //    m_dicTask.Add(TASK_PAGE_TYPE.ACTIVITY, m_dicActivityTask);
    //    m_dicTask.Add(TASK_PAGE_TYPE.WEEKLY, m_dicWeeklyTask);
    //}

    public TASK_PAGE_TYPE GetTaskPageType(int iTaskID)
    {
        return (TASK_PAGE_TYPE)TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_TABLE");
    }

    public TaskData GetTaskData(int iTaskID)
    {
        foreach (KeyValuePair<TASK_PAGE_TYPE, Dictionary<int, TaskData>> tempIter in m_dicTask)
        {
            if (tempIter.Value.ContainsKey(iTaskID))
                return tempIter.Value[iTaskID];
        }
        return null;
    }

    public bool AddAcceptTask(TaskData taskData)
    {
        if (taskData != null)
        {
            int iTaskID = taskData.m_iTaskID;
            TASK_PAGE_TYPE taskPageType = GetTaskPageType(iTaskID);

            if (m_dicTask[taskPageType].ContainsKey(iTaskID))
            {
                TaskData task = m_dicTask[taskPageType][iTaskID];
                task.m_iTaskAimCount = taskData.m_iTaskAimCount;
                task.m_TaskState = taskData.m_TaskState;
            }
            else
            {
                m_dicTask[taskPageType].Add(iTaskID, taskData);
            }
			int iType = (int)taskPageType;
            return true;
        }
        return false;
    }

	public bool AddDeliverTask(TaskData taskData)
    {
        if (taskData != null)
        {
            int iTaskID = taskData.m_iTaskID;
            TASK_PAGE_TYPE taskPageType = GetTaskPageType(iTaskID);

            if (taskPageType == TASK_PAGE_TYPE.ACHIEVEMENT)
            {
                taskPageType = TASK_PAGE_TYPE.ACHIEVEMENT_DILIVER;
                if (m_dicTask[taskPageType].ContainsKey(iTaskID))
                {
                    TaskData task = m_dicTask[taskPageType][iTaskID];
                    task.m_iTaskAimCount = taskData.m_iTaskAimCount;
					task.m_TaskState = taskData.m_TaskState;
                }
                else
                {
                    m_dicTask[taskPageType].Add(iTaskID, taskData);
                }
                return true;
            }
			int iType = (int)taskPageType;
        }        
        return false;
    }   

    public bool RomoveAcceptTask(TaskData taskData)
    {
        if (taskData != null)
        {
            int iTaskID = taskData.m_iTaskID;
			TASK_PAGE_TYPE taskPageType = GetTaskPageType(iTaskID);

			if (taskPageType == TASK_PAGE_TYPE.ACHIEVEMENT)
            {
                if (m_dicTask[taskPageType].ContainsKey(iTaskID))
                {
					m_dicTask[taskPageType].Remove(iTaskID);
                    return true;
                }
            }

			int iType = (int)taskPageType;
        }
        return false;
    }

	public bool RomoveDeliverTask(TaskData taskData)
    {
        if (taskData != null)
        {
            int iTaskID = taskData.m_iTaskID;
            TASK_PAGE_TYPE taskPageType = GetTaskPageType(taskData.m_iTaskID);

            if (taskPageType == TASK_PAGE_TYPE.ACHIEVEMENT)
            {
                taskPageType = TASK_PAGE_TYPE.ACHIEVEMENT_DILIVER;
                if (m_dicTask[taskPageType].ContainsKey(iTaskID))
                {
                    m_dicTask[taskPageType].Remove(iTaskID);
                    return true;
                }
            }

			int iType = (int)taskPageType;
        }
        
        return false;
    }

    public void CreateTaskData(int iTaskID)
    {
        TaskData task = new TaskData();
        task.m_iTaskID = iTaskID;
        task.m_iTaskAimCount = 0;

        task.m_TaskState = TASK_STATE.Had_Accept;

        UpdateTaskInfo(task);
    }

	public void AddTaskData(int iTaskID, int iTaskAimCount, TASK_STATE taskState)
	{
        TaskData task = new TaskData();

        task.m_iTaskID = iTaskID;
		task.m_iTaskAimCount = iTaskAimCount;
		task.m_TaskState = taskState;

        UpdateTaskInfo(task);
    }

    public void UpdateTaskInfo(int iTaskID)
    {
		TaskData task = GetTaskData(iTaskID);
		UpdateTaskInfo(task);
    }

	public void UpdateTaskInfo(TaskData taskData)
	{
		if (taskData == null)
			return;
		
		int iTaskState = (int)taskData.m_TaskState;
		int iTaskID = taskData.m_iTaskID;
//		DEBUG.LogError("iTaskID = " + iTaskID.ToString() + ", iTaskState = " + iTaskState.ToString());

        if (iTaskState == (int)TASK_STATE.Can_Not_Accept || iTaskState == (int)TASK_STATE.Deliver)
        {
            return;
        }

        // 若任务不可见，则不加入任务列表中
        if (TableCommon.GetNumberFromTaskConfig(iTaskID, "DISPLAY") == 0)
        {
            return;
        }

		// 已接取任务
		AddAcceptTask(taskData);
		
		//if (iTaskState == (int)TASK_STATE.Deliver)
		//{
		//	// 已交付任务
        //    RomoveAcceptTask(taskData);
        //
        //    AddDeliverTask(taskData);
		//}
        
        //if (GameCommon.bIsWindowExist("MissionWindow"))
        //{
        //    TaskLogicData logic = DataCenter.GetData("TASK_DATA") as TaskLogicData;
        //
        //    DataCenter.SetData("MissionWindow", "SET_SEL_PAGE", logic.GetTaskPageType(taskData.m_iTaskID));
        //}
	}

    public void SetTaskFinished(int taskID)
    {
        TaskData task = GetTaskData(taskID);
        if (task == null)
        {
            task = new TaskData();
            task.m_iTaskID = taskID;
            task.m_iTaskAimCount = 0;
        }
        task.m_TaskState = TASK_STATE.Finished;
        UpdateTaskInfo(task);
    }

	public bool CheckWhetherHadFinishTask()
	{
        //foreach (KeyValuePair<TASK_PAGE_TYPE, Dictionary<int, TaskData>> iter in m_dicTask)
        //{
        //    foreach(KeyValuePair<int, TaskData> tempIter in iter.Value)
		//    {
        //        if (tempIter.Value.m_TaskState == TASK_STATE.Finished)
        //            return true;
		//    }
        //}
		//
		//return false;
        return CheckTask(taskData => taskData.m_TaskState == TASK_STATE.Finished);
	}

    public bool CheckWhetherHadAcceptTask()
    {
        return CheckTask(taskData => (taskData.m_TaskState == TASK_STATE.Had_Accept || taskData.m_TaskState == TASK_STATE.Finished));
    }

    public bool CheckWhetherHadFinishTask(TASK_PAGE_TYPE page)
    {
        return CheckTaskByPage(page, taskData => taskData.m_TaskState == TASK_STATE.Finished);
    }

    public bool CheckWhetherHadAcceptTask(TASK_PAGE_TYPE page)
    {
        return CheckTaskByPage(page, taskData => (taskData.m_TaskState == TASK_STATE.Had_Accept || taskData.m_TaskState == TASK_STATE.Finished));
    }

    public bool CheckTaskByPage(TASK_PAGE_TYPE page, Predicate<TaskData> match)
    {
        foreach (KeyValuePair<int, TaskData> tempIter in m_dicTask[page])
        {
            if (match(tempIter.Value))
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckTask(Predicate<TaskData> match)
    {
        foreach (KeyValuePair<TASK_PAGE_TYPE, Dictionary<int, TaskData>> iter in m_dicTask)
        {
            if (CheckTaskByPage(iter.Key, match))
            {
                return true;
            }
        }
        return false;
    }

    public void ForEachByPage(TASK_PAGE_TYPE page, Action<TaskData> action)
    {
        foreach (KeyValuePair<int, TaskData> tempIter in m_dicTask[page])
        {
            action(tempIter.Value);
        }
    }

    public void ForEach(Action<TaskData> action)
    {
        foreach (KeyValuePair<TASK_PAGE_TYPE, Dictionary<int, TaskData>> iter in m_dicTask)
        {
            ForEachByPage(iter.Key, action);
        }
    }
}

public class StageBonusLogicData : tLogicData
{
    public HashSet<int> mAcceptedList = new HashSet<int>();

    public static bool IsBonusAccepted(int bonusId)
    {
        StageBonusLogicData data = DataCenter.GetData("STAGE_BONUS_DATA") as StageBonusLogicData;

        if (data == null)
            return true;

        return data.mAcceptedList.Contains(bonusId);
    }

    public static bool AddToAcceptedList(int bonusId)
    {
        StageBonusLogicData data = DataCenter.GetData("STAGE_BONUS_DATA") as StageBonusLogicData;

        if (data == null)
            return false;

        data.mAcceptedList.Add(bonusId);
        return true;
    }

    public static int GetAcceptedCount()
    {
        StageBonusLogicData data = DataCenter.GetData("STAGE_BONUS_DATA") as StageBonusLogicData;

        if (data == null)
            return -1;

        return data.mAcceptedList.Count;
    }
}

public class OnHookLogicData : tLogicData
{
	public OnHookData mOnHook;
}

//-------------------------------------------------
// consume item
//-------------------------------------------------
public class NetConsumeItemLogicData : RespMessage
{
    public Dictionary<string, ConsumeItemData> itemList = new Dictionary<string, ConsumeItemData>();
}
public class ConsumeItemLogicData : tLogicData
{
	public List<ConsumeItemData> mConsumeItemList;
//	public ConsumeItemData[] mConsumeItemData;

    public Dictionary<int, ConsumeItemData> mDicConsumeItemData = new Dictionary<int, ConsumeItemData>();

    public static ConsumeItemLogicData Self { get; private set; }

    public ConsumeItemLogicData()
    {
        Self = this;
    }

	public ConsumeItemData GetDataByGridIndex(int iGridIndex)
	{
        if (mConsumeItemList.Count != 0)
            return mConsumeItemList[iGridIndex];
        else
            return null;
	}

    public ConsumeItemData GetDataByTid(int iTid)
    {
        if (mDicConsumeItemData.ContainsKey(iTid))
        {
            return mDicConsumeItemData[iTid];
        }
        else
        {
            return AddItemData(-1, iTid, 0);
        }
    }

    public ConsumeItemData GetDataByItemId(int iItemId)
    {
        foreach (KeyValuePair<int, ConsumeItemData> pair in mDicConsumeItemData)
        {
            if (pair.Value.itemId == iItemId)
            {
                return pair.Value;
            }
        }
        return null;
    }

    public int GetItemIdByTid(int tid)
    {
        ConsumeItemData data = GetDataByTid(tid);
        if (data != null)
        {
            return data.itemId;
        }
        return -1;
    }

    public ConsumeItemData AddItemData(ItemDataBase itemData)
    {
        if (itemData != null)
        {
            ConsumeItemData data;
            if (mDicConsumeItemData.TryGetValue(itemData.tid, out data))
            {
                data.itemId = itemData.itemId;
                data.itemNum += itemData.itemNum;
                data.itemNum = Mathf.Clamp(data.itemNum, 0, CommonParam.packageOverlapLimit);
                return data;
            }

            data = new ConsumeItemData(itemData);
            mDicConsumeItemData.Add(itemData.tid, data);
            return data;
        }
        return null;
    }

    public ConsumeItemData AddItemData(int itemID, int tid, int count)
    {
        ItemDataBase item = new ItemDataBase();
        item.itemId = itemID;
        item.tid = tid;
        item.itemNum = count;
        return AddItemData(item);
    }

    public void UpdateItemData(ItemDataBase itemData)
    {
        if (itemData != null)
        {
            ConsumeItemData data;
            if (mDicConsumeItemData.TryGetValue(itemData.tid, out data))
            {
                data.itemId = itemData.itemId;
                data.itemNum = itemData.itemNum;
            }
            else
            {
                data = new ConsumeItemData(itemData);
                mDicConsumeItemData.Add(itemData.tid, data);
            }

            if (itemData.itemNum <= 0)
            {
                mDicConsumeItemData.Remove(itemData.tid);
            }
        }
    }

    public bool ChangeItemDataNum(int iItemId, int iTid, int dCount)
    {
        ItemDataBase item = new ItemDataBase();
        item.itemId = iItemId;
        item.tid = iTid;
        item.itemNum = dCount;
        return ChangeItemDataNum(item);
    }

    public bool ChangeItemDataNum(ItemDataBase itemData)
    {
        if (itemData != null)
        {
            if (mDicConsumeItemData.ContainsKey(itemData.tid))
            {
                ConsumeItemData data = mDicConsumeItemData[itemData.tid];
                data.itemNum += itemData.itemNum;
                if (data.itemNum <= 0)
                {
                    mDicConsumeItemData.Remove(itemData.tid);
                }
            }
            else
            {
                if (itemData.itemNum < 0)
                    return false;

                ConsumeItemData data = new ConsumeItemData(itemData);
                mDicConsumeItemData.Add(itemData.tid, data);
            }
        }
        return false;
    }

    public bool RemoveItemData(int tid, int iNum)
    {
        ConsumeItemData data;
        if (mDicConsumeItemData.TryGetValue(tid, out data))
        {
            if (TableCommon.GetNumberFromConsumeConfig(data.tid, "ITEM_TYPE") == (int)CONSUME_ITEM_TYPE.FUNCTIONAL)
                GameCommon.RoleChangeFunctionalProp(data.tid, -iNum);

            data.itemNum -= iNum;
            if(data.itemNum <= 0)
                mConsumeItemList.Remove(data);
            return true;
        }
        return false;
    }

	public bool RemoveConsumeItemData(int iGridIndex, int iNum)
	{
		ConsumeItemData data = mConsumeItemList[iGridIndex];
		if(data != null)
		{
			if(TableCommon.GetNumberFromConsumeConfig (data.tid, "ITEM_TYPE") == (int)CONSUME_ITEM_TYPE.FUNCTIONAL)
				GameCommon.RoleChangeFunctionalProp (data.tid, -iNum);
		
			if(iNum == data.itemNum)
				mConsumeItemList.Remove (data);
			else if(iNum < data.itemNum)
				mConsumeItemList[iGridIndex].itemNum -= iNum;
			else 
				return false;
		}
		else
			return false;

		return true;
	}

	public int GetCountByID(int ID)
	{
		int count = 0;
        foreach (KeyValuePair<int, ConsumeItemData> pair in mDicConsumeItemData)
		{
            if (pair.Value.tid == ID)
                count += pair.Value.itemNum;
		}

		return count;
	}
}

public class ConsumeItemLogicStatus : tLogicData
{
	public List<ConsumeItemStatus> mConsumeItemStatusList;

	public bool AddConsumeItemStatus(int index)
	{
		int type = TableCommon.GetNumberFromConsumeConfig (index, "ITEM_TYPE");

		if(GetItemStatusByIndex (index) != null)
		{
			GetItemStatusByIndex (index).mCountdownTime = CommonParam.NowServerTime ();

			if(type == (int)CONSUME_ITEM_TYPE.CHANGE_MODEL)
				NeedChangeCharacterConfigRecord();
		}
		else
		{
			if(type == (int)CONSUME_ITEM_TYPE.CHANGE_MODEL)
				mConsumeItemStatusList.Remove (GetItemStatusByIndex (index));
		
			ConsumeItemStatus status = new ConsumeItemStatus();
			status.mIndex = index;
			status.mCountdownTime = CommonParam.NowServerTime ();

			AddConsumeItemStatus(status);
		}

		return true;
	}

	public void AddConsumeItemStatus(ConsumeItemStatus status)
	{
		mConsumeItemStatusList.Add (status);

		NeedChangeCharacterConfigRecord();
	}


	public bool NeedChangeCharacterConfigRecord()
	{
		if(GetChangeModelIndex () != 0)
		{
			//GameCommon.ChangeCharacterConfigRecord ();
			return true;
		}
		
		return false;
	}

	public ConsumeItemStatus GetItemStatusByIndex(int index)
	{
		for(int i = 0; i < mConsumeItemStatusList.Count; i++)
		{
			if(mConsumeItemStatusList[i].mIndex == index)
				return mConsumeItemStatusList[i];
		}

		return null;
	}

	public int GetChangeModelIndex()
	{
		for(int i = 0; i < mConsumeItemStatusList.Count; i++)
		{
			Int64 time = (Int64)TableCommon.GetNumberFromConsumeConfig (mConsumeItemStatusList[i].mIndex, "ITEM_EFFECT_TIME");

			if(TableCommon.GetNumberFromConsumeConfig (mConsumeItemStatusList[i].mIndex, "ITEM_TYPE") == (int)CONSUME_ITEM_TYPE.CHANGE_MODEL 
			   && mConsumeItemStatusList[i].mCountdownTime + time > CommonParam.NowServerTime ())
				return TableCommon.GetNumberFromConsumeConfig (mConsumeItemStatusList[i].mIndex, "ITEM_PET_ID");
		}

		return 0;
	}

	public List<ConsumeItemStatus> GetBufferItemStatusList()
	{
		List<ConsumeItemStatus> buffList = new List<ConsumeItemStatus>();
		for(int i = 0; i < mConsumeItemStatusList.Count; i++)
		{
			Int64 time = (Int64)TableCommon.GetNumberFromConsumeConfig (mConsumeItemStatusList[i].mIndex, "ITEM_EFFECT_TIME");

			if(TableCommon.GetNumberFromConsumeConfig (mConsumeItemStatusList[i].mIndex, "ITEM_TYPE") == (int)CONSUME_ITEM_TYPE.BUFFER 
			   && mConsumeItemStatusList[i].mCountdownTime + time > CommonParam.NowServerTime ())
					buffList.Add (mConsumeItemStatusList[i]);
		}

		return buffList;
	}
}

public class MaterialLogicData : NetLogicData
{
    public static MaterialLogicData Self;
    public Dictionary<int, MaterialData> mDicMaterial = new Dictionary<int, MaterialData>();
    public Dictionary<string, MaterialData> itemList = new Dictionary<string, MaterialData>();

    public MaterialLogicData()
    {
        Self = this;
    }

    public bool ChangeMaterialDataNum(int iIndex, int dCount)
    {
        if (mDicMaterial.ContainsKey(iIndex))
        {
            MaterialData data = mDicMaterial[iIndex];
            data.mCount += dCount;
            if (data.mCount <= 0)
            {
                mDicMaterial.Remove(iIndex);
            }
        }
        else
        {
            if (dCount < 0)
                return false;

            MaterialData data = new MaterialData();
            data.mIndex = iIndex;
            data.mCount = dCount;
            data.mGridIndex = mDicMaterial.Count;
            mDicMaterial.Add(iIndex, data);
        }
        return true;
    }

    public MaterialData GetMaterialDataFromIndex(int iIndex)
    {
        if (mDicMaterial.ContainsKey(iIndex))
        {
            return mDicMaterial[iIndex];
        }
        return null;
    }

    public MaterialData GetMaterialDataFromGridIndex(int iGirdIndex)
    {
        foreach (KeyValuePair<int, MaterialData> pair in mDicMaterial)
        {
            if (pair.Value.mGridIndex == iGirdIndex)
                return pair.Value;
        }
        return null;
    }

    public int GetNumByIndex(int iIndex)
    {
        int iCount = 0;
        foreach (KeyValuePair<int, MaterialData> pair in mDicMaterial)
        {
            if (pair.Value.mIndex == iIndex)
            {
                iCount += pair.Value.mCount;
            }
        }
        return iCount;
    }
}

public class MaterialFragmentLogicData : NetLogicData
{
    public static MaterialFragmentLogicData Self;
    public Dictionary<int, MaterialFragmentData> mDicMaterialFragment = new Dictionary<int, MaterialFragmentData>();
    public Dictionary<string, MaterialFragmentData> itemList = new Dictionary<string, MaterialFragmentData>();

    public MaterialFragmentLogicData()
    {
        Self = this;
    }

    public bool ChangeMaterialFragmentDataNum(int iIndex, int dCount)
    {
        if (mDicMaterialFragment.ContainsKey(iIndex))
        {
            MaterialFragmentData data = mDicMaterialFragment[iIndex];
            data.mCount += dCount;
            if (data.mCount <= 0)
            {
                mDicMaterialFragment.Remove(iIndex);
            }
        }
        else
        {
            if (dCount < 0)
                return false;

            MaterialFragmentData data = new MaterialFragmentData();
            data.mFragmentIndex = iIndex;
            data.mMaterialIndex = TableCommon.GetNumberFromMaterialFragment(iIndex, "MATERIAL_INDEX");
            data.mCount = dCount;
            data.mGridIndex = mDicMaterialFragment.Count;
            mDicMaterialFragment.Add(iIndex, data);
        }
        return true;
    }

    public MaterialFragmentData GetMaterialFragmentDataFromIndex(int iIndex)
    {
        if (mDicMaterialFragment.ContainsKey(iIndex))
        {
            return mDicMaterialFragment[iIndex];
        }
        return null;
    }

    public MaterialFragmentData GetMaterialFragmentDataFromGridIndex(int iGirdIndex)
    {
        foreach (KeyValuePair<int, MaterialFragmentData> pair in mDicMaterialFragment)
        {
            if (pair.Value.mGridIndex == iGirdIndex)
                return pair.Value;
        }
        return null;
    }

    public int GetNumByIndex(int iIndex)
    {
        int iCount = 0;
        foreach (KeyValuePair<int, MaterialFragmentData> pair in mDicMaterialFragment)
        {
            if (pair.Value.mFragmentIndex == iIndex)
            {
                iCount += pair.Value.mCount;
            }
        }
        return iCount;
    }
}
//-------------------------------------------------------------------------

public class CS_RequestRoleData : BaseNetEvent
{
	public NiceData ROLE_DATA;
	public NiceData PET_DATA;

    public override bool _DoEvent()
    {
		WaitTime(20);
		return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        object d;
        if (!respEvent.getData("ROLE_DATA", out d))
        {
            Log("Erro: No response role data");
            DataCenter.SetData("InputNameWindow", "OPEN", true);
            return;
        }

        //        DataCenter.SetData("LandingWindow", "CLOSE", true);

		NiceData roleData = d as NiceData;

        string name = roleData.get("NAME");
        if (name == "")
        {
            DataCenter.SetData("InputNameWindow", "OPEN", true);
            return;
        }

        object charTable;
        if (!respEvent.getData("CHAR_DATA", out charTable))
        {
            Log("Erro: No response charTable data");
            return;
        }
        NiceTable charTableData = charTable as NiceTable;

        if (!ReadRoleFromData(roleData, charTableData))
        {
            DataCenter.SetData("InputNameWindow", "OPEN", true);
            return;
        }

        int pvpRecordCount = respEvent["PVP_RECORD_COUNT"];
        DataCenter.SetData("PVP_RECORD_SHOW_WINDOW", "RECORD_COUNT", pvpRecordCount);

        // read pet data
        ReadPetFromData(respEvent);

        // read role equip data
        object roleEquip;
        if (!respEvent.getData("FABAO_DATA", out roleEquip))
        {
            Log("Erro: No response roleEquip data");
            return;
        }
        NiceTable roleEquipData = roleEquip as NiceTable;
        ReadRoleEquipFromData(roleEquipData);

        // read pet equip data
        object petEquip;
        if (!respEvent.getData("ITEM_DATA", out petEquip))
        {
            Log("Erro: No response petEquip data");
            return;
        }
        NiceTable petEquipData = petEquip as NiceTable;
        ReadPetEquipFromData(petEquipData);

        // read gem data
        object gemd;
        if (!respEvent.getData("GEM_DATA", out gemd))
        {
            Log("Erro: No response gem data");
            return;
        }
        NiceTable gemData = gemd as NiceTable;

        ReadGemFromData(gemData);

        // read map data
        object mapd;
        if (!respEvent.getData("MAP_DATA", out mapd))
        {
            Log("Erro: No response map data");
            return;
        }
        NiceTable mapData = mapd as NiceTable;

        ReadMapFromData(mapData);

        object tujiand;
        if (!respEvent.getData("TUJIAN_DATA", out tujiand))
        {
            Log("Erro: No response tujian data");
            return; 
        }
        NiceTable tujianData = tujiand as NiceTable;

        ReadTujianFromData(tujianData);

        //object taskd;
        //if (!respEvent.getData("MISSION_DATA", out taskd))
        //{
        //    Log("Erro: No response task data");
        //    return; 
        //}
        //ReadTaskFromData(taskd as NiceTable, false);
        //
        //object dailytaskd;
        //if (!respEvent.getData("DAILY_MISSION_DATA", out dailytaskd))
        //{
        //    Log("Erro: No response daily task data");
        //    return;
        //}
        //ReadTaskFromData(dailytaskd as NiceTable, true);
        //
        //object weeklytaskd;
        //if (!respEvent.getData("WEEKLY_MISSION_DATA", out weeklytaskd))
        //{
        //    Log("Erro: No response weekly task data");
        //    return;
        //}
        //ReadTaskFromData(weeklytaskd as NiceTable, true);
        //
        //object activetaskd;
        //if (!respEvent.getData("ACTIVE_MISSION_DATA", out activetaskd))
        //{
        //    Log("Erro: No response active task data");
        //    return;
        //}
        //ReadTaskFromData(activetaskd as NiceTable, true);

//		object friendList;
//		if (!respEvent.getData("FRIEND_DATA", out friendList))
//		{
//			Log("Erro: No response active friend list data");
//			return;
//		}
//		ReadFriendListFromData(friendList as NiceTable);

		object dailySignData;
		if (!respEvent.getData("DAILY_SIGN_DATA", out dailySignData))
		{
			Log("Erro: No response daily sign data");
			return;
		}
		int iCanReceiveMaxNum = respEvent.get ("DAILY_SIGN_MAX");
		ReadDailySignFromData(dailySignData as NiceTable, iCanReceiveMaxNum);

		//shopData(get free card , first recharge)
		object shopData;
		if (!respEvent.getData("CASH_DATA", out shopData))
		{
			Log("Erro: No response shop data");
			return;
		}
		ReadShopFromData (shopData as NiceTable);

		//consume item data
		object consumeItemData;
		if (!respEvent.getData("CONSUM_DATA", out consumeItemData))
		{
			Log("Erro: No response consume data");
			return;
		}
		ReadConsumeItemFromData (consumeItemData as NiceTable);

		//consume item state
		object consumeItemStatus;
		if (!respEvent.getData("CONSUM_STATUS", out consumeItemStatus))
		{
			Log("Erro: No response consume status");
			return;
		}
		ReadConsumeItemStatusFromData (consumeItemStatus as NiceTable);

        object bonusData;
        if (!respEvent.getData("MAP_BONUS", out bonusData))
        {
            Log("Error: No response stage bonus data");
            return;
        }
        ReadStageBonusListFromData(bonusData as NiceTable);


		object onHookData;
		if (!respEvent.getData("ON_HOOK", out onHookData))
		{
			Log("Error: No response on hook data");
			return;
		}
		ReadOnHookFromData(onHookData as NiceData);

		// read pet fragment data
		object petFragment;
		if (!respEvent.getData("FRAGMENT_DATA", out petFragment))
		{
			Log("Erro: No response pet fragment data");
			return;
		}
		NiceTable petFragmentData = petFragment as NiceTable;
		ReadPetFragmentFromData(petFragmentData);

        // read material data
        object material;
        if (!respEvent.getData("MATERIAL_DATA", out material))
        {
            Log("Erro: No response material data");
            return;
        }
        NiceTable materialData = material as NiceTable;
        ReadMaterialDataFromData(materialData);

        // read material fragment data
        object materialFragment;
        if (!respEvent.getData("MATERIAL_FRAGMENT_DATA", out materialFragment))
        {
            Log("Erro: No response material fragment data");
            return;
        }
        NiceTable materialFragmentData = materialFragment as NiceTable;
        ReadMaterialFragmentFromData(materialFragmentData);

        int index = respEvent.get("GUIDE_STATE");
        //GuideManager.LoadGuideProcess(index);
        MainProcess.LoadRoleSelScene();     
    }

    // role data
    static public bool ReadRoleFromData(NiceData roleData, NiceTable charData)
    {
        bool bIsResult = false;
        if (CommonParam.bIsNetworkGame)
        {
            int count = 0;
            RoleLogicData logicData = new RoleLogicData();
            logicData.mRoleList = new RoleData[(int)CREATE_ROLE_TYPE.max];
            DataCenter.RegisterData("ROLE_DATA", logicData);
            foreach (KeyValuePair<int, DataRecord> r in charData.GetAllRecord())
            {
                DataRecord re = r.Value;
                if ((int)re.get("CHAR_MODEL") <= 0)
                    continue;
                RoleData role = new RoleData();
                role.mIndex = re.getData("CHAR_ID");
                role.level = re.getData("CHAR_LEVEL");
                role.exp = re.getData("CHAR_EXP");
                role.tid = re.getData("CHAR_MODEL");
                role.starLevel = TableCommon.GetNumberFromActiveCongfig(role.tid, "STAR_LEVEL"); //roleData.get("CHAR_1_STAR");
                role.mMaxLevelNum = TableCommon.GetNumberFromActiveCongfig(role.tid, "MAX_LEVEL");
                logicData.mRoleList[count] = role;
                count++;
            }

            logicData.AddGold(roleData.get("GOLD"));
            int nCurIndex = roleData.get("CURRENT_CHAR") - 1;
			if (nCurIndex < 0 || nCurIndex >= logicData.mRoleList.Length || logicData.mRoleList[nCurIndex] == null)
            {
                EventCenter.Log(LOG_LEVEL.ERROR, "Main role no exist, check server logic>" + nCurIndex.ToString());
                logicData.character = null;
            }
            else
				logicData.character = logicData.mRoleList[nCurIndex];


			logicData.AddDiamond(roleData.get("DIAMOND"));
            
            logicData.mStaminaMaxTime = 600;
            logicData.mStaminaTime = logicData.mStaminaMaxTime - roleData.get("POINT_TIME");
            logicData.mMaxStamina = 10;
			logicData.AddStamina(roleData.get("POINT"));
//			logicData.AddStamina(50);

			logicData.mMaxSpirit = 8000;
			logicData.AddSpirit(roleData.get("FRIEND_POINT"));
			logicData.AddHonorPoint (roleData.get ("HONOR_POINT"));

			logicData.mMaxPetNum = roleData.get("MAX_PET");
            logicData.mMaxRoleEquipNum = roleData.get("MAX_FABAO");
            logicData.mMaxPetEquipNum = roleData.get("MAX_ITEM");
			logicData.vipLevel = roleData.get("VIP_LEVEL");
			logicData.mVIPExp = roleData.get("VIP_EXP");
			logicData.name = roleData.get("NAME");
            logicData.mStrID = roleData.get("STR_ID");
            logicData.mInviteNum = roleData.get("FRIEND_INVITE_NUM");
            logicData.mMailNum = roleData.get("MAIL_NUM");
            logicData.mDBID = roleData.get("DBID");
			logicData.iconIndex = roleData.get ("ROLE_ICON_INDEX");
			logicData.mLuckyGuyMultipleIndex = roleData.get ("CASH_EXTRA_ID");
			logicData.mChatTime = roleData.get ("CHAT_TIME");

			logicData.AddFunctionalProp (ITEM_TYPE.SAODANG_POINT, roleData.get ("SAODANG_POINT"));
			logicData.AddFunctionalProp (ITEM_TYPE.RESET_POINT, roleData.get ("RESET_POINT"));
			logicData.AddFunctionalProp (ITEM_TYPE.LOCK_POINT, roleData.get ("LOCK_POINT"));

			logicData.chaLevel = roleData.get ("LEVEL");
			logicData.chaExp = roleData.get ("EXP");
			logicData.mMaxPlayerLevel = logicData.GetMaxPlayerLevel ();
			if(logicData.chaLevel > logicData.mMaxPlayerLevel) 
			{
                logicData.chaLevel = logicData.mMaxPlayerLevel;
                logicData.chaExp = 0;
			}

			//logicData.mStrName = TableCommon.GetStringFromActiveCongfig(logicData.GetMainRole().mModelIndex, "NAME");
            bIsResult = count > 0;
        }
        else
        {
            int iCount = (int)CREATE_ROLE_TYPE.max;
            RoleLogicData logicData = new RoleLogicData();
            logicData.mRoleList = new RoleData[iCount];
            DataCenter.RegisterData("ROLE_DATA", logicData);

            for (int i = 0; i < iCount; ++i)
            {
                RoleData role = new RoleData();
                logicData.mRoleList[i] = role;

                role.mIndex = i;
                role.level = UnityEngine.Random.Range(1, 20);
                role.starLevel = UnityEngine.Random.Range(1, 7);
                role.exp = UnityEngine.Random.Range(1, 20);
				role.tid = UnityEngine.Random.Range(191001, 191001);
            }

            logicData.character = logicData.mRoleList[0];

			logicData.AddGold(1000000);
			//logicData.mCurIndex = 0;
			logicData.AddDiamond(1000);
			
			logicData.mStaminaTime = 300;
			logicData.mStaminaMaxTime = 600;
			logicData.mMaxStamina = 10;
			logicData.AddStamina(50);
			logicData.mMaxSpirit = 8000;
			logicData.AddSpirit(35);
			
			logicData.mMaxPetNum = 81;
			logicData.vipLevel = 1;
			logicData.mVIPExp = 0;
			logicData.name = TableCommon.GetStringFromActiveCongfig(RoleLogicData.GetMainRole().tid, "NAME");

            bIsResult = true;
        }


        return bIsResult;
    }

    static public bool ReadRoleFromData(string text)
    {
        RoleLogicData logicData = new RoleLogicData();
        logicData.mRoleList = new RoleData[(int)CREATE_ROLE_TYPE.max];
        DataCenter.RegisterData("ROLE_DATA", logicData);

        logicData = JCode.Decode<RoleLogicData>(text);

        if (logicData.newPlayer == 1)
        {
            // create role
            //GuideManager.StartGuide();

            //need first open landing window, then creat role
            if (!DataCenter.Get("ENTER_GAME_IMMEDIATELY"))
            {
                DataCenter.CloseWindow("LOGIN_WINDOW");
                DataCenter.CloseWindow("SELECT_SERVER_WINDOW");
                DataCenter.Set("ENTER_GS", false);
                DataCenter.OpenWindow("LANDING_WINDOW", false);
                //						DataCenter.Set ("ENTER_GAME_IMMEDIATELY", false);
                //return;
            }
            //GlobalModule.ClearAllWindow();

            // 重置当前账号的新手引导本地记录
            GuideKit.SaveProgressByLocal(0);

            if (Guide.isPrologueOpened)
            {
                Guide.StartPrologue();
            }
            else 
            {
                DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "OPEN", true);
            }

            return false;
        }

        logicData.character.Init();
        logicData.chaLevel = logicData.character.level;
        CommonParam.SetServerTime(logicData.lastLoginTime);
        return true;
    }

    // pet data
    static public bool ReadPetFromData(tEvent respEvent)
    {       
        if (GameCommon.bIsLogicDataExist("PET_DATA"))
            DataCenter.Remove("PET_DATA");

        PetLogicData logicData = new PetLogicData();
        logicData.mDicPetData.Clear();
        DataCenter.RegisterData("PET_DATA", logicData);

        if (CommonParam.bIsNetworkGame)
        {
            object petd;
            if (!respEvent.getData("PET_DATA", out petd))
            {
                DEBUG.LogError("Erro: No response pet data");
                return false;
            }

            object teamd;
            if (!respEvent.getData("TEAM_DATA", out teamd))
            {
                DEBUG.LogError("Erro: No response team data");
                return false;
            }

            NiceTable petData = petd as NiceTable;
            NiceTable teamPosData = teamd as NiceTable;

			logicData.mCurrentTeam = (int)respEvent.get("TEAM_USE");
            logicData.mTeamTable = teamPosData;
            foreach (KeyValuePair<int, DataRecord> r in petData.GetAllRecord())
            {
                DataRecord re = r.Value;
                PetData pet = new PetData();
                pet.itemId = re.getData("ID");
                pet.level = re.getData("PET_LEVEL");
				pet.breakLevel = re.getData("PET_LEVEL_BREAK");
                pet.exp = re.getData("PET_EXP");
                pet.tid = re.getData("PET_ID");
                pet.mFailPoint = re.getData("FAIL_POINT");
                pet.strengthenLevel = re.getData("PET_ENHANCE");
                pet.starLevel = TableCommon.GetNumberFromActiveCongfig(pet.tid, "STAR_LEVEL");
				pet.mMaxLevelNum = TableCommon.GetNumberFromActiveCongfig(pet.tid, "MAX_LEVEL");
				pet.mMaxStrengthenLevel = TableCommon.GetNumberFromActiveCongfig(pet.tid, "MAX_GROW_LEVEL");
				pet.mMaxLevelBreakNum = TableCommon.GetNumberFromActiveCongfig(pet.tid, "MAX_LEVEL_BREAK");

                PetSkillData petSkillData = new PetSkillData();
                petSkillData.index = TableCommon.GetNumberFromActiveCongfig(pet.tid, "PET_SKILL_1");
                petSkillData.level = re.getData("SKILL_1");
                pet.mDicPetSkill.Add(0, petSkillData);

                for (int iIndex = 1; iIndex <= 2; iIndex++)
                {
                    petSkillData = new PetSkillData();
                    petSkillData.index = GameCommon.GetPetPassiveSkillIndex(pet, iIndex);
                    petSkillData.level = re.getData("SKILL_" + (iIndex + 1).ToString());
                    pet.mDicPetSkill.Add(iIndex, petSkillData);
                }

                logicData.AddItemData(pet);
            }
        }
        else
        {
            int iCount = 10;

            bool[] usePosVec = new bool[3];
            for (int i = 0; i < iCount; ++i)
            {
				PetData pet = new PetData();

                int iTag = 1;//UnityEngine.Random.Range(1, 6);
                switch (iTag)
                {
                    case 1:
						pet.tid = UnityEngine.Random.Range(191005, 191007);
                        break;
                    case 2:
						pet.tid = UnityEngine.Random.Range(192004, 192006);
                        break;
                    case 3:
						pet.tid = UnityEngine.Random.Range(193005, 193008);
                        break;
                    case 4:
						pet.tid = UnityEngine.Random.Range(194000, 194009);
                        break;
                    case 5:
						pet.tid = UnityEngine.Random.Range(195000, 195006);
                        break;
                }

				pet.itemId = i;
				pet.level = UnityEngine.Random.Range(1, 20);
				pet.breakLevel = UnityEngine.Random.Range(0, 20);
				pet.starLevel = TableCommon.GetNumberFromActiveCongfig(pet.tid, "STAR_LEVEL");
				pet.exp = UnityEngine.Random.Range(1, 20);
                //				bool b = Convert.ToBoolean(UnityEngine.Random.Range(0, 1));

				logicData.AddItemData(pet);

                logicData.mTeamTable = new NiceTable();
            }
        }
        return true;
    }

    static public bool ReadPetFromData(string text)
    {
        if (GameCommon.bIsLogicDataExist("PET_DATA"))
            DataCenter.Remove("PET_DATA");

        NetPetLogicData netLogicData = JCode.Decode<NetPetLogicData>(text);
        PetLogicData logicData = JCode.Decode<PetLogicData>(text);
        if (netLogicData != null && logicData != null)
        {
            DataCenter.RegisterData("PET_DATA", logicData);
            logicData.mDicPetData.Clear();
            foreach (KeyValuePair<string, PetData> pair in netLogicData.itemList)
            {
                PetData petData = pair.Value;
                int iItemId = Convert.ToInt32(pair.Key);
                if (!logicData.mDicPetData.ContainsKey(iItemId))
                {
                    petData.starLevel = TableCommon.GetNumberFromActiveCongfig(petData.tid, "STAR_LEVEL");
                    petData.mMaxLevelNum = TableCommon.GetNumberFromActiveCongfig(petData.tid, "MAX_LEVEL");
                    petData.mMaxStrengthenLevel = TableCommon.GetNumberFromActiveCongfig(petData.tid, "MAX_STRENGTH");
                    petData.mMaxLevelBreakNum = TableCommon.GetNumberFromActiveCongfig(petData.tid, "MAX_LEVEL_BREAK");

                    logicData.AddItemData(petData);
                }

                if (petData.inFairyland != 0 && !FairylandWindow.explore_list.Contains(petData.tid))
                {
                    FairylandWindow.explore_list.Add(petData.tid);
                }
            }
        }
      
        return true;
    }

    // role equip data
    static public bool ReadRoleEquipFromData(NiceTable equipData)
    {
        if (GameCommon.bIsLogicDataExist("EQUIP_DATA"))
            DataCenter.Remove("EQUIP_DATA");

        RoleEquipLogicData logicData = new RoleEquipLogicData();
        DataCenter.RegisterData("EQUIP_DATA", logicData);
        logicData.mDicEquip.Clear();

        if (CommonParam.bIsNetworkGame)
		{
            foreach (KeyValuePair<int, DataRecord> r in equipData.GetAllRecord())
            {
                logicData.AttachRoleEquip(r.Value);
                //itemTable->SetField("ID", FIELD_INT, 0);
                //itemTable->SetField("ITEM_ID", FIELD_INT, 1);
                //itemTable->SetField("ITEM_TYPE", FIELD_INT, 2);
                //itemTable->SetField("USER_ID", FIELD_INT, 3);
                //itemTable->SetField("ITEM_ENHANCE", FIELD_INT, 4);
                //itemTable->SetField("ITEM_STAR", FIELD_INT, 5);
                //itemTable->SetField("ITEM_ELEMENT", FIELD_INT, 6);
                //itemTable->SetField("ITEM_QUALITY", FIELD_INT, 7);
                //itemTable->SetField("EXTRA_TYPE_1", FIELD_INT, 8);
                //itemTable->SetField("EXTRA_NUM_1", FIELD_INT, 9);
                //itemTable->SetField("EXTRA_TYPE_2", FIELD_INT, 10);
                //itemTable->SetField("EXTRA_NUM_2", FIELD_INT, 11);
                //itemTable->SetField("EXTRA_TYPE_3", FIELD_INT, 12);
                //itemTable->SetField("EXTRA_NUM_3", FIELD_INT, 13);
                //itemTable->SetField("EXTRA_TYPE_4", FIELD_INT, 14);
                //itemTable->SetField("EXTRA_NUM_4", FIELD_INT, 15);
                //itemTable->SetField("EXTRA_TYPE_5", FIELD_INT, 16);
                //itemTable->SetField("EXTRA_NUM_5", FIELD_INT, 17);
                //itemTable->SetField("NEW_SIGN", FIELD_INT, 18);
            }
        }
        else
        {
            int iCount = 10;
            for (int i = 0; i < iCount; ++i)
            {
                RoleEquipData roleEquip = new RoleEquipData();

                roleEquip.tid = UnityEngine.Random.Range(1001, 1017);
                roleEquip.itemId = i;
                roleEquip.strengthenLevel = UnityEngine.Random.Range(1, 20);
				roleEquip.mStarLevel = TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "STAR_LEVEL");
				roleEquip.mMaxStrengthenLevel = TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "MAX_GROW_LEVEL");
				roleEquip.mQualityType = (EQUIP_QUALITY_TYPE)TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "QUALITY");
                roleEquip.mElementType = (ELEMENT_TYPE)UnityEngine.Random.Range(0, 5);
				roleEquip.mUserID = 0;
				roleEquip.isNew = true;
				roleEquip.mReset = 0;
                roleEquip.ApplyAffect();
                logicData.mDicEquip.Add(roleEquip.itemId, roleEquip);

            }
        }
        return true;
    }

    static public bool ReadEquipFromData(string text)
    {
        if (GameCommon.bIsLogicDataExist("EQUIP_DATA"))
            DataCenter.Remove("EQUIP_DATA");

        NetRoleEquipLogicData netLogicData = JCode.Decode<NetRoleEquipLogicData>(text);
        RoleEquipLogicData logicData = JCode.Decode<RoleEquipLogicData>(text);
        if (netLogicData != null && logicData != null)
        {
            DataCenter.RegisterData("EQUIP_DATA", logicData);
            logicData.mDicEquip.Clear();
            foreach (KeyValuePair<string, EquipData> pair in netLogicData.itemList)
            {
                int iItemId = Convert.ToInt32(pair.Key);
                if (!logicData.mDicEquip.ContainsKey(iItemId))
                {
                    logicData.AddItemData(pair.Value);
                }
            }
        }
        return true;
    }

    static public bool ReadEquipFragmentFromData(string text)
    {
        if (GameCommon.bIsLogicDataExist("EQUIP_FRAGMENT_DATA"))
            DataCenter.Remove("EQUIP_FRAGMENT_DATA");

        NetRoleEquipFragmentLogicData netLogicData = JCode.Decode<NetRoleEquipFragmentLogicData>(text);
        RoleEquipFragmentLogicData logicData = JCode.Decode<RoleEquipFragmentLogicData>(text);
        if (netLogicData != null && logicData != null)
        {
            DataCenter.RegisterData("EQUIP_FRAGMENT_DATA", logicData);
            logicData.mDicEquipFragmentData.Clear();
            foreach (KeyValuePair<string, EquipFragmentData> pair in netLogicData.itemList)
            {
                int tid = Convert.ToInt32(pair.Value.tid);
                if (!logicData.mDicEquipFragmentData.ContainsKey(tid))
                {
                    logicData.AddItemData(pair.Value);
                }
            }
        }

        return true;
    }

    // pet equip data
    public static bool ReadPetEquipFromData(NiceTable equipData)
    {
        if (GameCommon.bIsLogicDataExist("PET_EQUIP_DATA"))
            DataCenter.Remove("PET_EQUIP_DATA");

        RoleEquipLogicData logicData = new RoleEquipLogicData();
        DataCenter.RegisterData("PET_EQUIP_DATA", logicData);
        logicData.mDicEquip.Clear();

        if (CommonParam.bIsNetworkGame)
        {
            foreach (KeyValuePair<int, DataRecord> r in equipData.GetAllRecord())
            {
                DataRecord re = r.Value;
                RoleEquipData roleEquip = new RoleEquipData();
                roleEquip.itemId = re.getData("ID");
                roleEquip.tid = re.getData("PET_ID");
                roleEquip.strengthenLevel = re.getData("PET_ENHANCE");
				roleEquip.mStarLevel = TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "STAR_LEVEL");
                //roleEquip.mElementType = (ELEMENT_TYPE)(re.getData("ELEMENT_TYPE"));
                //roleEquip.mQualityType = (EQUIP_QUALITY_TYPE)(re.getData("EQUIP_QUALITY_TYPE"));
                logicData.mDicEquip.Add(roleEquip.itemId, roleEquip);
            }
        }
        else
        {
            int iCount = 10;
            for (int i = 0; i < iCount; ++i)
            {
                RoleEquipData roleEquip = new RoleEquipData();

                roleEquip.tid = UnityEngine.Random.Range(1001, 1017);
                roleEquip.itemId = i;
                roleEquip.strengthenLevel = UnityEngine.Random.Range(1, 20);
				roleEquip.mStarLevel = TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "STAR_LEVEL");
				roleEquip.mQualityType = (EQUIP_QUALITY_TYPE)TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "QUALITY");
                roleEquip.mElementType = (ELEMENT_TYPE)UnityEngine.Random.Range(0, 5);

                logicData.mDicEquip.Add(roleEquip.itemId, roleEquip);
            }
        }
        return true;
    }

    static public bool ReadMagicFromData(string text)
    {
        if (GameCommon.bIsLogicDataExist("MAGIC_DATA"))
            DataCenter.Remove("MAGIC_DATA");

        NetMagicLogicData netLogicData = JCode.Decode<NetMagicLogicData>(text);
        MagicLogicData logicData = JCode.Decode<MagicLogicData>(text);
        if (netLogicData != null && logicData != null)
        {
            DataCenter.RegisterData("MAGIC_DATA", logicData);
            logicData.mDicEquip.Clear();
            foreach (KeyValuePair<string, EquipData> pair in netLogicData.itemList)
            {
                int iItemId = Convert.ToInt32(pair.Key);
                if (!logicData.mDicEquip.ContainsKey(iItemId))
                {
                    logicData.AddItemData(pair.Value);
                }
            }
        }

        return true;
    }

    static public bool ReadMagicFragmentFromData(string text)
    {
        if (GameCommon.bIsLogicDataExist("MAGIC_FRAGMENT_DATA"))
            DataCenter.Remove("MAGIC_FRAGMENT_DATA");

        NetMagicFragmentLogicData netLogicData = JCode.Decode<NetMagicFragmentLogicData>(text);
        MagicFragmentLogicData logicData = JCode.Decode<MagicFragmentLogicData>(text);
        if (netLogicData != null && logicData != null)
        {
            DataCenter.RegisterData("MAGIC_FRAGMENT_DATA", logicData);
            logicData.mDicEquipFragmentData.Clear();
            foreach (KeyValuePair<string, EquipFragmentData> pair in netLogicData.itemList)
            {
                int tid = Convert.ToInt32(pair.Value.tid);
                if (!logicData.mDicEquipFragmentData.ContainsKey(tid))
                {
                    logicData.AddItemData(pair.Value);
                }
            }
        }

        return true;
    }

    // gem data
    static public bool ReadGemFromData(NiceTable gemData)
    {
		if (GameCommon.bIsLogicDataExist("GEM_DATA"))
			DataCenter.Remove("GEM_DATA");

        if (CommonParam.bIsNetworkGame)
        {
            int count = 0;
            GemLogicData logicData = new GemLogicData();
            int gemNum = gemData.GetAllRecord().Count;
            logicData.mGemList = new GemData[gemNum];
            DataCenter.RegisterData("GEM_DATA", logicData);

            foreach (KeyValuePair<int, DataRecord> r in gemData.GetAllRecord())
            {
                DataRecord re = r.Value;
                GemData gem = new GemData();
                gem.mType = re.getData("GEM_ID");
                gem.mCount = re.getData("GEM_NUM");
                logicData.mGemList[count] = gem;
                count++;
            }
        }
        else
        {
            int iCount = 15;
            GemLogicData logicData = new GemLogicData();
            logicData.mGemList = new GemData[iCount];
            DataCenter.RegisterData("GEM_DATA", logicData);

            for (int i = 0; i < iCount; ++i)
            {
                GemData item = new GemData();
                logicData.mGemList[i] = item;
                item.mType = i+1000;
                item.mCount = UnityEngine.Random.Range(0, 2000);
            }
        }

        return true;
    }

    static public bool ReadGemFromData(string text)
    {
        if (GameCommon.bIsLogicDataExist("GEM_DATA"))
            DataCenter.Remove("GEM_DATA");

        GemLogicData logicData = new GemLogicData();
        DataCenter.RegisterData("GEM_DATA", logicData);
        logicData.itemList.Clear();

        logicData = JCode.Decode<GemLogicData>(text);

        return true;
    }

    // map data
    static public bool ReadMapFromData(NiceTable mapData)
    {
        //if (CommonParam.bIsNetworkGame)
        //{
        //    int count = 0;
        //    MapLogicData logicData = new MapLogicData();
        //    int mapNum = mapData.GetAllRecord().Count;
        //    logicData.mMapList = new MapData[mapNum];
        //    DataCenter.RegisterData("MAP_DATA", logicData);
        //
        //    foreach (KeyValuePair<int, DataRecord> r in mapData.GetAllRecord())
        //    {
        //        DataRecord re = r.Value;
        //        MapData map = new MapData();
        //        map.battleId = re.getData("MAP_ID");
        //        map.successCount = re.getData("SUCCESS_COUNT");
        //        map.fightCount = re.getData("FIGHT_COUNT");
        //        map.bestRate = re.getData("BEST_RATE");
        //        logicData.mMapList[count] = map;
        //        count++;
        //    }
        //}
        //else
        //{
        //    List<MapData> list = new List<MapData>();
        //
        //    foreach (var pair in DataCenter.mStageTable.GetAllRecord())
        //    {
        //        if (pair.Key > 0)
        //        {
        //            MapData map = new MapData();
        //            map.battleId = pair.Value.getData("MAP_ID");
        //            map.successCount = 0;
        //            map.fightCount = 0;
        //            map.bestRate = 0;
        //            list.Add(map);
        //        }
        //    }
        //
        //    MapLogicData logicData = new MapLogicData();
        //    logicData.mMapList = list.ToArray();
        //    DataCenter.RegisterData("MAP_DATA", logicData);
        //}
        //
        return true;
    }

    //图鉴数据 
    public static bool ReadTujianFromData(NiceTable tujianData)
    {
        if (CommonParam.bIsNetworkGame)
        {
            int count = 0;
            TujianLogicData logicData = new TujianLogicData();
            int mapNum = tujianData.GetAllRecord().Count;
            logicData.mTujianList = new TujianData[mapNum];
            DataCenter.RegisterData("TUJIAN_DATA", logicData);
            foreach (KeyValuePair<int, DataRecord> r in tujianData.GetAllRecord())
            {
                DataRecord re = r.Value;
                TujianData tujian = new TujianData();
                tujian.mID = re.getData("PET_ID");
                tujian.mStatus = re.getData("PET_STATUS");
                logicData.mTujianList[count] = tujian;
                count++;
            }

			foreach (TujianData tujian in logicData.mTujianList)
			{
                if (tujian.mStatus == (int)TUJIAN_STATUS.TUJIAN_NEW)
                {
                    RoleLogicData.Self.mbHaveNewTuJian = true;
                    break;
                }
			}

            foreach (TujianData tujian in logicData.mTujianList)
            {
                if (tujian.mStatus == (int)TUJIAN_STATUS.TUJIAN_REWARD)
                {
                    RoleLogicData.Self.mbHaveAwardTuJian = true;
                    break;
                }
            }
        }
        else
        {
            int count = 2;
            TujianLogicData logicData = new TujianLogicData();
            logicData.mTujianList = new TujianData[count];
            DataCenter.RegisterData("TUJIAN_DATA", logicData);
            for (int i = 0; i < count; i++)
            {
                TujianData tujian = new TujianData();
                logicData.mTujianList[i] = tujian;
				tujian.mID = 191012+i;
                tujian.mStatus = (int)TUJIAN_STATUS.TUJIAN_NEW;
            }
        }
        return true;
    }
	
    // task data
    //public static bool ReadTaskFromData(NiceTable taskData, bool additive)
    //{
    //    if (CommonParam.bIsNetworkGame)
    //    {
    //        TaskLogicData logic;
    //        if (additive)
    //        {
    //            logic = DataCenter.GetData("TASK_DATA") as TaskLogicData;
    //        }
    //        else 
    //        {
    //            logic = new TaskLogicData();
    //            DataCenter.RegisterData("TASK_DATA", logic);
    //        }
    //
    //        int taskNum = taskData.GetAllRecord().Count;           
    //        foreach (KeyValuePair<int, DataRecord> r in taskData.GetAllRecord())
    //        {
    //            DataRecord re = r.Value;
	//			int iTaskID = re.getData("ID");
    //            int iTaskAimCount = re.getData("STEP");
	//			int iTaskState = re.getData("STATUS");
	//			TASK_STATE taskState = TASK_STATE.Had_Accept;
    //
	//			if(iTaskState == 1)
	//			{
	//				taskState = TASK_STATE.Deliver;
	//			}
	//			else
	//			{
	//				int iNeedNum = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_NUM");
    //                // Modify at 2014/8/26 by Flybird
    //                // if (iTaskAimCount >= iNeedNum)
	//				if (iTaskAimCount >= iNeedNum)
	//				{
	//					taskState = TASK_STATE.Finished;
	//				}
	//			}
    //
	//			logic.AddTaskData(iTaskID, iTaskAimCount, taskState);
    //        }
    //    }
    //    else
    //    {
    //        TaskLogicData logic;
    //        if (additive)
    //        {
    //            logic = DataCenter.GetData("TASK_DATA") as TaskLogicData;
    //        }
    //        else
    //        {
    //            logic = new TaskLogicData();
    //            DataCenter.RegisterData("TASK_DATA", logic);
    //        }
    //
    //        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mTaskConfig.GetAllRecord())
    //        {
    //            DataRecord record = pair.Value;
    //            int iTaskID = record.get("INDEX");
    //            if (iTaskID <= 0)
    //            {
    //                continue;
    //            }
    //            int iNeedNum = record.get("TASK_NUM");
    //            int iCurNum = UnityEngine.Random.Range(0, iNeedNum + 1);
    //            int iTaskAimCount = iCurNum >= iNeedNum ? iNeedNum : iCurNum;
    //            TASK_STATE taskState = (TASK_STATE)UnityEngine.Random.Range(1, 2);
    //            if (iTaskAimCount == iNeedNum)
    //            {
    //                taskState = (TASK_STATE)UnityEngine.Random.Range(2, 4);
    //            }
    //            logic.AddTaskData(iTaskID, iTaskAimCount, taskState);
    //        }
	//		//int iCount = 1;
	//		//for (int i = 0; i < iCount; i++)
	//		//{
	//		//	int iTaskID = 20001 + i;
    //        //
	//		//	int iNeedNum = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_NUM");
	//		//	int iCurNum = UnityEngine.Random.Range(0, iNeedNum);
	//		//	int iTaskAimCount = iCurNum >= iNeedNum ? iNeedNum : iCurNum;
	//		//	TASK_STATE taskState = (TASK_STATE)UnityEngine.Random.Range(1, 2);
	//		//	if (iTaskAimCount == iNeedNum)
	//		//	{
	//		//		taskState = (TASK_STATE)UnityEngine.Random.Range(2, 4);
	//		//	}
	//		//	logic.AddTaskData(iTaskID, iTaskAimCount, taskState);
	//		//}
    //    }
    //    return true;
    //}

	//DailySignData
	static public bool ReadDailySignFromData(NiceTable dailySignTable, int canReceiveMaxNum)
	{
		if(GameCommon.bIsLogicDataExist ("DAILY_SIGN_DATA"))
			DataCenter.Remove ("DAILY_SIGN_DATA");
		if(CommonParam.bIsNetworkGame)
		{
			int count = 0;
			DailySignLogicData logicData = new DailySignLogicData();
			logicData.set ("MAX_NUM", canReceiveMaxNum);
			int dataNum = dailySignTable.GetAllRecord().Count;
			logicData.mDailSignDataList = new DailySignData[dataNum];
			DataCenter.RegisterData ("DAILY_SIGN_DATA", logicData);
			foreach(KeyValuePair<int, DataRecord> r in dailySignTable.GetAllRecord ())
			{
				DataRecord re = r.Value;
				DailySignData dailySignData = new DailySignData();
				dailySignData.mID = re.getData ("DAILY_ID");
				dailySignData.mStatus = re.getData ("DAILY_STATUS");
//				dailySignData.mCanReceiveMaxNum = canReceiveMaxNum;
				logicData.mDailSignDataList[count] = dailySignData;
				count++;
			}
		}
		else
		{
			int count = 28;
			DailySignLogicData logicData = new DailySignLogicData();
			logicData.set ("MAX_NUM", canReceiveMaxNum);
			logicData.mDailSignDataList = new DailySignData[count];
			for(int i=0; i< count; i++)
			{
				DailySignData dailySignData = new DailySignData();
				dailySignData.mID = i+1001;
				if(i < 15) dailySignData.mStatus = 1;
				else dailySignData.mStatus = 0;
//				dailySignData.mCanReceiveMaxNum = canReceiveMaxNum;
				logicData.mDailSignDataList[i] = dailySignData;
			}

			DataCenter.RegisterData ("DAILY_SIGN_DATA", logicData);
		}
		return true;
	}

	//ShopData list (get free card, first recharge) 
	static public bool ReadShopFromData(NiceTable shopDataTabel)
	{
		if(GameCommon.bIsLogicDataExist ("SHOP_DATA"))
			DataCenter.Remove ("SHOP_DATA");

		if(CommonParam.bIsNetworkGame)
		{
			int count = 0;
			ShopLogicData logicData = new ShopLogicData();
			int dataNum = shopDataTabel.GetAllRecord().Count;
			logicData.mShopDataList = new ShopData[dataNum];
			DataCenter.RegisterData ("SHOP_DATA", logicData);
			foreach(KeyValuePair<int, DataRecord> r in shopDataTabel.GetAllRecord ())
			{
				DataRecord re = r.Value;
				ShopData shopData = new ShopData();
				shopData.mUsedCount = re.getData ("CASH_COUNT");
				shopData.mGetFreeCardLastTime = re.getData ("CASH_TIME");
				shopData.mIndex = re.getData ("CASH_ID");
				logicData.mShopDataList[count] = shopData;
				count++;
			}
		}
		else
		{
			int count = 16;
			ShopLogicData logicData = new ShopLogicData();
			logicData.mShopDataList = new ShopData[count];
			for(int i=0; i< count; i++)
			{
				ShopData shopData = new ShopData();
				shopData.mUsedCount = 0;
				logicData.mShopDataList[i] = shopData;
			}

			DataCenter.RegisterData ("SHOP_DATA", logicData);
		}
		return true;
	}

	//consume item list
	static public bool ReadConsumeItemFromData(NiceTable consumeItemDataTabel)
	{
		if(GameCommon.bIsLogicDataExist ("CONSUME_ITEM_DATA"))
			DataCenter.Remove ("CONSUME_ITEM_DATA");
		
		if(CommonParam.bIsNetworkGame)
		{
			int count = 0;
			ConsumeItemLogicData logicData = new ConsumeItemLogicData();
//			int dataNum = consumeItemDataTabel.GetAllRecord().Count;
			logicData.mConsumeItemList = new List<ConsumeItemData>();
			DataCenter.RegisterData ("CONSUME_ITEM_DATA", logicData);
			foreach(KeyValuePair<int, DataRecord> r in consumeItemDataTabel.GetAllRecord ())
			{
				DataRecord re = r.Value;
				ConsumeItemData consumeItemData = new ConsumeItemData();
				consumeItemData.itemNum = re.getData ("COUNT");
				consumeItemData.tid = re.getData ("CONSUM_ID");
				consumeItemData.mCountdownTime = re.getData ("CONSUM_TIME");

//				GameCommon.SplitConsumeData(ref consumeItemData, logicData);
				logicData.mConsumeItemList.Add (consumeItemData);
			}
		}
		else
		{
			int count = 16;
			ConsumeItemLogicData logicData = new ConsumeItemLogicData();
			logicData.mConsumeItemList = new List<ConsumeItemData>();
			for(int i=0; i< count; i++)
			{
				ConsumeItemData consumeItemData = new ConsumeItemData();
				consumeItemData.itemNum = 1;
				consumeItemData.tid = 1001;
				consumeItemData.mCountdownTime = 12;
				logicData.mConsumeItemList.Add (consumeItemData);
			}
			
			DataCenter.RegisterData ("CONSUME_ITEM_DATA", logicData);
		}
		return true;
	}

    static public bool ReadConsumeItemFromData(string text)
    {
        if (GameCommon.bIsLogicDataExist("CONSUME_ITEM_DATA"))
            DataCenter.Remove("CONSUME_ITEM_DATA");

        NetConsumeItemLogicData netLogicData = JCode.Decode<NetConsumeItemLogicData>(text);
        ConsumeItemLogicData logicData = JCode.Decode<ConsumeItemLogicData>(text);
        if (netLogicData != null && logicData != null)
        {
            DataCenter.RegisterData("CONSUME_ITEM_DATA", logicData);
            logicData.mDicConsumeItemData.Clear();
            foreach (KeyValuePair<string, ConsumeItemData> pair in netLogicData.itemList)
            {
                int tid = Convert.ToInt32(pair.Value.tid);
                if (!logicData.mDicConsumeItemData.ContainsKey(tid))
                {
                    logicData.AddItemData(pair.Value);
                }
            }
        }

        return true;
    }

	static public bool ReadConsumeItemStatusFromData(NiceTable consumeStatusTabel)
	{
		if(GameCommon.bIsLogicDataExist ("CONSUME_ITEM_STATUS"))
			DataCenter.Remove ("CONSUME_ITEM_STATUS");

		if(CommonParam.bIsNetworkGame)
		{
			//GameCommon.RestoreCharacterConfigRecord ();

			ConsumeItemLogicStatus logicData = new ConsumeItemLogicStatus();
			logicData.mConsumeItemStatusList = new List<ConsumeItemStatus>();
			DataCenter.RegisterData ("CONSUME_ITEM_STATUS", logicData);
			foreach(KeyValuePair<int, DataRecord> r in consumeStatusTabel.GetAllRecord ())
			{
				DataRecord re = r.Value;
				ConsumeItemStatus consumeItemData = new ConsumeItemStatus();
				consumeItemData.mIndex = re.getData ("CONSUM_ID");
				consumeItemData.mCountdownTime = re.getData ("CONSUM_TIME");

				logicData.AddConsumeItemStatus (consumeItemData);
			}
		}

		return true;
	}

    static public bool ReadStageBonusListFromData(NiceTable bonusData)
    {
        if (GameCommon.bIsLogicDataExist("STAGE_BONUS_DATA"))
            DataCenter.Remove("STAGE_BONUS_DATA");

        StageBonusLogicData logicData = new StageBonusLogicData();

        if (CommonParam.bIsNetworkGame)
        {
            foreach (var pair in bonusData.GetAllRecord())
            {
                int id = pair.Value.get("BONUS_ID");
                int status = pair.Value.get("BONUS_STATUS");

                if (status > 0)
                    logicData.mAcceptedList.Add(id);
            }            
        }
        else
        { }

        DataCenter.RegisterData("STAGE_BONUS_DATA", logicData);
        return true;       
    }

	static public bool ReadOnHookFromData(NiceData data)
	{
		if (GameCommon.bIsLogicDataExist("ON_HOOK_DATA"))
			DataCenter.Remove("ON_HOOK_DATA");
		
		OnHookLogicData logicData = new OnHookLogicData();
		
		if (CommonParam.bIsNetworkGame)
		{
			OnHookData onHook = new OnHookData();
			int x = (int)data.get("BOTTING_STATUS");
			onHook.mState = (ON_HOOK_STATE)((int)data.get("BOTTING_STATUS"));
			onHook.mLevel = data.get("BOTTING_LEVEL");
			onHook.mRemainingTime = data.get("BOTTING_TIME_REMAIN");
			onHook.mRateNum = data.get("AWARD_FACTOR");
			onHook.mRemainSpeedupToday = data.get("REMAIN_SPEEDUP_TODAY");
			logicData.mOnHook = onHook;
		}
		else
		{
			OnHookData onHook = new OnHookData();
			onHook.mState = ON_HOOK_STATE.LOCKED;
			onHook.mLevel = 1;
			onHook.mRemainingTime = 1000;
			onHook.mRateNum = 2;
			onHook.mRemainSpeedupToday = 3;
			logicData.mOnHook = onHook;
		}
		
		DataCenter.RegisterData("ON_HOOK_DATA", logicData);
		return true;
	}

    static public bool ReadPetFragmentFromData(NiceTable table)
    {
        if (GameCommon.bIsLogicDataExist("PET_FRAGMENT_DATA"))
            DataCenter.Remove("PET_FRAGMENT_DATA");

        PetFragmentLogicData logicData = new PetFragmentLogicData();
        DataCenter.RegisterData("PET_FRAGMENT_DATA", logicData);
        logicData.mDicPetFragmentData.Clear();

        return true;
    }

    static public bool ReadPetFragmentFromData(string text)
    {
        if (GameCommon.bIsLogicDataExist("PET_FRAGMENT_DATA"))
            DataCenter.Remove("PET_FRAGMENT_DATA");

        NetPetFragmentLogicData netLogicData = JCode.Decode<NetPetFragmentLogicData>(text);
        PetFragmentLogicData logicData = JCode.Decode<PetFragmentLogicData>(text);
        if (netLogicData != null && logicData != null)
        {
            DataCenter.RegisterData("PET_FRAGMENT_DATA", logicData);
            logicData.mDicPetFragmentData.Clear();
            foreach (KeyValuePair<string, PetFragmentData> pair in netLogicData.itemList)
            {
                int iItemId = Convert.ToInt32(pair.Key);
                if (!logicData.mDicPetFragmentData.ContainsKey(pair.Value.tid))
                {
                    logicData.AddItemData(pair.Value);
                }
            }
        }

        return true;
    }

    static public bool ReadMaterialDataFromData(NiceTable dataTabel)
    {
        if (GameCommon.bIsLogicDataExist("MATERIAL_DATA"))
            DataCenter.Remove("MATERIAL_DATA");

        MaterialLogicData logicData = new MaterialLogicData();
        if (CommonParam.bIsNetworkGame)
        {
            foreach (KeyValuePair<int, DataRecord> r in dataTabel.GetAllRecord())
            {
                DataRecord re = r.Value;
                MaterialData data = new MaterialData();
                data.mIndex = re.get("MATERIAL_ID");
                data.mCount = re.get("COUNT");
                logicData.mDicMaterial[data.mIndex] = data;
            }
        }
        else
        {
            int count = 20;
            for (int i = 0; i < count; i++)
            {
                MaterialData data = new MaterialData();
                data.mGridIndex = i;
                data.mIndex = 1000 + i;
                data.mCount = i + 5;
                logicData.mDicMaterial[data.mIndex] = data;
            }
        }
        DataCenter.RegisterData("MATERIAL_DATA", logicData);

        return true;
    }

    static public bool ReadMaterialDataFromData(string text)
    {
        if (GameCommon.bIsLogicDataExist("MATERIAL_DATA"))
            DataCenter.Remove("MATERIAL_DATA");

        MaterialLogicData logicData = new MaterialLogicData();
        DataCenter.RegisterData("MATERIAL_DATA", logicData);
        logicData.itemList.Clear();
        logicData = JCode.Decode<MaterialLogicData>(text);

        return true;
    }

    static public bool ReadMaterialFragmentFromData(NiceTable dataTabel)
    {
        if (GameCommon.bIsLogicDataExist("MATERIAL_FRAGMENT_DATA"))
            DataCenter.Remove("MATERIAL_FRAGMENT_DATA");

        MaterialFragmentLogicData logicData = new MaterialFragmentLogicData();
        if (CommonParam.bIsNetworkGame)
        {
            foreach (KeyValuePair<int, DataRecord> r in dataTabel.GetAllRecord())
            {
                DataRecord re = r.Value;
                MaterialFragmentData data = new MaterialFragmentData();
                data.mFragmentIndex = re.get("MATERIAL_FRAGMENT_ID");
                data.mMaterialIndex = TableCommon.GetNumberFromMaterialFragment(data.mFragmentIndex, "MATERIAL_INDEX");
                data.mCount = re.get("COUNT");
                logicData.mDicMaterialFragment[data.mFragmentIndex] = data;
            }
        }
        else
        {
            int count = 20;
            for (int i = 0; i < count; i++)
            {
                MaterialFragmentData data = new MaterialFragmentData();
                data.mGridIndex = i;
                data.mFragmentIndex = 1000 + i;
                data.mMaterialIndex = TableCommon.GetNumberFromMaterialFragment(data.mFragmentIndex, "MATERIAL_INDEX");
                data.mCount = i + 5;
                logicData.mDicMaterialFragment[data.mFragmentIndex] = data;
            }
        }
        DataCenter.RegisterData("MATERIAL_FRAGMENT_DATA", logicData);

        return true;
    }

    static public bool ReadMaterialFragmentFromData(string text)
    {
        if (GameCommon.bIsLogicDataExist("MATERIAL_FRAGMENT_DATA"))
            DataCenter.Remove("MATERIAL_FRAGMENT_DATA");

        MaterialFragmentLogicData logicData = new MaterialFragmentLogicData();
        DataCenter.RegisterData("MATERIAL_FRAGMENT_DATA", logicData);
        logicData.itemList.Clear();
        logicData = JCode.Decode<MaterialFragmentLogicData>(text);

        return true;
    }

//Friend List
//	static public bool ReadFriendListFromData(NiceTable friendData)
//	{
//		int count = 0;
//		FriendLogicData logicData = new FriendLogicData();
//		int playerNum = friendData.GetAllRecord().Count;
//		logicData.arr = new FriendData[playerNum];
//		DataCenter.RegisterData("FRIENDS_LIST", logicData);
//		foreach (KeyValuePair<int, DataRecord> r in friendData.GetAllRecord())
//		{
//			DataRecord re = r.Value;
//			FriendData role = new FriendData();
//			role.mID = re.getData("ID");
//			role.name = re.getData("NAME").ToString();
//			role.mModel = re.getData("CHAR_1_MODEL");
//			role.mLevel = re.getData("CHAR_1_LEVEL");
//			role.mDonate_time = re.getData("DONATE_TIME");
//			role.mCombat_time = re.getData("COMBAT_TIME");
//			role.mLogin_time = re.getData("LOGIN_TIME");
//			logicData.arr[count] = role;
//			count ++;
//		}
//
//		return true;
//	}


}

public class CS_SearchFriend : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int bResult = respEvt.get("RESULT");
        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
			int count = 0;
            object frienddata;
			DataCenter.Remove ("FRIEND_SEARCH_DATA");
            if (!respEvt.getData("FRIEND_LIST", out frienddata))
            {
                if (get("NAME")!="")
                {
					DataCenter.ErrorTipsLabelMessage (STRING_INDEX.ERROR_FRIEND_SEARCH_NULL);
                }
                return;
            }
//			DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "CLEAN_ALL_UI", true);

            NiceTable searchData = frienddata as NiceTable;

            FriendLogicData logicData = new FriendLogicData();
            int playerNum = searchData.GetAllRecord().Count;
            logicData.arr = new FriendData[playerNum];
            DataCenter.RegisterData("FRIEND_SEARCH_DATA", logicData);
            foreach (KeyValuePair<int, DataRecord> r in searchData.GetAllRecord())
            {
                DataRecord re = r.Value;
                FriendData role = new FriendData();
                role.mID = re.getData("ID");
                role.name = re.getData("NAME").ToString();
                role.mModel = re.getData("FRIEND_PET_ID");
                role.mLevel = re.getData("FRIEND_PET_LEVEL");
				role.mStrengthenLevel = re.getData("FRIEND_PET_ENHANCE");
                role.mTime = re.getData("LOGIN_TIME");
				logicData.arr[count] = role;
				count ++;
            }

			DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "CLEAN_ALL_UI", true);
			DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "REFRESH", true);
		}
	}
}

public class CS_RequestFriendList : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        GuideManager.Notify(GuideIndex.EnterWorldMap);
        GuideManager.Notify(GuideIndex.EnterWorldMap2);
        GuideManager.Notify(GuideIndex.EnterWorldMap3);

		int bResult = respEvt.get("RESULT");

		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			int count = 0;
			object roledata;		
			if(get ("IS_GAME_FRIEND_LIST"))
				DataCenter.Remove ("GAME_FRIEND_LIST");
			else
				DataCenter.Remove ("HELP_FRIEND_LIST");
            if (!respEvt.getData("FRIEND_LIST", out roledata))
			{
                Log("Erro: No response FRIEND_LIST");
				if(get ("IS_GAME_FRIEND_LIST"))
				{
					DataCenter.SetData ("GAME_FRIEND_WINDOW", "CLEAN_ALL_UI", true);
					DataCenter.SetData ("GAME_FRIEND_WINDOW", "REFRESH", true);
				}
				else
				{
//					DataCenter.SetData("SELECT_LEVEL_TEAM_INFO_WINDOW", "OPEN_AND_CLOSE_PLAYER", false);
					DataCenter.SetData("FRIEND_HELP_WINDOW", "REFRESH", SORT_TYPE.STAR_LEVEL);
				}
				return;
			}
			NiceTable searchData = roledata as NiceTable;
			
			FriendLogicData logicData = new FriendLogicData();
			int playerNum = searchData.GetAllRecord().Count;
			logicData.arr = new FriendData[playerNum];
			if(get("IS_GAME_FRIEND_LIST"))
				DataCenter.RegisterData("GAME_FRIEND_LIST", logicData);
			else 
				DataCenter.RegisterData("HELP_FRIEND_LIST", logicData);
			foreach (KeyValuePair<int, DataRecord> r in searchData.GetAllRecord())
			{
				DataRecord re = r.Value;
				FriendData data = new FriendData();
				data.mID = re.getData("ID");
				data.name = re.getData("NAME").ToString();
                data.mModel = re.getData("FRIEND_PET_ID");
                data.mLevel = re.getData("FRIEND_PET_LEVEL");
				data.mStrengthenLevel = re.getData("FRIEND_PET_ENHANCE");
                data.mDonateTime = re.getData("DONATE_TIME");
                data.mCombatTime = re.getData("COMBAT_TIME");
               	data.mLoginTime = re.getData("LOGIN_TIME");
				data.mIsFriend = re.getData ("IS_FRIEND");
				logicData.arr[count] = data;
				count ++;
			}
//			if(CommonParam.bIsGameFriendList)
			if(get("IS_GAME_FRIEND_LIST"))
			{
				if(get ("IS_CHAT_FRIEND_LIST"))
					DataCenter.SetData ("PRIVATE_CHAT_FRIEND_WINDOW", "REFRESH", true);
				else
				{
					DataCenter.SetData ("GAME_FRIEND_WINDOW", "CLEAN_ALL_UI", true);
					DataCenter.SetData ("GAME_FRIEND_WINDOW", "REFRESH", true);
				}
			}
			else
			{			
//				DataCenter.SetData("SELECT_LEVEL_TEAM_INFO_WINDOW", "OPEN_AND_CLOSE_PLAYER", false);
//				DataCenter.SetData("FRIEND_HELP_WINDOW", "REFRESH", SORT_TYPE.STAR_LEVEL);
			}
		}
	}
}

public class CS_RequestInviteList : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
		int bResult = respEvt.get("RESULT");
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			int count = 0;
            object frienddata;
			DataCenter.Remove ("REQUEST_INVITE_FRIEND_LIST");
            if (!respEvt.getData("FRIEND_LIST", out frienddata))
			{
				Log("Erro: No response roledata");
//				DataCenter.Remove ("REQUEST_INVITE_FRIEND_LIST");
				DataCenter.SetData ("ADD_FRIEND_WINDOW", "CLEAN_ALL_UI", true);
				DataCenter.SetData ("ADD_FRIEND_WINDOW", "UPDATE_INVITE_FRIEND_NUM", true);
				DataCenter.SetData ("ADD_FRIEND_WINDOW", "REFRESH", true);
				return;
			}

            NiceTable inviteData = frienddata as NiceTable;

            FriendLogicData logicData = new FriendLogicData();
            int playerNum = inviteData.GetAllRecord().Count;
            logicData.arr = new FriendData[playerNum];
            DataCenter.RegisterData("REQUEST_INVITE_FRIEND_LIST", logicData);
            foreach (KeyValuePair<int, DataRecord> r in inviteData.GetAllRecord())
            {
                DataRecord re = r.Value;
                FriendData role = new FriendData();
                role.mID = re.getData("ID");
                role.mailID = re.getData("MAIL_ID");
                role.name = re.getData("NAME").ToString();
                role.mModel = re.getData("FRIEND_PET_ID");
                role.mLevel = re.getData("FRIEND_PET_LEVEL");
				role.mStrengthenLevel = re.getData("FRIEND_PET_ENHANCE");
                logicData.arr[count] = role;
                count++;
            }

			DataCenter.SetData ("ADD_FRIEND_WINDOW", "CLEAN_ALL_UI", true);
			DataCenter.SetData ("ADD_FRIEND_WINDOW", "UPDATE_INVITE_FRIEND_NUM", true);
			DataCenter.SetData ("ADD_FRIEND_WINDOW", "REFRESH", true);
		}
    }
}
//-------------------------------------------------------------------------

public class CS_AddFriend : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int result = respEvt.get("RESULT");
		string strWindow = get ("WINDOW_NAME");
		if(strWindow == "PVE_ACCOUNT_WIN_WINDOW" || strWindow == "PVE_ACCOUNT_LOSE_WINDOW") return;
        switch ((STRING_INDEX)result)
		{
   	    case STRING_INDEX.ERROR_NONE:			
			if(strWindow == "FRIEND_WINDOW") DataCenter.SetData ("FIND_FRIEND_LIST_INFO_WINDOW", "ERROR_NONE", true);
			else if(strWindow == "INVITE_CODE_WINDOW") DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_INVITE_CODE_INPUT_SUCCESS);
			else if(strWindow == "PVE_ACCOUNT_WIN_WINDOW" || strWindow == "PVE_ACCOUNT_LOSE_WINDOW") return;
            else DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_FRIEND_RECOMMEND_SUCCESS);
			break;
		case STRING_INDEX.ERROR_INVALID_FRIEND:
			if(strWindow == "INVITE_CODE_WINDOW")
				DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_INVITE_CODE_INPUT_ERROR);
			break;
		default:
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
			break;
		}
	}
}

public class CS_AcceptFriend : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int result = respEvt.get("RESULT");
        switch ((STRING_INDEX)result)
        {
		case STRING_INDEX.ERROR_FRIEND_IS_FRIEND:
   		 case STRING_INDEX.ERROR_NONE:
			DataCenter.SetData ("ADD_FRIEND_WINDOW", "SUCCEED", true);
      	    break;
		default:
			DataCenter.ErrorTipsLabelMessage ((STRING_INDEX)result);
			break;
        }

//		DataCenter.SetData ("AddFriendWindow", "OPEN", true);
    }
}

//by chenliang
//begin

// public class CS_RefuseFriend : BaseNetEvent
// {
//     public override void _OnResp(tEvent respEvt)
// 	{
// 		int result = respEvt.get("RESULT");
// 		switch ((STRING_INDEX)result)
// 		{
// 		case STRING_INDEX.ERROR_NONE:
// 			DataCenter.SetData ("ADD_FRIEND_WINDOW", "REFUSE", true);
// 			break;
// 		}
//     }
// }
//-------------------
public class CS_RefuseFriend : GameServerMessage
{
    public string friendId;

    public CS_RefuseFriend():
        base()
    {
        pt = "CS_RejectFriendRequest";
    }

	public static void Send(string tmpFriendId)
    {
        CS_RefuseFriend tmp = new CS_RefuseFriend();
        tmp.friendId = tmpFriendId;
        HttpModule.Instace.SendGameServerMessage(tmp, OnRequestSuccess, OnRequestFailed);
    }
    public static void OnRequestSuccess(string text)
    {
        DataCenter.SetData ("ADD_FRIEND_WINDOW", "REFUSE", true);
    }
    public static void OnRequestFailed(string text)
    {
        //TODO
    }
}

public class CS_DeleteFriend : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int result = respEvt.get("RESULT");
        switch ((STRING_INDEX)result)
        {
 		case STRING_INDEX.ERROR_NONE:
			DataCenter.SetData("GAME_FRIEND_WINDOW", "DELETED_REFRESH", true);
        	break;
        }
    }
}

public class CS_SendSpirit : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
		int result = respEvt.get("RESULT");
		string friendID = get ("FRIEND_ID");
		switch ((STRING_INDEX)result)
		{
		case STRING_INDEX.ERROR_NONE:
			if(friendID != "")
				DataCenter.SetData ("GAME_FRIEND_WINDOW", "PRAISE_FRIEND_CD", true);
			else
			{
				DataCenter.SetData ("GAME_FRIEND_WINDOW", "NOT_GET_DATA_FROM_NET", false);
				DataCenter.OpenWindow ("FRIEND_WINDOW", true);
			}
			break;
		}
    }
}

public class CS_VisitFriend : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
		string strWindowName = get ("WINDOW_NAME");
		respEvt.set ("WINDOW_NAME", strWindowName);
		string iFriendID = get ("FRIEND_ID");
		respEvt.set ("FRIEND_ID", iFriendID);

		int result = respEvt.get("RESULT");
		switch ((STRING_INDEX)result)
		{
		case STRING_INDEX.ERROR_NONE:
                if (strWindowName == "RANK_WINDOW")
                    GlobalModule.ClearAllWindow();
				else if(strWindowName == "FRIEND_WINDOW")
					GlobalModule.ClearAllWindow();
                else
                    DataCenter.CloseWindow(strWindowName);

			DataCenter.OpenWindow ("FRIEND_VISIT_WINDOW");
			DataCenter.SetData ("FRIEND_VISIT_WINDOW", "REFRESH", respEvt);
			break;
		}
    }
}

public class CS_ZanResult : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		switch ((STRING_INDEX)result)
		{
		case STRING_INDEX.ERROR_NONE:
			DataCenter.SetData ("FRIEND_VISIT_WINDOW", "ADD_ZAN_NUM", true);
			break;
		default :
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_FRIEND_ALREADY_ZAN);
			break;
		}
	}
}
//-------------------------------------------------------------------------

public class CS_RequestMailList : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			int count = 0;
	        object maildata;
			DataCenter.Remove ("MAIL_LIST_DATA");
            if (!respEvt.getData("MAIL_LIST", out maildata))
            {
                Log("Erro: No response maildata");
				DataCenter.SetData("MAIL_WINDOW", "GET_DATA_AND_UPDATA_MAIL_NUM", true);
                DataCenter.SetData("MAIL_WINDOW", "REFRESH", true);
                return;
            }
	        NiceTable mailTable = maildata as NiceTable;
			MailLogicData logicData = new MailLogicData();
	        int mailNum = mailTable.GetAllRecord().Count;
			logicData.mails = new MailData[mailNum];
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
				logicData.mails[count] = mailData;
				count ++;
	        }
			DataCenter.SetData ("MAIL_WINDOW", "GET_DATA_AND_UPDATA_MAIL_NUM", true);
			DataCenter.SetData ("MAIL_WINDOW", "REFRESH", true);
	    }
	}
}

public class CS_ReadMail : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			DataCenter.SetData ("MAIL_WINDOW", "GET_MAIL_SUCCEND", respEvt);
		}
		else if (result == (int)STRING_INDEX.ERROR_PET_FULL)
		{
//			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_PET_BAG_FULL);
			DataCenter.SetData ("MAIL_WINDOW", "RESULT_BAG_FULL_CONTEXT", "PET_BAG");
		}
		else if (result == (int)STRING_INDEX.ERROR_FABAO_FULL)
		{
//			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_FABAO_BAG_FULL);
			DataCenter.SetData ("MAIL_WINDOW", "RESULT_BAG_FULL_CONTEXT", "RESOURCE_BAG");
		}
    }
}

public class CS_ReadAllMail : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
		bool isResources = get ("IS_RESOURCES");
        int result = respEvt.get("RESULT");
        if (result == (int)STRING_INDEX.ERROR_NONE)
        {
            object mailresult;
			int count = 0;
			int successCount = 0;
            if (!respEvt.getData("RESULT_LIST", out mailresult))
            {
                return;
            }
	        NiceTable mailresultTable = mailresult as NiceTable;
			List<DataRecord> resultList = new List<DataRecord>();
            foreach (KeyValuePair<int, DataRecord> r in mailresultTable.GetAllRecord())
            {
                DataRecord re = r.Value;
                int mailResult = re.getData("RESULT");
				//RESOURCE_ID, RESOURCE_TYPE, RESOURCE_COUNT
				if (mailResult == (int)STRING_INDEX.ERROR_NONE)
				{
					successCount ++;
					DataCenter.SetData ("MAIL_WINDOW", "GET_MAILS_SUCCESS", re);
					resultList.Add (re);
				}
				else count ++;
            }
			//other but pet
			if(isResources)
			{
				if(successCount == 0) 
				{
					if(count != 0)
						DataCenter.SetData ("MAIL_WINDOW", "RESULT_BAG_FULL_CONTEXT", "RESOURCE_BAG");
					else 
						DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_NO_RESOURCES_MAIL, true);
				}
				else 
				{
//					DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_RESOURCES_SUCCESS);
					DataCenter.SetData ("MAIL_WINDOW", "OPEN", true);

					DataCenter.SetData ("MAIL_WINDOW", "SHOW_RESULT_CONTEXT", resultList);
				}
			}
			//get pet
			else
			{
				if(successCount == 0)
				{
					if(count != 0)
					{
//						DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_PET_OR_FABAO_BAG_FULL);
						DataCenter.SetData ("MAIL_WINDOW", "RESULT_BAG_FULL_CONTEXT", "PET_BAG");
					}
					else DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_NO_PET_MAIL, true);
				}
				else
				{
//					if(count != 0) DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_GET_SOME);
//					else DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_SUCCESS);
					
					DataCenter.SetData ("MAIL_WINDOW", "OPEN", true);

					DataCenter.SetData ("MAIL_WINDOW", "SHOW_RESULT_CONTEXT", resultList);
				}
			}	
		}
    }
}


//public class CS_RequestCreateNewRole : BaseNetEvent
//{
//    public override void _OnResp(tEvent respEvt)
//    {
//        UInt64 newID = respEvt.get("NEW_KEY");
//        if (newID != 0)
//        {
//            //NiceTable temp = new NiceTable();
//            //temp.SetField("INDEX", FIELD_TYPE.FIELD_INT, 0);
//            //temp.SetField("DBKEY", FIELD_TYPE.FIELD_UINT64, 1);
//            //DataRecord r = temp.CreateRecord(0);
//            //r.set("DBKEY", newID);

//            //string mLogFileName = GameCommon.MakeGamePathFileName("__Account__.gam");
//            //temp.SaveBinary(mLogFileName);

//            tEvent requestData = Net.StartEvent("CS_RequestRoleData");
//            requestData.set("CREATE_NEW", true);
//            requestData.DoEvent();
//        }
//        //throw new System.NotImplementedException();
//        respEvt.Dump();
//    }
//}
//-------------------------------------------------------------------------

class CS_RequestChangeMainChar : BaseNetEvent
{
    public Action mAction = null;
    public override void _OnResp(tEvent respEvt)
    {
        int bResult = respEvt.get("RESULT");
        if ((STRING_INDEX)bResult != STRING_INDEX.ERROR_NONE)
        {
            Log("ERROR: change role fail");
            return;
        }

        if (mAction != null)
        {
            mAction();
            mAction = null;
        }
        
    }
}

class CS_BattleStart : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int bResult = respEvt.get("RESULT");
        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
            OnRequestSucceed();
            ObserverCenter.Notify("ON_BATTLE_REQUEST_SUCCEED");
            ObserverCenter.Clear("ON_BATTLE_REQUEST_SUCCEED");
            //EventListenerCenter.CancelListener("ON_BATTLE_REQUEST_SUCCEED");

            if (get("SKIP_LOADING") == 1)
                MainProcess.LoadBattleScene();
            else
                MainProcess.LoadBattleLoadingScene();
        }
        else
        {
            DEBUG.LogError("体力不足");
        }
    }

    private void OnRequestSucceed()
    {
        int diamondNeed = get("CURRENT_STAGE_COST");
        GameCommon.RoleChangeDiamond(-diamondNeed);
        //MapLogicData mapLogicData = DataCenter.GetData("MAP_DATA") as MapLogicData;
        //int iStageIndex = DataCenter.Get("CURRENT_STAGE");
        //mapLogicData.IncreaseFightCount(iStageIndex);
    }
}

//class CS_BattleResult : BaseNetEvent
//{
//    public override bool _DoEvent()
//    {
//        return true;
//    }
//    public override void _OnResp(tEvent respEvt)
//    {
//        int bResult = respEvt.get("RESULT");
//        int iItype = respEvt.get("TYPE");
//        int iIndex = respEvt.get("ID");
//        int iGOLD = respEvt.get("GOLD");
//        int iEXP = respEvt.get("EXP");

//        float time = get("BATTLETIME");
//        respEvt.set("BATTLETIME", time);
//        bool bIsWin = get ("ISWIN");

//        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
//        {
//            int mapID = get("MAP_ID");
//            MapData data = MapLogicData.Instance.GetMapDataByStageIndex(mapID);

//            if (data != null)
//                ++data.fightCount;

//            GameCommon.RoleChangeGold(iGOLD);
//            GameCommon.RoleChangeStamina(-1);           

//            if (bIsWin)
//            {
//                int successCount = respEvt.get("COUNT");              
//                int star = get("STAR_RATE");
//                int bestStar = respEvt.get("STAR_RATE_MAX");      

//                if(data != null)
//                {
//                    data.successCount = successCount;
//                    data.bestRate = bestStar;
//                }

//                DataCenter.OpenWindow("PVE_ACCOUNT_WIN_WINDOW", respEvt.getData());
//                GameCommon.AddPlayerLevelExpWhenPveWin();
//            }
//            else
//            {
//                DataCenter.OpenWindow("PVE_ACCOUNT_LOSE_WINDOW", respEvt.getData());
//            }

//            RoleLogicData.GetMainRole().AddExp(iEXP);
//        }
//    }
	

//    void SetCheckTaskConditionsNeedData(tEvent respEvt)
//    {
//        int bResult = respEvt.get("RESULT");
//        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
//        {
//            int stageIndex = DataCenter.Get("CURRENT_STAGE");
//            bool bIsWin = get("ISWIN");
//            float time = get("BATTLETIME");			
//            int iGOLD = respEvt.get("GOLD");
//            int iEXP = respEvt.get("EXP");
//            int iItype = respEvt.get("TYPE");
//            int iIndex = respEvt.get("ID");

//            tLogicData taskNeedData = DataCenter.GetData("TASK_NEED_DATA");
//            taskNeedData.set("MAP_ID", stageIndex);
//            taskNeedData.set("ISWIN", bIsWin);
//            taskNeedData.set("BATTLETIME", time);
//            taskNeedData.set("GOLD", iGOLD);
//            taskNeedData.set("EXP", iEXP);
//            taskNeedData.set("ITEM_TYPE", iItype);
//            taskNeedData.set("ITEM_ID", iIndex);
//            taskNeedData.set("NUM", 1);
//        }
//    }
//}

//-------------------------------------------------------------------------

class CS_RequestChangePetUsePos : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int bResult = respEvt.get("RESULT");
        bool bIsSuccess = ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE);

        if (bIsSuccess)
        {
            BagInfoWindow.isNeedRefresh = true;
            respEvt.Dump();
			string str = get("WINDOW");
			string teamWindowName = get ("TEAM_WINDOW_NAME");
			string bagWindowName = get ("BAG_WINDOW_NAME");
            switch (str)
            {
                case "TEAM_ADJUST_UNLOAD":
                case "TEAM_ADJUST_LOAD":
                    {
                        PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;
                        petLogic.mTeamTable = respEvt.getObject("TEAM_DATA") as NiceTable;
						DataCenter.SetData(teamWindowName, str, bIsSuccess);
						DataCenter.SetData(bagWindowName, str, bIsSuccess);
                    }
                    break;
                case "PET_BAG":
					Button_pet_play_flag_btn.ChangePetUsePos(bIsSuccess, get("USE_POS"), get ("PET_ID"));
                    break;
                case "SINGKE_PET_BAG":
                    Button_PetInfoSingleOKBtn.ChangePetUsePos(bIsSuccess);
                    break;
            }
        }
        else
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_ADJUST_TEAM_FAIL);
        }
    }
}

//-------------------------------------------------------------------------
class CS_RequestPetUpgrade : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int bResult = respEvt.get("RESULT");
        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
            DataCenter.SetData("PetUpgradeWindow", "UPGRADE_PET_PLAY_EFFECT", true);
    }
}

//-------------------------------------------------------------------------
class CS_RequestPetStrengthen : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int bResult = respEvt.get("RESULT");
        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
            bool iSuccess = respEvt.get("SUCCESS");
			DataCenter.SetData("PetEvolutionWindow", "STRENGTHEN_OR_EVOLUTION_PET_PLAY_EFFECT", iSuccess);
        }
		else
		{
			DataCenter.OpenMessageWindow((STRING_INDEX)bResult);
		}
    }
}

//-------------------------------------------------------------------------
class CS_RequestPetEvolution : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
		int bResult = respEvt.get("RESULT");

		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			int iPetID = respEvt.get("PET_ID");
			DataCenter.SetData("PetEvolutionWindow", "NEW_PET_MODEL_INDEX", iPetID);
			DataCenter.SetData("PetEvolutionWindow", "STRENGTHEN_OR_EVOLUTION_PET_PLAY_EFFECT", true);
        }
    }
}

class CS_RequestSalePet : BaseNetEvent
{
	public Action mAction;
	public override void _OnResp(tEvent respEvt)
	{
		int bResult = respEvt.get("RESULT");
		
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			int iPetID = get("PET_ID");

			mAction();
//			object val;			
//			bool b = getData("PET_RESULT", out val);
//			Action fun = val as Action;
//			fun();
//			DataCenter.SetData("PetEvolutionWindow", "NEW_PET_MODEL_INDEX", iPetID);
//			DataCenter.SetData("PetEvolutionWindow", "EVOLUTION_PET_OK", true);
		}
	}
}

//-------------------------------------------------------------------------
// task
class CS_RequestMissionData : BaseNetEvent
{
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvt)
    {
        // Receive Mission List
        //object taskd;
        //if (!respEvt.getData("MISSION_DATA", out taskd))
        //{
        //    Log("Error: No response task data");
        //    return;
        //}
        //CS_RequestRoleData.ReadTaskFromData(taskd as NiceTable);
        //if (!CommonParam.bIsNetworkGame)
        //{
        //    return;
        //}
        //
        //// Receive Mission List
        //object taskd;
        //if (!respEvt.getData("MISSION_DATA", out taskd))
        //{
        //    Log("Erro: No response task data");
        //    return;
        //}
        //CS_RequestRoleData.ReadTaskFromData(taskd as NiceTable, false);
        //
        //object dailytaskd;
        //if (!respEvt.getData("DAILY_MISSION_DATA", out dailytaskd))
        //{
        //    Log("Erro: No response daily task data");
        //    return;
        //}
        //CS_RequestRoleData.ReadTaskFromData(dailytaskd as NiceTable, true);
        //
        //object weeklytaskd;
        //if (!respEvt.getData("WEEKLY_MISSION_DATA", out weeklytaskd))
        //{
        //    Log("Erro: No response weekly task data");
        //    return;
        //}
        //CS_RequestRoleData.ReadTaskFromData(weeklytaskd as NiceTable, true);
        //
        //object activetaskd;
        //if (!respEvt.getData("ACTIVE_MISSION_DATA", out activetaskd))
        //{
        //    Log("Erro: No response active task data");
        //    return;
        //}
        //CS_RequestRoleData.ReadTaskFromData(activetaskd as NiceTable, true);
        /*
        NiceTable taskData = taskd as NiceTable;
        if (CommonParam.bIsNetworkGame)
        {
            TaskLogicData logic = new TaskLogicData();
            int taskNum = taskData.GetAllRecord().Count;
            DataCenter.RegisterData("TASK_DATA", logic);
            foreach (KeyValuePair<int, DataRecord> r in taskData.GetAllRecord())
            {
                DataRecord re = r.Value;
                int iTaskID = re.getData("ID");
                int iTaskAimCount = re.getData("STEP");
                int iTaskState = re.getData("STATUS");
                TASK_STATE taskState = TASK_STATE.Had_Accept;

                if (iTaskState == 1)
                {
                    taskState = TASK_STATE.Deliver;
                }
                else
                {
                    int iNeedNum = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_NUM");
                    // Modify at 2014/8/26 by Flybird
                    // if (iTaskAimCount >= iNeedNum)
                    if (iTaskAimCount >= iNeedNum)
                    {
                        taskState = TASK_STATE.Finished;
                    }
                }

                logic.AddTaskData(iTaskID, iTaskAimCount, taskState);              
            }
        }
        else
        {
            TaskLogicData logic = new TaskLogicData();
            DataCenter.RegisterData("TASK_DATA", logic);
            int iCount = 1;
            for (int i = 0; i < iCount; i++)
            {
                int iTaskID = 20001 + i;

                int iNeedNum = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_NUM");
                int iCurNum = UnityEngine.Random.Range(0, iNeedNum);
                int iTaskAimCount = iCurNum >= iNeedNum ? iNeedNum : iCurNum;
                TASK_STATE taskState = (TASK_STATE)UnityEngine.Random.Range(1, 2);
                if (iTaskAimCount == iNeedNum)
                {
                    taskState = (TASK_STATE)UnityEngine.Random.Range(2, 4);
                }
                logic.AddTaskData(iTaskID, iTaskAimCount, taskState);
            }
        }*/

        // Refresh Mission UI
        if (GameCommon.bIsLogicDataExist("MissionWindow"))
        {
            DataCenter.SetData("MissionWindow", "REFRESH", true);
        }
		else if(GameCommon.bIsLogicDataExist("ACTIVE_TASK_WINDOW"))
		{
			DataCenter.SetData ("ACTIVE_TASK_WINDOW","REQUSET_CHILD_ACTIVE_DATA", (int)get ("ACTIVE_INDEX"));
		}
    }
}


//class CS_TaskAcceptAwardResult : BaseNetEvent
//{
//    public override bool _DoEvent()
//    {
//        return true;
//    }

//    public override void _OnResp(tEvent respEvt)
//    {
//        int bResult = respEvt.get("RESULT");

//        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
//        {
//            int iTaskID = get("TASK_ID");
//            //DataCenter.SetData("MissionWindow", "AWARD_DBID", dbid);
//            DataCenter.SetData("MissionWindow", "ACCEPT_TASK_AWARD", iTaskID);

//            int iGetPlayerExp = TableCommon.GetNumberFromTaskConfig (iTaskID, "PLAYER_EXP");
//            string info = "奖励已领取";
//            if(iGetPlayerExp != 0)
//            {
//                RoleLogicData.Self.AddPlayerLevelExp (iGetPlayerExp);
//                info = "奖励已领取,\n获得玩家经验" + iGetPlayerExp.ToString ();
//            }
//            //DataCenter.OpenMessageWindow(info, true);
//            DataRecord record = DataCenter.mTaskConfig.GetRecord(iTaskID);
//            ItemData item = new ItemData() { mType = record["TASK_AWARD_TYPE"], mID = record["TASK_AWARD_ID"], mNumber = record["TASK_AWARD_NUM"] };
//            DataCenter.OpenWindow("GET_REWARDS_WINDOW", new ItemDataProvider(item));

//            tEvent evt = Net.StartEvent("CS_RequestMissionData");
//            evt.DoEvent();            
//        }
//        else if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_INVALID_MISSION)
//        { }
//        else if((STRING_INDEX)bResult == STRING_INDEX.ERROR_MISSION_HAS_DONE)        
//        { }
//        else if((STRING_INDEX)bResult == STRING_INDEX.ERROR_MISSION_LOW_STEP)
//        { }
//    }
//}

//class GC_MissionFinish : DefaultNetEvent
//{
//    public override bool _DoEvent()
//    {
//		int task_id = get("TASK_ID");
//
//        if (TableCommon.GetNumberFromTaskConfig(task_id, "DISPLAY") == 1)
//        {
//            Notification.Notify(NotifyType.Achievement, mData);
//        }
//
//        TaskLogicData taskLogicData = DataCenter.GetData("TASK_DATA") as TaskLogicData;
//
//        if (taskLogicData != null)
//            taskLogicData.SetTaskFinished(task_id);
//
//        return true;
//    }
//}

class GC_MailNum : DefaultNetEvent
{
    public override bool _DoEvent()
    {
        int mailNum = get("MAIL_NUM");
        int friendInviteNum = get("FRIEND_INVITE_NUM");

		RoleLogicData RoleData = RoleLogicData.Self;
		if(RoleData != null)
		{
			int maxMailCount = DataCenter.mGlobalConfig.GetData ("MAX_MAIL_COUNT", "VALUE");
			if(mailNum >= maxMailCount && RoleData.mInviteNum == friendInviteNum)
			{
				DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_MAIL_MORE_THAN_MAX_MAIL_COUNT);
				mailNum = maxMailCount;
			}

			RoleData.mMailNum = mailNum;
			RoleData.mInviteNum = friendInviteNum;

//			GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_ROLE_SELECT_SCENE", true);
			GameCommon.SetWindowData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_MAIL_MARK", true);
//			GameCommon.SetWindowData ("ROLE_SEL_TOP_RIGHT_GROUP", "UPDATE_ROLE_SELECT_SCENE", true);
			GameCommon.SetWindowData ("ROLE_SEL_BOTTOM_LEFT_GROUP", "UPDATE_ROLE_SELECT_SCENE", true);
		}
        return true;
    }
}


class GC_NewTujian : DefaultNetEvent
{
	public override bool _DoEvent()
	{
		RoleLogicData.Self.mbHaveNewTuJian = true;
		return true;
	}
}

class GC_AwardTujian : DefaultNetEvent
{
    public override bool _DoEvent()
    {
        RoleLogicData.Self.mbHaveAwardTuJian = true;
        return true;
    }
}

class GC_BossNum : DefaultNetEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.mHaveBossCount = get ("NUM");
		return true;
	}
}


class GC_DailySign : DefaultNetEvent
{
	public override bool _DoEvent()
	{
		int currentIndex = get ("DAILY_ID");
		DataCenter.Set ("FIRST_LANDING", currentIndex);
		return true;
	}
}

class GC_NotifyNotice : DefaultNetEvent
{
    public override bool _DoEvent()
    {
        Notification.Notify(NotifyType.Announcement, mData);
        return true;
    }
}

class GC_Exit : DefaultNetEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_NEED_QUIT_GAME_BECAUSE_LOGGED, () => Application.Quit ());
		return true;
	}
}

//DailySign
class CS_DiamondDailySign : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int bResult = respEvt.get("RESULT");
		int iCost = get ("COST");
		int iIndex = get ("DAILY_ID");
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			GameCommon.RoleChangeDiamond (iCost);
			RoleLogicData logicData = RoleLogicData.Self;
			logicData.mMailNum++;
			GameCommon.SetWindowData ("ROLE_SEL_TOP_RIGHT_GROUP", "UPDATE_ROLE_SELECT_SCENE", true);
			DataCenter.SetData ("DAILY_SIGN_WINDOW", "RESULT_SUPPLEMENTARY_SIGN", iIndex);
		}
	}
}


//-------------------------------------------------------------------------
// shop
class CS_BuyItemResult : BaseNetEvent
{
    public override bool _DoEvent()
    {
        return true;
    }
    public override void _OnResp(tEvent respEvt)
    {
        int bResult = respEvt.get("RESULT");

        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
			if(get ("IS_MYSTERIOUS"))
			{
				DataCenter.SetData ("MYSTERIOUS_SHOP_WINDOW", "BUY_RESULT", true);
				return ;
			}

			respEvt.set ("SHOP_SLOT_INDEX", (int)get("SHOP_SLOT_INDEX"));
			respEvt.set ("GRID_INDEX", (int)get("GRID_INDEX"));
			respEvt.set ("IS_FREE", (bool)get("IS_FREE"));
			DataCenter.SetData("SHOP_WINDOW", "BUY_ITEM_RESULT", respEvt);
        }
		else if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NO_ENOUGH_RESOURCE)
		{
			if(get ("IS_FREE"))
				DataCenter.OpenMessageWindow ("is not the same as the client and server data>>>>is free for getting pet");
			else
			{
				int iCostItemType = TableCommon.GetNumberFromShopSlotBase ((int)get ("SHOP_SLOT_INDEX"), "COST_ITEM_TYPE");
				if(iCostItemType == (int)ITEM_TYPE.GOLD) DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
				else if(iCostItemType == (int)ITEM_TYPE.YUANBAO) DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_SHOP_NO_ENOUGH_DIAMOND);
				else if(iCostItemType == (int)ITEM_TYPE.SPIRIT) DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_SHOP_NO_ENOUGH_FRIEND_POINT);
				else if(iCostItemType == (int)ITEM_TYPE.HONOR_POINT) DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_SHOP_NO_ENOUGH_HONOR_POINT);
			}

			DataCenter.SetData ("SHOP_WINDOW", "UI_HIDDEN", true);
		}
		else DataCenter.OpenMessageWindow ((STRING_INDEX)bResult);
    }
}

//-------------------------------------------------------------------------
// bag
public class CS_RequestPetList : BaseNetEvent
{
	public Action mAction;
	public Action mBackAction;
	public string mPvpPetWinName = "PVP_PET_BAG_WINDOW";

    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        // Receive pet List
        CS_RequestRoleData.ReadPetFromData(respEvent);

		if(mAction != null)
		{
			mAction();
			MainUIScript.Self.mWindowBackAction = mBackAction;

			if(get ("ACTION") == "FIVE_COPY_LEVEL_ACTION")
			{
				DataCenter.CloseWindow ("FIVE_COPY_LEVEL_SELECT_WINDOW");
				DataCenter.CloseWindow ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW");
			}
		}

		if(get ("IS_PET_BAG"))
		{
//      	    MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
//			MainUIScript.Self.mStrAllPetAttInfoPageWindowName = "PetInfoWindow";
		}
		else
		{
			DataCenter.SetData(mPvpPetWinName, "REFRESH", null);
//			tWindow t = DataCenter.GetData ("PVP_PET_BAG_WINDOW") as tWindow;
//			tWindow t1 = DataCenter.GetData ("PVP_ATTR_READY_PET_BAG") as tWindow;
//			if(t.IsOpen ())
//				DataCenter.SetData(mPvpPetWinName, "REFRESH", 2);
//			else if (t1.IsOpen ())
//				DataCenter.SetData(mPvpPetWinName, "REFRESH", 3);
//			else 
//				DataCenter.SetData(mPvpPetWinName, "REFRESH", 1);
		}
    }
}

class CS_RequestGem : BaseNetEvent
{
	public override bool _DoEvent()
	{
		return true;
	}
	
	public override void _OnResp(tEvent respEvent)
	{
		// read gemdata
		object gem;
		if (!respEvent.getData("GEM_DATA", out gem))
		{
			Log("Erro: No response roleEquip data");
			return;
		}
		NiceTable gemData = gem as NiceTable;
		CS_RequestRoleData.ReadGemFromData(gemData);

		DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_GEM_ICONS", true);
		DataCenter.SetData("BAG_INFO_WINDOW", "CLEAR_STONE_SELECT_STATE", true);
	}
}

public class CS_RequestConsumData : BaseNetEvent
{
	public override bool _DoEvent()
	{
		return true;
	}

	public override void _OnResp(tEvent respEvent)
	{
		RoleLogicData logicData = RoleLogicData.Self;
		logicData.mSweepNum = respEvent.get ("SAODANG_POINT");
		logicData.mResetNum = respEvent.get ("RESET_POINT");
		logicData.mLockNum = respEvent.get ("LOCK_POINT");

		// read role equip data
		object gem;
		if (!respEvent.getData("CONSUM_DATA", out gem))
		{
			Log("Erro: No response roleEquip data");
			return;
		}
		NiceTable gemData = gem as NiceTable;
		CS_RequestRoleData.ReadConsumeItemFromData(gemData);
		
//		DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_GEM_ICONS", true);
//		DataCenter.SetData("BAG_INFO_WINDOW", "CLEAR_STONE_SELECT_STATE", true);
	}
}


public class CS_RequestRoleEquip : BaseNetEvent
{
	public Action mAction = null;
	public Action mBackAction = null;
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        // read role equip data
        object roleEquip;
        if (!respEvent.getData("FABAO_DATA", out roleEquip))
        {
            Log("Erro: No response roleEquip data");
            return;
        }
        NiceTable roleEquipData = roleEquip as NiceTable;
        CS_RequestRoleData.ReadRoleEquipFromData(roleEquipData);

		mAction();
		mAction = null;
		string window_name = get ("WINDOW_NAME");
		MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = window_name;
		MainUIScript.Self.mWindowBackAction = mBackAction;

		if(get ("ACTION") == "FIVE_COPY_LEVEL_ACTION")
		{
			DataCenter.CloseWindow ("FIVE_COPY_LEVEL_SELECT_WINDOW");
			DataCenter.CloseWindow ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW");
		}

//		switch(window_name)
//		{
//		case "ROLE_INFO_WINDOW":
//			MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "ROLE_INFO_WINDOW";
////			DataCenter.OpenWindow("ROLE_INFO_WINDOW");
////        	DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS", true);
//			break;
//		case "ROLE_EQUIP_CULTIVATE_WINDOW":
//			MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "ROLE_EQUIP_CULTIVATE_WINDOW";
////			DataCenter.OpenWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
//			break;
//		}
    }
}

class CS_RequestPetEquip : BaseNetEvent
{
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        // read pet equip data
        object petEquip;
        if (!respEvent.getData("ITEM_DATA", out petEquip))
        {
            Log("Erro: No response petEquip data");
            return;
        }
        NiceTable petEquipData = petEquip as NiceTable;
        CS_RequestRoleData.ReadPetEquipFromData(petEquipData);

        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
    }
}

class CS_RequestMaterial : BaseNetEvent
{
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        // read data
        object obj;
        if (!respEvent.getData("MATERIAL_DATA", out obj))
        {
            Log("Erro: No response obj data");
            return;
        }
        NiceTable data = obj as NiceTable;
        CS_RequestRoleData.ReadMaterialDataFromData(data);
    }
}

class CS_RequestMaterialFragment : BaseNetEvent
{
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        // read data
        object obj;
        if (!respEvent.getData("MATERIAL_FRAGMENT_DATA", out obj))
        {
            Log("Erro: No response obj data");
            return;
        }
        NiceTable data = obj as NiceTable;
        CS_RequestRoleData.ReadMaterialFragmentFromData(data);
    }
}

class CS_ComposePet : BaseNetEvent
{
	public Action mAction = null;
	public override bool _DoEvent()
	{
		return true;
	}
	
	public override void _OnResp(tEvent respEvent)
	{
		if (respEvent == null)
			return;
		
		int bResult = respEvent.get("RESULT");
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			mAction();
			mAction = null;
		}
		else
		{
			DataCenter.OpenMessageWindow((STRING_INDEX)bResult);
		}
	}
}

class CS_ComposeMaterial : BaseNetEvent
{
    public Action mAction = null;
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        if (respEvent == null)
            return;

        int bResult = respEvent.get("RESULT");
        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
            mAction();
            mAction = null;
        }
        else
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_OTHER);
        }
    }
}

class CS_ComposeFabao : BaseNetEvent
{
    public Action mAction = null;
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        if (respEvent == null)
            return;

        int bResult = respEvent.get("RESULT");
        int iItemType = get("TYPE");
        int iItemIndex = get("ID");
        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
            if (iItemType == (int)ITEM_TYPE.EQUIP)
            {
                object obj = null;
                if (!respEvent.getData("FABAO_DATA", out obj))
                {
                    DEBUG.LogWarning("FABAO_DATA is null");
                    return;
                }

                NiceTable niceTable = obj as NiceTable;
                if (niceTable != null)
                {
                    RoleEquipLogicData logic = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
                    foreach (KeyValuePair<int, DataRecord> pair in niceTable.GetAllRecord())
                    {
                        logic.AttachRoleEquip(pair.Value);
                        set("ROLE_EQUIP_DB_ID", pair.Key);
                    }
                }
            }
            else if (iItemType == (int)ITEM_TYPE.MATERIAL)
            {
                MaterialLogicData materiallogicData = DataCenter.GetData("MATERIAL_DATA") as MaterialLogicData;
                materiallogicData.ChangeMaterialDataNum(iItemIndex, 1);
            }
            else if (iItemType == (int)ITEM_TYPE.MATERIAL)
            {
                MaterialFragmentLogicData materialFragmentlogicData = DataCenter.GetData("MATERIAL_FRAGMENT_DATA") as MaterialFragmentLogicData;
                materialFragmentlogicData.ChangeMaterialFragmentDataNum(iItemIndex, 1);
            }
            
            mAction();
            mAction = null;
            //GuideManager.Notify(GuideIndex.LoadRoleEquipOK);
        }
        else if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NO_ENOUGH_MONEY)
        {
            DataCenter.OpenMessageWindow((STRING_INDEX)bResult);
        }
        else
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_COMPISTION_NOT_ENOUGH);
        }
    }
}

class CS_RequestRoleEquipStrengthen : BaseNetEvent
{
	public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        if (respEvent == null)
            return;

        int bResult = respEvent.get("RESULT");
        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
			DataCenter.SetData("ExpansionInfoWindow", "STRENGTHEN_RESULT", true);
        }
		else
		{
            DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
		}
    }
}

class CS_RequestRoleEquipEvolution : BaseNetEvent
{
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        if (respEvent == null)
            return;

        int bResult = respEvent.get("RESULT");
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			// read equip data
			object equipRecord;
			if (!respEvent.getData("RESULT_TABLE", out equipRecord))
			{
				Log("Erro: No response equipRecord data");
				return;
			}
			NiceTable roleEquipData = equipRecord as NiceTable;
			
			foreach (KeyValuePair<int, DataRecord> r in roleEquipData.GetAllRecord())
			{
				DataRecord record = r.Value;
                DataCenter.SetData("ExpansionInfoWindow", "EVOLUTION_RESULT", record);
			}			
		}
    }
}

class CS_RequestRoleEquipReset : BaseNetEvent
{
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        if (respEvent == null)
            return;

        int bResult = respEvent.get("RESULT");
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			// read equip data
			object equipRecord;
			if (!respEvent.getData("RESULT_TABLE", out equipRecord))
			{
				Log("Erro: No response equipRecord data");
				return;
			}
			NiceTable roleEquipData = equipRecord as NiceTable;
			
			foreach (KeyValuePair<int, DataRecord> r in roleEquipData.GetAllRecord())
			{
				DataRecord record = r.Value;
                DataCenter.SetData("ExpansionInfoWindow", "RESET_RESULT", record);

				int iLockedNum = 0;
				RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
				EquipData equipData = roleEquipLogicData.GetEquipDataByItemId((int)record.getData ("ID"));
				if(equipData != null)
				{
					for(int i = 0; i < equipData.mAttachAttributeList.Count; i++)
					{
						bool bIsLocked = (bool)get ("INDEX_" + (i + 1).ToString());
						equipData.mAttachAttributeList[i].mLockState = bIsLocked ? LOCK_STATE.LOCKED : LOCK_STATE.UNLOCK;

						if(bIsLocked)
							iLockedNum++;
					}
				}
				DataCenter.SetData("ExpansionInfoWindow", "UPDATE_CUR_SEL_EQUIP_DATA", (int)record.getData ("ID"));
				RoleLogicData.Self.mLockNum -= iLockedNum;

				RoleEquipCultivateWindow window = DataCenter.GetData("ROLE_EQUIP_CULTIVATE_WINDOW") as RoleEquipCultivateWindow;
				if(window !=null)
				{
					UILabel lockTicketNumLabel = GameCommon.FindObject(window.mRoleEquipInfoGroup, "lock_ticket_num").GetComponent<UILabel>();
					lockTicketNumLabel.text = (Convert.ToInt32(lockTicketNumLabel.text) - iLockedNum).ToString();
				}
			}			
		}
    }
}

class CS_RequestRoleEquipUse : BaseNetEvent
{
    public Action mAction = null;
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        if (respEvent == null)
            return;

        int bResult = respEvent.get("RESULT");
        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
            mAction();
            mAction = null;
            GuideManager.Notify(GuideIndex.LoadRoleEquipOK);
        }
    }
}

class CS_RequestRoleEquipSale : BaseNetEvent
{
	public Action mAction = null;
    public override bool _DoEvent()
    {
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        if (respEvent == null)
            return;

        int bResult = respEvent.get("RESULT");
        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
			mAction();
			mAction = null;            
        }
		else
		{
			DataCenter.OpenMessageWindow((STRING_INDEX)bResult);
		}
    }
}

public class CS_SetTeam : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		if (respEvt == null)
			return;

		int bResult = respEvt.get("RESULT");
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			PetLogicData petLogicData = DataCenter.GetData ("PET_DATA") as PetLogicData;
			int iWhichTeam = get ("TEAM_ID");
			petLogicData.mCurrentTeam = iWhichTeam;
			string strWindowName = get ("WINDOW_NAME");
			DataCenter.SetData (strWindowName, "TEAM_REFRESH", true);

		}
		else
		{
			DataCenter.OpenMessageWindow((STRING_INDEX)bResult);
		}
	}
}

// tujian
public class CS_RequestTujian : BaseNetEvent
{
	public override void _OnResp(tEvent respEvent)
	{
		if (respEvent == null)
			return;

		// read role tujian data
		object tujiand;
		if (!respEvent.getData("TUJIAN_DATA", out tujiand))
		{
			Log("Erro: No response tujian data");
			return;
		}
		NiceTable tujianData = tujiand as NiceTable;
		
		CS_RequestRoleData.ReadTujianFromData(tujianData);
	}
}

public class CS_TujianReward : BaseNetEvent
{
	public override void _OnResp(tEvent respEvent)
	{
		if (respEvent == null)
			return;

		bool bIsSccuess = (bool)respEvent.get("RESULT");
		int bResult = Convert.ToInt32(bIsSccuess);
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			DataCenter.CloseWindow("PET_INFO_SINGLE_WINDOW");
			DataCenter.SetData("ILLUSTRATED_HANDBOOK_WINDOW", "REFRESH_GRID_ITEM_INFO", (int)TUJIAN_STATUS.TUJIAN_FULL);

			RoleLogicData.Self.AddDiamond(10);
			DataCenter.SetData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND", true);

            ItemData item = new ItemData() { mType = (int)ITEM_TYPE.YUANBAO, mNumber = 10 };
            DataCenter.OpenWindow("GET_REWARDS_WINDOW", new ItemDataProvider(item));
		}
		else
		{
			DataCenter.OpenMessageWindow((STRING_INDEX)bResult);
		}
	}
}

public class CS_AcceptStageBonus : BaseNetEvent
{
    public override void _OnResp(tEvent respEvent)
    {
        if (respEvent == null)
            return;

        int bResult = respEvent.get("RESULT");

        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
            int bonusIndex = get("INDEX");
            DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "ACCEPT_BONUS", bonusIndex);
        }

        DataCenter.CloseWindow("WORLD_MAP_BONUS_WINDOW");
    }
}

public class CS_RequestIdleBottingStatus : BaseNetEvent
{
	public override void _OnResp(tEvent respEvent)
	{
		if (respEvent == null)
			return;
		
		int bResult = respEvent.get("RESULT");
		
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
			int x = (int)respEvent.get("BOTTING_STATUS");
			logicData.mOnHook.mState = (ON_HOOK_STATE)((int)respEvent.get("BOTTING_STATUS"));
			logicData.mOnHook.mLevel = respEvent.get("BOTTING_LEVEL");
			logicData.mOnHook.mRemainingTime = respEvent.get("BOTTING_TIME_REMAIN");
			logicData.mOnHook.mRateNum = respEvent.get("AWARD_FACTOR");
			logicData.mOnHook.mRemainSpeedupToday = respEvent.get("REMAIN_SPEEDUP_TODAY");

			string strWindowName = get("WINDOW_NAME");
			if(strWindowName != "")
				DataCenter.OpenWindow(strWindowName);
			DataCenter.SetData("ROLE_SEL_TOP_LEFT_GROUP", "SET_ON_HOOK_REMAIN_TIME", true);
		}
	}
}

public class CS_RequestIdleBottingBegin : BaseNetEvent
{
	public override void _OnResp(tEvent respEvent)
	{
		if (respEvent == null)
			return;
		
		int bResult = respEvent.get("RESULT");
		
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			Int64 remainTime = respEvent.get("BOTTING_TIME_REMAIN");

			OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
			logicData.mOnHook.mRemainingTime = remainTime;
			logicData.mOnHook.mState = ON_HOOK_STATE.DOING;

			Logic.EventCenter.Start("Button_on_hook_btn").DoEvent();
		}
	}
}

public class CS_RequestIdleBottingSpeedUp : BaseNetEvent
{
	public override void _OnResp(tEvent respEvent)
	{
		if (respEvent == null)
			return;
		
		int bResult = respEvent.get("RESULT");
		
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			int iDiamondCost = respEvent.get("DIAMOND_COST");
			GameCommon.RoleChangeDiamond((-1) * iDiamondCost);

			DataCenter.CloseWindow("ON_HOOK_SPEEDUP_WINDOW");

			Logic.EventCenter.Start("Button_on_hook_btn").DoEvent();

			Logic.EventCenter.Start("Button_on_hook_award_button").DoEvent();
		}
	}
}

public class CS_RequestIdleBottingAward : BaseNetEvent
{
	public override void _OnResp(tEvent respEvent)
	{
		if (respEvent == null)
			return;
		
		int bResult = respEvent.get("RESULT");
		
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			NiceTable awardTable = respEvent.getTable("AWARD_TABLE");

            //DataCenter.OpenWindow("ON_HOOK_AWARD_WINDOW");
            //DataCenter.SetData("ON_HOOK_AWARD_WINDOW", "REFRESH", respEvent);     
            ItemDataProvider provider = new ItemDataProvider(awardTable, "ITEM_TYPE", "ITEM_ID", "ITEM_COUNT", "ITEM_ELEMENT");
            GameCommon.RoleChangeItem(provider);
            DataCenter.OpenWindow("GET_REWARDS_WINDOW", provider);
            Logic.EventCenter.Start("Button_on_hook_btn").DoEvent();
		}
	}
}


public class CS_RequestAnouncement : BaseNetEvent
{
    public override void _OnResp(tEvent respEvent)
    {
        if (respEvent == null)
            return;

        int bResult = respEvent.get("RESULT");

        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
            NiceTable boardTable = respEvent.getTable("ANNOUNCEMENT");
            DataCenter.SetData("BOARD_WINDOW", "REFRESH_DATA", boardTable);
        }
    }
}


public class CS_RequestSaoDang : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int bResult = respEvt.get("RESULT");
        int iItype = respEvt.get("TYPE");
        int iIndex = respEvt.get("ID");
        int iGOLD = respEvt.get("GOLD");
        int iEXP = respEvt.get("EXP");
        int successCount = respEvt.get("COUNT");
        int costDiamond = respEvt.get("COST_DIAMOND");
        int costSaoDang = respEvt.get("COST_SAODANG");

        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
        {
            DataCenter.CloseWindow("STAGE_INFO_WINDOW");
            int mapID = get("MAP_ID");
            MapData data = MapLogicData.Instance.GetMapDataByStageIndex(mapID);

            if (data != null)
            {
                ++data.fightCount;
                data.successCount = successCount;
            }

            GameCommon.RoleChangeGold(iGOLD);
            GameCommon.RoleChangeStamina(-1);
            GameCommon.RoleChangeDiamond(-costDiamond);
          
            DataCenter.OpenWindow("PVE_ACCOUNT_CLEAN_WINDOW", respEvt.getData());

            RoleLogicData.GetMainRole().AddExp(iEXP);
			GameCommon.AddPlayerLevelExpWhenPveWin();
        }
        else if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_SAODANG_NO_ENOUGH_DIAMON)
        {
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NO_ENOUGH_DIAMOND);
        }
        else
        {
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SAODANG_INVALID);
        }
    }
}

class CS_RequestFragmentData : BaseNetEvent
{
	public override void _OnResp(tEvent respEvent)
	{
		// read pet fragment data
		object petFragment;
		if (!respEvent.getData("FRAGMENT_DATA", out petFragment))
		{
			Log("Erro: No response fragment data");
			return;
		}
		NiceTable petFragmentData = petFragment as NiceTable;
		CS_RequestRoleData.ReadPetFragmentFromData(petFragmentData);
	}
}

public class CS_RequestFightPowerRank : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        if (respEvt != null)
        {
            DataCenter.SetData("RANK_WINDOW", "REFRESH_ROLE_FIGHT_RANK", respEvt.getData());
        }
    }
}

public class CS_RequestMyFightPowerRank : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        if (respEvt != null)
        {
            DataCenter.SetData("RANK_WINDOW", "REFRESH_MY_ROLE_FIGHT_RANK", respEvt.getData());
        }
    }
}


public class CS_RequestGuideProcess : BaseNetEvent
{
    public override void _OnResp(tEvent respEvent)
    {
        int netProc = respEvent.get("GUIDE_STATE");
        return;
    }
}


public class CS_SaveGuideProcess : BaseNetEvent
{
    public override bool DoEvent()
    {
        bool result = base.DoEvent();
        Net.StopWaitEffect();
        Net.mLastSendEvent = null;
        return result;
    }

    public override void _OnResp(tEvent respEvent)
    {
        return;
    }

    public override void DoOverTime()
    {
        Finish();
    }
}
/// <summary>
/// 每日签到
/// </summary>
public class ActivitySignData
{
	public int mIndex = 0;
}
/// <summary>
/// 每日签到
/// </summary>
public class ActivitySignLogicData : tLogicData
{
	static public ActivitySignLogicData Self;
	public int mCurIndex; // 当前索引
    public bool mHasSigned; // 是否已签到
	private Dictionary<int, ActivitySignData> mDicActivitySign;
	
	public ActivitySignLogicData()
	{
		Self = this;
		mDicActivitySign = new Dictionary<int, ActivitySignData>();
	}
	
	public void Init(int iIndex, int isSign)
	{
		mCurIndex = iIndex;
        mHasSigned = isSign == 1;
		int iTotalCount = mCurIndex;
		mDicActivitySign.Clear();

		for (int i = 1; i < iTotalCount; i++)
		{
			AddItem(i);
		}
	}
	
	/// <summary>
	/// 每日签到
	/// </summary>
	/// <param name="iIndex">被激活的每日签到索引</param>
	/// <returns></returns>
	public bool AddItem(int iIndex)
	{
		if (mDicActivitySign.Count < iIndex)
		{			
			mCurIndex = iIndex + 1;
			return true;
		}
		return false;
	}
}

/// <summary>
/// 次日登陆请求
/// </summary>
public class MorrowLandData
{
	public int dayIndex = 0; //
	public bool isReward = false;
}
/// <summary>
/// 次日登陆请求
/// </summary>
public class MorrowLandLogicData : tLogicData
{
	static private MorrowLandLogicData instance;

	public MorrowLandData[] dayRewardItem = new MorrowLandData[1];
//	public int iLeftTime = 0;
//	public long iCountDown = 0;
	private Dictionary<int, MorrowLandData> mMorrowLand;
	
	public MorrowLandLogicData()
	{
		for(int i =0; i < dayRewardItem.Length; i++)
		{
			dayRewardItem[i] = new MorrowLandData();
			dayRewardItem[i].dayIndex = i;
			dayRewardItem[i].isReward = false;
		}
		mMorrowLand = new Dictionary<int, MorrowLandData>();
	}

    public static MorrowLandLogicData Self
    {
        get
        {
            if (instance == null)
                instance = new MorrowLandLogicData();
            return instance;
        }
    }
	
	public override void Init(string text)
	{
		SC_LoginRewardQuery item = JCode.Decode<SC_LoginRewardQuery>(text);
        //CommonParam.mOpenMorrowLand = item.open;
		List<int> dayList = item.dayArr.ToList();
		for(int i = 0; i < dayRewardItem.Length; i++)
		{
			dayRewardItem[i].dayIndex = i + 1;
			dayRewardItem[i].isReward = dayList.IndexOf(i + 1) != -1;
		}

//		iLeftTime = item.leftTime;
//
//		if(iLeftTime != 0)
//		{
//			iCountDown = (CommonParam.NowServerTime() + iLeftTime);
//			RoleSelTopRightWindow tmpWin = DataCenter.GetData("ROLE_SEL_TOP_RIGHT_GROUP") as RoleSelTopRightWindow ;
//			if(tmpWin != null)
//			{
//				tmpWin.SetTime ();
//			}
//			if(iCountDown <= 0)
//			{
//				iCountDown = 0;
//			}
//		}else
//		{
//			iCountDown = 0;
//		}

		mMorrowLand.Clear();
	}
}

/*
/// <summary>
/// 获得开服狂欢列表
/// </summary>
public class RevelryLogicData : tLogicData
{
	static private RevelryLogicData instance;
	
	public List<RevelryObject> revelryObject;
	public int iLeftTime;
	public int iDayIndex = 0;
	public const int DAY_NUM = 7;
	
	public RevelryLogicData()
	{
		iLeftTime = 0;
		revelryObject = new List<RevelryObject>();

	}
	
	public static RevelryLogicData Self
	{
		get
		{
			if (instance == null)
				instance = new RevelryLogicData();
			return instance;
		}
	}
	
//	public override void Init(string text)
//	{
//		SC_GetRevelryList item = JCode.Decode<SC_GetRevelryList>(text);
//		List<RevelryObject> dayList = item.revelryArr.ToList();
//
//		revelryObject = dayList;
//		for(int i = 0; i < revelryObject.Count; i++)
//		{
//			revelryObject[i].revelryId = dayList[i].revelryId;
//			revelryObject[i].progress = dayList[i].progress;
//			revelryObject[i].accepted = dayList[i].accepted;
//		}
////		iLeftTime = item.leftTime;
//		iLeftTime = 140;
//		if(iLeftTime != 0)
//		{
//			iDayIndex = DAY_NUM - iLeftTime / (24 * 60 * 60);
//			bool flag=true;
//
//			if(flag)
//			{
//				vp_Timer.In(RevelryLogicData.Self.iLeftTime,TimeIsOver);
//				flag=false;
//			}
//		}
//	}
//	public  void TimeIsOver()
//	{
//		GlobalModule.DoCoroutine(_DoAction());	
//	}
//	private IEnumerator _DoAction()
//	{
//		yield return NetManager.StartWaitMorrowLandQuery();
//	}

	public bool haveRewards()
	{
		bool isCan = true;
		int i = 0;
		foreach(var v in revelryObject)
		{
			int iDay = TableCommon.GetNumberFromHDrevelry (v.revelryId, "DAY");
			if(v.accepted == true && iDay <= iDayIndex )
			{
				i ++;
			}
		}
		if(i > 0)
			isCan = true;
		else 
			isCan = false;
		return isCan;
	}
}
*/

//泡泡的判断触发条件，配置格式
public class TipsInfoBase
{
	public int iType = -1;			// 条件类型id
	public int iMinNum;				// 条件类型数值1
	public int iMaxNum; 			// 条件类型数值2

}