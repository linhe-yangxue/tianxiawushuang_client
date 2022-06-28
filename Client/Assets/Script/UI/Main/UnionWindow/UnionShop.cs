using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LHC.PipeLineUIRefreshSyetem;

public enum UnionShopType
{
    PROP = 1,
    CLOTH = 2,
    LIMIT = 3,
    AWARD = 4
}

public enum D_UnionShop
{
    tid,
    gid,/*发给服务器的字段*/
    gIndex,
    yuanBaoCost,
    contriCost,
    //UI_Sprite,
    limitEach,
    limitGuild,
    guildLimitLevel,
    eachDealNum,
    boughtNumEach,
    boughtNumGuild,
    showLevel,
    tabID,
}

public enum PL_UnionShop {
    Set_Buy_Button_Action,
    Set_YuanBao_Cost,
    Set_Contri_Cost,
    Set_Name,
    Set_Remain_Buy_Count_Each,
    Set_Remain_Buy_Count_Guild,
    Set_Each_Deal_Num,
    Set_UI_Sprite,
    Switch_YuanBao_Icon,
    Switch_Contri_Icon,
    Switch_CanBuy,
    Switch_CanBuy_Btn
}

public class UnionShopContainerWindow : ContainerWindowBase
{
    public static UnionShopContainerWindow instance;
    ExternalDataStation<PL_UnionShop, D_UnionShop> exDataStation;
    public static Dictionary<UnionShopType, List<DataGroup<D_UnionShop>>> unionShopDict { get; set; }
    public static PipeLineFactory<PL_UnionShop, D_UnionShop> mPipeLineFactory=new PipeLineFactory<PL_UnionShop,D_UnionShop>();
    
    public static long countDownTime;

  
    protected override void OpenInit() {
        base.OpenInit();
        instance = this;
        SetButtonAction();

    }
    public override void OnOpen() {
        base.OnOpen();
        DataCenter.OpenBackWindow(UIWindowString.union_shopContainer,"a_ui_zongmengsp_logo",() => {
            CloseAllTabWindow();
            DataCenter.OpenWindow(UIWindowString.union_main);
        });

        //by chenliang
        //begin

        //让道具toggle显示
        GameObject tmpGO = GameCommon.FindObject(mGameObjUI, "union_shop_title_buttons");
        if (tmpGO != null)
        {
            UIToggle tmpToggle = GameCommon.FindComponent<UIToggle>(tmpGO, "prop");
            tmpToggle.value = true;
        }

        //end
        RefreshContainer(UIWindowString.union_shop_prop); 

        //new mark
        UnionBase.SetShopFalse();
        
    }

    public override void OnClose() {
        base.OnClose();
        DataCenter.CloseBackWindow();
    }
    void SetButtonAction() {
        windowNameArr = new string[4]{
            UIWindowString.union_shop_cloth,
            UIWindowString.union_shop_prop,
            UIWindowString.union_shop_limit,
            UIWindowString.union_shop_award
        };
        toggleNameArr = new string[4] { "cloth", "prop", "limit", "award" };
        ContainerInit();
    }
    void PipeLineFactoryInit() {
        unionShopDict = new Dictionary<UnionShopType, List<DataGroup<D_UnionShop>>>();
        exDataStation = new ExternalDataStation<PL_UnionShop, D_UnionShop>("UnionShop");

        DataCenter.mGuildShopConfig.GetAllRecord().Keys.Foreach(key => {
            var dataGroup = exDataStation.GetDataGroup(key);
            if (dataGroup.GetInt(D_UnionShop.showLevel) <= UnionBase.guildBaseObject.level) {
                var type = (UnionShopType)dataGroup.GetInt(D_UnionShop.tabID);
                if (!unionShopDict.ContainsKey(type)) {
                    List<DataGroup<D_UnionShop>> list = new List<DataGroup<D_UnionShop>>();

                    unionShopDict.Add(type, list);
                }
                unionShopDict[type].Add(dataGroup);
            }
        });

        mPipeLineFactory.GetRefresherDict(exDataStation.GetRefresherDict());

        mPipeLineFactory.refresherDict[PL_UnionShop.Switch_YuanBao_Icon].handlerFunc=datas => datas.GetInt(D_UnionShop.yuanBaoCost)>0;
        mPipeLineFactory.refresherDict[PL_UnionShop.Set_YuanBao_Cost].handlerFunc=datas => 
            (datas.GetInt(D_UnionShop.yuanBaoCost)>0)? "x" + datas.GetInt(D_UnionShop.yuanBaoCost).ToString():"";
        mPipeLineFactory.refresherDict[PL_UnionShop.Switch_Contri_Icon].handlerFunc=datas => datas.GetInt(D_UnionShop.contriCost)>0;
        mPipeLineFactory.refresherDict[PL_UnionShop.Set_Contri_Cost].handlerFunc=datas =>
			(datas.GetInt(D_UnionShop.contriCost)>0)? "x" + datas.GetInt(D_UnionShop.contriCost).ToString():"";

        mPipeLineFactory.refresherDict[PL_UnionShop.Switch_CanBuy].handlerFunc=datas =>
            (UnionBase.guildBaseObject.level-datas.GetInt(D_UnionShop.guildLimitLevel)>=0
            &&(datas.GetInt(D_UnionShop.limitEach)-datas.GetInt(D_UnionShop.boughtNumEach)>0
            ||datas.GetInt(D_UnionShop.limitEach)==0))?
            "购买":"购买";

        mPipeLineFactory.refresherDict[PL_UnionShop.Switch_CanBuy_Btn].handlerFunc=datas => {
            return UnionBase.guildBaseObject.level-datas.GetInt(D_UnionShop.guildLimitLevel)>=0
            &&(datas.GetInt(D_UnionShop.limitEach)-datas.GetInt(D_UnionShop.boughtNumEach)>0
            ||datas.GetInt(D_UnionShop.limitEach)==0);
        };
            
        mPipeLineFactory.refresherDict[PL_UnionShop.Set_Name].handlerFunc=datas => datas.GetInt(D_UnionShop.tid);
        mPipeLineFactory.refresherDict[PL_UnionShop.Set_UI_Sprite].handlerFunc=datas => datas.GetInt(D_UnionShop.tid);

        mPipeLineFactory.refresherDict[PL_UnionShop.Set_Remain_Buy_Count_Each].handlerFunc=datas => {
            int limitEach=datas.GetInt(D_UnionShop.limitEach);
            int boughtNumEach=datas.GetInt(D_UnionShop.boughtNumEach);
            int guildLimitLevel=datas.GetInt(D_UnionShop.guildLimitLevel);
            if(UnionBase.guildBaseObject.level-guildLimitLevel<0)
                return string.Format("[ea3030]宗门{0}级可购[ffffff]", guildLimitLevel);
            else {
                if(limitEach==0) return "";
                else {
                    if (limitEach - boughtNumEach > 0) return string.Format("可购买{0}次".SetTextColor(LabelColor.Green), limitEach - boughtNumEach);
                    else return "[ea3030]已购买完毕[ffffff]";
                }
            }
        };

        mPipeLineFactory.refresherDict[PL_UnionShop.Set_Remain_Buy_Count_Guild].handlerFunc=datas => {
            int limitGuild=datas.GetInt(D_UnionShop.limitGuild);
            int boughtNumGuild=datas.GetInt(D_UnionShop.boughtNumGuild);
            if (limitGuild - boughtNumGuild > 0) return string.Format("宗门剩余{0}件".SetTextColor(LabelColor.Green), limitGuild - boughtNumGuild);
            else return "[ea3030]已卖完[ffffff]";    
        };

        mPipeLineFactory.refresherDict[PL_UnionShop.Set_Each_Deal_Num].handlerFunc=datas => (datas.GetInt(D_UnionShop.eachDealNum)>1)?"×"+datas.GetInt(D_UnionShop.eachDealNum):"";
    }
    public void RefreshContainer(string windowName) {
        PipeLineFactoryInit();
        HttpModule.CallBack requestSuccess = text => {
            var item = JCode.Decode<SC_GuildShopQuery>(text);
            Action action = () => {
                SetDict(item);
                DataCenter.OpenWindow(windowName);
            };
            UnionBase.InGuildThenDo(item, action);
        };
        CS_GuildShopQuery cs = new CS_GuildShopQuery(UnionBase.guildBaseObject.guildId);
        HttpModule.Instace.SendGameServerMessageT<CS_GuildShopQuery>(cs, requestSuccess, NetManager.RequestFail);
    }
    void SetDict(SC_GuildShopQuery sc) {

        Action<BuyObject> SetHaveBoughtEachCount = buyObject => {
            UnionShopType shopType = (UnionShopType)((int)DataCenter.mGuildShopConfig.GetRecord(buyObject.index).getData("TAB_ID"));
            //by chenliang
            //begin

//             unionShopDict[shopType].
//                 Where(dataGroup => dataGroup.GetInt(D_UnionShop.gIndex) == buyObject.index).
//                 SingleOrDefault().SetDictValue(D_UnionShop.boughtNumEach, buyObject.buyNum);
//---------------------
            //去除Linq相关
            List<DataGroup<D_UnionShop>> tmpListUnionShop = null;
            if (unionShopDict.TryGetValue(shopType, out tmpListUnionShop))
            {
                DataGroup<D_UnionShop> tmpDG = null;
                int tmpCount = 0;
                for (int i = 0, count = tmpListUnionShop.Count; i < count; i++)
                {
                    if (tmpListUnionShop[i].GetInt(D_UnionShop.gIndex) == buyObject.index)
                    {
                        if (tmpDG == null)
                            tmpDG = tmpListUnionShop[i];
                        tmpCount += 1;
                        if (tmpCount > 1)
                            break;
                    }
                }
                if (tmpCount == 1 && tmpDG != null)
                    tmpDG.SetDictValue(D_UnionShop.boughtNumEach, buyObject.buyNum);
            }

            //end
        };
        countDownTime = sc.time;
        sc.otherArr.Foreach(buyObject => SetHaveBoughtEachCount(buyObject));
        sc.priLimArr.Foreach(buyObject => SetHaveBoughtEachCount(buyObject));

        unionShopDict[UnionShopType.LIMIT].Clear();
        sc.pubLimArr.Foreach(buyObject => {
            var dataGroup=exDataStation.GetDataGroup(buyObject.index);
            dataGroup.SetDictValue(D_UnionShop.boughtNumGuild, buyObject.buyNum);

            int boughtNumEach = 0;
            sc.priLimArr.Foreach(buyObjectPri =>
            {
                if (buyObject.index == buyObjectPri.index)
                {
                    boughtNumEach = buyObjectPri.buyNum;
                }
            });
            dataGroup.SetDictValue(D_UnionShop.boughtNumEach, boughtNumEach);
            unionShopDict[UnionShopType.LIMIT].Add(dataGroup);   
        });

        //unionShopDict.Foreach(value => value.Value.ForEach(_value => _value.dict.Keys.Foreach(key => DEBUG.Log(key.ToString()))));
    }
    

}
abstract class UnionShopBase : UnionBase
{
    public override void OnOpen() {
        base.OnOpen();
        GetUILabel("myContri").text = RoleLogicData.Self.unionContr.ToString();
    }
    
    public void SortCurDataGroupList(List<DataGroup<D_UnionShop>> curDataGroupListOld, out List<DataGroup<D_UnionShop>> curDataGroupListNew)
    {
        List<DataGroup<D_UnionShop>> curDataGroupListTemp = new List<DataGroup<D_UnionShop>>();
        foreach (var dataGroup in curDataGroupListOld)
        {
            int limitEach = dataGroup.GetInt(D_UnionShop.limitEach);
            int boughtNumEach = dataGroup.GetInt(D_UnionShop.boughtNumEach);

            int limitGuild = dataGroup.GetInt(D_UnionShop.limitGuild);
            int boughtNumGuild = dataGroup.GetInt(D_UnionShop.boughtNumGuild);
            if ( (limitEach == 0 && limitGuild ==0)       //没有限制
                || limitEach - boughtNumEach != 0         //有一种限制
                )
            {
                curDataGroupListTemp.Add(dataGroup);
            }
        }
        foreach (var dataGroup in curDataGroupListOld)
        {
            int limitEach = dataGroup.GetInt(D_UnionShop.limitEach);
            int boughtNumEach = dataGroup.GetInt(D_UnionShop.boughtNumEach);

            int limitGuild = dataGroup.GetInt(D_UnionShop.limitGuild);
            int boughtNumGuild = dataGroup.GetInt(D_UnionShop.boughtNumGuild);
            if ((limitEach != 0 || limitGuild !=0)
                && limitEach - boughtNumEach == 0
                )
            {
                curDataGroupListTemp.Add(dataGroup);
            }
        }
        curDataGroupListNew = curDataGroupListTemp;
    }

    protected void AddGameObjectDataList(UnionShopType curShopType) {
        var curDataGroupListOld = UnionShopContainerWindow.unionShopDict[curShopType];
        List<DataGroup<D_UnionShop>> curDataGroupList;
        SortCurDataGroupList(curDataGroupListOld, out curDataGroupList);   //买光的置底

        GameCommon.FindComponent<UIScrollView>(mGameObjUI, "union_shop_goods_scrollView").ResetPosition();
        var container = GetUIGridContainer("boardList", curDataGroupList.Count);
        var goList=container.controlList;
        List<DataGameObject<PL_UnionShop, D_UnionShop>> dataGoList=new List<DataGameObject<PL_UnionShop,D_UnionShop>>();
        for (int i = 0; i < goList.Count; i++) {
            var dataGroup = curDataGroupList[i];
            var go=goList[i];
            dataGoList.Add(new DataGameObject<PL_UnionShop, D_UnionShop>(dataGroup, goList[i]));
			AddButtonAction (GameCommon.FindObject(go, "sprite"), () => GameCommon.SetItemDetailsWindow (dataGroup.GetInt(D_UnionShop.tid)));
            int gIndex = dataGroup.GetInt(D_UnionShop.gIndex);
            int yuanBaoCost = dataGroup.GetInt(D_UnionShop.yuanBaoCost);
            int contriCost = dataGroup.GetInt(D_UnionShop.contriCost);
            int index = i;
            CS_GuildShopBuy cs = new CS_GuildShopBuy(gIndex, 1, UnionBase.guildBaseObject.guildId);
            HttpModule.CallBack requestSuccess = text => {
                var item = JCode.Decode<SC_GuildShopBuy>(text);
                Action action = () => {
                    dataGroup.SetDictValue(D_UnionShop.boughtNumEach, dataGroup.GetInt(D_UnionShop.boughtNumEach)+1);
                    //by chenliang
                    //begin

//                    PackageManager.AddItem(item.buyItem);
//----------------------------
                    //改为UpdateItem
                    PackageManager.UpdateItem(item.buyItem);

                    //end
                    PackageManager.RemoveItem((int)ITEM_TYPE.YUANBAO, -1, yuanBaoCost);
                    PackageManager.RemoveItem((int)ITEM_TYPE.UNIONCONTR, -1,contriCost);
                    UnionShopContainerWindow.mPipeLineFactory.ExecutePipeLine(PL_UnionShop.Set_Remain_Buy_Count_Each,index);
                    UnionShopContainerWindow.mPipeLineFactory.ExecutePipeLine(PL_UnionShop.Switch_CanBuy_Btn,index);
                    UnionShopContainerWindow.mPipeLineFactory.ExecutePipeLine(PL_UnionShop.Switch_CanBuy,index);

                    if (curShopType == UnionShopType.LIMIT) {
                        dataGroup.SetDictValue(D_UnionShop.boughtNumGuild, dataGroup.GetInt(D_UnionShop.boughtNumGuild) + 1);
                        UnionShopContainerWindow.mPipeLineFactory.ExecutePipeLine(PL_UnionShop.Set_Remain_Buy_Count_Guild,index);    
                    }
                    DataCenter.OpenMessageWindow("购买成功");
                    GetUILabel("myContri").text = RoleLogicData.Self.unionContr.ToString();
                };
                UnionBase.InGuildThenDo(item, action);
            };

            string protocolName = (curShopType == UnionShopType.LIMIT) ? "CS_GuildShopLimitBuy" : "CS_GuildShopOtherBuy";
            AddButtonAction(GameCommon.FindObject(go, "canBuyBtn"), () => {
                if (RoleLogicData.Self.unionContr < contriCost) DataCenter.OpenMessageWindow("贡献值不足");    
                else if (RoleLogicData.Self.diamond < yuanBaoCost) DataCenter.OpenMessageWindow("元宝不足");
                else HttpModule.Instace.SendGameServerMessage(cs, protocolName, requestSuccess, NetManager.RequestFail);   
            });
        }

        
        UnionShopContainerWindow.mPipeLineFactory.GetDataGoList(dataGoList);        
    }
}
class UnionShopClothWindow : UnionShopBase
{
   

}
class UnionShopPropWindow : UnionShopBase
{
    public override void OnOpen() {
        base.OnOpen();
        AddGameObjectDataList(UnionShopType.PROP);
        //by chenliang
        //begin

//        UnionShopContainerWindow.mPipeLineFactory.ExecuteAllPipeLineExcept(PL_UnionShop.Set_Remain_Buy_Count_Guild, PL_UnionShop.Set_Buy_Button_Action);
//----------------
        UnionShopContainerWindow.mPipeLineFactory.ExecuteAllPipeLineExcept(
            new List<PL_UnionShop>()
            {
                PL_UnionShop.Set_Remain_Buy_Count_Guild,
                PL_UnionShop.Set_Buy_Button_Action
            });

        //end
    }
    
}
class UnionShopAwardWindow : UnionShopBase
{
    public override void OnOpen() {
        base.OnOpen();
        AddGameObjectDataList(UnionShopType.AWARD);
        //by chenliang
        //begin

//        UnionShopContainerWindow.mPipeLineFactory.ExecuteAllPipeLineExcept(PL_UnionShop.Set_Remain_Buy_Count_Guild, PL_UnionShop.Set_Buy_Button_Action);
//-----------------------
        UnionShopContainerWindow.mPipeLineFactory.ExecuteAllPipeLineExcept(
            new List<PL_UnionShop>()
            {
                PL_UnionShop.Set_Remain_Buy_Count_Guild,
                PL_UnionShop.Set_Buy_Button_Action
            });

        //end
        
    }
}
class UnionShopLimitWindow : UnionShopBase
{
    vp_Timer.Handle timeHandle;
    vp_Timer.Handle timeHandle_2;

    public override void OnOpen() {
        base.OnOpen();
        AddGameObjectDataList(UnionShopType.LIMIT);
        //by chenliang
        //begin

//        UnionShopContainerWindow.mPipeLineFactory.ExecuteAllPipeLineExcept(PL_UnionShop.Set_Buy_Button_Action);
//--------------------
        UnionShopContainerWindow.mPipeLineFactory.ExecuteAllPipeLineExcept(
            new List<PL_UnionShop>()
            {
                PL_UnionShop.Set_Buy_Button_Action
            });

        //end
        var label = GetUILabel("countDownTime");
        timeHandle = new vp_Timer.Handle();
        timeHandle_2=new vp_Timer.Handle();
        vp_Timer.In(0.01f, () => {
            label.text = vp_TimeUtility.TimeToString(UnionShopContainerWindow.countDownTime);
            UnionShopContainerWindow.countDownTime--;
        }, 9999999, 1, timeHandle);

        if (UnionShopContainerWindow.countDownTime == 0) UnionShopContainerWindow.countDownTime = 1;
        vp_Timer.In(UnionShopContainerWindow.countDownTime, () => {
            DataCenter.CloseWindow(UIWindowString.union_shop_limit);
            UnionShopContainerWindow.instance.RefreshContainer(UIWindowString.union_shop_limit);
        },timeHandle_2);
    }

    public override void OnClose() {
        base.OnClose();
        if (timeHandle!=null) timeHandle.Cancel();
        if(timeHandle_2!=null) timeHandle_2.Cancel();
    }

}


