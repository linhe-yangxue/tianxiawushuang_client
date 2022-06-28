using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DataTable;
using Logic;
using System;

public enum Evolution_Type
{
    NONE,
    GEM,
    CARD,
}

public class ItemDataBaseComparer<T>:IComparer where T:ItemDataBase
{
    readonly string[] keyArr;
    readonly NiceTable table;
    public ItemDataBaseComparer(NiceTable table, params string[] keyArr)
    {
        this.keyArr = keyArr;
        this.table = table;
    }

    public int Compare(object l, object r)
    {
        T lItem = l as T;
        T rItem = r as T;

        var lRecord=table.GetRecord(lItem.tid);
        var rRecord=table.GetRecord(rItem.tid);
        int result = 0;
        if (lRecord != null && rRecord != null)
        {
            for (int i = 0; i < keyArr.Length; i++)
            {
                var key = keyArr[i];
                int lValue = (int)lRecord.get(key);
                int rValue = (int)rRecord.get(key);
                result = lValue.CompareTo(rValue);
                if (result != 0) break;
            }
        }
        else
        {
            DEBUG.LogWarning("NiceTable is not exist");
            result = 0;
        } 
        
        return result;
    }

}
public class TableCommon
{
    static public DataRecord FindRecord(NiceTable table, Predicate<DataRecord> match)
    {
        foreach (var pair in table.GetAllRecord())
        {
            if (pair.Key > 0 && match(pair.Value))
                return pair.Value;
        }
        return null;
    }

    static public List<DataRecord> FindAllRecords(NiceTable table, Predicate<DataRecord> match)
    {
        List<DataRecord> result = new List<DataRecord>();

        foreach (var pair in table.GetAllRecord())
        {
            if (pair.Key > 0 && match(pair.Value))
                result.Add(pair.Value);
        }

        return result;
    }

    static public Dictionary<T, List<DataRecord>> GroupRecordsBy<T>(NiceTable table, Func<DataRecord, T> func)
    {
        Dictionary<T, List<DataRecord>> result = new Dictionary<T, List<DataRecord>>();

        foreach (var pair in table.GetAllRecord())
        {
            if (pair.Key > 0)
            {
                T key = func(pair.Value);

                if (!result.ContainsKey(key))
                    result.Add(key, new List<DataRecord>());

                result[key].Add(pair.Value);
            }
        }

        return result;
    }

    // ActiveConfig.csv
    public static string GetStringFromActiveCongfig(int iModelIndex, string strIndexName)
    {
        DataRecord dataRecord = null;
        dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);

        if (iModelIndex <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromActiveCongfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static int GetNumberFromActiveCongfig(int iModelIndex, string strIndexName)
    {
        DataRecord dataRecord = null;
        dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);

        if (iModelIndex <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromActiveCongfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static object GetObjectFromActiveCongfig(int iModelIndex, string strIndexName)
    {
        DataRecord dataRecord = null;
        dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);

        if (iModelIndex <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromActiveCongfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
            return 0;
        }
        return dataRecord.getObject(strIndexName);
    }

  

	//Get all the TujianData
	public static TujianData[] GetTujianDataListFromActiveConfig()
	{
		Dictionary<int, DataRecord> dataRecords = DataCenter.mActiveConfigTable.GetAllRecord();
		int iCount = 0;
		foreach (int dataRecord in dataRecords.Keys)
		{
			if(GetStringFromActiveCongfig(dataRecord, "CLASS") != "PET")
			{
				iCount++;
			}
		}
		TujianData[] tujians = new TujianData[dataRecords.Count - iCount];
		int index = -1;
		foreach (int dataRecord in dataRecords.Keys)
        {
			if(GetStringFromActiveCongfig(dataRecord, "CLASS") == "PET")
			{
	            index++;
	            TujianData data = new TujianData();
	            data.mID = dataRecord;
	            data.mStatus = -1;
	            tujians[index] = data;
			}
        }
        return tujians;
    }

	// getRammbockDataStruct
	public static List<RammbockShopData> GetRammbockInfo(int tabPage, out int maxTabs) {
		NiceTable t = TableManager.GetTable("TowerShopConfig");
		Dictionary<int, DataRecord> records = t.GetAllRecord();
		int maxTabId = 0;
		List<RammbockShopData> datas = new List<RammbockShopData>();
		RammbockShopData rammbockData = null;
        int roleLevel = RoleLogicData.GetMainRole().level;
		foreach (KeyValuePair<int, DataRecord> vRe in records)
		{
			DataRecord r = vRe.Value;
			int tabId = (int)r["TAB_ID"];
			if(tabId == tabPage) {
				rammbockData = new RammbockShopData();

				rammbockData.index = (int)r["INDEX"];
				rammbockData.tabName = (string)r["TAB_NAME"];
				rammbockData.tid = (int)r["ITEM_ID"];
				rammbockData.itemNum = (int)r["ITEM_NUM"];
				rammbockData.itemShowLevel = (int)r["ITEM_SHOW_LEVEL"];
				rammbockData.vipLevelDisplay = (int)r["VIP_LEVEL_DISPLAY"];
                int tmpType = (int)r["COST_TYPE_1"];
                if (tmpType != 0)
                {
                    rammbockData.costTid.Add(tmpType);
                    rammbockData.costNum.Add((int)r["COST_NUM_1"]);
                }
                tmpType = (int)r["COST_TYPE_2"];
                if (tmpType != 0)
                {
                    rammbockData.costTid.Add(tmpType);
                    rammbockData.costNum.Add((int)r["COST_NUM_2"]);
                }
				rammbockData.openStarNum = (int)r["OPEN_STAR_NUM"];
				rammbockData.buyNum = (int)r["BUY_NUM"];
                rammbockData.showLevel = (int)r["ITEM_SHOW_LEVEL"];

                if (roleLevel >= rammbockData.showLevel)
                { 
				    datas.Add(rammbockData);
                }
				//continue;
			}
			maxTabId = Mathf.Max(maxTabId, tabId);
		}
		maxTabs = maxTabId;
		return datas;
	}
	// endGetRammbockDataStruct

	// getRammbockTabName
	public static string[] GetRammbockTabName(int maxTab) {
		string[] names = new string[maxTab];
		int val = 0;
		for(int i=maxTab; i > 0; i--) {
			names[i-1] = GetRammbockInfo(i, out val)[0].tabName;
		}
		return names;
	}

	public static string[] GetRammbockTabName(int maxTab, int playLevel) {
		string[] names = new string[maxTab];
		int val = 0;
		int maxOpenLevel = 0;
		for(int i=maxTab; i > 0; i--) {
			// only need one of same group data
			string tempVal = GetRammbockInfo(i, out val)[0].tabName;
			if(tempVal.Contains("|")) {
				string[] splitVals = tempVal.Split(new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);
				string[] lateVals = splitVals[1].Split(new char[]{'#'}, StringSplitOptions.RemoveEmptyEntries);
				int maxLv = 0;
				int.TryParse(lateVals[0], out maxLv);
				if(playLevel < maxLv) {
					names[i - 1] = splitVals[0].Split(new char[]{'#'},StringSplitOptions.RemoveEmptyEntries)[1];
				} else {
					names[i - 1] = lateVals[1];
				}
			} else {
				names[i -1] = tempVal;
			}
		}
		return names;
	}

	// end GetRammbockName

	// getPrestigeShopData
	public static List<PrestigeShopData> GetPrestigeInfo(int tabPage, out int maxTabs) {
		NiceTable t = TableManager.GetTable("PrestigeShopConfig");
		Dictionary<int, DataRecord> records = t.GetAllRecord();
		int maxTabId = 0;
		List<PrestigeShopData> datas = new List<PrestigeShopData>();
		PrestigeShopData prestigeData = null;
		foreach (KeyValuePair<int, DataRecord> vRe in records)
		{
			DataRecord r = vRe.Value;
			int tabId = (int)r["TAB_ID"];
			if(tabId == tabPage) {
				prestigeData = new PrestigeShopData();
				
				prestigeData.index = (int)r["INDEX"];
				prestigeData.tabName = (string)r["TAB_NAME"];
				prestigeData.tid = (int)r["ITEM_ID"];
				prestigeData.itemNum = (int)r["ITEM_NUM"];
				prestigeData.itemShow = (int)r["ITEM_SHOW"];
				prestigeData.costType1 = (int)r["COST_TYPE_1"];
				prestigeData.costNum1 = (int)r["COST_NUM_1"];
				prestigeData.costType2 = (int)r["COST_TYPE_2"];
				prestigeData.costNum2 = (int)r["COST_NUM_2"];
				prestigeData.buyNum = (int)r["BUY_NUM"];
				prestigeData.refreshByDay = (int)r["REFRESH_BYDAY"];
				prestigeData.openRank = (int)r["OPEN_RANK"];
                prestigeData.firstShow = (int)r["FIRST_SHOW"];
                prestigeData.curRank = (int)r["CURRENT_RANK"];

				datas.Add(prestigeData);
			}
			maxTabId = Mathf.Max(maxTabId, tabId);
		}
		maxTabs = maxTabId;
		return datas;
	}
	// end get prestige shop data

	// get prestige tab name
	public static string[] GetPrestigeTabName(int maxTab) {
		string[] names = new string[maxTab];
		int val = 0;
		for(int i=maxTab; i > 0; i--) {
			names[i-1] = GetPrestigeInfo(i, out val)[0].tabName;
		}
		return names;
	}
	// end get prestige tab name

	// get feats shop data
	public static List<FeatsShopData> GetFeatsInfo(int tabPage, out int maxTabs) {
		NiceTable t = TableManager.GetTable("DeamonShopConfig");
		Dictionary<int, DataRecord> records = t.GetAllRecord();
		int maxTabId = 0;
		List<FeatsShopData> datas = new List<FeatsShopData>();
		FeatsShopData featsData = null;
		foreach (KeyValuePair<int, DataRecord> vRe in records)
		{
			DataRecord r = vRe.Value;
			int tabId = (int)r["TAB_ID"];
			if(tabId == tabPage) {
				featsData = new FeatsShopData();
				
				featsData.index = (int)r["INDEX"];
				featsData.tabName = (string)r["TAB_NAME"];
				featsData.tid = (int)r["ITEM_ID"];
				featsData.itemNum = (int)r["ITEM_NUM"];
				featsData.itemShowLevel = (int)r["ITEM_SHOW_LEVEL"];
				featsData.vipLevelDisplay = (int)r["VIP_LEVEL_DISPLAY"];
				featsData.costType1 = (int)r["COST_TYPE_1"];
				featsData.costNum1 = (int)r["COST_NUM_1"];
				featsData.costType2 = (int)r["COST_TYPE_2"];
				featsData.costNum2 = (int)r["COST_NUM_2"];
				featsData.buyNum = (int)r["BUY_NUM"];
				featsData.refreshByDay = (int)r["REFRESH_BYDAY"];
				
				datas.Add(featsData);
			}
			maxTabId = Mathf.Max(maxTabId, tabId);
		}
		maxTabs = maxTabId;
		return datas;
	}
	// end feats data

	//get feats tab name
	public static string[] GetFeatsTabName(int maxTab) {
		string[] names = new string[maxTab];
		int val = 0;
		for(int i=maxTab; i > 0; i--) {
			names[i-1] = GetFeatsInfo(i, out val)[0].tabName;
		}
		return names;
	}
	// end get feats name

	// get mystery shop data
	public static List<MysteriousShopData> GetMysteryInfo() {
		NiceTable t = TableManager.GetTable("MysteryShopConfig");
		Dictionary<int, DataRecord> records = t.GetAllRecord();
		List<MysteriousShopData> datas = new List<MysteriousShopData>();
		MysteriousShopData mysteryShopData = null;
		foreach (KeyValuePair<int, DataRecord> vRe in records)
		{
			DataRecord r = vRe.Value;
			mysteryShopData = new MysteriousShopData();
				
			mysteryShopData.index = (int)r["INDEX"];
			mysteryShopData.tid = (int)r["ITEM_ID"];
			mysteryShopData.itemNum = (int)r["ITEM_NUM"];
			mysteryShopData.itemShowLevel = (int)r["ITEM_SHOW_LEVEL"];
			mysteryShopData.costType1 = (int)r["COST_TYPE_1"];
			mysteryShopData.costNum1 = (int)r["COST_NUM_1"];
			mysteryShopData.firstShow = (int)r["FIRST_SHOW"];
			mysteryShopData.petId = (int)r["PET_ID"];
			mysteryShopData.refreshRate = (int)r["REFRESH_RATE"];
				
			datas.Add(mysteryShopData);
		}
		return datas;
	}
	// end get mystery data

	// get mystery shop need data
	public static List<MysteriousShopData> GetMysteryNeedInfo(int[] idxArray) {

		List<MysteriousShopData> msds = GetMysteryInfo();
		List<MysteriousShopData> handleData = new List<MysteriousShopData>();
		if(idxArray == null || idxArray.Length == 0) {
			DEBUG.LogError("net request err.");
			return null;
		}
		// 找需要显示的条目
		foreach(int idx in idxArray) {
            for (int i = 0; i < msds.Count; ++i)
            {
                MysteriousShopData msd = msds[i];
                if (msd == null) continue;
                if (idx == msd.index)
                {
                    handleData.Add(msd);
                    break;
                }

            }
		}

		return handleData;
	}
	// end get mystery shop need data

    // PetLevelExp.csv
    // Synthetic Exp
    public static int GetSyntheticExp(int iStarLevel, int iLevel)
    {
        int iIndex = iStarLevel * 1000 + iLevel;

        DataRecord dataRecord = DataCenter.mPetLevelExpTable.GetRecord(iIndex);

        if (iIndex <= 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetSyntheticExp iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }

        return (int)(dataRecord.get("SYNTHETIC_EXP"));
    }

    public static int GetMaxExp(int iStarLevel, int iLevel)
    {
        DataRecord dataRecord = DataCenter.mPetLevelExpTable.GetRecord(iLevel);

        if (iLevel <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetMaxExp iIndex = " + iLevel.ToString() + "is not exist");
            return 1;
        }
        return (int)(dataRecord.get("NEED_EXP_QUALITY_" + iStarLevel));
    }

    public static int GetSyntheticCoin(int iStarLevel, int iLevel)
    {
        int iIndex = iStarLevel * 1000 + iLevel;
        DataRecord dataRecord = DataCenter.mPetLevelExpTable.GetRecord(iIndex);

        if (iIndex <= 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetSyntheticCoin iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get("SYNTHETIC_COIN"));
    }

    // Element.csv
    public static string GetStringFromElement(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mElementTable.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromElement iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static int GetNumberFromElement(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mElementTable.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromElement iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    // Skill.csv skill config load
    public static string GetStringFromSkillConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mSkillConfigTable.GetRecord(iIndex);

        if (iIndex <= 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromSkillConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static int GetNumberFromSkillConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mSkillConfigTable.GetRecord(iIndex);

        if (iIndex <= 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromSkillConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static float GetFloatFromSkillConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mSkillConfigTable.GetRecord(iIndex);

        if (iIndex <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetObjectFromSkillConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0f;
        }
        return (float)dataRecord.getObject(strIndexName);
    }
    // StrengthenConsume.csv
    public static string GetStringFromStrengthenConsumeConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mStrengthenConsume.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromStrengthenConsumeConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }
    public static int GetNumberFromStrengthenConsumeConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mStrengthenConsume.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromStrengthenConsumeConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

	// MallShopConfig.csv
	public static string GetStringFromMallShopConfig(int iIndex, string strIndexName) {

		DataRecord dataRecord = DataCenter.mMallShopConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromMallShopConfig iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}

	public static int GetNumberFromMallConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mStrengthenConsume.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromMallConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}

	public static Dictionary<int, DataRecord> GetDataFromMallConfigByPage(int page) {
		Dictionary<int, DataRecord> rVa = new Dictionary<int, DataRecord>();
		NiceTable t = TableManager.GetTable("MallShopConfig");

        //by chenliang
        //begin

// 		int i = 0;
// 		foreach (KeyValuePair<int, DataRecord> vRe in t.GetAllRecord()) {
// 			if(page == 1) {
// 				DataRecord r = vRe.Value;
// 				if((int)r["TAB_ID"] == 1) {
// 					rVa.Add(i++, r);
// 				}
// 			} else if(page == 2) {
// 				DataRecord r = vRe.Value;
// 				if((int)r["TAB_ID"] == 2) {
// 					rVa.Add(i++, r);
// 				}
// 			}
// 		}
//-----------------------
		//以INDEX字段为索引，之后方便查找
        foreach (KeyValuePair<int, DataRecord> vRe in t.GetAllRecord())
        {
            if (page == 1)
            {
                DataRecord r = vRe.Value;
                if ((int)r["TAB_ID"] == 1)
                    rVa.Add((int)r["INDEX"], r);
            }
            else if (page == 2)
            {
                DataRecord r = vRe.Value;
                if ((int)r["TAB_ID"] == 2)
                    rVa.Add((int)r["INDEX"], r);
            }
        }

        //end

		return rVa;
	}

    // EvolutionConsume.csv
    public static string GetStringFromEvolutionConsumeConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mEvolutionConsume.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromEvolutionConsumeConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }
    public static int GetNumberFromEvolutionConsumeConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mEvolutionConsume.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromEvolutionConsumeConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    // GroupIDConfig.csv
    public static string GetStringFromGroupIDConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mGroupIDConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromGroupIDConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }
    public static int GetNumberFromGroupIDConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mGroupIDConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromGroupIDConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    // StoneTypeIcon.csv
    public static int GetNumberFromStoneTypeIconConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mStoneTypeIcon.GetRecord(iIndex);

        if (iIndex < 1000 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromStoneTypeIconConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromStoneTypeIconConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mStoneTypeIcon.GetRecord(iIndex);

		if (iIndex < 1000 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromStoneTypeIconConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    // CharacterLevelExp.csv
    public static int GetRoleMaxExp(int iLevel)
    {
        DataRecord dataRecord = DataCenter.mCharacterLevelExpTable.GetRecord(iLevel);
        if (iLevel <= 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetRoleMaxExp iLevel = " + iLevel.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get("LEVEL_EXP"));
    }

	public static int GetRoleLABOR(int iLevel)
	{
		DataRecord dataRecord = DataCenter.mCharacterLevelExpTable.GetRecord(iLevel);
		if (iLevel <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetRoleMaxExp iLevel = " + iLevel.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get("LABOR"));
	}

    public static int GetRoleEnergy(int iLevel) 
    {
        DataRecord dataRecord = DataCenter.mCharacterLevelExpTable.GetRecord(iLevel);
        if (iLevel <= 0 || dataRecord == null) 
        {
            DEBUG.LogWarning("fun : GetRoleEnergy iLevel = " + iLevel.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get("ENERGY"));
    }

	public static int GetNumberFromCharacterLevelExp(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mCharacterLevelExpTable.GetRecord(iIndex);
		
		if (iIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromCharacterLevelExp iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
    //PetOfPredestined.csv
    public static int[] GetPetIdsFormPetOfPredestined(int groupId)
    {
        DataRecord dataRecord = DataCenter.mPetOfPredestined.GetRecord(groupId);
        if (groupId <= 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetPetIdsPetOfPredestined groupId = " + groupId.ToString() + "is not exist");
            return new int[0];
        }     
        int knightNum = (int) dataRecord.get("FATE_ACTIVE_COUNT");
        int[] kInts = new int[knightNum];
        for (int i = 0; i < knightNum; i++)
        {
            kInts[i] = (int) dataRecord.get("FATE_PET_ID_" + (i + 1));
        }
        return kInts;
    }

    // get predestined of title
    public static string GetPredestinedTitleFormPetOfPredestined(int groupId)
    {
        DataRecord dataRecord = DataCenter.mPetOfPredestined.GetRecord(groupId);
        if (groupId <= 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetPredestinedTitle groupId = " + groupId.ToString() + "is not exist");
            return "";
        }
        return (string)dataRecord.get("FATE_TITLE");
    }

    //get predestined of data model add to list
    public static KnightPartyTableModel[] GetPetPartyModelFromPetOfPredestined()
    {
        Dictionary<int,DataRecord> dataRecord = DataCenter.mPetOfPredestined.GetAllRecord();
        KnightPartyTableModel[] tableModels = new KnightPartyTableModel[dataRecord.Count];
        int Index = -1;
        foreach (DataRecord knightPartyModel in dataRecord.Values)
        {
            Index++;
            KnightPartyTableModel tableModel = new KnightPartyTableModel();
            tableModel.groundId = (int)knightPartyModel.get("GROUND_ID");
            tableModel.title = knightPartyModel.get("FATE_TITLE");
            tableModel.property = knightPartyModel.get("FATE_PROPERTY");
            tableModel.addtiveId = (int)knightPartyModel.get("FATE_ADDTIVE_ID");
            tableModel.activeCount = (int)knightPartyModel.get("FATE_ACTIVE_COUNT");
            tableModel.KnghitIds = GetPetIdsFormPetOfPredestined(tableModel.groundId);
            tableModels[Index] = tableModel;
        }
        return tableModels;
    }

	// StageConfig.csv
	public static int GetNumberFromStageConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mStageTable.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromStageConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromStageConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mStageTable.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromStageConfig iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}

    // TaskConfig.csv
    public static int GetNumberFromTaskConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mTaskConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromTaskConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromTaskConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mTaskConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromTaskConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

	// AttackState.csv
	public static int GetNumberFromAttackState(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mAttackState.GetRecord(iIndex);
		
		if (iIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromAttackState iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromAttackState(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mAttackState.GetRecord(iIndex);
		
		if (iIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromAttackState iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static object GetObjectFromAttackState(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mAttackState.GetRecord(iIndex);
		
		if (iIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetObjectFromAttackState iIndex = " + iIndex.ToString() + "is not exist");
			return null;
		}
		return dataRecord.getObject(strIndexName);
	}

	// AffectBuffer.csv
	public static int GetNumberFromAffectBuffer(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mAffectBuffer.GetRecord(iIndex);
		
		if (iIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromAffectBuffer iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromAffectBuffer(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mAffectBuffer.GetRecord(iIndex);
		
		if (iIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromAffectBuffer iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}

	public static object GetObjectFromAffectBuffer(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mAffectBuffer.GetRecord(iIndex);
		
		if (iIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetObjectFromAffectBuffer iIndex = " + iIndex.ToString() + "is not exist");
			return null;
		}
		return dataRecord.getObject(strIndexName);
	}

    // ShopSlotBase.csv
    public static int GetNumberFromShopSlotBase(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mShopSlotBase.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromShopSlotBase iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromShopSlotBase(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mShopSlotBase.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromShopSlotBase iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static object GetObjectFromShopSlotBase(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mShopSlotBase.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetObjectFromShopSlotBase iIndex = " + iIndex.ToString() + "is not exist");
            return null;
        }
		return dataRecord.getObject(strIndexName);
    }

    // RoleEquipConfig.csv
    public static int GetNumberFromRoleEquipConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mRoleEquipConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromRoleEquipConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromRoleEquipConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mRoleEquipConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromRoleEquipConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static object GetObjectFromRoleEquipConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mRoleEquipConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetObjectFromRoleEquipConfig iIndex = " + iIndex.ToString() + "is not exist");
            return null;
        }
		return dataRecord.getObject(strIndexName);
    }

	// EquipStrengthCostConfig.csv
	public static int GetNumberFromEquipStrengthCostConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mEquipStrengthCostConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromEquipStrengthCostConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	// FateConfig.csv
	public static int GetNumberFromFateConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mFateConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromFateConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	// BreakLevelConfig.csv
	public static int GetNumberFromBreakLevelConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mBreakLevelConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromBreakLevelConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	// BreakBuff.csv
	public static int GetNumberFromBreakBuffConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mBreakBuffConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			//DEBUG.LogWarning("fun : GetNumberFromBreakBuffConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	// SetEquipConfig.csv
	public static int GetNumberFromSetEquipConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mSetEquipConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromSetEquipConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromSetEquipConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mSetEquipConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromSetEquipConfig iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static object GetObjectFromSetEquipConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mSetEquipConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromSetEquipConfig iIndex = " + iIndex.ToString() + "is not exist");
			return null;
		}
		return dataRecord.getObject(strIndexName);
	}
	// PointStarConfig.csv
	public static int GetNumberFromPointStarConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mPointStarConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromPointStarConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromPointStarConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mPointStarConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromPointStarConfig iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static object GetObjectFromPointStarConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mPointStarConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetObjectFromPointStarConfig iIndex = " + iIndex.ToString() + "is not exist");
			return null;
		}
		return dataRecord.getObject(strIndexName);
	}

    // EquipAttachAttributeConfig.csv
    public static int GetNumberFromEquipAttachAttributeConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mEquipAttachAttributeConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromEquipAttachAttributeConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromEquipAttachAttributeConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mEquipAttachAttributeConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromEquipAttachAttributeConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static object GetObjectFromEquipAttachAttributeConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mEquipAttachAttributeConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetObjectFromEquipAttachAttributeConfig iIndex = " + iIndex.ToString() + "is not exist");
            return null;
        }
		return dataRecord.getObject(strIndexName);
    }

	// EquipAttributeIconConfig.csv
	public static int GetNumberFromEquipAttributeIconConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mEquipAttributeIconConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromEquipAttributeIconConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromEquipAttributeIconConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mEquipAttributeIconConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromEquipAttributeIconConfig iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static object GetObjectFromEquipAttributeIconConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mEquipAttributeIconConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetObjectFromEquipAttributeIconConfig iIndex = " + iIndex.ToString() + "is not exist");
			return null;
		}
		return dataRecord.getObject(strIndexName);
	}

	//RoleStarLevel.csv
	public static int GetNumberFromRoleStarLevel(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mRoleStarLevel.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromRoleStarLevel iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

	public static string GetStringFromRoleStarLevel(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mRoleStarLevel.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromRoleStarLevel iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}

    //RoleSkinConfig.csv
    public static int GetNumberFromRoleSkinConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mRoleSkinConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromRoleSkinConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromRoleSkinConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mRoleSkinConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromRoleSkinConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

	//RoleUIConfig.csv
    public static int GetNumberFromRoleUIConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mRoleUIConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetNumberFromRoleUIConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromRoleUIConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mRoleUIConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
			DEBUG.LogWarning("fun : GetStringFromRoleUIConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

	// SelPetUIConfig.csv
	public static int GetNumberFromSelPetUIConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mSelPetUIConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromSelPetUIConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromSelPetUIConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mSelPetUIConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromSelPetUIConfig iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static object GetObjectFromSelPetUIConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mSelPetUIConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetObjectFromSelPetUIConfig iIndex = " + iIndex.ToString() + "is not exist");
			return null;
		}
		return dataRecord.getObject(strIndexName);
	}

	//Dialog.csv
    public static int GetNumberFromDialog(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mDialog.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromDialog iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromDialog(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mDialog.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromDialog iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

	//DailySignEvent.csv
	public static int GetNumberFromDailySign(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mDailySignEvent.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromDailySign iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromDailySign(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mDailySignEvent.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromDailySign iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}

	//VipList.csv
	public static int GetNumberFromVipList(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mVipListConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromVipList(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mVipListConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}

	//GuildDonation.csv
	public static int GetNumberGuildDonation(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mGuildDonationConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromGuildDonation(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mGuildDonationConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	//GuildShop.csv
	public static int GetNumberGuildShop(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mGuildShopConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromGuildShop(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mGuildShopConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	//GuildTech.csv
	public static int GetNumberGuildTech(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mGuildTechConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromGuildTech(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mGuildTechConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	//BeStronger.csv
	public static int GetNumberBeStronger(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mBeStrongerConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromBeStronger(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mBeStrongerConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	//EndlessDifficult.csv
	public static int GetNumberEndlessDifficult(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mEndlessDifficult.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromEndlessDifficult(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mEndlessDifficult.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromVipList iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}



    // AnnouncementConfig.csv
    public static int GetNumberFromAnnouncementConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mAnnouncementConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromAnnouncementConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromAnnouncementConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mAnnouncementConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromAnnouncementConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    // BossConfig.csv
    public static int GetNumberFromBossConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mBossConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromBossConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromBossConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mBossConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromBossConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

	// PetType.csv
	public static int GetNumberFromPetType(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mPetType.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromPetType iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromPetType(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mPetType.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromPetType iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}

    public static int GetNumberFromStagePoint(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mStagePoint.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromStagePoint iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromStageFather(int iIndex,string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mStageFather.GetRecord(iIndex);
        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromStageFather iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static int GetNumberFromStageBonus(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mStageBonus.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromStageBonus iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

	//Rankaward.csv
	public static int GetNumberFromRankaward(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mPvpRankAwardsConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromRankaward iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromRankaward(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mPvpRankAwardsConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromRankaward iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}

    //Fragment.csv
	public static int GetNumberFromFragment(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mFragment.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromFragment iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromFragment(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mFragment.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
            DEBUG.LogWarning("fun : GetStringFromFragment iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	//FragmentAdmin.csv
	public static int GetNumberFromFragmentAdmin(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mFragmentAdminConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromFragmentAdmin iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromFragmentAdmin(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mFragmentAdminConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromFragmentAdmin iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	

	//ToolItem.csv
	public static int GetNumberFromConsumeConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mConsumeConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromPropConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromConsumeConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mConsumeConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromPropConfig iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	//ItemIcon.csv
	public static int GetNumberFromItemIconConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mItemIcon.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromPropConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromItemIconConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mItemIcon.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromPropConfig iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	//PlayerLevel.csv
	public static int GetNumberFromPlayerLevelConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mPlayerLevelConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromPlayerLevel iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromPlayerLevelConfig(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mPlayerLevelConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromPlayerLevel iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}

	//pumping shop config
	public static string GetStringFromPumpingConfig(int iIndex, string colName) {
		DataRecord dataRecord = DataCenter.mPumpingConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromPumpingConfig iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(colName));
	}

	public static int GetNumberFromPumpingConfig(int iIndex, string colName) {
		DataRecord dataRecord = DataCenter.mPumpingConfig.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromPumpingConfig iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(colName));
	}


    //MaterialConfig.csv
    public static int GetNumberFromMaterialConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mMaterialConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromMaterialConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromMaterialConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mMaterialConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromMaterialConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    //MaterialFragment.csv
    public static int GetNumberFromMaterialFragment(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mMaterialFragment.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromMaterialFragment iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromMaterialFragment(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mMaterialFragment.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromMaterialFragment iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    //MaterialPrompt.csv
    public static int GetNumberFromMaterialPrompt(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mMaterialPrompt.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromMaterialPrompt iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromMaterialPrompt(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mMaterialPrompt.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromMaterialPrompt iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    //MaterialSynthesis.csv
    public static int GetNumberFromMaterialSynthesis(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mMaterialSynthesis.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromMaterialSynthesis iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static string GetStringFromMaterialSynthesis(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mMaterialSynthesis.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromMaterialSynthesis iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

	// SkillCost
	public static int GetNumberFromSkillCost(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mSkillCost.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromSkillCost iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static string GetStringFromSkillCost(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mSkillCost.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromSkillCost iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}

    // MagicEquipLvConfig.csv
    public static string GetStringFromMagicEquipLvConfig(int iModelIndex, string strIndexName)
    {
        DataRecord dataRecord = null;
        dataRecord = DataCenter.mMagicEquipLvConfig.GetRecord(iModelIndex);

        if (iModelIndex <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromMagicEquipLvConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static int GetNumberFromMagicEquipLvConfig(int iModelIndex, string strIndexName)
    {
        DataRecord dataRecord = null;
        dataRecord = DataCenter.mMagicEquipLvConfig.GetRecord(iModelIndex);

        if (iModelIndex <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromMagicEquipLvConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static object GetObjectFromMagicEquipLvConfig(int iModelIndex, string strIndexName)
    {
        DataRecord dataRecord = null;
        dataRecord = DataCenter.mMagicEquipLvConfig.GetRecord(iModelIndex);

        if (iModelIndex <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromMagicEquipLvConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
            return 0;
        }
        return dataRecord.getObject(strIndexName);
    }

    // PetPostionLevel.csv
    public static string GetStringFromPetPostionLevel(int iModelIndex, string strIndexName)
    {
        DataRecord dataRecord = null;
        dataRecord = DataCenter.mPetPostionLevel.GetRecord(iModelIndex);

        if (iModelIndex <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromPetPostionLevel iModelIndex = " + iModelIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static int GetNumberFromPetPostionLevel(int iModelIndex, string strIndexName)
    {
        DataRecord dataRecord = null;
        dataRecord = DataCenter.mPetPostionLevel.GetRecord(iModelIndex);

        if (iModelIndex <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromPetPostionLevel iModelIndex = " + iModelIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static object GetObjectFromPetPostionLevel(int iModelIndex, string strIndexName)
    {
        DataRecord dataRecord = null;
        dataRecord = DataCenter.mPetPostionLevel.GetRecord(iModelIndex);

        if (iModelIndex <= 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromPetPostionLevel iModelIndex = " + iModelIndex.ToString() + "is not exist");
            return 0;
        }
        return dataRecord.getObject(strIndexName);
    }
	// ResourceGainConfig.csv
	public static string GetStringFromResourceGainConfig(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mResourceGainConfig.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromResourceGainConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static int GetNumberFromResourceGainConfig(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mResourceGainConfig.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromResourceGainConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static object GetObjectFromResourceGainConfig(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mResourceGainConfig.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetObjectFromResourceGainConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return 0;
		}
		return dataRecord.getObject(strIndexName);
	}
	// GainFunctionConfig.csv
	public static string GetStringFromGainFunctionConfig(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mGainFunctionConfig.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromGainFunctionConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static int GetNumberFromGainFunctionConfig(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mGainFunctionConfig.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromGainFunctionConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static object GetObjectFromGainFunctionConfig(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mGainFunctionConfig.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetObjectFromGainFunctionConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return 0;
		}
		return dataRecord.getObject(strIndexName);
	}
	// FunctionConfig.csv
	public static string GetStringFromFunctionConfig(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mFunctionConfig.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromFunctionConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static int GetNumberFromFunctionConfig(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mFunctionConfig.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromFunctionConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static object GetObjectFromFunctionConfig(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mFunctionConfig.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetObjectFromFunctionConfig iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return 0;
		}
		return dataRecord.getObject(strIndexName);
	}

	// HDlogin.csv
	public static string GetStringFromHDlogin(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mHDlogin.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromHDlogin iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static int GetNumberFromHDlogin(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mHDlogin.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromHDlogin iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static object GetObjectFromHDlogin(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mHDlogin.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetObjectFromHDlogin iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return 0;
		}
		return dataRecord.getObject(strIndexName);
	}
	// HDrevelry.csv
	public static string GetStringFromHDrevelry(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mHdrevelry.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromHDrevelry iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static int GetNumberFromHDrevelry(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mHdrevelry.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromHDrevelry iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	
	public static object GetObjectFromHDrevelry(int iModelIndex, string strIndexName)
	{
		DataRecord dataRecord = null;
		dataRecord = DataCenter.mHdrevelry.GetRecord(iModelIndex);
		
		if (iModelIndex <= 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetObjectFromHDrevelry iModelIndex = " + iModelIndex.ToString() + "is not exist");
			return 0;
		}
		return dataRecord.getObject(strIndexName);
	}

    // QualityConfig.csv
    public static string GetStringFromQualityConfig(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mQualityConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromQualityConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static int GetNumberFromQualityConfig(int iIndex, string strIndexName)
    {
		DataRecord dataRecord = DataCenter.mQualityConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromQualityConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

    public static float GetFloatFromQualityConfig(int iIndex,string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mQualityConfig.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromQualityConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (float)(dataRecord.get(strIndexName));
    }

	// FundEvent.csv
	public static string GetStringFromFundEvent(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mFundEvent.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromFundEvent iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static int GetNumberFromFundEvent(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mFundEvent.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromFundEvent iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}

    //get num from config for strIndexName
    public static string GetStringFromConfig(int iIndex, string strIndexName, NiceTable config)
    {
        DataRecord dataRecord = config.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromQualityConfig iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }

    public static string getStringFromStringList(STRING_INDEX stringIndex)
    {
        return DataCenter.mStringList.GetData((int)stringIndex, "STRING_CN");
    }
    

    public static int GetNumberFromConfig(int iIndex, string strIndexName, NiceTable config)
    {
        DataRecord dataRecord = config.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetNumberFromQualityConfig iIndex = " + iIndex.ToString() + "is not exist");
            return 0;
        }
        return (int)(dataRecord.get(strIndexName));
    }

	// SevenDayLoginEvent.csv
	public static string GetStringFromSevenDayLoginEvent(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mSevenDayLoginEvent.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromSevenDayLoginEvent iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static int GetNumberFromSevenDayLoginEvent(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mSevenDayLoginEvent.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromSevenDayLoginEvent iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}

    public static string GetStringFromDefaultRole(int iIndex, string strIndexName)
    {
        DataRecord dataRecord = DataCenter.mDefaultRole.GetRecord(iIndex);

        if (iIndex < 0 || dataRecord == null)
        {
            DEBUG.LogWarning("fun : GetStringFromDefaultRole iIndex = " + iIndex.ToString() + "is not exist");
            return "";
        }
        return (string)(dataRecord.get(strIndexName));
    }
    //by chenliang
    //begin

    /// <summary>
    /// 根据价格获取充值月卡记录
    /// </summary>
    /// <param name="price"></param>
    /// <returns></returns>
    public static DataRecord GetRechargeMonthCardByPrice(int price)
    {
        if (DataCenter.mChargeConfig == null)
            return null;

        foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mChargeConfig.GetAllRecord())
        {
            int tmpIsCard = (int)tmpPair.Value.getObject("IS_CARD");
            if(tmpIsCard == 0)
                return null;
            int tmpPrice = (int)tmpPair.Value.getObject("PRICE");
            if (tmpPrice == price)
                return tmpPair.Value;
        }
        return null;
    }

    //end
    public static int GetIndexFromMHdrevelry(string dayTaskIndex)
    {
        int ret = 0;
        string[] dayAndTask = dayTaskIndex.Split('|');
        int dayIndex = 1;
        int taskIndex = 0;
        if (dayAndTask.Length > 1)
        {
            dayIndex = System.Convert.ToInt32(dayAndTask[0]);
            taskIndex = System.Convert.ToInt32(dayAndTask[1]);
        }
        

        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mHdrevelry.GetAllRecord())
        {
            DataRecord r = pair.Value;
            if (r["DAY"] == dayIndex && r["PAGE"] == taskIndex)
            {
                ret = r["INDEX"];
            }
        }
        return ret;
    }

	// MainUiNotice.csv
	public static string GetStringFromMainUiNotice(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mMainUiNotice.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromMainUiNotice iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static int GetNumberFromMainUiNotice(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mMainUiNotice.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromMainUiNotice iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
	// FightUiNotice.csv
	public static string GetStringFromFightUiNotice(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mFightUiNotice.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetStringFromFightUiNotice iIndex = " + iIndex.ToString() + "is not exist");
			return "";
		}
		return (string)(dataRecord.get(strIndexName));
	}
	
	public static int GetNumberFromFightUiNotice(int iIndex, string strIndexName)
	{
		DataRecord dataRecord = DataCenter.mFightUiNotice.GetRecord(iIndex);
		
		if (iIndex < 0 || dataRecord == null)
		{
			DEBUG.LogWarning("fun : GetNumberFromFightUiNotice iIndex = " + iIndex.ToString() + "is not exist");
			return 0;
		}
		return (int)(dataRecord.get(strIndexName));
	}
    
}