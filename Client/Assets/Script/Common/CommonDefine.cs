using UnityEngine;
using System.Collections;

//--------------------------------------------------------------------------------------------------
public enum LOAD_MODE
{
	UNKNOW,
	UNICODE,    //目录路径且Unicode编码
	ANIS,       //目录路径且默认编码
	BYTES,      //文件中
	BYTES_RES,  //资源包中
	BYTES_TRY_PATH, //先尝试从PATH中读，再从资源包中读
	BYTES_TRY_RES,  //先从资源中读取，再从包中读
	WWW,
	SCR_FILE,
	RESOURCE,
}

public enum ELEMENT_RELATION
{
	ADVANTAGEOUS,
	BLANCE,
	INFERIOR,
}

public enum ELEMENT_TYPE
{
	RED,
	BLUE,
	GREEN,
	GOLD,
	SHADOW,
	MAX,
}

public enum PET_TYPE
{
	ATTACK,
	DEFENCE,
	ASSIST,
}

public enum STAGE_TYPE
{
    MAIN_COMMON = 1,    // 主线普通关卡
    MAIN_ELITE,         // 主线精英关卡
    MAIN_MASTER,        // 主线大师关卡
    CHAOS,              // 天魔乱入关卡
    EXP,                // 经验蛋关卡（每日关卡）
    MONEY,              // 金币关卡（每日关卡）
    TOWER,              // 秘境关卡
    //by chenliang
    //begin

//    PVP4                // 4V4关卡
//---------------------
    PVP4,               // 4V4关卡
    GRAB,               //夺宝关卡
    FAIRYLAND,          //仙境关卡
    GUILDBOSS,          //公会boss

    //end
}

/// <summary>
/// 摄像机参数类型
/// </summary>
public enum CAMERA_TYPE
{
    DEFAULT = 1,
    PVP,
    CHAOS,
}

/// <summary>
/// item type
/// </summary>
public enum ITEM_TYPE
{
	GEM,    	        // 灵石	    
	EQUIP = 14, // 装备
    MAGIC = 15, // 法器
    EQUIP_FRAGMENT = 16, // 装备碎片
    MAGIC_FRAGMENT = 17, // 法器碎片
    PET = 30, // 符灵
    PET_FRAGMENT = 40, // 符灵碎片
    CHARACTER = 50, // 主角
    MONSTER = 70, // 怪物 
    ROLE_ATTRIBUTE = 1000, // 消费品
    CONSUME_ITEM = 2000, // 消费品
    NOVICE_PACKS = 2001, // 新手礼包
    YUANBAO = 1000001, // 元宝
    GOLD = 1000002, // 银币
    POWER = 1000003, // 体力
	PET_SOUL = 1000004, // 符魂
	SPIRIT = 1000005, // 精力值
	REPUTATION = 1000006, // 声望（竞技场货币）
	PRESTIGE = 1000007, // 威名（群魔乱舞货币）
	BATTLEACHV = 1000008, // 战功
	UNIONCONTR = 1000009, // 公会贡献(公会货币)
	BEATDEMONCARD = 1000010, // 降魔令
	CHARACTER_EXP = 1000011, //主角经验

    // 
	FATE_STONE = 2000001, // 天命石
	BREAK_STONE = 2000002, // 突破石
	MAGIC_REFINE_STONE = 2000003, // 法器精炼石
	LOW_REFINE_STONE = 2000004, // 低级精炼石
	MIDDLE_REFINE_STONE = 2000005, // 中级精炼石
	HIGH_REFINE_STONE = 2000006, // 高级精炼石
	BEST_REFINE_STONE = 2000007, // 极品精炼石
	SKILL_BOOK = 2000008,//技能书
    SAODANG_POINT = 2000009, // 扫荡卷
	
	SILVER_FL_COMMAND = 2000010, // 银符令
	GOLD_FL_COMMAND = 2000011, // 金符令
	POINT_STAR_TONE = 2000012, // 点星石
    STAMINA_ITEM = 2000013,         // 体力丹
    SPIRIT_ITEM = 2000014,          // 精力丹
    //by chenliang
    //begin
    BEATDEMONCARD_ITEM = 2000015,            //降魔令
    TRUCE_TOKEN_BIG = 2000016,          //8小时免战牌
    TRUCE_TOKEN_SMALL = 2000017,        //1小时免战牌

    //技能、BUFF
    SKILL_TYPE_ACTIVE = 100,                //主动技能 100-199
    SKILL_TYPE_PASSIVE = 200,               //被动技能
    SKILL_TYPE_BUFF_PASSIVE = 300,          //被动BUFF
    SKILL_TYPE_BUFF_BREAK = 310,            //突破BUFF
    SKILL_TYPE_BUFF_EQUIP = 320,            //装备套装
    SKILL_TYPE_BUFF_EXT_1 = 330,            //附加BUFF1
    SKILL_TYPE_BUFF_EXT_2 = 340,            //附加BUFF2
    SKILL_TYPE_BUFF_EXT_3 = 350,            //附加BUFF3
    SKILL_TYPE_BUFF_RELATIONSHIP = 360,     //武将缘分BUFF
    SKILL_TYPE_BUFF_EXT_4 = 370,            //附加BUFF5
    SKILL_TYPE_BUFF_EXT_5 = 380,            //附加BUFF6
    SKILL_TYPE_BUFF_EXT_6 = 390,            //附加BUFF7

	//end

	REFRESH_TOKEN = 2000018,  // 刷新令
   

	PET_EQUIP, // 装备
	AIR, // 空气
    
    RESET_POINT, // 重铸卷
    LOCK_POINT, // 锁定卷
    
    
    MATERIAL, // 材料
    MATERIAL_FRAGMENT, // 材料碎片
	HONOR_POINT, //荣誉点
}

//---------------------------------------------------------
/// <summary>
/// task page type
/// </summary>
public enum TASK_PAGE_TYPE
{
	DAILY = 1,          // 每日任务
	ACHIEVEMENT = 2,        // 成就任务
	WEEKLY = 3,             // 每周任务   
	ACTIVITY = 4,           // 活动任务   
	ACHIEVEMENT_DILIVER,// 已交付成就任务
}

/// <summary>
/// task accept contion
/// </summary>
public enum Task_Accept_Condition
{
	No_Condition,               // 无前置条件      
	Get_Designated_Pet,         // 获得指定侠客
	Level,                      // 玩家等级
	Task,                       // 任务
}

/// <summary>
/// task type
/// </summary>
public enum TASK_TYPE
{
	Get_Proficiency,            // 获得顶级熟练度
	PVE_Win_Satge,              // 通关关卡
	PVE_Enter_Satge,            // 进入关卡
	PVP_Finish_Stage,           // 完成PVP
	Kill_Monster,               // 杀死怪物
	PVP_Ranking,                // PVP排名
	Get_Glod,                   // 收集金币
	Get_Gem,                    // 收集灵石
	Game_Friend_Num,            // 游戏好友数量
	Plan_Friend_Num,            // 平台好友数量
	Pet_Num,                    // 累计符灵数量
	Send_Friendly_Num,          // 赠送好友度次数
	Pet_Upgrade_Num,            // 符灵升级次数
	Pet_Strengthen_Num,         // 符灵强化次数
	Pet_Evolution_Num,          // 符灵进化次数
	Sale_Pet_Num,               // 出售符灵次数
	Role_Level_Up,              // 玩家升级
	Role_Get_Star_Num,          // 玩家获得指定星级数量
	Max
}

/// <summary>
/// task state
/// </summary>
public enum TASK_STATE
{
	Can_Not_Accept, // 不可接取
	//Can_Accept,     // 可接取
	Had_Accept,     // 已接取
	Finished,       // 已完成
	Deliver,        // 已交付（已领取奖励）
}

//---------------------------------------------------------
public enum SHOP_PAGE_TYPE
{
    NONE,
	PET = 1,
	TOOL,    
	CHARACTER,
	GOLD,
	DIAMOND,
	MYSTERIOUS,
}

//---------------------------------------------------------
public enum TUJIAN_STATUS
{
	TUJIAN_NOTHAD = -1, //for  pet of predestined 
	
	TUJIAN_NEW = 0,
	TUJIAN_NORMAL = 1,
	TUJIAN_REWARD = 2,
	TUJIAN_FULL = 3
};

//---------------------------------------------------------
public enum STRING_INDEX
{
    ERROR_NONE = 1,
    ERROR_INVALID_PET = 2,                      		// 无效的符灵
    ERROR_INVALID_FRIEND = 3,                   		// 无效的好友
    ERROR_INVALID_CHAR = 4,                     		// 角色索引不正确
    ERROR_NO_POINT = 5,                         		// 体力值不足
    ERROR_NO_ENOUGH_MONEY = 6,                  		// 没有足够的金钱
    ERROR_NO_ENOUGH_GEM = 7,                    		// 没有足够的灵石
    ERROR_NO_ENOUGH_DIAMOND = 8,                		// 没有足够的元宝
    ERROR_MAX_ENHANCE = 9,                      		// 强化等级已达上限
    ERROR_INVALID_MAIL = 10,                    		// 无效的邮件
    ERROR_INVALID_MISSION = 11,                 		// 无效的任务
    ERROR_PET_FULL = 12,                        		// 符灵已满
    ERROR_FABAO_FULL = 13,                      		// 法宝已满
    ERROR_ITEM_FULL = 14,                       		// 装备已满
    ERROR_INVALID_FABAO = 15,                   		// 无效的法宝
    ERROR_FABAO_ENHANCE_MAX = 16,               		// 法宝强化已达上限
    ERROR_MAX_DAILY_POINT = 17,                 		// 每日赠送好友点已达上限
    ERROR_INVALID_GEM = 18,                     		// 无效的灵石
    ERROR_FABAO_RESET_MAX = 19,                 		// 法宝重铸已达上限
    ERROR_NEED_PASSWORD = 21,                      		// 需要正确密码
    ERROR_GENERIC = 22,                         		// 一般错误
    ERROR_SERVER_INBUSY = 23,                   		// 服务器繁忙忙
    ERROR_INVALID_SKIN = 24,                            // 无效的皮肤
    ERROR_INVALID_PLAYER = 25,                          // 玩家不存在
    ERROR_PLAYER_OFFLINE = 26,                          // 玩家不在线
    ERROR_TIME_OUT = 27,                                // 连接超时
    ERROR_DENY_ACCOUNT_LOGIN = 28,                      //您的账号被封号
    ERROR_SERVER_500 = 29,								//505
    ERROR_SERVER_404 = 30,								//404
    ERROR_SERVER_CANOT_CONNECT_TO_HOST = 31,			//无法连接到服务器
    ERROR_HTTP_PACKAGE_INDEX_REPEAT = 32,               //包序号重复
    ERROR_REQUEST_ERROR = 33,                           //请求错误
    ERROR_DATA_VERIFY_FAILED = 34,                      //数据验证失败
    ERROR_DATA_HAS_TAMPER = 35,                         //数据已被篡改
    ERROR_GTOKEN_TIME_OUT = 80,						    // Game Server Token超时
    ERROR_OTHER = 99,									// 其它错误
    ERROR_NAME_ILLEGAL = 101,                           // 名称不合法
    ERROR_NAME_EXIST = 102,                             // 玩家名已存在
    ERROR_PLAYER_NOT_EXIST = 103,                       // 玩家数据不存在
    ERROR_NAME_TOO_LONG = 104,                          //玩家名太长   
	ERROR_NO_ENOUGH_RESOURCE = 105,             		// 资源不足，无法购买
    ERROR_PLAYER_ALREADY_EXIST = 106,                   //玩家数据已存在
	ERROR_MISSION_HAS_DONE = 120,               		// 已经完成过的任务
    ERROR_MISSION_LOW_STEP = 121,               		// 任务进度不足
    ERROR_AQUIRE_PLAYER_INFO = 122,                     // 获取玩家信息失败
	ERROR_INVALID_COMBAT = 200,                 		// 战斗结果无效
														
	ERROR_SHOP = 201,							   	
	ERROR_SHOP_NO_ENOUGH_GOLD = 202,					//金币不足
	ERROR_SHOP_NO_ENOUGH_DIAMOND = 203,					//钻石不足
	ERROR_SHOP_NO_ENOUGH_FRIEND_POINT = 204,			//友情点不足
	ERROR_SHOP_BUY_SUCCESS = 205,					    //购买成功
	ERROR_SHOP_FULL_MAIL = 206,						    //邮箱将满，抽取的物品可能部分被丢弃，请清理邮箱后再来抽取
	ERROR_SHOP_NO_ENOUGH_HONOR_POINT = 207,				//荣誉点不足
    ERROR_SHOP_NO_ENOUGH_BUY_NUM = 208,                 //没有足够的购买次数
    ERROR_SHOP_VIP_LEVEL_LOW = 209,                     //VIP等级不足
    ERROR_SHOP_ITEM_BUY_COUNT_LOW = 210,                //购买数量需要大于0
    ERROR_SHOP_ITEM_MORE_THAN_LIMIT = 211,              //购买数量超过上限
    ERROR_SHOP_NO_ENOUGH_COST = 212,                    //消耗品不足
    ERROR_ITEM_USE_ZERO = 213,                          //使用道具的数量需要大于0

    ERROR_INVITE_CODE = 301,
    ERROR_INVITE_CODE_INPUT_SUCCESS = 302,   			//推荐码输入成功，请前往成就界面领取奖励
    ERROR_INVITE_CODE_INPUT_SELF = 303,		  			//不能输入自己的推荐码
    ERROR_INVITE_CODE_INPUT_ERROR = 304, 	  			//推荐码输入错误
    ERROR_INVITE_CODE_INPUTED = 305,			  		//您已经输入过玩家推荐码了
    ERROR_INVITE_CODE_NEED_INPUT = 306,			  		//请输入你所知道的玩家推荐码

    ERROR_SELECT_LEVEL = 401,
    ERROR_SELECT_LEVEL_NO_ENOUGH_STAMINA = 402,	        // 体力不足，无法进入关卡
    ERROR_SELECT_LEVEL_NEED_DIAMOND_FOR_COMMON = 403,   // 进入此关卡需要{0}钻石，是否继续？
    ERROR_SELECT_LEVEL_NEED_DIAMOND_FOR_EXTRA = 404,    // 本日挑战次数已满，消耗{0}钻石可获得额外一次挑战机会，是否继续？
    ERROR_SELECT_LEVEL_NO_CHANCE = 405,                 // 本日挑战次数已满，无法进入此关卡！  
    ERROR_SELECT_LEVEL_NO_ENOUGE_DIAMOND = 406,         // 钻石不足，无法继续挑战
    ERROR_SELECT_LEVEL_OVER_TIME = 407,                 // 此关卡已过期，无法进入
    //	ERROR_SELECT_LEVEL_HAVE_FRIEND = 408,           // 你已经选择了一位玩家助战

    ERROR_PET = 501,									// 符灵			
    ERROR_UPGRADE_PET_LEVEL_MAX = 502,					// 当前升级符灵已达满级，不需要再升级了！
    ERROR_UPGRADE_PET_AIM_LEVEL_MAX = 503,				// 当前升级符灵所需经验已满
    ERROR_PET_ADD_EXP = 504,					        // 符灵经验增加[00ff00]{0}[-]
    ERROR_PET_EVOLUTION_LEVEL = 505,					// 符灵还未达到满级
    ERROR_PET_EVOLUTION_STAR_LEVEL = 506,				// 符灵已达到最高星级
    ERROR_PET_OPERATE_NEED_COIN = 507,				    // 金币不足
    ERROR_UPGRADE_PET_SELECT = 508,					    // 点击+选择材料才能升级哦！
    ERROR_PET_STRENGTHEN_NO_CHANCE = 509,				// 该灵石强化概率为0，请换更高级别灵石
    ERROR_PET_SALE = 510,								// 您确定要出售该符灵吗？
    ERROR_UPGRADE_PET_LEVEL_BREAK_MAX = 511,			// 当前升级符灵突破等级已达满级
    ERROR_UPGRADE_PET_AIM_LEVEL_BREAK_MAX = 512,		// 当前升级符灵达到突破等级所需经验已满
    ERROR_PET_ADJUST_TEAM_FAIL = 513,                   // 调整队伍失败
    ERROR_PET_UPGRADE_SUCCESS = 514,                    // 成功升阶至{0}
    ERROR_UPGRADE_PET_HIGH_STAR_LEVEL = 515,			// 您选择了高星级（3星及以上）的符灵作为升级材料，确认升级吗？
    ERROR_PET_BREAK_NEED_COIN = 516,					// 突破金币不足
    ERROR_PET_BREAK_NEED_BREAK_STONE = 517,				// 突破石不足
    ERROR_PET_BREAK_NEED_SELF_ICON = 518,				// 突破消耗的自身卡不足
    ERROR_PET_FATE_NEED_FATE_STONE = 519,				// 天命石不足
    ERROR_PET_FATE_UPGRADE_LEVEL_MAX = 520,				// 天命等级已达顶级
    ERROR_UPGRADE_PET_LEVEL_BREAK = 521,				// 等级不足，无法突破
    ERROR_POINT_STAR_NEED_STONE = 522,					// 星芒不足
    ERROR_PET_UPGRADE_MAX_NUM = 523,				    // 最多选择{0}个
    ERROR_TEAM_POS_LOCKED = 524,                        // 该位置还未解锁
    ERROR_UPGRADE_PET_HIGHER_THAN_CHARACTER = 525,		//> 符灵等级大于角色等级
    ERROR_NO_PET_LABEL = 526,						    // 没有可选符灵
	ERROR_FATE_DESCRIBE_TEXT = 527,						// 注：升级天命可获得大量百分比属性加成。
    ERROR_SKILL_LARGER_THAN_PET_LV = 528,               //> 技能等级不能高于符灵等级
    ERROR_NO_THIS_LEVEL_PET = 529,                      //>您没有任何相应品质的符灵

    ERROR_ROLE = 601,									// 主角
    ERROR_ROLE_BUY_ROLE = 602,
    ERROR_ROLE_NO_ENOUGH_GOLD = 603,
    ERROR_ROLE_NO_ENOUGH_DIAMOND = 604,
    ERROR_NEED_BUY_CHARACET = 605,                      // 请先购买对应的主角

    ERROR_ROLE_EQUIP = 701,					        	// 法宝
    ERROR_ROLE_EQUIP_LEVEL_MAX = 702,               	// 法宝已达最高强化等级
    ERROR_ROLE_EQUIP_LEVEL_NOT_MAX = 703,           	// 法宝强化等级未达到{0}级，还不能进化
    ERROR_ROLE_EQUIP_STAR_LEVEL_MAX = 704,          	// 法宝星级已是最高级
    ERROR_ROLE_EQUIP_RESET_COUNT = 705,             	// 法宝重铸次数已超过{0}次，请明天再进行重铸
    ERROR_ROLE_EQUIP_STRENGTHEN_NEED_COIN = 706,    	// 法宝强化所需金币不足
    ERROR_ROLE_EQUIP_EVOLUTION_NEED_COIN = 707,     	// 法宝进化所需金币不足
    ERROR_ROLE_EQUIP_RESET_NEED_COIN = 708,         	// 法宝重铸所需元宝不足
    ERROR_ROLE_EQUIP_EVOLUTION_MAX = 709,     			// 法宝已经进化到满级
    ERROR_ROLE_EQUIP_STRENGTHEN_AIM_LEVEL_MAX = 710,	// 法宝强化所需经验已满
    ERROR_ROLE_EQUIP_STRENGTHEN_SELECT_COUNT_MAX = 711,	// 所选法宝数量已达上限
    ERROR_ROLE_EQUIP_IS_RESET = 712,			        // 确定重铸吗？
    ERROR_ROLE_EQUIP_SALE = 713,                        // [FFFFFF]出售法宝将获得[00FF00]{0}[FFFFFF]金币，是否确定？
    ERROR_COMPISTION_NOT_ENOUGH = 714,			        // 资源不足，无法合成
    ERROR_ROLE_EQUIP_STRENGTHEN_HIGH_STAR_LEVEL = 715,	// 您选择了高星级（3星及以上）的法宝作为升级材料，确认强化吗？
    ERROR_ROLE_EQUIP_STRENGTHEN_LEVEL_MAX = 716,    	// 装备强化等级已达上限
    ERROR_ROLE_EQUIP_NO_EQUIP = 717,    	            // 亲，没有装备哦
    ERROR_ROLE_EQUIP_NO_EQUIP_DATA = 718,    	        // 当前未选中装备
    ERROR_ROLE_EQUIP_NO_FRAGMENT = 719,    	            // 亲，没有装备碎片哦
    ERROR_ROLE_EQUIP_NO_FRAGMENT_DATA = 720,    	    // 没有装备碎片的资料哦
    ERROR_NO_EQUIP_INFORMATION_TIPS = 721,				//还没有该装备
    ERROR_NO_MAGIC_INFORMATION_TIPS = 722,				//还没有该法器

    ERROR_MAIL = 801,
    ERROR_MAIL_SUCCESS = 802,							//领取成功
    ERROR_MAIL_PET_BAG_FULL = 803,						//符灵背包已满
    ERROR_MAIL_GET_SOME = 804,							//部分物品领取成功
    ERROR_MAIL_NO_MAIL = 805,							//没有邮件可以领取
    ERROR_MAIL_FABAO_BAG_FULL = 806,					//法宝背包已满
    ERROR_MAIL_RESOURCES_SUCCESS = 807,					//金币，元宝，友情点等资源领取成功
    ERROR_MAIL_PET_OR_FABAO_BAG_FULL = 808, 			//符灵背包或者法宝背包已满
    ERROR_MAIL_NO_RESOURCES_MAIL = 809,					//没有金币，元宝，友情点等资源可以领取
    ERROR_MAIL_NO_PET_MAIL = 810,					    //没有符灵可以领取
    ERROR_MAIL_MORE_THAN_MAX_MAIL_COUNT = 811,			//邮箱已满，如果不及时提出物品，新物品将不再存储到邮箱
    ERROR_NO_PET_FRAGMENT_TIPS = 812,				    //没有符灵碎片
    ERROR_NO_PET_FRAGMENT_LABEL = 813,					//未选中符灵碎片
    ERROR_NO_GOODS_TIPS = 814,							//空背包
    ERROR_NO_INFORMATION_TIPS = 815,					//未选中物品
    ERROR_NO_PET_TIPS = 816,						    //没有符灵


    ERROR_FRIEND = 901,
    ERROR_FRIEND_SUCCESS = 902,							//恭喜你和{0}成为好友
    ERROR_FRIEND_OTHER_FULL = 903,						//对方好友已满，添加好友失败
    ERROR_FRIEND_FULL = 904,							//好友已满，添加好友失败
    ERROR_FRIEND_IS_FRIEND = 905,						//已经成为好友，添加好友失败
    ERROR_FRIEND_RECOMMEND_SUCCESS = 906,				//好友申请已发送
    ERROR_FRIEND_BATTLE_NOT_FRIEND = 907,  				//对方还不是你的好友，是否添加对方为你的游戏好友
    ERROR_FRIEND_GET_FRIEND_POINT = 908,    			//你和{0}获得了{1}点友情点奖励
    ERROR_FRIEND_RECOMMEND_SELF = 909,					//不能添加自己为好友
    ERROR_FRIEND_RECOMMEND_REPEAT = 910,				//已经发送过邀请
    ERROR_FRIEND_REFUSED = 911,							//请问你要拒绝{0}的好友申请吗？
    ERROR_FRIEND_SEARCH_NULL = 912,						//查无此人
    ERROR_FRIEND_DELETE = 913,							//请问你要删除{0}好友吗？
    ERROR_FRIEND_PRAISE = 914,							//请问你要给{0}送体力值吗？
    ERROR_FRIEND_ALREADY_ZAN = 915,						//你已经对其点过赞啦！
    ERROR_FRIEND_NEED_INPUT_NAME = 916,					//请输入你要查找的玩家名字
    ERROR_INVALID_INVIDE = 917,                         //无效的好友申请
    ERROR_FRIEND_FALL_RECOMMEND_FAIL = 918,				//好友已满，申请好友失败
    ERROR_NO_FRIEND_TIPS = 919,							//还没有好友
    ERROR_NO_ADD_FRIEND_TIPS = 920,						//还没有好友申请
    ERROR_AQUIRE_FRIEND_LIST = 921,                     //获取好友信息失败
    FRIEND_MAX_COUNT_TIPS = 922,                        //可以与好友一起挑战天魔降临，最多可添加{0}名游戏好友


    ERROR_ON_HOOK = 1001,								// 挂机错误
    ERROR_FRIEND_REQUEST_FULL = 1055,					// 对方的好友申请列表已满
    ERROR_SAODANG_NO_ENOUGH_DIAMON = 1101,              // 扫荡时元宝（钻石）不足
    ERROR_SAODANG_NEED_DIAMOND = 1102,                  // 扫荡券不足，是否消耗1元宝扫荡
    ERROR_SAODANG_INVALID = 1103,                       // 无效的扫荡请求
    ERROR_HAD_GET_AWARD = 1104,                         // 奖励已领取

    ERROR_PVP = 1201,								        //PVP错误
    ERROR_PVP_GET_SAME_ATTRIBUTE_PET = 1202,           	    //只能上阵{0}属性的宠物
    ERROR_PVP_ATTRIBUTE_CLOSE = 1203,        			    //属性竞技场暂未开放
    ERROR_PVP_FOUR_VS_FOUR_GET_RANK_AWARD_SUCCESS = 1204,   //领取成功
    ERROR_PVP_EQUIP_PETS_FIRST = 1205,				        //请您先装备好上阵符灵
    ERROR_PVP_NOT_OPEN = 1206,                              //竞技场尚未开启
    ERROR_PVP_SEARCH_AGAIN = 1207,                          //请您再次开战搜索
    ERROR_PVP_YOU_ARE_CHAMPION = 1208,                      //当前是冠军
    ERROR_PVP_NEED_ADJUST_PETS = 1209,                      //需要调整PVP上阵符灵
    ERROR_PVP_OPPONENT_NOE_EQUIP_PETS = 1210,               //对手未配置作战符灵
    PVP_SHOW_REWARD_MAX_RANK = 1211,                        //竞技场显示奖励的最大名次


    ERROR_REGISTRATION_ACCOUNT = 1301,
    ERROR_REGISTRATION_ACCOUNT_NOT_SAME = 1302,			    //两次密码不一样，请重新输入
    ERROR_REGISTRATION_ACCOUNT_SUCCEED = 1303,			    //注册成功
    ERROR_REGISTRATION_ACCOUNT_SELECT_SERVER_FIRST = 1304,  //请先选择服务器
    ERROR_BIND_ACCOUNT_SUCCEED = 1305,			            //绑定成功
    ERROR_REGISTRATION_NAME_CONTAIN_BANED_TEXT = 1306,	    //您的名字含有敏感字，起个更好的名字吧
    ERROR_CREATE_GUEST_USER_ERROR = 1307,           		//游客账号创建错误，请重新创建
    ERROR_CREATE_ROLE_NAME_CANOT_EMPTY = 1308,              //名字不能为空
    ERROR_CREATE_ROLE_NAME_TOO_SHORT = 1309,                //名字过短
    ERROR_CREATE_ROLE_NAME_TOO_LONG = 1310,                 //名字多长
    ERROR_CREATE_ROLE_NAME_ALL_NUMBER = 1311,               //名字不可使用纯数字
    ERROR_CREATE_ROLE_NAME_NOT_TEXT = 1312,                 //名字不可使用特殊符号

    RANK_ACTIVITY_END=1313,                                 //排名活动已结束
    RANK_ACTIVITY_OUT = 1314,                               //排名活动未上榜
    RANK_ACTIVITY_PEAK_NAME = 1315,                         //排名活动竞技场名字
    RANK_ACTIVITY_POWER_NAME = 1316,                        //排名活动战力比拼名字
    RANK_ACTIVITY_PEAK_REWARD_TIPS = 1317,                  //排名活动竞技场奖励描述提示
    RANK_ACTIVITY_POWER_REWARD_TIPS = 1318,                 //排名活动战力比拼奖励描述提示

    ERROR_PET_FRAGMENT_COUNT = 1401,					    //灵魂石数量不足
    ERROR_PET_FRAGMENT_COST_COIN = 1402,				    //合成所需金币不足

    ERROR_CONSUME = 1501,
    ERROR_CONSUME_NOT_ENOUGH_SAODANG_POINT = 1502,          //所需的扫荡卷不足
    ERROR_CONSUME_NOT_ENOUGH_RESET_POINT = 1503,		    //所需的重铸卷不足
    ERROR_CONSUME_NOT_ENOUGH_LOCK_POINT = 1504,			    //所需的锁定卷不足
    ERROR_CONSUME_INPUT_NUM = 1505,						    //请输入你要出售的物品数量
    ERROR_CONSUME_BRYOND_NUM_INPUT_AGAIN = 1506,		    //超出你所拥有的该物品的最大数量,请重新输入
    ERROR_CONSUME_BRYOND_NUM = 1507,						//超出你所拥有的该物品的最大数量
    ERROR_CONSUME_GET_BUFF_RESULT = 1508,					//获得: {0}\n持续时间: {1} 分钟
    ERROR_CONSUME_CHANGE_MODEL_RESULT = 1509,			    //变身为: {0}\n持续时间: {1}分钟
    ERROR_CONSUME_SALE = 1510,                              //[FFFFFF]出售该道具将获得[00FF00]{0}[FFFFFF]金币，是否确定？

    ERROR_COMPETITION_INVALID_REQUEST = 1601,			    //无效的挑战请求
    ERROR_COMPETITION_INVALID_ARENA = 1602,         	    //无效的挑战擂台
    ERROR_COMPETITION_ARENA_TOO_OLD = 1603,         	    //状态已过期
    ERROR_COMPETITION_ARENA_INBUSY = 1604,          	    //擂台赛正在进行中

    ERROR_NEED_QUIT_THE_GAME = 1701,					    // 您确定退出游戏吗？
    ERROR_NEED_QUIT_GAME_BECAUSE_LOGGED = 1702,				// 您的账号在另一台设备上登录，您将被强制下线

    ERROR_CHAT_NEED_SELECT_FRIEND = 1801,                   //请先选择好友
    ERROR_CHAT_NEED_INPUT_VALUE = 1802,                     //请输入内容
    ERROR_GIFT_CODE_NOT_EXIST = 1810,					    // 礼品码不存在
    ERROR_GIFT_CODE_OVERDUE = 1811,					        // 礼品码过期 
    ERROR_GIFT_CODE_INVALID = 1812,					        //礼品码无效 
    ERROR_CHAT_BANNED_TO_POST = 1813,                       //您已经被禁言

    ERROR_MATERIAL_FRAGMENT_COUNT = 1901,		            //材料数量不足
    ERROR_MATERIAL_FRAGMENT_COST_COIN = 1902,	            //合成所需金币不足

    ERROR_LOGIN = 2000,                                     //登陆
    ERROR_LOGIN_CONNECT_SERVER_FAIL = 2001,                 //连接服务器失败,请检查网络
    ERROR_LOGIN_NOT_RESOURCES_UPDATE_SERVER = 2002,         //没有可用的资源更新服务器
    ERROR_LOGIN_NOT_PROVIDE_RESOURCES_SERVER_ADDRESS = 2003,//未提供可用的资源服务器连接地址

    ERROR_BOSS_RAIN = 2100,
    ERROR_BOSS_RAIN_DEAD = 2101,                            //BOSS已经死亡
    ERROR_BOSS_RAIN_RUN = 2102,                             //BOSS已经逃跑	

    ERROR_MYSTERIOUS = 2200,
    ERROR_MYSTERIOUS_REFRESH = 2201,						//手动刷新需要消耗{0}元宝， 是否刷新？
    ERROR_MYSTERIOUS_NO_ENGOUGH_COST = 2202,		        //消耗品不足

    ERROR_DEMONBOSS = 2300,                                 //天魔错误
    ERROR_DEMONBOSS_BOSS_INFO_NOT_EXIST = 2301,			    //攻击天魔列表不存在
    ERROR_DEMONBOSS_BEATDEMONCARD_NOT_ENOUGH = 2302,        //降魔令不足
    ERROR_DEMONBOSS_BOSS_NOT_EXIST = 2303,                  //该天魔已不存在



    ERROR_RAMMBOCK = 2400,                                  //群魔乱舞
    ERROR_RAMMBOCK_STARS_NO_ENGOUGH = 2401,                 //星星不足
    ERROR_RAMMBOCK_STILL_CAN_CHALLENGE = 2402,              //仍然可以挑战
    ERROR_RAMMBOCK_CONFIRM_START_RESET = 2403,              //确定是否从第一关开始
    ERROR_RAMMBOCK_CONFIRM_BUY_AWARD = 2404,                //确定购买打折商品
    ERROR_RAMMBOCK_NO_ENGOUGH_COST = 2405,			        //用户{0}不足
    ERROR_RAMMBOCK_NO_EFFECT_ADD = 2406,                    //暂无加成

    ERROR_MAGIC = 2500,                                     // 法器
    ERROR_MAGIC_STRENGTHEN_NEED_COIN = 2501,                // 法器强化所需金币不足
    ERROR_MAGIC_STRENGTHEN_MAX_LEVEL = 2502,                // 法器强化等级已满
    ERROR_MAGIC_STRENGTHEN_AIM_LEVEL_MAX = 2503,            // 法器强化所需经验已满
    ERROR_MAGIC_REFINE_NEED_COIN = 2504,     	            // 法器精炼所需金币不足
    ERROR_MAGIC_STRENGTHEN_NEED_MAGIC = 2505,               // 点击+选择材料才能进行法器强化哦！
    ERROR_MAGIC_ADD_EXP = 2506,					            // 强化成功！经验增加[00ff00]{0}[-]！
    ERROR_MAGIC_REFINE_NEED_REFINE_STONE = 2507,            // 法器精炼所需精炼石不足
    ERROR_MAGIC_REFINE_NEED_REFINE_MAGIC = 2508,            // 法器精炼所需法器不足
    ERROR_MAGIC_ADD_REFINE_LEVEL = 2509,		            // 精炼成功！精炼等级上升为[00ff00]{0}[-]级！
    ERROR_MAGIC_NO_MAGIC = 2510,                            //亲，没有法器哦
    ERROR_MAGIC_NO_MGAIC_DATA = 2511,                       //木有法器资料哦
    ERROR_MAGIC_REFINE_FULL = 2512,                         //法器精炼等级已满

    ERROR_GRAB = 2600,                                      //夺宝
    ERROR_GRAB_ROBOT_EXCEPTION = 2601,                      //夺碎片等级不足
    ERROR_GRAB_SPIRIT_NO_ENOUGH = 2602,                     //精力不足
    ERROR_GRAB_TARGET_FRAGMENT_NO_ENOUGH = 2603,            //目标玩家碎片数量不足
    ERROR_GRAB_ALREADY_HAVE_FRAGMENT = 2604,                //已有当前碎片
    ERROR_GRAB_COMPOSE_FRAGMENT_NO_ENOUGH = 2605,           //合成所需碎片数量不足
    ERROR_GRAB_TARGET_IN_TRUCE_TIME = 2606,                 //该玩家使用了免战令
    ERROR_GRAB_TRUCE_NO_ENOUGH = 2607,                      //免战牌不足
    ERROR_GRAB_IN_TRUCE_NOW = 2608,                         //少侠，抢夺他人会解除免战状态，要继续吗？
    ERROR_GRAB_MULTI_PEASE = 2609,                          //您已经处在免战中，剩余{0}的免战时间，需要重复使用吗？
    TIPS_GRAB_FIRST_USE_PEASE = 2610,                       //使用免战令{0}小时
    ERROR_NO_GRAB_RECORD_TIPS = 2611,				        //暂无夺宝记录

    ERROR_UNIONNAME_TOO_LONG = 2701,                        //公会名字过长
    ERROR_UNIONINFO_TOO_LONG = 2702,                        //公会信息过长
    GUILD_BOSS_GUNAKA_NAME = 2703,                          //"第{0}关仙人"                           
    GUILD_BOSS_GUNAKA_CONTRI = 2704,                        //"挑战可获得[99ff66]{0}[ffffff]公会贡献" 
    GUILD_BOSS_GUNAKA_EXP = 2705,                           //"击杀后增加[99ff66]{0}[ffffff]公会经验" 
    GUILD_BOSS_GUNAKA_TIMES = 2706,                         //"已经没有挑战次数，是否使用{0}元宝购买？已购买{1}/{2}" 
    GUILD_BOSS_GUNAKA_NO_TIMES = 2707,                      //"今日购买次数已用完{0}/{1}"              
    GUILD_BOSS_GUNAKA_EXT_NOT_KILLED = 2708,                //"公会BOSS的前置BOSS没有完成过击杀"        
    GUILD_BOSS_GUNAKA_NEED_LEVEL = 2709,                    //"需公会等级{0}"                          
    GUILD_BOSS_GUNAKA_NEED_CROSS_LEVEL = 2710,              //"需要通关 第{0}关仙人 "                  
    GUILD_BOSS_GUNAKA_REWARD = 2711,                        //"第{0}关仙人奖励"    
    GUILD_BOSS_GUNAKA_RECHARGE = 2712,                      //"元宝不足，是否前往充值？"     
    GUILD_BOSS_GUNAKA_TIMES_OVER = 2713,                    // "当前没有剩余挑战次数"   
    GUILD_BOSS_GUNAKA_LAST_HIT_TIPS = 2714,                 // "你击杀了公会BOSS，获得了额外公会贡献"
    GUILD_BOSS_GUNAKA_CREATE_TIPS = 2715,                 // "请填写工会名称"
    GUILD_BOSS_GUNAKA_CREATE_NUM_TIPS = 2716,                 // "不能全是数字"
    GUILD_BOSS_GUNAKA_CREATE_VALID_TIPS = 2717,                 // "含有非法字符"


    ERROR_FAIRYLAND = 2800,                                 //符灵探险
    ERROR_FAIRYLAND_CANNOT_ENTER_FRIEND = 2801,             //不可随意进入好友的仙境哦~
    ERROR_FAIRYLAND_ITEM_NO_ENOUGH = 2802,                  //消耗品不足
    ERROR_FAIRYLAND_REPRESS_COUNT_NO_MORE = 2803,           //镇压次数已用完
    ERROR_FAIRYLAND_ALREADY_EXPLORING = 2804,               //符灵已经在寻仙中
    ERROR_FAIRYLAND_START_TIPS = 2805,                      //"当符灵达到{0}级时开启。"
    ERROR_SKILL_OPEN_TIPS = 2806,                           //当符灵突破+{0}时开启

    ERROR_HOTUPDATE = 2900,                                 //热更新
    ERROR_HOTUPDATE_VERSION_TOO_LOW = 2901,                 //您的客户端版本过低

    ERROR_NET = 3000,                                       //网络连接
    ERROR_NET_LOGIN_TOKEN_INVALID = 3001,                   //少侠超时了，要不要再试试？？？？？
    ERROR_NET_GAME_TOKEN_INVALID = 3002,                    //少侠超时了，试试返回登录！！！！！
    ERROR_NET_LOGIN_BY_OTHER = 3003,                        //您的账号在别处登录，请重新登录
    //    ERROR_NET_RETRY_CONNECT_ERROR = 3004,             //重连时返回错误
    ERROR_NET_KICKOFF_BY_SERVER = 3005,                     //被服务器踢出

    DAILY_STAGE_MAIN_CLOSE = 3200,                          //"关闭"                                  
    DAILY_STAGE_MAIN_BATTLE_POINT = 3201,                   //"[99ff66]{0}[ffffff]万"                 
    DAILY_STAGE_MAIN_BATTLE_CROSS = 3202,                   //"今日{0}副本已通关"                     
    DAILY_STAGE_MAIN_LEVEL_NOT_ENOUGH = 3203,               //"当前等级不足"                          
    DAILY_STAGE_MAIN_TIME_NOT_REACH = 3204,                 //"时间未到"                              
    DAILY_STAGE_MAIN_TIMES_NOT_ENOUGH = 3205,               //"当前挑战次数不足"                      
    DAILY_STAGE_MAIN_OPEN_LEVEL_NOT_ENOUGH = 3206,          //"每日副本开放等级没有达到"              

    ERROR_ACTIVITY = 4000,                                  //> 活动
    ERROR_ACTIVITY_SHOOTERPARK_NO_SHOOT_TIMES = 4001,       //> 已经没有射击次数
    ERROR_ACTIVITY_NO_ENOUGH_YUANBAO = 4002,                //> 元宝不足，次日登陆及七日狂欢可获得大量元宝
    ERROR_ACTIVITY_NO_LUCY_CARD_LEFT_TIMES = 4003,          //> 没有幸运翻牌剩余次数了
    ERROR_ACTIVITY_SHOOTER_PARK_ACTIVITY_IS_END = 4004,     //> 射手乐园活动已经结束
    HALF_PRICE_TIPS = 4006,                                 //"仅限前{0}名购买（剩余{0}件）"         
    HALF_PRICE_NO_NUM = 4007,                               //"已出售完毕" 
    HALF_PRICE_NOT_ENOUGH = 4008,                           //"需要消耗的物品不足"
    HALF_PRICE_GET_BUYED = 4009,                            //"已购买"                  

    ERROR_STRENGTHEN_MASTER = 4100,                         //> 强化大师
    ERROR_STRENGTHEN_MASTER_NEED_4_EQUIP = 4101,            //> 穿齐4件装备即可开启强化大师   

    ERROR_CAN_NOT_REWARD_POWER = 4200,                      //未在规定时间不能领取仙桃

    ERROR_SDK = 5000,
    ERROR_SDK_PAY_SUCCESS = 5001,                           //支付成功
    ERROR_SDK_PAY_FAILED = 5002,                            //支付失败
    ERROR_SDK_LOGIN_FAILED = 5003,                          //SDK登录失败

    ERROR_SPEED_X2 = 6001,                                  // 2倍速未开启
    ERROR_SPEED_X3 = 6002,                                  // 3倍速未开启

    RECOVER_RESOLVE_PET = 6100,                             //即将要分解选中符灵，是否确认？
    RECOVER_RESOLVE_EQUIP = 6101,                           //即将要分解选中装备，是否确认？
    RECOVER_REBIRTH_PET = 6102,                             //重生将使选中符灵回到最初状态，是否确认？
    RECOVER_REBIRTH_MAGIC = 6103,                           //重铸将使选中法器回到最初状态，是否确认？

    ARENA_BUY_TO_THINK = 6200,                              //"再想想"
    ARENA_TO_RECHAGRE = 6201,                               //"去充值"
    ARENA_TIMES_TIPS_ONE = 6202,                            //"0分钟前"
    ARENA_TIMES_TIPS_TWO = 6203,                            //"分钟前"
    ARENA_TIMES_TIPS_THREE = 6204,                          //"小时前"
    ARENA_TIMES_TIPS_FOUR = 6205,                           //"天前"
    ARENA_RANK_CHANGE_TIPS = 6206,                          //"由于对方低于你的排名，所以你的排名没有变化。"
    ARENA_NEET_VIP_LEVEL = 6207,                            //"VIP{0}可开启战5次，是否前往充值？"
    ARENA_NEED_TOP_RANK = 6208,                             //"杀入前{0}名即可获得排名奖励"
    ARENA_RANK_LIMIT = 6209,                                //"已达最高"
    ARENA_TREASURE_SELECT = 6210,                           //"点击宝箱三选一，切莫贪心哦！"
    ARENA_RANK_TIPS_FOR_GUIDE = 6211,                       //"恭喜您获得胜利。"

    RECHAGE_MAIN_TIPS = 6300,				 //元宝不足，去充值
    RECHAGE_MAIN_TIPS_LEFT = 6301,           //再想想
    RECHAGE_MAIN_TIPS_RIGHT = 6302,          //去看看
    RECHAGE_NAM_TIPS = 6303,                 //提升VIP等级可获得更多购买次数
    RECHAGE_EXTEND_BAG_TIPS = 6304,          //将军,提升VIP等级可增加背包上限
    RECHAGE_EXTEND_BAG_MAX = 6305,           //将军,背包上限已扩充至最大!
    VIP_EXCLUSIVE_CONSUMER_QQ_NUMBER = 6306,                //> 客服的QQ号,填具体的QQ号码 
    VIP_EXCLUSIVE_MAIN_CONTENT = 6307,                      //> 尊敬的少侠，专属客服小师妹随时准备为您服务哦，可以帮您解决任何游戏内的问题，块垒联系我吧~!
    VIP_EXCLUESIVE_OUTSIDE_CONTENT = 6308,                  //> 联系客服QQ可帮您去掉主界面上的按钮，还您一个清爽的界面

    RAMMBOCK_FIGHTPOINT_NOT_ENOUGH = 6400,                  //前方太过凶险，本层无法一键3星，少侠还请慎重
    RAMMBOCK_GET_STAR_TIPS = 6401,                          //攻击你在{0}~{1}关中，攻获得了{2}星
    RAMMBOCK_BUY_TIMES_TIPS = 6402,                         //"今日重置此数已达上限"
    RAMMBOCK_GUANKA_NUM = 6403,                             //,第{0}关
    RAMMBOCK_CRIKE_NORMAL = 6404,                           //暴击,
    RAMMBOCK_CRIKE_NICE = 6405 ,                            //幸运暴击,

    UNION_NOT_APPLY_TIME = 6501,                            //未满24小时不能申请

    WORLD_MAP_FORWARD_TIPS = 6601,                          //通关第{0}章后解锁

    TRIAL_DESCRIPTION = 6700,                               //> 历练描述
    TRIAL_DESCRIPTION_PVP = 6701,                           //> 竞技场描述
    TRIAL_DESCRIPTION_GRAB_TREASURE = 6702,                 //> 夺宝描述
    TRIAL_DESCRIPTION_RAMMBOCK = 6703,                      //> 封灵塔描述
    TRIAL_DESCRIPTION_FAIRYLAND = 6704,                     //> 寻仙描述
    TRIAL_DESCRIPTION_FEAST = 6705,                         //> 天魔描述
	TRIAL_ISPK_DESCRIPTION_FEAST = 6706,                    //> 天魔PK描述
	TRIAL_IS_SHARE_DESCRIPTION = 6707,                      //> 要求好友一起攻打天魔吗
	RELATE_TIPS_DESCRIPTION = 6708,                         //> 激活缘分可以大量提升战斗力
    TRIAL_DESCRIPTION_NUMBERLABEL = 6709,                     //>{0}级开放

    FUNC_NOT_OPEN = 6801,                                   //> 功能暂未开启
    FUNC_NOT_DEVELOP = 6802,                                //> 功能暂未开放

    FLASH_SALE_DESC_BODY = 6901,           //恭喜你，已经激活以下限时抢购礼包，抓紧时间，不要错过哦！

	BATTLE_REMAINED_TIME_TIPS = 7001,       //无限制

    SUCCESS_SEND_SPIRIT = 8001,                 // 赠送精力成功
    SUCCESS_AQUIRE_SPIRIT = 8002,               // 领取精力成功
    ERROR_NO_SPIRIT_AQUIRE = 8003,              // 没有精力可以领取哦
    ERROR_NO_SPIRIT_AQUIRE_LABEL = 8004,              // 没有精力可以领取
    ERROR_ZERO_SPIRIT_COUNT = 8005,             // 今天的次数已经全部领完了哦

    COMMON_CLICK_TO_CONTINUE = 8101,            //> 点击屏幕继续>>

    ERROR_RESOLVE_AUTO_PET = 8301,              // 没有可自动添加的符灵
    ERROR_RESOLVE_AUTO_EQUIPMENT = 8302,        // 没有可自动添加的装备
}

public enum HELP_INDEX 
{
    HELP_NONE = 0,
    HELP_LUCKCARD = 1,                                      //> 幸运翻牌奖励规则 

    HELP_UNION_BOSS = 10,                                   //公会boss战帮助
    HELP_ATHLETICS = 11,                                    //竞技场帮助
    HELP_RECOVER=12,                                        //回收规则
}

//NOTE: 服务器相关类型，如果更新，请同步服务器
public enum EQUIP_QUALITY_TYPE
{
	LOW = 1,
	MIDDLE,
	GOOD,
	BETTER,
    //by chenliang
    //begin

//	BEST
//---------------
    BEST,
    PERFECT

    //end
}

// 物品的品质颜色
public enum ITEM_QUALITY_TYPE
{
    LOW,
    MIDDLE,
    GOOD,
    BETTER,
    BEST
}

public enum EQUIP_TYPE
{
    ARM_EQUIP,
    ORNAMENT_EQUIP,
    DEFENCE_EQUIP,
    DEFENCE_EQUIP2,    
    MAX,
    ELEMENT_EQUIP,
}

public enum MAGIC_TYPE
{
    MAGIC1,
    MAGIC2,
    MAX,
}

public enum PET_EQUIP_TYPE
{
	Head,
	Hand,
	Body,
	Foot,
}

public enum PLAY_SOUND_MODEL_TYPE
{
	ALL,
	PET,
	MONSTER,
	NONE,
}

public enum GUARD_TYPE
{
	ARMOR,
	FLESH,
}

public enum CONSUME_ITEM_TYPE
{
	FUNCTIONAL = 1,
	BUFFER,
	TREASURE_BOX,
	CHANGE_MODEL,
	NUMERICAL,
	ACTIVE,
}
public enum UNLOCK_FUNCTION_TYPE
{
	NONE = 0,
	AUTO_FIGHT = 1,
	UPGRADE_PET,
	FA_BAO,
	BOSS_RAIN,
	ADD_FRIEND,
	SWEEP,
	STRENGTHER_PET,
	STRENGTHER_FA_BAO,
	DAILY_COPY_LEVEL,
	PEAK_FIGHT,
	UPGRADE_ROLE,
	RESET_FA_BAO,
	ENDLESS_PVP_ROLE,
	EXPLORE_LAND,
	PVP_PET,
	UNION,
	SKILL_PET,
}

public enum CompositionInfoPageType
{
    EQUIP_ARM,
    EQUIP_DEFENCE,
    EQUIP_ORNAMENT,
    EQUIP_ELEMENT,
    MATERIAL,
}

public enum STAGE_DIFFICULTY
{
    COMMON = 1,
    DIFFICUL,
    MASTER,
}

public class SortListParam
{
	public int mParam1;
	public int mParam2;
	public bool bIsDescending;
}

public enum SYSTEM_STATE
{
    NOTIF_MAIL = 1,                 // 邮件
    NOTIF_BOSS = 2,                 // BOSS
    NOTIF_TASK_FINISH = 3,          // 是否有任务已完成
    NOTIF_TASK_REFRESH = 4,         // 是否刷新任务数据
    NOTIF_SHOP = 5,                 // 是否刷新普通商店
    NOTIF_FRIEND_CHANGE = 6,        // 是否刷新好友数据
    NOTIF_FRIEND_REQUEST = 7,       // 好友申请
	NOTIF_REVELRY = 8,	            // 开服狂欢 
    NOTIF_ATLAS = 9,                /* 新图鉴通知 */
    NOTIF_LUXURY_SIGN = 10,         /* 豪华签到 */
    NOTIF_DAILY_SIGN = 11,          /* 每日签到 */         
    NOTIF_SEVEN_DAY_LOGIN = 12,     /* 七日登陆 */
    NOTIF_SHAKE_TREE = 13,          /* 摇钱树 */
    NOTIF_WELFARE_DAY = 14,         /* vip每日礼包 */
    NOTIF_WELFARE_WEEK = 15,        /* vip每周礼包 */
    NOTIF_FIRST_RECHARGE = 16,      /* 首冲礼包 */
    NOTIF_VITALITY = 17,            /* 领仙桃 */
    NOTIF_MONTH_CARD = 18,          /* 月卡 */
    NOTIF_LUCKY_CARD = 19,          /* 幸运翻牌 */
    NOTIF_CUMULATIVE = 20,          /* 消费返利 */
    NOTIF_FLASH_SALE = 21,          /* 限时折扣 */
    NOTIF_CHARGE_AWARD = 22,        /* 充值送礼 */
    //NOTIF_SHOOTER_PARK = 23,      /* 射手乐园 */
    NOTIF_FUND_DIAMOND = 23,        /* 基金元宝 */
    NOTIF_FUND_WELFARE = 24,        /* 基金福利 */
    NOTIF_ACTIVITY_MAX,
    NOTIF_SHOP_VIP_GIFT = 30,       /* 商店VIP礼包 */
    NOTIF_SHOP_ADVANCE = 31,        /* 高级免费  */
    NOTIF_SHOP_NORMAL = 32,         /* 普通免费 */
    
    NOTIF_GUILD_WORSHIP = 50,       /* 公会祭天 */
    NOTIF_GUILD_ADD = 51,           /* 同意加入公会 */
    NOTIF_GUILD_REFUSE = 52,        /* 拒绝加入公会 */
    NOTIF_GUILD_APPLY = 53,         /* 公会申请者 */
    NOTIF_GUILD_REWARD = 54,        /* 公会祭天奖励可领取 */

    NOTIF_GUILD_IN_OUT_MSG = 55,    /* 公会宣言或者公告修改 */
    NOTIF_GUILD_REMOVE = 56,        /* 公会有成员退出或踢出 */
    NOTIF_GUILD_SHOP = 57,          /* 公会限时商品 */

    NOTIF_REPUTATION_SHOP = 58,     /* 声望商店 */
    NOTIF_CLOTH_SHOP = 59,          /* 神装商店 */
    NOTIF_PET_SHOP = 60,            /* 符灵商铺 */

    NOTIF_FlASH_SALE_MULTI = 61,    /* 限时抢购-multi */
    NOTIF_FLASH_SALE_SINGLE = 62,   /* 限时抢购-single */
    NOTIF_FRIEND_SPIRIT = 63,       /* 好友赠送精力 */
    NOTIF_RANK_ACTIVITY = 64,       /* 排名活动 */
    NOTIF_MAX,
}



public enum TEAM_POS
{
    CHARACTER,
    PET_1,
    PET_2,
    PET_3,
    MAX,

    RELATE_1 = 11,
    RELATE_2 = 12,
    RELATE_3 = 13,
    RELATE_4 = 14,
    RELATE_5 = 15,
    RELATE_6 = 16,
}

public enum TEAM_POS_TYPE
{
    NOT_IN_TEAM,
    IN_RELATE,
    IN_TEAM,
}

public enum FUNC_ENTER_INDEX 
{
    NONE = -1,
    MAIL = 0,           //> 邮件
    TASK = 1,           //> 任务
    CLEAN_OUT = 2,      //> 扫荡界面
    PVP = 3,            //> 巅峰挑战
    GRABTREASURE = 4,   //> 夺宝
    RAMMBOCK = 5,       //> 封灵塔
    FEATS = 6,          //> 天魔
    ADVENTURE = 7,      //> 
    DAILYSTAGE = 8,     //>活动副本
    GUILDBOSS = 9,      //>宗门副本
	YIJIAN_GRABTREASURE = 10,	//> 一键夺宝
	Challenge_Five =11 ,	//>巅峰挑战战五次
    PACKAGE = 12,       //> 背包
}

/// <summary>
/// 按钮状态枚举
/// </summary>
public enum Btn_StateType
{
    GO_FORWARD = 0, //> 前往
    CAN_GET,        //> 可以领取
    HAS_GOT,        //> 已领取
    GO_FORWARD_GREY,//> 前往按钮灰色
}
/// <summary>
/// 角色脚底光圈类型
/// </summary>
public enum AUREOLE_TYPE 
{
    MAIN_UI,    //> 主界面
    UI,         //> 非主界面UI
    FIGHT_UI,   //> 战斗界面    
}


/// <summary>
/// 主角宠物品质
/// </summary>
public enum PET_QUALITY
{
    UNDEFINE = 0,   // 未定义
    WHITE = 1,  // 白色
    GREEN = 2,  // 绿色
    BLUE = 3,   // 蓝色
    PURPLE = 4, // 紫色
    ORANGE = 5, // 橙色
    RED = 6,    // 红色
    PINK = 7,   // 粉色
}