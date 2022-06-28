#define SAVE_BYTE_CONFIG_TABLE
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


using DataTable;
//---------------------------------------------------------------------
// 
//---------------------------------------------------------------------


public class TableManager
{
    static public TableManager Self = new TableManager();

    static public TableManager GetMe() { return Self; }

    Dictionary<string, NiceTable> mConfigMap = new Dictionary<string, NiceTable>();

    public TableManager()
    {

    }

    public String LoadResouceConfig(String configIndex, String tableFileName, LOAD_MODE loadMode)
    {
        string loadinfo = string.Empty;
        NiceTable config = GameCommon.LoadTable(tableFileName, loadMode);
        string info = "";
        if (config != null)
        {
			UnityEngine.Debug.Log ("jiazaibiaoge33333=========");
            mConfigMap.Add(configIndex, config);
            info += "Succeed load ";
#if SAVE_BYTE_CONFIG_TABLE && UNITY_EDITOR
            config.SaveBinary(GameCommon.MakeGamePathFileName("Resources/Config/StaticConfig/" + Path.GetFileNameWithoutExtension(tableFileName) + ".bytes"));
#endif
        }
        else
        {
            info += "Fail load ";
            DEBUG.LogError("Fail load config table >" + tableFileName);
        }
        info += configIndex;
        info += " : " + tableFileName;
        info += " Record count " + config.GetRecordCount().ToString();
        Logic.EventCenter.Log(false, info);
        info += "\r\n";
        loadinfo += info;
        return loadinfo;
    }

    public String LoadConfig(String configListTableName, LOAD_MODE loadMode)
    {
        DEBUG.Log("======================== parsing config ==========================");
        DEBUG.Log("   config list file name: " + configListTableName);
        DEBUG.Log("==================================================================");
        NiceTable configList = GameCommon.LoadTable(configListTableName, loadMode);
		//NiceTable configList = new NiceTable ();
        if (configList != null)
        {
#if SAVE_BYTE_CONFIG_TABLE && UNITY_EDITOR
            //by chenliang
            //begin

//            configList.SaveBinary(GameCommon.MakeGamePathFileName("Resources/Config/" + Path.GetFileNameWithoutExtension(configListTableName) + ".bytes"));
//--------------------
            string tmpBinaryConfig = GameCommon.DynamicGameDataBinaryPath + "/Config/" + Path.GetFileNameWithoutExtension(configListTableName);
            GameCommon.CheckAndCreateDirectory(GameCommon.DynamicGameDataRootPath + tmpBinaryConfig);
            configList.SaveBinary(GameCommon.MakeGamePathFileName(tmpBinaryConfig + ".bytes"));

            //end
#endif
#if (UNITY_ANDROID || UNITY_IPHONE)&&!UNITY_EDITOR
            var result = LoadConfig(configList, loadMode);
#else
			var result = LoadConfig(configList, LOAD_MODE.UNKNOW);
#endif
			DEBUG.Log("=========================== config done ==========================" + result);
			Debug.Log("cofig done=======" + result);
			return result;
        }

        return "Load config list table fail." + configListTableName;
        DEBUG.Log("=========================== config done ==========================");
    }

    public String LoadConfig(NiceTable configListTable, LOAD_MODE tempLoadMode)
    {
        String loadinfo = "";
        LOAD_MODE loadMode = tempLoadMode;
        if (configListTable != null)
        {
            foreach (KeyValuePair<int, DataRecord> kv in configListTable.GetAllRecord())
            {
                DataRecord r = kv.Value;
                string configIndex = (string)r.get("INDEXNAME");
                string tableFileName = (string)r.get("FILENAME");
                string fileType = (string)r.get("FILETYPE");
                if (tempLoadMode == LOAD_MODE.UNKNOW)
                {

                    switch (fileType)
                    {
                        case "ANIS":
                            loadMode = LOAD_MODE.ANIS;
                            break;

                        case "UNICODE":
                            loadMode = LOAD_MODE.UNICODE;
                            break;

                        case "BYTES":
                            loadMode = LOAD_MODE.BYTES;
                            break;

                        case "BYTES_RES":
                            loadMode = LOAD_MODE.BYTES_RES;
                            break;

                        case "BYTES_TRY_PATH":
                            loadMode = LOAD_MODE.BYTES_TRY_PATH;
                            break;

                        case "BYTES_TRY_RES":
                            loadMode = LOAD_MODE.BYTES_TRY_RES;
                            break;
                    }
                }
                try {
                    NiceTable config = GameCommon.LoadTable(tableFileName, loadMode);
                    string info = "";
                    if (config != null)
                    {
						UnityEngine.Debug.Log ("jiazaibiaoge444444=========");
                        mConfigMap.Add(configIndex, config);
                        info += "Succeed load ";
#if SAVE_BYTE_CONFIG_TABLE && UNITY_EDITOR
                        //by chenliang
                        //begin

//						config.SaveBinary( GameCommon.MakeGamePathFileName("Resources/Config/" + Path.GetFileNameWithoutExtension(tableFileName) + ".bytes") );
//--------------------------------
                        //支持多级目录
                        string tmpTableFileName = tableFileName.Replace("Config/", "");
                        tmpTableFileName = tmpTableFileName.Split('.')[0];
                        string tmpBinaryConfig = GameCommon.DynamicGameDataBinaryPath + "/Config/" + tmpTableFileName;
                        GameCommon.CheckAndCreateDirectory(GameCommon.DynamicGameDataRootPath + tmpBinaryConfig);
                        config.SaveBinary(GameCommon.MakeGamePathFileName(tmpBinaryConfig + ".bytes"));

                        //end
#endif
                    }
                    else
                    {
                        info += "Fail load ";
                        DEBUG.LogError("Fail load config table >" + tableFileName);
                    }
                    info += configIndex;
                    info += " : " + tableFileName;
                    info += " TYPE: " + fileType;
                    info += " Record count " + config.GetRecordCount().ToString();                    
                    Logic.EventCenter.Log(false, info);
					info += "\r\n";
                    loadinfo += info;
                }
                catch (Exception e)
                {
                    Logic.EventCenter.Self.Log("Error: load table fail >"+tableFileName+" >>"+ e.ToString() );
                }
            }
        }
        return loadinfo;
    }

    public bool LoadFromPack(string tablePackFileName, string configListName, bool isStrict)
    {
        string fileName = GameCommon.MakeGamePathFileName(tablePackFileName);
        ResourcesPack resPack = new ResourcesPack();
		if (!resPack.load(fileName, isStrict))
        {
			if(isStrict)
            	DEBUG.LogError("Load resources pack file fail >" + fileName);
            return false;
        }

        DataBuffer listData = resPack.loadResource(configListName);
        if (listData == null)
        {
			if(isStrict)
            	DEBUG.LogError("Load table list fail >" + configListName + "  from pack >" + tablePackFileName);
            return false;
        }

        NiceTable listTable = new NiceTable();
        listData.seek(0);
        if (!listTable.restore(ref listData))
        {
           	DEBUG.LogError("Restor list table fail from >" + tablePackFileName);
            return false;
        }

		DEBUG.Log("======================== parsing config ==========================");
		DEBUG.Log("        package: " + tablePackFileName);
		DEBUG.Log("==================================================================");

        foreach (KeyValuePair<int, DataRecord> kv in listTable.GetAllRecord())
        {
            DataRecord r = kv.Value;
            string configIndex = (string)r.get("INDEXNAME");
            string tableFileName = (string)r.get("FILENAME");

            string path = Path.GetDirectoryName(tableFileName);
            string fName = Path.GetFileNameWithoutExtension(tableFileName) + ".bytes";
			tableFileName = path + "/" + fName;
			DataBuffer tableData = resPack.loadResource(tableFileName);
            if (tableData != null)
            {
                NiceTable table = new NiceTable();
                if (table.restore(ref tableData))
                {
                    if (GetTable(configIndex)!=null)
                        DEBUG.LogWarning("Table already exist >" + configIndex);

                    mConfigMap[configIndex] = table;
                    DEBUG.Log("Succeed load table >" + tableFileName +" record >"+table.GetRecordCount().ToString() + " from table pack >" + tablePackFileName);
                }
            }
            else
            {
				if(isStrict)
                	DEBUG.LogError("No exist table >" + tableFileName + " data from pack >" + tablePackFileName);
            }
        }

		DEBUG.Log("======================== config pars end ==========================");

        resPack.close();
        return true;
       
    }

    static public NiceTable GetTable(string configIndex)
    {
        NiceTable table;
        if (!Self.mConfigMap.TryGetValue(configIndex, out table))
            DEBUG.Log("没有配置表格>>>" + configIndex);

        return table;
    }

    static public Data GetData(string tableIndex, int keyIndex, string fieldName)
    {
        NiceTable table = GetTable(tableIndex);
        if (table != null)
        {
            DataRecord re = table.GetRecord(keyIndex);
            if (re != null)
                return re.getData(fieldName);
        }
        return Data.NULL;
    }

    static public Data GetData(string tableIndex, string keyIndex, string fieldName)
    {
        NiceTable table = GetTable(tableIndex);
        if (table != null)
        {
            DataRecord re = table.GetRecord(keyIndex);
            if (re != null)
                return re.getData(fieldName);
        }
        return Data.NULL;
    }


    public void ClearAll()
    {
        mConfigMap.Clear();
    }

}
