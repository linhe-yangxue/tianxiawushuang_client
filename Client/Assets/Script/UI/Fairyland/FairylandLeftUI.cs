using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

//符灵探险操作界面左边

public class FairylandLeftWindow : tWindow
{
    protected static string[] smSubWindow = new string[] {
        "Point",
        "explore_pet_add_button",
        "intro_info",
        "get_info_group"
    };

    public override void Open(object param)
    {
        base.Open(param);

        __HideAllWindow();

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        FairylandOPWindowData tmpOPData = param as FairylandOPWindowData;
        if (tmpOPData == null)
            return true;

        _OnRefresh(tmpOPData);

        return true;
    }

    /// <summary>
    /// 隐藏有子窗口
    /// </summary>
    private void __HideAllWindow()
    {
        for (int i = 0, count = smSubWindow.Length; i < count; i++)
            SetVisible(smSubWindow[i], false);

        GameCommon.SetUIVisiable(mGameObjUI, "_role_", false);
//         ActiveBirthForUI birthUI = GameCommon.FindComponent<ActiveBirthForUI>(mGameObjUI, "Point");
//         if (birthUI.mActiveObject != null)
//             birthUI.mActiveObject.OnDestroy();
    }

    protected virtual void _OnRefresh(FairylandOPWindowData opData)
    {
    }

    protected void _ShowModel(int bossID, FAIRYLAND_OP_WINDOW_TYPE winType, bool isSearchOver)
    {
        ActiveBirthForUI birthUI = GameCommon.FindComponent<ActiveBirthForUI>(mGameObjUI, "Point");
        birthUI.enabled = true;
        if (birthUI.mActiveObject != null)
            birthUI.mActiveObject.OnDestroy();

        if (bossID == -1 || bossID == 0)
            return;

        birthUI.mBirthConfigIndex = bossID;
        birthUI.Init();
        if (birthUI.mActiveObject != null)
        {
            birthUI.mActiveObject.SetScale(80f);
            string strAnim = "cute";
            switch (winType)
            {
                case FAIRYLAND_OP_WINDOW_TYPE.CONQUER_READY: strAnim = "cute"; break;
                case FAIRYLAND_OP_WINDOW_TYPE.SEARCH_READY: strAnim = "cute"; break;
                case FAIRYLAND_OP_WINDOW_TYPE.SEARCH_RECORD: strAnim = isSearchOver ? "win" : "run"; break;
                case FAIRYLAND_OP_WINDOW_TYPE.REPRESS_RIOT: strAnim = "attack"; break;
            }
            tEvent tmpEvt = EventCenter.Self.StartEvent("RoleSelUI_PlayIdleEvent");
            birthUI.mActiveObject.PlayMotion(strAnim, tmpEvt);

            BossBirthOnApearUI modelScript = birthUI.mActiveObject.mMainObject.AddComponent<BossBirthOnApearUI>();
            if (modelScript != null)
                modelScript.mActiveObject = birthUI.mActiveObject;
        }

        GlobalModule.DoCoroutine(GameCommon.IE_ChangeRenderQueue(bossID, birthUI.mActiveObject.mMainObject, 3150));
    }
}

/// <summary>
/// 挑战Boss信息
/// </summary>
public class FairylandLeftInfoWindow : FairylandLeftWindow
{
    protected override void _OnRefresh(FairylandOPWindowData opData)
    {
        base._OnRefresh(opData);

        DataRecord tmpFairylandConfig = DataCenter.mFairylandConfig.GetRecord(opData.FairylandTid);
        if (tmpFairylandConfig == null)
        {
            FairylandLog.LogError("找不到配置" + opData.FairylandTid.ToString());
            return;
        }

        //BOSS
        int tmpStageID = (int)tmpFairylandConfig.getObject("STATE_ID");
        DataRecord tmpStageConfig = DataCenter.mStageTable.GetRecord(tmpStageID);
        int tmpBossID = (int)tmpStageConfig.getObject("HEADICON");
        _ShowModel(tmpBossID, opData.WindowType, false);

        //仙境描述
        GameObject tmpIntroIndo = GameCommon.FindObject(mGameObjUI, "intro_info");
        tmpIntroIndo.gameObject.SetActive(true);
        tmpIntroIndo.GetComponent<UILabel>().text = tmpFairylandConfig.getData("DESC");

		//仙境图片
		UITexture texture = GameCommon.FindObject(mGameObjUI, "pet_choose_texture").GetComponent<UITexture>();		
		if(texture != null)
		{
			string strPicName = "textures/UItextures/Background/" + "a_ui_xunxianbeijing0" + opData.FairylandTid;
			texture.mainTexture = Resources.Load(strPicName, typeof(Texture)) as Texture;
			
			if(texture.mainTexture == null)
			{
				DEBUG.LogError("texture.mainTexture == null");
			}
		}
    }
}

/// <summary>
/// 符灵选择
/// </summary>
public class FairylandLeftPetSelWindow : FairylandLeftWindow
{
    FairylandOPWindowData mOPData;
    private int mSelPetTid = -1;

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_explore_pet_add_button", new DefineFactoryLog<Button_explore_pet_add_button>());
    }

    public override void Open(object param)
    {
        mOPData = param as FairylandOPWindowData;
        mSelPetTid = -1;

        base.Open(param);

        NiceData tmpBtnAddData = GameCommon.GetButtonData(mGameObjUI, "explore_pet_add_button");
        if (tmpBtnAddData != null)
            tmpBtnAddData.set("OP_DATA", mOPData);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "SELECT_PET":
                {
                    mSelPetTid = (int)objVal;
                    _OnRefresh(mOPData);
                } break;
        }
    }

    protected override void _OnRefresh(FairylandOPWindowData opData)
    {
        base._OnRefresh(opData);

        DataRecord tmpFairylandConfig = DataCenter.mFairylandConfig.GetRecord(opData.FairylandTid);
        if (tmpFairylandConfig == null)
        {
            FairylandLog.LogError("找不到配置FairylandConfig - " + opData.FairylandTid.ToString());
            return;
        }

        //加符灵按钮
        GameCommon.SetUIVisiable(mGameObjUI, "explore_pet_add_button", mSelPetTid == -1);

        //BOSS
        ActiveBirthForUI tmpBirth = GameCommon.FindComponent<ActiveBirthForUI>(mGameObjUI, "Point");
        _ShowModel(mSelPetTid, FAIRYLAND_OP_WINDOW_TYPE.SEARCH_READY, false);

        //仙境描述
        GameObject tmpIntroIndo = GameCommon.FindObject(mGameObjUI, "intro_info");
        tmpIntroIndo.gameObject.SetActive(true);
        tmpIntroIndo.GetComponent<UILabel>().text = tmpFairylandConfig.getData("DESC");

		//仙境图片
		UITexture texture = GameCommon.FindObject(mGameObjUI, "pet_choose_texture").GetComponent<UITexture>();		
		if(texture != null)
		{
			string strPicName = "textures/UItextures/Background/" + "a_ui_xunxianbeijing0" + opData.FairylandTid;
			texture.mainTexture = Resources.Load(strPicName, typeof(Texture)) as Texture;
			
			if(texture.mainTexture == null)
			{
				DEBUG.LogError("texture.mainTexture == null");
			}
		}

    }
}

/// <summary>
/// 加符灵
/// </summary>
class Button_explore_pet_add_button : CEvent
{
    private FairylandOPWindowData mOPData;

    public override bool _DoEvent()
    {
        mOPData = getObject("OP_DATA") as FairylandOPWindowData;

        //TODO
        //手动选定一个已有的符灵
//         ActiveData tmpData = null;
//         for (int i = (int)TEAM_POS.PET_1; i < (int)TEAM_POS.MAX; i++)
//         {
//             tmpData = TeamManager.GetBodyDataByTeamPos(i);
//             if (tmpData != null)
//                 break;
//         }
//         if (tmpData == null)
//             return true;
//         __OnSelPetComplete(tmpData.tid, tmpData.itemId);

        OpenBagObject<PetData> openObj = new OpenBagObject<PetData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.FAIRYLAND;
        openObj.mFilterCondition = (itemData) =>
        {
            bool isInFairyland = false;
            foreach (int tid in FairylandWindow.explore_list)
            {
                if (tid == itemData.tid)
                {
                    isInFairyland = true;
                    break;
                }
            }
            //不显示已经寻仙的符灵和经验蛋、白蓝绿符灵
            return (!isInFairyland && (itemData.starLevel >= 4 && itemData.tid != 30293 && itemData.tid != 30299 && itemData.tid != 30305));
        };

        openObj.mSortCondition = (tempList) =>
        {
            DataCenter.Set("DESCENDING_ORDER_QUALITY", true);
            return GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByTeamPosType);
        };

        openObj.mSelectAction = __OnSelPetComplete;

        DataCenter.SetData("BAG_PET_WINDOW", "OPEN", openObj);

        return true;
    }

    /// <summary>
    /// 选择符灵结束
    /// </summary>
    /// <param name="petId"></param>
    private void __OnSelPetComplete(PetData petData)
    {
        if (petData == null)
            return;

        //设置新的窗口类型后，重新打开操作窗口
        mOPData.WindowType = FAIRYLAND_OP_WINDOW_TYPE.SEARCH_READY;
        DataCenter.OpenWindow("FAIRYLAND_OP_WINDOW", mOPData);
        DataCenter.SetData("FAIRYLAND_LEFT_PET_SEL_WINDOW", "SELECT_PET", petData.tid);
        DataCenter.SetData("FAIRYLAND_RIGHT_SEARCH_READY_WINDOW", "SELECT_PET", petData.itemId);
        DataCenter.SetData("FAIRYLAND_RIGHT_SEARCH_READY_WINDOW", "SELECT_PET_TID", petData.tid);
    }
}

/// <summary>
/// 符灵记录
/// </summary>
public class FairylandLeftRecordWindow : FairylandLeftWindow
{
    private long mRefreshTime;              //定时刷新间隔
    private FairylandOPWindowData mOPData;

    public override void Init()
    {
        base.Init();
    }

    public override void Open(object param)
    {
        mOPData = param as FairylandOPWindowData;

        base.Open(param);

        __InitUI();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_RECORD":
                {
                    __RefreshRecord(objVal);
                    __StartRefreshTimer();
                } break;
        }
    }

    private void __InitUI()
    {
        //BOSS
        ActiveBirthForUI tmpBirth = GameCommon.FindComponent<ActiveBirthForUI>(mGameObjUI, "Point");
        tmpBirth.gameObject.SetActive(false);

        //描述
        GameObject tmpGroup = GameCommon.FindObject(mGameObjUI, "get_info_group");
        tmpGroup.gameObject.SetActive(true);
//         UIGridContainer tmpGridContainer = GameCommon.FindComponent<UIGridContainer>(tmpGroup, "grid");
//         tmpGridContainer.MaxCount = 0;
//         tmpGridContainer.Reposition();
        GameObject tmpLabelInfo = GameCommon.FindObject(tmpGroup, "get_all_label_info");
        GameCommon.FindComponent<UILabel>(tmpLabelInfo, "time").text = "";
		//仙境图片
		UITexture texture = GameCommon.FindObject(mGameObjUI, "pet_choose_texture").GetComponent<UITexture>();		
		if(texture != null)
		{
			string strPicName = "textures/UItextures/Background/" + "a_ui_xunxianbeijing0" + mOPData.FairylandTid;
			texture.mainTexture = Resources.Load(strPicName, typeof(Texture)) as Texture;
			
			if(texture.mainTexture == null)
			{
				DEBUG.LogError("texture.mainTexture == null");
			}
		}
    }

    private void __RefreshRecord(object param)
    {
        SC_Fairyland_GetFairylandEvents resp = param as SC_Fairyland_GetFairylandEvents;

        long tmpCurrServerTime = CommonParam.NowServerTime();
        bool isSearchOver = (resp.endTime < tmpCurrServerTime);
        mRefreshTime = resp.refreshTime * 60;

        int tmpTid = -1;
//         //BOSS
//         if (mOPData.State == FAIRYLAND_ELEMENT_STATE.RIOTING)
//         {
//             DataRecord tmpFairylandConfig = DataCenter.mFairylandConfig.GetRecord(mOPData.FairylandTid);
//             DataRecord tmpStageConfig = DataCenter.mStageTable.GetRecord((int)tmpFairylandConfig.getObject("STATE_ID"));
//             tmpTid = (int)tmpStageConfig.getObject("HEADICON");
//         }
//         else
//             tmpTid = resp.petTid;
        //只用符灵
        tmpTid = resp.petTid;
        _ShowModel(tmpTid, mOPData.WindowType, isSearchOver);

        //描述
        GameObject tmpGroup = GameCommon.FindObject(mGameObjUI, "get_info_group");
//         UIGridContainer tmpGridContainer = GameCommon.FindComponent<UIGridContainer>(tmpGroup, "grid");
//         int tmpCount = resp.events.Length;
//         tmpGridContainer.MaxCount = tmpCount;
//         for (int i = 0; i < tmpCount; i++)
//         {
//             GameObject tmpItem = tmpGridContainer.controlList[i];
//             UILabel tmpDesc = GameCommon.FindComponent<UILabel>(tmpItem, "time");
//             tmpDesc.text = __GetSearchRecordInfo(resp.events[i]);
//         }
//         tmpGridContainer.Reposition();
        string tmpInfo = "";
        int tmpCount = resp.events.Length;
        for (int i = 0; i < tmpCount; i++)
        {
            if (i != 0)
                tmpInfo += "\n";
			tmpInfo += __GetSearchRecordInfo(resp.events[i],tmpTid);
        }
        GameObject tmpLabelInfo = GameCommon.FindObject(tmpGroup, "get_all_label_info");
        GameCommon.FindComponent<UILabel>(tmpLabelInfo, "time").text = tmpInfo;
    }

	private string __GetSearchRecordInfo(FairylandEvent fairylandEvt, int petTid)
    {
        string tmpDescIdx = fairylandEvt.index.ToString();
        string tmpDescMark = tmpDescIdx.Substring(0, 1);
        DataRecord tmpRecordConfig = DataCenter.mFairylandDescConfig.GetRecord(fairylandEvt.index);
        if(tmpRecordConfig == null)
        {
            FairylandLog.LogError("找不到配置FairylandDescConfig - " + fairylandEvt.index.ToString());
            return "";
        }
        string tmpRecordInfo = tmpRecordConfig.getData("DESC");
        switch (tmpDescMark)
        {
            case "2":
                {
                    tmpRecordInfo = string.Format(tmpRecordInfo, fairylandEvt.friendName);
                }break;
            case "3":
                {
					tmpRecordInfo = string.Format(tmpRecordInfo, new object[] { GameCommon.GetItemName(petTid), GameCommon.GetItemName(fairylandEvt.tid), fairylandEvt.itemNum });
                }break;
		    case "4":
		 		{
					tmpRecordInfo = string.Format(tmpRecordInfo, new object[] { GameCommon.GetItemName(petTid), GameCommon.GetItemName(fairylandEvt.tid), fairylandEvt.itemNum });
				}break;
        }
        return tmpRecordInfo;
    }

    /// <summary>
    /// 开始定时刷新
    /// </summary>
    private void __StartRefreshTimer()
    {
        if (mRefreshTime < 0)
            return;
        DoCoroutine(__RefreshTimer());
    }
    private IEnumerator __RefreshTimer()
    {
        yield return new WaitForSeconds(mRefreshTime);
        yield return GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandEvents(mOPData.FairylandTid));
    }
}
