using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using DataTable;


public abstract class DailyStageBase : tWindow
{

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
    }

    public static DailyStageInfo[] idailyStageInfoObject { get; set; }

    public static List<int> ilist = null;
    public static int iOpenEdType = -1;

    //副本类型 6种
    public enum DAILY_STAGE
    {
        DAILY_STAGE_ONE = 1, //普通
        DAILY_STAGE_TWO = 2,  //精英
        DAILY_STAGE_THREE = 3,   //英雄
        DAILY_STAGE_FOUR = 4,   //史诗
        DAILY_STAGE_FIVE = 5, //传奇
        DAILY_STAGE_SIX = 6,   //神话
    }

    //副本状态 6种
    public enum DAILY_STAGE_STATE
    {
        DAILY_STAGE_NORMAL = 1, //normal
        DAILY_STAGE_DOWN = 2,  //down
        DAILY_STAGE_DIS = 3,   //disable
    }

    //获得顺序type-list
    public static void InitData()
    {
        //必须clear啊
        if (ilist != null)
        {
            ilist.Clear();
            ilist = null;
        }

        //idailyStageInfoObject--我去每次会赋值的

        iOpenEdType = -1;
    }

    public static bool IsHasAdded(List<int> list, int type)
    {
        bool ret = false;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == type)
            {
                ret =  true;
                break;
            }
        }
        return ret;
    }

    public static int getMinLevelByType(int type)
    {
        int index = GetTopIndexByType(type);
        int openLevel = TableCommon.GetNumberFromConfig(index, "OPEN_LEVEL", DataCenter.mDailyStageConfig);
        return openLevel;
    }

    public static List<int> AddOrderByLevel(int type, List<int> listNew)
    {
        int openLevel = getMinLevelByType(type);

        int tempIndex = 0;
        for (int i = 0; i < listNew.Count; i++)
        {
            int openLevelTemp = getMinLevelByType(listNew[i]);
            if (openLevel >= openLevelTemp)
            {
                tempIndex = i + 1;
            }
            else
            {
                tempIndex = i;
            }
        }
        if (tempIndex == listNew.Count)
        {
            listNew.Add(type);
        }
        else
        {
            listNew.Insert(tempIndex, type);
        }
        return listNew;
    }

    public static List<int> GetListOrderByLevel(List<int> list)
    {
        //插入不满足条件的
        List<int> listByLevel = new List<int>();
        for (int i = 0; i < GetMaxTypeNums(); i++)
        {
            int type = i + 1;
            int index = GetTopIndexByType(type);
            if (IsOpenEdLevelByIndex(index) && IsOpenEdTimeByIndex(index))
            {
            }
            else
            {
                //等级又是从低到高插入
                if (!IsOpenEdLevelByIndex(index) && !IsHasAdded(list, type))
                {
                    if (listByLevel.Count == 0)
                    {
                        listByLevel.Add(type);
                    }
                    else
                    {
                        listByLevel =  AddOrderByLevel(type, listByLevel);
                    }
                    
                }
            }
        }
        return listByLevel;
    }

    public static void SetTypeOrderList()
    {
        InitData();

        bool openGet = true;
        int orderTemp = -1;
        List<int> list = new List<int>();
        //插入满足条件的
        for (int i = 0; i < GetMaxTypeNums(); i++ )
        {
            int type = i + 1;
            int index = GetTopIndexByType(type);
            if (IsOpenEdLevelByIndex(index) && IsOpenEdTimeByIndex(index))
            {
                orderTemp++;
                list.Add(type);
                if (openGet)
                {
                    iOpenEdType = orderTemp;
                    openGet = false;
                }
            }
        }
        //插入不满足条件的
        for (int i = 0; i < GetMaxTypeNums(); i++)
        {
            int type = i + 1;
            int index = GetTopIndexByType(type);
            if (IsOpenEdLevelByIndex(index) && IsOpenEdTimeByIndex(index))
            {     
            }
            else
            {
                if (!IsOpenEdTimeByIndex(index))
                {
                    orderTemp++;
                    list.Add(type);
                }
            }
        }

        List<int> listLevel = GetListOrderByLevel(list);

        for (int i = 0; i < listLevel.Count; i++)
        {
            orderTemp++;
            list.Add(listLevel[i]);
        }
        ilist = list;
    }

    public static int GetTypeByOrderIndex(int i)
    {
        List<int> list = ilist ;
        int ret = 1;
        if(i < ilist.Count && i >= 0)
        {
            if (ilist[i] != null)
            {
                ret = ilist[i];
            }
        }
        return ret;
    }

    //获取type索引
    public static int GetOrderIndexByType(int type)
    {
        int ret = -1;
        for (int i = 0; i < ilist.Count; i++)
        {
            if (ilist[i] != null)
            {
                if (type == ilist[i])
                {
                    ret = i;
                    break;
                }
            }
        }
        return ret;
    }

    //获取开放的关卡 没有返回-1
    public static int GetOpenEdType()
    {
        return iOpenEdType;
    }

    //获得type
    public static int GetTypeByIndex(int index)
    {
        int type = TableCommon.GetNumberFromConfig(index, "TYPE", DataCenter.mDailyStageConfig);
        return type;
    }

    //return 1234567
    public static string GetCNNumByNum(string numchar)
    {
        string ret = "一";
        switch (numchar)
        {
            case "0" :
                ret = "日";
                break;
            case "1":
                ret = "一";
                break;
            case "2":
                ret = "二";
                break;
            case "3":
                ret = "三";
                break;
            case "4":
                ret = "四";
                break;
            case "5":
                ret = "五";
                break;
            case "6" :
                ret = "六";
                break;
        }
        return ret;
    }

    //获取挑战此数
    public static int GetTianZhanTimesByType(int type)
    {
        int index = GetTopIndexByType(type);
        return GetTianZhanTimesByIndex(index);
    }

    public static int GetTianZhanTimesByIndex(int index)
    {
        int attackNum = TableCommon.GetNumberFromConfig(index, "DAILY_ATTACK_NUM", DataCenter.mDailyStageConfig);
        return attackNum;
    }

    //是否挑战过--挑战次数为0表示未挑战过 
    public static bool IsTiaoZhanEdByType(int type)
    {
        int battleTimes = 0;
        if (idailyStageInfoObject == null)
        {
            return false;
        }
        for (int i = 0; i < idailyStageInfoObject.Length; i++)
        {
            if (idailyStageInfoObject[i] != null)
            {
                if (idailyStageInfoObject[i].type == type)
                {
                    battleTimes = idailyStageInfoObject[i].battleTimes;
                    break;
                }
            }
        }
        int attackTime = GetTianZhanTimesByType(type);
        return (attackTime - battleTimes > 0 ? false : true);
    }

    //是否挑战过--挑战次数为0表示未挑战过
    public static bool IsTiaoZhanEdByIndex(int index)
    {
        int type = GetTypeByIndex(index);
        return IsTiaoZhanEdByType(type);
    }

    //该类型关卡-时间段是否开启
    public static bool IsOpenEdLevelByType(int type)
    {
        int index = GetTopIndexByType(type);
        return IsOpenEdLevelByIndex(index);
    }

    //..
    public static bool IsOpenEdByIndex(int index)
    {
        if (IsOpenEdTimeByIndex(index) && IsOpenEdLevelByIndex(index))
        {
            return true;
        }
        return false;
    }

    public static bool IsOpenEdByType(int type)
    {
        if (IsOpenEdTimeByType(type) && IsOpenEdLevelByType(type))
        {
            return true;
        }
        return false;
    }

    //获得玩家等级
    public static int GetPlayerLevel()
    {
        return RoleLogicData.Self.character.level;
    }

    //该类型关卡-等级是否够
    public static bool IsOpenEdLevelByIndex(int index)
    {
        int type = GetTypeByIndex(index);
        int openLevel = TableCommon.GetNumberFromConfig(index, "OPEN_LEVEL", DataCenter.mDailyStageConfig);
        string openDay = TableCommon.GetStringFromConfig(index, "OPEN_DAY", DataCenter.mDailyStageConfig);
        string[] openDays = openDay.Split('#');
        if (GetPlayerLevel() >= openLevel)
        {
            return true;
        }
        return false;
    }

    //通关获得经验
    public static int GetDailyStageExpByIndex(int index)
    {
        int exp = TableCommon.GetNumberFromConfig(index, "COST", DataCenter.mDailyStageConfig) * 10;
        return exp;
    }

    //获得难度
    public static int GetDailyStageDiffByIndex(int index)
    {
        int diff = TableCommon.GetNumberFromConfig(index, "DIFFICULTY", DataCenter.mDailyStageConfig);
        return diff;
    }

    //该类型关卡-时间段是否开启
    public static bool IsOpenEdTimeByType(int type)
    {
        int index = GetTopIndexByType(type);
        return IsOpenEdTimeByIndex(index);
    }

    //该类型关卡-时间段是否开启
    public static bool IsOpenEdTimeByIndex(int index)
    {
        DateTime nowTime = GameCommon.NowDateTime();
        int weekDays = (int)nowTime.DayOfWeek;
        int type = GetTypeByIndex(index);
        string openDay = TableCommon.GetStringFromConfig(index, "OPEN_DAY", DataCenter.mDailyStageConfig);
        string[] openDays = openDay.Split('#');
        for (int i = 0; i < openDays.Length; i++ )
        {
            if (openDays[i] != "" && int.Parse(openDays[i]) == weekDays)
            {
                return true;
            }
        }
        return false;
    }
    

    //获取右边scroll view的数据
    public static List<int> GetRightScrollViewData(int type)
    {
        List<int> list = new List<int>();
        Dictionary<int, DataRecord> dicConfig = DataCenter.mDailyStageConfig.GetAllRecord();
        foreach (var pairValue in dicConfig)
        {
            DataRecord record = pairValue.Value;
            int typeTemp = (int)(record.getData("TYPE"));
            if (typeTemp == type)
            {
                list.Add((int)(record.getData("INDEX")));
            }
        }
        return list;
    }

    //获取左边scroll view的长度
    public static int GetMaxTypeNums()
    {
        int ret = 0;
        Dictionary<int, DataRecord> dicConfig = DataCenter.mDailyStageConfig.GetAllRecord();
        foreach (var pairValue in dicConfig)
        {
            DataRecord record = pairValue.Value;
            int type = (int)(record.getData("TYPE"));
            if (ret < type)
            {
                ret = type;
            }
        }
        return ret;
    }

    //获取头数据初始化
    public static int GetTopIndex()
    {
        int index = 0;
        int ret = 0;
        Dictionary<int, DataRecord> dicConfig = DataCenter.mDailyStageConfig.GetAllRecord();
        foreach (var pareValue in dicConfig)
        {
            if (index == 0)
            {
                ret = pareValue.Key;
                break;
            }
        }
        return ret;
    }

    //获取头数据初始化
    public static int GetTopIndexByType(int type)
    {
        int ret = 1;
        Dictionary<int, DataRecord> dicConfig = DataCenter.mDailyStageConfig.GetAllRecord();
        foreach (var pareValue in dicConfig)
        {
            int typeTmep = pareValue.Value.getData("TYPE");
            if (typeTmep == type)
            {
                ret = pareValue.Key;
                break;
            }
        }
        return ret;
    }

    public static string GetDifficultyString(int difficulty)
    {
        string ret = "普通";
        switch (difficulty)
        {
            case 1:
                ret = "普通";
                break;
            case 2:
                ret = "精英";
                break;
            case 3:
                ret = "英雄";
                break;
            case 4:
                ret = "史诗";
                break;
            case 5:
                ret = "传奇";
                break;
            case 6:
                ret = "神话";
                break;
        }
        return ret;
    }

    //获得副本名字 by index
    public static string GetNameByType(int type)
    {
        int index = GetTopIndexByType(type);
        return GetNameByIndex(index);
    }

    //获得副本名字 by index
    public static string GetNameByIndex(int index)
    {
        string ret = TableCommon.GetStringFromConfig(index, "NAME", DataCenter.mDailyStageConfig);
        return ret;
    }

    //通过类型和状态获取图片名字
    public static string GetIconByNum(int type, DAILY_STAGE_STATE state)
    {
        string ret = "a_ui_richang_0" + type.ToString();
        switch(state)
        {
            case DAILY_STAGE_STATE.DAILY_STAGE_NORMAL:
                ret = ret +  "_normal";
                break;
            case DAILY_STAGE_STATE.DAILY_STAGE_DOWN:
                ret = ret + "_down";
                break;
            case DAILY_STAGE_STATE.DAILY_STAGE_DIS:
                ret = ret + "_dis";
                break;
        }
        return ret;
    }

    //获取解锁文字
    public static string GetLockStrByType(int type)
    {
        int index = GetTopIndexByType(type);
        return GetLockStrByIndex(index);
    }

    //获取解锁文字
    public static string GetLockStrByIndex(int index)
    {
        int openLevel = TableCommon.GetNumberFromConfig(index, "OPEN_LEVEL", DataCenter.mDailyStageConfig);
        string openLevelStr = openLevel.InsertToString("{0}级解锁");
        return openLevelStr;
    }

    //获取开放时间描述
    public static string GetTimeDesc(int type)
    {
        string ret = "";
        int index = GetTopIndexByType(type);
        string openDay = TableCommon.GetStringFromConfig(index, "OPEN_DAY", DataCenter.mDailyStageConfig);
        string[] openDays = openDay.Split('#');
        string oneStr = GetCNNumByNum(openDays[0]);
        string twoStr = GetCNNumByNum(openDays[1]);
        string threeStr = GetCNNumByNum(openDays[2]);
        string fourStr = GetCNNumByNum(openDays[3]);
        if (openDays.Length == 1)
        {
            ret = string.Format("每周{0}开放", oneStr);
        }
        if (openDays.Length == 2)
        {
            ret = string.Format("每周{0}、{1}开放", oneStr, twoStr);
        }
        if (openDays.Length == 3)
        {
            ret = string.Format("每周{0}、{1}、{2}开放", oneStr, twoStr, threeStr);
        }
        if (openDays.Length == 4)
        {
            ret = string.Format("每周{0}、{1}、{2}、{3}开放", oneStr, twoStr, threeStr, fourStr);
        }
        return ret;
    }

    //获取副本名字
    public static string GetDailyName(int index)
    {
        string desc = TableCommon.GetStringFromConfig(index, "NAME", DataCenter.mDailyStageConfig);
        return desc;
    }

    //获取副本名字
    public static string GetDiffIcon(int index)
    {
        string diffIcon = TableCommon.GetStringFromConfig(index, "DIFFICULTY_SPRITE_NAME", DataCenter.mDailyStageConfig);
        return diffIcon;
    }

    //获取推荐战斗力
    public static int GetBattlePoint(int index)
    {
        int battlePoint = TableCommon.GetNumberFromConfig(index, "BATTLE_POINT", DataCenter.mDailyStageConfig);
        return battlePoint;
    }
    
    
    //获取drop item
    public static List<string> GetDropItem(int index)
    {
        List<string> list = new List<string>();
        string drop = TableCommon.GetStringFromConfig(index, "DROP_ITEM", DataCenter.mDailyStageConfig);
        string[] drops = drop.Split('#');
        for (int i = 0; i < drops.Length; i++)
        {
            list.Add(drops[i]);
        }
        return list;
    }

    //refresh toogle
    public static void RefreshToogel(UIGridContainer container, int indexBTN, string stringBTN, string CheckMark, bool needSetUISprite = false)
    {
        var pDailyPveDiff = container.controlList;
        //setcheckmark
        if (needSetUISprite)
        {
            for (int i = 0; i < container.MaxCount; i++)
            {
                if (pDailyPveDiff[i] != null)
                {
                    GameObject board = pDailyPveDiff[i];
                    if (board != null)
                    {
                        GameCommon.FindObject(board, CheckMark).SetActive(true);
                        int type = GetTypeByOrderIndex(i);
                        GameCommon.SetUISprite(board, CheckMark, GetIconByNum(type, DAILY_STAGE_STATE.DAILY_STAGE_DOWN));
                    }
                }
            }
        }

            //---
        for (int i = 0; i < container.MaxCount; i++)
        {
            if (pDailyPveDiff[i] != null)
            {
                GameObject board = pDailyPveDiff[i];
                if (board != null)
                {
                    if (i == indexBTN)
                    {
                        GameCommon.FindObject(board, stringBTN).GetComponent<UIToggle>().value = true;
                        GameCommon.FindObject(board, CheckMark).SetActive(true);
                    }
                    else
                    {
                        GameCommon.FindObject(board, stringBTN).GetComponent<UIToggle>().value = false;
                        GameCommon.FindObject(board, CheckMark).SetActive(false);
                    }
                }
            }
        }
    }

    //消息成功-更新函数
    public static void UpdateBattleTimes(int type)
    {
        for(int i = 0; i < idailyStageInfoObject.Length; i++)
        {
            if(idailyStageInfoObject[i] != null)
            {
                if (type == idailyStageInfoObject[i].type)
                {
                    idailyStageInfoObject[i].battleTimes--;
                    break;
                }
            }
        }
    }

    public static void UpdateTili(int index)
    {
        int tili = TableCommon.GetNumberFromConfig(index, "COST", DataCenter.mDailyStageConfig);
        GameCommon.RoleChangeStamina(-tili);
    }
}
