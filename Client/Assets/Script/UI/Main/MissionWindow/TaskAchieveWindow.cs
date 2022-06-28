using UnityEngine;
using Logic;

public class TaskAchieveWindow : tWindow
{
    private TM_FadeOut evt = null;

    public override void Open(object param)
    {
        base.Open(param);

        int taskID = 0;
        tNiceData info = param as tNiceData;

        if (info != null)
            taskID = info.get("TASK_ID");

        if (taskID > 0)
        {
            string taskName = TableCommon.GetStringFromTaskConfig(taskID, "TASK_NAME");
            string taskDesc = TableCommon.GetStringFromTaskConfig(taskID, "TASK_TIP");
            int iNeedNum = TableCommon.GetNumberFromTaskConfig(taskID, "TASK_NUM");
            taskDesc = string.Format(taskDesc, iNeedNum, iNeedNum);
            GameCommon.SetUIText(mGameObjUI, "task_name", taskName);
            GameCommon.SetUIText(mGameObjUI, "task_desc", taskDesc);
        }
    }

    public override void OnOpen()
    {
        evt = EventCenter.Self.StartEvent("TM_FadeOut") as TM_FadeOut;
        evt.mTarget = GetSub("win");
        evt.mBefore = 2f;
        evt.mDuration = 1f;
        evt.mAfter = 0.5f;
        evt.onElapsed += x => { Close(); Notification.Next(); };
        evt.DoEvent();
    }

    public override void Close()
    {
        if (evt != null && !evt.GetFinished())
            evt.Finish();
        evt = null;
        base.Close();
    }
}