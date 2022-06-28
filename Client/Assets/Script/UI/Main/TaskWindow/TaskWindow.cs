using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;


public class TaskWindow : tWindow
{
    private bool mIsDailyDirty = true;
    private bool mIsAchieveDirty = true;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_task_daily_btn", new DefineFactory<Button_task_daily_btn>());
        EventCenter.Self.RegisterEvent("Button_task_achievement_btn", new DefineFactory<Button_task_achievement_btn>());
        EventCenter.Self.RegisterEvent("Button_task_back_btn", new DefineFactory<Button_task_back_btn>());
        EventCenter.Self.RegisterEvent("Button_treasure_score_award", new DefineFactory<Button_treasure_score_award>());
        EventCenter.Self.RegisterEvent("Button_task_shop_button",new DefineFactory<Button_task_shop_button>());
    }

    public override void OnOpen()
    {
        mIsDailyDirty = true;
        mIsAchieveDirty = true;
        DataCenter.OpenWindow("TASK_BACK_WINDOW");
		GameCommon.ToggleButton(mGameObjUI, "task_daily_btn");
        /*
         *task_daily_btn应该是优先 
         *但由于该版本daily被锁，所以临时优先打开成就任务
         *临时方案
         *by lhc
         */
//        GameCommon.ToggleButton(mGameObjUI,"task_achievement_btn");


        RefreshAcceptableMark();
    }

    public override void OnClose()
    {
        DataCenter.CloseWindow("TASK_BACK_WINDOW");
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "PAGE_DAILY":
                //by chenliang
                //begin

                //点击进入时也要刷新
                mIsDailyDirty = true;

                //end
                RefreshDailyTasks();
                break;

            case "PAGE_ACHIEVEMENT":
                //by chenliang
                //begin

                //点击进入时也要刷新
                mIsAchieveDirty = true;

                //end
                RefreshAchievements();
                break;

            case "ACCEPT_SCORE_AWARD":
                DoCoroutine(DoAcceptScoreAward((int)objVal));
			RefreshDailyScoreInfo();
                break;
        }
    }

    private void RefreshAcceptableMark()
    {
        GameCommon.SetUIVisiable(GetSub("task_daily_btn"), "NewMark", TaskState.hasDailyAwardAcceptable);
        GameCommon.SetUIVisiable(GetSub("task_achievement_btn"), "NewMark", TaskState.hasAchieveAwardAcceptable);
        DataCenter.SetData("ROLE_SEL_TOP_RIGHT_GROUP", "UPDATE_ROLE_SELECT_SCENE", true);
    }

    private void RefreshDailyTasks()
    {
        if (!mIsDailyDirty)
            return;

        mIsDailyDirty = false;

        RefreshDailyScoreInfo();

        props = TaskProperty.GetPropertyList(TASK_PAGE.DAILY, x => x.visible);
        props.Sort(TaskProperty.DefaultComparison);

        GameObject parent = GetSub("task_daily_window");     
        var grid = GameCommon.FindComponent<GridsContainer>(parent, "grid");
        grid.MaxCount = props.Count;
        grid.FuncRefreshCell=ShowTaskCell;
        grid.RefreshAllCell();

        //for (int i = 0; i < grid.MaxCount; ++i)
        //{
        //    ShowTaskCell(grid.controlList[i], props[i]);
        //}
    }
    List<TaskProperty> props;
    private void RefreshAchievements()
    {
        if (!mIsAchieveDirty)
            return;

        mIsAchieveDirty = false;

        props = TaskProperty.GetPropertyList(TASK_PAGE.ACHIEVEMENT, x => x.visible && !x.accepted);
        props.Sort(TaskProperty.DefaultComparison);

        GameObject parent = GetSub("task_achievement_window");
        var grid = GameCommon.FindComponent<GridsContainer>(parent, "grid");
        grid.MaxCount = props.Count;
        grid.FuncRefreshCell=ShowTaskCell;
        grid.RefreshAllCell();
        //for (int i = 0; i < grid.MaxCount; ++i)
        //{
        //    ShowTaskCell(grid.controlList[i], props[i]);
        //}
    }

    private void RefreshDailyScoreInfo()
    {
        GameObject parent = GetSub("task_daily_window");
        GameObject go = GameCommon.FindObject(parent, "integral_info");
        GameObject scoreLabel = GameCommon.FindObject(go, "integral_label");
        GameCommon.SetUIText(scoreLabel, "Label", TaskLogicData.Instance.curScore.ToString());

        UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(go, "treasure_grid");      

        for (int i = 0; i < 4; ++i)
        {
            int score = TaskProperty.GetDailyTaskAwardNeedScore(i + 1);
            GameCommon.SetUIText(grid.controlList[i], "Label", score.ToString());
			GameObject taskGetRewadsTipsObj = grid.controlList[i].transform.Find ("task_get_rewads_tips").gameObject;
			taskGetRewadsTipsObj.SetActive (false);
			GameObject hasNoOpenObj = GameCommon.FindObject (grid.controlList[i], "has_no_open").gameObject;
			GameObject hasOpenObj = GameCommon.FindObject (grid.controlList[i], "has_open").gameObject;
            GameObject hasAcceptObj = GameCommon.FindObject(grid.controlList[i], "has_accepted").gameObject;
			hasNoOpenObj.SetActive (false);
			hasOpenObj.SetActive (false);

            if (TaskLogicData.Instance.curScore < score)
            {
                hasNoOpenObj.SetActive(true);
                hasOpenObj.SetActive(false);
                hasAcceptObj.SetActive(false);
            }
            else
            {
                if (TaskLogicData.Instance.IsScoreAwardAccepted(i + 1))
                {
                    hasNoOpenObj.SetActive(false);
                    hasOpenObj.SetActive(true);
                    hasAcceptObj.SetActive(false);
                    taskGetRewadsTipsObj.SetActive(false);

                }
                else
                {
                    hasNoOpenObj.SetActive(false);
                    hasOpenObj.SetActive(false);
                    hasAcceptObj.SetActive(true);
                    taskGetRewadsTipsObj.SetActive(true);
                }
            }

            GameObject btn = GameCommon.FindObject(grid.controlList[i], "treasure_score_award");
            int index = i + 1;
            btn.GetComponent<UIButtonEvent>().AddAction(() => OnScoreAwardBtn(index));
        }

        float maxScore = (float)TaskProperty.GetDailyTaskAwardNeedScore(4);
        maxScore *= 1.25f;
        float rate = TaskLogicData.Instance.curScore / maxScore;
        UIProgressBar bar = GameCommon.FindComponent<UIProgressBar>(go, "Progress Bar");
        bar.value = rate;
    }

    private void ShowTaskCell(int index,GameObject go)
    {
        var prop=props[index];
//        GameCommon.SetIcon(go, "item_icon", prop.taskIconSprite, prop.taskIconAtlas);
		int _iTid = prop.taskIconTid;
		GameCommon.SetOnlyItemIcon (go, "item_icon",  _iTid);
		AddButtonAction (GameCommon.FindObject (go, "item_icon"), () => GameCommon.SetItemDetailsWindow (_iTid));

        GameCommon.SetUIText(go, "task_daily_label", prop.taskDesc);
        GameCommon.SetUIText(go, "reward_desc", prop.awardDesc);

        GameObject info = GameCommon.FindObject(go, "info_bg");
        UILabel label1 = GameCommon.FindComponent<UILabel>(info, "num_label");
        UILabel label2 = GameCommon.FindComponent<UILabel>(label1.gameObject, "label");
        label1.text = prop.progress.ToString();
        label1.color = prop.completed ? Color.green : Color.red;
        label2.text = "/ " + prop.aim;
		label2.width = 82;

        GameObject getAwardBtn = GameCommon.FindObject(go, "task_get_award_btn");
        GameObject goToBtn = GameCommon.FindObject(go, "task_go_btn");

        getAwardBtn.SetActive(prop.completed && !prop.accepted);
        goToBtn.SetActive(!prop.completed);
        GameCommon.SetUIVisiable(go, "already_accept", prop.accepted);

        if (prop.completed && !prop.accepted)
        {
            getAwardBtn.GetComponent<UIButtonEvent>().AddAction(() => OnGetAwardBtn(prop));
        }
        if (!prop.completed)
        {
            goToBtn.GetComponent<UIButtonEvent>().AddAction(() => OnGoToBtn(prop));
        }
    }

    private void OnGetAwardBtn(TaskProperty prop)
    {
        DoCoroutine(DoGetAward(prop));
    }

    private void OnGoToBtn(TaskProperty prop)
    {
        // 判断对应功能是否开启
        if (!GameCommon.IsFuncOpen(prop.gotoindex))
        {
            DataCenter.ErrorTipsLabelMessage(STRING_INDEX.FUNC_NOT_OPEN);
            return;
        }
        //前往
        System.Action _goToGetFragmentWinHandler = null;
        int iTypeId = TableCommon.GetNumberFromGainFunctionConfig(prop.gotoindex, "Type");
        GET_PARTH_TYPE parthType = (GET_PARTH_TYPE)iTypeId;
        if (GetPathHandlerDic.HandlerDic.TryGetValue(parthType, out _goToGetFragmentWinHandler))
        {
            //关闭当前窗口
            if (parthType != GET_PARTH_TYPE.CHARGE)
            {
                DataCenter.CloseWindow("TASK_WINDOW");
            }
            //by chenliang
            //begin

//             MainUIScript.Self.ShowMainBGUI();
//             _goToGetFragmentWinHandler();
//------------------
            //MainUIScript.Self.ShowMainBGUI();
            _goToGetFragmentWinHandler();

            //end
        }
        if (_goToGetFragmentWinHandler == null)
        {
            DataCenter.ErrorTipsLabelMessage(STRING_INDEX.FUNC_NOT_DEVELOP);            
        }
    
    }

    private void OnScoreAwardBtn(int index)
    {
        DataCenter.OpenWindow("GET_SCORE_AWARD_WINDOW", index);
		GameObject .Find ("task_back_window").GetComponent <UIPanel  > ().depth=4 ;
		UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "treasure_grid");   
		GameObject taskGetRewadsTipsObj = grid.controlList[index - 1].transform.Find ("task_get_rewads_tips").gameObject;
		taskGetRewadsTipsObj.SetActive (false);
	}

    private IEnumerator DoGetAward(TaskProperty prop)
    {
        //by chenliang
        //begin

        //判断背包是否已满
        List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(prop.awardItems);
        if (!CheckPackage.Instance.CanStartMission(tmpPackageTypes))
            yield break;

        //end
        if (prop.page == TASK_PAGE.DAILY)
        {
            GetDailyTaskAwardRequester req = new GetDailyTaskAwardRequester(prop.taskId);
            yield return req.Start();

            if (req.success)
            {
//                DataCenter.OpenWindow("GET_REWARDS_WINDOW", req.awardItems.ToArray());
				DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", req.awardItems.ToArray());     

                int t = DataCenter.mTaskConfig.GetData(prop.taskId, "TYPE");

                if (t == 8 || t == 9)
                {
                    yield return new GetDailyTaskDataRequester().Start();
                }

                mIsDailyDirty = true;
                RefreshDailyTasks();
                RefreshAcceptableMark();
            }
        }
        else 
        {
            GetAchievementAwardRequester req = new GetAchievementAwardRequester(prop.taskId);
            yield return req.Start();

            if (req.success)
            {
//                DataCenter.OpenWindow("GET_REWARDS_WINDOW", req.awardItems.ToArray());
				DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", req.awardItems.ToArray());     

                int t = DataCenter.mAchieveConfig.GetData(prop.taskId, "TYPE");

                if (t == 8 || t == 9)
                {
                    yield return new GetAchievementDataRequester().Start();
                }

                mIsAchieveDirty = true;
                RefreshAchievements();
                RefreshAcceptableMark();
            }
        }
    }

    private IEnumerator DoAcceptScoreAward(int awardIndex)
    {
        GetTaskScoreAwardRequester req = new GetTaskScoreAwardRequester(awardIndex);
        yield return req.Start();

        if (req.success)
        {
            //added by xuke
            //DataCenter.OpenWindow("AWARD_WINDOW", req.awardItems);
//            DataCenter.OpenWindow("GET_REWARDS_WINDOW", req.awardItems.ToArray());            
            //end
			DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", req.awardItems.ToArray());     
            RefreshDailyScoreInfo();
            RefreshAcceptableMark();
        }  
    }
}


public class Button_task_daily_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("TASK_WINDOW", "PAGE_DAILY", true);
        return true;
    }
}


public class Button_task_achievement_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("TASK_WINDOW", "PAGE_ACHIEVEMENT", true);
        return true;
    }
}


public class Button_task_back_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("TASK_WINDOW");
		MainUIScript.Self.ShowMainBGUI ();
        return true;
    }
}


public class Button_treasure_score_award : CEvent
{
    public override bool _DoEvent()
    {
        return base._DoEvent();
    }
}

public class Button_task_shop_button : CEvent 
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("TASK_WINDOW");
//        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
		DataCenter.OpenWindow("SHOP_WINDOW");
		//DataCenter.OpenWindow("BACK_GROUP_SHOP_WINDOW");

        return true;
    }
}

public class GetAwardWindow : tWindow
{
 
}