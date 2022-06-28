using UnityEngine;
using Logic;


public class WorldMapBonusWindow : tWindow
{
    private const string ITEM_PATH = "Prefabs/UI/WorldMap/bonus_item";
    private const int ITEM_INTERVAL = 140;

    private StageBonus bonus = null;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_world_map_bonus_commit", new DefineFactory<Button_world_map_bonus_commit>());
        EventCenter.Self.RegisterEvent("Button_world_map_bonus_back", new DefineFactory<Button_world_map_bonus_back>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        int bonusIndex = new Data(param);
        bonus = StageBonus.Create(bonusIndex);

        if (bonus != null)
            ShowBonus(bonus);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "COMMIT":
                if (bonus == null || !bonus.unlocked)
                    Close();
                else
                    AcceptBonus(bonus);
                break;
        }
    }

    private void AcceptBonus(StageBonus bonus)
    {
        CS_AcceptStageBonus evt = Net.StartEvent("CS_AcceptStageBonus") as CS_AcceptStageBonus;
        evt.set("INDEX", bonus.mIndex);
        evt.DoEvent();
    }

    private void ShowBonus(StageBonus bonus)
    {
        string stageName = TableCommon.GetStringFromStageConfig(bonus.mFrontStage, "STAGENUMBER");
        string stageDifficulty = "";
        //int difficulty = StageProperty.GetStageDifficulty(bonus.mFrontStage);

        //switch (difficulty)
        //{
        //    case 1:
        //        stageDifficulty = "封灵之地";
        //        break;
        //    case 2:
        //        stageDifficulty = "封魔之境";
        //        break;
        //    case 3:
        //        stageDifficulty = "天下无双";
        //        break;
        //}

        string title = "通关" + stageDifficulty + stageName + "后可获得如下奖励";
        SetText("title2", title);

        GameObject btn = GetSub("world_map_bonus_commit");

        if (bonus.unlocked)
            GameCommon.SetUIText(btn, "Label", "领取");
        else
            GameCommon.SetUIText(btn, "Label", "确定");

        for (int i = 0; i < StageBonus.MAX_ITEM_COUNT; ++i)
        {
            GameObject itemObj = GetSub("bonus_item_" + i);

            if (i < bonus.mItemCount)
            {
                itemObj.SetActive(true);
                StageBonusItem item = bonus.mItems[i];
                InitItemPos(itemObj, i, bonus.mItemCount);
                InitItem(itemObj, item);
            }
            else
            {
                itemObj.SetActive(false);
            }
        }
    }

    private void InitItemPos(GameObject obj, int index, int count)
    {
        if (count == 1)
        {
            obj.transform.localPosition = new Vector3(0, 20, 0);
        }
        else if (count == 2)
        {
            if (index == 0)
                obj.transform.localPosition = new Vector3(-ITEM_INTERVAL / 2, 20, 0);
            else
                obj.transform.localPosition = new Vector3(ITEM_INTERVAL / 2, 20, 0);
        }
        else // count == 3
        {
            if (index == 0)
                obj.transform.localPosition = new Vector3(-ITEM_INTERVAL, 20, 0);
            else if (index == 1)
                obj.transform.localPosition = new Vector3(0, 20, 0);
            else
                obj.transform.localPosition = new Vector3(ITEM_INTERVAL, 20, 0);
        }
    }

    private void InitItem(GameObject obj, StageBonusItem item)
    {
        UISprite sprite = GameCommon.FindComponent<UISprite>(obj, "icon");
        GameCommon.SetItemIcon(sprite, item.mType, item.mId);
        GameCommon.SetUIText(obj, "name", GetItemName(item));
        GameCommon.SetUIText(obj, "count", "X " + item.mCount.ToString());

        if (item.mType == (int)ITEM_TYPE.PET || item.mType == (int)ITEM_TYPE.EQUIP)
        {
            GameCommon.SetUIVisiable(obj, "star_level_label", true);
            GameCommon.SetUIText(obj, "star_level_label", item.mStarLevel.ToString());
        }
        else
        {
            GameCommon.SetUIVisiable(obj, "star_level_label", false);
        }
    }

    private string GetItemName(StageBonusItem item)
    {
        switch (item.mType)
        {
            case (int)ITEM_TYPE.GOLD:
                return "金币";

            case (int)ITEM_TYPE.YUANBAO:
                return "元宝";

            case (int)ITEM_TYPE.POWER:
                return "体力";

            case (int)ITEM_TYPE.GEM:
				return "灵石";

            case (int)ITEM_TYPE.PET:
                return TableCommon.GetStringFromActiveCongfig(item.mId, "NAME");

            case (int)ITEM_TYPE.EQUIP:
                return TableCommon.GetStringFromRoleEquipConfig(item.mId, "NAME");
        }

        return "";
    }
}


public class Button_world_map_bonus_commit : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("WORLD_MAP_BONUS_WINDOW", "COMMIT", null);
        return true;
    }
}

public class Button_world_map_bonus_back : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("WORLD_MAP_BONUS_WINDOW");
        return true;
    }
}