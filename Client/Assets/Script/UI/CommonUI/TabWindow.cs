using UnityEngine;
using DataTable;
using Logic;


public class TabWindow : tWindow
{
    protected int currentTabKey = -1;

    protected virtual GameObject GetContainer() { return null; }
    protected virtual int GetTabCount() { return 0; }
    protected virtual int GetTabKey(int index) { return 0; }
    protected virtual bool OnSelectTab(GameObject tab, int key) { return true; }

    protected virtual void InitTab(GameObject tab, int key)
    {
        tab.name = "tab_btn";
        Collider collider = tab.GetComponent<Collider>();
        UIButtonEvent evt = tab.GetComponent<UIButtonEvent>();

        if (collider == null)
            NGUITools.AddWidgetCollider(tab, false);

        if (evt == null)
            evt = tab.AddComponent<UIButtonEvent>();

        evt.mData.set("WIN_KEY", mWinName);
        evt.mData.set("TAB_KEY", key);
    }

    public override void OnOpen()
    {
        Refresh(null);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "SELECT_TAB":
                int index = new Data(objVal);
                SelectTab(index);
                break;
        }
    }

    public override bool Refresh(object param)
    {
        currentTabKey = -1;
        UIGridContainer container = GetGridCountainer();

        if (container == null)
            return false;

        container.MaxCount = GetTabCount();

        for (int i = 0; i < container.MaxCount; ++i)
        {
            int key = GetTabKey(i);
            InitTab(container.controlList[i], key);
        }

        if (container.MaxCount > 0)
        {
            int key = GetTabKey(0);
            SelectTab(key);
            ToggleTab(key);
        }

        return true;
    }

    protected GameObject GetTabByKey(int key)
    {
        UIGridContainer container = GetGridCountainer();

        if (container != null)
        {
            foreach (var tab in GetContainer().GetComponent<UIGridContainer>().controlList)
            {
                if (tab.GetComponent<UIButtonEvent>().mData.get("TAB_KEY") == key)
                    return tab;
            }
        }

        return null;
    }

    private UIGridContainer GetGridCountainer()
    {
        GameObject containerObj = GetContainer();
        return containerObj == null ? null : containerObj.GetComponent<UIGridContainer>();
    }

    private void SelectTab(int key)
    {
        if (currentTabKey != -1 && currentTabKey == key)
            return;

        GameObject obj = GetTabByKey(key);

        if (obj != null)
        {       
            if(OnSelectTab(obj, key))
                currentTabKey = key;
        }
    }

    private void ToggleTab(int key)
    {
        GameObject obj = GetTabByKey(key);

        if (obj != null)
        {
            UIToggle toggle = obj.GetComponent<UIToggle>();

            if (toggle != null)
                toggle.value = true;
        }
    }
}


public class Button_tab_btn : CEvent
{
    public override bool _DoEvent()
    {
        string win = get("WIN_KEY");
        int key = get("TAB_KEY");
        DataCenter.SetData(win, "SELECT_TAB", key);
        return true;
    }
}

public class Button_TipButton : CEvent
{
	public override bool _DoEvent()
	{
		// skill click
//		object val;
//		bool b = getData("BUTTON", out val);
//		GameObject btn = val as GameObject;
//		
//		DataCenter.OpenWindow("COMMON_TIP_WINDOW");
//		DataCenter.SetData("COMMON_TIP_WINDOW", "INIT_WINDOW", btn);
		
		return true;
	}
}
