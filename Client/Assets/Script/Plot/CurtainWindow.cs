using UnityEngine;
using Logic;


public class CurtainWindow : tWindow
{
    public override void OnOpen()
    {
        TweenAlpha tween = mGameObjUI.GetComponentInChildren<TweenAlpha>();

        if (tween != null)
        {
            EventDelegate.Add(tween.onFinished, OnTweenFinish, true);
            tween.ResetToBeginning();
            tween.PlayForward();
        }
        else
        {
            OnTweenFinish();
        }
    }

    private void OnTweenFinish()
    {
        //DataCenter.CloseWindow("MASK_OPERATE_WINDOW");
        //MainProcess.ClearBattle();
        //DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "OPEN", true);
        //GuideManager.OpenDialog(1001, 0.5f, Close);
    }
}