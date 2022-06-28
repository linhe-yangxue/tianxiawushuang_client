using System;

/*
 * 公会副本协议
 */

#region 公会战协议

/// <summary>
/// 获取设置的DailyStageInfo
/// </summary>
/// 请求
/// 
public class DailyStageInfo
{
    public int zid = 0; /* 大区 */
    public string uid = ""; /* 用户ID */
    public int type = 0; /* 每日副本类型 */
    public int battleTimes = 0; /* 挑战次数 */
    public long lastBattleTime = 0; /* 最近一次挑战的时间 */
}

public class CS_DailyStageInfo : GameServerMessage 
{
    public CS_DailyStageInfo() :
        base()
    {
        pt = "CS_DailyStageInfo";
    }
}
//回复
public class SC_DailyStageInfo : RespMessage
{
    public DailyStageInfo[] dailyStage;
}

/// <summary>
/// DailyStageBattleStart
/// </summary>
/// 请求
public class CS_DailyStageBattleStart : GameServerMessage
{
    public int mid = 0; //每日副本的index
    public CS_DailyStageBattleStart() :
        base()
    {
        pt = "CS_DailyStageBattleStart";
    }
}
//回复
public class SC_DailyStageBattleStart : RespMessage
{
    
}

/// <summary>
/// GuildBossBattleEnd
/// </summary>
/// 请求
public class CS_DailyStageBattleEnd : GameServerMessage
{
    public int mid = 0;
    public int damage = 0;
    public CS_DailyStageBattleEnd() :
        base()
    {
        pt = "CS_DailyStageBattleEnd";
    }
}
//回复
public class SC_DailyStageBattleEnd : RespMessage
{
    public ItemDataBase[] attr;
}

#endregion