using UnityEngine;
using System.Collections;
using Logic;
using System.Linq;
using System.Collections.Generic;
using System;

class EquipResolveWindow : RecoverBase<EquipData> 
{
    //威名＋基础银币＋强化银币消耗＋精炼经验折算成精炼石数量
    //威名+银币+炼精石X4（6）

    public override void Init()
    {
        base.Init();
        EventCenter.Self.RegisterEvent(resolveBtn + "equip", new DefineFactory<Resolve_Equip>());
        EventCenter.Self.RegisterEvent(autoAddBtn + "equip", new DefineFactory<AutoAdd_Equip>());
        EventCenter.Self.RegisterEvent(normalAddBtn + "equip", new DefineFactory<NormalAdd_Equip>());
        nativeGridCount = 6;
        resolventGridMaxCount = 6;
        
        resolveCondition = (equipData) =>
        {
            return
                equipData.teamPos < 0 && equipData.strengthenLevel == 1 && equipData.refineLevel == 0;
                //&& TableCommon.GetNumberFromRoleEquipConfig(equipData.tid, "QUALITY") > 1;
        };

        resolveConditionNormalAdd = (equipData) =>
        {
            return
                equipData.teamPos < 0;
            //&& TableCommon.GetNumberFromRoleEquipConfig(equipData.tid, "QUALITY") > 1;
        };
    }


    protected override void AutoAdd()
    {
        var logicData = DataCenter.GetData("EQUIP_DATA") as EquipLogicData;


        var itemDataArrTemp = logicData.mDicEquip.Values.Where(value => resolveCondition(value)).ToArray();

        Array.Sort(itemDataArrTemp, new ItemDataBaseComparer<EquipData>(DataCenter.mRoleEquipConfig, "QUALITY"));
        InsertTtemDataArrByIndex(itemDataArrTemp.ToList());

        int count = itemDataArr.Length < nativeGridCount ? itemDataArr.Length : nativeGridCount;

        // By XiaoWen
        // Begin
        if (count == 0)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_RESOLVE_AUTO_EQUIPMENT);
            return;
        }
        // End

        itemDataArr = itemDataArr.Take(count).ToArray();
        ShowNativeUI();
        CreateResolventInfo();
        ShowResolventUI();
    }

    protected override void CreateResolventInfo()
    {
        base.CreateResolventInfo();
        int token=0;
        int baseMoney = 0;
        int strengthenMoney = 0;
        int refineStoneExp = 0;

        for (int i = 0; i < itemDataArr.Length; i++)
        {
            var data = itemDataArr[i];
            var qualityLevel = TableCommon.GetNumberFromRoleEquipConfig(data.tid, "QUALITY");
            qualityLevel = (qualityLevel == 0) ? 0 : qualityLevel;//temp!!!!!!!!!
            DEBUG.Log(qualityLevel.ToString());
            var equipRecoverConfig = DataCenter.mEquipRecoverConfig.GetRecord(qualityLevel);
            token += (int)equipRecoverConfig.get("EQUIP_TOKEN");
            baseMoney += (int)equipRecoverConfig.get("MONEY");
            strengthenMoney += data.strengCostGold;
            //strengthenMoney += data.strengCostGold;
            var refineLevel = (data.refineLevel == 0) ? 0 : data.refineLevel;//temp!!!!!!!!!
            refineStoneExp += (int)DataCenter.mEquipRefineLvConfig.GetRecord(refineLevel).get("TOTAL_EXP_" + qualityLevel.ToString())+data.refineExp;
        }

        int[] refineStoneArr = new int[4];
        for (int i = 0; i < refineStoneArr.Length; i++)
        {
            if (refineStoneExp == 0) break;
            int exp = DataCenter.mEquipRefineStoneConfig.GetRecord(2000007-i).get("REFINE_STONE_EXP");
            refineStoneArr[i] = refineStoneExp / exp;
            refineStoneExp %= exp;
        }

        AddResolventInfoList(0, token, (int)ITEM_TYPE.PRESTIGE);
        AddResolventInfoList(0, baseMoney + strengthenMoney, (int)ITEM_TYPE.GOLD);
        AddResolventInfoList(0, refineStoneArr[0], 2000007);
        AddResolventInfoList(0, refineStoneArr[1], 2000006);
        AddResolventInfoList(0, refineStoneArr[2], 2000005);
        AddResolventInfoList(0, refineStoneArr[3], 2000004);

    }

    protected override void NormalAdd()
    {
        OpenBagObject<EquipData> openObj = new OpenBagObject<EquipData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.RESOLVE;
        openObj.mFilterCondition = resolveConditionNormalAdd;
        if (itemDataArr != null) openObj.mSelectList = itemDataArr.ToList();
        openObj.mMultipleSelectAction = list => {
            itemDataArr = list.ToArray();
            DataCenter.OpenWindow(UIWindowString.recover_equipResolve);
        };
        DataCenter.SetData("BAG_EQUIP_WINDOW", "OPEN", openObj);
    }

    protected override void ShowNativeUI()
    {
        if(itemDataArr!=null)
        {
            for (int i = 0; i < itemDataArr.Length; i++)
            {
                var spriteName = TableCommon.GetStringFromRoleEquipConfig(itemDataArr[i].tid, "ICON_SPRITE_NAME");
                var atlasName = TableCommon.GetStringFromRoleEquipConfig(itemDataArr[i].tid, "ICON_ATLAS_NAME");

                if (nativeUIArr != null && nativeUIArr.Length > i)
                {
                    nativeUIArr[i].SetActive(true);
                    GameCommon.SetIcon(nativeUIArr[i].GetComponent<UISprite>(), atlasName, spriteName);
                    
                    //添加删除事件
                    GameObject board = nativeUIArr[i];
                    int temp = i;
                    GameCommon.FindObject(board, "resolve_btn_close").SetActive(true);
                    AddButtonAction(GameCommon.FindObject(board, "resolve_btn_close"), () =>
                    {
                        RemoveResolveCloseBtn();           //把最后面的按钮删除掉而已
                        RemoveitemDataArrByIndex(temp);    //移除掉数据
                        DataCenter.OpenWindow(UIWindowString.recover_equipResolve);
                    });
                }
            }
        }
    }

    protected override void Resolve()
    {
        if (itemDataArr == null) return;
        if (itemDataArr.Length == 0) return;
        Action action = () =>
        {
            CS_EquipDisenchant cs = new CS_EquipDisenchant(GetItemDataArr());

            HttpModule.Instace.SendGameServerMessage(cs, "CS_EquipDisenchant", RequestResolveSuccess, RequestResolveFail);
        };
        DataCenter.OpenMessageOkWindow(TableCommon.getStringFromStringList(STRING_INDEX.RECOVER_RESOLVE_EQUIP), action);
    }


    class Resolve_Equip : CEvent
    {
        public override bool _DoEvent()
        {
            DataCenter.SetData(UIWindowString.recover_equipResolve, UIWindowString.resolveMethod, true);
            return true;
        }
    }
    class AutoAdd_Equip : CEvent
    {
        public override bool _DoEvent()
        {
            DataCenter.SetData(UIWindowString.recover_equipResolve, UIWindowString.autoAddMethod, true);
            return true;
        }
    }
    class NormalAdd_Equip : CEvent
    {
        public override bool _DoEvent()
        {
            DataCenter.SetData(UIWindowString.recover_equipResolve, UIWindowString.normalAddMethod, true);
            return true;
        }
    }


}
