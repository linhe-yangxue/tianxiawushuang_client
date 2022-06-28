using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using System.Linq;
//-------------------------------------------------------------------------
// register
//-------------------------------------------------------------------------

/*
 * 
exports.REGIST_TYPE_QUICK = 0;           // 快速登录方式
exports.REGIST_TYPE_CHECK = 1;           // 账号密码验证方式
exports.REGIST_TYPE_THIRD = 2;   
 */

public class CS_Register:LoginServerMessageDevice
{
	public string acc;
	public string pw;
    public readonly string ip=DeviceBaseData.ip;
    public readonly string mac=DeviceBaseData.mac;
    public readonly int channelUserId=0;
    
}

public class SC_Register : RespMessage
{
	public int registtype;
	public string regtoken;
	public int accExists;
}

//-------------------------------------------------------------------------
// login
//-------------------------------------------------------------------------
public class SC_SDKLogin:RespMessage {
    public readonly string uid;
    public readonly string token;
    public readonly int sdkRet;
    //by chenliang
    //begin

    public string channelUid;       //渠道用户名
    public string channelToken;     //渠道Token

    //end
}


public class CS_SDKLogin:MessageBase {
    public readonly string id;
    public readonly string token;
    public readonly string data;
    public readonly string channelId;
    //by chenliang
    //beign

    public string deviceId;         //设备ID
    public string channel;          //渠道名称
    public string systemSoftware;   //移动终端操作系统版本
    public string systemHardware;   //移动终端机型
    public string ip;               //IP地址
    public string mac;              //MAC地址

    //end

    public CS_SDKLogin(string id,string token,string data,string channelId) {
        this.id=id;
        this.token=token;
        this.data=data;
        this.channelId=channelId;
        //by chenliang
        //begin

        deviceId = SystemInfo.deviceUniqueIdentifier;
        channel = channelId;
        systemSoftware = SystemInfo.operatingSystem;
        systemHardware = DeviceBaseData.deviceName;
        ip = DeviceBaseData.ip;
        mac = DeviceBaseData.mac;

        //end
    }
}



public class CS_Login:LoginServerMessage
{
    public string regtoken;
    public string acc;
    public string pw;
    public readonly string channel = DeviceBaseData.channel;
    public readonly string ip = DeviceBaseData.ip;
    public readonly string mac=DeviceBaseData.mac;
    public readonly int level=LoginNet.msLevel;
};

public class SC_Login : RespMessage
{
    public string tk;
    public string uid;
    public string ip;
    public string port;
    //by chenliang
    //begin

    public int accNotExists;        //账号错误
    public int wrongPw;             //密码错误

    //end
};

//-------------------------------------------------------------------------
// history_server_info
//-------------------------------------------------------------------------
public class History_Server_Info : MessageBase
{
	public int zid;
	public string name; 
	public int level;
};

//-------------------------------------------------------------------------
// login_history_list
public class CS_Login_History_List : LoginServerMessage
{
    public string tk;                        /* 网络令牌 */
};

public class SC_Login_History_List : RespMessage
{
    public History_Server_Info[] arr;
};

//-------------------------------------------------------------------------
// game server login
//-------------------------------------------------------------------------
public class CS_GameServerLogin : GameServerDeviceBase
{
    public readonly string mac=DeviceBaseData.mac;
    public readonly string ip = DeviceBaseData.ip;
    //by chenliang
    //begin

    public int isWifi;

    public CS_GameServerLogin()
        : base()
    {
        isWifi = (NetManager.HasWifiNetConnectable() ? 1 : 0);
    }

    //end
}

public class SC_GameServerLogin : RespMessage
{
    public string gtk;
    //by chenliang
    //begin

    public int isSeal;          //是否封号
	public string openDate;		//开服时间
    public string zuid = "";
    public string zid = "1";
    
    //end
};
//-------------------------------------------------------------------------
// 获得指定服务器状态
//-------------------------------------------------------------------------
public class CS_GetZoneState : GameServerMessage 
{
    public string mac;
    public CS_GetZoneState() 
    {
        pt = "CS_GetZoneState";
        mac = DeviceBaseData.mac;
    }
}

public class SC_GetZoneState : RespMessage 
{
    public int state;   //> 服务器状态
}

//-------------------------------------------------------------------------
// 分页获得服务器状态
//-------------------------------------------------------------------------
public class CS_PageZoneList : GameServerMessage 
{
    public string mac; //> mac地址
    public int page;            //> 页数
    public int cnt;             //> 每页个数
    public CS_PageZoneList() 
    {
        pt = "CS_PageZoneList";
        mac = DeviceBaseData.mac;
    }
}
public class SC_PageZoneList : RespMessage 
{
    public InfoOfZone[] zoneInfos;
}


//-------------------------------------------------------------------------
// get player data
//-------------------------------------------------------------------------
public class CS_GetPlayerData : GameServerMessage
{
    
}
public class SC_GetPlayerData : RespMessage
{
	public string createDate;
}
//-------------------------------------------------------------------------
// get package data
//-------------------------------------------------------------------------
public class CS_GetPackageList : GameServerMessage
{
    public int pkgId;
}

//-------------------------------------------------
// 添加道具
//-------------------------------------------------
public class CS_AddItem : GameServerMessage
{
    public int packageId;
    public int tid;
}

//-------------------------------------------------------------------------
// create player
//-------------------------------------------------------------------------
public class CS_CreatePlayer : GameServerMessage
{
    public int chaModelIndex;
    public string playerName;
    public readonly string systemSoftware=SystemInfo.operatingSystem;
    public readonly string systemHardware=DeviceBaseData.deviceName;

}

public class SC_CreatePlayer : RespMessage
{

};

//-------------------------------------------------------------------------
// friend
/// <summary>
/// 获得好友申请列表
/// </summary>

public enum FRIEND_ERROR_CODE {
	FRIEND_REQUEST_ALREADY_EXISTS = 1051,       //对同一人已经发出过好友请求
	SELF_FRIEND_FULL = 1052,                    //自己的好友数量已到上限
	SELF_FRIEND_REQUEST_FULL = 1053,            //自己的好友申请列表已满
	OTHER_SIDE_FRIEND_FULL = 1054,              //对方的好友数量已到上限
	OTHER_SIDE_FRIEND_REQUEST_FULL = 1055,      //对方的好友申请列表已满
	FRIENDS_AREADY = 1056                      //双方已经是好友
}

public class CS_RequestFriendRequestList:GameServerMessage {

}

public class SC_RequestFriendRequestList:RespMessage {
	//FriendLogicData
}

/// <summary>
/// 获得好友列表
/// </summary>
public class CS_RequestFriendListHttp:GameServerMessage {

}

public class SC_RequestFriendListHttp:RespMessage {
	//FriendLogicData
}

/// <summary>
/// 查找好友
/// </summary>
public class CS_SearchFriendInfo:GameServerMessage {
	public string name;
}

public class SC_SearchFriendInfo:RespMessage {
	//public string name;
	//public int level;
	//public long lastloginTime;
	//public int icon;
	public FriendData[] arr;
}

/// <summary>
/// 发送好友添加请求
/// </summary>
public class CS_SendFriendRequest:GameServerMessage {
	public string friendId;
    public CS_SendFriendRequest(string friendId) {
        this.friendId=friendId;
    }

    public CS_SendFriendRequest() {}
}

public class SC_SendFriendRequest:RespMessage {
	// ret
	public int friendReqFull;
    public int friendsAlready;
}

/// <summary>
/// 接受添加好友
/// </summary>
public class CS_RequestAcceptFriend:GameServerMessage {
	public string friendId;
}

public class SC_RequestAcceptFriend:RespMessage {
    public int friendFull;
}

/// <summary>
/// 拒绝添加
/// </summary>
public class CS_RequestRejectFriend:GameServerMessage {
	public string friendId;
}

public class SC_RequestRejectFriend:RespMessage {
	// ret
}

/// <summary>
/// 删除好友
/// </summary>
public class CS_RequestDeleteFriend:GameServerMessage {
	public string friendId;
}

public class SC_RequestDeleteFriend:RespMessage {
	// ret
}

public class CS_RequestFriendRecommendList:GameServerMessage {

}

public class SC_RequestFriendRecommendList:RespMessage {
	// friendlogic data
}

// 拉赠送体力列表
public class CS_RequestGetSpiritSendList:GameServerMessage {

}

public class SC_RequestGetSpiritSendList:RespMessage {
	public string[] arr;
}

public class CS_RequestSendSpirit:GameServerMessage {
	public string friendId;
}

public class CS_RequestSendMeSpiritList:GameServerMessage
{

}

public class SC_RequestSendMeSpiritList:GameServerMessage
{
    public string[] arr;
    public int leftCnt;
}

public class CS_RequestAquireSpirit : GameServerMessage
{
    public string[] friendIds;
}

public class SC_RequestAquireSpirit : GameServerMessage
{
    public string[] idRcv;
}

public class SC_RequestSendSpirit:RespMessage {

}

// 参观好友
public class CS_RequestVisitPlayer:GameServerMessage {
	public string targetId;
}

public class VisitPlayerStruct{
	public int tid;
	public int level;
}

// 玩家信息走马灯
public class WallPaper
{
    public int type;
    public string name;
    public int charTid; //其他玩家的品质id,50001,50002等
    public int[] tidArr;
    public int propId;
}

public class SC_ResponeVisitPlayer:RespMessage {
	public string name;
    public int power;
    public string guildName;
	public VisitPlayerStruct[] arr = new VisitPlayerStruct[4];
}

//-------------------------------------------------------------------------
// 请求包裹数据
//-------------------------------------------------------------------------
public class CS_RequestPackageData : GameServerMessage
{
}

//-------------------------------------------------
// 获得各系统状态
//-------------------------------------------------
public class CS_GetNotification : GameServerMessage
{
}
//by chenliang
//begin

public class SC_GetNotification : RespMessage
{
    public int[] chatNo;            //当前聊天消息索引
    public int gold;                //银币
    public int diamond;             //元宝
    public int stamina;             //体力
    public int spirit;              //精力
    public int beatDemonCard;       //降魔令
    public int staminaStamp;        //体力开始恢复时间
    public int spiritStamp;         //精力开始恢复时间
    public int beatDemonCardStamp;  //降魔令开始恢复时间
    //by chenliang
    //begin

    public float power;         //战斗力
    public int vipLevel;
    public int vipExp;
    public int money;
    public int[] arr;

    //end
    public WallPaper wallPaper;
}

public class GetNotification_Requester:
    NetRequester<CS_GetNotification, SC_GetNotification>
{
    public static IEnumerator StartRequester()
    {
        GetNotification_Requester tmpRequester = new GetNotification_Requester();
        yield return tmpRequester.Start();

        if (tmpRequester.respMsg != null && tmpRequester.respMsg.ret == 1)
        {
            //by chenliang
            //begin

//             Dictionary<string, object> dicSystemState = JCode.Decode<Dictionary<string, object>>(tmpRequester.respText);
//             Array stateList = dicSystemState["arr"] as Array;
//------------------------
            Array stateList = tmpRequester.respMsg.arr as Array;

            //end
            SystemStateManager.SetUnionNotifStateFalse();
            SystemStateManager.SetAllNotifState(stateList);

            if (CommonParam.isTeamManagerInit && Mathf.Abs(tmpRequester.respMsg.power - GameCommon.GetPower()) >= GameCommon.PowerDelta)
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_DATA_VERIFY_FAILED);
                DataCenter.SetData("MESSAGE", "SET_PANEL_DEPTH", 300);
            }

            RoleLogicData.Self.gold = tmpRequester.respMsg.gold;
            RoleLogicData.Self.diamond = tmpRequester.respMsg.diamond;
            RoleLogicData.Self.stamina = tmpRequester.respMsg.stamina;
            RoleLogicData.Self.spirit = tmpRequester.respMsg.spirit;
            RoleLogicData.Self.beatDemonCard = tmpRequester.respMsg.beatDemonCard;
            RoleLogicData.Self.staminaStamp = tmpRequester.respMsg.staminaStamp;
            RoleLogicData.Self.spiritStamp = tmpRequester.respMsg.spiritStamp;
            RoleLogicData.Self.beatDemonCardStamp = tmpRequester.respMsg.beatDemonCardStamp;
            RoleLogicData.Self.vipLevel = tmpRequester.respMsg.vipLevel;
            RoleLogicData.Self.vipExp = tmpRequester.respMsg.vipExp;
            RoleLogicData.Self.money = tmpRequester.respMsg.money;

            //设置是否在游戏中
            LoginData.Instance.IsInGameScene = true;

            //by chenliang
            //begin

//             //刷新左上角玩家信息
//             DataCenter.SetData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_ROLE_NAME", null);
//             DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "UPDATE_MARK", true);
//------------------
            //刷新左上角玩家信息
            yield return null;
            DataCenter.SetData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_ROLE_NAME", null);
            yield return null;
            DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "UPDATE_MARK", true);
            yield return null;

            //end

            // 刷新活动红点信息
            DataCenter.SetData("ROLE_SEL_TOP_RIGHT_GROUP", "UPDATE_ACTIVITY_LIST_BTN_NEWMARK", null);
            yield return null;

            //刷新顶部数据
            GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_VITAKITY", true);
            GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_GOLD", true);
            GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND", true);
            GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_FRIEND_POINT", true);

            //检查体力、精力恢复
            RoleInfoTimerManager.Instance.CheckAndStartRecover(ROLE_INFO_TIMER_TYPE.STAMINA, false);
            RoleInfoTimerManager.Instance.CheckAndStartRecover(ROLE_INFO_TIMER_TYPE.SPIRIT, false);
            RoleInfoTimerManager.Instance.CheckAndStartRecover(ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD, false);
            
            // 开启玩家信息走马灯
            if (tmpRequester.respMsg.wallPaper != null)
                DataCenter.SetData("ROLE_SEL_BOTTOM_LEFT_GROUP", "DO_PLAYER_ROLL_PLAYING", tmpRequester.respMsg.wallPaper);

            //聊天新消息
            int[] tmpChatNo = tmpRequester.respMsg.chatNo;
            for (int i = 0, count = tmpChatNo.Length; i < count; i++)
            {
                if (ChatWindow.HasNewMessage(tmpChatNo[i], (ChatType)i))
                {
                    ChatNewMarkManager.Self.SetChatStateByType((ChatType)i,true);
                    //GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_CHAT_MARK", true);
                    //break;
                }
            }
            //主界面聊天红点
            ChatNewMarkManager.Self.RefreshChatNewMark();

            // 商店红点状态
            //1.商店是否有普通免费抽卡次数
            ShopNewMarkManager.Self.CheckNormalFreeDraw = SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_SHOP_NORMAL);
            ShopNewMarkManager.Self.CheckAdvanceFreeDraw = SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_SHOP_ADVANCE);
            //2.是否有符令
            if (ConsumeItemLogicData.Self != null)
            {
                ShopNewMarkManager.Self.CheckSilverFL = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND) > 0;
                ShopNewMarkManager.Self.CheckTenSilverFL = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND) >= 10;
                ShopNewMarkManager.Self.CheckGoldFL = PackageManager.GetItemLeftCount((int)ITEM_TYPE.GOLD_FL_COMMAND) > 0;                
            }
            //3.是否有礼包
            ShopNewMarkManager.Self.CheckSalePackage = SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_SHOP_VIP_GIFT);
            DataCenter.SetData("ROLE_SEL_TOP_RIGHT_GROUP", "UPDATE_SHOP_MARK", null);
        }

        //by chenliang
        //begin

        SystemStateManager.SaveAllStates();

        //end
    }

	protected override CS_GetNotification GetRequest()
    {
        CS_GetNotification tmpReq = new CS_GetNotification();
        return tmpReq;
    }
}

//end

public class CS_ReqMails : GameServerMessage {

}

public class SC_ReqMails : RespMessage {
	public MailData[] mails;
}

public class CS_RequestMailItem : GameServerMessage {
	public long mailId;
}

public class SC_RequestMailItem : RespMessage {
//	public int tid;
//	public int itemId;
//	public int itemNum;
	public ItemDataBase[] items;
}

public class CS_RequestMailAllItem : GameServerMessage {

}

public class SC_RequestMailAllItem : RespMessage {
	public ItemDataBase[] items;
}

public class AllMailDataStruct {
	public int tid;
	public int itemId;
	public int itemNum;
}
//-------------------------------------------------------------------------
// 请求装备强化数据
//-------------------------------------------------------------------------
public class CS_StrengthenEquip : GameServerMessage
{
	public int itemId;
	public int tid;
	public int strenTimes;
}
public class SC_StrengthenEquip : RespMessage
{
	public int succTimes;
	public int upgradeLevel;
	public int costGold;   // 强化的花费,(新加) 
	
}

//-------------------------------------------------------------------------
// 天命值请求
//-------------------------------------------------------------------------
public class CS_FateQuery : GameServerMessage
{
	public int itemId;			//唯一ID
	public int tid;
}
public class SC_FateQuery : RespMessage
{
	public int fateValue;
}
//-------------------------------------------------------------------------
// 天命等级升级请求
//-------------------------------------------------------------------------
public class CS_FateUpgrade : GameServerMessage
{
	public ItemDataBase consume;
	public int itemId;			//唯一ID
	public int tid;
}
public class SC_FateUpgrade : RespMessage
{
	public int isFateSuccess;
	public int fateValue;
}

//-------------------------------------------------------------------------
// 限时抢购界面刷新
//-------------------------------------------------------------------------
public class HaveBuyNum
{
    public int goodsIndex;
    public int hadBuyNum;    
}
public class CS_LimitTimeSale : GameServerMessage
{
}
public class SC_LimitTimeSale : RespMessage
{
    public HaveBuyNum[] haveBuyNum;    //已购买次数
}

//-------------------------------------------------------------------------
// 限时抢购
//-------------------------------------------------------------------------
public class CS_BuyCheapWares : GameServerMessage
{
    public int buyWareIndex;
}
public class SC_BuyCheapWares : RespMessage
{
    public ItemDataBase randWareID;
    public int haveBuyNum;    //已购买次数
}

//-------------------------------------------------------------------------
// TODO  测试发送邮件
//-------------------------------------------------------------------------
public class CS_SendMail : GameServerMessage
{
	public string title;
	public string content;
	public ItemDataBase[] items;
}
public class SC_SendMail : GameServerMessage
{
	public int tid;
	public string title;
}
//-------------------------------------------------------------------------
// TODO  测试发送公会经验
//-------------------------------------------------------------------------
public class CS_AddGuildExp : GameServerMessage
{
	public int exp;
}
public class SC_AddGuildExp : GameServerMessage
{

}

//-------------------------------------------------
// 角色、装备、法器上下阵
//-------------------------------------------------
public class TeamPosChangeData : GameServerMessage
{
    public int teamPos;
    public int upItemId;
    public int upTid;
    public int downItemId;
    public int downTid;
}
public class CS_BodyEquipChange : TeamPosChangeData
{
}

public class SC_BodyEquipChange : RespMessage
{
}

public class CS_PetLineupChange : TeamPosChangeData
{
}

public class SC_PetLineupChange : RespMessage
{
}
//-------------------------------------------------------------------------
// 突破请求
//-------------------------------------------------------------------------
public class CS_BreakUpgrade : GameServerMessage
{
	public ItemDataBase[] consume;
	public int itemId;			
	public int tid;
}
public class SC_BreakUpgrade : RespMessage
{
	public bool isBreakSuccess;
}
//宠物分解请求
public class CS_PetDisenchant:GameServerMessage
{
    public readonly ItemDataBase[] arr;

    public CS_PetDisenchant(ItemDataBase[] nativeDataArr)
    {
        this.arr = nativeDataArr;
    }
}

public class SC_PetDisenchant:RespMessage
{
    public ItemDataBase[] arr;
}

public class CS_EquipDisenchant:GameServerMessage
{
    public ItemDataBase[] arr;

    public CS_EquipDisenchant(ItemDataBase[] nativeDataArr)
    {
        this.arr = nativeDataArr;
    }
}

public class SC_EquipDisenchant : RespMessage
{
    public ItemDataBase[] arr;
}
public class CS_MagicRebirth : GameServerMessage
{
    public ItemDataBase item;

    public CS_MagicRebirth(ItemDataBase item)
    {
        this.item = item;
    }
}

public class SC_MagicRebirth : RespMessage
{
    public ItemDataBase[] arr;
}

public class CS_PetRebirth : GameServerMessage
{
    public ItemDataBase item;

    public CS_PetRebirth(ItemDataBase item)
    {
        this.item = item;
    }
}

public class SC_PetRebirth : RespMessage
{
    public ItemDataBase[] arr;
}

//-------------------------------------------------------------------------
// 是否有月卡以及是否可领
//-------------------------------------------------------------------------
public class CS_MonthCardQuery : GameServerMessage
{

}
public class SC_MonthCardQuery : RespMessage
{
   public bool haveCheapCard;
   public bool haveExpensiveCard;
   public int[] cardArr;
   public int cheapLeft;
   public int expensiveLeft;

}
//-------------------------------------------------------------------------
// 请求领取月卡奖励
//-------------------------------------------------------------------------
public class CS_MonthCardReward : GameServerMessage
{
    public int index;
}
public class SC_MonthCardReward : RespMessage
{
    public int diamond;
}

//-------------------------------------------------------------------------
// 请求单充福利
//-------------------------------------------------------------------------
public class CS_GetSingleRechargeInfo : GameServerMessage
{ 
    
}
public class SC_GetSingleRechargeInfo : RespMessage
{
    public long beginTime;
    public long endTime;
    public SingleRecharge[] singleRecharges;
}

public class SingleRecharge
{
    public int index;
    public int rechargeCnt;
    public int revCnt;
}

//-------------------------------------------------------------------------
// 请求领取单充福利奖励
//-------------------------------------------------------------------------
public class CS_RevSingleRechargeReward : GameServerMessage
{
    public int index;
}
public class SC_RevSingleRechargeReward : RespMessage
{
    public ItemDataBase[] rewards;
}

//-------------------------------------------------------------------------
// 主线副本数据
//-------------------------------------------------------------------------

public class CS_BattleMainMap : GameServerMessage
{ }

public class SC_BattleMainMap : RespMessage
{
    public MapData[] arr;
}


//-------------------------------------------------------------------------
// 主线副本请求战斗
//-------------------------------------------------------------------------

public class CS_BattleMainStart : GameServerMessage
{
    public int battleId = -1;
}

public class SC_BattleMainStart : RespMessage
{ }


//-------------------------------------------------------------------------
// 主线副本战斗结算
//-------------------------------------------------------------------------

public class CS_BattleMainResult : GameServerMessage
{
    public int battleId = -1;
    public int isWin = 0;
    public int starRate = -1;
    public int isAuto = 0;
}

//TODO
public class SC_BattleMainResult : RespMessage
{
    public ItemDataBase[] arr;
    //public int tid = -1;
    //public int bossIndex = -1;
    public Boss_GetDemonBossList_BossData demonBoss;
}


//-------------------------------------------------------------------------
// 主线副本扫荡
//-------------------------------------------------------------------------

public class CS_BattleMainSweep : GameServerMessage
{
    public int battleId = -1;
    public ItemDataBase sweepObj;
}

//TODO
public class SC_BattleMainSweep : RespMessage
{
    public ItemDataBase[] arr;
    //public int tid = -1;
    //public int bossIndex = -1;
    public Boss_GetDemonBossList_BossData demonBoss;
}


//-------------------------------------------------------------------------
// 主线副本退出
//-------------------------------------------------------------------------

public class CS_ExitBattleMain : GameServerMessage
{
    public int battleId = -1;
    public int isAuto = 0;
}

public class SC_ExitBattleMain : RespMessage
{ }


//-------------------------------------------------------------------------
// 活动副本数据
//-------------------------------------------------------------------------

public class CS_BattleActiveMap : GameServerMessage
{ }

public class SC_BattleActiveMap : RespMessage
{
    public MapData[] arr;
}



//-------------------------------------------------------------------------
// 活动副本状态
//-------------------------------------------------------------------------

public class CS_BattleActiveFresh : GameServerMessage
{ }

public class SC_BattleActiveFresh : RespMessage
{
    public int freshFlag = 0;
}


//-------------------------------------------------------------------------
// 活动副本请求战斗
//-------------------------------------------------------------------------

public class CS_BattleActiveStart : GameServerMessage
{
    public int battleId = -1;
}

public class SC_BattleActiveStart : RespMessage
{ }


//-------------------------------------------------------------------------
// 活动副本战斗结算
//-------------------------------------------------------------------------

public class CS_BattleActiveResult : GameServerMessage
{
    public int battleId = -1;
    public int isWin = 0;
    public int starRate = -1;
}

public class SC_BattleActiveResult : RespMessage
{
    public ItemDataBase[] arr;
}


/// <summary>
/// 技能升级
/// </summary>
public class CS_RequestSkillLevelUp : GameServerMessage {
	public int itemId;
	public int skillIndex;
}

public class SC_RequestSkillLevelUp : RespMessage {

}

/// <summary>
/// lottery query 
/// </summary>
/// 
public class LotteryQuery {
	public long leftTime;
	public int leftNum;
	public int isFree;
}
public class CS_RequestLotteryQuery:GameServerMessage {

}

public class SC_ResponseLotteryQuery:RespMessage {
	public LotteryQuery normalLottery;
	public LotteryQuery preciousLottery;
    public int times;           //已经单次高级抽奖次数
}

// one common
public class CS_RequestNormalLottery:GameServerMessage {
	public ItemDataBase consume;
}

public class SC_ResponseNormalLottery:RespMessage {
	public ItemDataBase reward;
    public int freeGold;
}
// one end

// ten common
public class CS_RequestTenNormalLottery:GameServerMessage {
	public ItemDataBase consume;
}

public class SC_ResponseTenNormalLottery:RespMessage {
	public ItemDataBase[] reward;
    public int freeGold;
}
// ten end

// one advance
public class CS_RequestPreciousLottery:GameServerMessage {
	public ItemDataBase consume;
}

public class SC_ResponsePreciousLottery:RespMessage {
	public ItemDataBase reward;
    public int freeGold;
}
// one advance end

// ten advace
public class CS_RequestTenPreciousLottery:GameServerMessage {
	public ItemDataBase consume;
}

public class SC_ResponseTenPreciousLottery:RespMessage {
	public ItemDataBase[] reward;
    public int freeGold;
}
// ten end

// pet level up start
public class CS_PetUpgrade : GameServerMessage
{
    public int itemId;
    public int tid;
    public ItemDataBase[] arr;
}

public class SC_PetUpgrade : RespMessage
{
}
// pet level up end

// shop item query
public class CS_RequestPropShopQuery : GameServerMessage {

}

public class ShopPropData {
	public int index;
	public int buyNum;
}

public class SC_ResponsePropShopQuery:RespMessage {
    //by chenliang
    //begin

//	public ShopPropData[] prop;
//------------------
    public List<ShopPropData> prop;

    //end
}
// shop item query end

// buy shop item 
public class CS_RequestShopPurchase : GameServerMessage {
	public int num;
	public int sIndex;
}

public class SC_ResponseShopPurchase:RespMessage {
	public int isBuySuccess;
    //by chenliang

    public ItemDataBase[] buyItem;		//购买的物品

    //end
}

// end buy shop item

// vip shop item
public class CS_RequestVipShop : GameServerMessage {
	public int index;
}

public class SC_ResponseVipShop:RespMessage {
	public int isBuySuccess;
	public ItemDataBase[] buyItem;
}
// end vip shop


#region 公会协议
public abstract class SC_GuildBase:RespMessage
{
    public readonly string guildId;
    public readonly int guildBossRedPoint;
}
public class CS_CreateGuild : GameServerMessage
{
    public readonly string name;
    public readonly string outInfo;
    public CS_CreateGuild(string name,string outInfo)
    {
        this.name = name;
        this.outInfo = outInfo;
    }

}

public class SC_CreateGuild : RespMessage
{
    public readonly GuildBaseObject guildObject;
    public readonly int guildExist;
}


public class CS_GetGuildArr : GameServerMessage
{
    
}
public class SC_GetGuildArr : RespMessage
{
    public readonly GuildBaseObject[] arr;
    public readonly string[] applyArr;
}


public class CS_SearchGuildName:GameServerMessage
{
    public readonly string guildName;
    public CS_SearchGuildName(string guildName)
    {
        this.guildName = guildName;
    }
}

public class SC_SearchGuildName : RespMessage
{
    public readonly GuildBaseObject[] guildObjectArr;
    public readonly string[] applyArr;
}

public class CS_GetGuildMemberArr:GameServerMessage
{
    public readonly string zgid = "";
    public CS_GetGuildMemberArr(string guildId)
    {
        this.zgid = guildId;
    }
}

public class SC_GetGuildMemberArr : SC_GuildBase
{
    public readonly GuildBaseObject guildObject;
    public readonly GuildMemberBaseObject[] baseArr;
    public readonly GuildMemberObject[] arr;
}

public class CS_GuildInfoArr : GameServerMessage
{
    public readonly string zgid;
    public CS_GuildInfoArr(string guildId)
    {
        this.zgid = guildId;
    }
}

public class SC_GuildInfoArr : SC_GuildBase
{
    public readonly GuildDynamicInfoObject[] arr;
}

public class CS_GuildAppointMember:GameServerMessage
{
    public readonly string zgid = "";
    public readonly string memberZuid = "";
    public readonly int titleType = -1;
    public CS_GuildAppointMember(string guildId, string memberUid, int titleType)
    {
        this.zgid = guildId;
        this.memberZuid = memberUid;
        this.titleType = titleType;
    }

}
public class SC_GuildAppointMember : SC_GuildBase
{

}

public class CS_GuildCancelAppointMember : GameServerMessage
{
    public readonly string zgid = "";
    public readonly string memberZuid = "";
    public readonly int titleType = -1;
    public CS_GuildCancelAppointMember(string guildId, string memberUid, int titleType)
    {
        this.zgid = guildId;
        this.memberZuid = memberUid;
        this.titleType = titleType;
    }

}
public class SC_GuildCancelAppointMember : SC_GuildBase
{

}

public class CS_GuildRemoveMember : GameServerMessage
{
    public readonly string zgid = "";
    public readonly string memberZuid = "";

    public CS_GuildRemoveMember(string guildId, string memberUid)
    {
        this.zgid = guildId;
        this.memberZuid = memberUid;
    }


}
public class SC_GuildRemoveMember : SC_GuildBase//踢出公会，假如他已经不在公会了
{

}


public class CS_GuildWorship:GameServerMessage
{
    public readonly int worshipType;
    public readonly string zgid;
    public CS_GuildWorship(string guildId, int worshipType)
    {
        this.worshipType = worshipType;
        this.zgid = guildId;
    }

}

public class SC_GuildWorship : SC_GuildBase
{


}

public class CS_GetGuildWorshipReward:GameServerMessage
{
    public readonly int rewardType;
    public readonly string zgid;
    public CS_GetGuildWorshipReward(string guildId, int rewardType)
    {
        this.rewardType = rewardType;
        this.zgid = guildId;
    }
}

public class SC_GetGuildWorshipReward : SC_GuildBase
{
    public readonly ItemDataBase[] arr;
}

public class CS_GuildApplyJoinOrCancel:GameServerMessage
{
    public readonly string zgid;
    public readonly int reqType;

    public CS_GuildApplyJoinOrCancel(string guildId, int reqType)
    {
        this.zgid=guildId;
        this.reqType = reqType;
    }
}

public class SC_GuildApplyJoinOrCancel : RespMessage
{
    public readonly int appliable;
    public readonly int isFull;

}

public class CS_GetApplyMemberArr:GameServerMessage
{
    public readonly string zgid;
    public CS_GetApplyMemberArr(string guildId)
    {
        this.zgid = guildId;
    }
}

public class SC_GetApplyMemberArr : RespMessage
{
    public readonly GuildMemberBaseObject[] arr;
}

public class CS_GuildAddMember:GameServerMessage
{
    public readonly string zgid;
    public readonly string memberZuid;
    public CS_GuildAddMember(string guildId, string memberID)
    {
        this.zgid = guildId;
        this.memberZuid = memberID;
    }
}

public class SC_GuildAddMember : SC_GuildBase
{
    public readonly int memberAddGuild;
    public readonly int isFull;
}

public class CS_GuildRefuseMember : GameServerMessage
{
    public readonly string zgid;
    public readonly string memberZuid;
    public CS_GuildRefuseMember(string guildId, string memberID)
    {
        this.zgid = guildId;
        this.memberZuid = memberID;
    }
}

public class SC_GuildRefuseMember : SC_GuildBase
{

}

public class CS_GuildShopBuy : GameServerMessage
{
    public readonly int gIndex = -1;
    public readonly int num = 0;
    public readonly string zgid = "";
    public CS_GuildShopBuy(int gindex, int num, string gid)
    {
        this.gIndex = gindex;
        this.num = num;
        this.zgid = gid;
    }
}

public class SC_GuildShopBuy : SC_GuildBase
{
    public readonly bool isBuySuccess;
    public readonly ItemDataBase[] buyItem;
}

public class CS_GuildShopQuery : GameServerMessage
{
    public readonly string zgid;
    public CS_GuildShopQuery(string gid)
    {
        this.zgid = gid;
    }
}

public class SC_GuildShopQuery:SC_GuildBase
{
    public readonly BuyObject[] otherArr;
    public readonly BuyObject[] pubLimArr;
    public readonly BuyObject[] priLimArr;
    public readonly long time;
}

public class CS_GuildDissolution : GameServerMessage
{
    public readonly string zgid;
    public CS_GuildDissolution(string guild)
    {
        this.zgid = guild;
    }
}
public class CS_ChangeDemonBossShareState : GameServerMessage
{
	public readonly int state;
	public CS_ChangeDemonBossShareState(int iState)
	{
		this.state = iState;
	}
}
public class SC_ChangeDemonBossShareState : RespMessage
{

}

public class SC_GuildDissolution : SC_GuildBase
{
    public readonly GuildMemberBaseObject[] arr; 
}

public class CS_GetGuildWorshipInfo : GameServerMessage
{
    public readonly string zgid;
    public CS_GetGuildWorshipInfo(string guildId) {
        this.zgid = guildId;
    }
}

public class SC_GetGuildWorshipInfo : SC_GuildBase
{
    public readonly GuildWorshipObject worshipInfo;
    public readonly int[] rewardInfo;
}

public class CS_GuildImpeach : GameServerMessage
{
    public readonly string zgid;
    public CS_GuildImpeach(string guildId)
    {
        this.zgid = guildId;
    }
}
public class SC_GuildImpeach : SC_GuildBase
{
    public readonly int canImpeach;
}

public class CS_GetGuildId : GameServerMessage
{

}

public class SC_GetGuildId : SC_GuildBase//这是刚进工会时收到的消息不用判断是否被踢出
{
    
}

//public class CS_ChangeGuildInInfo : GameServerMessage
//{
//    public readonly string msg;
//    public readonly int guildId;
//    public CS_ChangeGuildInInfo(int guildId,string msg) {
//        this.guildId = guildId;
//        this.msg = msg;
//    } 
//}

//public class SC_ChangeGuildInInfo : SC_GuildBase
//{

//}

public class CS_ChangeGuildInInfo : GameServerMessage
{
     public readonly string outMsg;
     public readonly string inMsg;
     public readonly string zgid;
     public CS_ChangeGuildInInfo(string guildId, string outMsg, string inMsg)
     {
        this.zgid = guildId;
        this.outMsg = outMsg;
        this.inMsg=inMsg;
    } 
}

public class SC_ChangeGuildOutInfo : SC_GuildBase
{

}

#endregion

//-------------------------------------------------------------------------
// 请求自身竞技场排名
//-------------------------------------------------------------------------

public class CS_GetArenaRank : GameServerMessage
{ }

public class SC_GetArenaRank : RespMessage
{
    public int bestRank = -1;
    public int curRank = -1;
}


//-------------------------------------------------------------------------
// 请求竞技场挑战列表
//-------------------------------------------------------------------------

public class ArenaPlayer
{
    public string name = "";
    public int level = -1;
    public int uid = 0;
    public int rank = -1;
    public int power = -1;
}

public class CS_RefreshChallengeList : GameServerMessage
{ }

public class SC_RefreshChallengeList : RespMessage
{
    public ArenaPlayer[] arr;
}

public class CS_ChallengeStart : GameServerMessage
{
    public int targetRank = -1;
    public int targetId = 0;
}

public class SC_ChallengeStart : RespMessage
{
    public int isFighting = 0;
    public int rankChanged = 0;
    public ChallengePlayer opponent;
}


//-------------------------------------------------------------------------
// 竞技场结算
//-------------------------------------------------------------------------

public class CS_ChallengeResult : GameServerMessage
{
    public int isWin = 0;
}

public class SC_ChallengeResult : RespMessage
{
    public ItemDataBase item;
}

//-------------------------------------------------------------------------
// 获取点星界面的功能
//-------------------------------------------------------------------------
public class CS_PointStarMap : GameServerMessage
{
	
}
public class SC_PointStarMap : RespMessage
{
	public int currentIndex;			//当前的索引
}
//-------------------------------------------------------------------------
// 点击点亮按钮
//-------------------------------------------------------------------------
public class CS_PointLightenClick: GameServerMessage
{
	public ItemDataBase[] itemArr;
}
public class SC_PointLightenClick : RespMessage
{
	public int currentIndex;
	public ItemDataBase[] arr;
}
//-------------------------------------------------------------------------
// 点击选择物品确定按钮
//-------------------------------------------------------------------------
//public class CS_SurButtonClick : GameServerMessage
//{
//	public int awardTid;			
//	public int awardItemNum;
//}
//public class SC_SurButtonClick : RespMessage
//{
//	public bool successFlag;
//	public ItemDataBase ItemObject;
//}
//-------------------------------------------------
// 法器强化
//-------------------------------------------------
public class CS_StrengthenMagic : GameServerMessage
{
    public int itemId;
    public int tid;
    public ItemDataBase[] arr;
}

public class SC_StrengthenMagic : RespMessage
{

}

/// <summary>
/// 神装商店
/// </summary>
public class  CS_ClothShopPurchase:GameServerMessage {
	public int indexNum;
	public int num;
	public int[] itemIdArr;
}

public class  SC_ClothShopPurchase:RespMessage {
	public ItemDataBase[] buyItem;
}
public class CS_RequestClothShopQuery:GameServerMessage {

}

public class ClothStruct{
	public int buyNum;
	public int index;
}

public class SC_ResponseClothShopQuery:RespMessage {
	public List<ClothStruct> cloth;
	public int star;
}
// 神装商店end


/// <summary>
/// 声望商店
/// </summary>
public class CS_RequestPrestigeShopQuery:GameServerMessage {

}

public class PrestigeStruce {
	public int buyNum;
	public int index;
}
public class SC_ResponsePrestigeShopQuery:RespMessage {
	public PrestigeStruce [] commodity;
	public int rank;
    public int curRank; //> 角色当前排名
}

// prestige shop buy
public class CS_RequestPrestigeShopPurchase:GameServerMessage {
	public int pIndex;
	public int num;
	public int[] itemIdArr;
}

public class SC_ResponsePrestigeShopPurchase:RespMessage {
	public int isBuySuccess;
	public ItemDataBase[] buyItem;
}
// 声望商店end

// 战功商店

public class FeatsStruct {
	public int buyNum;
	public int index;
}

public class CS_RequestFeatsShopQuery:GameServerMessage {

}

public class SC_ResponseFeatsShopQuery:RespMessage {
	public FeatsStruct[] buyState;
}

public class CS_RequestFeatsShopPurchase:GameServerMessage {
	public int indexNum;
	public int num;
	public int[] itemIdArr;
}

public class SC_ResponseFeatsShopPurchase:RespMessage {
	public int isBuySuccess;
    public ItemDataBase[] buyItem;
}

// end



//-------------------------------------------------------------------------
// 获取日常任务数据
//-------------------------------------------------------------------------

public class TaskObject
{
    public int taskId = -1;
    public int progress = 0;
    public int accepted = 0;
}

public class CS_GetDailyTaskData : GameServerMessage
{ }

public class SC_GetDailyTaskData : RespMessage
{
    public int curScore = -1;
    public int[] scoreAwards;
    public TaskObject[] arr;
}


//-------------------------------------------------------------------------
// 获取成就数据
//-------------------------------------------------------------------------

public class CS_GetAchievementData : GameServerMessage
{ }

public class SC_GetAchievementData : RespMessage
{
    public TaskObject[] arr;
}


//-------------------------------------------------------------------------
// 获取日常任务奖励
//-------------------------------------------------------------------------

public class CS_GetDailyTaskAward : GameServerMessage
{
    public int taskId = -1;
}

public class SC_GetDailyTaskAward : RespMessage
{
    public ItemDataBase[] arr;
}


//-------------------------------------------------------------------------
// 获取成就奖励
//-------------------------------------------------------------------------

public class CS_GetAchievementAward : GameServerMessage
{
    public int taskId = -1;
}

public class SC_GetAchievementAward : RespMessage
{
    public ItemDataBase[] arr;
}


//-------------------------------------------------------------------------
// 获取任务积分奖励
//-------------------------------------------------------------------------

public class CS_GetTaskScoreAward : GameServerMessage
{
    public int awardId = -1;
}

public class SC_GetTaskScoreAward : RespMessage
{
    public ItemDataBase[] arr;
}

//-------------------------------------------------------------------------
//  每日签到请求
//-------------------------------------------------------------------------

public class CS_DailySignQuery : GameServerMessage
{

}

public class SC_DailySignQuery : RespMessage
{
	public int signNum = 1;
    public int isSign = 0;
}
//-------------------------------------------------------------------------
//   每日签到领取
//-------------------------------------------------------------------------

public class CS_DailySign : GameServerMessage
{
	
}

public class SC_DailySign : RespMessage
{
	public bool isSignSuccess;
	public ItemDataBase[] item;
}

//-------------------------------------------------------------------------
//   豪华签到请求
//-------------------------------------------------------------------------

public class CS_LuxurySignQuery : GameServerMessage
{

}

public class SC_LuxurySignQuery : RespMessage
{
	public int isSign;
	public int isRecharge;
}

//-------------------------------------------------------------------------
//   豪华签到领取
//-------------------------------------------------------------------------

public class CS_LuxurySign : GameServerMessage
{
	
}

public class SC_LuxurySign: RespMessage
{
	public bool isSignSuccess;
	public ItemDataBase[] item;
}

/// <summary>
/// 神秘商店数据请求
/// </summary>
public class CS_MysteryShopQuery:GameServerMessage {

}

public class SC_MysteryShopQuery:RespMessage {
	public int[] indexArr;
	public int freeNum;
	public long leftTime;
	public int leftNum;
    //by chenliang
    //begin

    public List<int> mysteryArr;

    //end
}

/// <summary>
/// 神秘商店刷新
/// </summary>
public class CS_MysteryShopRefresh:GameServerMessage {
	public ItemDataBase consume;
}

public class SC_MysteryShopRefresh:RespMessage {
	public int[] indexArr;
}

/// <summary>
/// 神秘商店购买
/// </summary>
public class CS_MysteryShopPurchase:GameServerMessage {
	public int indexNum;
	public int num;
}

public class SC_MysteryShopPurchase:RespMessage {
	public int isBuySuccess;
	public ItemDataBase[] buyItem;
}


//-------------------------------------------------------------------------
// 装备精炼
//-------------------------------------------------------------------------

public class CS_RefineEquip : GameServerMessage
{
    public int itemId = 0;         // 精炼装备的itemId
    public int tid = 0;            // 精炼装备的tid
    public ItemDataBase[] arr;     // 消耗物品列表
}

public class SC_RefineEquip : RespMessage
{ }

//-------------------------------------------------
// 法器精炼
//-------------------------------------------------
public class CS_RefineMagic : GameServerMessage
{
    public int itemId;          // 精炼法器的itemId
    public int tid;             // 精炼法器的tid
    public ItemDataBase[] arr;  // 法器精炼消耗数组
}

public class SC_RefineMagic : RespMessage
{

}

//-------------------------------------------------
// 心跳包
//-------------------------------------------------
public class CS_Heartbeat : GameServerMessage
{
}

public class SC_HeartBeat : RespMessage
{

}


//-------------------------------------------------
// 使用消耗品
//-------------------------------------------------

public class CS_UseProp : GameServerMessage
{
    public ItemDataBase prop;
}

public class SC_UseProp : RespMessage
{
    public ItemDataBase[] items;
}


//-------------------------------------------------
// 使用多选一宝箱
//-------------------------------------------------

public class CS_OpenBoxSelect : GameServerMessage
{
    public ItemDataBase prop;
    public ItemDataBase selectItem;
}

public class SC_OpenBoxSelect : RespMessage
{
    public ItemDataBase[] items;
}

//-------------------------------------------------------------------------
//  摇钱树请求
//-------------------------------------------------------------------------
public class CS_TreeOfGold : GameServerMessage
{
	
}

public class SC_TreeOfGold : RespMessage
{
	public int dayNum;			//今日摇钱次数
	public int allNum;				//总摇钱次数
	public int leftTime;			//距离下次摇钱时间
	public int extraGold;			//额外摇钱总和
}

//-------------------------------------------------------------------------
//   摇钱树摇钱请求
//-------------------------------------------------------------------------
public class CS_ShakeTree : GameServerMessage
{
	
}

public class SC_ShakeTree : RespMessage
{
	public int goldNum;				//
	public int extraGoldNum;		//额外金币数
}

//-------------------------------------------------------------------------
//   获取摇钱树额外奖励
//-------------------------------------------------------------------------
public class CS_TreeExtraGold : GameServerMessage
{
	
}

public class SC_TreeExtraGold : RespMessage
{
	public int extraGold;			//额外摇钱总和
}

//-------------------------------------------------------------------------
//   获取领仙桃请求
//-------------------------------------------------------------------------
public class CS_PowerQuery : GameServerMessage
{
	
}

public class SC_PowerQuery : RespMessage
{
	public bool isPower_1;			//
	public bool isPower_2;			//
}
//-------------------------------------------------------------------------
//   获取领仙桃领取
//-------------------------------------------------------------------------
public class CS_RewardPower : GameServerMessage
{
	
}

public class SC_RewardPower : RespMessage
{

}
//------------------------------------------------------------------------
//   射手乐园获取信息
//------------------------------------------------------------------------
public class CS_ShootPark : GameServerMessage 
{
    public CS_ShootPark() : base() 
    {
        pt = "CS_ShooterQuery";
    }
}

public class SC_ShootPark : RespMessage 
{
    public int addDiamond;  //> 当前累计元宝
    public int usingTimes;  //> 今日已射击次数
}


//-----------------------------------------------------------------------
//   射手乐园射击请求
//-----------------------------------------------------------------------
public class CS_ShootParkShoot : GameServerMessage 
{
    public CS_ShootParkShoot() : base() 
    {
        pt = "CS_ShooterPark";
    }
}

public class SC_ShootParkShoot : RespMessage 
{
    public int earnRatio;    //> 获得的奖励
}


//---------------------------------------------------------------------
//  幸运翻牌获取剩余次数
//---------------------------------------------------------------------
public class CS_LuckyCard : GameServerMessage 
{
    public CS_LuckyCard() : base()
    {
        pt = "CS_FreeDiamond";
    }
}

public class SC_LuckyCard : RespMessage 
{
    public int residueNum;  //> 剩余翻牌次数
}

//---------------------------------------------------------------------
//  幸运翻牌开始翻牌请求
//---------------------------------------------------------------------
public class CS_LuckyCard_Draw : GameServerMessage
{
    public CS_LuckyCard_Draw() : base() 
    {
        pt = "CS_transformCard";
    }
}

public class SC_LuckyCard_Draw : RespMessage 
{
    public int diamondIndex; //> 元宝数量
    public int residueNum;   //> 剩余翻牌次数
}
//---------------------------------------------------------------------
//  充值送礼获取当前领取情况的请求
//---------------------------------------------------------------------
public class CS_RechargeGiftQuery : GameServerMessage 
{
    public CS_RechargeGiftQuery() : base() 
    {
        pt = "CS_ChargeRewardQuery";
    }
}

public class SC_RechargeGiftQuery : RespMessage 
{
    public int money;                         //> 累计充值金额
    public RechargeQueryInfo[] indexArr;      //> 当前已经领取过奖励的索引数组
}
//---------------------------------------------------------------------
//  充值送礼奖励领取的请求
//---------------------------------------------------------------------
public class CS_GetRechargeGift : GameServerMessage 
{
    public CS_GetRechargeGift(): base()
    {
        pt = "CS_ChargeReward";
    }
    public int rmbNum;  //> 奖励金额
}
public class SC_GetRechargeGift : RespMessage 
{
    public ItemDataBase[] awardArr;    //> 获得的奖励数组
}

//---------------------------------------------------------------------
//  累计消费获取当前领取情况的请求
//---------------------------------------------------------------------
public class CS_CumulativeQuery : GameServerMessage 
{
    public CS_CumulativeQuery() : base() 
    {
        pt = "CS_CostEvent";
    }
}

public class SC_CumulativeQuery : RespMessage 
{
    public int costNums;        //> 累计消耗元宝
    public int[] prizeStatus;   //> 领取状态,领取过奖励的消费元宝目标值
    public CumulativeQueryInfo[] chargeAward;  //> 包括消费进度目标,奖励物品信息
}

//---------------------------------------------------------------------
//  累计消费奖励领取的请求
//---------------------------------------------------------------------
public class CS_GetCumulativeGift : GameServerMessage 
{
    public CS_GetCumulativeGift() : base() 
    {
        pt = "CS_receivePrize";
    }
    public int clickItemMoney;     //> 领取奖励所需达到的元宝数量
}

public class SC_GetCumulativeGift : RespMessage 
{
    public ItemDataBase[] prizeContent;
}
//---------------------------------------------------------------------
//  首充礼包领奖情况的请求
//---------------------------------------------------------------------
public class CS_FirstChargeQuery : GameServerMessage 
{
    public CS_FirstChargeQuery() : base() 
    {
        pt = "CS_FirstChargeQuery";
    }
}

public class SC_FirstChargeQuery : RespMessage 
{
    public int code;    //> 首充礼包领取状态 0=>去充值 1=>领取 2=>已领取
}

//---------------------------------------------------------------------
//  首充礼包领取奖励的请求
//---------------------------------------------------------------------
public class CS_GetFirstChargeReward : GameServerMessage 
{
    public CS_GetFirstChargeReward() : base()
    {
        pt = "CS_FirstChargeReward";
    }
}
public class SC_GetFirstChargeReward : RespMessage 
{
    public ItemDataBase[] rewards;  //> 奖励道具
}

//-------------------------------------------------------------------------
//   获取活动结束时间的请求
//-------------------------------------------------------------------------
/// <summary>
/// 活动相关时间
/// </summary>
public class ActivityOpenTime
{
    public long activityOpenTime;           //> 活动开始时间
    public long activityEndTime;            //> 活动结束时间
    public long rewardEndTime;              //> 领奖结束时间
}

public class CS_GetActivitiesEndTime : GameServerMessage 
{
    public CS_GetActivitiesEndTime() : base() 
    {
        pt = "CS_OpenEndTime";
    }
}
public class SC_GetActivitiesEndTime : RespMessage 
{
    public ActivityOpenTime rechargeTime;   //> 充值送礼相关时间
    public ActivityOpenTime cumulativeTime; //> 累计消费相关时间
 
}


//-------------------------------------------------------------------------
//   VIP礼包请求
//-------------------------------------------------------------------------
//VIP每日礼包
public class CS_VIPDailyInfoQuery : GameServerMessage
{
	
}

public class SC_VIPDailyInfoQuery : RespMessage
{
	public Welfare dailyWelfare;
	//public WeekVipPackage[] weekWelfare;
	//public int leftTime;
}
//VIP每周礼包
public class CS_VIPWeeklyInfoQuery : GameServerMessage 
{

}

public class SC_VIPWeeklyInfoQuery : RespMessage 
{
    public WeekVipPackage[] weekWelfare;
}


//-------------------------------------------------------------------------
//   VIP每日福利
//-------------------------------------------------------------------------
public class CS_VIPDaily : GameServerMessage
{
	public int vipLevel;
}

public class SC_VIPDaily : RespMessage
{
	public ItemDataBase[] rewards;			//每日领取的福利 
}
//-------------------------------------------------------------------------
//   VIP每周福利
//-------------------------------------------------------------------------
public class CS_VIPWeek : GameServerMessage
{
	public int index;
}

public class SC_VIPWeek : RespMessage
{
	public ItemDataBase[] rewards;			//每周购买的福利 
	public int rewardVipLevel;
}

//-------------------------------------------------------------------------
//   礼品码兑换
//-------------------------------------------------------------------------
public class CS_GiftCode : GameServerMessage
{
	public string giftCode;
}

public class SC_GiftCode : RespMessage
{
	public ItemDataBase[] exchangeItem;			//兑换礼品数组 
	public int errorIndex;					//无效提示
}

//-------------------------------------------------------------------------
//   开服基金请求 
//-------------------------------------------------------------------------
public class CS_FundQuery : GameServerMessage
{
	
}

public class SC_FundQuery : RespMessage
{
	public int buyNum;
	public int[] fundArr;
	public bool isBuy;
} 
//-------------------------------------------------------------------------
//   开服基金购买 
//-------------------------------------------------------------------------
public class CS_FundPurchase : GameServerMessage
{
	
}

public class SC_FundPurchase : RespMessage
{
	public bool isBuySuccess;
} 
//-------------------------------------------------------------------------
//   开服基金领取 
//-------------------------------------------------------------------------
public class CS_FundReward : GameServerMessage
{
	public int index;
}

public class SC_FundReward : RespMessage
{

} 
//-------------------------------------------------------------------------
//   全民福利领取 
//-------------------------------------------------------------------------
public class CS_WelfareReward : GameServerMessage
{
	public int index;
}

public class SC_WelfareReward : RespMessage
{
	public ItemDataBase[] items;
} 
//-------------------------------------------------------------------------
//   七日登陆请求 
//-------------------------------------------------------------------------
public class CS_SevenDayLoginQuery : GameServerMessage
{
	
}

public class SC_SevenDayLoginQuery : RespMessage
{
	public int loginDay;			//登陆的天数
	public int[] indexArr;		//已领取
} 
//-------------------------------------------------------------------------
//   七日登陆领取 
//-------------------------------------------------------------------------
public class CS_SevenDayLoginReward : GameServerMessage
{
	public int index;				//领取索引
}

public class SC_SevenDayLoginReward : RespMessage
{
	public ItemDataBase[] items;
} 

/// <summary>
/// 保存新手引导进度
/// </summary>
public class CS_SaveGuideProgress : GameServerDeviceBase
{
    public int guideProgress = 0;       // 引导大步骤索引
    public int guideIndex = 0;          // 引导细分步骤索引
    public string name = "";            // 引导步骤名称
}

public class SC_SaveGuideProgress : RespMessage
{ }


/// <summary>
/// 获取新手引导进度
/// </summary>
public class CS_QueryGuideProgress : GameServerMessage
{ }

public class SC_QueryGuideProgress : RespMessage
{
    public int resGuideProgress = 0;
}

//-------------------------------------------------------------------------
// 次日登陆请求
//-------------------------------------------------------------------------
public class CS_LoginRewardQuery: GameServerMessage
{

}
public class SC_LoginRewardQuery : RespMessage
{
	public int[] dayArr;
    public bool open;   //> 明日有礼是否开启
//	public int leftTime;
}
//-------------------------------------------------------------------------
// 次日登陆奖励
//-------------------------------------------------------------------------
public class CS_LoginReward: GameServerMessage
{
	public int day;
}
public class SC_LoginReward : RespMessage
{
	public ItemDataBase[] loginReward;
}
//-------------------------------------------------------------------------
// 获得开服狂欢列表
//-------------------------------------------------------------------------
public class CS_GetRevelryList : GameServerMessage
{

}
public class SC_GetRevelryList : RespMessage
{
	public RevelryObject[] revelryArr;
//	public int leftTime;
}
//-------------------------------------------------------------------------
// 领取开服狂欢奖励
//-------------------------------------------------------------------------
public class CS_TakeRevelryAward : GameServerMessage
{
	public int revelryId;
}
public class SC_TakeRevelryAward : RespMessage
{
    public ItemDataBase[] awards;
    public int revelryId;
}

//-------------------------------------------------------------------------
// 开服狂欢-半价抢购-初始化
//-------------------------------------------------------------------------
public class CS_HalfPriceQuery : GameServerMessage
{
}
public class SC_HalfPriceQuery : RespMessage
{
    public int[] buyArr;
    public int[] useArr;
}

//-------------------------------------------------------------------------
// 开服狂欢-半价抢购-抢购
//-------------------------------------------------------------------------
public class CS_HalfPrice : GameServerMessage
{
    public int whichDay;
}
public class SC_HalfPrice : RespMessage
{
    public ItemDataBase[] buyItem;
}

//-------------------------------------------------------------------------
// 关卡重置
//-------------------------------------------------------------------------
public class CS_ResetBattleMain : GameServerMessage 
{
    public int battleId;
    public int resetCnt;
    public CS_ResetBattleMain() 
    {
        pt = "CS_ResetBattleMain";
    }
}

public class SC_ResetBattleMain : RespMessage 
{
    
}
//-------------------------------------------------------------------------
// gm超级账号工具
//-------------------------------------------------------------------------
public class CS_CreateSuperPlayer : GameServerMessage
{
    public SuperPlayerInfo superPlayerInfo;
    public SuperPetInfo[] superPetInfo;
    public SuperEquipInfo[] superEquipInfo;
    public int stageId;
    public CS_CreateSuperPlayer()
    {
        pt = "CS_CreateSuperPlayer";
    }
}

public class SC_CreateSuperPlayer : RespMessage
{

}
