using UnityEngine;
using System.Collections;
using DataTable;
using Logic;

public abstract class Item 
{
	public int mType;
	public int mCount;
	public DataRecord	mConfig;

	public abstract void Use(int useCount);

	static public Item CreateItem(int type)
	{
		DataRecord re = DataCenter.mItemConfig.GetRecord(type);
		if (re!=null)
		{
			string itemType = re.get ("TYPE");
            switch (itemType)
            {
                case "CHEST":
                    {
                        Item newItem = new TreasureChest();
                        newItem.mConfig = re;
                        newItem.mType = type;
                        newItem.mCount = 0;
                        return newItem;
                    }
            }
        }
		else 
		{
			EventCenter.Log(LOG_LEVEL.ERROR, "Item config no exist>"+type.ToString());
		}

		return null;
	}
}

// 背包
public class ItemBag : tLogicData
{
	public void AddItem(int nType, int count)
	{
		Item item = getObject(nType.ToString()) as Item;
		if (item==null)
        {
            item = Item.CreateItem(nType);
			set(nType.ToString(), item);
        }
        item.mCount += count;
	}

	public bool UseItem(int nType, int count)
	{
        Item item = getObject(nType.ToString()) as Item;
		if (item!=null)
        {
            if (item.mCount>=count)
            {
                item.mCount -= count;
                item.Use(count);
                return true;
            }
        }    
        return false;
	}
}

// 宝箱
public class TreasureChest : Item
{
	public override void Use(int useCount)
	{
	}
}

// 装备 
public class Equip : Item
{
	public override void Use(int useCount)
	{
	}
}

// 武器
