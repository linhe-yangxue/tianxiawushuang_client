using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using DataTable;

public abstract class ArenaBase : tWindow
{
    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
    }

    public static float lastResfreshTime = -10000f;

    public static float arenaButtonY = -35.5f;      //按钮的y位置

    public static int limitSpirit = 2;            //精力消耗

    public static int jiqirenRank = -1;           //战5次机器人排名

    public static string challengeUid = "";      //挑战id

    public static int challengeVipLevel;             //挑战目标
     
    public static int challengeRank;             //挑战目标

    public static string getChallengeUid()
    {
        return challengeUid;
    }

    public static void setChallengeUid(string uid)
    {
        challengeUid = uid;
    }

    public static int getChallengeRank()
    {
        return challengeRank;
    }

    public static void setChallengeRank(int rank)
    {
        challengeRank = rank;
    }

    public static int getChallengeVipLevel()
    {
        return challengeVipLevel;
    }

    public static void setChallengeVipLevel(int vipLevel)
    {
        challengeVipLevel = vipLevel;
    }

    /// <summary>
    /// 通过排名获取奖励tid
    /// </summary>
    /// <param name="rank"> 玩家当前竞技场排名 </param>
    /// <returns> PvpRankAwardConfig.csv中的tid，若排名不在配置范围内，返回-1 </returns>
    public static int GetAwardIndexByRank(int rank)
    {
        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mPvpRankAwardConfig.GetAllRecord())
        {
            DataRecord r = pair.Value;
            int min = r["RANK_MIN"];
            int max = r["RANK_MAX"];

            if (min <= rank && rank <= max)
            {
                return pair.Key;
            }
        }

        return -1;
    }

    public static ChallengePlayer challengeTarget;             //挑战目标
    public static ArenaWarrior arenaWarriorObject { get; set; }       //自己的数据
    public static ArenaWarrior[] arenaWarriorArrObject { get; set; }   //挑战列表

    public static void InitArenaData(ArenaWarrior arenaWarrior, ArenaWarrior[] arenaWarriorArr)
    {
        arenaWarriorObject = arenaWarrior;
        arenaWarriorArrObject = arenaWarriorArr;
    }
    
    //简单获取方法
    public static string GetName(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.name;
    }

    public static int GetLevel(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.level;
    }

    public static int GetShowRank(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.rank + 1;
    }

    public static int GetTrueRank(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.rank;
    }

    public static int GetPower(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.power;
    }

    public static string GetUid(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.uid;
    }

    public static int GetVipLevel(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.vipLevel;
    }

    public static string GetTid(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.tid;
    }

    public static int GetBestRank(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.bestRank + 1;
    }
    
    public static int[] GetPets(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.pets;
    }

    public static int GetCanBattle(ArenaWarrior arenaWarrior)
    {
        return arenaWarrior.canBattle;
    }

    //是否是真实玩家
    public static bool IsTruePlayer(string uid)
    {
        if (uid.Length > 0 && (uid[0] == '-' || uid[0] == '0'))
        {
            return false;
        }
        return true;
    }

    //是否是战5次玩家
    public static bool IsFiveTimesPlayer(string uid)
    {
        if (uid.Length > 0 && uid[0] == '0')
        {
            return true;
        }
        return false;
    }

    //是否是机器人
    public static bool IsRobot(string uid)
    {
        if (uid.Length > 0 && (uid[0] == '-' || uid[0] == '0'))
        {
            return true;
        }
        return false;
    }

    //算出索引
    public static int GetIndexByArenaWarrior(ArenaWarrior arenaWarrior)
    {
        int index = 0;
        for (int i = 0; i < arenaWarriorArrObject.Length; i++)
        {
            if (arenaWarriorArrObject[i] != null && arenaWarrior.uid == arenaWarriorArrObject[i].uid)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static bool CanTiaoZhan(ArenaWarrior arenaWarrior)
    {
        //是索引往上10个
        if (GetCanBattle(arenaWarrior) != 0)
        {
            return true;
        }
        return false;
    }

    //根据排名拿到排名奖励
    public static string[] GetRewardByRank(int rank)
    {
        string award = "";
        var recordList = DataCenter.mPvpRankAwardConfig.GetAllRecord();
        foreach (var record in recordList)
        {
            var value = record.Value;
            int minRank = value["RANK_MIN"];
            int maxRank = value["RANK_MAX"];
            if (rank >= minRank && rank <= maxRank)
            {
                award = value["AWARD_GROUPID"];
                break;
            }
        }
        if (award == ""||award =="0")
        {
            return null;
        }
        else
        {
            string[] awardTemp = award.Split('|');
            return awardTemp;
        }
    }

    //获取排名奖励的最大排名
    public static int GetMaxRankGetRankAward()
    {
        int ret = 0;
        var recordList = DataCenter.mPvpRankAwardConfig.GetAllRecord();
        foreach (var record in recordList)
        {
            var value = record.Value;
            int maxRank = value["RANK_MAX"];
            string str = value["AWARD_GROUPID"];
            if (maxRank >= ret && str != "0")
            {
                ret = value["RANK_MAX"];
            }
        }
        return ret;
    }

    //获取当前排名奖励的最小排名
    public static int GetCurrentMinRankByRank(int rank)
    {
        int ret = 0;
        var recordList = DataCenter.mPvpRankAwardConfig.GetAllRecord();
        foreach (var record in recordList)
        {
            var value = record.Value;
            int minRank = value["RANK_MIN"];
            int maxRank = value["RANK_MAX"];
            if (rank >= minRank && rank <= maxRank)
            {
                ret = value["RANK_MIN"];
            }
        }
        return ret - 1;
    }

    //获取目标排名
    public static int GetTargetRank(ArenaWarrior myWarrior, ArenaWarrior[] warriorArr)
    {
        int ret = 0;
        if (GetShowRank(myWarrior) > GetMaxRankGetRankAward())
        {
            ret =  GetMaxRankGetRankAward();
        }
        else
        {
            ret = GetCurrentMinRankByRank(GetShowRank(myWarrior));
        }
        return ret;
    }

    //判断是否上榜
    public static bool IsInTheList(ArenaWarrior myWarrior, ArenaWarrior[] warriorArr)
    {
        bool ret = false;
        for (int i = 0; i < warriorArr.Length; i++)
        {
            if (warriorArr[i] != null && warriorArr[i].uid == myWarrior.uid)
            {
                ret = true;
                break;
            }
        }
        return ret;
    }


    public static void openFourWindow(bool open)
    {
        if (open)
        {
            DataCenter.OpenWindow(UIWindowString.trial_window);
            DataCenter.OpenWindow(UIWindowString.trial_window_back);
            DataCenter.OpenWindow(UIWindowString.arena_window_back);
            DataCenter.OpenWindow(UIWindowString.arena_main_window);
        }
        else
        {
            DataCenter.CloseWindow(UIWindowString.arena_main_window);
            DataCenter.CloseWindow(UIWindowString.arena_window_back);
            //by chenliang
            //begin

//             DataCenter.CloseWindow(UIWindowString.trial_window);
//             DataCenter.CloseWindow(UIWindowString.trial_window_back);
//---------------------
            //不应该关闭历练界面，历练界面关闭时会自动打开主界面

            //end
        }
    }
}