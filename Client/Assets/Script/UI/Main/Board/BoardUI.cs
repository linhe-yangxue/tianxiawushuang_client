using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Logic;
using DataTable;


public class BoardUI : MonoBehaviour
{
    private void Start()
    {
        AnnounceWindow win = new AnnounceWindow();
        win.mWinName = "BOARD_WINDOW";
        win.mGameObjUI = this.gameObject;
        DataCenter.RegisterData("BOARD_WINDOW", win);
        DataCenter.SetData("BOARD_WINDOW", "OPEN", true);
    }

    private void OnDestroy()
    {
        DataCenter.Remove("BOARD_WINDOW");
    }
}


public class AnnounceWindow : TabWindow
{
    private Dictionary<int, List<DataRecord>> boardData = null;
    private List<int> keyList = null;

    public override void Open(object param)
    {
        base.Open(param);
        Net.StartEvent("CS_RequestAnouncement").DoEvent();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_DATA":
                NiceTable table = objVal as NiceTable;

                if (table != null)
                    RefreshData(table);

                break;
        }
    }

    protected override GameObject GetContainer()
    {
        return GameCommon.FindObject(mGameObjUI, "class_list", "grid");
    }

    protected override int GetTabCount()
    {
        return keyList == null ? 0 : keyList.Count;
    }

    protected override int GetTabKey(int index)
    {
        return keyList == null ? 0 : keyList[index];
    }

    protected override void InitTab(GameObject tab, int key)
    {
        base.InitTab(tab, key);

        DataRecord record = boardData[key][0];
        string tabName = record["PAGENAME"];
        GameObject label1 = GameCommon.FindObject(tab, "active_icon", "label");
        GameObject label2 = GameCommon.FindObject(tab, "inactive_icon", "label");
        label1.GetComponent<UILabel>().text = tabName;
        label2.GetComponent<UILabel>().text = tabName;
    }

    protected override bool OnSelectTab(GameObject tab, int key)
    { 
        List<DataRecord> records = boardData[key];
        BoardGrid grid = GameCommon.FindObject(mGameObjUI, "item_list", "grid").GetComponent<BoardGrid>();
        grid.Clear();
        grid.count = records.Count;
        grid.onInitDesc = InitDesc;

        for (int i = 0; i < grid.itemList.Count; ++i)
            InitItem(grid.itemList[i], records[i]);

        UIScrollView view = GameCommon.FindObject(mGameObjUI, "item_list", "view").GetComponent<UIScrollView>();
        view.ResetPosition();

        return true;
    }

    private void InitItem(GameObject obj, DataRecord record)
    {
        obj.AddComponent<BoardItemRecord>().record = record;
        UITexture texture = obj.GetComponent<UITexture>();
        string imgName = record.get("ADPIC");
        texture.mainTexture = GameCommon.LoadTexture(imgName, LOAD_MODE.RESOURCE);//Resources.Load(imgName, typeof(Texture)) as Texture;

        if (record["QUCIKBUTTON"] == "")
            GameCommon.SetUIVisiable(obj, "board_go", false);
        else
            GameCommon.SetUIVisiable(obj, "board_go", true);
    }

    private bool InitDesc(GameObject desc, GameObject item)
    {
        DataRecord record = item.GetComponent<BoardItemRecord>().record;

        if (record != null)
        {
            UISprite sprite = desc.GetComponent<UISprite>();
            UILabel label = GameCommon.FindComponent<UILabel>(desc, "label");
            string str = record.get("DESC");
            str = str.Replace("\\n", "\n");
            label.text = str;
            sprite.height = label.height + 10;
            return !string.IsNullOrEmpty(str);
        }

        return false;
    }

    private void RefreshData(NiceTable table)
    {
        boardData = new Dictionary<int, List<DataRecord>>();
        keyList = new List<int>();

        foreach (var pair in table.GetAllRecord())
        {
            if (pair.Value["INDEX"] > 0)
            {
                int page = pair.Value["PAGENUMBER"];

                if (!boardData.ContainsKey(page))
                    boardData.Add(page, new List<DataRecord>());

                boardData[page].Add(pair.Value);
            }
        }

        foreach (var key in boardData.Keys)
            keyList.Add(key);

        keyList.Sort((lhs, rhs) => lhs - rhs);

        Refresh(null);
    }
}


public class BoardItemRecord : MonoBehaviour
{
    public DataRecord record = null;
}