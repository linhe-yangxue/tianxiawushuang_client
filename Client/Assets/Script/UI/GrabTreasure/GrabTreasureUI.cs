using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

/// <summary>
/// 夺宝主界面
/// </summary>


//物品品质，从白色（值为1）开始，白、绿、蓝、紫、橙、红，对应EQUIP_QUALITY_TYPE中LOW、MIDDLE、GOOD、BETTER、BEST、PERFECT

/// <summary>
/// 夺宝物品数据
/// </summary>
public class GrabTreasureItemData : ItemDataBase
{
    private int mQuality;           //物品品质

    public int Quality
    {
        set { mQuality = value; }
        get { return mQuality; }
    }
}

public class GrabTreasureComposeData
{
    private ItemDataBase mTargetItem;
    private List<ItemDataBase> mListItems = new List<ItemDataBase>();

    public ItemDataBase TargetItem
    {
        set { mTargetItem = value; }
        get { return mTargetItem; }
    }

    public List<ItemDataBase> ListItems
    {
        set { mListItems = value; }
        get { return mListItems; }
    }
}

public class GrabTreasureWindow : tWindow
{
    public static string smMagicPrefix = "15";      //法器前缀标示
	public int needindex = 2202;
    private static Color[] smMagicColor = new Color[] {
        Color.white,
        Color.green,
        Color.blue,
        new Color(0.4f, 0.0f, 1.0f),
        new Color(1.0f, 0.68f, 0.0f),
        Color.red
    };

    private List<GrabTreasureItemData> mListBaseMagic = new List<GrabTreasureItemData>();      //基础法器列表备份
    private EquipData mCurrSelMagicData;            //当前选择法器数据
    private int mCurrSelIndex = -1;                 //当前选择法器在列表中索引，-1表示没选中任何法器
    private bool mIsAddFinishedAction = false;      //是否添加了滑动结束回调

    private CountdownUI mPeaceLeftTimeCount;        //免战倒计时控件
    private long mPeaceOverTime = 0;                //免战结束时间
    private bool mIsPeace = false;                  //是否免战中

    private UIGridContainer mLeftGridContainer;     //左侧法器列表
    private UIScrollView mLeftGridScrollView;       //左侧法器滑动区域
    private UIGridContainer mMiddleGridContainer;   //中间法器碎片列表
    private float mLeftScrollSpeed = 0.1f;          //左侧列表滚动速度

    private List<GrabTreasureItemData> mListMagicData = new List<GrabTreasureItemData>();         //法器列表，已排好序

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_grab_window_left_arrow_up", new DefineFactoryLog<Button_grab_window_left_arrow_up>());
        EventCenter.Self.RegisterEvent("Button_grab_window_left_arrow_down", new DefineFactoryLog<Button_grab_window_left_arrow_down>());
        EventCenter.Self.RegisterEvent("Button_grab_window_center_left_arrow", new DefineFactoryLog<Button_grab_window_center_left_arrow>());
        EventCenter.Self.RegisterEvent("Button_grab_window_center_right_arrow", new DefineFactoryLog<Button_grab_window_center_right_arrow>());
        EventCenter.Self.RegisterEvent("Button_grab_peace_btn", new DefineFactoryLog<Button_GrabTreasure_grab_peace_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_record_btn", new DefineFactoryLog<Button_GrabTreasure_grab_record_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_window_center_magic_btn", new DefineFactoryLog<Button_grab_window_center_magic_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_window_item_icon_btn", new DefineFactoryLog<Button_grab_window_item_icon_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_window_piece_btn", new DefineFactoryLog<Button_grab_window_piece_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_window_compose_btn", new DefineFactoryLog<Button_grab_window_compose_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_window_back_btn", new DefineFactoryLog<Button_grab_window_back_btn>());

        __InitBlueMagicList();

        /*
        //TODO 加碎片伪数据
        PackageManager.AddItem(new ItemDataBase() { tid = 17101, itemNum = 10, itemId = -1 });
        PackageManager.AddItem(new ItemDataBase() { tid = 17103, itemNum = 10, itemId = -1 });
        PackageManager.AddItem(new ItemDataBase() { tid = 17005, itemNum = 10, itemId = -1 });
         * */
    }

    public override void Open(object param)
    {
        base.Open(param);

        DataCenter.OpenWindow("GRABTREASURE_BACK_WINDOW");
       

        bool bFirstOpen = false;
//        if (!mIsAddFinishedAction)        因为进入战斗场景后GameObject被删除，所以每次进入窗口都加载
        {
            bFirstOpen = true;
            mIsAddFinishedAction = true;

            mLeftGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "magic_grid");
            mLeftGridScrollView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "magic_scrollview");
            mMiddleGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "piece_grid");

            GameCommon.SetUIVisiable(mGameObjUI,"grab_key_rod_btn",GameCommon.IsFuncCanShowByFuncID(needindex));

            //添加居中后回调
            UICenterOnChild tmpCenter = GameCommon.FindComponent<UICenterOnChild>(mGameObjUI, "piece_grid");
            if (mMiddleGridContainer != null && tmpCenter != null)
            {
                if (tmpCenter.onFinished != null)
                    tmpCenter.onFinished -= __OnSweepFragmentFinished;
                tmpCenter.onFinished += __OnSweepFragmentFinished;
            }
        }

        Refresh(null);
        if (bFirstOpen)
        {
            __RefreshPeaceTime(0);
            GlobalModule.DoCoroutine(GrabTreasure_GetTruceTime_Requester.StartRequester());
        }
        else
            __RefreshPeaceTimeFromData();
        if (mCurrSelIndex == -1 || mCurrSelIndex >= mListMagicData.Count)
        {
            __ToggleMagic(0);
            __ChangeToMagic(0, true);
        }
    }

    public static void LogError(string log)
    {
        DEBUG.LogError("GrabTreasure - " + log);
    }
    public static void Log(string log)
    {
        DEBUG.Log("GrabTreasure - " + log);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "CHANGE_TO":
                {
                    __ChangeToMagic((int)objVal, true);
                } break;
            case "GO_TO_FRAGMENT":
                {
                    //转向指定碎片位置
                    int tmpTargetFragId = (int)objVal;
                    __GoToFragment(tmpTargetFragId);
                } break;
            case "REFRESH_PEACE_TIME":
                {
                    long tmpPeaceTime = (long)objVal;
                    __RefreshPeaceTime(tmpPeaceTime);
                } break;
            case "RESET_PEACE_TIME":
                {
                    long tmpPeaceTime = (long)objVal;
                    __ResetPeaceTime(tmpPeaceTime);
                } break;
            case "ADD_PEACE_TIME":
                {
                    long tmpPeaceTime = (long)objVal;
                    __AddPeaceTime(tmpPeaceTime);
                } break;
            case "CHANGE_TO_FIRST_COMBINE":
                {
                    //跳转到第一个可以合成的法器处
                    if (mListMagicData == null)
                        mListMagicData = GetExistMagicList();
                    int tmpFirstIdx = -1;
                    for (int i = 0, count = mListMagicData.Count; i < count; i++)
                    {
                        if (CanCompose(i))
                        {
                            tmpFirstIdx = i;
                            break;
                        }
                    }
                    if (tmpFirstIdx != -1)
                        set("CHANGE_TO", tmpFirstIdx);
                }break;
		case "GRAB_COMPOSE_EFFECT":
			int iIndex = (int)objVal;
			GameObject obj = GameCommon.FindObject (mGameObjUI, "fragment_compound_effect_" + iIndex).gameObject;
//			GameObject grabEffectObj = GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UIEffect/ui_tianming_effect", GameCommon.FindObject (mGameObjUI, obj));
//			GlobalModule.DoLater (() => GameObject.DestroyImmediate(grabEffectObj), 1.0f);
			obj.SetActive (true);
			GlobalModule.DoLater (() => obj.SetActive (false), 2.0f);
			break;
        }
    }

    public override bool Refresh(object param)
    {
        __RefreshMagicList();
        __RefreshMagicFragmentList();
        mCurrSelIndex = Mathf.Min(mCurrSelIndex, mListMagicData.Count - 1);
        __ChangeToMagic(mCurrSelIndex, true);

        //为了使中间全部元素的ScrollView不遮挡元素点击，暂时做下面处理，仍然有问题，结算返回到此窗口时仍然被挡住
        GameObject tmpPeaceGroup = GetSub("piece_group");
        if (false && tmpPeaceGroup != null)
        {
            BoxCollider tmpBoxCollider = tmpPeaceGroup.GetComponent<BoxCollider>();
            tmpBoxCollider.enabled = false;
            tmpBoxCollider.enabled = true;
        }

        return true;
    }

    public override void OnClose()
    {
        DataCenter.CloseWindow("GRABTREASURE_BACK_WINDOW");

        base.OnClose();

        //刷新快捷入口界面
        DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);
    }

    private MaxShowTimeCountdownUI __SetCountdown(GameObject subObject, string lableName, long serverOverTime, CallBack callBack)
    {
        GameObject lableObj = GameCommon.FindObject(subObject, lableName);
        if (lableObj != null)
        {
            UILabel label = lableObj.GetComponent<UILabel>();
            if (label != null)
            {
                GrabMaxShowTimeCountdownUI countDown = lableObj.AddComponent<GrabMaxShowTimeCountdownUI>();
                countDown.IsUseDaysTime = false;
                countDown.Hours = 100;
                countDown.mServerOverTime = serverOverTime;
                countDown.mFinishCallBack = callBack;
                return countDown;
            }
        }
        return null;
    }

//     // <summary>
//     /// 直接设置免战结束时间
//     /// </summary>
//     /// <param name="peaceTime"></param>
//     private void __RefreshPeaceTime(long peaceTime)
//     {
//         long tmpNowTime = CommonParam.NowServerTime();
//         if (mPeaceLeftTimeCount == null)
//             mPeaceLeftTimeCount = __SetCountdown(GetSub("grab_peace_btn"), "number_label", peaceTime, new CallBack(this, "OnPeaceTimeOver", null));
//         else
//             mPeaceLeftTimeCount.mServerOverTime = peaceTime;
//         if (mPeaceLeftTimeCount != null)
//         {
//             mIsPeace = mPeaceLeftTimeCount.mServerOverTime > tmpNowTime;
//             mPeaceLeftTimeCount.enabled = mIsPeace;
//         }
//         GameCommon.SetUIVisiable(GetSub("grab_peace_btn"), "number_label", mIsPeace);
//     }
//     /// <summary>
//     /// 以当前时间为起点，将免战时间向后加peaceTime秒
//     /// </summary>
//     /// <param name="peaceTime"></param>
//     private void __ResetPeaceTime(long peaceTime)
//     {
//         long tmpNowTime = CommonParam.NowServerTime();
//         if (mPeaceLeftTimeCount == null)
//             mPeaceLeftTimeCount = __SetCountdown(GetSub("grab_peace_btn"), "number_label", peaceTime, new CallBack(this, "OnPeaceTimeOver", null));
//         if (mPeaceLeftTimeCount != null)
//             __RefreshPeaceTime(tmpNowTime + peaceTime);
//     }
//     /// <summary>
//     /// 以当前免战时间为准，将免战时间向后加peaceTime秒
//     /// </summary>
//     /// <param name="peaceTime"></param>
//     private void __AddPeaceTime(long peaceTime)
//     {
//         long tmpNowTime = CommonParam.NowServerTime();
//         if (mPeaceLeftTimeCount == null)
//             mPeaceLeftTimeCount = __SetCountdown(GetSub("grab_peace_btn"), "number_label", peaceTime, new CallBack(this, "OnPeaceTimeOver", null));
//         if (mPeaceLeftTimeCount != null && !mIsPeace)
//         {
//             __RefreshPeaceTime(tmpNowTime + peaceTime);
//             return;
//         }
//         mPeaceLeftTimeCount.mServerOverTime += peaceTime;
//         if (mPeaceLeftTimeCount != null)
//         {
//             mIsPeace = mPeaceLeftTimeCount.mServerOverTime > tmpNowTime;
//             mPeaceLeftTimeCount.enabled = mIsPeace;
//         }
//         GameCommon.SetUIVisiable(GetSub("grab_peace_btn"), "number_label", mIsPeace);
//     }
//     /// <summary>
//     /// 免战时间结束
//     /// </summary>
//     /// <param name="param"></param>
//     public void OnPeaceTimeOver(object param)
//     {
//         mIsPeace = false;
//     }

    // <summary>
    /// 直接设置免战结束时间
    /// </summary>
    /// <param name="peaceTime"></param>
    private void __RefreshPeaceTime(long peaceTime)
    {
        mPeaceOverTime = peaceTime;
        __RefreshPeaceTimeFromData();
    }
    /// <summary>
    /// 以当前时间为起点，将免战时间向后加peaceTime秒
    /// </summary>
    /// <param name="peaceTime"></param>
    private void __ResetPeaceTime(long peaceTime)
    {
        long tmpNowTime = CommonParam.NowServerTime();
        __RefreshPeaceTime(tmpNowTime + peaceTime);
    }
    /// <summary>
    /// 以当前免战时间为准，将免战时间向后加peaceTime秒
    /// </summary>
    /// <param name="peaceTime"></param>
    private void __AddPeaceTime(long peaceTime)
    {
        long tmpNowTime = CommonParam.NowServerTime();
        if (!mIsPeace)
        {
            __RefreshPeaceTime(tmpNowTime + peaceTime);
            return;
        }
        mPeaceOverTime += peaceTime;
        __RefreshPeaceTimeFromData();
    }
    private void __RefreshPeaceTimeFromData()
    {
        if (mGameObjUI == null)
            return;

        long tmpNowTime = CommonParam.NowServerTime();
        mIsPeace = mPeaceOverTime >= tmpNowTime;

        if (mPeaceLeftTimeCount == null)
            mPeaceLeftTimeCount = __SetCountdown(GetSub("grab_peace_btn"), "number_label", mPeaceOverTime, new CallBack(this, "OnPeaceTimeOver", null));
        else
            mPeaceLeftTimeCount.mServerOverTime = mPeaceOverTime;
        if (mPeaceLeftTimeCount != null)
            mPeaceLeftTimeCount.enabled = mIsPeace;
        GameCommon.SetUIVisiable(GetSub("grab_peace_btn"), "number_label", mIsPeace);
    }
    /// <summary>
    /// 免战时间结束
    /// </summary>
    /// <param name="param"></param>
    public void OnPeaceTimeOver(object param)
    {
        mIsPeace = false;
    }

    public static bool IsPeace
    {
        get
        {
            GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
            if (tmpWin == null)
                return false;
            //为防止disable时Update不执行，检查一次是否过了免战时间
//             if (!tmpWin.mGameObjUI.activeSelf)
//             {
//                 long tmpNowServerTime = CommonParam.NowServerTime();
//                 if (tmpWin.mPeaceLeftTimeCount == null)
//                 {
//                     GameObject tmpGONumLabel = GameCommon.FindObject(tmpWin.GetSub("grab_peace_btn"), "number_label");
//                     if (tmpGONumLabel != null)
//                     {
//                         UILabel tmpLabel = tmpGONumLabel.GetComponent<UILabel>();
//                         if (tmpLabel != null)
//                             tmpWin.mPeaceLeftTimeCount = tmpGONumLabel.GetComponent<CountdownUI>();
//                     }
//                 }
//                 if (tmpWin.mPeaceLeftTimeCount != null)
//                     tmpWin.mIsPeace = (tmpWin.mPeaceLeftTimeCount.mServerOverTime >= tmpNowServerTime);
//             }
            long tmpNowServerTime = CommonParam.NowServerTime();
            tmpWin.mIsPeace = (tmpWin.mPeaceOverTime >= tmpNowServerTime);
            return tmpWin.mIsPeace;
        }
    }
    /// <summary>
    /// 剩余免战时间
    /// </summary>
    /// <returns></returns>
    public static string LeftPeaceTime()
    {
        GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        if (tmpWin == null)
            return "";
        UILabel tmpLabel = GameCommon.FindComponent<UILabel>(tmpWin.GetSub("grab_peace_btn"), "number_label");
        return tmpLabel.text;
    }

    public static GrabTreasureComposeData GetCurrentComposeData()
    {
        GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        if (tmpWin == null)
            return null;

        GrabTreasureComposeData tmpComposeData = new GrabTreasureComposeData();
        if (tmpWin.mListMagicData == null || tmpWin.mListMagicData.Count <= 0)
            return null;
        if (tmpWin.mCurrSelIndex < 0 || tmpWin.mCurrSelIndex >= tmpWin.mListMagicData.Count)
            return null;
        tmpComposeData.TargetItem = tmpWin.mListMagicData[tmpWin.mCurrSelIndex];
        DataRecord tmpComposeConfig = DataCenter.mEquipComposeConfig.GetRecord(tmpComposeData.TargetItem.tid);
        if (tmpComposeConfig == null)
        {
            LogError("找不到合成目标记录" + tmpComposeData.TargetItem.tid.ToString());
            return null;
        }
        int tmpComposeNum = (int)tmpComposeConfig.getObject("NUM");
        for (int i = 0; i < tmpComposeNum; i++)
        {
            int tmpFragId = (int)tmpComposeConfig.getObject("FRAGMENT_" + (i + 1).ToString());
            EquipFragmentData tmpFragData = null;
            if (!MagicFragmentLogicData.Self.mDicEquipFragmentData.TryGetValue(tmpFragId, out tmpFragData))
            {
                //                Log("背包里找不到" + tmpFragId.ToString() + "碎片");
                break;
            }
            tmpComposeData.ListItems.Add(new ItemDataBase() { tid = tmpFragData.tid, itemId = tmpFragData.itemId, itemNum = tmpFragData.itemNum });
        }
        return tmpComposeData;
    }

    /// <summary>
    /// 左侧列表向上滑动
    /// </summary>
    public void LeftScrollUp()
    {
//         UIScrollView tmpScrollView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "magic_scrollview");
//         if (tmpScrollView == null)
//             return;
//         tmpScrollView.Scroll(-mLeftScrollSpeed);
        __ToggleMagic(mCurrSelIndex - 1);
        __ChangeToMagic(mCurrSelIndex - 1, true);
    }
    /// <summary>
    /// 左侧列表向下滑动
    /// </summary>
    public void LeftScrollDown()
    {
//         UIScrollView tmpScrollView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "magic_scrollview");
//         if (tmpScrollView == null)
//             return;
//         tmpScrollView.Scroll(mLeftScrollSpeed);
        __ToggleMagic(mCurrSelIndex + 1);
        __ChangeToMagic(mCurrSelIndex + 1, true);
    }

    /// <summary>
    /// 获取蓝法器列表
    /// </summary>
    private void __InitBlueMagicList()
    {
        if (mListBaseMagic == null)
            return;

        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mEquipComposeConfig.GetAllRecord())
        {
            DataRecord tmpEquipConfig = pair.Value;
            int tmpShow = (int)tmpEquipConfig.getObject("BASE_SHOW");
            if (tmpShow == 0)
                continue;
            string tmpIndex = tmpEquipConfig.getData("INDEX");
            DataRecord tmpRoleConfig = DataCenter.mRoleEquipConfig.GetRecord(tmpIndex);
            if (tmpRoleConfig == null)
                continue;
            int tmpEquipQuality = (int)tmpRoleConfig.getObject("QUALITY");
            mListBaseMagic.Add(new GrabTreasureItemData() { tid = pair.Key, Quality = tmpEquipQuality });
        }
    }

    /// <summary>
    /// 刷新法器列表
    /// </summary>
    private void __RefreshMagicList()
    {
        mListMagicData = GetExistMagicList();
        if (mLeftGridContainer == null)
            return;
        int count = mListMagicData.Count;
        mLeftGridContainer.MaxCount = count;
        for (int i = 0; i < count; i++)
        {
            GameObject goItem = mLeftGridContainer.controlList[i];
            GrabTreasureItemData tmpEquipData = mListMagicData[i];
            NiceData tmpBtnData = GameCommon.GetButtonData(goItem, "grab_window_item_icon_btn");
            tmpBtnData.set("ITEM_INDEX", i);
            tmpBtnData.set("ITEM_DATA", tmpEquipData);

            //显示图标
            GameCommon.SetItemIcon(goItem, new ItemData() { mID = tmpEquipData.tid, mType = (int)PackageManager.GetItemTypeByTableID(tmpEquipData.tid) });
            //图标颜色
            SetMagicSpriteColor(goItem, "item_icon", (EQUIP_QUALITY_TYPE)tmpEquipData.Quality);

            //added by xuke 刷新法器红点
            GameCommon.SetNewMarkVisible(mLeftGridContainer.controlList[i], CanCompose(i));
            //end
        }
        mLeftGridContainer.Reposition();
    }
    /// <summary>
    /// 获取当前可用显示的法器列表
    /// </summary>
    /// <returns>结果已排好序</returns>
    public List<GrabTreasureItemData> GetExistMagicList()
    {
        List<GrabTreasureItemData> listMagic = new List<GrabTreasureItemData>();
        Dictionary<int, GrabTreasureItemData> dicMagic = new Dictionary<int, GrabTreasureItemData>();

        //搜索所有碎片对应的法器
        foreach (KeyValuePair<int, EquipFragmentData> pair in MagicFragmentLogicData.Self.mDicEquipFragmentData)
        {
            DataRecord tmpFragConfig = DataCenter.mFragmentAdminConfig.GetRecord(pair.Value.tid);
            if (tmpFragConfig == null)
                continue;
            int tmpMagicId = (int)tmpFragConfig.getObject("ROLEEQUIPID");
            DataRecord tmpMagicConfig = DataCenter.mRoleEquipConfig.GetRecord(tmpMagicId);
            if (tmpMagicConfig == null)
                continue;
            EquipData tmpMagicData = null;
            MagicLogicData.Self.mDicEquip.TryGetValue(tmpMagicId, out tmpMagicData);
            GrabTreasureItemData tmpGrabMagicData = new GrabTreasureItemData();
            tmpGrabMagicData.tid = tmpMagicId;
            tmpGrabMagicData.itemId = (tmpMagicData != null) ? tmpMagicData.itemId : -1;
            tmpGrabMagicData.Quality = tmpMagicConfig.getData("QUALITY");
            dicMagic[tmpGrabMagicData.tid] = tmpGrabMagicData;
        }

        //加入蓝法器
        if (mListBaseMagic != null)
        {
            for (int i = 0, count = mListBaseMagic.Count; i < count; i++)
            {
                if (!dicMagic.ContainsKey(mListBaseMagic[i].tid))
                    dicMagic[mListBaseMagic[i].tid] = mListBaseMagic[i];
            }
        }

        foreach (KeyValuePair<int, GrabTreasureItemData> tmpPair in dicMagic)
            listMagic.Add(tmpPair.Value);

        __SortMagicList(listMagic);
        return listMagic;
    }
    /// <summary>
    /// 排序指定的装备列表
    /// </summary>
    private void __SortMagicList(List<GrabTreasureItemData> listMagic)
    {
        if (listMagic == null)
            return;

        listMagic.Sort((GrabTreasureItemData l, GrabTreasureItemData r) =>
        {
            //先把经验法器放在最前面
            if (PackageManager.IsExpMagic(l.tid) && !PackageManager.IsExpMagic(r.tid))
                return -1;
            else if (!PackageManager.IsExpMagic(l.tid) && PackageManager.IsExpMagic(r.tid))
                return 1;
            else
            {
                //按品质排序
                if (l.Quality > r.Quality)
                    return -1;
                else if (l.Quality == r.Quality)
                {
                    //如果品质相同，按tid排序
                    if (l.tid < r.tid)
                        return -1;
                    else if (l.tid == r.tid)
                        return 0;
                    else
                        return 1;
                }
                else
                    return 1;
            }
        });
    }

    /// <summary>
    /// 刷新法器碎片列表
    /// </summary>
    private void __RefreshMagicFragmentList()
    {
        if (mMiddleGridContainer == null)
            return;
        int count = mListMagicData.Count;
        mMiddleGridContainer.MaxCount = count;
        for (int i = 0; i < count; i++)
        {
            GameObject goItem = mMiddleGridContainer.controlList[i];
            GrabTreasureItemData tmpEquipData = mListMagicData[i];
            __ShowComposeUI(goItem, tmpEquipData);
        }
        mMiddleGridContainer.Reposition();
    }
    /// <summary>
    /// 显示组合界面类型
    /// </summary>
    /// <param name="composeUI"></param>
    /// <param name="equipData"></param>
    private void __ShowComposeUI(GameObject composeUI, GrabTreasureItemData equipData)
    {
        //显示指定GameObject
        GameObject tmpGOShow = null;
        for (int i = 0; i < 4; i++)
        {
            if (i + 3 == equipData.Quality)
            {
                tmpGOShow = GameCommon.FindObject(composeUI, "piece_info_group0" + (i + 1).ToString());
                if (tmpGOShow != null)
                    tmpGOShow.gameObject.SetActive(true);
            }
            else
                GameCommon.SetUIVisiable(composeUI, "piece_info_group0" + (i + 1).ToString(), false);
        }

        if (tmpGOShow == null)
        {
            LogError("GrabTreasure - 质量为" + equipData.Quality.ToString() + "的装备缺失合成界面");
            return;
        }

        Color tmpMagicColor = GetMagicColor((EQUIP_QUALITY_TYPE)equipData.Quality);
        string tmpMagicColorString = tmpMagicColor.ToString();

        //中间法器
        GameObject tmpGOMagic = GameCommon.FindObject(tmpGOShow, "magic_info");

		if (tmpGOMagic != null) {
			GameCommon.SetItemIcon(tmpGOMagic, new ItemData() { mID = equipData.tid, mType = (int)PackageManager.GetItemTypeByTableID(equipData.tid) });
			UILabel _magicName = GameCommon.FindComponent<UILabel>(composeUI,"item_name");
			_magicName.text = GameCommon.GetItemName(equipData.tid);
            _magicName.color = GameCommon.GetNameColor(equipData.tid);
		}
            
        SetMagicSpriteColor(tmpGOMagic, "item_icon", tmpMagicColor);
        NiceData tmpBtnMagicData = GameCommon.GetButtonData(tmpGOMagic, "grab_window_center_magic_btn");
        if (tmpBtnMagicData != null)
            tmpBtnMagicData.set("TARGET_MAGIC_ID", equipData.tid);

        //碎片
        DataRecord tmpComposeConfig = DataCenter.mEquipComposeConfig.GetRecord(equipData.tid);
        if (tmpComposeConfig == null)
        {
            LogError("找不到" + equipData.tid.ToString() + "对应的合成项");
            return;
        }
        List<int> tmpListFragIndex = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            int tmpFragId = (int)tmpComposeConfig.getObject("FRAGMENT_" + (i + 1).ToString());
            if (tmpFragId != 0)
                tmpListFragIndex.Add(tmpFragId);
        }
        for (int i = 0, count = tmpListFragIndex.Count; i < count; i++)
        {
            int tmpFragId = tmpListFragIndex[i];
            DataRecord tmpFragConfig = DataCenter.mFragmentAdminConfig.GetRecord(tmpFragId);
            if (tmpFragConfig == null)
                continue;
            EquipFragmentData tmpFragData = null;
            GameObject tmpGOFrag = GameCommon.FindObject(tmpGOShow, "piece_info(Clone)_" + i.ToString());
            if (tmpGOFrag == null)
            {
                LogError(equipData.tid.ToString() + "对应的碎片数量多于界面显示");
                break;
            }
            SetMagicSpriteColor(tmpGOFrag, "item_icon", tmpMagicColor);
            int tmpFragItemNum = 0;
            GameObject tmpGOBtn = GameCommon.FindObject(tmpGOFrag, "grab_window_piece_btn");
            if (MagicFragmentLogicData.Self.mDicEquipFragmentData.TryGetValue(tmpFragId, out tmpFragData))
            {
                tmpFragItemNum = tmpFragData.itemNum;
                GameCommon.SetUIText(tmpGOFrag, "number_label", tmpFragData.itemNum.ToString());
				GameCommon.SetIcon(tmpGOFrag, "item_icon", tmpFragConfig.getData("ICON_NAME"), tmpFragConfig.getData("ICON_ATLAS"));
                //                tmpGOBtn.GetComponent<BoxCollider>().enabled = false;
                //                tmpGOBtn.GetComponent<UIButton>().isEnabled = false;
            }
            else
            {
                GameCommon.SetUIText(tmpGOFrag, "number_label", "0");
				GameCommon.SetIcon(tmpGOFrag, "item_icon", tmpFragConfig.getData("ICON_NAME") + "_i", tmpFragConfig.getData("ICON_BLACKATLAS"));
                //                tmpGOBtn.GetComponent<BoxCollider>().enabled = true;
                //                tmpGOBtn.GetComponent<UIButton>().isEnabled = true;
            }

            
            NiceData tmpBtnFragData = GameCommon.GetButtonData(tmpGOFrag, "grab_window_piece_btn");
            if (tmpBtnFragData != null)
            {
                tmpBtnFragData.set("TARGET_FRAG_ID", tmpFragId);
                tmpBtnFragData.set("TARGET_FRAG_NUMBER", tmpFragItemNum);
            }
        }
    }

    /// <summary>
    /// 改变到指定索引位置的法器
    /// </summary>
    /// <param name="dstIndex"></param>
    /// <param name="changeFrag">是否改变合成碎片居中位置</param>
    private void __ChangeToMagic(int dstIndex, bool changeFrag)
    {
        if (mLeftGridContainer == null || mLeftGridContainer.MaxCount <= 0)
            return;
        if (dstIndex < 0 || dstIndex >= mLeftGridContainer.MaxCount)
            return;

        int indexDelta = 0;
        if (mCurrSelIndex != -1)
            indexDelta = Mathf.Abs(dstIndex - mCurrSelIndex);
        mCurrSelIndex = dstIndex;
        if (changeFrag)
        {
            UICenterOnChild tmpCenter = GameCommon.FindComponent<UICenterOnChild>(mGameObjUI, "piece_grid");
            if (mMiddleGridContainer != null && tmpCenter != null && dstIndex >= 0 && dstIndex < mMiddleGridContainer.controlList.Count)
            {
                GameObject go = mMiddleGridContainer.controlList[dstIndex];
                tmpCenter.springStrength = (indexDelta == 1) ? 8 : 9999999;
                tmpCenter.CenterOn(go.transform);
                //added by xuke
                __PlayPlayTreasureFragTween();
                //end
            }
        }
        __MoveCurrMagicToShow();
    }

    #region 法器碎片动态UI
    private void __PlayPlayTreasureFragTween() 
    {
        GameObject _treasureGroup = mMiddleGridContainer.controlList[mCurrSelIndex];
        int _treasureIndex = mListMagicData[mCurrSelIndex].Quality - 2;

        GameObject _groupObj = GameCommon.FindObject(_treasureGroup, "piece_info_group0" + _treasureIndex);
        if (_groupObj == null)
            return;
        TransformTweenAnim _tween = GameCommon.FindComponent<TransformTweenAnim>(_groupObj,"transform_tween_root");
        if (_tween != null) 
        {
            _tween.PlayTweenAnim();
        }
        
    }
    #endregion
    /// <summary>
    /// 激活指定索引的法器列表元素
    /// </summary>
    /// <param name="dstIndex"></param>
    private void __ToggleMagic(int dstIndex)
    {
        if (mLeftGridContainer == null)
            return;
        if (dstIndex < 0 || dstIndex >= mLeftGridContainer.controlList.Count)
            return;
        GameObject tmpGOMagicItem = mLeftGridContainer.controlList[dstIndex];
        UIToggle tmpGOToggle = GameCommon.FindComponent<UIToggle>(tmpGOMagicItem, "grab_window_item_icon_btn");
        if (tmpGOToggle == null)
            return;
        tmpGOToggle.value = true;
    }
    /// <summary>
    /// 将当前选择的法器移动到显示区域
    /// </summary>
    private void __MoveCurrMagicToShow()
    {
        if (mLeftGridScrollView == null || mLeftGridContainer == null || mLeftGridContainer.MaxCount <= 0)
            return;

        int tmpMaxCount = mLeftGridContainer.MaxCount;
        if (mCurrSelIndex < 0 || mCurrSelIndex >= tmpMaxCount)
            return;

        UIPanel tmpLeftPanel = mLeftGridScrollView.panel;
        GameObject tmpGOCurrItem = mLeftGridContainer.controlList[mCurrSelIndex];
        if(mCurrSelIndex == 0 || mCurrSelIndex == mLeftGridContainer.MaxCount - 1)
        {
            //mLeftGridScrollView.SetDragAmount(0.0f, mCurrSelIndex / (mLeftGridContainer.MaxCount - 1), false);
            return;
        }
        //为保证所选物品能完全显示，现让上下相连物品中心点也显示
        bool isUpShow = tmpLeftPanel.IsVisible(mLeftGridContainer.controlList[mCurrSelIndex - 1].transform.position);
        bool isDownShow = tmpLeftPanel.IsVisible(mLeftGridContainer.controlList[mCurrSelIndex + 1].transform.position);
        if (!isUpShow)
            mLeftGridScrollView.SetDragAmount(0.0f, (float)(mCurrSelIndex - 1) / (float)(tmpMaxCount - 1), false);
        else if (!isDownShow)
            mLeftGridScrollView.SetDragAmount(0.0f, (float)(mCurrSelIndex + 1) / (float)(tmpMaxCount - 1), false);
        else if (!tmpLeftPanel.IsVisible(tmpGOCurrItem.transform.position))
            mLeftGridScrollView.SetDragAmount(0.0f, (float)mCurrSelIndex / (float)(tmpMaxCount - 1), false);
//         UISprite tmpItemSprite = GameCommon.FindComponent<UISprite>(tmpGOCurrItem, "item_icon");
//         Vector3[] tmpItemWorldCorner = tmpItemSprite.worldCorners;
//         Vector3[] tmpPanelSides = tmpLeftPanel.GetSides(null);
//         bool isToUp = (tmpItemWorldCorner[0].y > tmpPanelSides[0].y && mCurrSelIndex >= 5);
//         bool isToDown = (tmpItemWorldCorner[1].y < tmpPanelSides[1].y && mCurrSelIndex <= tmpMaxCount - 5);
//         float tmpToY = 0.0f;
//         if (isToUp)
//             tmpToY = (float)(mCurrSelIndex - 4) / (float)(tmpMaxCount - 1);
//         else if (isToDown)
//             tmpToY = (float)(mCurrSelIndex + 4) / (float)(tmpMaxCount - 1);
//         else if (!tmpLeftPanel.IsVisible(tmpGOCurrItem.transform.position))
//             tmpToY = (float)mCurrSelIndex / (float)(tmpMaxCount - 1);
//         mLeftGridScrollView.SetDragAmount(0.0f, tmpToY, false);

//         if (mCurrSelIndex < 0 || mCurrSelIndex > mLeftGridContainer.MaxCount - 1) return;
// 
//         if (mCurrSelIndex == 0 || mCurrSelIndex == mLeftGridContainer.MaxCount - 1)
//         {
//             mLeftGridScrollView.SetDragAmount(0, mCurrSelIndex / (mLeftGridContainer.MaxCount - 1), false);
//             return;
//         }
// 
//         bool bBack = tmpLeftPanel.IsVisible(mLeftGridContainer.controlList[mCurrSelIndex - 1].transform.position);        // panel.ConstrainTargetToBounds (backObj.transform, false);
//         bool bBefore = tmpLeftPanel.IsVisible(mLeftGridContainer.controlList[mCurrSelIndex + 1].transform.position);     // panel.ConstrainTargetToBounds (beforeObj.transform, false);
// 
//         if (!bBack)
//         {
//             mLeftGridScrollView.SetDragAmount(0, (float)(mCurrSelIndex - 1) / (float)mLeftGridContainer.MaxCount, false);
//         }
//         else if (!bBefore)
//         {
//             mLeftGridScrollView.SetDragAmount(0, (float)(mCurrSelIndex + 1) / (float)mLeftGridContainer.MaxCount, false);
//         }
    }

    /// <summary>
    /// 滑动完成时回调
    /// </summary>
    private void __OnSweepFragmentFinished()
    {
        UICenterOnChild tmpCenter = GameCommon.FindComponent<UICenterOnChild>(mGameObjUI, "piece_grid");
        if (tmpCenter != null)
        {
            string centerGOName = tmpCenter.centeredObject.name;
            int index = -1;
            if (centerGOName == "piece_groups")
                index = 0;
            else
            {
                string preName = "piece_groups(Clone)_";
                string centerGOIndex = centerGOName.Substring(preName.Length);
                if (!int.TryParse(centerGOIndex, out index))
                    index = -1;
            }
            if (index != -1)
            {
                __ToggleMagic(index);
                __ChangeToMagic(index, false);
            }
        }
    }

    /// <summary>
    /// 转向指定碎片位置
    /// </summary>
    /// <param name="fragId"></param>
    private void __GoToFragment(int fragId)
    {
        if (mListMagicData == null || mListMagicData.Count <= 0)
            return;

        DataRecord tmpFragConfig = DataCenter.mFragmentAdminConfig.GetRecord(fragId);
        if (tmpFragConfig == null)
            return;
        int tmpMagicId = (int)tmpFragConfig.getObject("ROLEEQUIPID");
        for (int i = 0, count = mListMagicData.Count; i < count; i++)
        {
            if (mListMagicData[i].tid == tmpMagicId)
            {
                __ChangeToMagic(i, true);
                break;
            }
        }
    }

    /// <summary>
    /// 获取指定品质法器颜色
    /// </summary>
    /// <param name="equipQuality"></param>
    /// <returns></returns>
    public static Color GetMagicColor(EQUIP_QUALITY_TYPE equipQuality)
    {
        int quality = (int)equipQuality;
        if (quality < (int)EQUIP_QUALITY_TYPE.LOW || quality > (int)EQUIP_QUALITY_TYPE.PERFECT)
            return Color.white;
        return smMagicColor[quality - 1];
    }
    /// <summary>
    /// 设置指定法器物品颜色
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="targetName"></param>
    /// <param name="equipQuality"></param>
    public static void SetMagicSpriteColor(GameObject parent, string targetName, EQUIP_QUALITY_TYPE equipQuality)
    {
        return;
        UISprite tmpSprite = GameCommon.FindComponent<UISprite>(parent, targetName);
        if (tmpSprite != null)
            tmpSprite.color = GetMagicColor(equipQuality);
    }
    /// <summary>
    /// 设置指定法器图片颜色
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="targetName"></param>
    /// <param name="color"></param>
    public static void SetMagicSpriteColor(GameObject parent, string targetName, Color color)
    {
        return;
        UISprite tmpSprite = GameCommon.FindComponent<UISprite>(parent, targetName);
        if (tmpSprite != null)
            tmpSprite.color = color;
    }

    /// <summary>
    /// 判断指定索引处法器现在是否可以合成
    /// </summary>
    /// <param name="targetIndex"></param>
    /// <returns></returns>
    public static bool CanCompose(int targetIndex)
    {
        GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        if (tmpWin == null)
            return false;
        if (tmpWin.mListMagicData == null)
            tmpWin.mListMagicData = tmpWin.GetExistMagicList();
        if (targetIndex < 0 || targetIndex >= tmpWin.mListMagicData.Count)
            return false;

        GrabTreasureItemData tmpComposeData = tmpWin.mListMagicData[targetIndex];
        DataRecord tmpComposeConfig = DataCenter.mEquipComposeConfig.GetRecord(tmpComposeData.tid);
        if (tmpComposeConfig == null)
        {
            Log("找不到合成物品" + tmpComposeData.tid.ToString());
            return false;
        }
        int tmpFragNum = (int)tmpComposeConfig.getObject("NUM");
        bool canCompose = true;
        for (int i = 0; i < tmpFragNum; i++)
        {
            int tmpFragId = (int)tmpComposeConfig.getObject("FRAGMENT_" + (i + 1).ToString());
            EquipFragmentData tmpFragData = null;
            if (!MagicFragmentLogicData.Self.mDicEquipFragmentData.TryGetValue(tmpFragId, out tmpFragData) ||
                tmpFragData.itemNum <= 0)
            {
                canCompose = false;
                break;
            }
        }
        return canCompose;
    }

    #region 红点相关
    public static bool CanComposeByTid(int kTid) 
    {
        DataRecord tmpComposeConfig = DataCenter.mEquipComposeConfig.GetRecord(kTid);
        if (tmpComposeConfig == null)
        {
            Log("找不到合成物品" + kTid.ToString());
            return false;
        }
        int tmpFragNum = (int)tmpComposeConfig.getObject("NUM");
        bool canCompose = true;
        for (int i = 0; i < tmpFragNum; i++)
        {
            int tmpFragId = (int)tmpComposeConfig.getObject("FRAGMENT_" + (i + 1).ToString());
            EquipFragmentData tmpFragData = null;
            if (!MagicFragmentLogicData.Self.mDicEquipFragmentData.TryGetValue(tmpFragId, out tmpFragData) ||
                tmpFragData.itemNum <= 0)
            {
                canCompose = false;
                break;
            }
        }
        return canCompose;
    }
    #endregion
    /// <summary>
    /// 判断当前索引处法器选择是否可以合成
    /// </summary>
    /// <returns></returns>
    public static bool CanCurrentTargetCompose()
    {
        GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        if (tmpWin == null || tmpWin.mListMagicData == null)
            return false;

        return CanCompose(tmpWin.mCurrSelIndex);
    }
    /// <summary>
    /// 根据指定法器Id移除相应碎片
    /// </summary>
    /// <param name="targetId"></param>
    public static void RemoveFragmentsByCompose(int targetId)
    {
        GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        if (tmpWin == null)
            return;

        DataRecord tmpComposeConfig = DataCenter.mEquipComposeConfig.GetRecord(targetId);
        if (tmpComposeConfig == null)
        {
            LogError("找不到合成物品" + targetId.ToString());
            return;
        }
        int tmpFragNum = (int)tmpComposeConfig.getObject("NUM");
        for (int i = 0; i < tmpFragNum; i++)
        {
            int tmpFragId = (int)tmpComposeConfig.getObject("FRAGMENT_" + (i + 1).ToString());
            EquipFragmentData tmpFragData = null;
            if (!MagicFragmentLogicData.Self.mDicEquipFragmentData.TryGetValue(tmpFragId, out tmpFragData) ||
                tmpFragData.itemNum <= 0)
                break;
            //可以移除
            PackageManager.RemoveItem(tmpFragData.tid, tmpFragData.itemId, 1);
        }
    }

    public static void StartGrabBattle()
    {
        //         int pvpBattleIndex = 20001;
        //         MainProcess.ClearBattle();
        //         DataCenter.Set("CURRENT_STAGE", pvpBattleIndex);
        //         MainProcess.LoadBattleScene();

        GrabBattle battle = MainProcess.mStage as GrabBattle;
        GrabTreasureWindow grabWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        ChallengePlayer currChallengePlayer = grabWin.getObject("CURRENT_CHALLENGE_PLAYER") as ChallengePlayer;
        string tmpAimUid = grabWin.get("CURRENT_GRAB_UID");
        ArenaBase.challengeTarget = currChallengePlayer;
        DataCenter.OpenWindow("PET_ATTACK_BOSS_WINDOW", "OpenGrabAutoFight");
        if (currChallengePlayer != null)
        {
            battle.SetOpponentCharacterDetail(currChallengePlayer.petFDs[0], currChallengePlayer.power, GameCommon.IsRobot(tmpAimUid));
            int len = currChallengePlayer.petFDs.Length - 1;

            for (int i = 0, count = len; i < count; i++)
            {
                var detail = currChallengePlayer.petFDs[i + 1];

                if (detail != null)
                {
                    var data = new PetData();
                    data.tid = detail.tid;
                    data.teamPos = -1;
                    battle.SetOpponentPetAndDetail(i, data, detail);
                }
            }
        }
    }
}

/// <summary>
/// 左侧向上滚动
/// </summary>
class Button_grab_window_left_arrow_up : CEvent
{
    public override bool _DoEvent()
    {
        GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        if (tmpWin != null)
            tmpWin.LeftScrollUp();

        return true;
    }
}

/// <summary>
/// 左侧向下滚动
/// </summary>
class Button_grab_window_left_arrow_down : CEvent
{
    public override bool _DoEvent()
    {
        GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        if (tmpWin != null)
            tmpWin.LeftScrollDown();

        return true;
    }
}

/// <summary>
/// 中心向左滚动
/// </summary>
class Button_grab_window_center_left_arrow : CEvent
{
    public override bool _DoEvent()
    {
        GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        if (tmpWin != null)
            tmpWin.LeftScrollUp();

        return true;
    }
}

/// <summary>
/// 中心向右滚动
/// </summary>
class Button_grab_window_center_right_arrow : CEvent
{
    public override bool _DoEvent()
    {
        GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        if (tmpWin != null)
            tmpWin.LeftScrollDown();

        return true;
    }
}

/// <summary>
/// 免战按钮
/// </summary>
class Button_GrabTreasure_grab_peace_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("GRABTREASURE_SELECT_PEACE_WINDOW");

        return true;
    }
}

/// <summary>
/// 夺宝历史按钮
/// </summary>
class Button_GrabTreasure_grab_record_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("GRABTREASURE_RECORD_WINDOW");
        GlobalModule.DoCoroutine(__DoAction());

        return true;
    }

    private IEnumerator __DoAction()
    {
        GrabTreasure_GetRobbedHistoryList_Requester tmpRequester = new GrabTreasure_GetRobbedHistoryList_Requester();
        yield return tmpRequester.Start();

        if (tmpRequester.respCode == 1)
        {
            SC_GetRobbedHistoryList tmpResp = tmpRequester.respMsg as SC_GetRobbedHistoryList;
            DataCenter.SetData("GRABTREASURE_RECORD_WINDOW", "REFRESH", tmpResp);
        }
    }
}

/// <summary>
/// 中心完整法器按钮
/// </summary>
class Button_grab_window_center_magic_btn : CEvent
{
    public override bool _DoEvent()
    {
        int targetMagicId = (int)getObject("TARGET_MAGIC_ID");
        //modified by xuke
        //DataCenter.OpenWindow("GRABTREASURE_COMPOSE_ITEM_INFO_WINDOW", targetMagicId);
        //----
        //added by xuke
        DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW",targetMagicId);
        //end
        return true;
    }
}

/// <summary>
/// 法器列表法器按钮
/// </summary>
class Button_grab_window_item_icon_btn : CEvent
{
    public override bool _DoEvent()
    {
        int itemIndex = (int)getObject("ITEM_INDEX");
        DataCenter.SetData("GRABTREASURE_WINDOW", "CHANGE_TO", itemIndex);

        return true;
    }
}

/// <summary>
/// 碎片按钮
/// </summary>
class Button_grab_window_piece_btn : CEvent
{
    public override bool _DoEvent()
    {
        int targetFragId = (int)getObject("TARGET_FRAG_ID");
        int targetFragNum = (int)getObject("TARGET_FRAG_NUMBER");
        if (targetFragNum == 0)
            GlobalModule.DoCoroutine(__DoAction(targetFragId));
        else
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_ALREADY_HAVE_FRAGMENT);

        return true;
    }

    private IEnumerator __DoAction(int targetFragId)
    {
        GrabTreasure_GetRobAimList_Requester tmpRequester = new GrabTreasure_GetRobAimList_Requester();
        tmpRequester.Tid = targetFragId;
        yield return tmpRequester.Start();

        if (tmpRequester.respCode == 1)
        {
            DataCenter.OpenWindow("GRABTREASURE_FIGHT_WINDOW", targetFragId);
            DataCenter.SetData("GRABTREASURE_FIGHT_WINDOW", "REFRESH_FIGHT_LIST", tmpRequester.respMsg);
        }
    }
}

/// <summary>
/// 合成按钮
/// </summary>
class Button_grab_window_compose_btn : CEvent
{
    public override bool _DoEvent()
    {
        GrabTreasureComposeData tmpCurrComposeData = GrabTreasureWindow.GetCurrentComposeData();
        if(tmpCurrComposeData == null)
            return true;
        if (!GrabTreasureWindow.CanCurrentTargetCompose())
        {
            DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_GRAB_COMPOSE_FRAGMENT_NO_ENOUGH);
            return true;
        }

        //判断背包是否已满
        List<PACKAGE_TYPE> tmpPackageTypes = new List<PACKAGE_TYPE>()
        {
            PackageManager.GetPackageTypeByItemTid(tmpCurrComposeData.TargetItem.tid)
        };
        if (!CheckPackage.Instance.CanGrabTreasure(tmpPackageTypes))
            return true;

        DataCenter.SetData("GRABTREASURE_WINDOW", "CURRENT_COMPOSE_TARGET_ITEM", tmpCurrComposeData.TargetItem);

		DataCenter.SetData ("GRABTREASURE_WINDOW" , "GRAB_COMPOSE_EFFECT", tmpCurrComposeData.ListItems.Count);

        GlobalModule.DoCoroutine(__DoAction(tmpCurrComposeData.TargetItem.tid, tmpCurrComposeData.ListItems));

        return true;
    }

    private IEnumerator __DoAction(int tid, List<ItemDataBase> frags)
    {
        GrabTreasure_MagicCompose_Requester tmpRequester = new GrabTreasure_MagicCompose_Requester();
        tmpRequester.Tid = tid;
        tmpRequester.Frags = frags;
        yield return tmpRequester.Start();

        if (tmpRequester.respCode == 1)
        {
            SC_MagicCompose tmpResp = tmpRequester.respMsg;

            ////加物品
            //if (tmpResp.ItemMagic.itemId == -1)
            //    PackageManager.AddItem(tmpResp.ItemMagic);
            //else
            //    PackageManager.UpdateItem(tmpResp.ItemMagic);
            //移除物品
            //GrabTreasureWindow.RemoveFragmentsByCompose(tid);

            // 刷新法宝
            PackageManager.UpdateItem(tmpResp.ItemMagic);
            // 刷新法宝碎片
            PackageManager.UpdateItem(tmpResp.magicFragmentsInfo);

            GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
            ItemDataBase tmpItem = tmpWin.getObject("CURRENT_COMPOSE_TARGET_ITEM") as ItemDataBase;
            GameObject.Find("grab_record_btn").GetComponent<BoxCollider>().enabled=false;
            GameObject.Find("grab_peace_btn").GetComponent<BoxCollider>().enabled = false;
            GlobalModule.DoLater(() =>
            {
                DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW", tmpItem.tid);
                GameObject.Find("grab_record_btn").GetComponent<BoxCollider>().enabled =true;
                GameObject.Find("grab_peace_btn").GetComponent<BoxCollider>().enabled = true;
            }, 1.0f);
//            DataCenter.OpenWindow("GRABTREASURE_COMPOSE_RESULT_WINDOW", tmpItem.tid);

            // 合成之后刷新碎片界面
            DataCenter.SetData("GRABTREASURE_WINDOW", "REFRESH", null);
        }
    }
}

/// <summary>
/// 退出按钮
/// </summary>
class Button_grab_window_back_btn : CEvent
{
    public override bool _DoEvent()
    {
        //added by xuke 红点相关
        GrabTreasureNewMarkManager.Self.CheckComposeTid_NewMark();
        TrialNewMarkManager.Self.RefreshTrialNewMark();
        //end
        DataCenter.CloseWindow("GRABTREASURE_WINDOW");
        return true;
    }
}

/// <summary>
/// 超过100小时只显示100时
/// </summary>
class GrabMaxShowTimeCountdownUI : MaxShowTimeCountdownUI
{
    protected override string _FormatTime(int[] time)
    {
        if (!IsUseDaysTime && time[(int)MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.HOURS] >= 100)
            return "100时";
        return base._FormatTime(time);
    }
}
