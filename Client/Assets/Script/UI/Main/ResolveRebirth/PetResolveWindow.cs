using UnityEngine;
using System.Collections;
using Logic;
using System.Linq;
using System.Collections.Generic;
using System;
class PetResolveWindow : RecoverBase<PetData> {
    //基础符魂＋基础银币＋升级银币消耗＋升级经验折算成经验蛋数量＋突破石+突破消耗的同ID符灵对应的基础符魂+突破消耗的同ID符灵对应的基础银币＋技能书+技能书消耗的银币＋天命石
    //符魂+银币+经验蛋X3+突破石+技能书+天命石（8）
    public override void Init() {
        base.Init();
        EventCenter.Self.RegisterEvent(resolveBtn + "pet", new DefineFactory<Resolve_Pet>());
        EventCenter.Self.RegisterEvent(autoAddBtn + "pet", new DefineFactory<AutoAdd_Pet>());
        EventCenter.Self.RegisterEvent(normalAddBtn + "pet", new DefineFactory<NormalAdd_Pet>());
        EventCenter.Self.RegisterEvent("Button_recover_resolve_btn_go_shop", new DefineFactory<Button_recover_resolve_btn_go_shop>());
        EventCenter.Self.RegisterEvent("Button_recover_rebirth_btn_goto_shop", new DefineFactory<Button_recover_rebirth_btn_goto_shop>());
        EventCenter.Self.RegisterEvent("Button_recover_equip_btn_equip", new DefineFactory<Button_recover_equip_btn_equip>());
        nativeGridCount = 6;
        resolventGridMaxCount = 8;

        resolveCondition = (petData) => {
            return
                petData.teamPos < 0
                && TableCommon.GetNumberFromActiveCongfig(petData.tid, "STAR_LEVEL") > 1
                && petData.tid != 30305
                && petData.tid != 30299
                && petData.tid != 30293
                && petData.inFairyland == 0;// 屏蔽寻仙
        };
    }

    protected override void AutoAdd() {
        var logicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        PetData[] itemDataArrTemp = logicData.mDicPetData.Values.Where(value => resolveCondition(value) && value.starLevel < 5).ToArray();

        Array.Sort(itemDataArrTemp, new ItemDataBaseComparer<PetData>(DataCenter.mActiveConfigTable, "STAR_LEVEL"));
        InsertTtemDataArrByIndex(itemDataArrTemp.ToList());

        int count = itemDataArr.Length < nativeGridCount ? itemDataArr.Length : nativeGridCount;

        // By XiaoWen
        // Begin
        if (count == 0)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_RESOLVE_AUTO_PET);
            return;
        }
        // End

        itemDataArr = itemDataArr.Take(count).ToArray();
        ShowNativeUI();
        CreateResolventInfo();
        ShowResolventUI();
    }

    protected override void CreateResolventInfo() {
        base.CreateResolventInfo();
        int baseGhost = 0;
        int baseMoney = 0;
        int totalExp = 0;
        int expMoney = 0;
        int breakStoneCount = 0;
        int breakStoneGhost = 0;
        int breakStoneMoney = 0;
        int skillMoney = 0;
        int skillBookCount = 0;
        int fateStoneCost = 0;
        int breakBaseMoney = 0;

        for (int i = 0; i < itemDataArr.Length; i++) {
            var data = itemDataArr[i];
            var starLevel = TableCommon.GetNumberFromActiveCongfig(data.tid, "STAR_LEVEL");
            var petRecoverConfig = DataCenter.mPetRecoverConfig.GetRecord(starLevel);
            baseGhost += (int)petRecoverConfig.get("PET_GHOST");
            baseMoney += (int)petRecoverConfig.get("MONEY");
            totalExp += (int)DataCenter.mPetLevelExpTable.GetRecord(data.level).get("TOTAL_EXP_" + starLevel) + data.exp;

            breakStoneCount += TableCommon.GetNumberFromBreakLevelConfig(data.breakLevel, "TOTAL_NEED_GEM_NUM");
            breakStoneGhost += (TableCommon.GetNumberFromBreakLevelConfig(data.breakLevel, "TOTAL_ACTIVE_NUM") * (int)petRecoverConfig.get("PET_GHOST"));
            breakStoneMoney += (TableCommon.GetNumberFromBreakLevelConfig(data.breakLevel, "TOTAL_ACTIVE_NUM") * (int)petRecoverConfig.get("MONEY"));
            breakBaseMoney += (TableCommon.GetNumberFromBreakLevelConfig(data.breakLevel, "TOTAL_NEED_COIN_NUM"));

            for (int j = 0; j < data.skillLevel.Length; j++) {
                var skillLevel = data.skillLevel[j];
                skillMoney += TableCommon.GetNumberFromSkillCost(skillLevel, "TOTAL_MONEY_COST");
                skillBookCount += TableCommon.GetNumberFromSkillCost(skillLevel, "TOTAL_SKILL_BOOK_COST");
            }
            fateStoneCost += data.fateStoneCost;
        }

        expMoney = totalExp;//升级的money和升级的经验是一样的;

        //计算经验蛋数量
        int[] eggArr = new int[3];

        for (int i = 0; i < eggArr.Length; i++) {
            if (totalExp == 0) break;
            int exp = TableCommon.GetNumberFromActiveCongfig(30305 - i * 6, "DROP_EXP");
            eggArr[i] = totalExp / exp;
            totalExp %= exp;
        }

        AddResolventInfoList(1000004, baseGhost + breakStoneGhost, (int)ITEM_TYPE.PET_SOUL);
        AddResolventInfoList(1000002, baseMoney + breakStoneMoney + expMoney + skillMoney + breakBaseMoney, (int)ITEM_TYPE.GOLD);
        AddResolventInfoList(30305, eggArr[0], 30305);
        AddResolventInfoList(30299, eggArr[1], 30299);
        AddResolventInfoList(30293, eggArr[2], 30293);
        AddResolventInfoList(0, breakStoneCount, (int)ITEM_TYPE.BREAK_STONE);
        AddResolventInfoList(0, skillBookCount, (int)ITEM_TYPE.SKILL_BOOK);
        AddResolventInfoList(0, fateStoneCost, (int)ITEM_TYPE.FATE_STONE);
    }

    protected override void NormalAdd() {
        OpenBagObject<PetData> openObj = new OpenBagObject<PetData>();
        openObj.mBagShowType = BAG_SHOW_TYPE.RESOLVE;
        openObj.mFilterCondition = resolveCondition;
        openObj.mSortCondition = (tempList) =>
        {
            List<PetData> petDataList = new List<PetData>();
            DataCenter.Set("DESCENDING_ORDER_QUALITY", true);
            petDataList = GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByTeamPosType);
            DataCenter.Set("DESCENDING_ORDER", false);
            petDataList =  GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByStarLevel);
            return petDataList;
        };
        if (itemDataArr!=null) openObj.mSelectList = itemDataArr.ToList();
        openObj.mMultipleSelectAction = list => {
            itemDataArr = list.ToArray();
            DataCenter.OpenWindow(UIWindowString.recover_petResolve);
        };
        DataCenter.SetData("BAG_PET_WINDOW", "OPEN", openObj);
    }

    protected override void Resolve() {
        if (itemDataArr == null) return;
        if (itemDataArr.Length == 0) return;
        Action action = () =>
        {
            CS_PetDisenchant cs = new CS_PetDisenchant(GetItemDataArr());
            HttpModule.Instace.SendGameServerMessage(cs, "CS_PetDisenchant", RequestResolveSuccess, RequestResolveFail);

			for (int i = 0; i < itemDataArr.Length; i++) 
			{
				GameObject obj =  nativeUIArr[i].transform.Find ("pet_resolve_tips").gameObject;
				obj.SetActive (true);
				GlobalModule.DoLater (() => obj.SetActive (false), 1.2f);
			}
        };
        DataCenter.OpenMessageOkWindow(TableCommon.getStringFromStringList(STRING_INDEX.RECOVER_RESOLVE_PET), action);
        DataCenter.SetData("MESSAGE_WINDOW", "SET_PANEL_DEPTH", 155);
    }


    protected override void ShowNativeUI()
    {
        if (itemDataArr != null)
        {
            for (int i = 0; i < itemDataArr.Length; i++)
            {
                var spriteName = TableCommon.GetStringFromActiveCongfig(itemDataArr[i].tid, "HEAD_SPRITE_NAME");
                var atlasName = TableCommon.GetStringFromActiveCongfig(itemDataArr[i].tid, "HEAD_ATLAS_NAME");
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
                        DataCenter.OpenWindow(UIWindowString.recover_petResolve);
                    });
                }
            }
        }
    }

    class Resolve_Pet : CEvent {
        public override bool _DoEvent() {
            DataCenter.SetData(UIWindowString.recover_petResolve, UIWindowString.resolveMethod, true);
            return true;
        }
    }

    class AutoAdd_Pet : CEvent {
        public override bool _DoEvent() {
            DataCenter.SetData(UIWindowString.recover_petResolve, UIWindowString.autoAddMethod, true);
            return true;
        }
    }
    class NormalAdd_Pet : CEvent {
        public override bool _DoEvent() {
            DataCenter.SetData(UIWindowString.recover_petResolve, UIWindowString.normalAddMethod, true);
            return true;
        }
    }

    //goto 黑市
    class Button_recover_resolve_btn_go_shop : CEvent
    {
        public override bool _DoEvent()
        {
            //TODO
            DataCenter.CloseWindow("RECOVER_WINDOW");
            DataCenter.OpenWindow("NEW_MYSTERIOUS_SHOP_WINDOW");
            DataCenter.OpenBackWindow("NEW_MYSTERIOUS_SHOP_WINDOW", "a_ui_heishilogo", 
                () =>{
                        DataCenter.SetData("RECOVER_WINDOW", "OPEN", true);
		                MainUIScript.Self.HideMainBGUI ();
                }, 184);
            MainUIScript.Self.HideMainBGUI();
            return true;
        }
    }

    //goto 黑市
    class Button_recover_rebirth_btn_goto_shop : CEvent
    {
        public override bool _DoEvent()
        {
            //TODO
            DataCenter.CloseWindow("RECOVER_WINDOW");
            DataCenter.OpenWindow("NEW_MYSTERIOUS_SHOP_WINDOW");
            DataCenter.OpenBackWindow("NEW_MYSTERIOUS_SHOP_WINDOW", "a_ui_heishilogo",
                () =>
                {
                    DataCenter.SetData("RECOVER_WINDOW", "OPEN", 2);
                    DataCenter.SetData("RECOVER_WINDOW", "SHOW_WINDOW", 2);
                    MainUIScript.Self.HideMainBGUI();
                }, 184);
            MainUIScript.Self.HideMainBGUI();
            return true;
        }
    }

    //goto 声望商店
    class Button_recover_equip_btn_equip : CEvent
    {
        public override bool _DoEvent()
        {
            //TODO
            DataCenter.CloseWindow("RECOVER_WINDOW");
            DataCenter.OpenWindow("SHOP_RENOWN_WINDOW", "BACK_RECOVER");
            DataCenter.OpenBackWindow("SHOP_RENOWN_WINDOW", "a_ui_lhsd_logo",
                () =>
                {
                    DataCenter.SetData("RECOVER_WINDOW", "OPEN", 1);
                    DataCenter.SetData("RECOVER_WINDOW", "SHOW_WINDOW", 1);
                    MainUIScript.Self.HideMainBGUI();
                }, 184);
            MainUIScript.Self.HideMainBGUI();
            return true;
        }
    }

}

