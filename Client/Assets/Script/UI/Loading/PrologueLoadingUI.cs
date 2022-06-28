using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities;


public class PrologueLoadingUI : GameLoadingWithAnimUI
{ }


public class PrologueLoadingWindow : GameLoadingWithAnimWindow
{
    protected override void _OnStartLoading(object param)
    {
        this.StartCoroutine(DoLoad());
    }

    private IEnumerator DoLoad()
    {
        float tmpProgress = 0.0f;
        float tmpProgressStepCount = 3.0f;          //如果新加了请求步骤，需要设置此变量为新的请求步骤数量
        float tmpProgressStep = 1.0f / tmpProgressStepCount;

        GameObject prologueWindow = GameCommon.FindUI("prologue_window");

        if (prologueWindow != null)
        {
            GameObject.DestroyImmediate(prologueWindow);
        }
        tmpProgress += tmpProgressStep;
        _SetLoadingProgress(tmpProgress);
        yield return null;

        DataCenter.OpenWindow("SELECT_CREATE_ROLE_WINDOW");
        tmpProgress += tmpProgressStep;
        _SetLoadingProgress(tmpProgress);
        yield return null;

        tmpProgress += tmpProgressStep;
        _SetLoadingProgress(tmpProgress);
    }
}