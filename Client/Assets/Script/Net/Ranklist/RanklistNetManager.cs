using UnityEngine;
using System.Collections;

/// <summary>
/// 天魔伤害排行
/// </summary>
public class Ranklist_BossBattleDamageRanklist_Requester:
    NetRequester<CS_Ranklist_BossBattleDamageRanklist, SC_Ranklist_BossBattleDamageRanklist>
{
    protected override CS_Ranklist_BossBattleDamageRanklist GetRequest()
    {
        CS_Ranklist_BossBattleDamageRanklist req = new CS_Ranklist_BossBattleDamageRanklist();
        return req;
    }

    protected override void OnSuccess()
    {
        base.OnSuccess();

        if(respCode == 1)
            DataCenter.SetData("RANKLIST_BOSSBATTLE_DAMAGE_WINDOW", "REFRESH", respMsg);
    }
    protected override void OnFail()
    {
        base.OnFail();
        //TODO
    }
}
/// <summary>
/// 天魔功勋排行
/// </summary>
public class Ranklist_BossBattleReatsRanklist_Requester:
    NetRequester<CS_Ranklist_BossBattleFeatsRanklist, SC_Ranklist_BossBattleFeatsRanklist>
{
    protected override CS_Ranklist_BossBattleFeatsRanklist GetRequest()
    {
        CS_Ranklist_BossBattleFeatsRanklist req = new CS_Ranklist_BossBattleFeatsRanklist();
        return req;
    }

    protected override void OnSuccess()
    {
        base.OnSuccess();

        if (respCode == 1)
            DataCenter.SetData("RANKLIST_BOSSBATTLE_FEATS_WINDOW", "REFRESH", respMsg);
    }
    protected override void OnFail()
    {
        base.OnFail();
        //TODO
    }
}

/// <summary>
/// 群魔乱舞排行
/// </summary>
public class Ranklist_GetClimbTowerStarsRank_Requester:
    NetRequester<CS_Ranklist_GetClimbTowerStarsRank, SC_Ranklist_GetClimbTowerStarsRank>
{
    protected override CS_Ranklist_GetClimbTowerStarsRank GetRequest()
    {
        CS_Ranklist_GetClimbTowerStarsRank req = new CS_Ranklist_GetClimbTowerStarsRank();
        return req;
    }

    protected override void OnSuccess()
    {
        base.OnSuccess();

        if (respCode == 1)
            DataCenter.SetData("RANKLIST_RAMMBOCK_WINDOW", "REFRESH", respMsg);
    }
    protected override void OnFail()
    {
        base.OnFail();
        //TODO
    }
}

/// <summary>
/// 竞技场排行
/// </summary>
public class Ranklist_GetArenaRankList_Requester:
    NetRequester<CS_Ranklist_GetArenaRankList, SC_Ranklist_GetArenaRankList>
{
    protected override CS_Ranklist_GetArenaRankList GetRequest()
    {
        CS_Ranklist_GetArenaRankList req = new CS_Ranklist_GetArenaRankList();
        return req;
    }

    protected override void OnSuccess()
    {
        base.OnSuccess();

        if (respCode == 1)
            DataCenter.SetData("RANKLIST_PVP_WINDOW", "REFRESH", respMsg);
    }
    protected override void OnFail()
    {
        base.OnFail();
        //TODO
    }
}

/// <summary>
/// 宗门排行
/// </summary>
public class Ranklist_GuildRanklist_Requester:
    NetRequester<CS_Ranklist_GuildRanklist, SC_Ranklist_GuildRanklist>
{
    protected override CS_Ranklist_GuildRanklist GetRequest()
    {
        CS_Ranklist_GuildRanklist req = new CS_Ranklist_GuildRanklist();
        return req;
    }

    protected override void OnSuccess()
    {
        base.OnSuccess();

        if (respCode == 1)
            DataCenter.SetData("RANKLIST_GUILD_WINDOW", "REFRESH", respMsg);
    }
    protected override void OnFail()
    {
        base.OnFail();
        //TODO
    }
}

/// <summary>
/// 主界面战斗力排行
/// </summary>
public class Ranklist_MainUIPowerRanklist_Requester:
    NetRequester<CS_Ranklist_MainUIPowerRanklist, SC_Ranklist_MainUIPowerRanklist>
{
    protected override CS_Ranklist_MainUIPowerRanklist GetRequest()
    {
        CS_Ranklist_MainUIPowerRanklist req = new CS_Ranklist_MainUIPowerRanklist();
        return req;
    }

    protected override void OnSuccess()
    {
        base.OnSuccess();

        if (respCode == 1)
            DataCenter.SetData("RANKLIST_MAIN_UI_POWER_WINDOW", "REFRESH", respMsg);
    }
    protected override void OnFail()
    {
        base.OnFail();
        //TODO
    }
}
/// <summary>
/// 主界面等级排行
/// </summary>
public class Ranklist_MainUILevelRanklist_Requester:
    NetRequester<CS_Ranklist_MainUILevelRanklist, SC_Ranklist_MainUILevelRanklist>
{
    protected override CS_Ranklist_MainUILevelRanklist GetRequest()
    {
        CS_Ranklist_MainUILevelRanklist req = new CS_Ranklist_MainUILevelRanklist();
        return req;
    }

    protected override void OnSuccess()
    {
        base.OnSuccess();

        if (respCode == 1)
            DataCenter.SetData("RANKLIST_MAIN_UI_LEVEL_WINDOW", "REFRESH", respMsg);
    }
    protected override void OnFail()
    {
        base.OnFail();
        //TODO
    }
}

public class RanklistNetManager
{
    private static bool msIsSend = true;       //是否发送请求
    public static bool IsSend
    {
        get { return msIsSend; }
    }

    /// <summary>
    /// 天魔伤害排行
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RequestBossBattleDamageRanklist()
    {
        if (!msIsSend)
            yield break;
        yield return (new Ranklist_BossBattleDamageRanklist_Requester().Start());
    }
    /// <summary>
    /// 天魔功勋排行
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RequestBossBattleFeatsRanklist()
    {
        if (!msIsSend)
            yield break;
        yield return (new Ranklist_BossBattleReatsRanklist_Requester().Start());
    }

    /// <summary>
    /// 群魔乱舞排行
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RequestRammbockRanklist()
    {
        if (!msIsSend)
            yield break;
        yield return (new Ranklist_GetClimbTowerStarsRank_Requester().Start());
    }

    /// <summary>
    /// 竞技场排行
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RequestPVPRanklist()
    {
        if (!msIsSend)
            yield break;
        yield return (new Ranklist_GetArenaRankList_Requester().Start());
    }

    /// <summary>
    /// 宗门排行
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RequestGuildRanklist()
    {
        if (!msIsSend)
            yield break;
        yield return (new Ranklist_GuildRanklist_Requester().Start());
    }

    /// <summary>
    /// 主界面战斗力排行
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RequestMainUIPowerRanklist()
    {
        if (!msIsSend)
            yield break;
        yield return (new Ranklist_MainUIPowerRanklist_Requester().Start());
    }
    /// <summary>
    /// 主界面等级排行
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RequestMainUILevelRanklist()
    {
        if (!msIsSend)
            yield break;
        yield return (new Ranklist_MainUILevelRanklist_Requester().Start());
    }
}
