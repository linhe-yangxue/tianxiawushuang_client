using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable;
using System.Collections.Generic;
using Utilities;
using System.Threading;

public class ShopAlbumWindow : tWindow
{
    List<int> fireID = new List<int>();
    List<int> waterID = new List<int>();
    List<int> leafID = new List<int>();
    List<int> shadowID = new List<int>();
    List<int> lightID = new List<int>();
    Dictionary<int, int> mPetQualityMap = new Dictionary<int, int>();

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_shop_album_window_close", new DefineFactory<Button_shop_album_window_close>());
        EventCenter.Self.RegisterEvent("Button_fire", new DefineFactory<Button_fire>());
        EventCenter.Self.RegisterEvent("Button_water", new DefineFactory<Button_water>());
        EventCenter.Self.RegisterEvent("Button_leaf", new DefineFactory<Button_leaf>());
        EventCenter.Self.RegisterEvent("Button_shadow", new DefineFactory<Button_shadow>());
        EventCenter.Self.RegisterEvent("Button_light", new DefineFactory<Button_light>());
    }

    public override void Open(object param)
    {
        base.Open(param);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if (keyIndex == "OPEN")
        {
            getList(objVal.ToString());
            refreshUI("fire");
            InitUIToogle();
        }
        else if (keyIndex == "CHANGE_POS")
        {
            refreshUI(objVal.ToString());
        }        
    }

    public void InitUIToogle()
    {
		GameObject obj = GetSub("fire");
        obj.GetComponent<UIToggle>().value = true;
    }

    void getList(string price)
    {
       fireID.Clear();
       waterID.Clear();
       leafID.Clear();
       shadowID.Clear();
       lightID.Clear();
       int groupID = 0;
       Dictionary<int, DataRecord> mRecordMap = DataCenter.mStageLootGroupIDConfig.GetAllRecord();
       List<int> mpetsID = new List<int>();

       if(price=="CHEAP")
       {
           groupID = DataCenter.mPumpingConfig.GetData(4, "SHOW_GROUP_ID");
       }
       else if (price == "EXPENSIVE")
       {
           groupID = DataCenter.mPumpingConfig.GetData(2, "SHOW_GROUP_ID");
       }
       foreach (KeyValuePair<int, DataRecord> r in mRecordMap)
       {
           if (r.Value.get("GROUP_ID") == groupID)
           {
               if (!mpetsID.Contains(r.Value.get("ITEM_ID")))
               {
                   mpetsID.Add(r.Value.get("ITEM_ID"));
               }           
           }
       }
       foreach (int id in mpetsID)
       {                  
           int type = DataCenter.mActiveConfigTable.GetData(id, "ELEMENT_INDEX");
           int quality = DataCenter.mActiveConfigTable.GetData(id, "STAR_LEVEL");
           if (!mPetQualityMap.ContainsKey(id))
           {
               mPetQualityMap.Add(id, quality);
           }
           switch (type)
           {
               case 0:
                   fireID.Add(id);
                   break;
               case 1:
                   waterID.Add(id);
                   break;
               case 2:
                   leafID.Add(id);
                   break;
               case 4:
                   shadowID.Add(id);
                   break;
               case 3:
                   lightID.Add(id);
                   break;
           }
       }

       fireID.Sort((a, b) =>
        {
            if (mPetQualityMap[a] > mPetQualityMap[b]) return -1;
            else if (mPetQualityMap[a] < mPetQualityMap[b]) return 1;
            else return 0;
        });
       waterID.Sort((a, b) =>
       {
           if (mPetQualityMap[a] > mPetQualityMap[b]) return -1;
           else if (mPetQualityMap[a] < mPetQualityMap[b]) return 1;
           else return 0;
       });
       leafID.Sort((a, b) =>
       {
           if (mPetQualityMap[a] > mPetQualityMap[b]) return -1;
           else if (mPetQualityMap[a] < mPetQualityMap[b]) return 1;
           else return 0;
       });
       shadowID.Sort((a, b) =>
       {
           if (mPetQualityMap[a] > mPetQualityMap[b]) return -1;
           else if (mPetQualityMap[a] < mPetQualityMap[b]) return 1;
           else return 0;
       });
       lightID.Sort((a, b) =>
       {
           if (mPetQualityMap[a] > mPetQualityMap[b]) return -1;
           else if (mPetQualityMap[a] < mPetQualityMap[b]) return 1;
           else return 0;
       });
    }

    void refreshUI(string kind)
    {
        UIGridContainer grid = GetComponent<UIGridContainer>("grid");
        switch (kind)
        {
            case "fire":
                setGrid(fireID, grid);
                break;
            case "water":               
                setGrid(waterID, grid);         
                break;
            case "leaf":
                setGrid(leafID, grid);           
                break;
            case "shadow":
                setGrid(shadowID, grid); 
                break;
            case "light":
                setGrid(lightID, grid);
                break;
        }
        UIScrollView mScrollView = GetComponent<UIScrollView>("ScrollView");
        mScrollView.ResetPosition();
    }

    void setGrid(List<int> temp, UIGridContainer grid)
    {
        if (temp.Count != 0)
        {
            grid.MaxCount = temp.Count;
            for (int i = 0; i < temp.Count; i++)
            {
                GameObject item = grid.controlList[i];
                GameCommon.SetItemIconNew(item, "itemIcon", temp[i]);
                GameObject icon = GameCommon.FindObject(item, "itemIcon");
                int mtid = temp[i];
                AddButtonAction(icon, () => DataCenter.OpenWindow(UIWindowString.petDetail, mtid));
            }
        }
        else
        {
            grid.MaxCount = 0;
        }
    }

}

public class Button_shop_album_window_close : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("SHOP_ALBUM_WINDOW");
        return true;
    }
}

public class Button_fire : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("SHOP_ALBUM_WINDOW","CHANGE_POS","fire");
        return true;
    }
}
public class Button_water : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("SHOP_ALBUM_WINDOW", "CHANGE_POS", "water");
        return true;
    }
}
public class Button_leaf : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("SHOP_ALBUM_WINDOW", "CHANGE_POS", "leaf");
        return true;
    }
}
public class Button_shadow : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("SHOP_ALBUM_WINDOW", "CHANGE_POS", "shadow");
        return true;
    }
}
public class Button_light : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("SHOP_ALBUM_WINDOW", "CHANGE_POS", "light");
        return true;
    }
}
