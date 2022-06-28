using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using LHC.PipeLineUIRefreshSyetem;
using System.Linq;
using System;





public enum D_MasterEquip_1 { 
    tid,
    CurLevel,
    NextLevel
}

public enum PL_MasterEquip_1 { 
    Set_Name,
    Set_Icon,
    Set_Level_Slider,
    Set_Level_Num
}

public enum PL_MasterEquip_2 { 
    Set_CurLevel,
    Set_TarLevel,
    Set_TarLevel_Explain,
    Set_CurLabel_1,
    Set_CurLabel_2,
    Set_CurLabel_3,
    Set_CurLabel_4,
    Set_TarLabel_1,
    Set_TarLabel_2,
    Set_TarLabel_3,
    Set_TarLabel_4,
    Set_Explain
}

public enum D_MasterEquip_2 { 
    CurLevel,
    TarLevel,
    StrengMasterBuff1,
    StrengMasterBuff2,
    StrengMasterBuff3,
    StrengMasterBuff4,
    EquipRefinBuff1,
    EquipRefinBuff2,
    MagicBuff1,
    MagicBuff2,
    MagicBuff3,
    RefinStrumentsBuff1,
    RefinStrumentsBuff2,

    StrengMasterBuff1_Tar,
    StrengMasterBuff2_Tar,
    StrengMasterBuff3_Tar,
    StrengMasterBuff4_Tar,
    EquipRefinBuff1_Tar,
    EquipRefinBuff2_Tar,
    MagicBuff1_Tar,
    MagicBuff2_Tar,
    MagicBuff3_Tar,
    RefinStrumentsBuff1_Tar,
    RefinStrumentsBuff2_Tar,
}


public class StrongMasterContainer:ContainerWindowBase
{
    
    public static EquipData[] mEquipArr;
    public static EquipData[] mMagicArr;
    public static PipeLineFactory<PL_MasterEquip_1, D_MasterEquip_1> mPipeLineFactory_1 = new PipeLineFactory<PL_MasterEquip_1, D_MasterEquip_1>();
    public static PipeLineFactory<PL_MasterEquip_2, D_MasterEquip_2> mPipeLineFactory_2 = new PipeLineFactory<PL_MasterEquip_2, D_MasterEquip_2>();
    public static ExternalDataStation<PL_MasterEquip_2, D_MasterEquip_2> exDataStation_2;


    public override void OnOpen() {
        base.OnOpen();
        List<EquipData> equipList = new List<EquipData>();
        for (int i = 0; i < 4; i++) {
            var equip=TeamManager.GetRoleEquipDataByCurTeamPos(i);
            if (equip != null) equipList.Add(equip);
        }
        mEquipArr = equipList.ToArray();

        List<EquipData> magicList = new List<EquipData>();
        for (int i = 0; i < 2; i++) {
            var magic = TeamManager.GetMagicDataByCurTeamPos(i);
            if (magic != null) magicList.Add(magic);
        }
        mMagicArr = magicList.ToArray();

        //added by xuke
        // 如果法器没有穿齐则法器相关页签要灰掉
        bool _isMagicTabActive = TeamManager.IsMagicEquipFull(TeamManager.mCurTeamPos)? true : false;
     
        GameCommon.FindComponent<UIButton>(mGameObjUI,"magicStren").isEnabled = _isMagicTabActive;
        GameCommon.FindComponent<UIButton>(mGameObjUI, "magicRefine").isEnabled = _isMagicTabActive;
        //end
        DataCenter.OpenWindow(UIWindowString.master_equipStren);
    }

    protected override void OpenInit() {
        base.OpenInit();
        windowNameArr = new string[4]{
            UIWindowString.master_equipStren,
            UIWindowString.master_equipRefine,
            UIWindowString.master_magicStren,
            UIWindowString.master_magicRefine
        };
        toggleNameArr = new string[4] { "equipStren", "equipRefine", "magicStren", "magicRefine" };
        ContainerInit();

        var exDataStation_1 = new ExternalDataStation<PL_MasterEquip_1, D_MasterEquip_1>("Strengthen_1");
        mPipeLineFactory_1.GetRefresherDict(exDataStation_1.GetRefresherDict());

        mPipeLineFactory_1.refresherDict[PL_MasterEquip_1.Set_Name].handlerFunc=datas => datas.GetInt(D_MasterEquip_1.tid);
        mPipeLineFactory_1.refresherDict[PL_MasterEquip_1.Set_Icon].handlerFunc=datas => datas.GetInt(D_MasterEquip_1.tid);
        mPipeLineFactory_1.refresherDict[PL_MasterEquip_1.Set_Level_Slider].handlerFunc=datas => datas.GetFloat(D_MasterEquip_1.CurLevel)/datas.GetInt(D_MasterEquip_1.NextLevel);
        mPipeLineFactory_1.refresherDict[PL_MasterEquip_1.Set_Level_Num].handlerFunc = datas => datas.GetInt(D_MasterEquip_1.CurLevel) + "/" + datas.GetInt(D_MasterEquip_1.NextLevel);

        exDataStation_2 = new ExternalDataStation<PL_MasterEquip_2, D_MasterEquip_2>("Strengthen_2");
        mPipeLineFactory_2.GetRefresherDict(exDataStation_2.GetRefresherDict());
    }

    public static int GetMinNextStrengthenLevel(int strenthenLevel, string fieldName)
    {
        int ret = strenthenLevel;
        NiceTable table = TableManager.GetTable("StrengMaster");
        for (int i = 0; i < table.GetAllRecord().Count; i++)
        {
            DataRecord record = table.GetRecord(i);
            if (record != null && ret < record[fieldName])
            {
                ret = record[fieldName];
                break;
            }
        }

        return ret;
    }

    public static Tuple<int, int> GetLevel(int level, string fieldName) {
        //这种算法，策划表中不能插0，暂未优化
        int index = 0;
        int ret = 0; //这是要的值
        NiceTable table=TableManager.GetTable("StrengMaster");
        while (table.GetRecord(index) != null && level >= table.GetRecord(index).getData(fieldName))
        {
            if (table.GetRecord(index).getData(fieldName) != 0)
            {
                ret++;
            }
            index++;
        }
        return new Tuple<int, int>(ret, table.GetRecord(ret).getData(fieldName));
    }

    public static void CloseAllWindow() {
        DataCenter.CloseWindow(UIWindowString.master_container);
    }

}

class StrongMasterBase : tWindow {

    protected string GetBuffDescribe(int index) {
        if (index == 0) return "";
        //else return string.Format(AffectBuffer.GetAffectName(index)+"+{0}",AffectBuffer.GetAffectValue(index));
        // add by LC
        // begin
        else return AffectBuffer.GetAffectName(index);
        // end

    }

    protected virtual void StrongMasterInit(string levelName) {
        
        StrongMasterContainer.mPipeLineFactory_1.ExecuteAllPipeLine();
    }
}

class StrengthenEquip : StrongMasterBase {
    protected DataGroup<D_MasterEquip_1> GetDataGroup(EquipData equip) {
        DataGroup<D_MasterEquip_1> dataGroup = new DataGroup<D_MasterEquip_1>();
        dataGroup.SetDictValue(D_MasterEquip_1.tid, equip.tid);
        dataGroup.SetDictValue(D_MasterEquip_1.CurLevel, equip.strengthenLevel);
        int iNextLevel = StrongMasterContainer.GetMinNextStrengthenLevel(equip.strengthenLevel, "EQUIP_STERNG_LEVEL");
        dataGroup.SetDictValue(D_MasterEquip_1.NextLevel, iNextLevel == 0 ? equip.strengthenLevel : iNextLevel);
        return dataGroup;
    }
    public override void OnOpen() {
        base.OnOpen();
        if (StrongMasterContainer.mEquipArr == null || StrongMasterContainer.mEquipArr.Length == 0) return;
        var container = GetUIGridContainer("equipList", StrongMasterContainer.mEquipArr.Length);
        var goList=container.controlList;
        List<DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>> dataGoList=new List<DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>>();
        for (int i = 0; i < goList.Count; i++) {
            var go = goList[i];
            var equip = StrongMasterContainer.mEquipArr[i];
            dataGoList.Add(new DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>(GetDataGroup(equip), go));
            AddButtonAction(go,()=>{
                DataCenter.OpenWindow("EQUIP_INFO_WINDOW", equip);
                DataCenter.SetData("EQUIP_INFO_WINDOW", "STRENGTHEN", equip);
                StrongMasterContainer.CloseAllWindow();
            });
        }
        StrongMasterContainer.mPipeLineFactory_1.GetDataGoList(dataGoList);

        StrongMasterInit("EQUIP_STERNG_LEVEL");
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLevel);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLevel);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_1);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_2);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_3);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_4);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_1);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_2);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_3);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_4);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLevel_Explain);


    }

    protected override void StrongMasterInit(string levelName)
    {
        base.StrongMasterInit(levelName);

        int minLevel = StrongMasterContainer.mEquipArr.Select(equip => StrongMasterContainer.GetLevel(equip.strengthenLevel, levelName).field1).Min();

        int minNextLevel = StrongMasterContainer.mEquipArr.Select(equip => StrongMasterContainer.GetLevel(StrongMasterContainer.GetMinNextStrengthenLevel(equip.strengthenLevel, levelName), levelName).field2).Min();

        int maxMasterLevel = 0;
        int index = 0;
        NiceTable table = TableManager.GetTable("StrengMaster");
        while (true)
        {
            if ((object)table.GetData(index, levelName) == null)
            {
                break;
            }
            if (table.GetData(index, levelName) == 0 && index != 0)
            {
                maxMasterLevel = index - 1;
                break;
            }
            else
            {
                index += 1;
            }
        }

        var dataGroup = StrongMasterContainer.exDataStation_2.GetDataGroup(minLevel);
        
        Func<string, int, int> getValueByTable = (colName, _index) => TableManager.GetTable("StrengMaster").GetRecord(_index).getData(colName);
        dataGroup.SetDictValue(D_MasterEquip_2.CurLevel, minLevel);
        dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff1, getValueByTable("EQUIP_STERNG_BUFF1", minLevel));
        dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff2, getValueByTable("EQUIP_STERNG_BUFF2", minLevel));
        dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff3, getValueByTable("EQUIP_STERNG_BUFF3", minLevel));
        dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff4, getValueByTable("EQUIP_STERNG_BUFF4", minLevel));

        int nextLevel = minLevel + 1;
        if (minLevel + 1 > maxMasterLevel)
        {
            nextLevel = maxMasterLevel;
            dataGroup.SetDictValue(D_MasterEquip_2.TarLevel, nextLevel);
            dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff1_Tar, 0);
            dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff2_Tar, 0);
            dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff3_Tar, 0);
            dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff4_Tar, 0);
        }
        else
        {
            dataGroup.SetDictValue(D_MasterEquip_2.TarLevel, nextLevel);
            dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff1_Tar, getValueByTable("EQUIP_STERNG_BUFF1", nextLevel));
            dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff2_Tar, getValueByTable("EQUIP_STERNG_BUFF2", nextLevel));
            dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff3_Tar, getValueByTable("EQUIP_STERNG_BUFF3", nextLevel));
            dataGroup.SetDictValue(D_MasterEquip_2.StrengMasterBuff4_Tar, getValueByTable("EQUIP_STERNG_BUFF4", nextLevel));
        }

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLevel].handlerFunc = datas => string.Format("装备强化大师{0}级", datas.GetInt(D_MasterEquip_2.CurLevel));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLevel].handlerFunc = datas => maxMasterLevel == minLevel ? "[99ff66]恭喜您已满级" : string.Format("装备强化大师[99ff66]{0}[-]级", datas.GetInt(D_MasterEquip_2.TarLevel));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_1].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.StrengMasterBuff1));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_2].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.StrengMasterBuff2));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_3].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.StrengMasterBuff3));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_4].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.StrengMasterBuff4));

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_1].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.StrengMasterBuff1_Tar));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_2].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.StrengMasterBuff2_Tar));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_3].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.StrengMasterBuff3_Tar));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_4].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.StrengMasterBuff4_Tar));

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLevel_Explain].handlerFunc =  datas =>minLevel!=maxMasterLevel?string.Format("全身装备强化{0}级", minNextLevel):"";

        StrongMasterContainer.mPipeLineFactory_2.GetDataGo(new DataGameObject<PL_MasterEquip_2, D_MasterEquip_2>(dataGroup, mGameObjUI));
    }
}

class RefineEquip : StrongMasterBase {
    protected DataGroup<D_MasterEquip_1> GetDataGroup(EquipData equip) {
        DataGroup<D_MasterEquip_1> dataGroup = new DataGroup<D_MasterEquip_1>();
        dataGroup.SetDictValue(D_MasterEquip_1.tid, equip.tid);
        dataGroup.SetDictValue(D_MasterEquip_1.CurLevel, equip.refineLevel);
        int iNextLevel = StrongMasterContainer.GetMinNextStrengthenLevel(equip.refineLevel, "EQUIP_REFINE_LEVEL");
        dataGroup.SetDictValue(D_MasterEquip_1.NextLevel, iNextLevel == 0 ? equip.refineLevel : iNextLevel);
        return dataGroup;
    }
    public override void OnOpen() {
        base.OnOpen();
        if (StrongMasterContainer.mEquipArr == null || StrongMasterContainer.mEquipArr.Length == 0) return;
        var container = GetUIGridContainer("equipList", StrongMasterContainer.mEquipArr.Length);
        var goList = container.controlList;
        List<DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>> dataGoList = new List<DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>>();
        for (int i = 0; i < goList.Count; i++) {
            var go = goList[i];
            var equip = StrongMasterContainer.mEquipArr[i];
            dataGoList.Add(new DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>(GetDataGroup(equip), go));
            AddButtonAction(go, () => {
                DataCenter.OpenWindow("EQUIP_INFO_WINDOW", equip);
                DataCenter.SetData("EQUIP_INFO_WINDOW", "EQUIP_REFINE", equip);
                StrongMasterContainer.CloseAllWindow();
            });
        }
        StrongMasterContainer.mPipeLineFactory_1.GetDataGoList(dataGoList);
        StrongMasterInit("EQUIP_REFINE_LEVEL");
        
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLevel);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLevel);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_1);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_2);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_1);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_2);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLevel_Explain);
    }

    protected override void StrongMasterInit(string levelName)
    {
        base.StrongMasterInit(levelName);

        int minLevel = StrongMasterContainer.mEquipArr.Select(equip => StrongMasterContainer.GetLevel(equip.refineLevel, levelName).field1).Min();
        int minNextLevel = StrongMasterContainer.mEquipArr.Select(equip => StrongMasterContainer.GetLevel(StrongMasterContainer.GetMinNextStrengthenLevel(equip.refineLevel, levelName), levelName).field2).Min();

        int maxMasterLevel = 0;
        int index = 0;
        NiceTable table = TableManager.GetTable("StrengMaster");
        while (true)
        {
            if ((object)table.GetData(index, levelName) == null)
            {
                break;
            }
            if (table.GetData(index, levelName) == 0 && index != 0)
            {
                maxMasterLevel = index - 1;
                break;
            }
            else
            {
                index += 1;
            }
        }

        var dataGroup = StrongMasterContainer.exDataStation_2.GetDataGroup(minLevel);
        dataGroup.SetDictValue(D_MasterEquip_2.CurLevel, minLevel);
        Func<string, int, int> getValueByTable = (colName, _index) => TableManager.GetTable("StrengMaster").GetRecord(_index).getData(colName);
        dataGroup.SetDictValue(D_MasterEquip_2.EquipRefinBuff1, getValueByTable("EQUIP_REFINE_BUFF1", minLevel));
        dataGroup.SetDictValue(D_MasterEquip_2.EquipRefinBuff2, getValueByTable("EQUIP_REFINE_BUFF2", minLevel));

        int nextLevel = minLevel + 1;
        if (minLevel + 1 > maxMasterLevel)
        {
            nextLevel = maxMasterLevel;
            dataGroup.SetDictValue(D_MasterEquip_2.TarLevel, nextLevel);
            dataGroup.SetDictValue(D_MasterEquip_2.EquipRefinBuff1_Tar, 0);
            dataGroup.SetDictValue(D_MasterEquip_2.EquipRefinBuff2_Tar, 0);
        }
        else
        {
            dataGroup.SetDictValue(D_MasterEquip_2.TarLevel, nextLevel);
            dataGroup.SetDictValue(D_MasterEquip_2.EquipRefinBuff1_Tar, getValueByTable("EQUIP_REFINE_BUFF1", nextLevel));
            dataGroup.SetDictValue(D_MasterEquip_2.EquipRefinBuff2_Tar, getValueByTable("EQUIP_REFINE_BUFF2", nextLevel));
        }
        

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLevel].handlerFunc = datas => string.Format("装备精炼大师{0}级", datas.GetInt(D_MasterEquip_2.CurLevel));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLevel].handlerFunc = datas => maxMasterLevel == minLevel ? "[99ff66]恭喜您已满级" : string.Format("装备精炼大师[99ff66]{0}[-]级", datas.GetInt(D_MasterEquip_2.TarLevel));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_1].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.EquipRefinBuff1));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_2].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.EquipRefinBuff2));

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_1].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.EquipRefinBuff1_Tar));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_2].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.EquipRefinBuff2_Tar));

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLevel_Explain].handlerFunc = datas => minLevel != maxMasterLevel ? string.Format("全身装备精炼{0}级", minNextLevel) : "";

        StrongMasterContainer.mPipeLineFactory_2.GetDataGo(new DataGameObject<PL_MasterEquip_2, D_MasterEquip_2>(dataGroup, mGameObjUI));
    }
}

class StrengthenMagic : StrongMasterBase{
    protected DataGroup<D_MasterEquip_1> GetDataGroup(EquipData equip) {
        DataGroup<D_MasterEquip_1> dataGroup = new DataGroup<D_MasterEquip_1>();
        dataGroup.SetDictValue(D_MasterEquip_1.tid, equip.tid);
        dataGroup.SetDictValue(D_MasterEquip_1.CurLevel, equip.strengthenLevel);
        int iNextLevel = StrongMasterContainer.GetMinNextStrengthenLevel(equip.strengthenLevel, "MAGIC_STERNG_LEVEL");
        dataGroup.SetDictValue(D_MasterEquip_1.NextLevel, iNextLevel == 0 ? equip.strengthenLevel : iNextLevel);
        return dataGroup;
    }
    public override void OnOpen() {
        base.OnOpen();
        if (StrongMasterContainer.mMagicArr == null || StrongMasterContainer.mMagicArr.Length == 0) return;
        var container=GetUIGridContainer("equipList",StrongMasterContainer.mMagicArr.Length);
        var goList = container.controlList;
        List<DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>> dataGoList = new List<DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>>();
        for (int i = 0; i < goList.Count; i++)
        {
            var go = goList[i];
            var equip = StrongMasterContainer.mMagicArr[i];
            dataGoList.Add(new DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>(GetDataGroup(equip), go));
            AddButtonAction(go, () =>
            {
                DataCenter.OpenWindow("MAGIC_INFO_WINDOW", equip);
                DataCenter.SetData("MAGIC_INFO_WINDOW", "STRENGTHEN", equip);
                StrongMasterContainer.CloseAllWindow();
            });
        }
        StrongMasterContainer.mPipeLineFactory_1.GetDataGoList(dataGoList);
        StrongMasterInit("MAGIC_STERNG_LEVEL");
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLevel);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLevel);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_1);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_2);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_3);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_1);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_2);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_3);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLevel_Explain);

    }

    protected override void StrongMasterInit(string levelName)
    {
        base.StrongMasterInit(levelName);

        int minLevel = StrongMasterContainer.mMagicArr.Select(equip => StrongMasterContainer.GetLevel(equip.strengthenLevel, levelName).field1).Min();
        int minNextLevel = StrongMasterContainer.mMagicArr.Select(equip => StrongMasterContainer.GetLevel(StrongMasterContainer.GetMinNextStrengthenLevel(equip.strengthenLevel, levelName), levelName).field2).Min();

        int maxMasterLevel = 0;
        int index = 0;
        NiceTable table = TableManager.GetTable("StrengMaster");
        while (true)
        {
            if ((object)table.GetData(index, levelName) == null)
            {
                break;
            }
            if (table.GetData(index, levelName) == 0 && index != 0)
            {
                maxMasterLevel = index-1;
                break;
            }
            else
            {
                index += 1;
            }
        }

        var dataGroup = StrongMasterContainer.exDataStation_2.GetDataGroup(minLevel);
        dataGroup.SetDictValue(D_MasterEquip_2.CurLevel, minLevel);
        Func<string, int, int> getValueByTable = (colName, _index) => TableManager.GetTable("StrengMaster").GetRecord(_index).getData(colName);
        dataGroup.SetDictValue(D_MasterEquip_2.MagicBuff1, getValueByTable("MAGIC_STERNG_BUFF1", minLevel));
        dataGroup.SetDictValue(D_MasterEquip_2.MagicBuff2, getValueByTable("MAGIC_STERNG_BUFF2", minLevel));
        dataGroup.SetDictValue(D_MasterEquip_2.MagicBuff3, getValueByTable("MAGIC_STERNG_BUFF3", minLevel));

        int nextLevel = minLevel + 1;
        if (minLevel + 1 > maxMasterLevel)
        {
            nextLevel = maxMasterLevel;
            dataGroup.SetDictValue(D_MasterEquip_2.TarLevel, nextLevel);
            dataGroup.SetDictValue(D_MasterEquip_2.MagicBuff1_Tar, 0);
            dataGroup.SetDictValue(D_MasterEquip_2.MagicBuff2_Tar, 0);
            dataGroup.SetDictValue(D_MasterEquip_2.MagicBuff3_Tar, 0);
        }
        else
        {
            dataGroup.SetDictValue(D_MasterEquip_2.TarLevel, nextLevel);
            dataGroup.SetDictValue(D_MasterEquip_2.MagicBuff1_Tar, getValueByTable("MAGIC_STERNG_BUFF1", nextLevel));
            dataGroup.SetDictValue(D_MasterEquip_2.MagicBuff2_Tar, getValueByTable("MAGIC_STERNG_BUFF2", nextLevel));
            dataGroup.SetDictValue(D_MasterEquip_2.MagicBuff3_Tar, getValueByTable("MAGIC_STERNG_BUFF3", nextLevel));
        }

        minNextLevel = TableManager.GetTable("StrengMaster").GetRecord(nextLevel).getData(levelName);

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLevel].handlerFunc = datas => string.Format("神器强化大师{0}级", datas.GetInt(D_MasterEquip_2.CurLevel));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLevel].handlerFunc = datas => maxMasterLevel == minLevel ? "[99ff66]恭喜您已满级" : string.Format("神器强化大师[99ff66]{0}[-]级", datas.GetInt(D_MasterEquip_2.TarLevel));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_1].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.MagicBuff1));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_2].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.MagicBuff2));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_3].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.MagicBuff3));

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_1].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.MagicBuff1_Tar));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_2].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.MagicBuff2_Tar));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_3].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.MagicBuff3_Tar));

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLevel_Explain].handlerFunc = datas => minLevel != maxMasterLevel ? string.Format("全身神器强化{0}级", minNextLevel) : "";

        StrongMasterContainer.mPipeLineFactory_2.GetDataGo(new DataGameObject<PL_MasterEquip_2, D_MasterEquip_2>(dataGroup, mGameObjUI));
    }
}

class RefineMagic : StrongMasterBase {
    protected DataGroup<D_MasterEquip_1> GetDataGroup(EquipData equip) {
        DataGroup<D_MasterEquip_1> dataGroup = new DataGroup<D_MasterEquip_1>();
        dataGroup.SetDictValue(D_MasterEquip_1.tid, equip.tid);
        dataGroup.SetDictValue(D_MasterEquip_1.CurLevel, equip.refineLevel);
        int iNextLevel = StrongMasterContainer.GetMinNextStrengthenLevel(equip.refineLevel, "MAGIC_REFINE_LEVEL");
        dataGroup.SetDictValue(D_MasterEquip_1.NextLevel, iNextLevel == 0 ? equip.refineLevel : iNextLevel);
        return dataGroup;
    }
    public override void OnOpen() {
        base.OnOpen();
        if (StrongMasterContainer.mMagicArr == null || StrongMasterContainer.mMagicArr.Length == 0) return;
        var container=GetUIGridContainer("equipList",StrongMasterContainer.mMagicArr.Length);
        var goList = container.controlList;
        List<DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>> dataGoList = new List<DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>>();
        for (int i = 0; i < goList.Count; i++)
        {
            var go = goList[i];
            var equip = StrongMasterContainer.mMagicArr[i];
            dataGoList.Add(new DataGameObject<PL_MasterEquip_1, D_MasterEquip_1>(GetDataGroup(equip), go));
            AddButtonAction(go, () =>
            {
                DataCenter.OpenWindow("MAGIC_INFO_WINDOW", equip);
                DataCenter.SetData("MAGIC_INFO_WINDOW", "MAGIC_REFINE", equip);
                StrongMasterContainer.CloseAllWindow();
            });
        }
        StrongMasterContainer.mPipeLineFactory_1.GetDataGoList(dataGoList);
        StrongMasterInit("MAGIC_REFINE_LEVEL");
        
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLevel);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLevel);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_1);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_CurLabel_2);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_1);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLabel_2);
        StrongMasterContainer.mPipeLineFactory_2.ExecutePipeLine(PL_MasterEquip_2.Set_TarLevel_Explain);
    }

    protected override void StrongMasterInit(string levelName)
    {
        base.StrongMasterInit(levelName);

        int minLevel = StrongMasterContainer.mMagicArr.Select(equip => StrongMasterContainer.GetLevel(equip.refineLevel, levelName).field1).Min();
        int minNextLevel = StrongMasterContainer.mMagicArr.Select(equip => StrongMasterContainer.GetLevel(StrongMasterContainer.GetMinNextStrengthenLevel(equip.refineLevel, levelName), levelName).field2).Min();

        int maxMasterLevel = 0;
        int index = 0;
        NiceTable table = TableManager.GetTable("StrengMaster");
        while (true)
        {
            if ((object)table.GetData(index, levelName) == null)
            {
                break;
            }
            if (table.GetData(index, levelName) == 0 && index != 0)
            {
                maxMasterLevel = index-1;
                break;
            }
            else
            {
                index += 1;
            }
        }

        var dataGroup = StrongMasterContainer.exDataStation_2.GetDataGroup(minLevel);
        dataGroup.SetDictValue(D_MasterEquip_2.CurLevel, minLevel);
        Func<string, int, int> getValueByTable = (colName, _index) => TableManager.GetTable("StrengMaster").GetRecord(_index).getData(colName);

        int nextLevel = minLevel + 1;
        if (minLevel + 1 > maxMasterLevel)
        {
            nextLevel = maxMasterLevel;
            dataGroup.SetDictValue(D_MasterEquip_2.TarLevel, nextLevel);
            dataGroup.SetDictValue(D_MasterEquip_2.RefinStrumentsBuff1_Tar, 0);
            dataGroup.SetDictValue(D_MasterEquip_2.RefinStrumentsBuff2_Tar, 0);
        }
        else
        {
            dataGroup.SetDictValue(D_MasterEquip_2.TarLevel, nextLevel);
            dataGroup.SetDictValue(D_MasterEquip_2.RefinStrumentsBuff1_Tar, getValueByTable("MAGIC_REFINE_BUFF1", nextLevel));
            dataGroup.SetDictValue(D_MasterEquip_2.RefinStrumentsBuff2_Tar, getValueByTable("MAGIC_REFINE_BUFF2", nextLevel));
        }

        minNextLevel = TableManager.GetTable("StrengMaster").GetRecord(nextLevel).getData(levelName);
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLevel].handlerFunc = datas => string.Format("神器精炼大师{0}级", datas.GetInt(D_MasterEquip_2.CurLevel));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLevel].handlerFunc = datas => maxMasterLevel == minLevel ? "[99ff66]恭喜您已满级" : string.Format("神器精炼大师[99ff66]{0}[-]级", datas.GetInt(D_MasterEquip_2.TarLevel));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_1].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.RefinStrumentsBuff1));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_CurLabel_2].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.RefinStrumentsBuff2));

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_1].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.RefinStrumentsBuff1_Tar));
        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLabel_2].handlerFunc = datas => GetBuffDescribe(datas.GetInt(D_MasterEquip_2.RefinStrumentsBuff2_Tar));

        StrongMasterContainer.mPipeLineFactory_2.refresherDict[PL_MasterEquip_2.Set_TarLevel_Explain].handlerFunc = datas => minLevel != maxMasterLevel ? string.Format("全身神器精炼{0}级", minNextLevel) : "";

        StrongMasterContainer.mPipeLineFactory_2.GetDataGo(new DataGameObject<PL_MasterEquip_2, D_MasterEquip_2>(dataGroup, mGameObjUI));
    }
}
