using UnityEngine;
using System.Collections;
using LHC.PipeLineUIRefreshSyetem;
using DataTable;
using System.Collections.Generic;

public enum D_RoleDetail {
    yuanBaoTid,
    silverTid,
    ghostTid,
    reputationTid,
    moLingTid,
    coreTid,
    contriTid,
    energy,
    stamina,
    xiangMo,
    nextLevelExp
}

public enum PL_RoleDetail {
    Set_YuanBao_Cost,
    Set_Contri_Cost,
    Set_Silver_Cost,
    Set_Ghost_Cost,
    Set_Reputation_Cost,
    Set_MoLing_Cost,
    Set_Core_Cost,
    Set_Energy_Cost,
    Set_Stamina_Cost,
    Set_XiangMo_Cost,
    Set_YuanBao_Icon,
    Set_Contri_Icon,
    Set_Silver_Icon,
    Set_Ghost_Icon,
    Set_Reputation_Icon,
    Set_Moling_Icon,
    Set_Core_Icon,
    Set_Level_Label,
    Set_LevelPer_Slider,
    Set_LevelPer_Label,
    Set_Vip_Label,
    Set_Role_Name
}

public class RoleDetail : tWindow{
    PipeLineFactory<PL_RoleDetail, D_RoleDetail> mPipeLineFactory = new PipeLineFactory<PL_RoleDetail, D_RoleDetail>();
	string[] iconNameArr = new string[]{"yuanBaoIcon","silverIcon","ghostIcon","reputationIcon","moLingIcon","coreIcon","contriIcon"};
	int[] iconTidArr = new int[]{1000001,1000002,1000004,1000006,1000008,1000007,1000009};
    //by chenliang
    //begin

    private DataGroup<D_RoleDetail> mDataGroup;

    private UILabel mStaminaTime;           //体力恢复时间
    private UILabel mStaminaTimeFull;       //体力恢复时间已满
    private UILabel mAllStaminaTime;        //全部体力恢复时间
    private UILabel mAllStaminaTimeFull;    //全部体力恢复时间已满

    private UILabel mSpiritTime;            //精力恢复时间
    private UILabel mSpiritTimeFull;        //精力恢复时间已满
    private UILabel mAllSpiritTime;         //全部精力恢复时间
    private UILabel mAllSpiritTimeFull;     //全部精力恢复时间已满

    private UILabel mBeatDemonCardTime;         //降魔令时间
    private UILabel mBeatDemonCardTimeFull;     //降魔令时间已满
    private UILabel mAllBeatDemonCardTime;      //全部降魔令时间
    private UILabel mAllBeatDemonCardTimeFull;  //全部降魔令时间已满

    //end
    protected override void OpenInit() {
        base.OpenInit();
        ExternalDataStation<PL_RoleDetail, D_RoleDetail> exStation = new ExternalDataStation<PL_RoleDetail, D_RoleDetail>("RoleDetail");
        //by chenliang
        //begin

//         var dataGroup = exStation.GetDataGroup(RoleLogicData.Self.chaLevel);
//         mPipeLineFactory.GetDataGo(new DataGameObject<PL_RoleDetail, D_RoleDetail>(dataGroup,mGameObjUI));
//--------------------
        mDataGroup = exStation.GetDataGroup(RoleLogicData.Self.character.level);
        mPipeLineFactory.GetDataGo(new DataGameObject<PL_RoleDetail, D_RoleDetail>(mDataGroup, mGameObjUI));

        __InitRecoverTimeCallback();
        
        //end
        mPipeLineFactory.GetRefresherDict(exStation.GetRefresherDict());
		AddButtonAction("back", () => {DataCenter.CloseWindow(UIWindowString.role_detail);MainUIScript.Self.ShowMainBGUI();});
        AddButtonAction("role_details_window_bg_btn", () => { DataCenter.CloseWindow(UIWindowString.role_detail); MainUIScript.Self.ShowMainBGUI(); });

    }

    public override void OnOpen() {
        base.OnOpen();
        var role=RoleLogicData.Self;
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Contri_Cost].handlerFunc = datas => role.unionContr;
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Contri_Icon].handlerFunc = datas => datas.GetInt(D_RoleDetail.contriTid);
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Core_Cost].handlerFunc = datas => role.prestige;
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Core_Icon].handlerFunc = datas => datas.GetInt(D_RoleDetail.coreTid);
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Ghost_Cost].handlerFunc = datas => role.soulPoint;
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Ghost_Icon].handlerFunc = datas => datas.GetInt(D_RoleDetail.ghostTid);

        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Level_Label].handlerFunc = datas => RoleLogicData.GetMainRole().level;
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_MoLing_Cost].handlerFunc = datas => role.battleAchv;
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Moling_Icon].handlerFunc = datas => datas.GetInt(D_RoleDetail.moLingTid);

        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Reputation_Cost].handlerFunc = datas => role.reputation;
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Reputation_Icon].handlerFunc = datas => datas.GetInt(D_RoleDetail.reputationTid);

        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Role_Name].handlerFunc = datas => role.name;
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Silver_Cost].handlerFunc = datas => GameCommon.ShowNumUI(role.gold);
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Silver_Icon].handlerFunc = datas => datas.GetInt(D_RoleDetail.silverTid);

        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Vip_Label].handlerFunc = datas => role.vipLevel;
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_YuanBao_Cost].handlerFunc = datas => GameCommon.ShowNumUI(role.diamond);
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_YuanBao_Icon].handlerFunc = datas => datas.GetInt(D_RoleDetail.yuanBaoTid);

        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Stamina_Cost].handlerFunc = datas => role.stamina+"/"+datas.GetInt(D_RoleDetail.stamina);

        //mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Stamina_Cost].handlerFunc=datas => "";


        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_Energy_Cost].handlerFunc = datas => role.spirit+"/"+datas.GetInt(D_RoleDetail.energy);
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_XiangMo_Cost].handlerFunc = datas => role.beatDemonCard+"/"+datas.GetInt(D_RoleDetail.xiangMo);

        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_LevelPer_Label].handlerFunc = datas => RoleLogicData.GetMainRole().exp + "/" + RoleLogicData.GetMainRole().GetMaxExp();
        mPipeLineFactory.refresherDict[PL_RoleDetail.Set_LevelPer_Slider].handlerFunc = datas => RoleLogicData.GetMainRole().exp /(float) RoleLogicData.GetMainRole().GetMaxExp(); 

        //by chenliang
        //begin

//        mPipeLineFactory.ExecuteAllPipeLineExcept(PL_RoleDetail.Set_Silver_Icon, PL_RoleDetail.Set_YuanBao_Icon, PL_RoleDetail.Set_Stamina_Cost, PL_RoleDetail.Set_XiangMo_Cost, PL_RoleDetail.Set_Energy_Cost);
//--------------------
        mPipeLineFactory.ExecuteAllPipeLineExcept(
            new List<PL_RoleDetail>()
            {
                PL_RoleDetail.Set_Silver_Icon,
                PL_RoleDetail.Set_YuanBao_Icon,
                PL_RoleDetail.Set_Stamina_Cost,
                PL_RoleDetail.Set_XiangMo_Cost,
                PL_RoleDetail.Set_Energy_Cost
            });

        __InitRecoverTime();

        __SetStaminaCountUI();
        __SetSpiritCountUI();
        __SetBeatDemonCardCountUI();

        //end

		//added by xuke begin
		__InitRoleInfo ();
		//end
    }

	/// <summary>
	/// 初始化图标等信息
	/// </summary>
	private void __InitRoleInfo()
	{
		// 头像,Test
		GameObject _roleInfoObj = GameCommon.FindObject (mGameObjUI,"role_info");
		GameObject _roleIcon = GameCommon.FindObject (_roleInfoObj,"item_icon");
		GameCommon.SetRoleIcon (_roleIcon.GetComponent<UISprite>(),RoleLogicData.Self.character.tid,GameCommon.ROLE_ICON_TYPE.PHOTO);
		// VIP等级
		GameCommon.SetUIText (mGameObjUI,"vipLabel","VIP" + RoleLogicData.Self.vipLevel.ToString());
		// 战斗力
		GameCommon.SetUIText (mGameObjUI,"fight_strength_number", GameCommon.ShowNumUI(System.Convert.ToInt32(GameCommon.GetPower().ToString("f0"))));
		// 设置货币图标
		UIGridContainer grid = GameCommon.FindObject (mGameObjUI,"Grid").GetComponent<UIGridContainer>();
		int _gridLength = iconNameArr.Length;
		for (int i = 0; i < _gridLength; i++) 
		{
			GameCommon.SetResIcon(grid.controlList[i],iconNameArr[i],iconTidArr[i],false);
		}
	}


	private void __SetIcon (string kParentName,string kIconName,ITEM_TYPE kType)
	{
		GameObject _yuanBaoObj = GameCommon.FindObject(mGameObjUI,kParentName);
		GameObject _spriteObj = GameCommon.FindObject (_yuanBaoObj,kIconName);
		GameCommon.SetOnlyItemIcon (_spriteObj,(int)kType);
	}

    //by chenliang
    //begin

    /// <summary>
    /// 初始化回调函数
    /// </summary>
    private void __InitRecoverTimeCallback()
    {
        //体力
        RoleInfoTimerManager.Instance.AddTimerUpdateCallback(ROLE_INFO_TIMER_TYPE.STAMINA, __OnStaminaUpdate);
        RoleInfoTimerManager.Instance.AddTimerCompleteCallback(ROLE_INFO_TIMER_TYPE.STAMINA, __OnStaminaComplete);

        //精力
        RoleInfoTimerManager.Instance.AddTimerUpdateCallback(ROLE_INFO_TIMER_TYPE.SPIRIT, __OnSpiritUpdate);
        RoleInfoTimerManager.Instance.AddTimerCompleteCallback(ROLE_INFO_TIMER_TYPE.SPIRIT, __OnSpiritComplete);

        //降魔令
        RoleInfoTimerManager.Instance.AddTimerUpdateCallback(ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD, __OnBDCUpdate);
        RoleInfoTimerManager.Instance.AddTimerCompleteCallback(ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD, __OnBDCComplete);
    }
    /// <summary>
    /// 初始化回复时间
    /// </summary>
    private void __InitRecoverTime()
    {
        //体力
        GameObject tmpGOStaminaParent = GameCommon.FindObject(mGameObjUI, "info_group(Clone)_0");
        GameObject tmpGOStaminaRecoverParent = GameCommon.FindObject(tmpGOStaminaParent, "restoer_time");
        mStaminaTime = GameCommon.FindComponent<UILabel>(tmpGOStaminaRecoverParent, "time");
        mStaminaTime.gameObject.SetActive(false);
        mStaminaTimeFull = GameCommon.FindComponent<UILabel>(tmpGOStaminaRecoverParent, "full");
        GameObject tmpGOStaminaAllParent = GameCommon.FindObject(tmpGOStaminaParent, "all_time");
        mAllStaminaTime = GameCommon.FindComponent<UILabel>(tmpGOStaminaAllParent, "time");
        mAllStaminaTime.gameObject.SetActive(false);
        mAllStaminaTimeFull = GameCommon.FindComponent<UILabel>(tmpGOStaminaAllParent, "full");

        //精力
        GameObject tmpGOSpiritParent = GameCommon.FindObject(mGameObjUI, "info_group(Clone)_1");
        GameObject tmpGOSpiritRecoverParent = GameCommon.FindObject(tmpGOSpiritParent, "restoer_time");
        mSpiritTime = GameCommon.FindComponent<UILabel>(tmpGOSpiritRecoverParent, "time");
        mSpiritTime.gameObject.SetActive(false);
        mSpiritTimeFull = GameCommon.FindComponent<UILabel>(tmpGOSpiritRecoverParent, "full");
        GameObject tmpGOSpiritAllParent = GameCommon.FindObject(tmpGOSpiritParent, "all_time");
        mAllSpiritTime = GameCommon.FindComponent<UILabel>(tmpGOSpiritAllParent, "time");
        mAllSpiritTime.gameObject.SetActive(false);
        mAllSpiritTimeFull = GameCommon.FindComponent<UILabel>(tmpGOSpiritAllParent, "full");

        //降魔令
        GameObject tmpGOBDCParent = GameCommon.FindObject(mGameObjUI, "info_group(Clone)_2");
        GameObject tmpGOBDCRecoverParent = GameCommon.FindObject(tmpGOBDCParent, "restoer_time");
        mBeatDemonCardTime = GameCommon.FindComponent<UILabel>(tmpGOBDCRecoverParent, "time");
        mBeatDemonCardTime.gameObject.SetActive(false);
        mBeatDemonCardTimeFull = GameCommon.FindComponent<UILabel>(tmpGOBDCRecoverParent, "full");
        GameObject tmpGOBDCAllParent = GameCommon.FindObject(tmpGOBDCParent, "all_time");
        mAllBeatDemonCardTime = GameCommon.FindComponent<UILabel>(tmpGOBDCAllParent, "time");
        mAllBeatDemonCardTime.gameObject.SetActive(false);
        mAllBeatDemonCardTimeFull = GameCommon.FindComponent<UILabel>(tmpGOBDCAllParent, "full");
    }
    /// <summary>
    /// 设置体力数量
    /// </summary>
    private void __SetStaminaCountUI()
    {
        UILabel tmpLBStaminaCount = GameCommon.FindComponent<UILabel>(mGameObjUI, "staminaLabel");
        if (tmpLBStaminaCount != null)
            tmpLBStaminaCount.text = RoleLogicData.Self.stamina + "/100";
    }
    /// <summary>
    /// 设置精力数量
    /// </summary>
    private void __SetSpiritCountUI()
    {
        UILabel tmpLBSpiritCount = GameCommon.FindComponent<UILabel>(mGameObjUI, "energyLabel");
        if (tmpLBSpiritCount != null)
            tmpLBSpiritCount.text = RoleLogicData.Self.spirit + "/30";
    }
    /// <summary>
    /// 设置降魔令数量
    /// </summary>
    private void __SetBeatDemonCardCountUI()
    {
        UILabel tmpLBBeatDemonCardCount = GameCommon.FindComponent<UILabel>(mGameObjUI, "xiangMoLabel");
        if (tmpLBBeatDemonCardCount != null)
            tmpLBBeatDemonCardCount.text = RoleLogicData.Self.beatDemonCard + "/10";
    }

    /// <summary>
    /// 获取全部恢复结束时间点描述
    /// </summary>
    /// <param name="dstTime"></param>
    /// <returns></returns>
    private string __GetAllRecoverEndTimeDesc(long dstTime)
    {
        string tmpDesc = "";
        System.DateTime tmpDstDate = GameCommon.ConvertServerSecTimeTo1970(dstTime);
        System.DateTime tmpNowDate = GameCommon.ConvertServerSecTimeTo1970(CommonParam.NowServerTime());
        System.TimeSpan tmpSpan = tmpDstDate - tmpNowDate;
		System.DateTime tmpDayEndDate = GameCommon.ConvertServerSecTimeTo1970(CommonParam.NowServerTime());
		tmpDayEndDate = tmpDayEndDate.AddDays(1);
		tmpDayEndDate = tmpDayEndDate.AddSeconds(-(tmpDayEndDate.Hour * 3600 + tmpDayEndDate.Minute * 60 + tmpDayEndDate.Second));
		System.TimeSpan tmpRestTime = tmpDayEndDate - tmpNowDate;
		System.TimeSpan tmpTS = tmpSpan - tmpRestTime;
		if (tmpTS.Ticks >= 0)
			tmpDesc += "明天";
		else if (tmpTS.Days >= 1)
			tmpDesc += "后天";
		else
			tmpDesc += "今天";
		tmpDesc += string.Format("{0:hh:mm:ss}", tmpDstDate.TimeOfDay);
        return tmpDesc;
    }

    private void __OnStaminaUpdate(float leftTime)
    {
        if (mStaminaTime != null)
        {
            mStaminaTime.gameObject.SetActive(leftTime > 0.0f);
            mStaminaTime.text = GameCommon.FormatTime(leftTime);
        }
        if (mStaminaTimeFull != null)
            mStaminaTimeFull.gameObject.SetActive(leftTime <= 0.0f);

        int tmpMaxValue = RoleInfoTimerManager.Instance.GetRecoverMaxValue(ROLE_INFO_TIMER_TYPE.STAMINA);
        int tmpDelta = RoleInfoTimerManager.Instance.GetRecoverDelta(ROLE_INFO_TIMER_TYPE.STAMINA);
        int tmpLeftTime = (tmpMaxValue - RoleLogicData.Self.stamina) * tmpDelta;
        long tmpDstTime = RoleLogicData.Self.staminaStamp + tmpLeftTime;
        if (mAllStaminaTime != null)
        {
            mAllStaminaTime.gameObject.SetActive(tmpLeftTime > 0);
            mAllStaminaTime.text = __GetAllRecoverEndTimeDesc(tmpDstTime);
        }
        if (mAllStaminaTimeFull != null)
            mAllStaminaTimeFull.gameObject.SetActive(tmpLeftTime <= 0);
    }
    private void __OnStaminaComplete()
    {
        __OnStaminaUpdate(0.0f);

        __SetStaminaCountUI();

//         DataGroup<D_RoleDetail> dataGroup = new DataGroup<D_RoleDetail>();
//         dataGroup.SetDictValue(D_RoleDetail.stamina, RoleLogicData.Self.stamina);
//         mPipeLineFactory.ExecutePipeLine(PL_RoleDetail.Set_Stamina_Cost, 0, dataGroup);
    }
    private void __OnSpiritUpdate(float leftTime)
    {
        if (mSpiritTime != null)
        {
            mSpiritTime.gameObject.SetActive(leftTime > 0.0f);
            mSpiritTime.text = GameCommon.FormatTime(leftTime);
        }
        if (mSpiritTimeFull != null)
            mSpiritTimeFull.gameObject.SetActive(leftTime <= 0.0f);

        int tmpMaxValue = RoleInfoTimerManager.Instance.GetRecoverMaxValue(ROLE_INFO_TIMER_TYPE.SPIRIT);
        int tmpDelta = RoleInfoTimerManager.Instance.GetRecoverDelta(ROLE_INFO_TIMER_TYPE.SPIRIT);
        int tmpLeftTime = (tmpMaxValue - RoleLogicData.Self.spirit) * tmpDelta;
        long tmpDstTime = RoleLogicData.Self.spiritStamp + tmpLeftTime;
        if (mAllSpiritTime != null)
        {
            mAllSpiritTime.gameObject.SetActive(tmpLeftTime > 0);
            mAllSpiritTime.text = __GetAllRecoverEndTimeDesc(tmpDstTime);
        }
        if (mAllSpiritTimeFull != null)
            mAllSpiritTimeFull.gameObject.SetActive(tmpLeftTime <= 0);
    }
    private void __OnSpiritComplete()
    {
        __OnSpiritUpdate(0.0f);

        __SetSpiritCountUI();

//         DataGroup<D_RoleDetail> dataGroup = new DataGroup<D_RoleDetail>();
//         dataGroup.SetDictValue(D_RoleDetail.energy, RoleLogicData.Self.spirit);
//         mPipeLineFactory.ExecutePipeLine(PL_RoleDetail.Set_Energy_Cost, 0, dataGroup);
    }
    private void __OnBDCUpdate(float leftTime)
    {
        if (mBeatDemonCardTime != null)
        {
            mBeatDemonCardTime.gameObject.SetActive(leftTime > 0.0f);
            mBeatDemonCardTime.text = GameCommon.FormatTime(leftTime);
        }
        if (mBeatDemonCardTimeFull != null)
            mBeatDemonCardTimeFull.gameObject.SetActive(leftTime <= 0.0f);

        int tmpMaxValue = RoleInfoTimerManager.Instance.GetRecoverMaxValue(ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD);
        int tmpDelta = RoleInfoTimerManager.Instance.GetRecoverDelta(ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD);
        int tmpLeftTime = (tmpMaxValue - RoleLogicData.Self.beatDemonCard) * tmpDelta;
        long tmpDstTime = RoleLogicData.Self.beatDemonCardStamp + tmpLeftTime;
        if (mAllBeatDemonCardTime != null)
        {
            mAllBeatDemonCardTime.gameObject.SetActive(tmpLeftTime > 0);
            mAllBeatDemonCardTime.text = __GetAllRecoverEndTimeDesc(tmpDstTime);
        }
        if (mAllBeatDemonCardTimeFull != null)
            mAllBeatDemonCardTimeFull.gameObject.SetActive(tmpLeftTime <= 0);
    }
    private void __OnBDCComplete()
    {
        __OnBDCUpdate(0.0f);

        __SetBeatDemonCardCountUI();

//         DataGroup<D_RoleDetail> dataGroup = new DataGroup<D_RoleDetail>();
//         dataGroup.SetDictValue(D_RoleDetail.xiangMo, RoleLogicData.Self.beatDemonCard);
//         mPipeLineFactory.ExecutePipeLine(PL_RoleDetail.Set_XiangMo_Cost, 0, dataGroup);
    }

    //end
}

public class Button_OpenRoleDetail : Logic.CEvent {
    public override bool _DoEvent() {
		MainUIScript.Self.HideMainBGUI (); 
        DataCenter.OpenWindow(UIWindowString.role_detail);
        return true;
    }
}
