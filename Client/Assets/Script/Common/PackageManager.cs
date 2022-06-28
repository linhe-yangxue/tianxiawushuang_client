using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;

public class PackageManager
{
    static public ITEM_TYPE GetItemTypeByTableID(int tid)
    {
        int iType = tid / 1000;
        if ((int)ITEM_TYPE.ROLE_ATTRIBUTE == iType)
            return (ITEM_TYPE)tid;

        if (iType >= 70 && iType <= 79)
        {
            iType = 70;
        }
        return (ITEM_TYPE)iType;
    }
    //by chenliang
    //begin

    /// <summary>
    /// 仅除以1000获取物品类型
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public static ITEM_TYPE GetItemRealTypeByTableID(int tid)
    {
        int iType = tid / 1000;

        // 怪物
        if (iType >= 70 && iType <= 79)
        {
            return ITEM_TYPE.MONSTER;
        }
        // 技能
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_ACTIVE && iType < (int)ITEM_TYPE.SKILL_TYPE_PASSIVE)
            return ITEM_TYPE.SKILL_TYPE_ACTIVE;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_PASSIVE && iType < (int)ITEM_TYPE.SKILL_TYPE_BUFF_PASSIVE)
            return ITEM_TYPE.SKILL_TYPE_PASSIVE;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_BUFF_PASSIVE && iType < (int)ITEM_TYPE.SKILL_TYPE_BUFF_BREAK)
            return ITEM_TYPE.SKILL_TYPE_BUFF_PASSIVE;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_BUFF_BREAK && iType < (int)ITEM_TYPE.SKILL_TYPE_BUFF_EQUIP)
            return ITEM_TYPE.SKILL_TYPE_BUFF_BREAK;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_BUFF_EQUIP && iType < (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_1)
            return ITEM_TYPE.SKILL_TYPE_BUFF_EQUIP;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_1 && iType < (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_2)
            return ITEM_TYPE.SKILL_TYPE_BUFF_EXT_1;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_2 && iType < (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_3)
            return ITEM_TYPE.SKILL_TYPE_BUFF_EXT_2;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_3 && iType < (int)ITEM_TYPE.SKILL_TYPE_BUFF_RELATIONSHIP)
            return ITEM_TYPE.SKILL_TYPE_BUFF_EXT_3;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_BUFF_RELATIONSHIP && iType < (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_4)
            return ITEM_TYPE.SKILL_TYPE_BUFF_RELATIONSHIP;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_4 && iType < (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_5)
            return ITEM_TYPE.SKILL_TYPE_BUFF_EXT_4;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_5 && iType < (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_6)
            return ITEM_TYPE.SKILL_TYPE_BUFF_EXT_5;
        else if (iType >= (int)ITEM_TYPE.SKILL_TYPE_BUFF_EXT_6 && iType < 400)
            return ITEM_TYPE.SKILL_TYPE_BUFF_EXT_6;

        return (ITEM_TYPE)iType;
    }

    /// <summary>
    /// 根据物品获取背包类型
    /// </summary>
    /// <returns></returns>
    public static PACKAGE_TYPE GetPackageTypeByItemTid(int itemTid)
    {
        PACKAGE_TYPE tmpPackageType = PACKAGE_TYPE.MAX;
        ITEM_TYPE tmpItemType = PackageManager.GetItemTypeByTableID(itemTid);
        switch (tmpItemType)
        {
            case ITEM_TYPE.PET: tmpPackageType = PACKAGE_TYPE.PET; break;
            case ITEM_TYPE.PET_FRAGMENT: tmpPackageType = PACKAGE_TYPE.PET_FRAGMENT; break;
            case ITEM_TYPE.EQUIP: tmpPackageType = PACKAGE_TYPE.EQUIP; break;
            case ITEM_TYPE.EQUIP_FRAGMENT: tmpPackageType = PACKAGE_TYPE.EQUIP_FRAGMENT; break;
            case ITEM_TYPE.MAGIC: tmpPackageType = PACKAGE_TYPE.MAGIC; break;
            case ITEM_TYPE.MAGIC_FRAGMENT: tmpPackageType = PACKAGE_TYPE.MAGIC_FRAGMENT; break;
        }
        return tmpPackageType;
    }
    /// <summary>
    /// 获取背包类型
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static List<PACKAGE_TYPE> GetPackageTypes(IEnumerable<ItemDataBase> items)
    {
        List<PACKAGE_TYPE> tmpTypes = new List<PACKAGE_TYPE>();
        foreach (ItemDataBase tmpItem in items)
            tmpTypes.Add(GetPackageTypeByItemTid(tmpItem.tid));
        return tmpTypes;
    }

    /// <summary>
    /// 是否有背包满了
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public static bool IsAnyPackageFull(IEnumerable<PACKAGE_TYPE> types)
    {
        if (types == null)
            return false;
        foreach(PACKAGE_TYPE tmpType in types)
        {
            if (IsPackageFull(tmpType))
                return true;
        }
        return false;
    }
    /// <summary>
    /// 获取已经满的背包
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public static List<PACKAGE_TYPE> GetFullPackage(IEnumerable<PACKAGE_TYPE> types)
    {
        List<PACKAGE_TYPE> tmpRet = new List<PACKAGE_TYPE>();
        if (types == null)
            return tmpRet;
        foreach (PACKAGE_TYPE tmpType in types)
        {
            if (IsPackageFull(tmpType))
                tmpRet.Add(tmpType);
        }
        return tmpRet;
    }

    //end

    static public int GetSlotPosByTid(int tid)
    {
        return tid / 100 - tid / 1000 * 10;
    }

    // 是否是经验法器
    static public bool IsExpMagic(int tid)
    {
        return tid / 100 == 152;
    }


    static public void AddItem(int tid, int itemId, int count)
    {
        ItemDataBase idb = new ItemDataBase();
        idb.tid = tid;
        idb.itemId = itemId;
        idb.itemNum = count;
        AddItem(idb);
        DEBUG.Log("加物品成功！ itemId = " + itemId + ", tId = " + tid + ", count = " + count);
    }

    static public void AddItem(ItemDataBase itemData)
    {
        if (null == itemData)
        {
            return;
        }

        switch (GetItemTypeByTableID(itemData.tid))
        {
            case ITEM_TYPE.PET:
                PetLogicData.Self.AddItemData(new PetData(itemData));
                break;
            case ITEM_TYPE.PET_FRAGMENT:
                PetFragmentLogicData.Self.AddItemData(itemData);
                break;
            case ITEM_TYPE.EQUIP:
                RoleEquipLogicData.Self.AddItemData(new EquipData(itemData));
                break;
            case ITEM_TYPE.EQUIP_FRAGMENT:
                RoleEquipFragmentLogicData.Self.AddItemData(itemData);
                break;
            case ITEM_TYPE.MAGIC:
                MagicLogicData.Self.AddItemData(new EquipData(itemData));
                break;
            case ITEM_TYPE.MAGIC_FRAGMENT:
                MagicFragmentLogicData.Self.AddItemData(itemData);
                break;
            case ITEM_TYPE.CONSUME_ITEM:
                ConsumeItemLogicData.Self.AddItemData(itemData);
                break;
            case ITEM_TYPE.YUANBAO:
            case ITEM_TYPE.GOLD:
            case ITEM_TYPE.POWER:
            case ITEM_TYPE.PET_SOUL:
            case ITEM_TYPE.SPIRIT:
            case ITEM_TYPE.REPUTATION:
            case ITEM_TYPE.PRESTIGE:
            case ITEM_TYPE.BATTLEACHV:
            case ITEM_TYPE.UNIONCONTR:
            case ITEM_TYPE.BEATDEMONCARD:

            //case ITEM_TYPE.SAODANG_POINT:
            case ITEM_TYPE.RESET_POINT:
            case ITEM_TYPE.LOCK_POINT:
            case ITEM_TYPE.CHARACTER_EXP:
                GameCommon.RoleChangeNumericalAboutRole((int)GetItemTypeByTableID(itemData.tid), itemData.itemNum);
                break;

        }
    }

    static public void AddItem(IEnumerable<ItemDataBase> itemDataList)
    {
        foreach (ItemDataBase item in itemDataList)
        {
            AddItem(item);
        }
    }

    static public void RemoveItem(int tid, int itemId, int count)
    {
        ItemDataBase idb = new ItemDataBase();
        idb.tid = tid;
        idb.itemId = itemId;
        idb.itemNum = count;
        RemoveItem(idb);
        DEBUG.Log("减物品成功！");
    }

    static public void RemoveItem(ItemDataBase itemData)
    {
        if (null == itemData)
        {
            return;
        }

        ItemDataBase ChangeItemData = new ItemDataBase();
        ChangeItemData.itemId = itemData.itemId;
        ChangeItemData.tid = itemData.tid;
        ChangeItemData.itemNum = itemData.itemNum * (-1);
        switch (GetItemTypeByTableID(ChangeItemData.tid))
        {
            case ITEM_TYPE.UNIONCONTR:
                RoleLogicData.Self.unionContr -= itemData.itemNum;
                break;
            case ITEM_TYPE.PET:
                PetLogicData.Self.RemoveItemData(ChangeItemData.itemId);
                break;
            case ITEM_TYPE.PET_FRAGMENT:
                PetFragmentLogicData.Self.ChangeItemDataNum(ChangeItemData);
                break;
            case ITEM_TYPE.EQUIP:
                RoleEquipLogicData.Self.RemoveItemData(ChangeItemData.itemId);
                break;
            case ITEM_TYPE.EQUIP_FRAGMENT:
                RoleEquipFragmentLogicData.Self.ChangeItemDataNum(ChangeItemData);
                break;
            case ITEM_TYPE.MAGIC:
                MagicLogicData.Self.RemoveItemData(ChangeItemData.itemId);
                break;
            case ITEM_TYPE.MAGIC_FRAGMENT:
                MagicFragmentLogicData.Self.ChangeItemDataNum(ChangeItemData);
                break;
            case ITEM_TYPE.NOVICE_PACKS:
            case ITEM_TYPE.CONSUME_ITEM:
                ConsumeItemLogicData.Self.ChangeItemDataNum(ChangeItemData);
                break;
            case ITEM_TYPE.YUANBAO:
            case ITEM_TYPE.GOLD:
            case ITEM_TYPE.POWER:
            case ITEM_TYPE.PET_SOUL:
            case ITEM_TYPE.SPIRIT:
            case ITEM_TYPE.REPUTATION:
            case ITEM_TYPE.PRESTIGE:
            case ITEM_TYPE.BATTLEACHV:
            case ITEM_TYPE.BEATDEMONCARD:

            //case ITEM_TYPE.SAODANG_POINT:
            case ITEM_TYPE.RESET_POINT:
            case ITEM_TYPE.LOCK_POINT:
            case ITEM_TYPE.CHARACTER_EXP:
                GameCommon.RoleChangeNumericalAboutRole((int)GetItemTypeByTableID(ChangeItemData.tid), ChangeItemData.itemNum);
                break;
        }
    }

    static public void RemoveItem(IEnumerable<ItemDataBase> itemDataList)
    {
        foreach (ItemDataBase itemData in itemDataList)
        {
            RemoveItem(itemData);
        }
    }

    static public ItemDataBase UpdateItem(int tid, int itemId, int count)
    {
        ItemDataBase idb = new ItemDataBase();
        idb.tid = tid;
        idb.itemId = itemId;
        idb.itemNum = count;
        return UpdateItem(idb);
        DEBUG.Log("更新物品成功！");
    }

    /// <summary>
    /// 刷新背包数据，返回对象为拷贝对象
    /// </summary>
    /// <param name="itemData">数据对象</param>
    /// <returns>拷贝对象</returns>
    static public ItemDataBase UpdateItem(ItemDataBase itemData)
    {
        if (null == itemData)
        {
            return itemData;
        }

        ItemDataBase ChangeItemData = new ItemDataBase();
        ChangeItemData.itemId = itemData.itemId;
        ChangeItemData.tid = itemData.tid;
        switch (GetItemTypeByTableID(itemData.tid))
        {
            case ITEM_TYPE.PET:
                PetData petData = PetLogicData.Self.GetPetDataByItemId(itemData.itemId);
                ChangeItemData.itemNum = ChangeItemDataNum(itemData, petData);

                PetLogicData.Self.UpdateItemData(itemData);
                TeamManager.mIsRelateDirty = true;
                return ChangeItemData;
            case ITEM_TYPE.PET_FRAGMENT:
                PetFragmentData petFragmentData = PetFragmentLogicData.Self.GetItemDataByTid(itemData.tid);
                ChangeItemData.itemNum = ChangeItemDataNum(itemData, petFragmentData);

                PetFragmentLogicData.Self.UpdateItemData(itemData);
                return ChangeItemData;
            case ITEM_TYPE.EQUIP:
                EquipData equipData = RoleEquipLogicData.Self.GetEquipDataByItemId(itemData.itemId);
                ChangeItemData.itemNum = ChangeItemDataNum(itemData, equipData);

                RoleEquipLogicData.Self.UpdateItemData(itemData);
                return ChangeItemData;
            case ITEM_TYPE.EQUIP_FRAGMENT:
                //RoleEquipFragmentLogicData.Self.UpdateItemData(itemData);

                EquipFragmentData equipFragmentData = RoleEquipFragmentLogicData.Self.GetItemDataByTid(itemData.tid);
                ChangeItemData.itemNum = ChangeItemDataNum(itemData, equipFragmentData);

                RoleEquipFragmentLogicData.Self.UpdateItemData(itemData);
                return ChangeItemData;
            case ITEM_TYPE.MAGIC:
                EquipData magicData = MagicLogicData.Self.GetEquipDataByItemId(itemData.itemId);
                ChangeItemData.itemNum = ChangeItemDataNum(itemData, magicData);

                MagicLogicData.Self.UpdateItemData(itemData);
                return ChangeItemData;
            case ITEM_TYPE.MAGIC_FRAGMENT:
                EquipFragmentData magicFragmentData = MagicFragmentLogicData.Self.GetItemDataByTid(itemData.tid);
                ChangeItemData.itemNum = ChangeItemDataNum(itemData, magicFragmentData);

                MagicFragmentLogicData.Self.UpdateItemData(itemData);
                return ChangeItemData;
            case ITEM_TYPE.NOVICE_PACKS:
            case ITEM_TYPE.CONSUME_ITEM:
                ConsumeItemData consumeItemData = ConsumeItemLogicData.Self.GetDataByTid(itemData.tid);
                ChangeItemData.itemNum = ChangeItemDataNum(itemData, consumeItemData);

                ConsumeItemLogicData.Self.UpdateItemData(itemData);
                return ChangeItemData;
            case ITEM_TYPE.YUANBAO:
            case ITEM_TYPE.GOLD:
            case ITEM_TYPE.POWER:
            case ITEM_TYPE.PET_SOUL:
            case ITEM_TYPE.SPIRIT:
            case ITEM_TYPE.REPUTATION:
            case ITEM_TYPE.PRESTIGE:
            case ITEM_TYPE.BATTLEACHV:
            case ITEM_TYPE.UNIONCONTR:
            case ITEM_TYPE.BEATDEMONCARD:
            case ITEM_TYPE.SAODANG_POINT:
            case ITEM_TYPE.RESET_POINT:
            case ITEM_TYPE.LOCK_POINT:
            case ITEM_TYPE.CHARACTER_EXP:
                if (itemData.itemNum > 0)
                {
                    AddItem(itemData);
                }
                else
                {
                    RemoveItem(itemData);
                }
                break;
        }

        return itemData;
    }

    /// <summary>
    /// 刷新背包数据
    /// </summary>
    /// <param name="itemDataList">可枚举数据列表</param>
    /// <returns>拷贝列表</returns>
    static public List<ItemDataBase> UpdateItem(IEnumerable<ItemDataBase> itemDataList, bool isMerge = false)
    {
        List<ItemDataBase> dItemDataList = new List<ItemDataBase>();
        foreach (ItemDataBase item in itemDataList)
        {
            ItemDataBase itemData = UpdateItem(item);
            if (itemData != null)
            {
                dItemDataList.Add(itemData);
            }
        }
        if (isMerge)
        {
            List<ItemDataBase> itemRet = new List<ItemDataBase>();
            GameCommon.MergeItemDataBase(dItemDataList, out itemRet);
            return itemRet;
        }
        
        return dItemDataList;
    }

    /// <summary>
    /// 增量差值
    /// </summary>
    /// <param name="curItemData">现有物品数据</param>
    /// <param name="srcItemData">之前物品数据</param>
    /// <returns></returns>
    static private int ChangeItemDataNum(ItemDataBase curItemData, ItemDataBase srcItemData)
    {
        if (curItemData != null)
        {
            if (srcItemData != null)
            {
                return curItemData.itemNum - srcItemData.itemNum;                
            }
            else
            {
                return curItemData.itemNum;
            }
        }

        return 0;
    }

    /// <summary>
    /// 背包是否已满——任意背包已满，即为true
    /// </summary>
    /// <returns></returns>
    static public bool IsPackageFull()
    {
        // 获取包裹数据
        for (int i = (int)PACKAGE_TYPE.PET; i < (int)PACKAGE_TYPE.MAX; ++i)
        {
            if (IsPackageFull((PACKAGE_TYPE)i))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 指定背包是否已满
    /// </summary>
    /// <param name="packageType">背包类型</param>
    /// <returns></returns>
    static public bool IsPackageFull(PACKAGE_TYPE packageType)
    {
        switch ((PACKAGE_TYPE)packageType)
        {
            case PACKAGE_TYPE.PET:
                if (PetLogicData.Self.mDicPetData.Count >= GetMaxPetPackageNum())
                {
                    return true;
                }
                break;
            case PACKAGE_TYPE.EQUIP:
			if (RoleEquipLogicData.Self.mDicEquip.Count >= GetMaxEquipPackageNum())
                {
                    return true;
                }
                break;
            case PACKAGE_TYPE.MAGIC:
                if (MagicLogicData.Self.mDicEquip.Count >= GetMaxMagicPackageNum())
                {
                    return true;
                }
                break;
        }
        return false;
    }
    /// <summary>
    /// 判断背包中是否有这个物品
    /// </summary>
    /// <returns></returns>
    static public bool IsHasItemByTid(int kTid) 
    {
        foreach(KeyValuePair<int,PetData> pair in PetLogicData.Self.mDicPetData)
        {
            if (pair.Value.tid == kTid)
                return true;
        }
        foreach (KeyValuePair<int, EquipData> pair in RoleEquipLogicData.Self.mDicEquip) 
        {
            if (pair.Value.tid == kTid)
                return true;
        }
        return false;   
    }

    static public bool IsHasEnoughItemByTid(int kTid,int num)
    {
        switch (GetItemTypeByTableID(kTid))
        {
            case ITEM_TYPE.CONSUME_ITEM:
            ConsumeItemLogicData consumeItemLogicData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
            if (consumeItemLogicData == null)
                return false;
            ConsumeItemData curConsumeItemData = consumeItemLogicData.GetDataByTid(kTid);
            return num <= curConsumeItemData.itemNum;
            case ITEM_TYPE.YUANBAO:
            return num <= RoleLogicData.Self.diamond;
            case ITEM_TYPE.GOLD:
            return num <= RoleLogicData.Self.gold;
            case ITEM_TYPE.POWER:
            return num <= RoleLogicData.Self.power;
            case ITEM_TYPE.PET_SOUL:
            return num <= RoleLogicData.Self.soulPoint;
            case ITEM_TYPE.SPIRIT:
            return num <= RoleLogicData.Self.spirit;
            case ITEM_TYPE.REPUTATION:
            return num <= RoleLogicData.Self.reputation;
            case ITEM_TYPE.PRESTIGE:
            return num <= RoleLogicData.Self.prestige;
            case ITEM_TYPE.BATTLEACHV:
            return num <= RoleLogicData.Self.battleAchv;
            case ITEM_TYPE.UNIONCONTR:
            return num <= RoleLogicData.Self.unionContr;
        }
        return false;
        
    }


    static public int GetMaxPetPackageNum()
    {
        return DataCenter.mVipListConfig.GetData(RoleLogicData.Self.vipLevel, "PET_BAG_NUM");
    }
    static public int GetMaxEquipPackageNum()
    {
        return DataCenter.mVipListConfig.GetData(RoleLogicData.Self.vipLevel, "EQUIP_BAG_NUM");
    }
    static public int GetMaxMagicPackageNum()
    {
        return DataCenter.mVipListConfig.GetData(RoleLogicData.Self.vipLevel, "MAGIC_BAG_NUM");
    }

    /// <summary>
    /// 获取指定物品剩余拥有数量
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    static public int GetItemLeftCount(int tid)
    {
        ITEM_TYPE tmpType = PackageManager.GetItemTypeByTableID(tid);
        int itemCount = 0;
        switch (tmpType)
        {
            case ITEM_TYPE.PET: itemCount = PetLogicData.Self.GetCountByTid(tid); break;
            case ITEM_TYPE.PET_FRAGMENT: itemCount = PetFragmentLogicData.Self.GetCountByTid(tid); break;
            case ITEM_TYPE.EQUIP: itemCount = RoleEquipLogicData.Self.GetNumByIndex(tid); break;
            case ITEM_TYPE.MAGIC: itemCount = MagicLogicData.Self.GetNumByIndex(tid); break;
            case ITEM_TYPE.EQUIP_FRAGMENT: itemCount = RoleEquipFragmentLogicData.Self.GetCountByTid(tid); break;
            case ITEM_TYPE.MAGIC_FRAGMENT: itemCount = MagicFragmentLogicData.Self.GetCountByTid(tid); break;
            case ITEM_TYPE.CONSUME_ITEM: itemCount = ConsumeItemLogicData.Self.GetCountByID(tid); break;

            case ITEM_TYPE.YUANBAO: itemCount = RoleLogicData.Self.diamond; break;
            case ITEM_TYPE.GOLD: itemCount = RoleLogicData.Self.gold; break;
            case ITEM_TYPE.POWER: itemCount = RoleLogicData.Self.power; break;
            case ITEM_TYPE.SPIRIT: itemCount = RoleLogicData.Self.spirit; break;
            case ITEM_TYPE.HONOR_POINT: itemCount = RoleLogicData.Self.mHonorPoint; break;
            case ITEM_TYPE.SAODANG_POINT: itemCount = RoleLogicData.Self.mSweepNum; break;
            case ITEM_TYPE.RESET_POINT: itemCount = RoleLogicData.Self.mResetNum; break;
            case ITEM_TYPE.LOCK_POINT: itemCount = RoleLogicData.Self.mLockNum; break;
            case ITEM_TYPE.PET_SOUL: itemCount = RoleLogicData.Self.soulPoint; break;
            case ITEM_TYPE.PRESTIGE: itemCount = RoleLogicData.Self.prestige; break;
            case ITEM_TYPE.UNIONCONTR: itemCount = RoleLogicData.Self.unionContr; break;
            case ITEM_TYPE.BATTLEACHV: itemCount = RoleLogicData.Self.battleAchv; break;
            case ITEM_TYPE.BEATDEMONCARD: itemCount = RoleLogicData.Self.beatDemonCard; break;
            case ITEM_TYPE.REPUTATION: itemCount = RoleLogicData.Self.reputation; break;
            default:
                DEBUG.LogError("不存在(tid ：" + tid + "): " + GameCommon.GetItemName(tid));
                break;
        }
        return itemCount;
    }
}
