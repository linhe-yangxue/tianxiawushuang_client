using System;

/*
 * 竞技场协议
 */

#region 竞技场协议

public class ArenaWarrior
{
    public string name = ""; /* 名字 */ 
    public int level = -1; /* 用户等级 */ 
    public int rank = -1; /* 排名 */ 
    public int power = -1; /* 战斗力 */ 
    public string uid = ""; /* 用户Id */ 
    public int vipLevel = 0; /* VIP等级 */ 
    public string tid = ""; /* 玩家头像 */ 
    public int[] pets; /* 宠物头像 */
    public int bestRank = 0; /* 最好排名 */
    public int canBattle = 0; /*0-不能挑战 1-能挑战*/
}

public class PetFightDetail{
    public int tid = 0; /* 符灵Tid */
    public int attack = 0; /* 攻击力 */
    public int hp = 0; /* 生命值 */
    public int phyDef = 0; /* 物防值 */
    public int mgcDef = 0; /* 法防值 */
    public int criRt = 0; /* 暴击率 */
    public int dfCriRt = 0; /* 抗暴率 */
    public int hitTrRt = 0; /* 命中率 */
    public int gdRt = 0; /* 闪避率 */
    public int hitRt = 0; /* 伤害率 */
    public int dfHitRt = 0; /* 减伤率 */
}

//-------------------------------------------------------------------------
// 挑战对象
//-------------------------------------------------------------------------
public class ChallengePlayer
{
    public string name = "";
    public int level = 0;
    public int power = 0;
    public PetFightDetail[] petFDs;
}

public class ArenaBattleRecord{
    public string rivalUid = ""; /* 对手用户ID */
    public long attackTime = 0; /* 攻击时间 */
    public int rankChange = 0; /* 排名变化 */
    public int tid = 0;       /*等级*/
    public string name = "";    /*名字*/
    public int level = 0;       /*等级*/
}

/// <summary>
/// 获取设置的ArenaChallengeList
/// </summary>
/// 请求
/// 
public class CS_ArenaChallengeList : GameServerMessage 
{
    public CS_ArenaChallengeList() :
        base()
    {
        pt = "CS_ArenaChallengeList";
    }
}
//回复
public class SC_ArenaChallengeList : RespMessage
{
    public ArenaWarrior arenaPrincipal;
    public ArenaWarrior[] arenaChallengeList;
}

/// <summary>
/// ArenaBattleStart
/// </summary>
/// 请求
public class CS_ArenaBattleStart : GameServerMessage
{
    public int rivalRank = 0; 
    public CS_ArenaBattleStart() :
        base()
    {
        pt = "CS_ArenaBattleStart";
    }
}
//回复
public class SC_ArenaBattleStart : RespMessage
{
    public ChallengePlayer rival;
}

/// <summary>
/// CS_ArenaBattleEnd
/// </summary>
/// 请求
public class CS_ArenaBattleEnd : GameServerMessage
{
    public int success = 0;
    public int rivalRank = 0;
    public CS_ArenaBattleEnd() :
        base()
    {
        pt = "CS_ArenaBattleEnd";
    }
}
//回复
public class SC_ArenaBattleEnd : RespMessage
{
    public ItemDataBase[] rewards;
    public ItemDataBase[] addItems;
}

/// <summary>
/// SC_ArenaRankList
/// </summary>
/// 请求
public class CS_ArenaRankList : GameServerMessage
{
    public CS_ArenaRankList() :
        base()
    {
        pt = "CS_ArenaRankList";
    }
}
//回复
public class SC_ArenaRankList : RespMessage
{
    public ArenaWarrior arenaPrincipal;
    public ArenaWarrior[] arenaRankList;
}

/// <summary>
/// ArenaBattleRecord
/// </summary>
/// 请求
public class CS_ArenaBattleRecord : GameServerMessage
{
    public CS_ArenaBattleRecord() :
        base()
    {
        pt = "CS_ArenaBattleRecord";
    }
}
//回复
public class SC_ArenaBattleRecord : RespMessage
{
    public ArenaBattleRecord[] arenaBattleRecord;
}

/// <summary>
/// ArenaSweep
/// </summary>
/// 请求
public class CS_ArenaSweep : GameServerMessage
{
    public CS_ArenaSweep() :
        base()
    {
        pt = "CS_ArenaSweep";
    }
}
//回复
public class SC_ArenaSweep : RespMessage
{
    public ItemDataBase[] rewards;
    public ItemDataBase[] addItems;
}


#endregion