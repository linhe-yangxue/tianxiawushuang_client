using System;
using System.Collections.Generic;
using UnityEngine;
using DataTable;


public class ItemData : IComparable<ItemData>
{
    public int mNumber = 1;
    public int mType;
    public int mID;
    public int mEquipElement;
    public int mLevel = 1;
    public int mStrengthen = 0;

    public int CompareTo(ItemData other)
    {
        if (mType != other.mType)
        {
            return mType - other.mType;
        }
        else
        {
            switch (mType)
            {
                case (int)ITEM_TYPE.CHARACTER:
                case (int)ITEM_TYPE.PET_FRAGMENT:
                case (int)ITEM_TYPE.GEM:
                case (int)ITEM_TYPE.MATERIAL:
                case (int)ITEM_TYPE.MATERIAL_FRAGMENT:               
                case (int)ITEM_TYPE.CONSUME_ITEM:
                    return mID - other.mID;

                case (int)ITEM_TYPE.PET:
                    return mID == other.mID ? mLevel - other.mLevel : mID - other.mID;

                case (int)ITEM_TYPE.PET_EQUIP:
                case (int)ITEM_TYPE.EQUIP:
                case (int)ITEM_TYPE.MAGIC:
                    return mID == other.mID ? (mEquipElement == other.mEquipElement ? mStrengthen - other.mStrengthen : mEquipElement - other.mEquipElement) : mID - other.mID;

                default:
                    return 0;
            }
        }
    }
}


public interface IItemDataProvider
{
    int GetCount();
    ItemData GetItem(int index);
    void Append(ItemData item);
    int Remove(Predicate<ItemData> match);
    void Clear();
}


public class ItemDataProvider : IItemDataProvider
{
    private List<ItemData> itemList = new List<ItemData>();

    public ItemDataProvider()
    { }

    public ItemDataProvider(ItemData itemData)
    {
        Append(itemData);
    }

    public ItemDataProvider(IEnumerable<ItemData> itemDatas)
    {
        foreach (var item in itemDatas)
        {
            Append(item);
        }
    }
    //
    //public ItemDataProvider(int type, int id, int number)
    //    : this(type, id, number, 0)
    //{ }
    //
    //public ItemDataProvider(int type, int id, int number, int equipElement)
    //{
    //    Append(new ItemData() { mType = type, mID = id, mNumber = number, mEquipElement = equipElement });
    //}

    public ItemDataProvider(IEnumerable<DataRecord> records, string typeName, string idName)
    {
        foreach (var record in records)
        {
            ItemData data = new ItemData { mType = record[typeName], mID = record[idName], mNumber = 1 };
            Append(data);
        }
    }

    public ItemDataProvider(IEnumerable<DataRecord> records, string typeName, string idName, string numberName)
    {
        foreach (var record in records)
        {
            ItemData data = new ItemData { mType = record[typeName], mID = record[idName], mNumber = record[numberName] };
            Append(data);
        }
    }

    public ItemDataProvider(IEnumerable<DataRecord> records, string typeName, string idName, string numberName, string equipElementName)
    {
        foreach (var record in records)
        {
            ItemData data = new ItemData { mType = record[typeName], mID = record[idName], mNumber = record[numberName], mEquipElement = record[equipElementName] };
            Append(data);
        }
    }

    public ItemDataProvider(IEnumerable<ItemDataBase> items)
    {
        foreach (var it in items)
        {
            ItemData data = new ItemData();
            data.mID = it.tid;
            data.mNumber = it.itemNum;
            data.mType = (int)PackageManager.GetItemTypeByTableID(it.tid);
            Append(data);
        }
    }

    public ItemDataProvider(NiceTable table, string typeName, string idName)
        : this(table.GetAllRecord().Values, typeName, idName)
    { }

    public ItemDataProvider(NiceTable table, string typeName, string idName, string numberName)
        : this(table.GetAllRecord().Values, typeName, idName, numberName)
    { }

    public ItemDataProvider(NiceTable table, string typeName, string idName, string numberName, string equipElementName)
        : this(table.GetAllRecord().Values, typeName, idName, numberName, equipElementName)
    { }

    public int GetCount()
    {
        return itemList.Count;
    }

    public ItemData GetItem(int index)
    {
        return itemList[index];
    }

    public void Append(ItemData data)
    {
        if (data.mNumber == 0 || data.mType == (int)ITEM_TYPE.AIR)
        {
            return;
        }
        
        //ItemData d = itemList.Find(x => data.CompareTo(x) == 0);
        //
        //if (d == null)
        //{
        //    itemList.Add(data);
        //}
        //else
        //{
        //    d.mNumber += data.mNumber;
        //}

        // 同类数据不合并，直接追加至列表尾部
        itemList.Add(data);
    }

    public int Remove(Predicate<ItemData> match)
    {
        return itemList.RemoveAll(match);
    }

    public void Clear()
    {
        itemList.Clear();
    }
}


// 默认Icon布局，邮件及通用奖励展示窗口采用此布局
public class ItemIcon
{
    private GameObject icon;

    public ItemIcon(GameObject icon)
    {
        this.icon = icon;
    }

    public void Set(ItemData data)
    {
        GameCommon.SetUIText(icon, "count_label", "x" + data.mNumber.ToString());

        switch ((ITEM_TYPE)data.mType)
        {
            case ITEM_TYPE.PET:
                {                 
                    GameCommon.SetUIVisiable(icon, "stars_grid", true);
                    GameCommon.SetUIVisiable(icon, "level_label", true);
                    int starLevel = TableCommon.GetNumberFromActiveCongfig(data.mID, "STAR_LEVEL");
                    UISprite main = GameCommon.FindComponent<UISprite>(icon, "item_icon");
                    GameCommon.SetPetIcon(main, data.mID);
                    SetStars(starLevel);
                    GameCommon.SetUIText(icon, "level_label", "Lv." + data.mLevel);
                }
                break;

            case ITEM_TYPE.PET_FRAGMENT:
                {
                    GameCommon.SetUIVisiable(icon, "element", true);
                    GameCommon.SetUIVisiable(icon, "stars_grid", true);
                    GameCommon.SetUIVisiable(icon, "fragment_sprite", true);
                    int fragmentPetID = TableCommon.GetNumberFromFragment(data.mID, "ITEM_ID");
                    int elementIndex = TableCommon.GetNumberFromActiveCongfig(fragmentPetID, "ELEMENT_INDEX");
                    int starLevel = TableCommon.GetNumberFromActiveCongfig(fragmentPetID, "STAR_LEVEL");
                    UISprite main = GameCommon.FindComponent<UISprite>(icon, "item_icon");
                    UISprite element = GameCommon.FindComponent<UISprite>(icon, "element");
                    GameCommon.SetPetIcon(main, fragmentPetID);
                    GameCommon.SetElementIcon(element, elementIndex);
                    GameCommon.SetElementFragmentIcon(icon, "fragment_sprite", elementIndex);
                    SetStars(starLevel);
                }
                break;

            case ITEM_TYPE.EQUIP:
            case ITEM_TYPE.MAGIC:
                {
                    GameCommon.SetUIVisiable(icon, "role_equip_element", true);
                    GameCommon.SetUIVisiable(icon, "element", true);
                    GameCommon.SetUIVisiable(icon, "stars_grid", true);                  
                    GameCommon.SetEquipElementBgIcons(icon, "element", "role_equip_element", data.mID, data.mEquipElement);                   
                    GameCommon.SetEquipIcon(icon, data.mID, "item_icon");
                    int starLevel = TableCommon.GetNumberFromRoleEquipConfig(data.mID, "STAR_LEVEL");
                    SetStars(starLevel);

                    if (data.mStrengthen > 0)
                    {
                        GameCommon.SetUIVisiable(icon, "level_label", true);
                        GameCommon.SetUIText(icon, "level_label", "+" + data.mStrengthen);
                    }
                }
                break;
            default:
                {
                    UISprite itemSprite = GameCommon.FindObject(icon, "item_icon").GetComponent<UISprite>();
//                    GameCommon.SetItemIcon(itemSprite, data.mType, data.mID);
			GameCommon.SetOnlyItemIcon (GameCommon.FindObject(icon, "item_icon"), data.mID);
                }
                break;
        }
    }

    public void Reset()
    {
        GameCommon.SetUIVisiable(icon, "element", false);
        GameCommon.SetUIVisiable(icon, "star_level_label", false);
        GameCommon.SetUIVisiable(icon, "fragment_sprite", false);
        GameCommon.SetUIVisiable(icon, "role_equip_element", false);
        GameCommon.SetUIVisiable(icon, "level_label", false);
        GameCommon.SetUIVisiable(icon, "stars_grid", false);
    }

    private void SetStars(int count)
    {
        GameObject grid = GameCommon.FindObject(icon, "stars_grid");

        if (grid != null)
        {
            Vector3 pos = grid.transform.localPosition;
            UIGridContainer container = grid.GetComponent<UIGridContainer>();
            container.MaxCount = count;
            grid.transform.localPosition = new Vector3((1 - count) * container.CellWidth / 2f, pos.y, pos.z);
        }
    }
}


public class MonsterIcon
{
    private GameObject icon;

    public MonsterIcon(GameObject icon)
    {
        this.icon = icon;
    }

    public void Set(int index)
    {
        GameCommon.SetUIVisiable(icon, "stars_grid", true);
        GameCommon.SetUIVisiable(icon, "level_label", true);
        DataRecord r = DataCenter.mMonsterObject.GetRecord(index);
        int starLevel = r["STAR_LEVEL"];
        string atlas = r["HEAD_ATLAS_NAME"];
        string sprite = r["HEAD_SPRITE_NAME"];
        UISprite main = GameCommon.FindComponent<UISprite>(icon, "item_icon");
        GameCommon.SetIcon(main, atlas, sprite);
        SetStars(starLevel);
    }

    public void Reset()
    {
        GameCommon.SetUIVisiable(icon, "element", false);
        GameCommon.SetUIVisiable(icon, "star_level_label", false);
        GameCommon.SetUIVisiable(icon, "fragment_sprite", false);
        GameCommon.SetUIVisiable(icon, "role_equip_element", false);
        GameCommon.SetUIVisiable(icon, "level_label", false);
        GameCommon.SetUIVisiable(icon, "stars_grid", false);
    }

    private void SetStars(int count)
    {
        GameObject grid = GameCommon.FindObject(icon, "stars_grid");

        if (grid != null)
        {
            Vector3 pos = grid.transform.localPosition;
            UIGridContainer container = grid.GetComponent<UIGridContainer>();
            container.MaxCount = count;
            grid.transform.localPosition = new Vector3((1 - count) * container.CellWidth / 2f, pos.y, pos.z);
        }
    }
}


public class ItemGrid
{
    private UIGridContainer container;

    public ItemGrid(UIGridContainer container)
    {
        this.container = container;
    }

    public void Reset()
    {
        container.MaxCount = 0;
    }

    public void Set(IItemDataProvider provider)
    {
        int count = provider.GetCount();
        container.MaxCount = count;

        for (int i = 0; i < count; ++i)
        {
            GameObject _iconObj = container.controlList[i];
            ItemIcon icon = new ItemIcon(_iconObj);
            icon.Reset();
            //icon.Set(provider.GetItem(i));
            // added by xuke begin
            ItemData _item = provider.GetItem(i);
            GameCommon.SetUIText(_iconObj, "count_label", "x" + _item.mNumber.ToString());
            GameCommon.SetOnlyItemIcon(GameCommon.FindObject(_iconObj, "item_icon"), _item.mID);
			GameCommon.AddButtonAction (GameCommon.FindObject (_iconObj, "item_icon"), () => GameCommon.SetAccountItemDetailsWindow (_item.mID));
            // end
        }
    }
}