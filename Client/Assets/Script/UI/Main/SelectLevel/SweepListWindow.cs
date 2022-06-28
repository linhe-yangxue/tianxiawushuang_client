using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable;
using System.Collections.Generic;
using Utilities;

public class SweepListWindow : tWindow
{
    int piece_kind = 0;
    int piece_num = 0;
    int _piece_num = 0;
    int preTid = 0;
    int newTid = 0;
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_sweep_list_close_button", new DefineFactory<Button_sweep_list_close_button>());
    }

    public override void Open(object param)
    {
        if (!Button_stage_info_clean_multi.forcedStop || param.ToString() == "ONE")
        {
            base.Open(param);
            if (param.ToString() == "ONE")
            {
                GameCommon.FindObject(mGameObjUI, "sweep_fight_grid").transform.localPosition = new Vector3(0, 125, 0);
            }
            mGameObjUI.transform.localPosition = new Vector3(0, 0, -2000);
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        if (!Button_stage_info_clean_multi.forcedStop || keyIndex == "0_ONE" || objVal.ToString() == "ONE")  //当未强制停止扫荡或是单次扫荡
        {  
            Open(objVal);
            GameObject mlabel = GameCommon.FindObject(mGameObjUI, "sweeping_label");
            GameObject mbutton = GameCommon.FindObject(mGameObjUI, "sweep_list_close_button");            
            BattleAccountInfo info = null;
            if (keyIndex == "STOP" )
            {
                if (!(objVal is string && objVal.ToString() == "ONE"))
                    GetComponent<UIScrollView>("sweep_fight_scrollview").SetDragAmount(0, 1, false);
                mlabel.SetActive(false);
                mbutton.SetActive(true);
            }
            else if (objVal != null && objVal is BattleAccountInfo)
            {
                if (keyIndex == "0_ONE" || keyIndex == "0")
                {
                    UILabel _mpiece = GameCommon.FindObject(mGameObjUI, "piece_num").GetComponent<UILabel>();
                    _mpiece.text = "";
                     piece_kind = 0;
                     piece_num = 0;
                     _piece_num = 0;
                     preTid = 0;
                     newTid = 0;
                }
                info = (BattleAccountInfo)objVal;
                base.onChange(keyIndex, objVal);
                mlabel.SetActive(true);
                mbutton.SetActive(false);
                UIGridContainer mgrid = GetComponent<UIGridContainer>("sweep_fight_grid");
                int i=0;
                if (keyIndex == "0_ONE")             //当是单次扫荡
                {
                     i= 0;              
                }
                else
                {
                     i = int.Parse(keyIndex);
                }
                if (i>1)
                GetComponent<UIScrollView>("sweep_fight_scrollview").SetDragAmount(0, 1, false);    //2次后跳转
                mgrid.MaxCount = i + 1;
                GameObject item = mgrid.controlList[i];
                GameCommon.FindObject(item, "time_label").GetComponent<UILabel>().text = "第 " + (i + 1).ToString() + " 次";
                if (info.isSweep)
                {
                    GameCommon.FindObject(GameCommon.FindObject(item, "get_exp"), "num_label").GetComponent<UILabel>().text = info.roleExp.ToString();
                    GameCommon.FindObject(GameCommon.FindObject(item, "get_coin"), "num_label").GetComponent<UILabel>().text = info.gold.ToString();
                    UIGridContainer rewardGrid = GameCommon.FindObject(item, "rewardGrid").GetComponent<UIGridContainer>();
                    rewardGrid.MaxCount = info.dropList.Length;
                    for (int j = 0; j < info.dropList.Length; j++)
                    {
                        GameObject reward_item = rewardGrid.controlList[j];
                        GameCommon.FindObject(reward_item, "num_label").GetComponent<UILabel>().text = "X" + info.dropList[j].itemNum.ToString();
                        GameCommon.FindObject(reward_item, "name_label").GetComponent<UILabel>().text = GameCommon.GetItemName(info.dropList[j].tid);
                        GameCommon.SetItemIconNew(reward_item, info.dropList[j].tid);
						int iItemId = info.dropList[j].tid;
						AddButtonAction (reward_item, () => GameCommon.SetAccountItemDetailsWindow (iItemId));
                    }
                }
                else
                {
                    GameCommon.FindObject(item, "info_labels").SetActive(false);
                    GameCommon.FindObject(item, "tips_failed").SetActive(true);
                }
                
                int groupId = DataCenter.mStageTable.GetData(info.battleId, "DROPGROUPID");
                UILabel mpiece = GameCommon.FindObject(mGameObjUI, "piece_num").GetComponent<UILabel>();
                bool isFirst = true;
                bool isFirstRight = true;
                foreach (var reward in info.dropList)
                {
                    if (PackageManager.GetItemRealTypeByTableID(reward.tid) == ITEM_TYPE.PET_FRAGMENT || PackageManager.GetItemRealTypeByTableID(reward.tid) == ITEM_TYPE.EQUIP_FRAGMENT)
                    {
                        if (piece_kind == 0)
                        {
                            preTid = reward.tid;
                            piece_kind = 1;
                        }
                        else if(piece_kind == 1)
                        {
                            if (preTid != reward.tid)
                            {
                                newTid = reward.tid;
                                piece_kind = 2;
                            }
                        }

                        if (piece_kind == 1)
                        {
                            if (isFirst)
                            {
                                piece_num += PackageManager.GetItemLeftCount(reward.tid);
                                isFirst = false;
                            }
                            piece_num += reward.itemNum;
                            mpiece.text = GameCommon.GetItemName(reward.tid) + "X[00FF00]" + piece_num + "/" + DataCenter.mFragment.GetData(reward.tid, "COST_NUM");
                        }
                        else if (piece_kind == 2)
                        {
                            if (preTid == reward.tid) 
                            {
                                if (isFirst)
                                {
                                    piece_num += PackageManager.GetItemLeftCount(reward.tid);
                                    isFirst = false;
                                }
                                piece_num += reward.itemNum; 
                            }
                            else 
                            {
                                if (isFirstRight)
                                {
                                    _piece_num += PackageManager.GetItemLeftCount(reward.tid);
                                    isFirstRight = false;
                                }
                                _piece_num += reward.itemNum;
                            }
                            mpiece.text = GameCommon.GetItemName(preTid) + "X[00FF00]" + piece_num + "/" + DataCenter.mFragment.GetData(preTid, "COST_NUM") +"[-]  "+ GameCommon.GetItemName(newTid) + "X[00FF00]" + _piece_num + "/" + DataCenter.mFragment.GetData(newTid, "COST_NUM");

                        }                       
                    }
                }
            }
        }
    }
}

public class Button_sweep_list_close_button : CEvent
{
    public override bool _DoEvent() 
    {
        Button_stage_info_clean_multi.stop = true;
        Button_stage_info_clean_multi.forcedStop = true;
        DataCenter.CloseWindow("SWEEP_LIST_WINDOW");
        if (GameCommon.FindObject(GameObject.Find("CenterAnchor"), "sweep_list_window")!=null)
           MonoBehaviour.Destroy(GameCommon.FindObject(GameObject.Find("CenterAnchor"), "sweep_list_window"));
        return true;
    }
}