using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;


public class GetScoreAwardWindow : tWindow
{
    private int mAwardIndex = 0;

    public override void Open(object param)
    {
        base.Open(param);

        mAwardIndex = (int)param;

        if (TaskLogicData.Instance.IsScoreAwardAccepted(mAwardIndex))
        {
            SetVisible("get_score_award_btn", false);
            SetVisible("accepted_label", true);
        }
        else if (TaskLogicData.Instance.curScore < TaskProperty.GetDailyTaskAwardNeedScore(mAwardIndex))
        {
            SetVisible("get_score_award_btn", true);
            SetVisible("accepted_label", false);
            GameCommon.SetUIButtonEnabled(mGameObjUI, "get_score_award_btn", false);
        }
        else 
        {
            SetVisible("get_score_award_btn", true);
            SetVisible("accepted_label", false);
            GameCommon.SetUIButtonEnabled(mGameObjUI, "get_score_award_btn", true);
            AddButtonAction("get_score_award_btn", OnGetScoreAward);
        }

        List<ItemDataBase> itemList = TaskProperty.GetDailyTaskScoreAwardItems(mAwardIndex);
        int count = itemList.Count;

        UIScrollView scrollview = GetComponent<UIScrollView>("Scroll View");
        UIGridContainer container = GetComponent<UIGridContainer>("Grid");
        GameObject scrollbar = GetSub("scroll_bar");

        container.MaxCount = 0;
        container.MaxCount = count;
        scrollview.ResetPosition();
        scrollview.enabled = count > 4;
        scrollbar.SetActive(count > 4);
        container.transform.localPosition = new Vector3(count < 4 ? -50 * (count - 1) : -150, 0, 0);

        for (int i = 0; i < container.MaxCount; ++i)
        {
            GameObject obj = container.controlList[i];
            ItemDataBase item = itemList[i];
            //GameCommon.SetItemIcon(obj, item);
			GameCommon.SetUIText(obj,"count_label","x" + item.itemNum);
			GameCommon.SetOnlyItemIcon(GameCommon.FindObject(obj,"item_icon"),item.tid);
			AddButtonAction (GameCommon.FindObject(obj, "item_icon"), () => GameCommon.SetAccountItemDetailsWindow (item.tid));
        }
    }

    private void OnGetScoreAward()
    {
        DataCenter.SetData("TASK_WINDOW", "ACCEPT_SCORE_AWARD", mAwardIndex);
		GameObject .Find ("task_back_window").GetComponent <UIPanel > ().depth = 16;
        Close();
    }
}