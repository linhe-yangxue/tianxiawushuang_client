using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;
using DataTable;
using Logic;


public enum UIArrangement
{ 
    Horizontal = 0,
    Vertical = 1,
}


public class MaskRoutine : Routine 
{
    public MaskRoutine()
    {
        Bind(DoMask());
    }

    public MaskRoutine(GameObject target, string tip, bool showCursor = true)
    {
        Bind(DoMask(target, tip, showCursor));
    }

    public MaskRoutine(Camera camera, Vector3 worldPoint, float width, float height, string tip, bool showCursor = true)
    {
        float w = GuideKit.GetLocalWidth();
        float h = GuideKit.GetLocalHeight();
        Vector3 viewportCoord = camera.WorldToViewportPoint(worldPoint);
        Vector2 center = new Vector2(w * (viewportCoord.x - 0.5f), h * (viewportCoord.y - 0.5f));
        Bind(DoMask(center, width, height, tip, showCursor));
    }

    public IEnumerator DoMask()
    {
        bool clicked = false;
        GuideKit.OpenMask(() => clicked = true);
        yield return new WaitUntil(() => clicked);
        GuideKit.CloseMask();
    }

    public IEnumerator DoMask(Vector2 center, float width, float height, string tip, bool showCursor)
    {
        bool clicked = false;
        GuideKit.OpenMask(center, width, height, () => clicked = true);

        if (/*!GuideMaskWindow.isLocked*/showCursor)
            GuideKit.OpenCursor(center, 1f);

        if (!string.IsNullOrEmpty(tip))
            GuideKit.OpenTip(center, tip);

        yield return new WaitUntil(() => clicked);

        if(showCursor)
            GuideKit.CloseCursor();

        if (!string.IsNullOrEmpty(tip))
            GuideKit.CloseTip();

        GuideKit.CloseMask();
    }

    private IEnumerator DoMask(GameObject target, string tip, bool showCursor)
    {
        bool clicked = false;
        GuideKit.OpenMask(target, () => clicked = true);

        if (target != null && showCursor)
            GuideKit.OpenCursor(target, 1f);

        if (!string.IsNullOrEmpty(tip))
            GuideKit.OpenTip(target, tip);

        yield return new WaitUntil(() => clicked);

        if(showCursor)
            GuideKit.CloseCursor();

        if (!string.IsNullOrEmpty(tip))
            GuideKit.CloseTip();

        GuideKit.CloseMask();
    }

    protected override void OnBreak()
    {
        GuideKit.CloseTip();
        GuideKit.CloseCursor();
        GuideKit.CloseMask();
    }
}


public class ButtonMaskRoutine : Routine 
{
    public GameObject button { get; private set; }
    public string tip { get; private set; }
    public bool autoCallback { get; private set; }
    public bool showCursor { get; private set; }

    public ButtonMaskRoutine(GameObject button, string tip)
        : this(button, tip, true, true)
    { }

    public ButtonMaskRoutine(string[] buttonPath, string tip)
    {
        this.button = GameCommon.FindUI(buttonPath);
        this.tip = tip;
        this.autoCallback = true;
        this.showCursor = true;
        Bind(DoButtonGuide());
    }

    public ButtonMaskRoutine(GameObject button, string tip, bool showCursor, bool autoCallback)
    {
        this.button = button;
        this.tip = tip;
        this.autoCallback = autoCallback;
        this.showCursor = showCursor;
        Bind(DoButtonGuide());
    }
    
    private IEnumerator DoButtonGuide()
    {
        yield return new MaskRoutine(button, tip, showCursor);

        if (autoCallback)
        {
            button.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
        }
    }

    public void Callback()
    {
        if (!autoCallback)
        {
            button.SendMessage("OnClick", SendMessageOptions.RequireReceiver);
        }
    }
}


public class DialogRoutine : Routine 
{
    public DialogRoutine(int startIndex)
    {
        Bind(DoDialog(startIndex));
    }

    private IEnumerator DoDialog(int startIndex)
    {
        GlobalModule.DontCheckClick(this);
        int current = startIndex;

        while (current > 0)
        {
            bool clicked = false;
            GuideKit.OpenDialog(current, () => clicked = true);
            yield return new WaitUntil(() => clicked);
            current = GuideKit.GetNextDialogIndex(current);
        }

        GuideKit.CloseDialog();
        GlobalModule.ReleaseCheckClickFlag(this);
    }

    protected override void OnBreak()
    {
        GuideKit.CloseDialog();
        GlobalModule.ReleaseCheckClickFlag(this);
    }
}


public class ListenNetResp : Routine
{
    public string targetName { get; private set; }
    public bool autoCallback { get; private set; }
    public HttpModule.CallBack callback { get; private set; }
    public string callbackParam { get; private set; }

    private bool listenDone = false;

    public ListenNetResp(string targetName)
        : this(targetName, true)
    { }

    public ListenNetResp(string targetName, bool autoCallback)
    {
        this.targetName = targetName;
        this.autoCallback = autoCallback;
        this.callback = null;
        this.callbackParam = "";

        Bind(DoWaitNetResp());
    }

    private IEnumerator DoWaitNetResp()
    {
        HttpModule.Instace.mRespListener = OnListen;
        yield return new WaitUntil(() => listenDone);

        if (autoCallback && callback != null)
        {
            callback(callbackParam);
        }
    }

    private bool OnListen(string name, HttpModule.CallBack call, string param)
    {
        if (name == targetName)
        {
            callback = call;
            callbackParam = param;
            listenDone = true;
            return true;
        }
        else
        {
            call(param);
            return false;
        }
    }

    protected override void OnBreak()
    {
        HttpModule.Instace.mRespListener = null;
    }

    public void Callback()
    {
        if (!autoCallback && callback != null)
        {
            callback(callbackParam);
        }
    }
}


public class SaveProgressRoutine : Routine
{
    public SaveProgressRoutine(int progress, bool isNeedResponse = true)
    {
        Bind(DoSaveProgress(progress, isNeedResponse));
    }

    private IEnumerator DoSaveProgress(int progress, bool isNeedResponse)
    {
        GuideKit.SaveProgressByLocal(progress);
        yield return new SaveProgressByServerRoutine(progress, isNeedResponse);
    }
}


public class SaveProgressByServerRoutine : Routine 
{
    public SaveProgressByServerRoutine(int progress, bool isNeedResponse = true)
    {
        Bind(DoSaveProgressByServer(progress, isNeedResponse));
    }

    private IEnumerator DoSaveProgressByServer(int progress, bool isNeedResponse)
    {
        yield return new WaitUntil(() => Net.msUIWaitEffect == null || Net.msUIWaitEffect.waiting == 0);
        GuideKit.SaveProgressByServer(progress, () => /*RoleLogicData.Self.guideProgress*/SetServerProgress(progress), null, isNeedResponse);

        if (isNeedResponse)
        {
            yield return new ListenNetResp("SC_SaveGuideProgress", true);
        }
    }

    private void SetServerProgress(int progress)
    {
        if (Guide.serverIndex == Guide.MAX_INDEX || Guide.serverIndex < progress)
        {
            Guide.serverIndex = progress;
            Guide.serverReloadIndex = DataCenter.mBeginnerConfig.GetData(progress, "RELOAD_INDEX");
        }
    }
}


public class GetProgressRoutine : Routine 
{
    public int progress { get; private set; }

    public GetProgressRoutine()
    {
        Bind(DoGetProgress());
    }

    private IEnumerator DoGetProgress()
    {      
        yield return new WaitUntil(() => Net.msUIWaitEffect == null || Net.msUIWaitEffect.waiting == 0);
        string text = null;
        GuideKit.GetProgressByServer(x => text = x, null);
        yield return new WaitUntil(() => text != null);
        var resp = JCode.Decode<SC_QueryGuideProgress>(text);
        progress = resp.resGuideProgress;
    }
}


public class MoveCameraRoutine : Routine
{
    public MoveCameraRoutine(Vector3 targetPos, float duration)
    {
        Bind(DoMoveCamera(targetPos, duration));
    }

    private IEnumerator DoMoveCamera(Vector3 targetPos, float duration)
    {
        if (MainProcess.mCameraMoveTool != null)
        {
            //MainProcess.mCameraMoveTool.Finish();
            MainProcess.mCameraMoveTool._MoveByObject(null);
        }

        float elapsed = 0f;
        Vector3 from = MainProcess.mCameraObject.transform.position;

        while (elapsed < duration)
        {
            MainProcess.mCameraObject.transform.position = LerpPosition(from, targetPos, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
        }

        MainProcess.mCameraObject.transform.position = targetPos;
    }

    private Vector3 LerpPosition(Vector3 from, Vector3 to, float factor)
    {
        return Vector3.Lerp(from, to, factor);
    }
}


public class WaitUIVisible : Routine 
{
    public GameObject target { get; private set; }
    public float rate { get; set; }
    public float delay { get; set; }

    public WaitUIVisible(params string[] path)
        : this(0f, 0.15f, path)
    { }

    public WaitUIVisible(UIArrangement sortArrangement, int index, params string[] path)
        : this(0f, 0.15f, sortArrangement, index, path)
    { }

    public WaitUIVisible(float delay, float rate, params string[] path)
    {
        this.delay = delay;
        this.rate = rate;
        Bind(DoWaitUIVisible(path));
    }

    public WaitUIVisible(float delay, float rate, UIArrangement sortArrangement, int index, params string[] path)
    {
        this.delay = delay;
        this.rate = rate;
        Bind(DoWaitUIVisible(sortArrangement, index, path));
    }

    private IEnumerator DoWaitUIVisible(string[] path)
    {
        if (delay > 0.001f)
        {
            yield return new Delay(delay);
        }

        target = FindTarget(path);

        while (target == null || !target.activeInHierarchy)
        {
            yield return rate > 0.001f ? new Delay(rate) : null;

            if (target == null)
            {
                target = FindTarget(path);
            }
        }
    }

    private IEnumerator DoWaitUIVisible(UIArrangement sortArrangement, int index, string[] path)
    {
        if (delay > 0.001f)
        {
            yield return new Delay(delay);
        }

        target = FindTarget(sortArrangement, index, path);

        while (target == null || !target.activeInHierarchy)
        {
            yield return new Delay(rate);

            if (target == null)
            {
                target = FindTarget(sortArrangement, index, path);
            }
        }
    }

    private GameObject FindTarget(string[] path)
    {
        return GameCommon.FindUI(path);
    }

    private GameObject FindTarget(UIArrangement sortArrangement, int index, string[] path)
    {
        if (path.Length == 0)
            return null;

        GameObject target = GameCommon.FindUI(path);
        string targetName = path[path.Length - 1];

        if (target == null || target.transform.parent == null || !target.transform.parent.gameObject.activeInHierarchy)
            return null;

        List<Transform> targets = new List<Transform>();

        foreach (Transform child in target.transform.parent)
        {
            if (child.gameObject.activeSelf && child.name == targetName)
            {
                bool sorted = false;

                for (int i = 0; i < targets.Count; ++i)
                {
                    float childPos = sortArrangement == UIArrangement.Horizontal ? child.position.x : -child.position.y;
                    float targetPos = sortArrangement == UIArrangement.Horizontal ? targets[i].position.x : -targets[i].position.y;

                    if (childPos <= targetPos)
                    {
                        targets.Insert(i, child);
                        sorted = true;
                        break;
                    }
                }

                if (!sorted)
                {
                    targets.Add(child);
                }
            }
        }

        if (index >= 0 && index < targets.Count)
        {
            return targets[index].gameObject;
        }

        return null;
    }
}


public class SyncGuideProgress : Routine 
{
    public bool isNewPlayer { get; private set; }
    public int finalIndex { get; private set; }

    public SyncGuideProgress()
    {
        isNewPlayer = false;
        finalIndex = 0;
        Bind(DoSyncGuideProgress());
    }

    private IEnumerator DoSyncGuideProgress()
    {
        int local = GuideKit.GetProgressByLocal();
        //int server = GuideKit.GetProgressByServer();
        var getRoutine = new GetProgressRoutine();
        yield return getRoutine;
        int server = getRoutine.progress;
        
        DEBUG.Log("Guide Local Progress = " + local);
        DEBUG.Log("Guide Server Progress = " + server);

        if (local == server && local == 0)
        {
            isNewPlayer = true;
            yield break;
        }

        finalIndex = local;

        if (server > local)
        {
            finalIndex = server;
            GuideKit.SaveProgressByLocal(finalIndex);
        }
        else if (local > server)
        {
            finalIndex = local;
            yield return new SaveProgressByServerRoutine(finalIndex);
        }
    }
}


//public class ClickGroundGuide : Routine
//{
//    private Vector3 center;
//    private float width = 100f;
//    private float height = 100f;
//    private string tip = "";

//    public ClickGroundGuide(Vector3 worldPos, float normalizedWidth, float normlizedHeight, string tip)
//    {
//        this.center = worldPos;
//        this.width = normalizedWidth;
//        this.height = normlizedHeight;
//        this.tip = tip;
//        Bind(DoClickGroundGuide());
//    }

//    private IEnumerator DoClickGroundGuide()
//    {
//        yield return new MaskRoutine(MainProcess.mMainCamera, center, width, height, tip);
//        RaycastHit hit;

//        if (GuideKit.RaycastToObstruct(center, out hit))
//        {
//            GuideKit.SetAllFriendsAI(x => new FollowRoutine(x, Character.Self, x.mFollowOffset, false));
//            yield return new MoveToRoutine(Character.Self, hit.point);
//            GuideKit.SetAllFriendsAI(null);
//        }
//    }
//}


public class WaitEnemyVisible : RoutineBlock 
{
    private int enemyIndex = 0;

    public WaitEnemyVisible(int enemyIndex)
    {
        this.enemyIndex = enemyIndex;
    }

    protected override bool OnBlock()
    {
        if (MainProcess.mStage == null || Character.Self == null)
        {
            return true;
        }

        return ObjectManager.Self.FindAlived(x => !Character.Self.IsSameCamp(x) && AIKit.InBounds(Character.Self, x, Character.Self.LookBounds() + 0.5f) && (enemyIndex == 0 || x.mConfigIndex == enemyIndex)) == null;
    }
}


public class WaitEnemyDoSkill : RoutineBlock
{
    public int enemyIndex { get; private set; }
    public int skillIndex { get; private set; }
    public BaseObject enemy { get; private set; }
    public Skill skill { get; private set; }

    public WaitEnemyDoSkill(int enemyIndex, int skillIndex)
    {
        this.enemyIndex = enemyIndex;
        this.skillIndex = skillIndex;
    }

    protected override bool OnBlock()
    {
        if (MainProcess.mStage == null || Character.Self == null)
        {
            return true;
        }

        var objMap = ObjectManager.Self.mObjectMap;
        var len = objMap.Length;

        for (int i = 0; i < len; ++i)
        {
            var obj = objMap[i];

            if (obj != null
                && !obj.IsDead()
                && !Character.Self.IsSameCamp(obj)
                && AIKit.InBounds(Character.Self, obj, Character.Self.LookBounds() + 0.5f)
                && (enemyIndex == 0 || obj.mConfigIndex == enemyIndex))
            {
                skill = obj.aiMachine.currentSkill;

                if (skill != null && (skill.mConfigIndex == skillIndex || (skillIndex == 0 && !skill.isNormalAttack)))
                {
                    enemy = obj;
                    return true;
                }
            }
        }

        return false;
    }
}


public class PrologueRoutine : Routine
{
    public PrologueRoutine()
    {
        Bind(DoPrologueBattle());
    }

    private IEnumerator DoPrologueBattle()
    {
        int entry = 0;

        foreach (var pair in DataCenter.mBeginnerConfig.GetAllRecord())
        {
            if ((int)pair.Value["TRIGGER_TYPE"] == (int)GUIDE_TRIGGER_TYPE.PROLOGUE_BATTLE)
            {
                entry = pair.Key;
                break;
            }
        }

        GuideData d = new GuideData(entry);
        ConfigParam param = d.valid ? d.triggerParam : new ConfigParam();

        int stageId = param.GetInt("stage", 20001);
        int roleId = param.GetInt("role", 50001);
        int[] petId = param.GetIntArray("pet");
        Guide.InitPrologueData(roleId, petId);
        DataCenter.Set("CURRENT_STAGE", stageId);

        GuideKit.OpenBaseMask();
        MainProcess.mStage = null;
        GameLoadingUI.lockLoading = true;
        LoadBattleScene();

        yield return new WaitUntil(() => MainProcess.mStage != null);
        PVEStageBattle pve = MainProcess.mStage as PVEStageBattle;
        pve.mManualActivate = true;
        pve.mManualAccountOnFinish = true;
        pve.mTeamInitAngle = param.GetInt("dir");
        Vector3 backDir = Quaternion.Euler(0f, pve.mTeamInitAngle, 0f) * Vector3.forward;

        if (Character.Self != null)
        {
            Character.Self.SetRealDirection(Character.Self.GetPosition() - backDir);

            for (int i = 0; i < Character.msFriendsCount; ++i)
            {
                var f = Character.Self.mFriends[i];

                if (f != null && !f.IsDead())
                {
                    f.SetRealDirection(f.GetPosition() - backDir);
                }
            }
        }

        yield return new WaitUntil(() => MainProcess.mStage.mbBattleStart);
        GameLoadingUI.lockLoading = false;

        GuideKit.SetPVEBattleSettingEnabled(false);
        GuideKit.CloseBaseMask();

        Guide.currentGuide = new GuideProcess(entry);
        yield return Guide.currentGuide;
        Guide.currentGuide = null;
        GlobalModule.ReleaseCheckClickFlag(MainProcess.Self);

        Time.timeScale = 1f;
        GuideKit.CloseBaseMask();
        TeamManager.mDicTeamPosData.Clear();
        MainProcess.ClearBattle();
        GlobalModule.ClearAllWindow(new string[] { "prologue_window" }, true);
        //by chenliang
        //begin

        GameCommon.ResetWorldCameraColor();

        //end
        DataCenter.OpenWindow("PROLOGUE_LOADING_WINDOW", null);
    }

    private void LoadBattleScene()
    {
        DataCenter.CloseWindow("REGISTRATION_WINDOW");
        DataCenter.Set("ENTER_GS", false);
        DataCenter.CloseWindow("LOGIN_WINDOW");
        DataCenter.SetData("LANDING_WINDOW", "REFRESH", true);
        DataCenter.CloseWindow("LANDING_WINDOW");
        DataCenter.SetData("SelectRoleUI", "CLOSE", true);

        MainProcess.LoadBattleLoadingScene();
    }

    protected override void OnBreak()
    {
        CGTools.ClearCG();
        Guide.currentGuide = null;
        GuideKit.CloseBaseMask();
        TeamManager.mDicTeamPosData.Clear();
        GlobalModule.ReleaseCheckClickFlag(MainProcess.Self);
    }

    protected override void OnException(Exception e)
    {
        MainProcess.ClearBattle();
        GlobalModule.ClearAllWindow(new string[] { "prologue_window" }, true);
        DataCenter.OpenWindow("PROLOGUE_LOADING_WINDOW", null);
    }
}


//public class WaitMainUILoadingDone : Routine
//{
//    public WaitMainUILoadingDone()
//    {
//        Bind(DoWaitMainUILoadingDone());
//    }

//    private IEnumerator DoWaitMainUILoadingDone()
//    {
//        bool loadOk = false;
//        MainUIScript.mLoadingFinishAction = () => loadOk = true;
//        yield return new WaitFor(() => loadOk);
//    }
//}


//public class WaitBattleActive : YieldRoutine
//{
//    protected override IEnumerator DoYield()
//    {
//        yield return new WaitFor(() => MainProcess.mStage != null && MainProcess.mStage.mbBattleStart);
//        GuideKit.OpenMask();
//        yield return new WaitFor(() => MainProcess.mStage.mbBattleActive);
//    }
//}


//public class WaitNetRespThenSave : Routine 
//{
//    public WaitNetRespThenSave(string respName, int index)
//    {
//        Bind(DoWaitNetRespThenSave(respName, index));
//    }

//    private IEnumerator DoWaitNetRespThenSave(string respName, int index)
//    {
//        var waiter = new ListenNetResp(respName, false);
//        yield return waiter;
//        yield return new SaveProgressRoutine(index);
//        waiter.Callback();
//    }
//}


//public class GuideNodeRoutine : Routine 
//{
//    public int index { get; private set; }
//    public int preDialogIndex { get; private set; }
//    public int postDialogIndex { get; private set; }
//    public float preDialogDelay { get; private set; }
//    public float postDialogDelay { get; private set; }
//    public int nextIndex { get; protected set; }

//    private IEnumerator preDelay = null;
//    private IEnumerator postDelay = null;

//    public GuideNodeRoutine()
//    {          
//        Bind(DoGuideNode());
//    }

//    public virtual void Init(int index)
//    {
//        this.index = index;
//        this.nextIndex = index + 1;
//        DataRecord r = DataCenter.mBeginnerConfig.GetRecord(index);

//        if (r != null)
//        {
//            preDialogIndex = r["DIALOGUE_BEFOR"];
//            postDialogIndex = r["DIALOGUE_AFTER"];
//            preDialogDelay = (int)r["DIALOGUE_BEFOR_DELAY"];
//            postDialogDelay = (int)r["DIALOGUE_AFTER_DELAY"];
//        }
//    }

//    public GuideNodeRoutine PreDelay(IEnumerator delay)
//    {
//        this.preDelay = delay;
//        return this;
//    }

//    public GuideNodeRoutine PostDelay(IEnumerator delay)
//    {
//        this.postDelay = delay;
//        return this;
//    }

//    private IEnumerator DoGuideNode()
//    {
//        yield return DoBefore();

//        if (preDelay != null)
//        {
//            yield return preDelay;
//        }

//        yield return CastPreDialog(0f);

//        yield return DoMain();

//        if (postDelay != null)
//        {
//            yield return postDelay;
//        }

//        yield return CastPostDialog(0f);

//        yield return DoAfter();
//    }

//    protected virtual IEnumerator DoBefore() { yield break; }

//    protected virtual IEnumerator DoMain() { yield break; }

//    protected virtual IEnumerator DoAfter() { yield break; }

//    public virtual bool VerifyOnLoad() { return true; }

//    public virtual IEnumerator DoPrepareOnLoad() { yield return new WaitMainUILoadingDone(); }

//    protected IEnumerator CastPreDialog(float delay)
//    {
//        if (preDialogDelay > 0.001f)
//        {
//            yield return new Wait(preDialogDelay);
//        }

//        if (preDialogIndex > 0)
//        {           
//            yield return new DialogRoutine(preDialogIndex);
//        }
//    }

//    protected IEnumerator CastPostDialog(float delay)    
//    {
//        if (postDialogDelay > 0.001f)
//        {
//            yield return new Wait(postDialogDelay);
//        }

//        if (postDialogIndex > 0)
//        {           
//            yield return new DialogRoutine(postDialogIndex);
//        }
//    }
//}