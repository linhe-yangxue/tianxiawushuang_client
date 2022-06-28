using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using Utilities;

public class GrabTreasureFiveBattleResultWindow : tWindow
{
    private int mFragId;                //要夺得的碎片Id
    public UIGridContainer grid;
    public static int count = 0;
    public int grabTreasureCount = 0;
    public static int grabTreasureCountTemp = 0;

    // By XiaoWen
    // Begin
    // 是否夺取成功
    public static bool mIsSuccessGrab = false;
    // End
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_grab_list_close_button", new DefineFactoryLog<Button_grab_list_close_button>());
        EventCenter.Self.RegisterEvent("Button_grab_key_rod_finish", new DefineFactoryLog<Button_grab_list_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);
        grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "grab_fight_grid");
        if (grid != null)
        {
            grid.MaxCount = count;
            grabTreasureCount = grabTreasureCountTemp;
            //Debug.Log("=========================>grid.MaxCount" + grid.MaxCount + "      grabTreasureCount" + grabTreasureCount);
        }
        mFragId = (int)param;
        OnOpen();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        UIScrollView _resultScrollView = GameCommon.FindObject(mGameObjUI, "grab_fight_scrollview").GetComponent<UIScrollView>();
        _resultScrollView.ResetPosition();
    }

    public override bool Refresh(object param)
    {
        if (grid == null)
            return true;
        if (param is SC_Rob5Times)
        {
            SC_Rob5Times tmpResp = param as SC_Rob5Times;
            int tmpSuccessCount = tmpResp.SuccessGrabCount;
            int tmpMaxCount = tmpResp.MaxGrabCount;
            grid.MaxCount = tmpMaxCount;
            for (int i = 0; i < tmpMaxCount; i++)
            {
                GameObject tmpGOItem = grid.controlList[i];
                int tmpGold = tmpResp.golds[i];
                int tmpExp = tmpResp.exps[i];

                //第几次
                GameCommon.SetUIVisiable(tmpGOItem, "time_label", true);
                GameCommon.SetUIText(tmpGOItem, "time_label", "第" + (i + 1).ToString() + "次");

                //是否抢到
                GameCommon.SetUIVisiable(tmpGOItem, "tips_label01", i + 1 != tmpSuccessCount);
                GameCommon.SetUIVisiable(tmpGOItem, "tips_label02", i + 1 == tmpSuccessCount);
                //             if (i + 1 == tmpSuccessCount)
                //                 GameCommon.SetUIText(tmpGOItem, "fight_label", GameCommon.GetItemName(mFragId));
                if (i + 1 == tmpSuccessCount)
                {
                    // By XiaoWen
                    // Begin
                    mIsSuccessGrab = true;
                    // End
                    GameCommon.SetUIText(tmpGOItem, "fight_label", GameCommon.GetItemName(mFragId));
                    GameCommon.FindObject(tmpGOItem, "fight_label").GetComponent<UILabel>().color = GameCommon.GetMagicFragColor(mFragId);
                }

                //金币
                GameCommon.SetUIText(GameCommon.FindObject(tmpGOItem, "get_coin"), "num_label", tmpGold.ToString());

                //经验
                GameCommon.SetUIText(GameCommon.FindObject(tmpGOItem, "get_exp"), "num_label", tmpExp.ToString());

                //获得标志
                //            GameCommon.SetUIVisiable(tmpGOItem, "get_grab", i + 1 == tmpSuccessCount);

                //对号显示
                //UISprite tmpTimeBG = GameCommon.FindComponent<UISprite>(tmpGOItem, "time_bg");
                //tmpTimeBG.enabled = (i + 1 == tmpSuccessCount);

                //图标
                GameObject tmpGOIcon = GameCommon.FindObject(tmpGOItem, "item_icon");
                //            tmpGOIcon.gameObject.SetActive(i + 1 == tmpSuccessCount);
                if (tmpResp.awardItems != null && tmpResp.awardItems.Length > i)
                {
                    int _tid = tmpResp.awardItems[i].tid;
                    GameCommon.SetOnlyItemIcon(tmpGOIcon, _tid);
                    GameCommon.BindItemDescriptionEvent(tmpGOIcon, _tid);
                    GameCommon.SetUIText(tmpGOIcon, "num_label", "x" + tmpResp.awardItems[i].itemNum.ToString());
                    UIScrollView _resultScrollView = GameCommon.FindObject(mGameObjUI, "grab_fight_scrollview").GetComponent<UIScrollView>();
                    _resultScrollView.ResetPosition();
                }
            }

            //show in turn
            DoCoroutine(ShowItems());

            return true;
        }
        else if (param is SC_RobOneKey)
        {
            SC_RobOneKey mRob = param as SC_RobOneKey;
            __OnReceive(mRob, false, false);
            return true;
        }
        else if (param is GrabTreasureReslutItemState)
        {
            if ((GrabTreasureReslutItemState)param == GrabTreasureReslutItemState.CANCOMPOSE)
            {
                __OnReceive(null, false, true);
            }
            else if ((GrabTreasureReslutItemState)param == GrabTreasureReslutItemState.USEPROP)
            {
                __OnReceive(null, true, false);
            }
            return true;
        }
        else
        {
            GlobalModule.DoCoroutine(UpdataGrid());
            return true;
        }
    }

    private IEnumerator ShowItems()
    {
        yield return this.StartCoroutine(ShowItemsInTurn(grid));
    }

    private IEnumerator ShowItemsInTurn(UIGridContainer container)
    {
        foreach (var item in container.controlList)
        {
            item.SetActive(false);
        }

        int index = 0;
        foreach (var item in container.controlList)
        {
            index++;
            yield return new WaitForSeconds(0.2f);
            item.SetActive(true);
            container.Reposition();

            if (index >= 3)
            {
                UIScrollView uiScrollView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "grab_fight_scrollview");
                DoLater(() =>
                {
                    uiScrollView.SetDragAmount(0, 1.0f, false);
                }, 0.1f);
            }
        }
    }

    private void __OnReceive(SC_RobOneKey ret, bool isPropTip, bool isEndTip)
    {
        grid.MaxCount += 1;
        count = grid.MaxCount;
        //Debug.Log("===========================>SC_RobOneKeygrid.MaxCount" + grid.MaxCount);
        GameObject tmpGOItem = grid.controlList[grid.MaxCount - 1];
        //TODO	显示代码
        if (ret != null && !isPropTip && !isEndTip)
        {
            grabTreasureCount += 1;
            grabTreasureCountTemp = grabTreasureCount;
            GameCommon.SetUIVisiable(tmpGOItem, "prop_tip", false);
            GameCommon.SetUIVisiable(tmpGOItem, "label_bg", true);
            GameCommon.SetUIVisiable(tmpGOItem, "item_icon", true);
            GameCommon.SetUIVisiable(tmpGOItem, "can_compose_tip", false);

            GameCommon.SetUIVisiable(tmpGOItem, "time_label", true);
            GameCommon.SetUIText(tmpGOItem, "time_label", "第" + grabTreasureCount.ToString() + "次");
            if (ret.succeed != 0)
            {
                GameCommon.SetUIVisiable(tmpGOItem, "tips_label02", true);
                GameCommon.SetUIVisiable(tmpGOItem, "tips_label01", false);
            }
            else
            {
                GameCommon.SetUIVisiable(tmpGOItem, "tips_label02", false);
                GameCommon.SetUIVisiable(tmpGOItem, "tips_label01", true);
            }
            GameCommon.SetUIText(tmpGOItem, "fight_label", GameCommon.GetItemName(mFragId));
            GameCommon.FindObject(tmpGOItem, "fight_label").GetComponent<UILabel>().color = GameCommon.GetMagicFragColor(mFragId);

            GameCommon.SetUIText(GameCommon.FindObject(tmpGOItem, "get_coin"), "num_label", ret.gold.ToString());
            GameCommon.SetUIText(GameCommon.FindObject(tmpGOItem, "get_exp"), "num_label", ret.exp.ToString());
            GameObject tmpGOIcon = GameCommon.FindObject(tmpGOItem, "item_icon");
            if (tmpGOIcon != null)
            {
                int _tid = ret.awardItem.tid;
                GameCommon.SetOnlyItemIcon(tmpGOIcon, ret.awardItem.tid);
                GameCommon.BindItemDescriptionEvent(tmpGOIcon, _tid);
                GameCommon.SetUIText(tmpGOIcon, "num_label", "x" + ret.awardItem.itemNum.ToString());
            }
        }
        else if (ret == null && isPropTip && !isEndTip)
        {
            GameCommon.SetUIVisiable(tmpGOItem, "prop_tip", true);
            GameCommon.SetUIVisiable(tmpGOItem, "label_bg", false);
            GameCommon.SetUIVisiable(tmpGOItem, "item_icon", false);
            GameCommon.SetUIVisiable(tmpGOItem, "can_compose_tip", false);
        }
        else if (ret == null && !isPropTip && isEndTip)
        {
            GameCommon.SetUIVisiable(tmpGOItem, "prop_tip", false);
            GameCommon.SetUIVisiable(tmpGOItem, "label_bg", false);
            GameCommon.SetUIVisiable(tmpGOItem, "item_icon", false);
            GameCommon.SetUIVisiable(tmpGOItem, "can_compose_tip", true);

            string canComposeTip = "恭喜你抢夺到了{0}的全部碎片";
            canComposeTip = string.Format(canComposeTip, Button_grab_key_rod_btn.equipName);
            GameCommon.SetUIText(tmpGOItem, "can_compose_tip", canComposeTip);
        }

        GlobalModule.DoCoroutine(UpdataGrid());

        if (count > 3)
        {
            this.DoCoroutine(ChangeScrollValue());
        }

        int currSpritCount = ConsumeItemLogicData.Self.GetDataByTid(2000014).itemNum;
        GameCommon.SetUIText(mGameObjUI, "curr_sprit_num", "×" + currSpritCount.ToString());
    }

    IEnumerator UpdataGrid()
    {
        yield return null;
        grid.Reposition();
        grid.GetComponent<DynamicGridContainer>().Adjust();
    }

    private IEnumerator ChangeScrollValue()
    {
        UIScrollView tmpResultScrollView = GameCommon.FindObject(mGameObjUI, "grab_fight_scrollview").GetComponent<UIScrollView>();
        tmpResultScrollView.ResetPosition();

        yield return null;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.2f);
        tmpResultScrollView.SetDragAmount(0.0f, 1.0f, false);
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_grab_list_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW");

        // By XiaoWen
        // Begin
        if (GrabTreasureFiveBattleResultWindow.mIsSuccessGrab)
        {
            DataCenter.CloseWindow("GRABTREASURE_FIGHT_WINDOW");
        }
        // End

        DataCenter.SetData("GRABTREASURE_WINDOW", "REFRESH", null);
        GrabTreasureFiveBattleResultWindow.count = 0;
        GrabTreasureFiveBattleResultWindow.grabTreasureCountTemp = 0;
        GrabTreasureFiveBattleResultWindow.mIsSuccessGrab = false;
        return true;
    }
}
