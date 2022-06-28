using UnityEngine;
using System;


public partial class GuideManager
{
    private static void PrepareGuide(GuideIndex index)
    {
        MainUIScript.mLoadingFinishAction = null;
        MainUIScript.mbAlwaysOpenMenu = !IsGuideFinished((int)index);

        switch (index)
        {
            case GuideIndex.Prologue:
                PreparePrologue();
                break;

            case GuideIndex.EnterMainUIForShop:
                PrepareEnterMainUIForShop();
                break;

            //case GuideIndex.BuySecondFreePet:
            //    PrepareBuySecondFreePet();
            //    break;

            case GuideIndex.EnterMainUIForMap:
                PrepareEnterMainUIForMap();
                break;

            case GuideIndex.ReturnToStageInfo:
                PrepareReturnToStageInfo();
                break;

            case GuideIndex.EnterMainUIForTask:
                PrepareEnterMainUIForTask();
                break;

            case GuideIndex.EnterMainUIForPet:
                PrepareEnterMainUIForPet();
                break;

            case GuideIndex.EnterMainUIForMap2:
                PrepareEnterMainUIForMap2();
                break;

            case GuideIndex.EnterMainUIForRolePage:
                PrepareEnterMainUIForRolePage();
                break;

            case GuideIndex.EnterMainUIForMap3:
                PrepareEnterMainUIForMap3();
                break;

            case GuideIndex.EnterMainUIForFriend:
                PrepareEnterMainUIForFriend();
                break;
        }
    }

    private static void PreparePrologue()
    {
        Notify(GuideIndex.Prologue);
    }

    private static void PrepareEnterMainUIForShop()
    {
        MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForShop);
    }

    //private static void PrepareBuySecondFreePet()
    //{
    //    MainUIScript.mLoadingFinishAction = () =>
    //    {
    //        OpenMaskWithoutOperateRegion();
    //
    //        ExecuteDelayed(() =>
    //            {
    //                MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
    //                Notify(GuideIndex.BuySecondFreePet);
    //            },
    //            0f);
    //    };
    //}

    private static void PrepareReturnToStageInfo()
    {
        MainUIScript.mLoadingFinishAction = () =>
        {
            OpenMaskWithoutOperateRegion();

            ExecuteDelayed(() =>
                {
                    MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
                    DataCenter.OpenWindow("STAGE_INFO_WINDOW", GUIDE_STAGE_INDEX);
                    Notify(GuideIndex.ReturnToStageInfo);
                },
                0f);
        };
    }

    private static void PrepareEnterMainUIForMap()
    {
        MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForMap);
    }

    private static void PrepareEnterMainUIForTask()
    {
        MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForTask);
    }

    private static void PrepareEnterMainUIForPet()
    {
        MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForPet);
    }

    private static void PrepareEnterMainUIForMap2()
    {
        MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForMap2);
    }

    private static void PrepareEnterMainUIForMap3()
    {
        MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForMap3);
    }

    private static void PrepareEnterMainUIForFriend()
    {
        MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForFriend);
    }

    private static void PrepareEnterMainUIForRolePage()
    {
        MainUIScript.mLoadingFinishAction = () => Notify(GuideIndex.EnterMainUIForRolePage);
    }
}