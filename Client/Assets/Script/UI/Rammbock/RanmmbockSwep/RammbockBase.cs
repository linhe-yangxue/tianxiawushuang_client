using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using DataTable;


public abstract class RammbockBase : tWindow
{

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
    }

    public static SC_ClimbTowerSweep iSCClimbTowerSweep = null;

    public static void SetClimbData(SC_ClimbTowerSweep data)
    {
        iSCClimbTowerSweep = data;
    }

    public static SC_ClimbTowerSweep GetClimbData()
    {
        return iSCClimbTowerSweep;
    }

    public static int[] GetAwardIndexes()
    {
        int[] ret = null;
        if (iSCClimbTowerSweep != null)
        {
            ret = iSCClimbTowerSweep.awardIndexes;
        }
        return ret;
    }

    public static ItemDataBase[] GettierPassRewards()
    {
        ItemDataBase[] ret = null;
        if (iSCClimbTowerSweep != null)
        {
            ret = iSCClimbTowerSweep.tierPassRewards;
        }
        return ret;
    }


    //获取本层剩余的关卡
    public static int GetRemainedGuan()
    {
        int nextTier = GetGuanka();
        int ret = 3;
        if (nextTier % 3 == 0)
        {
            ret = 1;
        }
        else
        {
            ret = (nextTier / 3 + 1) * 3 - nextTier + 1;
        }
        return ret;
    }

    //获取奖励箱的位置
    public static Vector3 GetRewardPos()
    {
        Vector3 vector3 = new Vector3(0, -355.8f, 0);
        int remainedGuan = GetRemainedGuan();
        switch(remainedGuan)
        {
            case 1:
                vector3 = new Vector3(0, -56.5f, 0);
                break;
            case 2:
                vector3 = new Vector3(0, -205.78f, 0);
                break;
            case 3:
                vector3 = new Vector3(0, -355.8f, 0);
                break;
        }
        return vector3;
    }


    //扫荡加星
    public static void AddStar()
    {
        RammbockWindow win = DataCenter.Self.getData("RAMMBOCK_WINDOW") as RammbockWindow;
        if (win != null)
        {
            win.m_climbingInfo.remainStars += GetRemainedGuan() * 3; //可能加3 6 9
        }
    }

    //获取当前挑战的关卡--一层三关
    public static int GetGuanka()
    {
        int ret = 1;
        RammbockWindow win = DataCenter.Self.getData("RAMMBOCK_WINDOW") as RammbockWindow;
        if (win != null)
        {
            ret = win.m_climbingInfo.nextTier;
        }
        return ret;
    }

    //计算扫荡该层需要的战斗力
    public static int GetMaxFightPoint()
    {
        int fightPoint = 0;
        int climbIndex = GetGuanka();
        int maxIndex = 1;
        if (climbIndex % 3 == 0)
        {
            maxIndex = climbIndex;
        }
        else
        {
            maxIndex = (climbIndex / 3 + 1) * 3;
        }
        fightPoint = DataCenter.mClimbingTowerConfig.GetData(maxIndex, "FIGHT_POINT_STAR");
        return fightPoint;
    }

    //奖励暴击文字
    public static string GetCrikeText(int rewardIndex)
    {
        string ret = "";
        switch (rewardIndex)
        {
            case 1:
                {
                    ret = "";
                    break;
                }
            case 2:
                {
                    ret = "[CC33FF]" + TableCommon.getStringFromStringList(STRING_INDEX.RAMMBOCK_CRIKE_NORMAL);
                    break;
                }
            case 3:
                {
                    ret = "[FF9900]" + TableCommon.getStringFromStringList(STRING_INDEX.RAMMBOCK_CRIKE_NICE); ;
                    break;
                }
        }
        return ret;
    }

    //通过关卡数去获得相应的奖励--这里默认最高难度
    public static int GetEquipToken(int guanka, int rewardIndex)
    {
        string ret = "";
        string equipToken = DataCenter.mClimbingTowerConfig.GetData(guanka, "BASE_EQUIP_TOKEN");
        string[] token = equipToken.Split('|');
        if (token != null && token.Length > rewardIndex - 1)
        {
            ret = token[rewardIndex - 1];
        }
        return int.Parse(ret) * 3;
    }

    //通过关卡数去获得相应的奖励--这里默认最高难度
    public static int GetPrice(int guanka, int rewardIndex)
    {
        string ret = "";
        string baseMoney = DataCenter.mClimbingTowerConfig.GetData(guanka, "BASE_MONEY");
        string[] money = baseMoney.Split('|');
        if (money != null && money.Length > rewardIndex - 1)
        {
            ret = money[rewardIndex - 1];
        }
        return int.Parse(ret) * 3;
    }

    //add消耗品
    public static void AddConsumables(int[] awardIndex)
    {
        if (awardIndex == null)
        {
            return;
        }
        int tokenMax = 0;
        int moneyMax = 0;
        int currentGuanka = GetGuanka();
        for (int i = 0; i < awardIndex.Length; i++)
        {
            tokenMax += GetEquipToken(currentGuanka + i, awardIndex[i]);
            moneyMax += GetPrice(currentGuanka + i, awardIndex[i]);
        }

        //adds
        GameCommon.RoleChangePrestige(tokenMax);
        GameCommon.RoleChangeGold(moneyMax);
        
    }

}
