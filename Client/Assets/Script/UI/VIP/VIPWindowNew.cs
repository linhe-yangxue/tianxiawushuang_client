using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

//VIP窗口

//TODO UIPanel嵌套裁剪

public class VIPWindowNew : VIPWindowBase
{
    private int mCurrSelIndex = 0;
    private UICenterOnChild mCenterOnChild;

    public override void Init()
    {
        base.Init();

        EventCenter.Register("Button_vip_window_recharge_btn", new DefineFactoryLog<Button_vip_window_recharge_btn>());
        EventCenter.Register("Button_vip_gift_left_btn", new DefineFactoryLog<Button_vip_gift_left_btn>());
        EventCenter.Register("Button_vip_gift_right_btn", new DefineFactoryLog<Button_vip_gift_right_btn>());
        EventCenter.Register("Button_vip_window_get_gift_btn", new DefineFactoryLog<Button_vip_window_get_gift_btn>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "SCROLL_LEFT": __ScrollLeft(); break;
            case "SCROLL_RIGHT": __ScrollRight(); break;
        }
    }

    protected override void _OnOpen(object param)
    {
        __SetPanelDepth(mOpenData.PanelDepth + 2);
        __SetLocalPosZ(CommonParam.rechagePosZ);

        if (mCenterOnChild == null)
        {
            mCenterOnChild = GameCommon.FindComponent<UICenterOnChild>(mGameObjUI, "vip_grid");
            //TODO
            mCenterOnChild.enabled = false;
//             if (mCenterOnChild != null)
//                 mCenterOnChild.onFinished += __OnCenterOver;
        }

        NiceData tmpBtnRecharge = GameCommon.GetButtonData(mGameObjUI, "vip_window_recharge_btn");
        if (tmpBtnRecharge != null)
            tmpBtnRecharge.set("OPEN_DATA", mOpenData);

		GameObject obj = GetSub ("vip_extreme_title_label").gameObject;
		if(CommonParam.isOnLineVersion)
		{
			obj.GetComponent<UILabel>().text = "VIP特权";
		}else
		{
			obj.GetComponent<UILabel>().text = "至尊特权";
		}
    }

    protected override void PlayTweenAnim()
    {
    }

    protected override bool _OnRefresh(object param)
    {
//         UIGridContainer tmpGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "vip_grid");
//         int tmpCount = (mListVIPRecords != null) ? mListVIPRecords.Count : 0;
//         tmpGridContainer.MaxCount = tmpCount;
//         for (int i = 0; i < tmpCount; i++)
//         {
//             GameObject tmpGOItem = tmpGridContainer.controlList[i];
//             __RefreshVIPInfo(tmpGOItem, i);
//         }

        __RefreshVIPRightInfo(__GetVIPGridItem(0), mCurrSelIndex);

        return true;
    }

    /// <summary>
    /// 设置Panel深度
    /// </summary>
    /// <param name="depth"></param>
    private void __SetPanelDepth(int depth)
    {
        UIPanel tmpPanel = mGameObjUI.GetComponent<UIPanel>();
        if (tmpPanel != null)
            tmpPanel.depth = depth;

        UIPanel tmpPanelScrolLView = GetComponent<UIPanel>("vip_scrollview");
        if (tmpPanelScrolLView != null)
            tmpPanelScrolLView.depth = depth + 1;

        if (tmpPanelScrolLView != null)
        {
            GameObject obj = tmpPanelScrolLView.gameObject;
            GameObject objGroup = GameCommon.FindObject(obj, "group(Clone)_Show");
            GameObject objScrollReward = GameCommon.FindObject(objGroup, "reward_scroll");
            UIPanel rewardPanelScrolLView = objScrollReward.GetComponent<UIPanel>();
            if (rewardPanelScrolLView != null)
                rewardPanelScrolLView.depth = depth + 2;

            GameObject objScrollVip = GameCommon.FindObject(objGroup, "vip_label_scroll");
            UIPanel vipPanelScrolLView = objScrollVip.GetComponent<UIPanel>();
            if (vipPanelScrolLView != null)
                vipPanelScrolLView.depth = depth + 2;

            GameCommon.FindObject(objGroup, "Sprite_tequan").GetComponent<UISprite>().depth = depth + 3;
        }
    }

    private void __SetLocalPosZ(int posZ)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.transform.localPosition = new Vector3(0, 0, posZ);
        }
    }

    /// <summary>
    /// 获取指定的VIP特权网格
    /// </summary>
    /// <param name="gridIdx"></param>
    /// <returns></returns>
    private GameObject __GetVIPGridItem(int gridIdx)
    {
        UIGridContainer tmpGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "vip_grid");
        if (gridIdx < 0 || gridIdx >= tmpGridContainer.MaxCount)
            return null;
        return tmpGridContainer.controlList[gridIdx];
    }
    /// <summary>
    /// 刷新指定VIP特权网格信息
    /// </summary>
    /// <param name="goGridItem"></param>
    /// <param name="vipIdx"></param>
    private void __RefreshVIPRightInfo(GameObject goGridItem, int vipIdx)
    {
        if (goGridItem == null)
            return;
        if (mListVIPRecords == null || vipIdx < 0 || vipIdx >= mListVIPRecords.Count)
            return;

        DataRecord tmpRecord = mListVIPRecords[vipIdx];
        int tmpVIPLevel = (int)tmpRecord.getObject("INDEX");
        bool tmpIsStandard = VIPHelper.IsReachStandard(tmpVIPLevel);

        //标题
		if(CommonParam.isOnLineVersion)
		{
			GameCommon.SetUIText(goGridItem, "vip_level", "VIP" + tmpVIPLevel.ToString());
			GameCommon.SetUIText(goGridItem, "title_label", "等级特权");
		}else
		{
			GameCommon.SetUIText(goGridItem, "vip_level", tmpVIPLevel.ToString() + "级");
			GameCommon.SetUIText(goGridItem, "vip_title_label", "至尊特权");
		}       

        //前往充值礼包按钮
        GameCommon.SetUIVisiable(goGridItem, "vip_window_get_gift_btn", tmpIsStandard);
        if (tmpIsStandard)
        {
            NiceData tmpBtnRecharge = GameCommon.GetButtonData(goGridItem, "vip_window_get_gift_btn");
            if (tmpBtnRecharge != null)
                tmpBtnRecharge.set("VIP_LEVEL", tmpVIPLevel);
        }

        //奖励显示
        UIGridContainer tmpAwardGridContainer = GameCommon.FindComponent<UIGridContainer>(goGridItem, "reward_grid");
        int tmpGiftGroupID = (int)tmpRecord.getObject("GIFT");
        List<ItemDataBase> tmpGiftItems = GameCommon.GetItemGroup(tmpGiftGroupID, true);
        int tmpGiftCount = tmpGiftItems.Count;
        tmpAwardGridContainer.MaxCount = tmpGiftCount;
        for (int j = 0; j < tmpGiftCount; j++)
        {
            GameObject tmpGOAwardItem = tmpAwardGridContainer.controlList[j];
            GameCommon.SetOnlyItemIcon(tmpGOAwardItem, "itemIcon", tmpGiftItems[j].tid);
            GameCommon.SetOnlyItemCount(tmpGOAwardItem, "itemCount", tmpGiftItems[j].itemNum);
			int iTid = tmpGiftItems[j].tid;
			AddButtonAction (GameCommon.FindObject (tmpGOAwardItem, "itemIcon").gameObject, () =>
            {
                GameCommon.SetDataByZoneUid("vip_window_new_from", "YES");
                GameCommon.SetItemDetailsWindow(iTid);
            });
        }
        UIScrollView tmpAwardScrollView = tmpAwardGridContainer.transform.parent.GetComponent<UIScrollView>();
        if (tmpAwardScrollView != null)
            tmpAwardScrollView.ResetPosition();

        //特权展示
        //TODO
        string tmpRightDesc = tmpRecord.getData("VIPDESC");
        tmpRightDesc = tmpRightDesc.Replace("\\n", "\n");
        GameCommon.SetUIText(goGridItem, "vip_right_label", tmpRightDesc);
        UIScrollView tmpVIPRightScrollView = GameCommon.FindComponent<UIScrollView>(goGridItem, "vip_label_scroll");
        if (tmpVIPRightScrollView != null)
            tmpVIPRightScrollView.ResetPosition();
    }

    private void __ScrollLeft()
    {
        __ScrollTo(mCurrSelIndex - 1);
    }
    private void __ScrollRight()
    {
        __ScrollTo(mCurrSelIndex + 1);
    }
    private void __ScrollTo(int targetIdx)
    {
        if (mListVIPRecords == null || targetIdx < 0 || targetIdx >= mListVIPRecords.Count)
            return;

//         if (mCenterOnChild == null)
//             return;

//         GameObject tmpGOGrid = GameCommon.FindObject(mGameObjUI, "vip_grid");
//         GameObject tmpGOCenterObj = GameCommon.FindObject(tmpGOGrid, "group(Clone)_" + targetIdx.ToString());
//         if (tmpGOCenterObj == null || !tmpGOCenterObj.activeSelf)
//             return;
//         mCenterOnChild.CenterOn(tmpGOCenterObj.transform);

        mCurrSelIndex = targetIdx;
        __RefreshVIPRightInfo(__GetVIPGridItem(0), mCurrSelIndex);
    }

    private void __OnCenterOver()
    {
        if (mCenterOnChild == null)
            return;
        GameObject tmpCenterdObj = mCenterOnChild.centeredObject;
        if (tmpCenterdObj == null)
            return;
        string tmpCenterdObjName = tmpCenterdObj.name;
        tmpCenterdObjName = tmpCenterdObjName.Substring(13);        //字符串group(Clone)_之后
        int tmpCenterdObjIdx = -1;
        if (!int.TryParse(tmpCenterdObjName, out tmpCenterdObjIdx))
            return;
        mCurrSelIndex = tmpCenterdObjIdx;
    }
}

/// <summary>
/// VIP充值
/// </summary>
class Button_vip_window_recharge_btn : CEvent
{
    public override bool _DoEvent()
    {
        RechargeContainerOpenData tmpCurrData = getObject("OPEN_DATA") as RechargeContainerOpenData;
        if (tmpCurrData != null)
        {
			Debug.Log ("打开充值界面333-----");
            tmpCurrData.Page = RECHARGE_PAGE.RECHARGE;
            RechargeContainerWindow.OpenMyself(tmpCurrData);
        }
        else
        {
            GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, CommonParam.rechageDepth);
        }

        return true;
    }
}

/// <summary>
/// 向左查看VIP特权
/// </summary>
class Button_vip_gift_left_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("VIP_WINDOW", "SCROLL_LEFT", null);

        return true;
    }
}

/// <summary>
/// 向右查看VIP特权
/// </summary>
class Button_vip_gift_right_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("VIP_WINDOW", "SCROLL_RIGHT", null);

        return true;
    }
}

/// <summary>
/// 获取VIP特权
/// </summary>
class Button_vip_window_get_gift_btn : CEvent
{
    public override bool _DoEvent()
    {
        GlobalModule.ClearAllWindow();
        DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.CHARACTER);
        int tmpVIPLevel = (int)getObject("VIP_LEVEL");
        DataCenter.SetData("SHOP_WINDOW", "GO_TO_VIP", tmpVIPLevel);

//         //延迟播放
//         GlobalModule.DoCoroutine(__ShowSpeciedVIP());

        return true;
    }

    /// <summary>
    /// 显示指定的VIP特权礼包
    /// </summary>
    /// <returns></returns>
    private IEnumerator __ShowSpeciedVIP()
    {
        yield return new WaitForEndOfFrame();
        DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.CHARACTER);
        int tmpVIPLevel = (int)getObject("VIP_LEVEL");
        DataCenter.SetData("SHOP_WINDOW", "GO_TO_VIP", tmpVIPLevel);
    }
}

public enum VIP_CONFIG_FIELD
{
    DESC,                           //VIP描述

    SHOP_STAMINA_DAN,               //体力丹
    SHOP_SPIRIT_DAN,                //精力丹
    SHOP_BEATDEMONCARD,             //征讨令
    SHOP_EXP_EGG,                   //经验蛋
    SHOP_GOLD,                      //铜钱
    SHOP_ORANGE_EQUIP,              //橙色装备箱子
    SHOP_ORANGE_MAGIC,              //橙色法器箱子

    UNION_SACRIFICE,                //高级祭祀

    EQUIP_STRENGTH_PROBABILITY,     //装备强化概率
    EQUIP_STRENGTH_CRITICAL_LV,     //装备强化增加等级

    DAILY_TASK_RESET_COUNT,         //重置次数
    DAILY_TASK_DESTINY_STONE,       //天命石
    DAILY_TASK_EQUIP_REFINE_STONE,  //装备精炼石
    DAILY_TASK_BREAK_STONE,         //装备突破石

    FAIRYLAND_RIOT_COUNT,           //好友暴动次数
    FAIRYLAND_20MINUTES_INCOME,     //20分钟收益
    FAIRYLAND_10MINUTES_INCOME,     //10分钟收益

    RAMMBOCK_RESET_COUNT,           //每日重置次数

    BLACK_SHOP_REFRESH_COUNT,       //每日可刷新次数

    AWAKEN_SHOP_REFRESH_COUNT,      //刷新次数

    BAG_MAX_PET,                    //符灵背包最大值
    BAG_MAX_EQUIP,                  //装备背包最大值
    BAG_MAX_MAGIC                   //法器背包最大值
}
