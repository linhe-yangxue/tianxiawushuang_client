using UnityEngine;
using System.Collections;
using System.Linq;
using Logic;
using System.Collections.Generic;
using DataTable;

public class FragmentInfoWindow : tWindow
{
    protected FragmentBaseData mCurItemData;
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_compose_fragment_btn", new DefineFactory<ComposeFragmentBtn>());

    }

    protected override void OpenInit() {
        base.OpenInit();
        
    }
    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if ("COMPOSE" == keyIndex)
        {
            RequestComposeItem();
        }
        
    }

    protected virtual void RequestComposeItem()
    {

    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);

        mCurItemData=(FragmentBaseData)param;
        //mCurItemData = GameCommon.GetItemDataBase(-1, mCurTid) as FragmentBaseData;
        UpdateUI();

        return true;
    }

    protected virtual void SetFragmentData()
    {

    }

    private void UpdateUI()
    {
		UpdateInfoUI();
        UpdateGetWayUI();
    }

    private void UpdateGetWayUI()
    {
        UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "grid");
		GameCommon.GetParth (grid, mCurItemData.tid);
    }

    protected virtual void UpdateInfoUI()
    {
        GameObject obj = GetSub("fragment_info");
       
        if (mCurItemData != null && obj != null)
        {
            // 设置碎片头像
            GameCommon.SetItemIcon(obj, "item_icon", mCurItemData.mComposeItemTid);

            // set star level
            //GameCommon.SetStarLevelLabel(obj, TableCommon.GetNumberFromActiveCongfig(itemData.mComposeItemTid, "STAR_LEVEL"));

            // 设置名称
            GameCommon.SetUIText(obj, "name", TableCommon.GetStringFromFragment(mCurItemData.tid, "NAME"));

            // 是否在阵上
            GameObject stateObj = GameCommon.FindObject(obj, "state_icon");
            if (stateObj != null)
            {
                stateObj.SetActive(TeamManager.IsPetInTeamByTid(mCurItemData.mComposeItemTid));
            }

            // 拥有数量
            UILabel numLabel = GameCommon.FindComponent<UILabel>(mGameObjUI, "fragment_num");
            numLabel.text = mCurItemData.itemNum.ToString();

            // 设置合成按钮状态
            int costNum = TableCommon.GetNumberFromFragment(mCurItemData.tid, "COST_NUM");
            GameObject conposeBtn = GetSub("compose_fragment_btn");
            if (conposeBtn != null)
            {
                conposeBtn.GetComponent<UIImageButton>().isEnabled=mCurItemData.itemNum>=costNum;
                //conposeBtn.GetComponent<>SetActive(mCurItemData.itemNum >= costNum);
            }

            tNiceData data = GameCommon.GetButtonData(conposeBtn);
            data.set("FRAGMENT_DATA",mCurItemData);
        }
        
    }
}

public class ComposeFragmentBtn : CEvent
{
    class CS_FragmentCompose:GameServerMessage {
        public readonly ItemDataBase fragment;
        public CS_FragmentCompose(ItemDataBase fragment) {
            this.fragment=fragment;
        }
    }

    class SC_FragmentCompose:GameServerMessage {
        public readonly ItemDataBase item;
    }


    public override bool _DoEvent()
    {
        FragmentBaseData fragmentData = getObject("FRAGMENT_DATA") as FragmentBaseData;
        //by chenliang
        //begin

        //判断背包是否已满
        PACKAGE_TYPE[] tmpPackageTypes = new PACKAGE_TYPE[] { PackageManager.GetPackageTypeByItemTid(fragmentData.tid) };
        if (!CheckPackage.Instance.CanCompose(tmpPackageTypes))
            return true;

        //end
        switch(PackageManager.GetItemTypeByTableID(fragmentData.tid))
        {
            case ITEM_TYPE.PET_FRAGMENT:
                int costNum=TableCommon.GetNumberFromFragment(fragmentData.tid,"COST_NUM");
                ItemDataBase itemData=new ItemDataBase();
                itemData.itemId=fragmentData.itemId;
                itemData.itemNum=costNum;
                itemData.tid=fragmentData.tid;

                var cs=new CS_FragmentCompose(itemData);
                
                HttpModule.CallBack requestSuccess=text => {
                    var item=JCode.Decode<SC_FragmentCompose>(text);
                    PackageManager.RemoveItem(fragmentData.tid,fragmentData.itemId,costNum);
                    PackageManager.UpdateItem(item.item);
                    DataCenter.OpenWindow("TEAM_PET_FRAGMENT_PACKAGE_WINDOW");
                    DataCenter.OpenWindow(UIWindowString.petDetail,item.item.tid);

                    DataCenter.OpenWindow("PET_GAIN_WINDOW",item.item.tid);

                    //added by xuke 红点相关逻辑
                    TeamNewMarkManager.Self.CheckComposeNewPet();
                    TeamNewMarkManager.Self.RefreshTeamNewMark();
                    //end
                };

                HttpModule.Instace.SendGameServerMessageT<CS_FragmentCompose>(cs,requestSuccess,NetManager.RequestFail);
                break;
            case ITEM_TYPE.EQUIP_FRAGMENT:
                int _costNum=TableCommon.GetNumberFromFragment(fragmentData.tid,"COST_NUM");
                ItemDataBase _itemData=new ItemDataBase();
                _itemData.itemId=fragmentData.itemId;
                _itemData.itemNum=_costNum;
                _itemData.tid=fragmentData.tid;

                var _cs=new CS_FragmentCompose(_itemData);
                
                HttpModule.CallBack _requestSuccess=text => {
                    var _item=JCode.Decode<SC_FragmentCompose>(text);
                    PackageManager.RemoveItem(fragmentData.tid,fragmentData.itemId,_costNum);
                    PackageManager.UpdateItem(_item.item);
                    //DataCenter.OpenWindow("EQUIP_FRAGMENT_INFO_WINDOW");
                    // added by xuke begin
                    // 添加装备合成窗口
                    DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW", _item.item.tid);
                    DataCenter.SetData("PACKAGE_EQUIP_WINDOW", "COMPOSE_FRAGMENT_BTN", true);
                    //end
                   //added by xuke 红点相关
                    EquipBagNewMarkManager.Self.CheckEquipFragNumLimit_NewMark();
                    EquipBagNewMarkManager.Self.CheckEquipNumLimit_NewMark();
                    EquipBagNewMarkManager.Self.CheckEquipCompose_NewMark();
                    EquipBagNewMarkManager.Self.RefreshEquipBagTabNewMark();
                   //end
                };

                HttpModule.Instace.SendGameServerMessageT<CS_FragmentCompose>(_cs,_requestSuccess,NetManager.RequestFail);
                //DataCenter.SetData("EQUIP_FRAGMENT_INFO_WINDOW", "COMPOSE", true);
                break;
            case ITEM_TYPE.MAGIC_FRAGMENT:
                DataCenter.SetData("MAGIC_FRAGMENT_INFO_WINDOW", "COMPOSE", true);
                break;
        }
        
        return true;
    }
}