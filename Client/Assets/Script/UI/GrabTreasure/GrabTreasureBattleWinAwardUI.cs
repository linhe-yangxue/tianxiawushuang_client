using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

public class GrabTreasureBattleWinAwardWindow : tWindow
{
    private static string[] mBoxName = new string[] {
        "grab_treasure_platina_btn",
        "grab_treasure_gold_btn",
        "grab_treasure_diamond_btn"
    };

    private ItemDataBase mPlayerSelItem;                                //玩家选择的物品
    private List<ItemDataBase> mItems = new List<ItemDataBase>();       //剩余展示的物品

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_grab_treasure_platina_btn", new DefineFactoryLog<Button_grab_treasure_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_treasure_gold_btn", new DefineFactoryLog<Button_grab_treasure_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_treasure_diamond_btn", new DefineFactoryLog<Button_grab_treasure_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_treasure_close_button", new DefineFactoryLog<Button_grab_treasure_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        mPlayerSelItem = param as ItemDataBase;

        __HideAllItems();
        Refresh(param);
        __CloseButtonVisible(false);
        __EnableAllBoxClick(true);
    }

    public override bool Refresh(object param)
    {
        __RefreshItems(param);

        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "SELECT_AWARD":
                {
                    int index = (int)objVal;
                    __OpenBox(index);
                }break;
        }
    }

    /// <summary>
    /// 隐藏所有物品，关闭所有箱子
    /// </summary>
    private void __HideAllItems()
    {
        for (int i = 0, count = mBoxName.Length; i < count; i++)
		{
            GameCommon.SetUIVisiable(GetSub(mBoxName[i]), "item_icon", false);
			GameCommon.SetUIVisiable(GetSub(mBoxName[i]), "open_box", false);
			GameCommon.SetUIVisiable(GetSub(mBoxName[i]), "Background", true);
		}
    }

    /// <summary>
    /// 刷新所有箱子数据
    /// </summary>
    /// <param name="param"></param>
    private void __RefreshItems(object param)
    {
        if (mPlayerSelItem == null)
            return;

        __CreateNoClickItems(true);
        for (int i = 0, count = mBoxName.Length; i < count; i++)
        {
            NiceData tmpBtnBoxData = GameCommon.GetButtonData(GetSub(mBoxName[i]));
            if (tmpBtnBoxData != null)
                tmpBtnBoxData.set("BOX_INDEX", i);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="canDuplication">是否可以重复</param>
    private void __CreateNoClickItems(bool canDuplication)
    {
        int tmpGroupID = -1;//角色等级控制夺宝翻牌掉落组
        int tmpPlayerLevel = RoleLogicData.Self.character.level;
        Dictionary<int, DataRecord> groupInfo = DataCenter.mIndianaDraw.GetAllRecord();//从IndianaDraw表中获取主角当前等级对应的GroupID
        foreach (DataRecord groupRecrod in groupInfo.Values)
        {
            int tmpLevelMin;
            int tmpLevelMax;
            groupRecrod.get("LEVEL_MIN", out tmpLevelMin);
            groupRecrod.get("LEVEL_MAX", out tmpLevelMax);

            if (tmpPlayerLevel >= tmpLevelMin && tmpPlayerLevel <= tmpLevelMax)
            {
                groupRecrod.get("DRAW_GROUP_ID", out tmpGroupID);
                break;
            }
        }

        List<ItemDataBase> tmpItems = GameCommon.GetItemGroup(tmpGroupID, true);
        tmpItems.RemoveAll((ItemDataBase tmp) =>
        {
            return (tmp.tid == mPlayerSelItem.tid);
        });
        mItems.Clear();
        for (int i = 0; i < 2; i++)
        {
            int tmpIdx = Random.Range(0, tmpItems.Count - 1);
            mItems.Add(tmpItems[tmpIdx]);
            if (!canDuplication)
                tmpItems.RemoveAt(tmpIdx);
        }
    }

    /// <summary>
    /// 开启、禁止所有箱子点击
    /// </summary>
    private void __EnableAllBoxClick(bool bEnable)
    {
        for (int i = 0, count = mBoxName.Length; i < count; i++)
        {
            BoxCollider tmpBox = GameCommon.FindComponent<BoxCollider>(mGameObjUI, mBoxName[i]);
            if (tmpBox != null)
                tmpBox.enabled = bEnable;
            if (bEnable)
            {
                UIButton tmpBtn = GameCommon.FindComponent<UIButton>(mGameObjUI, mBoxName[i]);
                tmpBtn.isEnabled = true;
            }
        }
    }
    /// <summary>
    /// 显示隐藏关闭按钮
    /// </summary>
    /// <param name="bVisible"></param>
    private void __CloseButtonVisible(bool bVisible)
    {
        GameCommon.SetUIVisiable(mGameObjUI, "grab_treasure_close_button", bVisible);
    }

    /// <summary>
    /// 开启箱子
    /// </summary>
    /// <param name="selIndex"></param>
    private void __OpenBox(int selIndex)
    {
        //设置碎片Icon
        int tmpNoSelBoxIdx = 0;      //没被选择的物品索引，对应mItems
        for (int i = 0, count = mBoxName.Length; i < count; i++)
        {
            ItemDataBase tmpItem = null;
            if (i == selIndex)
            {
                //选择的物品
                tmpItem = mPlayerSelItem;
            }
            else
            {
                //没选择的物品
                tmpItem = mItems[tmpNoSelBoxIdx];
                tmpNoSelBoxIdx += 1;
            }
            GameCommon.SetOnlyItemIcon(GetSub(mBoxName[i]), "item_icon", tmpItem.tid);
            GameCommon.FindComponent<UILabel>(GetSub(mBoxName[i]), "count_label").text = "x" + tmpItem.itemNum.ToString();
            GameCommon.FindComponent<UILabel>(GetSub(mBoxName[i]), "item_name_label").text = GameCommon.GetItemName(tmpItem.tid);
        }

        //TODO 输出宝物信息
        string tmpStrInfo = "";
        tmpStrInfo += ("Select item : " + mPlayerSelItem.tid + ", " + GameCommon.GetItemName(mPlayerSelItem.tid));
        tmpStrInfo += ("\nNo select item : ");
        for (int i = 0, count = mItems.Count; i < count; i++)
            tmpStrInfo += (mItems[i].tid + ", " + GameCommon.GetItemName(mItems[i].tid) + ", ");
        DEBUG.Log(tmpStrInfo);

//        GlobalModule.DoCoroutine(__DoOpenBoxAnim(selIndex));
		__DoOpenBoxAnim(selIndex);
    }
///	//统一（夺宝和巅峰挑战）开宝箱的方法 故注释掉了。
	/// <summary>
	/// 打开宝箱动画
	/// </summary>
	/// <param name="selIndex"></param>
	/// <returns></returns>
	private void __DoOpenBoxAnim(int selIndex)
	{
		for (int i = 0, count = mBoxName.Length; i < count; i++)
		{
			if (i == selIndex)
			{
				GameObject obj = GetSub(mBoxName[i]);
				__DoOpenSelBoxAnim(obj);
			}				
		}
		for (int i = 0, count = mBoxName.Length; i < count; i++)
		{
			if (i != selIndex)
			{
				GameObject obj = GetSub(mBoxName[i]);
				GlobalModule.DoLater(() => __DoOpenNoSelBoxAnim(obj), 0.5f);
			}				
		}
		GlobalModule.DoLater(() => __EnableAllBoxClick(false), 0.3f);
		__CloseButtonVisible(true);
	}

	/// <summary>
	/// 打开选择的宝箱动画
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	private void __DoOpenSelBoxAnim(GameObject obj)
	{
		GameObject tmpGOBox = GameCommon.FindObject(obj, "item_icon");
		GameObject boxBackground = GameCommon.FindObject(obj, "Background");
		GameObject boxOpen = GameCommon.FindObject(obj, "open_box");
		boxBackground.SetActive (false);
		boxOpen.SetActive (true);
		tmpGOBox.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		tmpGOBox.transform.localPosition = Vector3.zero;
		tmpGOBox.SetActive(true);
		GlobalModule.DoLater (() => GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UIEffect/ui_zhuangbei_jihuo", GameCommon.FindObject (obj, "item_icon")), 0.3f);
		TweenScale.Begin(tmpGOBox, 0.3f, Vector3.one);
		TweenPosition.Begin (tmpGOBox, 0.3f, new Vector3(0f, 26.0f, 0f));

	}
	/// <summary>
	/// 打开未选择的宝箱动画
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	private void __DoOpenNoSelBoxAnim(GameObject obj)
	{
        if (obj == null)
            return;

		GameObject tmpGOBox = GameCommon.FindObject(obj, "item_icon");
		GameObject boxBackground = GameCommon.FindObject(obj, "Background");
		GameObject boxOpen = GameCommon.FindObject(obj, "open_box");
        if (boxBackground != null)
            boxBackground.SetActive(false);
        if (boxOpen != null)
            boxOpen.SetActive(true);
        if (tmpGOBox != null)
        {
            tmpGOBox.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            tmpGOBox.transform.localPosition = Vector3.zero;
            tmpGOBox.SetActive(true);
            TweenScale.Begin(tmpGOBox, 0.3f, Vector3.one);
            TweenPosition.Begin(tmpGOBox, 0.3f, new Vector3(0f, 26.0f, 0f));
        }
	}
///	//统一（夺宝和巅峰挑战）开宝箱的方法 故注释掉了。

//    /// <summary>
//    /// 打开宝箱动画
//    /// </summary>
//    /// <param name="selIndex"></param>
//    /// <returns></returns>
//    private IEnumerator __DoOpenBoxAnim(int selIndex)
//    {
//        for (int i = 0, count = mBoxName.Length; i < count; i++)
//        {
//            if (i == selIndex)
//                yield return GlobalModule.DoCoroutine(__DoOpenSelBoxAnim(i));
//        }
//        for (int i = 0, count = mBoxName.Length; i < count; i++)
//        {
//            if (i != selIndex)
//                yield return GlobalModule.DoCoroutine(__DoOpenNoSelBoxAnim(i));
//        }
//
//        __EnableAllBoxClick(false);
//        __CloseButtonVisible(true);
//    }
//    /// <summary>
//    /// 打开选择的宝箱动画
//    /// </summary>
//    /// <param name="index"></param>
//    /// <returns></returns>
//    private IEnumerator __DoOpenSelBoxAnim(int index)
//    {
//		GameObject obj = GetSub(mBoxName[index]);
//		GameObject tmpGOBox = GameCommon.FindObject(obj, "item_icon");
//		GameObject boxBackground = GameCommon.FindObject(obj, "Background");
//		GameObject boxOpen = GameCommon.FindObject(obj, "open_box");
//		boxBackground.SetActive (false);
//		boxOpen.SetActive (true);
//        tmpGOBox.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
//		tmpGOBox.transform.localPosition = Vector3.zero;
//        tmpGOBox.SetActive(true);
//		GlobalModule.DoLater (() => GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UIEffect/ui_zhuangbei_jihuo", GameCommon.FindObject (obj, "item_icon")), 0.3f);
//        TweenScale.Begin(tmpGOBox, 0.3f, Vector3.one);
//		TweenPosition.Begin (tmpGOBox, 0.3f, new Vector3(0f, 26.0f, 0f));
//        yield return new WaitForSeconds(0.5f);
//    }
//    /// <summary>
//    /// 打开未选择的宝箱动画
//    /// </summary>
//    /// <param name="index"></param>
//    /// <returns></returns>
//    private IEnumerator __DoOpenNoSelBoxAnim(int index)
//    {
//		GameObject obj = GetSub(mBoxName[index]);
//		GameObject tmpGOBox = GameCommon.FindObject(obj, "item_icon");
//		GameObject boxBackground = GameCommon.FindObject(obj, "Background");
//		GameObject boxOpen = GameCommon.FindObject(obj, "open_box");
//		boxBackground.SetActive (false);
//		boxOpen.SetActive (true);
//        tmpGOBox.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
//		tmpGOBox.transform.localPosition = Vector3.zero;
//        tmpGOBox.SetActive(true);
//        TweenScale.Begin(tmpGOBox, 0.3f, Vector3.one);
//		TweenPosition.Begin (tmpGOBox, 0.3f, new Vector3(0f, 26.0f, 0f));
//        yield return new WaitForSeconds(0.3f);
//    }
}

/// <summary>
/// 箱子点击
/// </summary>
class Button_grab_treasure_btn : CEvent
{
    public override bool _DoEvent()
    {
        int tmpBoxIndex = (int)getObject("BOX_INDEX");
        DataCenter.SetData("GRABTREASURE_BATTLE_WIN_AWARD_WINDOW", "SELECT_AWARD", tmpBoxIndex);

        return true;
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_grab_treasure_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GRABTREASURE_BATTLE_WIN_AWARD_WINDOW");
        MainProcess.ClearBattle();
        MainUIScript.mLoadingFinishAction = () => DataCenter.OpenWindow("GRABTREASURE_WINDOW");
        MainProcess.LoadRoleSelScene();

        DataCenter.OpenWindow("GRABTREASURE_WINDOW");

        return true;
    }
}
