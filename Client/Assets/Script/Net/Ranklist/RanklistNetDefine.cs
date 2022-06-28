using UnityEngine;
using System.Collections;
using System;
using System.Linq;
//所有排行协议

//天魔乱入
/// <summary>
/// 天魔排行数据基类
/// </summary>
public class BossBattleRanklist_ItemData : RankingItemData
{
    public string nickname;         //昵称
    public string playerId;            //用户Id
    public int power;               //战斗力
    public int headIconId;          //头像Id
    public int ranking;             //排名
    public int vipLv;               //VIP等级

    public override string Nickname { set { nickname = value; } get { return nickname; } }
    public override int Power { set { power = value; } get { return power; } }
    public override int HeadIconId { set { headIconId = value; } get { return headIconId; } }
    public override int Ranking { set { ranking = value; } get { return ranking; } }
    public override int VIPLv { set { vipLv = value; } get { return vipLv; } }
}
/// <summary>
/// 天魔排行协议回复基类
/// </summary>
public class BossBattleDamageRanklist_RespMessage : RespMessage
{
    public int myRanking;               //自己排名
}
/// <summary>
/// 伤害排行数据
/// </summary>
public class BossBattleDamageRanklist_ItemData : BossBattleRanklist_ItemData
{
    public int damage;              //伤害值
}
/// <summary>
/// 功勋排行数据
/// </summary>
public class BossBattleFeatsRanklist_ItemData : BossBattleRanklist_ItemData
{
    public int feats;               //功勋
}

/// <summary>
/// 伤害排行
/// </summary>
/// 请求
public class CS_Ranklist_BossBattleDamageRanklist : GameServerMessage
{
    public CS_Ranklist_BossBattleDamageRanklist():
        base()
    {
        pt = "CS_BossBattleDamageRanklist";
    }
}
//回复
public class SC_Ranklist_BossBattleDamageRanklist : BossBattleDamageRanklist_RespMessage
{
    public BossBattleDamageRanklist_ItemData[] ranklist;        //排行榜
}
/// <summary>
/// 功勋排行
/// </summary>
/// 请求
public class CS_Ranklist_BossBattleFeatsRanklist : GameServerMessage
{
    public CS_Ranklist_BossBattleFeatsRanklist():
        base()
    {
        pt = "CS_BossBattleFeatsRanklist";
    }
}
//回复
public class SC_Ranklist_BossBattleFeatsRanklist : BossBattleDamageRanklist_RespMessage
{
    public BossBattleFeatsRanklist_ItemData[] ranklist;         //排行榜
}


//群魔乱舞
/// <summary>
/// 群魔乱舞排行数据
/// </summary>
public class RammbockRanklist_ItemData : RankingItemData
{
    public string nickname;         //昵称
    public int power;               //战斗力
    public int headIconId;          //头像Id
    public int ranking;             //排名
    public int vipLv;               //VIP等级
    public int starCount;           //星星数量

    public override string Nickname { set { nickname = value; } get { return nickname; } }
    public override int Power { set { power = value; } get { return power; } }
    public override int HeadIconId { set { headIconId = value; } get { return headIconId; } }
    public override int Ranking { set { ranking = value; } get { return ranking; } }
    public override int VIPLv { set { vipLv = value; } get { return vipLv; } }
}
/// <summary>
/// 群魔乱舞排行
/// </summary>
/// 请求
public class CS_Ranklist_GetClimbTowerStarsRank : GameServerMessage
{
    public CS_Ranklist_GetClimbTowerStarsRank():
        base()
    {
        pt = "CS_GetClimbTowerStarsRank";
    }
}
//回复
public class SC_Ranklist_GetClimbTowerStarsRank : RespMessage
{
    public int myRanking;           //我的排名
    public int myMaxStarCount;      //历史最高星星数量
    public RammbockRanklist_ItemData[] ranklist;      //排行榜
}


//竞技场
public class GetArenaRankList_ItemData : RankingItemData
{
    public string nickname;         //昵称
    public int power;               //战斗力
    public int headIconId;          //头像Id
    public int ranking;             //排名
    public string playerId;            //用户Id
    public int level;
    public string guildName;

    public override string Nickname { set { nickname = value; } get { return nickname; } }
    public override int Power { set { power = value; } get { return power; } }
    public override int HeadIconId { set { headIconId = value; } get { return headIconId; } }
    public override int Ranking { set { ranking = value; } get { return ranking; } }
}
/// <summary>
/// 竞技场排行榜
/// </summary>
/// 请求
public class CS_Ranklist_GetArenaRankList : GameServerMessage
{
    public CS_Ranklist_GetArenaRankList():
        base()
    {
        pt = "CS_GetArenaRankList";
    }
}
//回复
public class SC_Ranklist_GetArenaRankList : RespMessage
{
    public int myRanking;           //我的排行
    public GetArenaRankList_ItemData[] ranklist;     //排行榜
}

//活动竞技场排行
public class CS_Ranklist_PvpList : GameServerMessage
{
    public CS_Ranklist_PvpList() :
        base()
    {
        pt = "CS_PvpList";
    }
}
//回复
public class SC_Ranklist_PvpList : RespMessage
{
    public int myRanking;           //我的排行
    public Int64 endTime;           //活动结束时间
    public GetArenaRankList_ItemData[] ranklist;     //排行榜
}


//宗门
/// <summary>
/// 宗门排行数据
/// </summary>
public class GuildRanklist_ItemData : RankingItemData
{
    public string guildName;     //宗门名字
    public string hierarchName;  //教主名
    public int currMemberCount;  //当前成员数
    public int ranking;          //排名
    public int guildLv;          //宗门等级

    public override int Ranking { set { } get { return ranking; } }
}
/// <summary>
/// 宗门排行
/// </summary>
/// 请求
public class CS_Ranklist_GuildRanklist : GameServerMessage
{
    public CS_Ranklist_GuildRanklist():
        base()
    {
        pt = "CS_GuildRanklist";
    }
}
//回复
public class SC_Ranklist_GuildRanklist : RespMessage
{
    public int myRanking;           //我的排名
    public GuildRanklist_ItemData[] ranklist;        //排行榜
}


//主界面
/// <summary>
/// 主界面排行基类
/// </summary>
public class MainUIRanklist_ItemData : RankingItemData
{
    public string nickname;         //昵称
    public int power;               //战斗力
    public int headIconId;          //头像Id
    public int ranking;             //排名
    public int vipLv;               //VIP等级
    public int level;               //等级
    public string guildName;        //公会名

    public override string Nickname { set { nickname = value; } get { return nickname; } }
    public override int Power { set { power = value; } get { return power; } }
    public override int HeadIconId { set { headIconId = value; } get { return headIconId; } }
    public override int Ranking { set { ranking = value; } get { return ranking; } }
    public override int VIPLv { set { vipLv = value; } get { return vipLv; } }
}
/// <summary>
/// 主界面排行协议回复基类
/// </summary>
public class MainUIRanklist_RespMessage : RespMessage
{
    public int myRanking;           //我的排名
    public Int64 endTime;           //活动结束时间
    public MainUIRanklist_ItemData[] ranklist;      //排行榜
}

/// <summary>
/// 战斗力排行
/// </summary>
/// 请求
public class CS_Ranklist_MainUIPowerRanklist : GameServerMessage
{
    public CS_Ranklist_MainUIPowerRanklist():
        base()
    {
        pt = "CS_MainUIPowerRanklist";
    }
}
//回复
public class SC_Ranklist_MainUIPowerRanklist : MainUIRanklist_RespMessage
{
}
//活动战力排行
public class CS_Ranklist_FightingList : GameServerMessage
{
    public CS_Ranklist_FightingList() :
        base()
    {
        pt = "CS_FightingList";
    }
}
//回复
public class SC_Ranklist_FightingList : MainUIRanklist_RespMessage
{
}
/// <summary>
/// 等级排行
/// </summary>
/// 请求
public class CS_Ranklist_MainUILevelRanklist : GameServerMessage
{
    public CS_Ranklist_MainUILevelRanklist() :
        base()
    {
        pt = "CS_MainUILevelRanklist";
    }
}
//回复
public class SC_Ranklist_MainUILevelRanklist : MainUIRanklist_RespMessage
{
}
