using System;
using System.Collections.Generic;
using UnityEngine;
using Logic;
using DataTable;


public class ActiveStageWindow : TabWindow
{
    private List<int> tabKeys = null;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_active_stage_back", new DefineFactory<Button_active_stage_back>());
        EventCenter.Self.RegisterEvent("Button_active_stage_item", new DefineFactory<Button_active_stage_item>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        tabKeys = new List<int>();

        foreach (var pair in DataCenter.mEventList.GetAllRecord())
        {
            if (pair.Key > 0)
                tabKeys.Add(pair.Key);
        }

		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "explore_land_button", UNLOCK_FUNCTION_TYPE.EXPLORE_LAND);
    }

    protected override int GetTabCount() { return tabKeys.Count; }

    protected override int GetTabKey(int index) { return tabKeys[index]; }

    protected override GameObject GetContainer() { return GameCommon.FindObject(mGameObjUI, "class_list", "grid"); }

//    protected override void InitTab(GameObject tab, int key)
//    {
//        base.InitTab(tab, key);
//        
//        DataRecord record = DataCenter.mEventList.GetRecord(key);
//        GameCommon.SetIcon(tab, "active_icon", record.get("LISTIMG_SPRITE"), record.get("LISTIMG_ATLAS"));
//        GameCommon.SetIcon(tab, "inactive_icon", record.get("LISTIMG_OFF_SPRITE"), record.get("LISTIMG_OFF_ATLAS"));
//    }

    protected override bool OnSelectTab(GameObject tab, int key)
    {
        List<DataRecord> itemList = GetItemList(key);
        GameObject obj = GameCommon.FindObject(mGameObjUI, "item_list");
        UIGridContainer container = GameCommon.FindComponent<UIGridContainer>(obj, "grid");
        container.MaxCount = itemList.Count;

        for (int i = 0; i < itemList.Count; ++i)
        {
            InitItemCell(container.controlList[i], itemList[i]);
        }

        UIScrollView view = GameCommon.FindComponent<UIScrollView>(obj, "view");
        view.ResetPosition();

        return true;
    }

    public void RefreshCurrentClass(object param)
    {
        GuideManager.ExecuteDelayed(() => OnSelectTab(null, currentTabKey), 0f);
    }

    private void InitItemCell(GameObject cell, DataRecord record)
    {
        int stageIndex = record.get("STAGE_ID");
        StageProperty property = StageProperty.Create(stageIndex);

        string stageName = TableCommon.GetStringFromStageConfig(stageIndex, "NAME");
        GameCommon.SetUIText(cell, "stage_name", stageName);

        if (property.openForever)
        {
            GameCommon.SetUIVisiable(cell, "stage_time", false);
        }
        else
        {
            GameCommon.SetUIVisiable(cell, "stage_time", true);
            CallBack callback = new CallBack(this, "RefreshCurrentClass", null);
            SetCountdown(cell, "stage_time_num", property.closingTime, callback);
        }

        int difficulty = record.get("STAGEDIFFICULTY");
        UIGridContainer difficultyStarGrid = GameCommon.FindComponent<UIGridContainer>(cell, "stage_difficulty_grid");
        difficultyStarGrid.MaxCount = difficulty;

        UITexture texture = GameCommon.FindComponent<UITexture>(cell, "bg");
        string imgName = record.get("EVENTBARIMG");
        texture.mainTexture = GameCommon.LoadTexture(imgName, LOAD_MODE.RESOURCE);//Resources.Load(imgName, typeof(Texture)) as Texture;
        GameCommon.GetButtonData(cell, "active_stage_item").set("STAGE_INDEX", stageIndex);

        string count = "∞";

        if (property.mFightLimit > 0)
        {
            int chance = property.mFightLimit - property.mPassCount;

            if (chance <= 0)
            {
                count = "0";
                GameCommon.SetUIButtonEnabled(cell, "active_stage_item", false);
            }
            else 
            {
                count = Mathf.Max(0, property.mFightLimit - property.mPassCount).ToString();
                GameCommon.SetUIButtonEnabled(cell, "active_stage_item", true);
            }           
        }

        GameCommon.SetUIText(cell, "count_num", count);
    }

    private List<DataRecord> GetItemList(int listIndex)
    {
        return TableCommon.FindAllRecords(DataCenter.mEventConfig, r => r.get("INDEX") > 0 && r.get("LIST_ID") == listIndex && IsItemUnlocked(r));
    }

    private bool IsItemUnlocked(DataRecord record)
    {
        int stageIndex = record.get("STAGE_ID");
        StageProperty property = StageProperty.Create(stageIndex);
        return property != null && property.unlocked;
    }
}


public class Button_active_stage_back : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("ACTIVE_STAGE_WINDOW");
        return true;
    }
}

public class Button_active_stage_item : CEvent
{
    public override bool _DoEvent()
    {
        int index = get("STAGE_INDEX");

        if (index <= 0)
            return false;

        DataCenter.OpenWindow("STAGE_INFO_WINDOW", index);
        return true;
    }
}