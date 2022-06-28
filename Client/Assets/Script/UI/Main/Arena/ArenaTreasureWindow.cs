using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ArenaTreasureWindow : tWindow
{
    private ArenaNetManager.ArenaAccountInfo info;
    private bool hasChoosed = false;
    private bool hasAllOpened = false;

    public override void Open(object param)
    {
        base.Open(param);

        SetText("tips_label", TableCommon.getStringFromStringList(STRING_INDEX.ARENA_TREASURE_SELECT));
        info = param as ArenaNetManager.ArenaAccountInfo;

        InitTreasure("treasure_1");
        InitTreasure("treasure_2");
        InitTreasure("treasure_3");
        AddButtonAction("treasure_mask", OnClickMask);

        hasChoosed = info == null || info.mAddItem == null;
        hasAllOpened = info == null || info.mAddItem == null;

        SetVisible("treasure_mask", hasChoosed);
    }

    private void OnClick(string target)
    {
        if (!hasChoosed)
        {
            hasChoosed = true;
            if (info.mAddItem.Count > 3)
            {
                PerformEffect(target, info.mAddItem[3]);
            }
			GameObject obj = GameCommon.FindObject (mGameObjUI, target, "get_rewards_info_cell");
			GlobalModule.DoLater (() => GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UIEffect/ui_zhuangbei_jihuo", GameCommon.FindObject (obj, "item_icon_2")), 0.3f);
            SetVisible("treasure_mask", true);
			GlobalModule.DoLater (() => OpenNoChoseBox (), 0.5f);
//            SetText("tips_label", "点击任意处继续");
        }
    }
	void OpenNoChoseBox()
	{
		if (!hasAllOpened)
		{
			List<ItemDataBase> dropList = GameCommon.GetItemGroup(info.mAwardGroup, true);
			
			if (dropList.Count == 0)
				return;
			
			ItemDataBase item1 = dropList[UnityEngine.Random.Range(0, dropList.Count)];
			PerformEffect("treasure_1", item1);
			
			ItemDataBase item2 = dropList[UnityEngine.Random.Range(0, dropList.Count)];
			PerformEffect("treasure_2", item2);
			
			ItemDataBase item3 = dropList[UnityEngine.Random.Range(0, dropList.Count)];
			PerformEffect("treasure_3", item3);
			
//			SetText("tips_label", "点击任意处退出");
			hasAllOpened = true;
		}
	}

    private void OnClickMask()
    {
        Close();
        PVP4Battle.GoBack();
    }

    private void InitTreasure(string treasure)
    {
        GameObject icon = GameCommon.FindObject(mGameObjUI, treasure, "get_rewards_info_cell");
        icon.SetActive(false);
		GameCommon.FindObject(mGameObjUI, treasure, "open_box").SetActive (false);
		GameCommon.FindObject(mGameObjUI, treasure, "Background").SetActive (true);
        AddButtonAction(treasure, () => OnClick(treasure));
    }

    private void PerformEffect(string treasure, ItemDataBase item)
    {
        GameObject icon = GameCommon.FindObject(mGameObjUI, treasure, "get_rewards_info_cell");
        
        if (icon != null && !icon.activeSelf)
        {
            icon.transform.localScale = Vector3.one * 0.01f;
			icon.transform.localPosition = Vector3.zero;
            icon.SetActive(true);
			GameCommon.FindObject(mGameObjUI, treasure, "open_box").SetActive (true);
			GameCommon.FindObject(mGameObjUI, treasure, "Background").SetActive (false);
            GameCommon.SetOnlyItemIcon(icon, "item_icon_2",item.tid);
            GameCommon.FindComponent<UILabel>(icon, "item_name_label").text = GameCommon.GetItemName(item.tid);
            GameCommon.FindComponent<UILabel>(icon, "count_label").text = "x" + item.itemNum.ToString();
            //GameCommon.SetItemIcon(icon, item);  
            TweenScale.Begin(icon, 0.3f, Vector3.one);
			TweenPosition.Begin (icon, 0.3f, new Vector3(0, 26.0f, 0));
        }
    }
}