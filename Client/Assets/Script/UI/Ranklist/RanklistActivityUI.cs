using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using System;
using System.Linq;

public class RanklistActivityUIWindow : tWindow
{
    public static int mpowerRank;
    public static int mpeakRank;
    public override void Init()
    {
        EventCenter.Register("Button_activity_rank_window_info_back", new DefineFactory<Button_activity_rank_window_info_back>());
        EventCenter.Register("Button_peak_button", new DefineFactory<Button_peak_button>());
        EventCenter.Register("Button_power_button", new DefineFactory<Button_power_button>());
        EventCenter.Register("Button_peak_see_reward", new DefineFactory<Button_peak_see_reward>());
        EventCenter.Register("Button_power_see_reward", new DefineFactory<Button_power_see_reward>());
        EventCenter.Register("Button_peak_act", new DefineFactory<Button_peak_act>());
        EventCenter.Register("Button_power_act", new DefineFactory<Button_power_act>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if (keyIndex == "OPEN")
        {
            DataCenter.OpenWindow("RANKLIST_ACTIVITY_BACK_WINDOW");
            NetManager.RequestActivityPeak();
        }       
        else if (keyIndex == "REFRESH_PEAK")
        {
            refreshPeakRank(objVal.ToString());          
        }
        else if (keyIndex == "REFRESH_POWER")
        {
            refreshPowerRank(objVal.ToString());          
        }
    }

    void refreshPeakRank(string param)
    {
        SC_Ranklist_PvpList peakRank = JCode.Decode<SC_Ranklist_PvpList>(param);
        if (peakRank.endTime >= GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime()))
        {
            SetCountdownTime("activity_time_label", peakRank.endTime);
        }
        else
        {
            GameCommon.SetUIText(mGameObjUI, "activity_time_label", DataCenter.mStringList.GetData((int)STRING_INDEX.RANK_ACTIVITY_END, "STRING_CN"));
        }
        GameObject peak_down = GameCommon.FindObject(mGameObjUI, "PeakBackground");
        if (true)
        {
            change("peak");
            ActiveBirthForUI birthUI = GameCommon.FindComponent<ActiveBirthForUI>(mGameObjUI, "UIPoint");
            BaseObject character = GameCommon.ShowCharactorModel(birthUI.gameObject, 30098, 1f);
            birthUI.transform.parent.localPosition = new Vector3(410,-355,-100);
            birthUI.transform.parent.localScale = new Vector3(1,1,1);
            GlobalModule.DoCoroutine(GameCommon.IE_ChangeRenderQueue(character.mConfigIndex, character.mMainObject, CommonParam.AureoleRenderQueue));
            UIGridContainer grid = null;
            grid = GetComponent<UIGridContainer>("rank_list_grid");
            if (grid != null)
            {
               if(peakRank.ranklist.Length>10)
               {
                  grid.MaxCount = 10;
               }
               else
               {
                   grid.MaxCount = peakRank.ranklist.Length;
               }
               int i=0;
               foreach (GetArenaRankList_ItemData rankInfo in peakRank.ranklist)
               {
                   if (i > 9) break;
                   GameObject item = grid.controlList[i++];           
                   GameCommon.SetUIText(item,"role_name_label",rankInfo.nickname);
                   GameCommon.SetUIText(item,"level_label_fighting_num",rankInfo.level.ToString());
                   GameCommon.SetUIText(item,"union_label_name",rankInfo.guildName);
                   GameCommon.SetUIText(item,"fighting_label_fighting_num",rankInfo.power.ToString());
                   GameCommon.SetPalyerIcon(GameCommon.FindObject(item,"item_icon").GetComponent<UISprite>(),rankInfo.HeadIconId);
                   GameObject first = GameCommon.FindObject(item, "no1");
                   GameObject second = GameCommon.FindObject(item, "no2");
                   GameObject third = GameCommon.FindObject(item, "no3");
                   if (rankInfo.ranking > 3)
                   {
                       GameCommon.SetUIVisiable(item, "peak_rank_num", true);
                       GameCommon.SetUIText(item, "peak_rank_num", rankInfo.ranking.ToString());
                       first.SetActive(false);
                       second.SetActive(false);
                       third.SetActive(false);
                   }
                   else
                   {
                       GameCommon.SetUIVisiable(item, "peak_rank_num", false);
                       switch (rankInfo.ranking)
                       {
                           case 1: first.SetActive(true);
                               second.SetActive(false);
                               third.SetActive(false);
                               break;
                           case 2: first.SetActive(false);
                               second.SetActive(true);
                               third.SetActive(false);
                               break;
                           case 3: first.SetActive(false);
                               second.SetActive(false);
                               third.SetActive(true);
                               break;
                       }                    
                   }
               }
            }
            mpeakRank=peakRank.myRanking+1;
            if (mpeakRank > 0)
            {
                GameCommon.SetUIText(mGameObjUI, "activity_rank_number_label", mpeakRank.ToString());
            }
            else 
            {
                GameCommon.SetUIText(mGameObjUI, "activity_rank_number_label", DataCenter.mStringList.GetData((int)STRING_INDEX.RANK_ACTIVITY_OUT, "STRING_CN"));
            }
            GameCommon.SetUIVisiable(mGameObjUI, "fighting_label",false);
            GameCommon.SetUIVisiable(mGameObjUI, "peak_label", mpeakRank > 10);
            GameCommon.SetUIText(mGameObjUI, "peak_label_num", "10");
        }
        GameCommon.FindObject(mGameObjUI, "rank_list_scrollView").GetComponent<UIScrollView>().ResetPosition();
    }

    void refreshPowerRank(string param)
    {
        SC_Ranklist_FightingList powerRank = JCode.Decode<SC_Ranklist_FightingList>(param);
        if (powerRank.endTime >= GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime()))
        {
            SetCountdownTime("activity_time_label", powerRank.endTime);
        }
        else
        {
            GameCommon.SetUIText(mGameObjUI, "activity_time_label", DataCenter.mStringList.GetData((int)STRING_INDEX.RANK_ACTIVITY_END, "STRING_CN"));
        }
        GameObject power_down = GameCommon.FindObject(mGameObjUI, "PowerBackground");
        if (true)
        {
            change("power");          
            UIGridContainer grid = null;
            grid = GetComponent<UIGridContainer>("rank_list_grid");
            if (grid != null)
            {
                if (powerRank.ranklist.Length > 10)
                {
                    grid.MaxCount = 10;
                }
                else
                {
                    grid.MaxCount = powerRank.ranklist.Length;
                }
               int i=0;
               foreach(MainUIRanklist_ItemData rankInfo in powerRank.ranklist)
               {
                   if (i > 9) break;
                   GameObject item = grid.controlList[i++];           
                   GameCommon.SetUIText(item,"role_name_label",rankInfo.nickname);
                   GameCommon.SetUIText(item,"level_label_fighting_num",rankInfo.level.ToString());
                   GameCommon.SetUIText(item,"union_label_name",rankInfo.guildName);
                   GameCommon.SetUIText(item,"fighting_label_fighting_num",rankInfo.power.ToString());
                   GameCommon.SetPalyerIcon(GameCommon.FindObject(item,"item_icon").GetComponent<UISprite>(),rankInfo.HeadIconId);
                   GameObject first = GameCommon.FindObject(item, "no1");
                   GameObject second = GameCommon.FindObject(item, "no2");
                   GameObject third = GameCommon.FindObject(item, "no3");
                   if (rankInfo.ranking > 3)
                   {
                       GameCommon.SetUIVisiable(item, "peak_rank_num", true);
                       GameCommon.SetUIText(item, "peak_rank_num", rankInfo.ranking.ToString());
                       first.SetActive(false);
                       second.SetActive(false);
                       third.SetActive(false);
                   }
                   else
                   {
                       GameCommon.SetUIVisiable(item, "peak_rank_num", false);
                       switch (rankInfo.ranking)
                       {
                           case 1: first.SetActive(true);
                               second.SetActive(false);
                               third.SetActive(false);
                               break;
                           case 2: first.SetActive(false);
                               second.SetActive(true);
                               third.SetActive(false);
                               break;
                           case 3: first.SetActive(false);
                               second.SetActive(false);
                               third.SetActive(true);
                               break;
                       }                    
                   }
               }
            }
            mpowerRank = powerRank.myRanking;
            if (mpowerRank > 0)
            {
                GameCommon.SetUIText(mGameObjUI, "activity_rank_number_label", mpowerRank.ToString());
            }
            else 
            {
                GameCommon.SetUIText(mGameObjUI, "activity_rank_number_label", DataCenter.mStringList.GetData((int)STRING_INDEX.RANK_ACTIVITY_OUT, "STRING_CN"));
            }
            GameCommon.SetUIVisiable(mGameObjUI, "fighting_label",true);
            GameCommon.SetUIVisiable(mGameObjUI, "peak_label", false);
            GameCommon.SetUIText(mGameObjUI, "fighting_label_num",GameCommon.GetPower().ToString("f0"));
        }
        GameCommon.FindObject(mGameObjUI, "rank_list_scrollView").GetComponent<UIScrollView>().ResetPosition();
    }

    void change(string kind)
    {
        GameObject peak_up = GameCommon.FindObject(mGameObjUI, "PeakCheckmark");
        GameObject peak_down = GameCommon.FindObject(mGameObjUI, "PeakBackground");
        GameObject power_up = GameCommon.FindObject(mGameObjUI, "PowerCheckmark");
        GameObject power_down = GameCommon.FindObject(mGameObjUI, "PowerBackground");
        GameObject peak = GameCommon.FindObject(mGameObjUI, "peak");
        GameObject power = GameCommon.FindObject(mGameObjUI, "power");
        GameObject mpower_label = GameCommon.FindObject(mGameObjUI, "fighting_label");
        GameObject mpeak_label = GameCommon.FindObject(mGameObjUI, "peak_label");
        if (kind == "peak")
        {
                peak_up.SetActive(true);
                peak_down.SetActive(false);
                power_up.SetActive(false);
                power_down.SetActive(true);
                peak.SetActive(true);
                power.SetActive(false);
                mpeak_label.SetActive(true);
                mpower_label.SetActive(false);
        }
        else
        {
                peak_up.SetActive(false);
                peak_down.SetActive(true);
                power_up.SetActive(true);
                power_down.SetActive(false);
                peak.SetActive(false);
                power.SetActive(true);
                mpeak_label.SetActive(false);
                mpower_label.SetActive(true);
        }
    }
}

public class RanklistActivityRewardUIWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Register("Button_activity_rank_reward_window_close", new DefineFactory<Button_activity_rank_reward_window_close>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if (keyIndex == "REFRESH_PEAK")
        {
            refreshPeakReward();
        }
        else if (keyIndex == "REFRESH_POWER")
        {
            refreshPowerReward();
        }
    }

    void refreshPeakReward() 
    {
        GameCommon.SetUIText(mGameObjUI, "title_label_reward", DataCenter.mStringList.GetData((int)STRING_INDEX.RANK_ACTIVITY_PEAK_NAME, "STRING_CN"));
        UIGridContainer grid = null;
        grid = GameObject.Find("rank_list_grid_reward").GetComponent<UIGridContainer>();
        if (grid != null)
        {
            Dictionary<int, DataRecord> mRecordMap = DataCenter.mRankActivity.GetAllRecord();
            int mindex = 0;
            int mnum = 0;
            List<DataRecord> mrecords=new List<DataRecord>();
            foreach (KeyValuePair<int, DataRecord> r in mRecordMap) 
            {
                if (r.Value.get("TYPE") == 1)
                {
                    mrecords.Add(r.Value);
                   // mrecords[mnum] = r.Value;
                    mnum++;
                    if (r.Key> mindex)
                    {
                        mindex = r.Key;
                    }
                }
            }
            int can_reward_num = mRecordMap[mindex].get("RANK_MAX");
            string peak_reward_tips = DataCenter.mStringList.GetData((int)STRING_INDEX.RANK_ACTIVITY_PEAK_REWARD_TIPS, "STRING_CN");
            peak_reward_tips=peak_reward_tips.Replace("{0}",can_reward_num.ToString());
            GameCommon.SetUIText(mGameObjUI, "introduce_reward", peak_reward_tips);
            grid.MaxCount = mnum;
            int i = 0;
            foreach (DataRecord info in mrecords)
            {
                GameObject item = grid.controlList[i++];
                int rank_min = info.get("RANK_MIN");
                int rank_max = info.get("RANK_MAX");
                GameObject first = GameCommon.FindObject(item, "no1_reward");
                GameObject second = GameCommon.FindObject(item, "no2_reward");
                GameObject third = GameCommon.FindObject(item, "no3_reward");
                if (i <=3)
                {
                    GameCommon.SetUIText(item, "peak_rank_num_reward","");
                    switch (i)
                    {
                        case 1: first.SetActive(true);
                            second.SetActive(false);
                            third.SetActive(false);
                            break;
                        case 2: first.SetActive(false);
                            second.SetActive(true);
                            third.SetActive(false);
                            break;
                        case 3: first.SetActive(false);
                            second.SetActive(false);
                            third.SetActive(true);
                            break;
                    }
                }
                else
                {
                    first.SetActive(false);
                    second.SetActive(false);
                    third.SetActive(false);
                    if(rank_min==rank_max)
                    {
                         GameCommon.SetUIText(item, "peak_rank_num_reward", rank_max.ToString());
                    }
                    else
                    {
                        GameCommon.SetUIText(item, "peak_rank_num_reward", "第"+rank_min.ToString()+"-"+rank_max.ToString()+"名");
                    }
                }
                string reward_what=info.get("AWARD_GROUPID");
                string[] reward_group=reward_what.Split('|');
                string[] first_reward=reward_group[0].Split('#');
                string[] second_reward=reward_group[1].Split('#');
                GameCommon.SetItemIconNew(item, "reward_icon1", int.Parse(first_reward[0]),true);
                GameCommon.SetItemIconNew(item, "reward_icon2", int.Parse(second_reward[0]),true);
                GameCommon.SetUIText(item, "reward_label1", "X" + first_reward[1]);
                GameCommon.SetUIText(item, "reward_label2", "X" + second_reward[1]);
				AddButtonAction (GameCommon.FindObject (item, "icon_tips_btn1"), () => GameCommon.SetItemDetailsWindow (int.Parse(first_reward[0])));
				AddButtonAction (GameCommon.FindObject (item, "icon_tips_btn2"), () => GameCommon.SetItemDetailsWindow (int.Parse(second_reward[0])));
            }
        }
        if (RanklistActivityUIWindow.mpeakRank > 0)
        {
            GameCommon.SetUIText(mGameObjUI, "activity_rank_number_label_reward", RanklistActivityUIWindow.mpeakRank.ToString());
        }
        else
        {
            GameCommon.SetUIText(mGameObjUI, "activity_rank_number_label_reward", DataCenter.mStringList.GetData((int)STRING_INDEX.RANK_ACTIVITY_OUT, "STRING_CN"));
        }
        UIScrollView mscroll=GameCommon.FindObject(mGameObjUI,"rank_list_scrollView_reward").GetComponent<UIScrollView>();
        mscroll.ResetPosition();
    }

    void refreshPowerReward()
    {
        GameCommon.SetUIText(mGameObjUI, "title_label_reward", DataCenter.mStringList.GetData((int)STRING_INDEX.RANK_ACTIVITY_POWER_NAME, "STRING_CN"));
        UIGridContainer grid = null;
        grid = GameObject.Find("rank_list_grid_reward").GetComponent<UIGridContainer>();
        if (grid != null)
        {
            Dictionary<int, DataRecord> mRecordMap = DataCenter.mRankActivity.GetAllRecord();
            int mindex = 0;
            int mnum = 0;
            List<DataRecord> mrecords = new List<DataRecord>();
            foreach (KeyValuePair<int, DataRecord> r in mRecordMap)
            {
                if (r.Value.get("TYPE") == 2)
                {
                    mrecords.Add(r.Value);
                    // mrecords[mnum] = r.Value;
                    mnum++;
                    if (r.Key > mindex)
                    {
                        mindex = r.Key;
                    }
                }
            }
            int can_reward_num = mRecordMap[mindex].get("RANK_MAX");
            grid.MaxCount = mnum;
            int i = 0;
            foreach (DataRecord info in mrecords)
            {
                GameObject item = grid.controlList[i++];
                int rank_min = info.get("RANK_MIN");
                int rank_max = info.get("RANK_MAX");
                GameObject first = GameCommon.FindObject(item, "no1_reward");
                GameObject second = GameCommon.FindObject(item, "no2_reward");
                GameObject third = GameCommon.FindObject(item, "no3_reward");
                if (i <= 3)
                {
                    GameCommon.SetUIText(item, "peak_rank_num_reward", "");
                    switch (i)
                    {
                        case 1: first.SetActive(true);
                            second.SetActive(false);
                            third.SetActive(false);
                            break;
                        case 2: first.SetActive(false);
                            second.SetActive(true);
                            third.SetActive(false);
                            break;
                        case 3: first.SetActive(false);
                            second.SetActive(false);
                            third.SetActive(true);
                            break;
                    }
                }
                else
                {
                    first.SetActive(false);
                    second.SetActive(false);
                    third.SetActive(false);
                    if (rank_min == rank_max)
                    {
                        GameCommon.SetUIText(item, "peak_rank_num_reward", rank_max.ToString());
                    }
                    else
                    {
                        GameCommon.SetUIText(item, "peak_rank_num_reward", "第" + rank_min.ToString() + "-" + rank_max.ToString() + "名");
                    }
                }
                string reward_what = info.get("AWARD_GROUPID");
                string[] reward_group = reward_what.Split('|');
                string[] first_reward = reward_group[0].Split('#');
                string[] second_reward = reward_group[1].Split('#');
                GameCommon.SetItemIconNew(item, "reward_icon1", int.Parse(first_reward[0]), true);
                GameCommon.SetItemIconNew(item, "reward_icon2", int.Parse(second_reward[0]), true);
                GameCommon.SetUIText(item, "reward_label1", "X" + first_reward[1]);
                GameCommon.SetUIText(item, "reward_label2", "X" + second_reward[1]);
				AddButtonAction (GameCommon.FindObject (item, "icon_tips_btn1"), () => GameCommon.SetItemDetailsWindow (int.Parse(first_reward[0])));
				AddButtonAction (GameCommon.FindObject (item, "icon_tips_btn2"), () => GameCommon.SetItemDetailsWindow (int.Parse(second_reward[0])));
                if (i == 1)
                {
                    string power_reward_tips = DataCenter.mStringList.GetData((int)STRING_INDEX.RANK_ACTIVITY_POWER_REWARD_TIPS, "STRING_CN");
                    power_reward_tips = power_reward_tips.Replace("{0}", can_reward_num.ToString()).Replace("{1}", first_reward[1]);
                    GameCommon.SetUIText(mGameObjUI, "introduce_reward", power_reward_tips);
                }
            }           
        }
        if (RanklistActivityUIWindow.mpowerRank > 0)
        {
            GameCommon.SetUIText(mGameObjUI, "activity_rank_number_label_reward", RanklistActivityUIWindow.mpowerRank.ToString());
        }
        else
        {
            GameCommon.SetUIText(mGameObjUI, "activity_rank_number_label_reward", DataCenter.mStringList.GetData((int)STRING_INDEX.RANK_ACTIVITY_OUT, "STRING_CN"));
        }
        UIScrollView mscroll = GameCommon.FindObject(mGameObjUI, "rank_list_scrollView_reward").GetComponent<UIScrollView>();
        mscroll.ResetPosition();
    }
}
  
public class Button_activity_rank_window_info_back : CEvent
{
	public override bool _DoEvent ()
	{
        DataCenter.CloseWindow("RANKLIST_ACTIVITY_UI_WINDOW");
        DataCenter.CloseWindow("RANKLIST_ACTIVITY_BACK_WINDOW");
        MonoBehaviour.DestroyObject(GameCommon.FindObject(GameObject.Find("UI Root"), "activity_rank_window"));
		MainUIScript.Self.OpenMainWindowByIndex (MAIN_WINDOW_INDEX.RoleSelWindow);
		return true;
	}
}
public class Button_peak_button : CEvent
{
	public override bool _DoEvent ()
	{
        NetManager.RequestActivityPeak();
		return true;
	}
}
public class Button_power_button : CEvent
{
    public override bool _DoEvent()
    {
        NetManager.RequestActivityPower();
        return true;
    }
}
public class Button_peak_see_reward : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("RANKLIST_ACTIVITY_REWARD_UI_WINDOW");
        DataCenter.SetData("RANKLIST_ACTIVITY_REWARD_UI_WINDOW", "REFRESH_PEAK",null);
        return true;
    }
}
public class Button_power_see_reward : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("RANKLIST_ACTIVITY_REWARD_UI_WINDOW");
        DataCenter.SetData("RANKLIST_ACTIVITY_REWARD_UI_WINDOW", "REFRESH_POWER", null);
        return true;
    }
}
public class Button_peak_act : CEvent
{
    public override bool _DoEvent()
    {
        if (RoleLogicData.GetMainRole().level < 10)
        {
            DataCenter.OpenMessageWindow("10级才可进入哦!");
        }
        else
        {
            System.Action _goToGetFragmentWinHandler = null;
            if (GetPathHandlerDic.HandlerDic.TryGetValue(GET_PARTH_TYPE.PVP, out _goToGetFragmentWinHandler))
            {
                //关闭当前窗口
                DataCenter.CloseWindow("RANKLIST_ACTIVITY_UI_WINDOW");
                DataCenter.CloseWindow("RANKLIST_ACTIVITY_BACK_WINDOW");
                MonoBehaviour.DestroyObject(GameCommon.FindObject(GameObject.Find("UI Root"), "activity_rank_window"));
                MainUIScript.Self.ShowMainBGUI();
                _goToGetFragmentWinHandler();
            }
            if (_goToGetFragmentWinHandler == null)
                DataCenter.OpenMessageWindow("该功能暂未开启");
        }
        return true;
    }
}
public class Button_power_act : CEvent
{
    public override bool _DoEvent()
    {
        System.Action _goToGetFragmentWinHandler = null;
        if (GetPathHandlerDic.HandlerDic.TryGetValue(GET_PARTH_TYPE.PET_LEVELUP, out _goToGetFragmentWinHandler))
        {
            //关闭当前窗口
            DataCenter.CloseWindow("RANKLIST_ACTIVITY_UI_WINDOW");
            DataCenter.CloseWindow("RANKLIST_ACTIVITY_BACK_WINDOW");
            MonoBehaviour.DestroyObject(GameCommon.FindObject(GameObject.Find("UI Root"), "activity_rank_window"));            
            MainUIScript.Self.ShowMainBGUI();
            _goToGetFragmentWinHandler();
            MainUIScript.Self.mWindowBackAction = () =>
            {
                MainUIScript.Self.OpenMainUI();
                DataCenter.OpenWindow("RANKLIST_ACTIVITY_UI_WINDOW"); 
                NetManager.RequestActivityPower();
            };
        }
        if (_goToGetFragmentWinHandler == null)
            DataCenter.OpenMessageWindow("该功能暂未开启");
        return true;
    }
}

public class Button_activity_rank_reward_window_close : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RANKLIST_ACTIVITY_REWARD_UI_WINDOW");
        return true;
    }
}


