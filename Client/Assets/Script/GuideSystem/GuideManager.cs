using DataTable;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using Utilities;
using Utilities.Events;
using Utilities.Tasks;


public enum GuideIndex
{
    None = 0,

    Prologue = 1000,
    SelectRole,
    SelectPet,
    SelectName,
    NamingSucceed,
    EnterMainUIForShop,
    BuyFirstFreePet,
    GetFirstFreePet,
    GetFirstFreePetEnd,

    //BuySecondFreePet,
    //GetSecondFreePet,
    //GetSecondFreePetEnd,
    EnterMainUIForMap,
    EnterWorldMap,
    EnterStageInfo,
    EnterTeamWindow,
    ChangeTeamOK,

    ReturnToStageInfo,
    EnterBattle,
    ReadyBattle,
    EncounterMonster,
    EncounterBoss,
    BattleAccountStart,
    BattleAccountEnd,

    EnterMainUIForTask,
    EnterTaskWindow,
    AcceptTaskAward,

    EnterMainUIForPet,
    EnterTeamWindow2,
    EnterUpgradeWindow,
    PetUpgradeOK,

    EnterMainUIForMap2,
    EnterWorldMap2,
    EnterStageInfo2,
    EnterBattle2,
    ReadyBattle2,
    BattleAccountStart2,
    BattleAccountEnd2,

    EnterMainUIForRolePage,
    EnterRolePage,
    LoadRoleEquipOK,

    EnterMainUIForMap3,
    EnterWorldMap3,
    EnterStageInfo3,
    EnterFriendHelpWindow,
    EnterBattle3,
    ReadyBattle3,
    EncounterBoss2,
    BattleAccountStart3,
    BattleAccountEnd3,

    TriggerBoss,
    EnterMainUIForShop2,
    BuySecondFreePet,
    GetSecondFreePet,
    EnterMainUIForPet2,
    EnterTeamWindow3,
    ChangeTeamOK2,
    EnterMainUIForBoss,
    EnterBossListWindow,
    EnterBossBattle,
    BossAccountStart,
    BossAccountEnd,

    EnterMainUIForFriend,
    EnterFriendWindow,
    EnterAddFriendPage,
    //AddFriendSucceed
    Max = 2000
}


public struct GuideEvent
{
    public const string OnMaskMouseClick = "OnMaskMouseClick";
    public const string OnDialogFinish = "OnDialogFinish";  
}


public partial class GuideManager
{
    public const int GUIDE_STAGE_INDEX = 33101;
    public const int GUIDE_BUFF_INDEX = 10001;
    public const string GUIDE_STAGE_NAME = "loyalismwood_1";

    public static int currentIndex { get; private set; }
    public static int startIndex { get; private set; }
    public static int finishIndex { get; private set; }
    public static bool locked { get; private set; }
    public static bool achieved { get; private set; }

    private static Action waitAction = null;

    private static GameObject finger;
    private static TM_UpdateEvent monsterTrigger;
    private static TM_UpdateEvent forceIdelEvent;
    private static TM_UpdateEvent forceStopAIEvent;
    private static TaskQueue<ITask> taskQueue;
    private static Subscriber sbr = new Subscriber();


    static GuideManager()
    {
        // 清理垃圾数据
        PlayerPrefs.DeleteKey("GUIDE_PROCESS");
        PlayerPrefs.DeleteKey("GUIDE_ACHIEVED");
        
        startIndex = (int)GuideIndex.Prologue;
        finishIndex = (int)GuideIndex.Max;
    }

    public static void LoadGuideProcess(int netProc)
    {
        //GamePrefs.DeleteKey("GUIDE_PROCESS");
        //return;
        UnlockGuide();

        if (currentIndex == (int)GuideIndex.EnterMainUIForShop)
        {
            SaveProcessOnNet((int)GuideIndex.EnterMainUIForShop);
            PrepareGuide(GuideIndex.EnterMainUIForShop);
        }
        else 
        {
            int localProc = LoadProcessOnLocal();
            currentIndex = HandleProcessConflict(localProc, netProc);
            PrepareGuide((GuideIndex)currentIndex);
        }               
    }

    public static void SaveGuideProcess(GuideIndex index)
    {
        SaveProcessOnNet((int)index);
        SaveProcessOnLocal((int)index);
    }

    public static int LoadProcessOnLocal()
    {
        //achieved = PlayerPrefs.GetInt("GUIDE_ACHIEVED", 0) == 1;
        achieved = GamePrefs.Get<bool>("GUIDE_PROCESS/__ACHIEVED", false);

        if (string.IsNullOrEmpty(CommonParam.mAccount))
        {
            return (int)GuideIndex.None;
        }

        string key = "GUIDE_PROCESS/" + LoginNet.msGameServerIP + "/" + CommonParam.mAccount;
        return GamePrefs.Get<int>(key, (int)GuideIndex.None);
        //
        //Dictionary<string, int> process = null;
        //
        //if (!GamePrefs.TryGet<Dictionary<string, int>>("GUIDE_PROCESS", out process))
        //{
        //    ClearGuideProcess();
        //    return (int)GuideIndex.None;
        //}
        //
        //return process.ContainsKey(CommonParam.mAccount) ? process[CommonParam.mAccount] : (int)GuideIndex.None;
    }

    public static int HandleProcessConflict(int local, int net)
    {
        Type t = typeof(GuideIndex);

        if (!Enum.IsDefined(t, local) && !Enum.IsDefined(t, net))
        {
            SaveProcessOnLocal((int)GuideIndex.Max);
            SaveProcessOnNet((int)GuideIndex.Max);
            return (int)GuideIndex.Max;
        }

        if (local > net || !Enum.IsDefined(t, net))
        {
            SaveProcessOnNet(local);
            return local;
        }
        else if (local < net || !Enum.IsDefined(t, local))
        {
            SaveProcessOnLocal(net);
            return net;
        }
        else
        {
            return local;
        }
    }

    private static void SaveProcessOnLocal(int index)
    {
        if (string.IsNullOrEmpty(CommonParam.mAccount))
        {
            return;
        }

        string key = "GUIDE_PROCESS/" + LoginNet.msGameServerIP + "/" + CommonParam.mAccount;        
        GamePrefs.Set<int>(key, index);
        GamePrefs.Set<bool>("GUIDE_PROCESS/__ACHIEVED", IsGuideFinished(index));

        //Dictionary<string, int> process = null;
        //
        //if (!GamePrefs.TryGet<Dictionary<string, int>>("GUIDE_PROCESS", out process))
        //{
        //    ClearGuideProcess();
        //    process = new Dictionary<string, int>();
        //}
        //
        //if (process.ContainsKey(CommonParam.mAccount))
        //{
        //    process[CommonParam.mAccount] = index;
        //}
        //else 
        //{
        //    process.Add(CommonParam.mAccount, index);
        //}
        //
        //if (IsGuideFinished(index))
        //{
        //    PlayerPrefs.SetInt("GUIDE_ACHIEVED", 1);
        //}
        //
        //GamePrefs.TrySet<Dictionary<string, int>>("GUIDE_PROCESS", process);
    }

    private static void SaveProcessOnNet(int process)
    {
        tEvent evt = Net.StartEvent("CS_SaveGuideProcess");
        evt.set("GUIDE_STATE", process);
        evt.DoEvent();
    }

    public static void ClearGuideProcess()
    {
        GamePrefs.DeleteDir("GUIDE_PROCESS");
        //GamePrefs.DeleteKey("GUIDE_PROCESS");
        //PlayerPrefs.DeleteKey("GUIDE_ACHIEVED");
    }

    public static void StartGuide()
    {
        //by chenliang
        //begin

//         if (((string)DataCenter.mGlobalConfig.GetData("ENABLE_GUIDE", "VALUE")).ToLower() == "no")
//         {
//             return;
//         }
//----------------------
        if (HotUpdateLoading.IsUseHotUpdate)
        {
            if (!Guide.EnableGuide)
                return;
        }
        else
        {
            if (((string)DataCenter.mGlobalConfig.GetData("ENABLE_GUIDE", "VALUE")).ToLower() == "no")
                return;
        }

        //end

        UnlockGuide();
        //achieved = PlayerPrefs.GetInt("GUIDE_ACHIEVED", 0) == 1;
        achieved = GamePrefs.Get<bool>("GUIDE_PROCESS/__ACHIEVED", false);
        currentIndex = GuideManager.startIndex;
        PrepareGuide((GuideIndex)currentIndex);
    }

    public static void ExecuteQueue(params ITask[] tasks)
    {
        ExecuteQueue(true, tasks);
    }

    public static void ExecuteQueue(bool addMaskAtEnd, params ITask[] tasks)
    {
        ClearQueue();
        taskQueue = CreateGuideQueue(addMaskAtEnd, tasks);
        taskQueue.Start();
    }

    public static void ExecuteQueueThenNext(params ITask[] tasks)
    {
        ClearQueue();
        taskQueue = CreateGuideQueue(false, tasks);
        taskQueue.Accept(new Act(() => Notify((GuideIndex)currentIndex)));
        taskQueue.Start();
    }

    public static void ClearQueue()
    { 
        if (taskQueue != null)
        {
            taskQueue.Clear();
        }
    }

    public static void OpenDialog(int index)
    {
        OpenDialog(index, 0f);
    }

    public static void OpenDialog(int index, float delay)
    {
        OpenDialog(index, delay, null);
    }

    public static void OpenDialog(int index, float delay, Action onDialogFinish)
    {
        DialogWindow.SetOpenDelay(delay);
        DataCenter.OpenWindow("GUIDE_DIALOG_WINDOW", index);
        if (onDialogFinish != null)
        {
            ObserverCenter.Add(GuideEvent.OnDialogFinish, onDialogFinish);
        }
    }

    public static void OpenDialogOnStageStart(int index)
    {      
        Action onDialogFinish = () =>
        {
            MainProcess.mCameraMoveTool._MoveByObject(Character.Self);
            BattleRestartAI();
            TM_WaitToPlayPveBeginTextEffect.Play();
        };

        Action onEnterBattle = () =>
        {
            OpenDialog(index, 0.5f, onDialogFinish);
            MainProcess.mCameraMoveTool.MoveTo(Character.Self.mMainObject.transform.position);
            BattleStopAI();
        };

        ExecuteDelayed(onEnterBattle, 0f);
    }

    public static void CloseDialog()
    {
        DataCenter.CloseWindow("GUIDE_DIALOG_WINDOW");
    }

    public static void ClearGuideEventCallBack()
    {
        ObserverCenter.Clear(GuideEvent.OnMaskMouseClick);
        ObserverCenter.Clear(GuideEvent.OnDialogFinish);
    }

    public static int GetCurrentDialogID()
    {
        return DialogWindow.currentDialogID;
    }

    public static void ShowFinger(GameObject uiPoint, float scale)
    {
        DestroyFinger();
        finger = GameCommon.LoadAndIntanciatePrefabs("Prefabs/Finger");
        finger.transform.parent = uiPoint.transform;
        finger.transform.localPosition = Vector3.zero;
        finger.transform.localScale = Vector3.one * scale;
    }

    public static void ShowFinger(GameObject uiPoint, float scale, Vector2 offset)
    {
        DestroyFinger();
        finger = GameCommon.LoadAndIntanciatePrefabs("Prefabs/Finger");
        finger.transform.parent = uiPoint.transform;
        finger.transform.localPosition = new Vector3(offset.x, offset.y, 0f);
        finger.transform.localScale = Vector3.one * scale;
    }

    public static void ShowFinger(Vector3 pos, float scale)
    {
        DestroyFinger();
        GameObject centerAnchor = GameCommon.FindUI("CenterAnchor");
        finger = GameCommon.LoadAndIntanciatePrefabs("Prefabs/Finger");
        finger.transform.parent = centerAnchor.transform;
        finger.transform.localPosition = pos;
        finger.transform.localScale = Vector3.one * scale;
    }

    public static void DestroyFinger()
    {
        if (finger != null)
            MonoBehaviour.DestroyImmediate(finger);
        finger = null;
    }

    public static TM_UpdateEvent CreateUpdateEvent(Func<float, bool> update)
    {
        TM_UpdateEvent evt = EventCenter.Start("TM_UpdateEvent") as TM_UpdateEvent;
        evt.update = update;
        evt._DoEvent();
        return evt;
    }

    public static TM_UpdateEvent CreateRepeatEvent(float delay, Func<float> act, Func<bool> finishCondition)
    {
        if (act == null)
            return null;

        float time = delay;

        Func<float, bool> update = t =>
            {
                if (finishCondition())
                    return false;

                time -= t;

                if (time < 0)
                    time += act();

                return true;
            };

        return CreateUpdateEvent(update);
    }

    public static TM_UpdateEvent CreateTriggerEvent(Func<bool> match, Action onTrigger)
    {
        if (match == null || onTrigger == null)
            return null;

        Func<float, bool> update = t =>
            {
                if (match())
                {
                    onTrigger();
                    return false;
                }
                return true;
            };

        return CreateUpdateEvent(update);
    }

    public static void CreateMonsterTrigger(float range, Predicate<BaseObject> filter, Action onTrigger)
    {
        DestroyMonsterTrigger();

        Predicate<BaseObject> match = x =>
            Character.Self != null 
            && x != null 
            && x != Character.Self 
            && InRange(Character.Self, x, range) 
            && (filter == null || filter(x));

        monsterTrigger = CreateTriggerEvent(() => ExistInVisibleAliveObjects(match), onTrigger);
    }

    public static void DestroyMonsterTrigger()
    {
        if (monsterTrigger != null)
            monsterTrigger.Finish();
        monsterTrigger = null;
    }

    public static void OpenTip(string tip, float x, float y, Action callback)
    {
        DataCenter.OpenWindow("GUIDE_TIP_WINDOW", new Vector3(x, y, 0f));
        DataCenter.SetData("GUIDE_TIP_WINDOW", "TIP", tip);
        OpenMask(() => { CloseTip(); if (callback != null) callback(); });
    }

    public static void CloseTip()
    {
        DataCenter.CloseWindow("GUIDE_TIP_WINDOW");
    }

    public static void OpenMask(Action callback)
    {
        CloseMask();
        DataCenter.OpenWindow("GUIDE_MASK_WINDOW");
        ObserverCenter.Add(GuideEvent.OnMaskMouseClick, callback);
    }

    public static void OpenMask(Rect operateRegion, Action callback, bool showShadow)
    {
        CloseMask();
        DataCenter.OpenWindow("GUIDE_MASK_WINDOW", operateRegion);
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SHADOW", showShadow);
        ObserverCenter.Add(GuideEvent.OnMaskMouseClick, callback);
    }

    public static void OpenMaskRelative(Rect relativeRegion, Action callback, bool showShadow)
    {
        float width = PlotTools.GetLocalWidth();
        float height = PlotTools.GetLocalHeight();
        Rect region = new Rect(width * relativeRegion.x, height * relativeRegion.y, width * relativeRegion.width, height * relativeRegion.height);
        OpenMask(region, callback, showShadow);
    }

    public static void OpenMask(GameObject btn, Action callback)
    {
        OpenMask(btn, callback, 0, 0, 0, 0);
    }

    public static void OpenMask(GameObject btn, Action callback, int leftOffset, int rightOffset, int bottomOffset, int topOffset)
    {
        CloseMask();
        MaskWindow.leftOffset = leftOffset;
        MaskWindow.rightOffset = rightOffset;
        MaskWindow.bottomOffset = bottomOffset;
        MaskWindow.topOffset = topOffset;
        DataCenter.OpenWindow("GUIDE_MASK_WINDOW", btn);
        ObserverCenter.Add(GuideEvent.OnMaskMouseClick, callback);
    }

    public static void OpenButtonMask(GameObject btn, Action callback, int leftOffset, int rightOffset, int bottomOffset, int topOffset, int fingerOffsetX, int fingerOffsetY)
    {
        Action onClick = () => 
        {
            DestroyFinger();
            BoxCollider collider = btn.GetComponentInChildren<BoxCollider>();

            if(collider != null)
                collider.gameObject.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
            else
                btn.SendMessage("OnClick", SendMessageOptions.RequireReceiver);

            if(callback != null)
                callback();
        };

        OpenMask(btn, onClick, leftOffset, rightOffset, bottomOffset, topOffset);
        ShowMaskFinger(1f, new Vector2(fingerOffsetX, fingerOffsetY));
    }

    public static void OpenButtonMask(GameObject btn, Action callback, int leftOffset, int rightOffset, int bottomOffset, int topOffset)
    {
        OpenButtonMask(btn, callback, leftOffset, rightOffset, bottomOffset, topOffset, 0, 20);
    }

    public static void OpenButtonMask(GameObject btn, Action callback, int fingerOffsetX, int fingerOffsetY)
    {
        OpenButtonMask(btn, callback, 0, 0, 0, 0, fingerOffsetX, fingerOffsetY);
    }

    public static void OpenButtonMask(GameObject btn, Action callback)
    {
        OpenButtonMask(btn, callback, 0, 0, 0, 0, 0, 20);
    }

    public static void OpenMaskWithoutOperateRegion()
    {
        CloseMask();
        DataCenter.OpenWindow("GUIDE_MASK_WINDOW", new Rect(0f, 0f, 0f, 0f));
        DataCenter.SetData("GUIDE_MASK_WINDOW", "ONE_SHOT", false);
    }

    public static void OpenMaskInWorldSpace(Camera camera, Vector3 worldPoint, float relativeWidth, float relativeHeight, Action callback)
    {
        Vector3 vp = camera.WorldToViewportPoint(worldPoint);
        Rect region = new Rect(vp.x - 0.5f, vp.y - 0.5f, relativeWidth, relativeHeight);
        OpenMaskRelative(region, callback, true);
    }

    public static void OpenMaskInWorldSpace(Vector3 worldPoint, float relativeWidth, float relativeHeight, Action callback)
    {
        Camera camera = null;

        if (MainProcess.mStage == null)
        {
            GameObject obj = GameObject.Find("Mainmenu_bg/Camera");

            if (obj != null)
            {
                camera = obj.GetComponent<Camera>();
            }
        }
        else 
        {
            camera = MainProcess.mMainCamera;
        }

        if (camera != null)
        {
            OpenMaskInWorldSpace(camera, worldPoint, relativeWidth, relativeHeight, callback);
        }
    }

    public static void CloseMask()
    {
        DataCenter.CloseWindow("GUIDE_MASK_WINDOW");
    }

    public static void ShowMaskFinger(float scale)
    {
        ShowMaskFinger(scale, Vector2.zero);
    }

    public static void ShowMaskFinger(float scale, Vector2 offset)
    {
        FingerLocation location = new FingerLocation();
        location.mScale = scale;
        location.mOffset = offset;
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SHOW_FINGER", location);
    }

    public static bool Notify(GuideIndex index)
    {
        return Notify(index, null);
    }

    public static bool Notify(GuideIndex index, Action action)
    {
        if (!IsGuideFinished() && !locked)
        {
            switch (index)
            {
                case GuideIndex.BattleAccountStart:
                    if (currentIndex == (int)GuideIndex.EncounterBoss)
                    {
                        currentIndex = (int)GuideIndex.BattleAccountStart;
                    }
                    break;

                case GuideIndex.BattleAccountStart3:
                    if (currentIndex == (int)GuideIndex.EncounterBoss2)
                    {
                        currentIndex = (int)GuideIndex.BattleAccountStart3;
                    }
                    break;
            }

            if (currentIndex == (int)index)
            {
                waitAction = action;
                LockGuide();
                OnNotifySucceed(index);
                return true;
            }
        }

        if (action != null)
            action();

        return false;
    }

    public static void PrepareNextGuide()
    {
        UnlockGuide();
        
        switch ((GuideIndex)currentIndex)
        {
            default:
                ++currentIndex;
                break;
        }

        if (IsGuideFinished())
        {
            FinishGuide();
            SaveGuideProcess(GuideIndex.Max);
        }
    }

    public static void FinishGuide()
    {
        if (currentIndex != (int)GuideIndex.Max)
        {
            currentIndex = (int)GuideIndex.Max;
            sbr.CancleAll();
            MainUIScript.mbAlwaysOpenMenu = false;
            DataCenter.CloseWindow("GUIDE_SKIP_WINDOW");
            ExecuteDelayed(OnGuideFinish, 0f);
        }
    }

    public static bool IsGuideFinished()
    {
        return IsGuideFinished(currentIndex);
    }

    public static bool IsGuideFinished(int process)
    {
        return process == (int)GuideIndex.Max
                   || (finishIndex != (int)GuideIndex.Max && process > finishIndex)
                   || !Enum.IsDefined(typeof(GuideIndex), process);
    }

    public static bool InGuideStage(int stageIndex)
    {
        return stageIndex >= GUIDE_STAGE_INDEX && stageIndex <= GUIDE_STAGE_INDEX + 2;
    }

    public static bool InGuidePVEBattle()
    {
        return (currentIndex >= (int)GuideIndex.EnterBattle && currentIndex <= (int)GuideIndex.BattleAccountEnd) ||
            (currentIndex >= (int)GuideIndex.EnterBattle2 && currentIndex <= (int)GuideIndex.BattleAccountEnd2) ||
            (currentIndex >= (int)GuideIndex.EnterBattle3 && currentIndex <= (int)GuideIndex.BattleAccountEnd3);
    }

    public static void SkipToBeforeEnterBattle()
    {
        if (currentIndex >= (int)GuideIndex.EnterBattle && currentIndex <= (int)GuideIndex.BattleAccountEnd)
        {
            currentIndex = (int)GuideIndex.EnterBattle - 1;
        }
        else if (currentIndex >= (int)GuideIndex.EnterBattle2 && currentIndex <= (int)GuideIndex.BattleAccountEnd2)
        {
            currentIndex = (int)GuideIndex.EnterBattle2 - 1;
        }
        else if (currentIndex >= (int)GuideIndex.EnterBattle3 && currentIndex <= (int)GuideIndex.BattleAccountEnd3)
        {
            currentIndex = (int)GuideIndex.EnterBattle3 - 1;
        }
    }

    public static void LockGuide()
    {
        locked = true;
    }

    public static void UnlockGuide()
    {
        locked = false;
    }

    public static void ForceIdle()
    {
        ReleaseIdle();

        Func<float, bool> update = t =>
            {
                ForEachVisibleAliveObject(x => x.ResetIdle());
                return true;
            };

        forceIdelEvent = CreateUpdateEvent(update);
    }

    public static void ReleaseIdle()
    {
        if (forceIdelEvent != null)
            forceIdelEvent.Finish();

        forceIdelEvent = null;
    }

    public static void ForceStopAI()
    {
        ReleaseAI();

        Func<float, bool> update = t =>
        {
            ForEachVisibleAliveObject(x => { x.StopAI(); x.StopMove(); x.mCurrentAI = null; x.PlayAnim("idle"); });
            return true;
        };

        forceStopAIEvent = CreateUpdateEvent(update);
    }

    public static void ReleaseAI()
    {
        if (forceStopAIEvent != null)
            forceStopAIEvent.Finish();

        forceStopAIEvent = null;
    }

    public static void ExecuteDelayed(Action action, float delay)
    {
        TM_DelayEvent delayEvent = EventCenter.Start("TM_DelayEvent") as TM_DelayEvent;
        delayEvent.onOverTime = action;
        delayEvent.WaitTime(delay);
    }

    public static bool ExistInVisibleAliveObjects(Predicate<BaseObject> match)
    {
        return FindInVisibleAliveObjects(match) != null;
    }

    public static void ForEachVisibleAliveObject(Action<BaseObject> act)
    {
        foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
        {
            if (obj != null && obj.mbVisible && !obj.IsDead())
            {
                act(obj);
            }
        }
    }

    public static BaseObject FindInVisibleAliveObjects(Predicate<BaseObject> match)
    {
        foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
        {
            if (obj != null && obj.mbVisible && !obj.IsDead())
            {
                if (match(obj))
                    return obj;
            }
        }

        return null;
    }

    public static void BattleStopAI()
    {
        MainProcess.mLockCharacterPos = true;
        MainProcess.mbStartPetAIOnCreate = false;
        ForceStopAI();
    }

    public static void BattleRestartAI()
    {
        MainProcess.mLockCharacterPos = false;
        MainProcess.mbStartPetAIOnCreate = true;
        ReleaseAI();
        ForEachVisibleAliveObject(x => x.StartAI());
    }

    //public static void SetAutoFightBtnVisible(bool visible)
    //{
    //    DataCenter.SetData("BATTLE_AUTO_FIGHT_BUTTON", "ENABLE_AUTO_FIGHT_BTN", visible);
    //}

    public static void SetPetSkillBtnVisible(bool visible)
    {
        GameObject skillWin = GameCommon.FindUI("battle_skill_window");
        GameCommon.SetUIVisiable(skillWin, "do_skill_father_1", visible);
        GameCommon.SetUIVisiable(skillWin, "do_skill_father_2", visible);
        GameCommon.SetUIVisiable(skillWin, "do_skill_father_3", visible);
    }

    public static void SetFriendSkillBtnVisible(bool visible)
    {
        GameObject skillWin = GameCommon.FindUI("battle_skill_window");
        GameCommon.SetUIVisiable(skillWin, "do_skill_father_4", visible);
    }

    public static void SetCharacterSkillBtnVisible(bool visible)
    {
        GameObject skillWin = GameCommon.FindUI("battle_skill_window");
        GameCommon.SetUIVisiable(skillWin, "do_skill_father_0", visible);
    }

    public static void SetPVEBattleSettingEnabled(bool isEnabled)
    {
        DataCenter.SetData("PVE_TOP_RIGHT_WINDOW", "ENABLE_SETTING_BTN", isEnabled);        
    }

    public static void SetBossBattleSettingEnabled(bool isEnabled)
    {
        DataCenter.SetData("BOSS_ATTACK_INFO_WINDOW", "ENABLE_SETTING_BTN", isEnabled);
    }

    public static void SkipGuide()
    {
        FinishGuide();
        SaveGuideProcess(GuideIndex.Max);
    }

    private static void DoWaitAction()
    {
        if (waitAction != null)
        {
            waitAction();
            waitAction = null;
        }
    }

    private static bool InRange(BaseObject center, BaseObject target, float radius)
    {
        return radius <= 0 || (center.GetPosition() - target.GetPosition()).sqrMagnitude < radius * radius;
    }   

    private static void OnGuideFinish()
    {
        ClearQueue();
        ClearGuideEventCallBack();
        CloseDialog();
        CloseMask();
        CloseTip();
        DestroyFinger();
        DestroyMonsterTrigger();

        if (MainProcess.mStage != null && MainProcess.mStage.mbBattleStart && !MainProcess.mStage.mbBattleFinish)
        {
            Character.Self.StopAffect(GUIDE_BUFF_INDEX);

            if (forceStopAIEvent != null)
            {
                BattleRestartAI();               
            }

            if (MainProcess.mStage is BossBattle)
            {
                SetBossBattleSettingEnabled(true);
            }
            else
            {
                SetPVEBattleSettingEnabled(true);
            }
        }  
    }

    private static TaskQueue<ITask> CreateGuideQueue(params ITask[] tasks)
    {
        return CreateGuideQueue(true, tasks);
    }

    private static TaskQueue<ITask> CreateGuideQueue(bool addMaskAtEnd, params ITask[] tasks)
    {
        TaskQueue<ITask> queue = new TaskQueue<ITask>(100, false);

        foreach (var task in tasks)
        {
            queue.Accept(task);
        }

        if (addMaskAtEnd)
        {
            queue.Accept(new Act(OpenMaskWithoutOperateRegion));
        }

        queue.Accept(new Act(DoWaitAction, PrepareNextGuide));
        return queue;
    }


    public class FingerLocation
    {
        public float mScale = 1f;
        public Vector2 mOffset = Vector2.zero;
    }
}


public class TM_DelayEvent : CEvent
{
    public Action onOverTime;

    public override void DoOverTime()
    {
        if (onOverTime != null)
            onOverTime();
    }
}


public class TM_UpdateEvent : CEvent
{
    public Func<float, bool> update;

    public override bool needLowUpdate()
    {
        return true;
    }

    public override bool _DoEvent()
    {
        if (update != null)
        {
            StartUpdate();
            return true;
        }

        return false;
    }

    public override bool Update(float secondTime)
    {
        return update(secondTime);
    }
}