using System;
using System.Collections.Generic;

/*
 * 天魔副本协议
 */

/// <summary>
/// 获取伤害输出和功勋
/// </summary>
/// 请求
public class CS_Boss_GetDamageAndMerit : GameServerMessage
{
    public CS_Boss_GetDamageAndMerit():
        base()
    {
        pt = "CS_GetDamageAndMerit";
    }
}
//回复
public class SC_Boss_GetDamageAndMerit : RespMessage
{
    public long damageOutput;		//伤害输出
	public int selfDamageRank;		//伤害排行
    public long merit;				//功勋
	public int selfMeritRank;		//功勋排行
}

/// <summary>
/// 获取天魔列表
/// </summary>
/// 请求
public class CS_Boss_GetDemonBossList : GameServerMessage
{
    public CS_Boss_GetDemonBossList():
        base()
    {
        pt = "CS_GetDemonBossList";
    }
}
//回复
public class Boss_GetDemonBossList_BossData
{
    //public int bossIndex = -1;   //天魔Id
    public int tid;         //天魔表格Id
    public string finderId;      //发现者玩家Id
	public string finderName;	//发现者名字
    public long findTime;	//发现时间
    public int hpLeft;      //剩余血量
	public int finderTid;
    public int ifShareWithFriend = 0;   // 是否分享给好友
    public int quality = -1;     // 品质
    public int bossLevel = -1;   // 等级
    public long standingTime = -1;   // 停留时间
    public int hpBase; // 总血量
}

public class SC_Boss_GetDemonBossList : RespMessage
{
    public Boss_GetDemonBossList_BossData[] arr;
}

/// <summary>
/// 进入天魔副本
/// </summary>
/// 请求
public class CS_Boss_DemonBossStart : GameServerMessage
{
    public string finderId;        //发现者玩家Id
    public int bossIndex;       //天魔Id
    public int buttonIndex;     //选择按钮Id
    public int eventIndex;      //活动序号

    public CS_Boss_DemonBossStart() :
        base()
    {
        pt = "CS_DemonBossStart";
    }
}
//回复
public class SC_Boss_DemonBossStart : RespMessage
{
    public int bossDead = 0;    // 天魔已死
    //public int quality = -1;    // 天魔品质
    public int hpLeft = -1;     // 天魔剩余血量
    public int duration = -1;   // 战斗持续时间
}

/// <summary>
/// 退出天魔副本
/// </summary>
/// 请求
public class CS_Boss_DemonBossResult : GameServerMessage
{
    //public long damageOutput;   //伤害输出

    public CS_Boss_DemonBossResult():
        base()
    {
        pt = "CS_DemonBossResult";
    }
}
//回复
public class SC_Boss_DemonBossResult : RespMessage
{
    public int battleAchv;      //战功
    public int merit;           //功勋
	public int playerTid;      
}

/// <summary>
/// 获取伤害排行
/// </summary>
/// 请求
public class CS_Boss_GetDamageOutputRank : GameServerMessage
{
    public CS_Boss_GetDamageOutputRank():
        base()
    {
    }
}
//回复
public class Boss_GetDamageOutputRank_RoleData
{
    public int uid;             //用户id
    public string name;         //用户名
    public int iconIndex;       //用户头像
    public int power;           //用户战斗力
    public long damageOutput;   //伤害值
}
public class SC_Boss_GetDamageOutputRank : RespMessage
{
    public Boss_GetDamageOutputRank_RoleData[] arr;     //伤害排行榜
    public int selfRank;        //自己的排名
}

/// <summary>
/// 获取功勋排行
/// </summary>
/// 请求
public class CS_Boss_GetMeritRank : GameServerMessage
{
    public CS_Boss_GetMeritRank():
        base()
    {
    }
}
public class Boss_GetMeritRank_RoleData
{
    public int uid;         //用户id
    public string name;     //用户名
    public int iconIndex;   //用户头像
    public int power;       //用户战斗力
    public long merit;      //功勋
}
//回复
public class SC_Boss_GetMeritRank : RespMessage
{
    public Boss_GetMeritRank_RoleData[] arr;    //功勋排行榜
    public int selfRank;
}

/// <summary>
/// 领取功勋奖励
/// </summary>
/// 请求
public class CS_Boss_ReceiveMeritAward : GameServerMessage
{
    public int awardIndex;

    public CS_Boss_ReceiveMeritAward():
        base()
    {
        pt = "CS_ReceiveMeritAward";
    }
}
//回复
public class SC_Boss_ReceiveMeritAward : RespMessage
{
    public ItemDataBase[] featAddArr;
}

/// <summary>
/// 领取功勋奖励记录
/// </summary>
/// 请求
public class CS_Boss_GetMeritAwardList : GameServerMessage
{
    public CS_Boss_GetMeritAwardList() :
        base()
    {
        pt = "CS_GetMeritAwardList";
    }
}
//回复
public class SC_Boss_GetMeritAwardList : RespMessage
{
    public List<int> arr;
}
