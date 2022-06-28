using System;

/*
 * 公会副本协议
 */

#region 公会战协议

/// <summary>
/// 获取设置的bossIndex
/// </summary>
/// 请求
/// 
public class GuildBossSimple
{
    public string gid = ""; /* 公会ID */
    public int mid = -1; /* 怪物ID */
    public long nextSetTime = 0; /* 下一次可以初始化公会BOSS的时间 */
    public int monsterHealth = -1; /* 公会BOSS当前血量 */
    public string criticalStrikeUID = ""; /* 致命一击玩家ID */
    public long criticalStrikeTime = -1; /* 致命一击的时间 */
    public int[] killedGuildBossIndex = null; //击杀过的boss
    public int nextMid = 0; //击杀过的boss
}

public class CS_GuildBossInfoSimple : GameServerMessage
{
    public string zgid;
    public CS_GuildBossInfoSimple() :
        base()
    {
        pt = "CS_GuildBossInfoSimple";
    }
}
//回复
public class SC_GuildBossInfoSimple : SC_GuildBase
{
    public GuildBossSimple guildBossSimple;
    public GuildBossWarrior guildBossWarrior;
}

/// <summary>
/// 公会boss初始化
/// </summary>
/// 请求
public class CS_GuildBossInit : GameServerMessage
{
    public string zgid;
    public int mid;
    public CS_GuildBossInit() :
        base()
    {
        pt = "CS_GuildBossInit";
    }
}
//回复
public class GuildBoss
{
    public string gid = ""; /* 公会ID */
    public int mid = -1; /* 怪物ID */
    public long nextSetTime = 0; /* 下一次可以初始化公会BOSS的时间 */
    public int monsterHealth = -1; /* 公会BOSS当前血量 */
    public string criticalStrikeUID = ""; /* 致命一击玩家ID */
    public long criticalStrikeTime = -1; /* 致命一击的时间 */
    public int[] killedGuildBossIndex = null; //击杀过的boss
    public GuildBossWarrior[] warriors = null; /* 公会BOSS战斗人员  */
    public int nextMid = 0;
}

//-公会人员 - -
public class GuildBossWarrior{
    public string gid = ""; /* 公会ID */
    public string uid = ""; /* 用户ID */
    public int damage = 0; /* 对公会BOSS造成的伤害 */
    public int leftBattleTimes = 0; /* 可以挑战的次数 */
    public int rewardState = 0; /* 是否已经领取奖励 */
    public int rank = 0; /* 玩家在公会BOSS战中的排名,每次战斗结束都会进行排名 */
    public int maxDamage = 0; /* 对公会BOSS造成的最大伤害 */
    public int totalChallengeTimes = 0; /* 挑战公会BOSS的总次数 */
    public int totalBuyChanllengeTimes = 0; /* 购买公会BOSS挑战机会的次数 */
    public string name = "";
    public int[] pets = null; /*/ *  存tid * /*/
    public int tid = 0;
    public int vipLevel = 0;   //vip
}

public class SC_GuildBossInit : SC_GuildBase
{
}

/// <summary>
/// 公会bossinfo
/// </summary>
/// 请求
public class CS_GuildBossInfo : GameServerMessage
{
    public string zgid;
    public CS_GuildBossInfo() :
        base()
    {
        pt = "CS_GuildBossInfo";
    }
}
//回复
public class SC_GuildBossInfo : SC_GuildBase
{
    public GuildBoss guildBoss;
}

/// <summary>
/// GuildBossBattleStart
/// </summary>
/// 请求
public class CS_GuildBossBattleStart : GameServerMessage
{
    public string zgid;
    public CS_GuildBossBattleStart() :
        base()
    {
        pt = "CS_GuildBossBattleStart";
    }
}
//回复
public class SC_GuildBossBattleStart : SC_GuildBase
{
}

/// <summary>
/// GuildBossBattleEnd
/// </summary>
/// 请求
public class CS_GuildBossBattleEnd : GameServerMessage
{
    public string zgid;
    public int damage;
    public CS_GuildBossBattleEnd() :
        base()
    {
        pt = "CS_GuildBossBattleEnd";
    }
}
//回复
public class SC_GuildBossBattleEnd : SC_GuildBase
{
    public GuildBoss guildBoss;
    public ItemDataBase[] attr;
}

/// <summary>
/// GuildBossGetReward
/// </summary>
/// 请求
public class CS_GuildBossGetReward : GameServerMessage
{
    public string zgid;
    public CS_GuildBossGetReward() :
        base()
    {
        pt = "CS_GuildBossGetReward";
    }
}
//回复
public class SC_GuildBossGetReward : SC_GuildBase
{
    public ItemDataBase[] attr;
}

/// <summary>
/// GuildBossBuyBattleTimes
/// </summary>
/// 请求
public class CS_GuildBossBuyBattleTimes : GameServerMessage
{
    public string zgid;
    public int times;
    public CS_GuildBossBuyBattleTimes() :
        base()
    {
        pt = "CS_GuildBossBuyBattleTimes";
    }
}
//回复
public class SC_GuildBossBuyBattleTimes : SC_GuildBase
{
    public GuildBoss guildBoss;
    public ItemDataBase[] attr;
}

#endregion