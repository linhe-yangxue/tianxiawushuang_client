using UnityEngine;
using System.Collections;
using Logic;
using System.Linq;
using System.Collections.Generic;
using System;

class PetRebirthWindow : RecoverBase<PetData>
{
    //基础符灵＋升级银币消耗＋升级经验折算成经验蛋数量＋突破石+突破消耗的同ID符灵＋技能书+技能书消耗的银币＋天命石
    //符灵+银币+经验蛋X3+突破石+技能书+天命石（8）


    public override void Init() {
        base.Init();
        EventCenter.Self.RegisterEvent(resolveBtn + "rebirth", new DefineFactory<Resolve_PetRebirth>());
        EventCenter.Self.RegisterEvent(normalAddBtn + "rebirth", new DefineFactory<NormalAdd_PetRebirth>());

        nativeGridCount = 1;
        resolventGridMaxCount = 8;

        resolveCondition = (petData) => {
            return
                petData.teamPos < 0
                &&TableCommon.GetNumberFromActiveCongfig(petData.tid,"STAR_LEVEL")>1
                && petData.tid != 30305
                && petData.tid != 30299
                && petData.tid != 30293
                && petData.inFairyland == 0 // 屏蔽寻仙
                &&(petData.level>1||petData.breakLevel>0
                ||petData.skillLevel.Where(level=>level>1).Count()>0);
        };

    }



    protected override void CreateResolventInfo() {
        base.CreateResolventInfo();
        var data = itemDataArr[0];
        var starLevel = TableCommon.GetNumberFromActiveCongfig(data.tid, "STAR_LEVEL");
        var petRecoverConfig = DataCenter.mPetRecoverConfig.GetRecord(starLevel);

        int totalExp = (int)DataCenter.mPetLevelExpTable.GetRecord(data.level).get("TOTAL_EXP_" + starLevel) + data.exp;
        int breakStoneCount = TableCommon.GetNumberFromBreakLevelConfig(data.breakLevel, "TOTAL_NEED_GEM_NUM");
        int breakStonePet = TableCommon.GetNumberFromBreakLevelConfig(data.breakLevel, "TOTAL_ACTIVE_NUM");
       
        int skillMoney = 0;
        int skillBookCount = 0;

        for (int i = 0; i < data.skillLevel.Length; i++) {
            var skillLevel = data.skillLevel[i];
            skillMoney += TableCommon.GetNumberFromSkillCost(skillLevel, "TOTAL_MONEY_COST");
            skillBookCount += TableCommon.GetNumberFromSkillCost(skillLevel, "TOTAL_SKILL_BOOK_COST");
        }
        int baseMoney = TableCommon.GetNumberFromConfig(data.level, "NEED_EXP_QUALITY_" + starLevel, DataCenter.mPetLevelExpTable);
        
        int expMoney=totalExp;//升级的money和升级的经验是一样的;
        int breakMoney = TableCommon.GetNumberFromConfig(data.breakLevel - 1, "TOTAL_NEED_COIN_NUM", DataCenter.mBreakLevelConfig);


        int fateStoneCost = data.fateStoneCost;
        


        //计算经验蛋数量
        int[] eggArr = new int[3];

        for (int i = 0; i < eggArr.Length; i++) {
            if (totalExp == 0) break;
            int exp = TableCommon.GetNumberFromActiveCongfig(30305 - i * 6, "DROP_EXP");
            eggArr[i] = totalExp / exp;
            totalExp %= exp;
        }

        AddResolventInfoList(0, 1 + breakStonePet, data.tid);
        AddResolventInfoList(0, breakMoney+expMoney + skillMoney, (int)ITEM_TYPE.GOLD);
        AddResolventInfoList(0, eggArr[0], 30305);
        AddResolventInfoList(0, eggArr[1], 30299);
        AddResolventInfoList(0, eggArr[2], 30293);
        AddResolventInfoList(0, breakStoneCount, (int)ITEM_TYPE.BREAK_STONE);
        AddResolventInfoList(0, skillBookCount, (int)ITEM_TYPE.SKILL_BOOK);
        AddResolventInfoList(0, fateStoneCost, (int)ITEM_TYPE.FATE_STONE);
    }

    protected override void NormalAdd() {
        OpenBagObject<PetData> openObj = new OpenBagObject<PetData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.USE;
        openObj.mFilterCondition = resolveCondition;
        openObj.mSortCondition = (tempList) =>
        {
            DataCenter.Set("DESCENDING_ORDER_QUALITY", true);
            return GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByTeamPosType);
        };
        openObj.mSelectAction = itemData => {
            itemDataArr = new PetData[1];
            itemDataArr[0] = itemData;
            DataCenter.OpenWindow(UIWindowString.recover_petRebirth);
        };
        DataCenter.SetData("BAG_PET_WINDOW", "OPEN", openObj);
    }

    protected override void Resolve() {
        if (itemDataArr == null) return;
        if (RoleLogicData.Self.diamond < 50) { 
			GameCommon.ToGetDiamond();
			//DataCenter.OpenMessageWindow ("元宝不足");
		}
        else {
            Action action = ()=>{
                CS_PetRebirth cs = new CS_PetRebirth(GetItemData());
                HttpModule.Instace.SendGameServerMessage(cs, "CS_PetRebirth", RequestResolveSuccess, RequestResolveFail);
            };
            DataCenter.OpenMessageOkWindow(TableCommon.getStringFromStringList(STRING_INDEX.RECOVER_REBIRTH_PET), action);
        }
    }



    protected override void ShowNativeUI() {
        var spriteName = TableCommon.GetStringFromActiveCongfig(itemDataArr[0].tid, "HEAD_SPRITE_NAME");
        var atlasName = TableCommon.GetStringFromActiveCongfig(itemDataArr[0].tid, "HEAD_ATLAS_NAME");

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
                DataCenter.OpenWindow(UIWindowString.recover_petRebirth);
            });
        }
    }

    protected override void RequestResolveSuccess(string text) {
        base.RequestResolveSuccess(text);
        PackageManager.RemoveItem((int)ITEM_TYPE.YUANBAO, -1, 50);
    }

    class Resolve_PetRebirth : CEvent
    {
        public override bool _DoEvent() {
            DataCenter.SetData(UIWindowString.recover_petRebirth, UIWindowString.resolveMethod, true);
            return true;
        }
    }

    class NormalAdd_PetRebirth : CEvent
    {
        public override bool _DoEvent() {
            DataCenter.SetData(UIWindowString.recover_petRebirth, UIWindowString.normalAddMethod, true);
            return true;
        }
    }

}
