using System;
using System.Collections.Generic;
using UnityEngine;
using DataTable;


public class StageProperty
{
    public int mIndex = 0;
    public int mFrontIndex = 0;
    public int mPassCount = -1;     // -1: Lock;        0: Unlock but not pass;     1,2...: Unlock and pass
    public int mFightCount = 0;
    public int mFightLimit = 0;      // 0: Unlimit;     1,2...: Max fight limit
    public int mFightCost = 0;
    public int mBestStar = 0;
    public StageOpenDateTime mOpenTime;

    public static StageProperty Create(int stageIndex)
    {
        DataRecord record = DataCenter.mStageTable.GetRecord(stageIndex);

        if (record != null)
            return Create(record);

        return null;
    }

    public static StageProperty Create(DataRecord record)
    {      
        StageProperty stageProperty = new StageProperty();
        int index = record.get("INDEX");
        int frontIndex = record.get("FRONT_INDEX");
        MapData data = MapLogicData.Instance.GetMapDataByStageIndex(index);
        MapData frontData = MapLogicData.Instance.GetMapDataByStageIndex(frontIndex);

        stageProperty.mIndex = index;
        stageProperty.mFrontIndex = frontIndex;
        stageProperty.mFightCount = data == null ? 0 : data.fightCount;
        stageProperty.mBestStar = data == null ? 0 : data.bestRate;
        //by chenliang
        //begin

//        stageProperty.mFightLimit = record.get("STAGELIMIT");
//-----------------
        //改为根据VIP表设置上限
        if (IsActiveStage(index))
        {
            stageProperty.mFightLimit = (int)VIPHelper.GetCurrVIPValueByField(VIP_CONFIG_FIELD.DAILY_TASK_RESET_COUNT);
        }
        else 
        {
            stageProperty.mFightLimit = record.get("STAGELIMIT");
        }

        //end
        stageProperty.mOpenTime = new StageOpenDateTime(record);

        if (data != null && data.successCount > 0)
            stageProperty.mPassCount = data.successCount;
        else if (frontData != null && frontData.successCount > 0)
            stageProperty.mPassCount = 0;
        else
            stageProperty.mPassCount = frontIndex == 0 ? 0 : -1;

        if (stageProperty.mPassCount < stageProperty.mFightLimit)
        {
            stageProperty.mFightCost = record.get("EVENT_OPEN_COST");
        }
        else if (stageProperty.mPassCount == stageProperty.mFightLimit)
        {
            stageProperty.mFightCost = record.get("EVENT_EXTRA_COST");
        }
        else
        {
            stageProperty.mFightCost = 0;
        }

        return stageProperty;
    }

    public static int ComparePriority(StageProperty lhs, StageProperty rhs)
    {
        if (lhs.mPassCount == 0)
        {
            if (rhs.mPassCount == 0)
                return lhs.mIndex - rhs.mIndex;
            else
                return -1;
        }
        else if (lhs.mPassCount > 0)
        {
            if (rhs.mPassCount == 0)
                return 1;
            else if (rhs.mPassCount > 0)
                return rhs.mIndex - lhs.mIndex;
            else
                return -1;
        }
        else
        {
            if (rhs.mIndex >= 0)
                return 1;
            else
                return lhs.mIndex - rhs.mIndex;
        }
    }

    // if now is in active duration, return closing time (total seconds calculate by ServerTime)
    // else return -1;
    // argument "month" values 1 to 12 means "January" to "December", 0 means any month
    // argument "dayOfWeek" valus 1 to 7 means "Monday" to "Sunday", 0 means any day of week
    public static Int64 GetClosingTime(int month, int dayOfWeek, int startHour, int startMinute, int endHour, int endMinute)
    {
        DateTime now = GameCommon.NowDateTime();
        DayOfWeek nowDayOfWeek = now.DayOfWeek;
        int iNowDayOfWeek = nowDayOfWeek == DayOfWeek.Sunday ? 7 : (int)nowDayOfWeek;
        int iNowMonth = now.Month;

        if (month != 0 && month != iNowMonth)
            return -1;
        if (dayOfWeek != 0 && dayOfWeek != iNowDayOfWeek)
            return -1;

        DateTime startTime;
        DateTime endTime;

        if (startHour == 0 && startMinute == 0 && endHour == 24 && endMinute == 0)
        {
            if (dayOfWeek == 0)  // whole month
            {
                if (month == 0) // whole year
                {
                    startTime = new DateTime(now.Year, 1, 1);
                    endTime = new DateTime(now.Year + 1, 1, 1);
                }
                else // whole month
                {
                    startTime = new DateTime(now.Year, now.Month, 1);
                    if (now.Month == 12)
                        endTime = new DateTime(now.Year + 1, 1, 1);
                    else
                        endTime = new DateTime(now.Year, now.Month + 1, 1);
                }
            }
            else  // whole day
            {
                startTime = new DateTime(now.Year, now.Month, now.Day);
                endTime = startTime + TimeSpan.FromDays(1);
            }
        }
        else
        {
            startTime = new DateTime(now.Year, now.Month, now.Day, startHour, startMinute, 0);
            endTime = new DateTime(now.Year, now.Month, now.Day, endHour, endMinute, 0);
        }

        Int64 startSeconds = GameCommon.DateTime2TotalSeconds(startTime);
        Int64 endSeconds = GameCommon.DateTime2TotalSeconds(endTime);
        Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(now);

        if (nowSeconds < startSeconds || nowSeconds > endSeconds)
            return -1;
        else
            return endSeconds;
    }

    static public STAGE_TYPE GetStageType(int index)
    {
        int t = TableCommon.GetNumberFromStageConfig(index, "TYPE");
        return (STAGE_TYPE)t;
    }

    // 关卡体力消耗
    static public int GetStageStaminaCost(int index)
    {
        STAGE_TYPE t = GetStageType(index);

        if (IsMainStage(t))
        {
            return DataCenter.mEnergyCost.GetData((int)t, "ENERGY_COST");
        }
        else if (IsActiveStage(t))
        {
            return 1;
        }

        return 0;
    }

    static public int GetCurrentStageStaminaCost()
    {
        int stageIndex = DataCenter.Get("CURRENT_STAGE");
        return GetStageStaminaCost(stageIndex);
    }

    static public bool IsMainStage(int index)
    {
        return IsMainStage(GetStageType(index));
    }

    static public bool IsActiveStage(int index)
    {
        return IsActiveStage(GetStageType(index));
    }

    static public bool IsMainStage(STAGE_TYPE t)
    {
        return t == STAGE_TYPE.MAIN_COMMON || t == STAGE_TYPE.MAIN_ELITE || t == STAGE_TYPE.MAIN_MASTER;
    }

    static public bool IsActiveStage(STAGE_TYPE t)
    {
        return t == STAGE_TYPE.EXP || t == STAGE_TYPE.MONEY || t == STAGE_TYPE.TOWER;
    }

    //static public ELEMENT_TYPE GetStageElement(int index)
    //{
    //    if (IsMainStage(index))
    //    {
    //        string strType = index.ToString().Substring(1, 1);
    //        int iType = int.Parse(strType);

    //        if (iType >= 1 && iType <= 5)
    //            return (ELEMENT_TYPE)(iType - 1);
    //    }

    //    return ELEMENT_TYPE.MAX;
    //}

    //static public int GetStageDifficulty(int index)
    //{
    //    if (IsMainStage(index))
    //    {
    //        string strDifficuty = index.ToString().Substring(2, 1);
    //        int iDifficulty = int.Parse(strDifficuty);
    //        if (iDifficulty >= 1 && iDifficulty <= 3)
    //        {
    //            return iDifficulty;
    //        }
    //    }
    //    return 0;
    //}

    //static public int GetStageLevel(int index)
    //{
    //    return index % 100;
    //}

    static public STAGE_TYPE GetCurrentStageType()
    {
        int stageIndex = DataCenter.Get("CURRENT_STAGE");
        return GetStageType(stageIndex);
    }

    static public bool IsCurrentMainStage()
    {
        return IsMainStage(GetCurrentStageType());
    }

    static public bool IsCurrentActiveStage()
    {
        return IsActiveStage(GetCurrentStageType());
    }

    static public bool IsFirstFight(int stageIndex)
    {
        StageProperty property = StageProperty.Create(stageIndex);
        return property != null && property.mFightCount == 0;
    }

    public RequestBattleResult GetRequestFightResult()
    {
        if (mFightLimit > 0 && mPassCount >= mFightLimit && mFightCost == 0)
        {
            return RequestBattleResult.NO_CHANCE;
        }
        else if (!openForever && closingTime < 0)
        {
            return RequestBattleResult.OVER_TIME;
        }
        else
        {
            if (RoleLogicData.Self.stamina == 0)
            {
                return RequestBattleResult.NO_ENOUGH_STAMINA;
            }
            else if (mFightCost > 0)
            {
                if (RoleLogicData.Self.diamond < mFightCost)
                {
                    if (mPassCount == mFightLimit)
                    {
                        return RequestBattleResult.NO_ENOUGH_COST_FOR_EXTRA;
                    }
                    else
                    {
                        return RequestBattleResult.NO_ENOUGH_COST_FOR_COMMON;
                    }
                }
                else
                {
                    if (mPassCount == mFightLimit)
                    {
                        return RequestBattleResult.NEED_COST_FOR_EXTRA;
                    }
                    else
                    {
                        return RequestBattleResult.NEED_COST_FOR_COMMON;
                    }
                }
            }
            else
            {
                return RequestBattleResult.ACCEPT;
            }
        }
    }

    public STAGE_TYPE stageType
    {
        get { return GetStageType(mIndex); }
    }

    //public ELEMENT_TYPE stageElement
    //{
    //    get { return GetStageElement(mIndex); }
    //}
    //
    //public int difficuty
    //{
    //    get { return GetStageDifficulty(mIndex); }
    //}

    //public int level
    //{
    //    get { return GetStageLevel(mIndex); }
    //}
    //
    public bool openForever
    {
        get { return mOpenTime.mOpenForever; }
    }

    public Int64 closingTime
    {
        get { return GetClosingTime(mOpenTime.mMonth, mOpenTime.mDayOfWeek, mOpenTime.mStartHour, mOpenTime.mStartMinute, mOpenTime.mEndHour, mOpenTime.mEndMinute); }
    }

    public bool visible
    {
        get { return openForever || closingTime >= 0; }
    }

    public bool unlocked
    {
        get { return visible && mPassCount >= 0; }
    }

    public bool passed
    {
        get { return unlocked && mPassCount > 0; }
    }

    public int nextIndex
    {
        get 
        {
            DataRecord record = TableCommon.FindRecord(DataCenter.mStageTable, r => r["FRONT_INDEX"] == mIndex);
            return record == null ? 0 : record["INDEX"];
        }
    }
}


public class StageBonus
{
    public const int MAX_ITEM_COUNT = 3;

    public int mIndex = 0;
    public int mFrontStage = 0;
    public StageBonusItem[] mItems = new StageBonusItem[MAX_ITEM_COUNT];
    public int mItemCount = 0;

    public static StageBonus Create(int index)
    {
        DataRecord record = DataCenter.mStageBonus.GetRecord(index);

        if (record != null)
            return Create(record);

        return null;
    }

    public static StageBonus Create(DataRecord record)
    {
        StageBonus bonus = new StageBonus();
        bonus.mIndex = record.get("INDEX");
        bonus.mFrontStage = record.get("STAGELOCK");
        int groupId1 = record.get("BONUSDROP_1");
        int groupId2 = record.get("BONUSDROP_2");
        int groupId3 = record.get("BONUSDROP_3");
        Predicate<DataRecord> match = r => { int id = r.get("GROUP_ID"); return id == groupId1 || id == groupId2 || id == groupId3; };
        List<DataRecord> records = TableCommon.FindAllRecords(DataCenter.mGroupIDConfig, match);
        bonus.mItemCount = Mathf.Min(MAX_ITEM_COUNT, records.Count);

        for (int i = 0; i < bonus.mItemCount; ++i)
            bonus.mItems[i] = StageBonusItem.Create(records[i]);

        return bonus;
    }

    public bool unlocked
    {
        get { return mFrontStage == 0 || StageProperty.Create(mFrontStage).passed; }
    }

    public bool accepted
    {
        get { return StageBonusLogicData.IsBonusAccepted(this.mIndex); }
    }

    public bool Accept()
    {
        if (accepted)
            return false;

        foreach (var item in mItems)
        {
            if(item != null)
                item.Accept();
        }

        StageBonusLogicData.AddToAcceptedList(mIndex);
        return true;
    }
}


public class StageBonusItem
{
    public int mType = 0;
    public int mId = 0;
    public int mCount = 0;
    public int mStarLevel = 0;

    public static StageBonusItem Create(DataRecord record)
    {
        StageBonusItem item = new StageBonusItem();
        item.mType = record.get("ITEM_TYPE");
        item.mId = record.get("ITEM_ID");

        if (item.mType == (int)ITEM_TYPE.PET)
        {
            item.mCount = 1;
            item.mStarLevel = TableCommon.GetNumberFromActiveCongfig(item.mId, "STAR_LEVEL");
        }
        else if(item.mType == (int)ITEM_TYPE.EQUIP)
        {
            item.mCount = 1;
			item.mStarLevel = TableCommon.GetNumberFromRoleEquipConfig (item.mId, "STAR_LEVEL");
        }
        else
        {
            item.mCount = record.get("ITEM_COUNT");
            item.mStarLevel = 0;
        }
        return item;
    }

    public void Accept()
    {
        switch (mType)
        {
            case (int)ITEM_TYPE.GOLD:
                GameCommon.RoleChangeGold(mCount);
                break;
            case (int)ITEM_TYPE.YUANBAO:
                GameCommon.RoleChangeDiamond(mCount);
                break;
            case (int)ITEM_TYPE.POWER:
                GameCommon.RoleChangeStamina(mCount);
                break;
            default:
                break;
        }
    }
}


public class StageOpenDateTime
{
    public bool mOpenForever = true;

    public int mMonth = 0;
    public int mDayOfWeek = 0;
    public int mStartHour = 0;
    public int mStartMinute = 0;
    public int mEndHour = 0;
    public int mEndMinute = 0;

    public StageOpenDateTime(DataRecord record)
    {
        string time = record.get("OPENTIME");
        mMonth = record.get("OPENMONTH");
        mDayOfWeek = record.get("OPENDAY");

        if (time == "" || time == "0")
        {
            mStartHour = 0;
            mStartMinute = 0;
            mEndHour = 24;
            mEndMinute = 0;

            if (mMonth == 0 && mDayOfWeek == 0)
                mOpenForever = true;
            else
                mOpenForever = false;
        }
        else
        {
            mOpenForever = false;
            ParseTime(time);
        }
    }

    // Format: HH/MM|HH/MM
    private void ParseTime(string time)
    {
        string[] parseResult = time.Split(new string[] { "/", "|" }, StringSplitOptions.RemoveEmptyEntries);

        if (parseResult.Length == 4)
        {
            mStartHour = Convert.ToInt32(parseResult[0]);
            mStartMinute = Convert.ToInt32(parseResult[1]);
            mEndHour = Convert.ToInt32(parseResult[2]);
            mEndMinute = Convert.ToInt32(parseResult[3]);
        }
    }
}


//public class StagePropertyList : List<StageProperty>
//{
//    public static StagePropertyList Create(Predicate<DataRecord> match)
//    {
//        StagePropertyList list = new StagePropertyList();

//        foreach (KeyValuePair<int, DataRecord> r in DataCenter.mStageTable.GetAllRecord())
//        {
//            if (r.Value.get("INDEX") <= 0)
//                continue;

//            if (match == null || match(r.Value))
//                list.Add(StageProperty.Create(r.Value));
//        }

//        return list;
//    }

//    public static StagePropertyList Create()
//    {
//        return Create(null);
//    }

//    public static StagePropertyList Create(STAGE_TYPE stageType)
//    {
//        return Create(x => StageProperty.GetStageType(x.get("INDEX")) == stageType);
//    }

//    public static StagePropertyList Create(STAGE_TYPE stageType, ELEMENT_TYPE stageElement)
//    {
//        return Create(x => StageProperty.GetStageType(x.get("INDEX")) == stageType && StageProperty.GetStageElement(x.get("INDEX")) == stageElement);
//    }

//    public static bool HasUnlocked(STAGE_TYPE stageType)
//    {
//        return Create(stageType).ExistsUnlocked();
//    }

//    public static bool HasUnlocked(STAGE_TYPE stageType, ELEMENT_TYPE stageElement)
//    {
//        return Create(stageType, stageElement).ExistsUnlocked();
//    }

//    public StagePropertyList FindAll(Predicate<StageProperty> match)
//    {
//        StagePropertyList result = new StagePropertyList();

//        foreach (var stage in this)
//        {
//            if (match(stage))
//                result.Add(stage);
//        }

//        return result;
//    }

//    public StagePropertyList FindByType(STAGE_TYPE stageType)
//    {
//        return FindAll(x => x.stageType == stageType);
//    }

//    public StagePropertyList FindByElement(ELEMENT_TYPE stageElement)
//    {
//        return FindAll(x => x.stageElement == stageElement);
//    }

//    public StagePropertyList FindByDifficuty(int difficuty)
//    {
//        return FindAll(x => difficuty == 0 || x.difficuty == difficuty);
//    }

//    public StagePropertyList FindVisible()
//    {
//        return FindAll(x => x.visible);
//    }

//    public StagePropertyList FindUnlocked()
//    {
//        return FindAll(x => x.unlocked);
//    }

//    public StagePropertyList FindPassed()
//    {
//        return FindAll(x => x.passed);
//    }

//    public bool ExistsVisible()
//    {
//        return Exists(x => x.visible);
//    }

//    public bool ExistsUnlocked()
//    {
//        return Exists(x => x.unlocked);
//    }

//    public bool ExistsPassed()
//    {
//        return Exists(x => x.passed);
//    }

//    public StagePropertyList GetByPage(int page, int stagePerPage)
//    {
//        StagePropertyList result = new StagePropertyList();
//        for (int i = 0; i < stagePerPage; ++i)
//        {
//            int index = (page - 1) * stagePerPage + i;

//            if (index >= 0 && index < Count)
//                result.Add(this[index]);
//        }
//        return result;
//    }

//    public int GetIndexOfMostPriority()
//    {
//        if (Count == 0)
//            return -1;

//        int index = 0;

//        for (int i = 1; i < Count; ++i)
//        {
//            if (StageProperty.ComparePriority(this[index], this[i]) > 0)
//                index = i;
//        }

//        return index;
//    }
//}


public enum RequestBattleResult
{
    NO_CHANCE,
    ACCEPT,
    NEED_COST_FOR_COMMON,
    NEED_COST_FOR_EXTRA,
    NO_ENOUGH_STAMINA,
    NO_ENOUGH_COST_FOR_COMMON,
    NO_ENOUGH_COST_FOR_EXTRA,
    OVER_TIME,
}


public enum MapPointType
{
    Invalid = 0,
    Normal = 1,
    Boss = 2,
    Bonus = 3
}


public class WorldMapPoint
{
    public int mIndex = 0;
    public MapPointType mType = MapPointType.Normal;
    public int mId = 0;
    
    public static WorldMapPoint Create(DataRecord record)
    {
        WorldMapPoint point = new WorldMapPoint();
        point.mIndex = record.get("INDEX");
        point.mType = (MapPointType)(int)record.get("STAGETYPE");
        point.mId = record.get("STAGEID");
        return point;
    }

    public static WorldMapPoint Create(int index)
    {
        DataRecord record = DataCenter.mStagePoint.GetRecord(index);

        if (record != null)
            return Create(record);
        
        return null;
    }   

    public static WorldMapPoint Create(int difficulty, int page, int number)
    {
        return Create(GetIndex(difficulty, page, number));
    }

    public static List<WorldMapPoint> CreateList()
    {
        return CreateList(null);
    }

    public static List<WorldMapPoint> CreateList(Predicate<DataRecord> match)
    {
        List<WorldMapPoint> points = new List<WorldMapPoint>();

        foreach (var pair in DataCenter.mStagePoint.GetAllRecord())
        {
            if (pair.Key > 0 && (match == null || match(pair.Value)))
                points.Add(Create(pair.Value));
        }
        return points;
    }

    public static List<WorldMapPoint> CreateListByDifficulty(int difficulty)
    {
        if (difficulty == 0)
            return CreateList();
        else
            return CreateList(x => GetDifficulty(x.get("INDEX")) == difficulty);
    }

    public static int GetIndex(int difficulty, int page, int number)
    {
        return 1000000 + page * 1000 + number * 10 + difficulty;
    }

    public static int GetDifficulty(int index)
    {
        return index % 10;
    }

    public static int GetPage(int index)
    {
        return (index / 1000) % 1000;
    }

    public static int GetNumber(int index)
    {
        return (index / 10) % 100;
    }

    public static int Compare(WorldMapPoint lhs, WorldMapPoint rhs)
    {
        if(lhs.difficulty != rhs.difficulty)
            return lhs.difficulty - rhs.difficulty;
        if (lhs.page != rhs.page)
            return lhs.page - rhs.page;
        else
            return lhs.number - rhs.number;
    }

    public int difficulty
    {
        get { return GetDifficulty(mIndex); }
    }

    public int page
    {
        get { return GetPage(mIndex); }
    }

    public int number
    {
        get { return GetNumber(mIndex); }
    }

    public bool unlocked
    {
        get 
        {
            if (mType == MapPointType.Bonus)
                return mId > 0 && StageBonus.Create(mId).unlocked;
            else
                return mId > 0 && StageProperty.Create(mId).unlocked;
        }
    }

    public bool passed
    {
        get 
        {
            if (mType == MapPointType.Bonus)
                return mId > 0 && StageBonus.Create(mId).accepted;
            else
                return mId > 0 && StageProperty.Create(mId).passed;
        }
    }
}