using UnityEngine;
using System;
using System.Collections.Generic;
using Logic;


public class SelectItemWindow : tWindow
{
    public static Action<ItemDataBase> onCommit { get; set; }

    private ItemDataBase selectedItem = null; 

    public override void Open(object param)
    {
        base.Open(param);

        selectedItem = null;
        IEnumerable<ItemDataBase> items = param as IEnumerable<ItemDataBase>;

        if (items == null)
        {
            Close();
            return;
        }

        List<ItemDataBase> itemList = new List<ItemDataBase>(items);
        int count = itemList.Count;

        if (count == 0)
        {
            Close();
            return;
        }

        UIScrollView scrollview = GetComponent<UIScrollView>("Scroll View");
        UIGridContainer container = GetComponent<UIGridContainer>("Grid");
        GameObject scrollbar = GetSub("scroll_bar");

        container.MaxCount = 0;
        container.MaxCount = count;
        scrollview.ResetPosition();
        scrollview.enabled = count > 4;
        scrollbar.SetActive(count > 4);
        container.transform.localPosition = new Vector3(count < 4 ? -50 * (count - 1): -150, 0, 0);

        for (int i = 0; i < container.MaxCount; ++i)
        {
            GameObject obj = container.controlList[i];
            ItemDataBase item = itemList[i];

            GameCommon.SetItemIcon(obj, item);
            obj.GetComponent<UIButtonEvent>().AddAction(() => selectedItem = item);
        }

        AddButtonAction("select_ok_button", OnClickOk);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "TITLE":
                SetText("title", (string)objVal);
                break;

            case "DESC":
                SetText("desc", (string)objVal);
                break;
        }
    }

    private void OnClickOk()
    {
        if (selectedItem == null)
        {
            DataCenter.OpenMessageWindow("请选择物品");
        }
        else 
        {
            Close();

            if (onCommit != null)
            {
                onCommit(selectedItem);
                onCommit = null;
            }
        }
    }
}