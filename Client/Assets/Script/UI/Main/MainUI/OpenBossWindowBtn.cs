using UnityEngine;
using System.Collections;
using DataTable;

public class OpenBossWindowBtn : MonoBehaviour {
    public int BtnID = 1401;
	void OnClick()
	{
        //if(!GameCommon.FunctionIsUnlock (UNLOCK_FUNCTION_TYPE.BOSS_RAIN))
        //{	
        //    DataCenter.OpenWindow ("PLAYER_LEVEL_UP_SHOW_WINDOW", false);
        //    DataCenter.SetData ("PLAYER_LEVEL_UP_SHOW_WINDOW", "NEED_LEVEL", UNLOCK_FUNCTION_TYPE.BOSS_RAIN);
        //    return;
        //}
        if (!CheckBossRainIsOpen()) 
        {
            return;
        }
		//MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);		
		//DataCenter.OpenWindow ("BOSS_RAID_WINDOW");
        DataCenter.SetData("INFO_GROUP_WINDOW", "PRE_WIN", 1);
        GetPathHandlerDic.HandlerDic[GET_PARTH_TYPE.TIAN_MO]();
	}

    //tmp
    private bool CheckBossRainIsOpen() 
    {
        DataRecord record = DataCenter.mFunctionConfig.GetRecord(BtnID);
        int conditonType = record.getData("FUNC_CONDITION");
        if (conditonType == 1)
        {
            // add by LC
            bool isOpen = GameCommon.IsFuncCanUse(BtnID);
            // end

            if (!isOpen)
            {
                DataCenter.OpenMessageWindow(record.getData("FUNC_CONDITION_DESCRIBE"));           
                return false;
            }
        }
        return true;
    }
}
