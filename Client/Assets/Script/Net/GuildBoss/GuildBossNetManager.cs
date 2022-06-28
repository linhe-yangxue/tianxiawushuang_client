using UnityEngine;
using System.Collections;
using DataTable;
using Logic;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// GuildBoss
/// </summary>
public enum GUILD_BOSS
{
    SELECT_LEVEL,
    NEXT_LEVEL,
    AGAIN_LEVEL,
    CLEAN_LEVEL,
    QUIT_CLEAN_LEVEL,
    BACK_HOME_PAGE,
}

//public class BossBattleStartData
//{
//	public Boss_GetDemonBossList_BossData bossData;
// 	public int beatDemonCard;
// 	public int meritTimes;
// 	public int battleAchieve;
//}

public class GuildBossNetManager
{
    public static int iDamage = 0;
    /// <summary>
    /// GuildBossIndex
    /// </summary>
    public static void RequestGuildBossInfoSimple(string gid)
    {
        CS_GuildBossInfoSimple data = new CS_GuildBossInfoSimple();
        data.zgid = gid;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestGuildBossInfoSimpleSuccess, __OnRequestFailed);
    }
    private static void __OnRequestGuildBossInfoSimpleSuccess(string text)
    {
        SC_GuildBossInfoSimple retRoleData = JCode.Decode<SC_GuildBossInfoSimple>(text);
        if (retRoleData != null)
        {
            UnionBase.InGuildThenDo(retRoleData, () =>
            {
                DataCenter.SetData(UIWindowString.union_pk_enter_window, "UNION_PKENTER_INIT", retRoleData);
            });
        }
    }

    /// <summary>
    /// 获取
    /// </summary>
    public static void RequestGuildBossInit(string gid, int mid)
    {
        CS_GuildBossInit data = new CS_GuildBossInit();
        data.zgid = gid;
        data.mid = mid;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestGuildBossInitSuccess, __OnRequestFailed);
    }
    private static void __OnRequestGuildBossInitSuccess(string text)
    {
        SC_GuildBossInit retRoleData = JCode.Decode<SC_GuildBossInit>(text);
        if (retRoleData != null)
        {
            UnionBase.InGuildThenDo(retRoleData, () =>
            {
                int index = DataCenter.Get("CHECK_TOGGLE_BTN_INDEX");
                DateTime nowTime = GameCommon.NowDateTime();
                float secTemp = nowTime.Hour * 3600 + nowTime.Minute * 60 + nowTime.Second;
                if (UnionBase.guildBossObject.monsterHealth <= 0 && secTemp > 16 * 3600 && secTemp < 24 * 3600)
                {
                    UnionBase.guildBossObject.nextMid = index;
                }
                else if (UnionBase.IsTimeContained(0, 15.5f))
                {
                    UnionBase.guildBossObject.nextMid = index;
                    UnionBase.guildBossObject.mid = index;
                    UnionBase.guildBossObject.monsterHealth = UnionBase.GetMonsterBaseHp(index);
                }

                DataCenter.SetData(UIWindowString.union_set_pk_aim_window, "SET_PK_REFRESHUI", null);
            });
            
        }
    }

    /// <summary>
    /// GuildBossInfo
    /// </summary>
    public static void RequesGuildBossInfo(string guildId)
    {
        CS_GuildBossInfo data = new CS_GuildBossInfo();
        data.zgid = guildId;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestGuildBossInfoSuccess, __OnRequestFailed);
    }
    private static void __OnRequestGuildBossInfoSuccess(string text)
    {
        SC_GuildBossInfo retRoleData = JCode.Decode<SC_GuildBossInfo>(text);
        if (retRoleData != null){
            UnionBase.InGuildThenDo(retRoleData, () =>
            {
                DataCenter.CloseWindow(UIWindowString.union_pk_enter_window);
                GuildBoss guildbossTemp = retRoleData.guildBoss;
                if (guildbossTemp != null)
                {
                    DataCenter.SetData(UIWindowString.union_pk_prepare_window, "PKPREPARE_INIT", guildbossTemp);
                }
            });
        }
    }

    /// <summary>
    /// GuildBossBattleStart
    /// </summary>
    public static void RequesGuildBossBattleStart(string guildId)
    {
        if (!UnionBase.IsCanAttackBoss())
        {
            DataCenter.OpenMessageWindow("每天16:00至22:00可挑战宗门boss");
            return;
        }
        CS_GuildBossBattleStart data = new CS_GuildBossBattleStart();
        data.zgid = guildId;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestGuildBossBattleStartSuccess, __OnRequestFailed);
    }
    private static void __OnRequestGuildBossBattleStartSuccess(string text)
    {
        SC_GuildBossBattleStart retRoleData = JCode.Decode<SC_GuildBossBattleStart>(text);
        if (retRoleData != null)
        {
            UnionBase.InGuildThenDo(retRoleData, () =>
            {
                DataCenter.Set("QUIT_BACK_SCENE", QUIT_BACK_SCENE_TYPE.GUILDBOSS);
                DataCenter.SetData(UIWindowString.union_pk_prepare_window, "REQUESTBATTLE", null);
            }
            );
        }
    }

    /// <summary>
    /// GuildBossBattleEnd
    /// </summary>
    public static void RequesGuildBossBattleEnd(string guildId, int damage)
    {
        CS_GuildBossBattleEnd data = new CS_GuildBossBattleEnd();
        iDamage = damage;
        data.zgid = guildId;
        data.damage = damage;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestGuildBossBattleEndSuccess, __OnRequestFailed);
    }
    private static void __OnRequestGuildBossBattleEndSuccess(string text)
    {
        SC_GuildBossBattleEnd retRoleData = JCode.Decode<SC_GuildBossBattleEnd>(text);
        if (retRoleData != null) {
            //update data 
            bool isInGuild = (retRoleData.guildId != "");
            if (isInGuild)
            {
                UnionBase.guildBossObject.monsterHealth = retRoleData.guildBoss.monsterHealth;
                if (UnionBase.guildBossObject.monsterHealth <= 0)
                {
                    UnionBase.guildBossObject.monsterHealth = 0;
                }
                UnionBase.guildBossWarriorObject.leftBattleTimes--;
                if (UnionBase.guildBossWarriorObject.leftBattleTimes <= 0)
                {
                    UnionBase.guildBossWarriorObject.leftBattleTimes = 0;
                }

                //update贡献--
                if (UnionBase.guildBossObject.monsterHealth <= 0)
                {
                    //致命一击额外贡献
                    GameCommon.RoleChangeUnionContr(UnionBase.GetGuildBossLastHitContri(UnionBase.guildBossObject.mid));
                }
                GameCommon.RoleChangeUnionContr(UnionBase.GetGuildBossAttackContri(UnionBase.guildBossObject.mid));

                //刷新数据--并且刷新显示界面
                List<ItemDataBase> _itemList = PackageManager.UpdateItem(retRoleData.attr);
                DataCenter.SetData(UIWindowString.union_pk_win_window, "PK_WIN_SHOW_REWARD", _itemList);
            }
            else
            {
                DataCenter.OpenMessageWindow("你已被踢出公会", ()=>
                {
                    MainProcess.ClearBattle();
                    MainProcess.LoadRoleSelScene();
                });
            }
        }
    }

    /// <summary>
    /// GuildBossGetReward
    /// </summary>
    public static void RequesGuildBossGetReward(string guildId)
    {
        CS_GuildBossGetReward data = new CS_GuildBossGetReward();
        data.zgid = guildId;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestGuildBossGetRewardSuccess, __OnRequestFailed);
    }
    private static void __OnRequestGuildBossGetRewardSuccess(string text)
    {
        SC_GuildBossGetReward retRoleData = JCode.Decode<SC_GuildBossGetReward>(text);
        if (retRoleData != null)
        {
            UnionBase.InGuildThenDo(retRoleData, () =>
            {
                var itemArr = retRoleData.attr;
                List<ItemDataBase> list = PackageManager.UpdateItem(itemArr.ToList());
                List<ItemDataBase> itemDataListNew;
                GameCommon.MergeItemDataBase(list, out itemDataListNew);
                DataCenter.OpenWindow("GET_REWARDS_WINDOW", itemDataListNew.ToArray());
                UnionBase.guildBossWarriorObject.rewardState = 1;
                DataCenter.SetData(UIWindowString.union_pk_prepare_window, "UPDATE_BUTTONS", null);
            });
        }
    }

    /// <summary>
    /// GuildBossBuyBattleTimes
    /// </summary>
    public static void RequesGuildBossBuyBattleTimes(string guildId)
    {
        CS_GuildBossBuyBattleTimes data = new CS_GuildBossBuyBattleTimes();
        data.zgid = guildId;
        data.times = 1;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestGuildBossBuyBattleTimesSuccess, __OnRequestFailed);
    }
    private static void __OnRequestGuildBossBuyBattleTimesSuccess(string text)
    {
        SC_GuildBossBuyBattleTimes retRoleData = JCode.Decode<SC_GuildBossBuyBattleTimes>(text);
        if (retRoleData != null)
        {
           UnionBase.InGuildThenDo(retRoleData, () =>
            {
                //update 数据
                UnionBase.guildBossWarriorObject.leftBattleTimes++;
                UnionBase.guildBossWarriorObject.totalBuyChanllengeTimes++;
                UnionBase.guildBossWarriorObject.rewardState = 1;
                GameCommon.RoleChangeDiamond(-UnionBase.GetBuyTimesPrice());
                DataCenter.SetData(UIWindowString.union_pk_prepare_window, "UPDATAUI", null);
            });
        }
        
    }


    //统一处理失败报错 客户端能避免，尽量避免
    private static void __OnRequestFailed(string text)
    {
        //TODO
        int ret = int.Parse(text);
        if (ret == 2102)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_BOSS_RAIN_DEAD);
            return;
        }
    }
}
