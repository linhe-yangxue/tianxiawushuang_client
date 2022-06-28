using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System;

public enum GrabTreasureReslutItemState
{
    USEPROP,        //消耗精力丹
    CANCOMPOSE      //可以合成
}
//夺宝之战玩家列表界面

public class GrabTreasureFightWindow : tWindow
{
    private int mTargetFragId = -1;        //目标碎片id
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_grab_fight_five_button", new DefineFactoryLog<Button_grab_fight_five_button>());
        EventCenter.Self.RegisterEvent("Button_grab_fight_one_button", new DefineFactoryLog<Button_grab_fight_one_button>());
        EventCenter.Self.RegisterEvent("Button_grab_fight_change_button", new DefineFactoryLog<Button_grab_fight_change_button>());
        EventCenter.Self.RegisterEvent("Button_grab_fight_close_button", new DefineFactoryLog<Button_grab_fight_close_button>());
        EventCenter.Self.RegisterEvent("Button_grab_key_rod_btn", new DefineFactory<Button_grab_key_rod_btn>());
        EventCenter.Self.RegisterEvent("Button_mseeage_state_btn", new DefineFactory<Button_mseeage_state_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_fight_window", new DefineFactoryLog<Button_grab_fight_close_button>());

    }

    public override void Open(object param)
    {
        base.Open(param);

        mTargetFragId = (int)param;

        NiceData btnRefresh = GameCommon.GetButtonData(mGameObjUI, "grab_fight_change_button");
        if (btnRefresh != null)
            btnRefresh.set("TARGET_FRAG_ID", mTargetFragId);

        Refresh(null);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_FIGHT_LIST":
                {
                    __RefreshFightList(objVal);
                } break;
            case "REFRESH_COUNTDOWN":
                {
                    long tmpServerTime = (long)objVal;
                    __RefreshButtonCountdown(tmpServerTime);
                } break;
        }
    }

    public override bool Refresh(object param)
    {
        __RefreshRoleInfo();

        return true;
    }

    /// <summary>
    /// 刷新角色信息
    /// </summary>
    private void __RefreshRoleInfo()
    {
        //精力
        GameCommon.SetUIText(GetSub("token_info"), "spirit_num_label", RoleLogicData.Self.spirit.ToString());
    }

    /// <summary>
    /// 刷新战斗列表
    /// </summary>
    private void __RefreshFightList(object param)
    {
        SC_GetRobAimList tmpResp = param as SC_GetRobAimList;
        if (tmpResp == null)
            return;

        UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "grab_fight__grid");
        if (grid == null)
            return;
        int count = tmpResp.arr.Length;
        grid.MaxCount = count;
        for (int i = 0; i < count; i++)
        {
            RobAim tmpGrabData = tmpResp.arr[i];
            __RefreshFightListItem(grid.controlList[i], tmpGrabData);
        }
    }

    /// <summary>
    /// 刷新战斗列表元素
    /// </summary>
    /// <param name="item"></param>
    /// <param name="grabData"></param>
    private void __RefreshFightListItem(GameObject item, RobAim grabData)
    {
        bool tmpIsRobot = GameCommon.IsRobot(grabData.uid);

        //图标
        GameObject tmpIconParent = GameCommon.FindObject(item, "item_group");
        UISprite tmpRoleIcon = GameCommon.FindComponent<UISprite>(tmpIconParent, "item_icon(Clone)_0");
        GameCommon.SetOnlyItemIcon(tmpRoleIcon.gameObject, grabData.charTid);
        int[] tmpPetsIcon = grabData.petTid;
        for (int i = 0; i < 3; i++)
        {
            UISprite tmpSpritePetIcon = GameCommon.FindComponent<UISprite>(tmpIconParent, "item_icon(Clone)_" + (i + 1).ToString());
            if (i < tmpPetsIcon.Length)
            {
                tmpSpritePetIcon.gameObject.SetActive(true);
                GameCommon.SetPetIcon(tmpSpritePetIcon, tmpPetsIcon[i]);
            }
            else
                tmpSpritePetIcon.gameObject.SetActive(false);
        }

        GameObject tmpRoleParent = GameCommon.FindObject(item, "info_labels");
        //玩家名
        GameCommon.SetUIText(tmpRoleParent, "name_label", grabData.name);
        //等级
        GameCommon.SetUIText(tmpRoleParent, "level_label", grabData.level.ToString());
        //概率
        GameCommon.SetUIText(tmpRoleParent, "fight_label", __GetProbilityString(mTargetFragId, grabData.level, tmpIsRobot));

        //夺五次
        DataRecord mvip = DataCenter.mVipListConfig.GetRecord(RoleLogicData.Self.vipLevel);
        if (mvip == null)
        {
            return;
        }
        GameObject btnone = GameCommon.FindObject(item, "grab_fight_one_button");
        if (!GameCommon.IsFuncCanShow(item, "grab_fight_five_button"))
        {
            GameCommon.SetUIVisiable(item, "grab_fight_five_button", false);
            btnone.transform.localPosition = new Vector3(295, 0, 0);
        }
        else
        {
            btnone.transform.localPosition = new Vector3(295, -27, 0);
            GameCommon.SetUIVisiable(item, "grab_fight_five_button", tmpIsRobot);
            NiceData btnGrab5Times = GameCommon.GetButtonData(item, "grab_fight_five_button");
            if (btnGrab5Times != null)
            {
                btnGrab5Times.set("TARGET_FRAG_ID", mTargetFragId);
                btnGrab5Times.set("TARGET_UID", grabData.uid);
                //设置夺宝次数显示
                GameObject _grabBtn = GameCommon.FindObject(item, "grab_fight_five_button");
                UILabel _grabTimesLbl = GameCommon.FindComponent<UILabel>(_grabBtn, "label");
                //_grabTimesLbl.text = string.Format("夺宝{0}次",Mathf.Min(RoleLogicData.Self.spirit / 2,5).ToString());
                _grabTimesLbl.text = string.Format("夺宝{0}次", "5");
            }
        }

        //夺宝
        NiceData btnGrab = GameCommon.GetButtonData(item, "grab_fight_one_button");
        if (btnGrab != null)
        {
            btnGrab.set("TARGET_FRAG_ID", mTargetFragId);
            btnGrab.set("TARGET_UID", grabData.uid);
            btnGrab.set("IS_ROBOT", tmpIsRobot);
        }
    }

    /// <summary>
    /// 获取抢宝物概率字符串
    /// </summary>
    /// <returns></returns>
    private string __GetProbilityString(int fragTid, int targetLevel, bool isRobot)
    {
        DataRecord fragConfig = DataCenter.mFragmentAdminConfig.GetRecord(fragTid);
        if (fragConfig == null)
            return "";
        string str = "";
        string _gaoPro = "[ff9900]高概率[-]";
        string _jiaoGaoPro = "[cc33ff]较高概率[-]";
        string _yibanPro = "[0099cc]一般概率[-]";
        string _jiaodiPro = "[009900]较低概率[-]";
        int magicId = (int)fragConfig.getData("ROLEEQUIPID");
        EQUIP_QUALITY_TYPE magicType = (EQUIP_QUALITY_TYPE)TableCommon.GetNumberFromRoleEquipConfig(magicId, "QUALITY");
        switch (magicType)
        {
            case EQUIP_QUALITY_TYPE.GOOD:
                {
                    str = _gaoPro;
                } break;
            case EQUIP_QUALITY_TYPE.BETTER:
                {
                    str = isRobot ? _yibanPro : _gaoPro;
                } break;
            case EQUIP_QUALITY_TYPE.BEST:
                {
                    if (isRobot)
                        str = _jiaodiPro;
                    else
                    {
                        int tmpLvDelta = Mathf.Abs(targetLevel - RoleLogicData.Self.character.level);
                        if (tmpLvDelta <= 5)
                            str = _jiaoGaoPro;
                        else
                            str = _gaoPro;
                    }
                } break;
        }
        return str;
    }

    private void __RefreshButtonCountdown(long serverTime)
    {
        UILabel tmpLabel = GameCommon.FindComponent<UILabel>(mGameObjUI, "grab_fight_refresh_countdown_label");
        tmpLabel.gameObject.SetActive(true);
        GrabFightRefreshCountdownUI.SetCountdown(mGameObjUI, "grab_fight_refresh_countdown_label", serverTime, new CallBack(this, "RefreshButtonCountdownEnd", null));
    }
    public void RefreshButtonCountdownEnd(object param)
    {
        UILabel tmpLabel = GameCommon.FindComponent<UILabel>(mGameObjUI, "grab_fight_refresh_countdown_label");
        tmpLabel.gameObject.SetActive(false);
    }
}

/// <summary>
/// 换一批按钮
/// </summary>
class Button_grab_fight_change_button : CEvent
{
    private GameObject mGOMySelf;

    public override bool _DoEvent()
    {
        float tmpDisableTime = 5.0f;
        int tmpTargetFragId = (int)getObject("TARGET_FRAG_ID");
        GlobalModule.DoCoroutine(__DoAction(tmpTargetFragId, tmpDisableTime));

        return true;
    }

    private IEnumerator __DoAction(int targetFragId, float disableTime)
    {
        GrabTreasure_GetRobAimList_Requester tmpRequester = new GrabTreasure_GetRobAimList_Requester();
        tmpRequester.Tid = targetFragId;
        yield return tmpRequester.Start();

        if (tmpRequester.respCode == 1)
        {
            DataCenter.SetData("GRABTREASURE_FIGHT_WINDOW", "REFRESH_FIGHT_LIST", tmpRequester.respMsg);
            __DisableButton(disableTime);
        }
    }

    private void __DisableButton(float time)
    {
        mGOMySelf = getObject("BUTTON") as GameObject;
        if (mGOMySelf == null)
            return;

        mGOMySelf.GetComponent<UIImageButton>().isEnabled = false;
        mGOMySelf.GetComponent<BoxCollider>().enabled = false;
        GuideManager.ExecuteDelayed(() => __OnEnableButton(), time);
        DataCenter.SetData("GRABTREASURE_FIGHT_WINDOW", "REFRESH_COUNTDOWN", CommonParam.NowServerTime() + (long)time - 1);
    }
    private void __OnEnableButton()
    {
        mGOMySelf.GetComponent<UIImageButton>().isEnabled = true;
        mGOMySelf.GetComponent<BoxCollider>().enabled = true;
    }
}

/// <summary>
/// 多次夺宝按钮,不一定是夺宝5次,当精力<10的情况下,夺宝次数 = 当前精力/2 的整数部分  
/// </summary>
class Button_grab_fight_five_button : CEvent
{
    public override bool _DoEvent()
    {
        //判断精力是否足够
        //        int needSpirit = 10;    //2 * 5
        if (RoleLogicData.Self.spirit < 2)
        {
            //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_SPIRIT_NO_ENOUGH);
            GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.SPIRIT_ITEM);
            return true;
        }

        int fragId = (int)getObject("TARGET_FRAG_ID");
        string robedId = (string)getObject("TARGET_UID");
        GlobalModule.DoCoroutine(__DoAction(robedId, fragId));
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.YIJIAN_GRABTREASURE);
        return true;
    }

    private IEnumerator __DoAction(string aimUid, int tid)
    {
        GrabTreasure_Rob5Times_Requester tmpRequester = new GrabTreasure_Rob5Times_Requester();
        tmpRequester.AimUid = aimUid;
        tmpRequester.Tid = tid;
        yield return tmpRequester.Start();

        if (tmpRequester.respCode == 1)
        {
            SC_Rob5Times tmpResp = tmpRequester.respMsg;

            //减精力
            int tmpCostSpirit = tmpResp.MaxGrabCount * 2;
            RoleLogicData.Self.AddSpirit(-tmpCostSpirit);

            //加物品
            for (int i = 0, count = tmpResp.allAddItems.Length; i < count; i++)
            {
                if (tmpResp.allAddItems[i].itemId == -1)
                    PackageManager.AddItem(tmpResp.allAddItems[i]);
                else
                    PackageManager.UpdateItem(tmpResp.allAddItems[i]);
            }

            DataCenter.OpenWindow("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", tid);
            DataCenter.SetData("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", "REFRESH", tmpResp);
        }
    }
}

/// <summary>
/// 夺宝按钮
/// </summary>
class Button_grab_fight_one_button : CEvent
{
    public override bool _DoEvent()
    {
        //判断精力是否足够
        int needSpirit = 2;
        if (RoleLogicData.Self.spirit < needSpirit)
        {
            //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_SPIRIT_NO_ENOUGH);
            GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.SPIRIT_ITEM);
            return true;
        }

        //判断是否在免战中
        bool tmpIsRobot = (bool)getObject("IS_ROBOT");
        if (GrabTreasureWindow.IsPeace && !tmpIsRobot)
            DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_GRAB_IN_TRUCE_NOW, "", __StartGrab);
        else
            __StartGrab();

        return true;
    }

    private void __StartGrab()
    {
        string tmpUid = (string)getObject("TARGET_UID");
        int tmpFragId = (int)getObject("TARGET_FRAG_ID");
        GlobalModule.DoCoroutine(__DoAction(tmpUid, tmpFragId));
    }

    private IEnumerator __DoAction(string aimUid, int tid)
    {
        GrabTreasure_RobMagicStart_Requester tmpRequester = new GrabTreasure_RobMagicStart_Requester();
        tmpRequester.AimUid = aimUid;
        tmpRequester.Tid = tid;
        yield return tmpRequester.Start();

        if (tmpRequester.respCode == 1)
            __StartFight(aimUid, tid, tmpRequester.respMsg);
    }

    private void __StartFight(string aimUid, int fragId, SC_RobMagicStart resp)
    {
        if (resp.noEnoughFrag == 1)
        {
            //目标碎片不足
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_TARGET_FRAGMENT_NO_ENOUGH);
            return;
        }
        if (resp.InTruce == 1)
        {
            //目标在免战中
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_TARGET_IN_TRUCE_TIME);
            return;
        }

        //减精力(精力改在结算时扣除)
        //int tmpCostSpirit = 2;
        //RoleLogicData.Self.AddSpirit(-tmpCostSpirit);
        //去除免战
        bool tmpIsRobot = (bool)getObject("IS_ROBOT");
        if (!tmpIsRobot)
            DataCenter.SetData("GRABTREASURE_WINDOW", "REFRESH_PEACE_TIME", (long)0);

        GrabTreasureWindow win = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        win.set("CURRENT_GRAB_FRAG_ID", fragId);
        win.set("CURRENT_GRAB_UID", aimUid);
        win.set("CURRENT_CHALLENGE_PLAYER", resp.opponent);

        //进入战斗
        GlobalModule.ClearAllWindow();
        //        MainProcess.ClearBattle();
        int grabPVPBattleIndex = 27001;
        DataCenter.Set("CURRENT_STAGE", grabPVPBattleIndex);
        MainProcess.LoadBattleLoadingScene("GRABTREASURE_BATTLE_LOADING_WINDOW");
        //        GrabTreasureWindow.StartGrabBattle();

        //TODO 临时结算
        //        int isWin = 1;// (UnityEngine.Random.Range(0, 1) == 1) ? 1 : 0;
        //        RequestResultIndiana(isWin, fragId, grabType, grabUid, level);
    }
}
//一键夺宝
class Button_grab_key_rod_btn : CEvent
{
    public bool flag;
    public static string equipName;
    public IEnumerator __Action(int tid, int times)
    {
        bool bIsChooseState = (bool)DataCenter.Get("IS_CHOOSE_STATE");
        DataRecord targetmagic = DataCenter.mEquipComposeConfig.GetRecord(tid);
        int length = targetmagic["NUM"];
        int magictid = tid;
        int count = 0;
        bool isFirst = false;
        for (int i = 1; i < length + 1; )
        {
            EquipFragmentData tmpFragData = null;
            int fragID = targetmagic["FRAGMENT_" + i.ToString()];
            
            if (MagicFragmentLogicData.Self.mDicEquipFragmentData.TryGetValue(fragID, out tmpFragData))
            {
                i++;
                count++;
                if (count == length)
                { DataCenter.OnlyTipsLabelMessage("当前可以合成神器！"); }
            }
            else
            {
                isFirst = true;
                if (GameCommon.bIsWindowOpen("UPGRADE_WINDOW"))
                    break;
                if (times > 0)
                {
                    times--;
                    yield return GlobalModule.DoCoroutine(__DoAction(magictid, fragID));
                    if (flag)
                    { i++; }
                }
                else
                {
                    if (bIsChooseState)
                    {
                        int Spritcount = ConsumeItemLogicData.Self.GetDataByTid(2000014).itemNum;
                        ItemDataBase mItemDataBase = new ItemDataBase();
                        mItemDataBase.tid = 2000014;
                        mItemDataBase.itemNum = 1;
                        mItemDataBase.itemId = ConsumeItemLogicData.Self.GetDataByTid(2000014).itemId;
                        if (Spritcount > 0)
                        {
                            yield return ResourceHintNetManager.RequestUseItemToken(mItemDataBase, RESOURCE_HINT_TYPE.SPIRIT_ITEM);
                            DataCenter.SetData("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", "REFRESH", GrabTreasureReslutItemState.USEPROP);
                            times = (RoleLogicData.Self.spirit) / 2;
                            //GlobalModule.DoCoroutine (__Action (tid, times));
                            if (times > 0)
                            {
                                times--;
                                yield return GlobalModule.DoCoroutine(__DoAction(magictid, fragID));
                                if (flag)
                                { i++; }
                            }
                        }
                        else
                        {
                            DataCenter.SetData("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", "REFRESH", null);
                            GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.SPIRIT_ITEM);
                            break;
                        }
                    }
                    else
                    {
                        DataCenter.SetData("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", "REFRESH", null);
                        GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.SPIRIT_ITEM);
                        break;
                    }
                }
                
            }
        }
        if (isFirst)
        {
            if ((times > 0 && !GameCommon.bIsWindowOpen("UPGRADE_WINDOW")) || (times == 0 && !GameCommon.bIsWindowOpen("ADD_VITALITY_UP_WINDOW") && !GameCommon.bIsWindowOpen("UPGRADE_WINDOW")))
            {
                DataCenter.OnlyTipsLabelMessage("当前可以合成神器！");
                DataCenter.SetData("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", "REFRESH", GrabTreasureReslutItemState.CANCOMPOSE);
            }
        }
    }
    public override bool _DoEvent()
    {
        GrabTreasureComposeData tmpCurrComposeData = GrabTreasureWindow.GetCurrentComposeData();
        if (tmpCurrComposeData == null)
            return true;
        if (RoleLogicData.Self.spirit < 2)
        {
            //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_SPIRIT_NO_ENOUGH);
            GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.SPIRIT_ITEM);
            return true;
        }
        int times = RoleLogicData.Self.spirit / 2;
        DataCenter.OpenMessageOkWindow("自动消耗精力丹补充精力", () =>
        {
            GlobalModule.DoCoroutine(__Action(tmpCurrComposeData.TargetItem.tid, times));
        }, false, true);
        return true;
    }
    private IEnumerator __DoAction(int aimTid, int tid)
    {
        GrabTreasure_RobOneKey_Requester tmpRequester = new GrabTreasure_RobOneKey_Requester();
        tmpRequester.EquipTid = aimTid;
        tmpRequester.FragmentTid = tid;
        yield return tmpRequester.Start();
        equipName = GameCommon.GetItemName(aimTid);
        if (tmpRequester.respCode == 1)
        {
            SC_RobOneKey tmpResp = tmpRequester.respMsg;

            //减精力
            int tmpCostSpirit = 2;
            RoleLogicData.Self.AddSpirit(-tmpCostSpirit);
            if (tmpResp.succeed == 0)
            {
                flag = false;
            }
            else { flag = true; }
            //加物品
            //			for (int i = 0, count = tmpResp.allAddItems.Length; i < count; i++)
            //			{
            //				if (tmpResp.allAddItems[i].itemId == -1)
            //					PackageManager.AddItem(tmpResp.allAddItems[i]);
            //				else
            //					PackageManager.UpdateItem(tmpResp.allAddItems[i]);
            //			}
            //
            //            //awarditem itemid = -1的就要加
            //            if (tmpResp.awardItem != null && tmpResp.awardItem.itemId == -1)
            //                PackageManager.AddItem(tmpResp.awardItem);

            PackageManager.UpdateItem(tmpResp.allAddItems);
            //			RoleLogicData.Self.character.AddExp(tmpResp.exp,false);
            //			RoleLogicData.Self.AddGold(tmpResp.gold);
            DataCenter.OpenWindow("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", tid);
            DataCenter.SetData("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", "REFRESH", tmpResp);
            DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.YIJIAN_GRABTREASURE);
        }
    }
}
public class Button_mseeage_state_btn : CEvent
{
    static bool ischooseState = false;
    public override bool DoEvent()
    {
        ischooseState = !ischooseState;
        DataCenter.Set("IS_CHOOSE_STATE", ischooseState);
        return true;
    }
}
/// <summary>
/// 关闭按钮
/// </summary>
class Button_grab_fight_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GRABTREASURE_FIGHT_WINDOW");
        return true;
    }
}

class GrabFightRefreshCountdownUI : CountdownUI
{
    public static CountdownUI SetCountdown(GameObject goTarget, string lableName, long serverOverTime, CallBack callback)
    {
        GameObject lableObj = GameCommon.FindObject(goTarget, lableName);
        if (lableObj != null)
        {
            UILabel label = lableObj.GetComponent<UILabel>();
            if (label != null)
            {
                CountdownUI countDown = lableObj.AddComponent<GrabFightRefreshCountdownUI>();
                countDown.mServerOverTime = serverOverTime;
                countDown.mFinishCallBack = callback;
                return countDown;
            }
        }
        return null;
    }

    protected override string _GetTimeFormatTime(long leftUseTime)
    {
        TimeSpan tmpTimeSpan = new TimeSpan(leftUseTime * TimeSpan.TicksPerSecond);
        string tmpTime = string.Format("{0:D2}:{1:D2}", tmpTimeSpan.Minutes, tmpTimeSpan.Seconds);
        return tmpTime;
    }
}
