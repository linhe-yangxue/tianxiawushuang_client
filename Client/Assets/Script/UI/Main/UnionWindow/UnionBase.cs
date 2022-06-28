using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;


public abstract class UnionBase : tWindow
{

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex,objVal);
	}
    public static void InGuildThenDo<T>(T item,Action action=null) where T:SC_GuildBase{

        bool isInGuild = (item.guildId != "");
        if (isInGuild) {
            if (action != null) action();
        } 
        else DataCenter.OpenMessageWindow("你已被踢出公会", () =>BaseUI.OpenWindow("MAIN_UI_WIDNOW", null, true));
    }
    public class GuildMemberCompater : IComparer
    {
        public int Compare(object l, object r) {
            var lMember = l as GuildMember;
            var rMember = r as GuildMember;

            int result = 0;

            result = -lMember.title.CompareTo(rMember.title);
            if(lMember.title==UnionTitle.MEMBER&&rMember.title==UnionTitle.MEMBER){
                if (IsMySelf(lMember)) result = -1;
                else result = 1;
            }
            return result;
        }
    }
    public class GuildMember
    {
        public readonly string uid = "";
        public readonly int level = -1;
        public readonly int power = -1;
        public readonly int unionContr = -1;
        public readonly int todayWorship = -1;
        public readonly string nickName;
        public readonly int iconIndex = -1;
        public readonly Int64 time;
        public readonly UnionTitle title = UnionTitle.NONE;
        public GuildMember(GuildMemberObject memberTitle, GuildMemberBaseObject memberBase) {
            uid = memberTitle.zuid;
            title = (UnionTitle)memberTitle.title;

            level = memberBase.level;
            power = memberBase.power;
            unionContr = memberBase.unionContr;
            todayWorship = memberBase.todayWorship;
            nickName = memberBase.nickname;
            iconIndex = memberBase.iconIndex;
            time=memberBase.time;
        }
        public Int64 outTime {
            get {
                return CommonParam.NowServerTime()-time;
            }
        }
    }
    public enum UnionTitle
    {
        NONE = -1,
        MEMBER = 0,
        VICE_PRESIDENT = 1,
        PRESIDENT = 2
    }
    public enum NewsType
    {
        DONATE_NORMAL = 1,
        DONATE_MIDDLE=2,
        DONATE_SENIOR=3,
        ENTER_GUILD=4,
        LEAVE_GUILD=5,
        APPOINT=6,  //任命掌门
        TRANSFERENCE=7,  //任命堂主
        RECALL=8,
        KICK=9,

    }

    static string uid {
        get {
            return CommonParam.mUId;
        }
    }

    public static GuildBoss guildBossObject { get; set; }
    public static GuildBossWarrior guildBossWarriorObject { get; set; }
    public static int btnId = 900;

    //红点
    public static int iGuildBossRedPoint = 0;

    public static void SetGuildBossRedPoint(int redPoint)
    {
        iGuildBossRedPoint = redPoint;
    }

    public static bool GetGuildBossRedPoint()
    {
        return iGuildBossRedPoint == 1;
    }

    //根据vip获取，公会boss挑战此数
    public static int GetMaxPkTimes()
    {
        int vip = RoleLogicData.Self.vipLevel;
        int maxTimes = 0;
        maxTimes = TableCommon.GetNumberFromConfig(vip, "GUILD_BOSS_BUY_TIME", DataCenter.mVipListConfig);
        return maxTimes;
    }

    //获取表头 index
    public static int GetGuildBossIndex()
    {
        int ret = 0;
        if (guildBossObject != null)
        {
            ret = guildBossObject.mid;
        }
        return ret;
    }

    //获取boss名字
    public static string GetGuildBossName(int index)
    {
        string name = TableCommon.GetStringFromConfig(index, "NAME", DataCenter.mGuildBoss);
        return name;
    }

    //获取boss headicon
    public static int GetGuildBossMonsteId(int index)
    {
        int stageId = TableCommon.GetNumberFromConfig(index, "STAGE_ID", DataCenter.mGuildBoss);
        int monsterId = TableCommon.GetNumberFromStageConfig(stageId, "HEADICON");
        return monsterId;
    }
    
    //获取获得贡献
    public static int GetGuildBossAttackContri(int index)
    {
        int contri = TableCommon.GetNumberFromConfig(index, "ATTACK_CONTRIBUTE", DataCenter.mGuildBoss);
        return contri;
    }

    //获取获得贡献
    public static int GetGuildBossLastHitContri(int index)
    {
        int contri = TableCommon.GetNumberFromConfig(index, "LASTHIT_CONTRIBUTE", DataCenter.mGuildBoss);
        return contri;
    }

    //获得购买挑战次数需要多少元宝
    public static int GetBuyTimesPrice()
    {
        int butTimes = guildBossWarriorObject.totalBuyChanllengeTimes + 1;
        int price = TableCommon.GetNumberFromConfig(butTimes, "PRICE", DataCenter.mGuildBossPrice);
        return price;
    }

    //是否可以设置目标
    public static bool IsCanSetBoosIndex()
    {
        if (IsTimeContained(0, 15.5f) || (guildBossObject.monsterHealth <= 0 && IsTimeContained(16, 24)) )
        {
            return true;
        }
        return false;
    }

    //是否在挑战时间内
    public static bool IsCanAttackBoss()
    {
        return IsTimeContained(16, 22);
    }

    public static bool IsYiJingGuoqi()
    {
        if (IsTimeContained(22, 24) && guildBossObject.monsterHealth > 0)
        {
            return true;
        }
        return false;
    }

    public static bool IsShiJianWeiDao()
    {
        if (IsTimeContained(0, 16))
        {
            return true;
        }
        return false;
    }

    public static bool IsHasRemainedTimes()
    {
        if (guildBossWarriorObject != null)
        {
            return guildBossWarriorObject.leftBattleTimes > 0;
        }
        return false;
    }

    public static bool IsTimeContained(float start, float end)
    {
        DateTime nowTime = GameCommon.NowDateTime();
        float secTemp = nowTime.Hour * 3600 + nowTime.Minute * 60 + nowTime.Second;
        float startTime = start * 3600;
        float endTime = end * 3600;
        if (secTemp >= startTime && secTemp < endTime)
        {
            return true;
        }
        return false;
    }

    public static bool IsKilledExIndex(int index)
    {
        int[] killedIndex = guildBossObject.killedGuildBossIndex;
        for (int i = 0; i < killedIndex.Length; i++)
        {
            if (killedIndex[i] == index)
            {
                return true;
            }
        }
        return false;
    }

    public static int GetMonsterBaseHp(int index)
    {
        int stageId = TableCommon.GetNumberFromConfig(index, "STAGE_ID", DataCenter.mGuildBoss);
        int monsterHeadIcon = TableCommon.GetNumberFromConfig(stageId, "HEADICON", DataCenter.mStageTable);
        int monsterBaseBlood = TableCommon.GetNumberFromConfig(monsterHeadIcon, "BASE_HP", DataCenter.mMonsterObject);
        return monsterBaseBlood <= 0 ? 0 : monsterBaseBlood;
    }

    public static void SetProgressBar(int index, GameObject gameObj, string ratename)
    {
        int monsterBaseBlood = GetMonsterBaseHp(index);
        if (guildBossObject.monsterHealth <= 0)
        {
            guildBossObject.monsterHealth = 0;
        }
        float percent = 0;
        if (monsterBaseBlood == 0)
        {
            percent = 0;
        }
        else
        {
            percent = guildBossObject.monsterHealth * 1.0f / monsterBaseBlood;
        }
        GameCommon.FindObject(gameObj, "Progress Bar").GetComponent<UISlider>().value = percent;
        int num = (int)(percent * 100.0f);
        string numStr = num.InsertToString("{0}%");
        GameCommon.SetUIText(gameObj, ratename, numStr);
    }

    public static bool IsGettedReward()
    {
        return (guildBossWarriorObject.rewardState == 1  && guildBossObject.monsterHealth <= 0) ? true : false;
    }

    public static int GetSelectMid()
    {
        int tempId = 0;
        DateTime nowTime = GameCommon.NowDateTime();
        float secTemp = nowTime.Hour * 3600 + nowTime.Minute * 60 + nowTime.Second;
        if (UnionBase.guildBossObject.monsterHealth <= 0 && secTemp > 16 * 3600 && secTemp < 24 * 3600)
        {
            tempId = guildBossObject.nextMid;
            if (tempId > 0)
            {
                //
            }
            else
            {
                tempId = guildBossObject.mid;
            }
        }
        else if (UnionBase.IsTimeContained(0, 15.5f))
        {
            tempId = guildBossObject.mid;
        }
        else 
        {
            tempId = guildBossObject.mid;
        }
        
        return tempId;
    }

    //主界面的接口
    public static bool CheckMainUIHasNewMark()
    {
        bool ret = false;
        if(CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_WORSHIP))
        {
            ret = true;
        }
        else if(CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_ADD))
        {
            ret = true;
        }
        else if (CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_REFUSE))
        {
            ret = true;
        }
        else if (CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_APPLY))
        {
            ret = true;
        }
        else if (CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_REWARD))
        {
            ret = true;
        }
        else if (CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_IN_OUT_MSG))
        {
            ret = true;
        }
        else if (CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_REMOVE))
        {
            ret = true;
        }
        else if (CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_SHOP))
        {
            ret = true;
        }
        else if (GameCommon.IsFuncCanUse(btnId))
        {
            string key = CommonParam.mUId + "UNION_OPEN_NEW_MARK";
            if (PlayerPrefs.GetInt(key) == 0)
            {
                ret = true;
            }
        }
        return ret;
    }

    public static void SetOpenLevelValue() 
    {
        string key = CommonParam.mUId + "UNION_OPEN_NEW_MARK";
        PlayerPrefs.SetInt(key, 1);
    }

    //里面各大红点接口
    public static bool CheckUnionMainNewMark(SYSTEM_STATE systemState)
    {
        bool ret = false;
        ret = SystemStateManager.GetNotifState(systemState);
        return ret;
    }

    //进入商店调用
    public static void SetShopFalse()
    {
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_GUILD_SHOP, false);
    }

    //祭祀完调用
    public static void SetWorkshipFalse()
    {
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_GUILD_REWARD, false);
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_GUILD_WORSHIP, false);
    }

    //祭祀完调用
    public static void SetInfoFalse()
    {
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_GUILD_REMOVE, false);
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_GUILD_IN_OUT_MSG, false);
    }

    //祭祀完调用
    public static void SetApplyFalse()
    {
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_GUILD_APPLY, false);
        DataCenter.SetData(UIWindowString.union_main, "REFRESH_NEW_MARK_APPLY", null);
    }

    //判断是否商店显示条件
    public static bool IsShowShopNewMark()
    {
        if (CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_SHOP))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //能领取贡献奖励
    public static bool IsShowWorkshipNewMark()
    {
        if (CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_WORSHIP) || CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_REWARD))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //有动态信息改变或者有人离开公会
    public static bool IsShowInfoNewsNewMark()
    {
        if (CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_IN_OUT_MSG) || CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_REMOVE))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //有申请者
    public static bool IsShowaApplyNewMark()
    {
        if (CheckUnionMainNewMark(SYSTEM_STATE.NOTIF_GUILD_APPLY))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static GuildBaseObject guildBaseObject { get; set; }
    protected static GuildMember[] memberArr { get; private set; }
    protected string guildId {
        get {
            return RoleLogicData.Self.guildId;
        }
    }
    protected void SetGuildId(string guildId) {
        RoleLogicData.Self.guildId = guildId;
    }

    protected void SetGuildInfo(SC_GetGuildMemberArr sc) {
        guildBaseObject = sc.guildObject;

        var memberBaseArr = sc.baseArr;
        var memberTitleArr = sc.arr;

        memberArr = new GuildMember[memberBaseArr.Length];

        for (int i = 0; i < memberTitleArr.Length; i++) {
            var memberTitle = memberTitleArr[i];
            var memberBase = memberBaseArr.Where(x => x.zuid == memberTitle.zuid).SingleOrDefault();
            memberArr[i] = new GuildMember(memberTitle, memberBase);
        }

        var list=memberArr.ToList();
        list.Sort((lMember,rMember) => {
            int result=0;
            result=-lMember.title.CompareTo(rMember.title);
            if(lMember.title==UnionTitle.MEMBER&&rMember.title==UnionTitle.MEMBER) {
                if(IsMySelf(lMember)) result=-1;
                else result=1;
            }
            return result;
        });
        memberArr=list.ToArray();
        //Array.Sort(memberArr, new GuildMemberCompater());
    }

    public GuildMember myMember { 
        get { 
            return memberArr.Where(member => member.uid == uid).SingleOrDefault(); 
        } 
    }
    protected UnionTitle myTitle
    {
        get { return (UnionTitle)myMember.title; }
    }
    protected static bool IsMySelf(GuildMember member) {
        return member.uid == uid;
    }

    protected void GetNews(UILabel label) {
        CS_GuildInfoArr cs = new CS_GuildInfoArr(guildId);
        Func<int, string> AddPoint_Green = (point) => {
            return ((point.ToString() + "点").SetTextColor(LabelColor.Green));
        };

        Func<string, int, string> GetWorshipInfoStr = (name, worshipLevel) => {
            return "成员" + name + "捐献，增加" + AddPoint_Green(GetWorshipSchedle(worshipLevel)) + "进度，为宗门增加" + AddPoint_Green(GetWorshipExp(worshipLevel)) + "经验";
        };


        Func<GuildDynamicInfoObject, string> GetInfoStr = (info) => {
            var newsType = (NewsType)info.infoType;
            var name = info.nickname.SetTextColor(LabelColor.Red);
            string newsText = "";
            switch (newsType) {
                case NewsType.DONATE_NORMAL: newsText = GetWorshipInfoStr(name, 1); break;
                case NewsType.DONATE_MIDDLE: newsText = GetWorshipInfoStr(name, 2); break;
                case NewsType.DONATE_SENIOR: newsText = GetWorshipInfoStr(name, 3); break;
                case NewsType.ENTER_GUILD: newsText = "欢迎" + name + "成为宗门一员"; break;
                case NewsType.LEAVE_GUILD: newsText = "成员" + name + "退出了宗门"; break;
			    case NewsType.APPOINT: newsText = name+"被任命为掌门"; break;
                case NewsType.RECALL: newsText = "成员" + name + "堂主职位被罢免"; break;
			    case NewsType.TRANSFERENCE: newsText="成员" + name + "被任命为堂主"; break;
                case NewsType.KICK: newsText=name+"已被踢出宗门"; break;

            }
            return newsText;
        };

        HttpModule.CallBack requestSuccess = text => {
            DEBUG.Log("RequestSuccess:text = " + text);

            var item = JCode.Decode<SC_GuildInfoArr>(text);

            Action action = () => {
                var arr = item.arr;
                StringBuilder newsText = new StringBuilder();
                for (int i = 0; i < arr.Length; i++) {
                    if (i != arr.Length - 1) newsText.Append(GetInfoStr(arr[i]) + "\r\n");
                    else newsText.Append(GetInfoStr(arr[i]));
                }
                if (arr != null && arr.Length != 0) label.text = newsText.ToString();
            };

            UnionBase.InGuildThenDo(item, action);
        };
       

        HttpModule.Instace.SendGameServerMessage(cs, "CS_GuildInfoArr", requestSuccess, NetManager.RequestFail);
    }


    protected Action GetAppointAction(string memberUid, UnionTitle title) {
        return () => {
            HttpModule.CallBack requestSuccess = text => {
                Action action = () => {
                    if (title == UnionTitle.PRESIDENT) {
                        DataCenter.CloseWindow(UIWindowString.union_infoNews);
                        DataCenter.CloseWindow(UIWindowString.dynamic_MsgBox_4);
                    }
                    DataCenter.OpenWindow(UIWindowString.union_infoNews);
                };
                var item = JCode.Decode<SC_GuildAppointMember>(text);
                UnionBase.InGuildThenDo(item, action);
            };
            
            CS_GuildAppointMember cs = new CS_GuildAppointMember(guildId, memberUid, (int)title);
            HttpModule.Instace.SendGameServerMessage(cs, "CS_GuildAppointMember", requestSuccess, NetManager.RequestFail);
        };
    }
    protected Action GetRecallAction(string memberUid, UnionTitle title) {
        return () => {
            CS_GuildCancelAppointMember cs = new CS_GuildCancelAppointMember(guildId, memberUid, (int)title);
            HttpModule.CallBack requestSuccess = text => {
                var item = JCode.Decode<SC_GuildCancelAppointMember>(text);
                UnionBase.InGuildThenDo(item, () => DataCenter.OpenWindow(UIWindowString.union_infoNews));
            };
            HttpModule.Instace.SendGameServerMessage(cs, "CS_GuildCancelAppointMember", requestSuccess, NetManager.RequestFail);
        };
    }

    protected Action GetQuitAction(string memberUid) {
        return () => {
            HttpModule.CallBack requestSuccess=text => {
                var item=JCode.Decode<SC_GuildRemoveMember>(text);
                UnionBase.InGuildThenDo(item,() => {
                    DataCenter.CloseWindow(UIWindowString.union_infoNews);
                    RoleLogicData.Self.guildId = "";
                    MainUIScript.Self.ShowMainBGUI();
                });
            };

            CS_GuildRemoveMember cs=new CS_GuildRemoveMember(guildId,memberUid);
            HttpModule.Instace.SendGameServerMessage(cs,"CS_GuildRemoveMember",requestSuccess,NetManager.RequestFail);
        };
    }


    protected Action GetKickAction(string memberUid) {
        return () => {
            HttpModule.CallBack requestSuccess = text => {
                var item = JCode.Decode<SC_GuildRemoveMember>(text);
                UnionBase.InGuildThenDo(item, () => DataCenter.OpenWindow(UIWindowString.union_infoNews));
            };

            CS_GuildRemoveMember cs = new CS_GuildRemoveMember(guildId, memberUid);
            HttpModule.Instace.SendGameServerMessage(cs, "CS_GuildRemoveMember", requestSuccess, NetManager.RequestFail);
        };
    }
  

    protected void ImpeachAction() {
        HttpModule.CallBack requestSuccess = text => {
            var item = JCode.Decode<SC_GuildImpeach>(text);
            Action action=() => {
                if(item.canImpeach==0) DataCenter.OpenMessageWindow("弹劾失败");
            };
            UnionBase.InGuildThenDo(item,action);
        };

        CS_GuildImpeach cs = new CS_GuildImpeach(guildId);
        HttpModule.Instace.SendGameServerMessage(cs, "CS_GuildImpeach", requestSuccess, NetManager.RequestFail);
    }


    protected int GetWorshipSchedle(int worshipLevel) {
        DEBUG.Log(worshipLevel.ToString());
        return DataCenter.mWorshipConfig.GetRecord(worshipLevel).get("SCHEDULE");
    }
    protected int GetWorshipExp(int worshipLevel) {
        return DataCenter.mWorshipConfig.GetRecord(worshipLevel).get("EXP");
    }
    protected int GetWorshipPrice(int worshipLevel) {
        return DataCenter.mWorshipConfig.GetRecord(worshipLevel).get("PRICE");
    }
    protected int GetWorshipContribute(int worshipLevel) {
        return DataCenter.mWorshipConfig.GetRecord(worshipLevel).get("CONTRIBUTE");
    }

    protected const int nameLengthLimit = 8;
    protected const int outInfoLengthLimit = 50;
    protected const int inInfoLengthLimit = 50;

    protected Func<string, bool> isInNameLimit = (name) => { return name.Length <= nameLengthLimit; };
    protected Func<string, bool> isInOutInfoLimit = (name) => { return name.Length <= outInfoLengthLimit; };
    protected Func<string, bool> isInInInfoLimit = (name) => { return name.Length <= inInfoLengthLimit; };
    protected Func<string, bool> isAllNumber = name => {
        int valueCount = 0;
        for (int i = 0; i < name.Length; i++) {
            int value;
            if (int.TryParse(name.Substring(i, 1), out value)) valueCount++;
        }
        return valueCount==name.Length;
    };

    protected void ShowIllegalTips(string name){
        if(!isInNameLimit(name)) DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_UNIONNAME_TOO_LONG);
        if (isAllNumber(name)) DataCenter.OpenMessageWindow(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_CREATE_NUM_TIPS));
        if (isEmpty(name)) DataCenter.OpenMessageWindow(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_CREATE_TIPS));
        if (isHasSpecialChar(name)) DataCenter.OpenMessageWindow(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_CREATE_VALID_TIPS));       
    }

    protected Func<string,bool> isHasSpecialChar=name => {
        for(int i=0;i<name.Length;i++){
            if(!char.IsLetter(name[i])&&!char.IsNumber(name[i]))
                return true;
        }
        return false;
  
    };

    protected Func<string,bool> isEmpty=name => {
        return name.Length<=0;
    };

}

//协议用
public class GuildBaseObject
{
    public readonly string guildId = "";
    public readonly string name;
    public readonly string outInfo;
    public readonly string inInfo;
    public readonly int memberCount;
    public readonly int level;
    public int exp;


    public int memberLimit { get { return DataCenter.mGuildCreated.GetRecord(level).get("NUMBRE_LIMIT"); } }
    public bool isFull {
        get { return memberCount >= memberLimit; }
    }

    public int nextExp {
        get {
            int _level = Mathf.Min(level + 1,DataCenter.mGuildCreated.GetRecordCount());
            return DataCenter.mGuildCreated.GetRecord(_level).get("EXP"); 
        }
    }

    public float expPercent { 
        get { 
            return exp / (float)nextExp; 
        } 
    }

}

public class GuildMemberBaseObject
{
    public readonly string zuid = "";
    public readonly int level = -1;
    public readonly int power = -1;
    public readonly int unionContr = -1;
    public readonly int todayWorship = -1;
    public readonly string nickname;
    public readonly int iconIndex = -1;
    public readonly int time;

    public int outTime { get { return time; } }
}

public class GuildMemberObject
{
    public readonly string zuid = "";
    public readonly int title = -1;
}

public class GuildDynamicInfoObject
{
    public readonly string zuid;
    public readonly string nickname;
    public readonly int infoType;
}

public class Button_ToUnionBtn : Logic.CEvent
{
    public override bool _DoEvent() {
        HttpModule.CallBack requestSuccess = text => {
            var item = JCode.Decode<SC_GetGuildId>(text);
            UnionBase.SetGuildBossRedPoint(item.guildBossRedPoint);//秘境红点
            RoleLogicData.Self.guildId=item.guildId;
            DEBUG.Log(RoleLogicData.Self.guildId);
            if (RoleLogicData.Self.guildId != "") 
                DataCenter.OpenWindow(UIWindowString.union_main);
            else 
                DataCenter.OpenWindow(UIWindowString.union_list);
        
			MainUIScript.Self.HideMainBGUI();
        };
        CS_GetGuildId cs=new CS_GetGuildId();
        HttpModule.Instace.SendGameServerMessageT<CS_GetGuildId>(cs, requestSuccess, NetManager.RequestFail);

        return true;
    }
}