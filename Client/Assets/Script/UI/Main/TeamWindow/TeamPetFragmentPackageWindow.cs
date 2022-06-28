using UnityEngine;
using System.Collections;
using Logic;
using System.Linq;
using System.Collections.Generic;

public class TeamPetFragmentPackageWindow : tWindow {

   

    //private int mTid = 0;
    FragmentBaseData curFragment;
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_team_pet_fragment_bag_icon_btn", new DefineFactory<TeamPetFragmentBagIconBtn>());

    }

    protected override void OpenInit() {
        base.OpenInit();
        AddButtonAction("sale",() => DataCenter.OpenWindow(UIWindowString.pet_fragment_bag));
    }
    public override void Open(object param)
    {
        base.Open(param);

        if ((bool)param)
        {
            Refresh(param);
        }
        else
        {
            InitWindow();
        }
    }

    public virtual void InitWindow()
    {
       
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if ("REFRESH_INFO" == keyIndex)
        {
            UpdateInfo((FragmentBaseData)objVal);
        }
    }
    public override bool Refresh(object param)
    {
        base.Refresh(param);
        UpdateUI();

        return true;
    }


    public void UpdateUI()
    {
        //by chenliang
        //begin

        __UpdateUI();
        return;

        //end
        UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "grid");
        if (grid != null)
        {
            grid.MaxCount = PetFragmentLogicData.Self.GetItemDataCount();
            List<PetFragmentData> itemDataList = PetFragmentLogicData.Self.mDicPetFragmentData.Values.ToList();
			string str1=DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_PET_FRAGMENT_TIPS,"STRING_CN");
			string str2=DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_PET_FRAGMENT_LABEL,"STRING_CN");

            GameObject tipsObj = GameCommon.FindUI("Label_no_pet_fragment_tips");
            GameObject labelObj = GameCommon.FindUI("Label_no_pet_fragment_label");
            if (tipsObj == null || labelObj == null)
                return;
            tipsObj.SetActive(grid.MaxCount == 0);
            labelObj.SetActive(grid.MaxCount == 0);

			if (grid.MaxCount == 0){
                tipsObj.GetComponent<UILabel>().text = str1;
                labelObj.GetComponent<UILabel>().text = str2;
			}

            DataCenter.Set("DESCENDING_ORDER", true);
            itemDataList = GameCommon.SortList(itemDataList, GameCommon.SortFragmentDataByIsCanCompose);
            for (int i = 0; i < grid.MaxCount; i++)
            {
                FragmentBaseData itemData=itemDataList[i];
                GameObject obj = grid.controlList[i];

                if (obj != null && itemData != null)
                {
                    tNiceData data = GameCommon.GetButtonData(obj, "team_pet_fragment_bag_icon_btn");
                    if (data != null)
                    {
                        data.set("FRAGMENT_DATA", itemData);
                    }

                    if (0 == i)
                    {
                        UpdateInfo(itemData);
                    }


                    // 设置宠物头像
                    GameCommon.SetItemIcon(obj, "item_icon", itemData.mComposeItemTid);

                    // set element icon
                    int iElementIndex = TableCommon.GetNumberFromActiveCongfig(itemData.mComposeItemTid, "ELEMENT_INDEX");
                    GameCommon.SetElementIcon(obj, iElementIndex);

                    // set element fragment icon
                    GameCommon.SetElementFragmentIcon(obj, iElementIndex);

                    // set star level
                    //GameCommon.SetStarLevelLabel(obj, TableCommon.GetNumberFromActiveCongfig(itemData.mComposeItemTid, "STAR_LEVEL"));

                    // 设置名称
                    GameCommon.SetUIText(obj, "name", TableCommon.GetStringFromFragment(itemData.tid, "NAME"));

                    // 设置宠物类型名称
                    string strPetTypeName = GameCommon.GetAttackType(itemData.mComposeItemTid);
                    GameCommon.SetUIText(obj, "defence_type", strPetTypeName);

                    // 是否在阵上
                    GameObject stateObj = GameCommon.FindObject(obj, "battle");
                    if (stateObj != null)
                    {
                        stateObj.SetActive(TeamManager.IsPetInTeamByTid(itemData.mComposeItemTid));
                    }

                    int costNum = TableCommon.GetNumberFromFragment(itemData.tid, "COST_NUM");
                    // 拥有数量
                    UILabel itemNumLabel = GameCommon.FindComponent<UILabel>(obj, "pet_fragment_num");
                    itemNumLabel.text = itemData.itemNum + "/" + costNum;
					if(itemData.itemNum < costNum)
					{
						itemNumLabel.text = "[ff3333]"+ itemData.itemNum + "[ffffff] / " + costNum;
					}

                    // 是否可合成
                    GameCommon.FindComponent<UILabel>(obj,"lack").text=(itemData.itemNum>=costNum)?"可合成".SetTextColor(LabelColor.Green):"数量不足".SetTextColor(LabelColor.Red);
                }
            }
            GameCommon.FindComponent<UIScrollView>(mGameObjUI, "pet_scroll_view").ResetPosition();
        }
    }

    private void UpdateInfo(FragmentBaseData petFragment)
    {
        curFragment=petFragment;

        TeamInfoWindow.CloseAllWindow();
        DataCenter.OpenWindow("PET_FRAGMENT_INFO_WINDOW",petFragment);
    }
    //by chenliang
    //begin

    private List<PetFragmentData> mItemDataList;
    private void __UpdateUI()
    {
        UIWrapGrid grid = GameCommon.FindComponent<UIWrapGrid>(mGameObjUI, "grid");
        if (grid != null)
        {
            int tmpCount = PetFragmentLogicData.Self.GetItemDataCount();
            mItemDataList = PetFragmentLogicData.Self.mDicPetFragmentData.Values.ToList();
            string str1 = DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_PET_FRAGMENT_TIPS, "STRING_CN");
            string str2 = DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_PET_FRAGMENT_LABEL, "STRING_CN");

            GameObject tipsObj = GameCommon.FindUI("Label_no_pet_fragment_tips");
            GameObject labelObj = GameCommon.FindUI("Label_no_pet_fragment_label");
            if (tipsObj == null || labelObj == null)
                return;
            tipsObj.SetActive(tmpCount == 0);
            labelObj.SetActive(tmpCount == 0);

            if (tmpCount == 0)
            {
                tipsObj.GetComponent<UILabel>().text = str1;
                labelObj.GetComponent<UILabel>().text = str2;
            }

            DataCenter.Set("DESCENDING_ORDER", true);
            mItemDataList = GameCommon.SortList(mItemDataList, GameCommon.SortFragmentDataByIsCanCompose);

            grid.onInitializeItem = __OnGridUpdate;
            grid.maxIndex = tmpCount - 1;
            grid.ItemsCount = 16;
            GlobalModule.DoOnNextLateUpdate(2, () =>
            {
                GameCommon.FindComponent<UIScrollView>(mGameObjUI, "pet_scroll_view").ResetPosition();
            });
        }
    }
    private void __OnGridUpdate(GameObject item, int wrapIndex, int index)
    {
        FragmentBaseData tmpItemData = mItemDataList[index];

        if (item != null && tmpItemData != null)
        {
            tNiceData data = GameCommon.GetButtonData(item, "team_pet_fragment_bag_icon_btn");
            if (data != null)
            {
                data.set("FRAGMENT_DATA", tmpItemData);
            }

            if (0 == index)
            {
                UpdateInfo(tmpItemData);
            }


            // 设置宠物头像
            GameCommon.SetItemIcon(item, "item_icon", tmpItemData.mComposeItemTid);

            // set element icon
            int iElementIndex = TableCommon.GetNumberFromActiveCongfig(tmpItemData.mComposeItemTid, "ELEMENT_INDEX");
            GameCommon.SetElementIcon(item, iElementIndex);

            // set element fragment icon
            GameCommon.SetElementFragmentIcon(item, iElementIndex);

            // set star level
            //GameCommon.SetStarLevelLabel(obj, TableCommon.GetNumberFromActiveCongfig(itemData.mComposeItemTid, "STAR_LEVEL"));

            // 设置名称
            GameCommon.SetUIText(item, "name", TableCommon.GetStringFromFragment(tmpItemData.tid, "NAME"));

            // 设置宠物类型名称
            string strPetTypeName = GameCommon.GetAttackType(tmpItemData.mComposeItemTid);
            GameCommon.SetUIText(item, "defence_type", strPetTypeName);

            // 是否在阵上
            GameObject stateObj = GameCommon.FindObject(item, "battle");
            if (stateObj != null)
            {
                stateObj.SetActive(TeamManager.IsPetInTeamByTid(tmpItemData.mComposeItemTid));
            }

            int costNum = TableCommon.GetNumberFromFragment(tmpItemData.tid, "COST_NUM");
            // 拥有数量
            UILabel itemNumLabel = GameCommon.FindComponent<UILabel>(item, "pet_fragment_num");
            itemNumLabel.text = tmpItemData.itemNum + "/" + costNum;
            if (tmpItemData.itemNum < costNum)
            {
                itemNumLabel.text = "[ff3333]" + tmpItemData.itemNum + "[ffffff] / " + costNum;
            }

            // 是否可合成
            GameCommon.FindComponent<UILabel>(item, "lack").text = (tmpItemData.itemNum >= costNum) ? "可合成".SetTextColor(LabelColor.Green) : "数量不足".SetTextColor(LabelColor.Red);
        }
    }

    //end
}


public class TeamPetFragmentBagIconBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("TEAM_PET_FRAGMENT_PACKAGE_WINDOW","REFRESH_INFO",getObject("FRAGMENT_DATA"));
        return true;
    }
}