using UnityEngine;
using System.Collections;
using Logic;
using System.Linq;
using System.Collections.Generic;
using System;
class MagicRecastWindow :RecoverBase<EquipData>
{
    //基础法器＋经验计算强化经验石＋强化金币消耗＋法器精炼石+精炼消耗的同ID法器
    //法器 + 经验石 + 银币 + 精炼石X4（7）

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent(resolveBtn + "magic", new DefineFactory<Resolve_Magic>());
        EventCenter.Self.RegisterEvent(normalAddBtn + "magic", new DefineFactory<NormalAdd_Magic>());

        nativeGridCount = 1;
        resolventGridMaxCount = 7;

        resolveCondition = equipData =>
            equipData.teamPos < 0
            && (equipData.strengthenLevel>1
            ||equipData.refineLevel > 0);
    }


    protected override void CreateResolventInfo()
    {
        base.CreateResolventInfo();
        
        var data = itemDataArr[0];
       
        int qualityLevel = TableCommon.GetNumberFromRoleEquipConfig(data.tid, "QUALITY");
//        qualityLevel = (qualityLevel == 0) ? 1 : qualityLevel;//temp!!!!!!!!!
//
//        int refineLevel = (data.refineLevel == 0) ? 1 : data.refineLevel;//temp!!!!!!!!!
//        int strengthenLevel = (data.strengthenLevel == 0) ? 1 : data.strengthenLevel;//temp!!!!!!!!!

		int refineLevel = data.refineLevel;
		int strengthenLevel = data.strengthenLevel;
        int expTotal = DataCenter.mMagicEquipLvConfig.GetRecord(strengthenLevel).get("TOTAL_EXP_" + qualityLevel) + data.strengthenExp;
		int strengthenMoney = expTotal;
        int refineMoney = DataCenter.mMagicEquipRefineConfig.GetRecord(refineLevel).get("TOTAL_REFINE_EQUIP_MONEY");



        int refineStone = DataCenter.mMagicEquipRefineConfig.GetRecord(refineLevel).get("TOTAL_REFINESTONE_NUM");
        int magicCount = DataCenter.mMagicEquipRefineConfig.GetRecord(refineLevel).get("TOTAL_REFINE_EQUIP_NUM") + 1;


        int[] expStoneArr = new int[3];
        for (int i = 0; i < expStoneArr.Length; i++)
        {
            if (expTotal == 0) break;
            int exp = DataCenter.mRoleEquipConfig.GetRecord(15203 - i).get("SUPPLY_EXP");
            expStoneArr[i] = expTotal / exp;
            expTotal %= exp;
        }

        int _totalMoney=refineMoney+strengthenMoney;
        AddResolventInfoList(0, magicCount,data.tid);
		AddResolventInfoList(0, _totalMoney,(int)ITEM_TYPE.GOLD);
		AddResolventInfoList(0, refineStone,(int)ITEM_TYPE.MAGIC_REFINE_STONE);
        AddResolventInfoList(0, expStoneArr[0], 15203);
        AddResolventInfoList(0, expStoneArr[1],15202);
        AddResolventInfoList(0, expStoneArr[2],15201); 
    }

    //protected override void ShowResolventUI()
    //{


    //    if (itemDataArr == null) return;

    //    var count = resolventInfoList.Count;
    //    var list = GetComponent<UIGridContainer>("resolvent_group").controlList;
    //    for (int i = 0; i < count; i++)
    //    {
    //        string atlasName = (i == 0) ? TableCommon.GetStringFromRoleEquipConfig(itemDataArr[0].tid, "ICON_ATLAS_NAME") : DataCenter.mItemIcon.GetData(resolventInfoList[i][0], "ITEM_ICON_ATLAS");
    //        string spriteName = (i == 0) ? TableCommon.GetStringFromRoleEquipConfig(itemDataArr[0].tid, "ICON_SPRITE_NAME") : DataCenter.mItemIcon.GetData(resolventInfoList[i][0], "ITEM_ICON_SPRITE");
    //        GameCommon.SetIcon(list[i].transform.Find("resolvent_icon_btn").GetComponent<UISprite>(), atlasName, spriteName);
    //        list[i].SetActive(true);
    //        var label = list[i].GetComponentInChildren<UILabel>();
    //        label.text = "×" + resolventInfoList[i][1];
    //    }

    //}

    protected override void NormalAdd()
    {
       
        //openObj.mFilterCondition = (itemData) => {
        //    return !itemData.IsInTeam() && iType == PackageManager.GetSlotPosByTid(itemData.tid);
        //};

        //openObj.mSelectAction = (upItemData) => {
        //    EquipData downEquipData = TeamManager.GetRoleEquipDataByCurTeamPos(PackageManager.GetSlotPosByTid(upItemData.tid));

        //    TeamManager.RequestChangeTeamPos((int)TeamManager.mCurTeamPos, upItemData.itemId, upItemData.tid,
        //        downEquipData != null ? downEquipData.itemId : -1, downEquipData != null ? downEquipData.tid : -1);
        //};

        //DataCenter.SetData("BAG_EQUIP_WINDOW", "OPEN", openObj);

        OpenBagObject<EquipData> openObj = new OpenBagObject<EquipData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.USE;
        openObj.mFilterCondition = resolveCondition;
        openObj.mSelectAction = itemData =>
        {
            itemDataArr = new EquipData[1];
            itemDataArr[0] = itemData;

            DEBUG.Log(itemData.refineLevel.ToString());
            DEBUG.Log(itemData.strengthenLevel.ToString());
            
            DataCenter.OpenWindow(UIWindowString.recover_magicRecast);
        };
        DataCenter.SetData("BAG_MAGIC_WINDOW", "OPEN", openObj);

    }

    protected override void Resolve()
    {
        if (itemDataArr == null) return;
		if (RoleLogicData.Self.diamond < 50) {
			GameCommon.ToGetDiamond();
			//DataCenter.OpenMessageWindow ("元宝不足");
		}
        else
        {
            Action action = () =>
            {
                CS_MagicRebirth cs = new CS_MagicRebirth(GetItemData());
                HttpModule.Instace.SendGameServerMessage(cs, "CS_MagicRebirth", RequestResolveSuccess, RequestResolveFail);
            };
            DataCenter.OpenMessageOkWindow(TableCommon.getStringFromStringList(STRING_INDEX.RECOVER_REBIRTH_MAGIC), action);
        }
    }

    protected override void ShowNativeUI()
    {
        var spriteName = TableCommon.GetStringFromRoleEquipConfig(itemDataArr[0].tid, "ICON_SPRITE_NAME");
        var atlasName = TableCommon.GetStringFromRoleEquipConfig(itemDataArr[0].tid, "ICON_ATLAS_NAME");

        if (nativeUIArr != null && nativeUIArr.Length > 0)
        {
            nativeUIArr[0].SetActive(true);
            GameCommon.SetIcon(nativeUIArr[0].GetComponent<UISprite>(), atlasName, spriteName);

            //添加删除事件
            GameObject board = nativeUIArr[0];
            int temp = 0;
            GameCommon.FindObject(board, "resolve_btn_close").SetActive(true);
            AddButtonAction(GameCommon.FindObject(board, "resolve_btn_close"), () =>
            {
                RemoveResolveCloseBtn();           //把最后面的按钮删除掉而已
                RemoveitemDataArrByIndex(temp);    //移除掉数据
                DataCenter.OpenWindow(UIWindowString.recover_magicRecast);
            });
        }
    }

    protected override void RequestResolveSuccess(string text)
    {
        base.RequestResolveSuccess(text);
        PackageManager.RemoveItem((int)ITEM_TYPE.YUANBAO, -1, 50);
    }

    class Resolve_Magic : CEvent
    {
        public override bool _DoEvent()
        {
            DataCenter.SetData(UIWindowString.recover_magicRecast, UIWindowString.resolveMethod, true);
            return true;
        }
    }

    class NormalAdd_Magic : CEvent
    {
        public override bool _DoEvent()
        {
            DataCenter.SetData(UIWindowString.recover_magicRecast, UIWindowString.normalAddMethod, true);
            return true;
        }
    }
    
}

