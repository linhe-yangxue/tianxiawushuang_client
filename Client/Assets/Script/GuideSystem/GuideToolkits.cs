using UnityEngine;
using System;
using System.Collections.Generic;
using DataTable;
using Utilities.Routines;


public class GuideKit
{
    /// <summary>
    /// 打开基础遮罩，屏蔽用户操作
    /// </summary>
    public static void OpenBaseMask()
    {
        DataCenter.OpenWindow("GUIDE_BASE_MASK_WINDOW");
    }

    /// <summary>
    /// 关闭基础遮罩
    /// </summary>
    public static void CloseBaseMask()
    {
        DataCenter.CloseWindow("GUIDE_BASE_MASK_WINDOW");
    }

    ///// <summary>
    ///// 打开一个遮罩，不可点击
    ///// </summary>
    //public static void OpenMask()
    //{
    //    GlobalModule.CancleOnNextLateUpdate(CloseMaskImmediate);
    //    DataCenter.OpenWindow("GUIDE_MASK_WINDOW");
    //    DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_SIZE", Vector2.zero);
    //}

    /// <summary>
    /// 打开一个可以全屏点击的遮罩
    /// </summary>
    /// <param name="onClike"> 点击后的回调 </param>
    public static void OpenMask(Action onClick)
    {
        GlobalModule.CancleOnNextLateUpdate(CloseMaskImmediate);
        DataCenter.OpenWindow("GUIDE_MASK_WINDOW");
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_ACTION", onClick);
    }

    /// <summary>
    /// 打开一个具有和指定GameObject的Collider区域相同的点击区域的遮罩
    /// </summary>
    /// <param name="button"> 指定的GameObject，若为null则无点击区域 </param>
    /// <param name="onClick"> 点击后的回调 </param>
    public static void OpenMask(GameObject button, Action onClick)
    {
        GlobalModule.CancleOnNextLateUpdate(CloseMaskImmediate);
        DataCenter.OpenWindow("GUIDE_MASK_WINDOW");
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_BUTTON_SCALE", maskButtonScale);
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_BUTTON", button);
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_ACTION", onClick);
        maskButtonScale = 1f;
    }

    /// <summary>
    /// 打开一个具有指定尺寸的点击区域的遮罩
    /// </summary>
    /// <param name="center"> 点击区域的中心 </param>
    /// <param name="width"> 点击区域的宽 </param>
    /// <param name="height"> 点击区域的高 </param>
    /// <param name="onClick"> 点击后的回调 </param>
    public static void OpenMask(Vector2 center, float width, float height, Action onClick)
    {
        GlobalModule.CancleOnNextLateUpdate(CloseMaskImmediate);
        DataCenter.OpenWindow("GUIDE_MASK_WINDOW");
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_CENTER", center);
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_SIZE", new Vector2(width, height));
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_ACTION", onClick);
    }

    /// <summary>
    /// 重置遮罩，不可点击
    /// </summary>
    public static void ResetMask()
    {
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_SIZE", Vector2.zero);
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_ACTION", null);
    }

    /// <summary>
    /// 立即关闭遮罩
    /// </summary>
    public static void CloseMaskImmediate()
    {
        DataCenter.CloseWindow("GUIDE_MASK_WINDOW");
    }

    /// <summary>
    /// 延迟至下一帧的LaterUpdate中关闭遮罩，但点击回调会被立即取消
    /// 若在此期间再次打开遮罩，则本次调用效果终结
    /// </summary>
    public static void CloseMask()
    {
        DataCenter.SetData("GUIDE_MASK_WINDOW", "SET_ACTION", null);
        GlobalModule.DoOnNextLateUpdate(CloseMaskImmediate);
    }

    private static float maskButtonScale = 1f;

    /// <summary>
    /// 设置遮罩触发区域的缩放，不会影响当前已打开的遮罩，在下次打开遮罩后会重置缩放
    /// </summary>
    /// <param name="scale"> 缩放值 </param>
    public static void SetMaskButtonScale(float scale)
    {
        maskButtonScale = scale;
    }

    private static GameObject mCursor = null;
    private static Vector3 cursorOffset = Vector3.zero;
    private static float cursorScale = 1f;
    private static Vector3 cursorAngle = Vector3.zero;

    /// <summary>
    /// 显示指示光标
    /// </summary>
    /// <param name="center"> 光标位置 </param>
    /// <param name="scale"> 光标大小 </param>
    public static void OpenCursor(Vector3 center, float scale)
    {
        GlobalModule.CancleOnNextLateUpdate(CloseCursorImmediate);

        if (mCursor == null)
        {      
            mCursor = GameCommon.LoadAndIntanciatePrefabs("Prefabs/Finger");        
        }

        if (mCursor.transform.parent == null || mCursor.transform.parent.name != "CenterAnchor")
        {
            GameObject centerAnchor = GameCommon.FindUI("CenterAnchor");
            mCursor.transform.parent = centerAnchor.transform;
        }

        mCursor.SetActive(true);
        mCursor.transform.localPosition = center + cursorOffset;
        mCursor.transform.localScale = Vector3.one * cursorScale;
        mCursor.transform.Find("ui_shouzhi").localEulerAngles = cursorAngle;

        cursorOffset = Vector3.zero;
        cursorScale = 1f;
        cursorAngle = Vector3.zero;
    }

    /// <summary>
    /// 显示指示光标
    /// </summary>
    /// <param name="point"> 光标所在点 </param>
    /// <param name="scale"> 光标大小 </param>
    public static void OpenCursor(GameObject point, float scale)
    {
        GlobalModule.CancleOnNextLateUpdate(CloseCursorImmediate);

        if (mCursor == null)
        {
            mCursor = GameCommon.LoadAndIntanciatePrefabs("Prefabs/Finger");
        }

        mCursor.SetActive(true);
        mCursor.transform.parent = point.transform;
        mCursor.transform.localPosition = cursorOffset;
        mCursor.transform.localScale = Vector3.one * cursorScale;
        mCursor.transform.Find("ui_shouzhi").localEulerAngles = cursorAngle;

        cursorOffset = Vector3.zero;
        cursorScale = 1f;
        cursorAngle = Vector3.zero;
    }

    /// <summary>
    /// 延迟至下一帧的LaterUpdate中销毁光标
    /// 若在此期间再次打开光标，则本次调用效果终结
    /// </summary>
    public static void CloseCursor()
    {
        GlobalModule.DoOnNextLateUpdate(CloseCursorImmediate);
    }

    /// <summary>
    /// 立即销毁光标
    /// </summary>
    public static void CloseCursorImmediate()
    {
        if (mCursor != null)
        {
            MonoBehaviour.DestroyImmediate(mCursor);
            mCursor = null;
        }      
    }  

    /// <summary>
    /// 设置光标的偏移，不会影响当前已打开的光标，在下次OpenCursor后会重置偏移值
    /// </summary>
    /// <param name="offset"> 偏移量 </param>
    public static void SetCursorOffset(float x, float y)
    {
        cursorOffset = new Vector3(x, y, 0);
    }

    /// <summary>
    /// 设置光标的缩放，不会影响当前已打开的光标，在下次OpenCursor后会重置缩放
    /// </summary>
    /// <param name="scale"> 缩放值 </param>
    public static void SetCursorScale(float scale)
    {
        cursorScale = scale;
    }

    /// <summary>
    /// 设置光标的旋转角度，不会影响当前已打开的光标，在下次OpenCursor后会重置旋转角度
    /// </summary>
    /// <param name="angle"> 旋转角度 </param>
    public static void SetCursorRotate(Vector3 angle)
    {
        cursorAngle = angle;
    }

    private static GameObject mTip = null;
    private static Vector3 tipOffset = Vector3.zero;

    /// <summary>
    /// 显示提示框
    /// </summary>
    /// <param name="center"> 提示框位置 </param>
    /// <param name="text"> 提示框内容 </param>
    public static void OpenTip(Vector3 center, string text)
    {
        GlobalModule.CancleOnNextLateUpdate(CloseTipImmediate);

        if (mTip == null)
        {
            mTip = GameCommon.LoadAndIntanciatePrefabs("Prefabs/GuideTip");
        }

        if (mTip.transform.parent == null || mTip.transform.parent.name != "CenterAnchor")
        {
            GameObject centerAnchor = GameCommon.FindUI("CenterAnchor");
            mTip.transform.parent = centerAnchor.transform;
        }

        mTip.SetActive(true);
        mTip.transform.localPosition = center + tipOffset;
        mTip.transform.localScale = Vector3.one;
        UIWidget widget = GameCommon.FindComponent<UIWidget>(mTip, "background");
        UILabel label = GameCommon.FindComponent<UILabel>(widget.gameObject, "label");
        label.text = text;
        int textWidth = text.Length <= 7 ? label.width * text.Length / 7 : label.width;
        widget.width = textWidth + 20;
        widget.height = label.height + 20;
        mTip.GetComponent<UIPanel>().alpha = 0f;
        TweenAlpha.Begin(mTip, 0.3f, 1f);
        tipOffset = Vector3.zero;
    }

    /// <summary>
    /// 显示提示框
    /// </summary>
    /// <param name="point"> 提示框所在点 </param>
    /// <param name="text"> 提示框内容 </param>
    public static void OpenTip(GameObject point, string text)
    {
        GlobalModule.CancleOnNextLateUpdate(CloseTipImmediate);

        if (mTip == null)
        {
            mTip = GameCommon.LoadAndIntanciatePrefabs("Prefabs/GuideTip");
        }

        mTip.SetActive(true);
        mTip.transform.parent = point.transform;
        mTip.transform.localPosition = tipOffset;
        mTip.transform.localScale = Vector3.one;
        UIWidget widget = GameCommon.FindComponent<UIWidget>(mTip, "background");
        UILabel label = GameCommon.FindComponent<UILabel>(widget.gameObject, "label");
        label.text = text;
        int textWidth = text.Length <= 7 ? label.width * text.Length / 7 : label.width;
        widget.width = textWidth + 20;
        widget.height = label.height + 20;
        mTip.GetComponent<UIPanel>().alpha = 0f;
        TweenAlpha.Begin(mTip, 0.3f, 1f);
        tipOffset = Vector3.zero;
    }

    /// <summary>
    /// 延迟至下一帧的LaterUpdate中销毁提示框
    /// 若在此期间再次打开提示框，则本次调用效果终结
    /// </summary>
    public static void CloseTip()
    {
        GlobalModule.DoOnNextLateUpdate(CloseTipImmediate);
    }

    /// <summary>
    /// 立即销毁提示框
    /// </summary>
    public static void CloseTipImmediate()
    {
        if (mTip != null)
        {
            MonoBehaviour.DestroyImmediate(mTip);
            mTip = null;
        }
    }

    /// <summary>
    /// 设置提示框的偏移，不会影响当前已打开的提示框，在下次OpenTip后会重置偏移值
    /// </summary>
    /// <param name="offset"> 偏移量 </param>
    public static void SetTipOffset(float x, float y)
    {
        tipOffset = new Vector3(x, y, 0);
    }

    /// <summary>
    /// 打开对话窗口
    /// </summary>
    /// <param name="index"> 对话索引 </param>
    /// <param name="onClick"> 点击后的回调 </param>
    public static void OpenDialog(int index, Action onClick)
    {
        GlobalModule.CancleOnNextLateUpdate(CloseDialogImmediate);

        DataCenter.OpenWindow("GUIDE_DIALOG_WINDOW", index);
        DataCenter.SetData("GUIDE_DIALOG_WINDOW", "SET_ACTION", onClick);
    }

    /// <summary>
    /// 立即关闭对话窗口
    /// </summary>
    public static void CloseDialogImmediate()
    {
        DataCenter.CloseWindow("GUIDE_DIALOG_WINDOW");
    }

    /// <summary>
    /// 延迟至下一帧的LaterUpdate中关闭对话窗口，但点击回调会被立即取消
    /// 若在此期间再次打开对话窗口，则本次调用效果终结
    /// </summary>
    public static void CloseDialog()
    {
        DataCenter.SetData("GUIDE_DIALOG_WINDOW", "SET_ACTION", null);
        GlobalModule.DoOnNextLateUpdate(CloseDialogImmediate);
    }

    /// <summary>
    /// 获取下一条对话的索引
    /// </summary>
    /// <param name="currentIndex"> 当前对话索引 </param>
    /// <returns> 下一条对话的索引，若不存在返回0 </returns>
    public static int GetNextDialogIndex(int currentIndex)
    {
        return DataCenter.mDialog.GetData(currentIndex, "NEXT");
    }

    /// <summary>
    /// 打开序章窗口
    /// </summary>
    /// <param name="param"> 窗口参数 </param>
    public static void OpenPrologueWindow(PrologueWindowParam param, Action onClick)
    {
        GlobalModule.CancleOnNextLateUpdate(FadeOutPrologueWindowImmediate);

        DataCenter.OpenWindow("PROLOGUE_WINDOW", param);
        DataCenter.SetData("PROLOGUE_WINDOW", "SET_ACTION", onClick);
    }

    /// <summary>
    /// 延迟至下一帧的LaterUpdate中淡出序章窗口，但点击回调会被立即取消
    /// 若在此期间再次打开序章窗口，则本次调用效果终结
    /// </summary>
    public static void FadeOutPrologueWindow()
    {
        DataCenter.SetData("PROLOGUE_WINDOW", "SET_ACTION", null);
        GlobalModule.DoOnNextLateUpdate(FadeOutPrologueWindowImmediate);
    }

    private static void FadeOutPrologueWindowImmediate()
    {
        DataCenter.SetData("PROLOGUE_WINDOW", "FADE_OUT", true);
    }

    /// <summary>
    /// 设置序章图片的淡入淡出时间
    /// </summary>
    /// <param name="time"> 淡入淡出时间 </param>
    public static void SetProloguePictureFadeTime(float time)
    {
        PrologueWindow.fadeTime = time;
    }

    /// <summary>
    /// 立即关闭序章图片
    /// </summary>
    public static void ClosePrologueWindowImmediate()
    {
        GlobalModule.CancleOnNextLateUpdate(FadeOutPrologueWindowImmediate);
        DataCenter.CloseWindow("PROLOGUE_WINDOW");
    }

    /// <summary>
    /// 在本地保持引导进度
    /// </summary>
    /// <param name="progress"> 进度索引 </param>
    public static void SaveProgressByLocal(int progress)
    {
        string key = "GUIDE_PROGRESS/" + CommonParam.mUId;
        PlayerPrefs.SetInt(key, progress);
    }

    /// <summary>
    /// 通过服务器保持引导进度
    /// </summary>
    /// <param name="progress"> 进度索引 /param>
    /// <param name="onSuccess"> 成功回调 </param>
    /// <param name="onFail"> 失败回调 </param>
    public static void SaveProgressByServer(int progress, Action onSuccess, Action onFail, bool isNeedWaitEffect = true)
    {
        DataRecord r = DataCenter.mBeginnerConfig.GetRecord(progress);
        CS_SaveGuideProgress msg = new CS_SaveGuideProgress();
        msg.guideProgress = progress;
        msg.name = r == null ? "" : r["DESCRIBE"];
        
        HttpModule.Instace.SendGameServerMessage(msg, "CS_SaveGuideProgress",
            x => { if (onSuccess != null) onSuccess(); },
            x => { if (onFail != null) onFail(); },
            isNeedWaitEffect);
    }

    /// <summary>
    /// 读取本地引导进度
    /// </summary>
    /// <returns> 进度 </returns>
    public static int GetProgressByLocal()
    {
        string key = "GUIDE_PROGRESS/" + CommonParam.mUId;
        return PlayerPrefs.GetInt(key, 0);
    }

    /// <summary>
    /// 读取服务器引导进度
    /// 服务器引导进度在登陆的时候获取，缓存在本地
    /// </summary>
    /// <returns> 进度 </returns>
    //public static int GetProgressByServer()
    //{
    //    return Guide.serverProgress;//RoleLogicData.Self.guideProgress;
    //}

    public static void GetProgressByServer(Action<string> onSuccess, Action<string> onFail)
    {
        CS_QueryGuideProgress msg = new CS_QueryGuideProgress();

        HttpModule.Instace.SendGameServerMessage(msg, "CS_QueryGuideProgress",
                   x => { if (onSuccess != null) onSuccess(x); },
                   x => { if (onFail != null) onFail(x); });
    }

    /// <summary>
    /// 创建活动对象，该对象是独立的，不通过ObjectManager进行管理，主要用于模型的显示
    /// </summary>
    /// <param name="index"> 对象索引 </param>
    /// <returns> 活动对象 </returns>
    public static BaseObject CreateModel(int index)
    {
        DataRecord r = ObjectManager.Self.GetObjectConfig(index);

        if (r == null)
            return null;

        ActiveObject obj = new ActiveObject();
        obj.mConfigIndex = index;
        obj.mConfigRecord = r;
        obj.mbIsMainUI = true;
        obj.mbIsUI = true;
        obj.mMainObject = new GameObject("_role_");
        obj.CreateMainObject((int)r.get("MODEL"));
        obj.SetMainObject(obj.mMainObject);
        return obj;
    }

    /// <summary>
    /// 在指定位置显示指定大小的模型
    /// </summary>
    /// <param name="index"> 对象索引 </param>
    /// <param name="uiPoint"> 位置 </param>
    /// <param name="scale"> 缩放 </param>
    /// <returns> 活动对象 </returns>
    public static BaseObject ShowModel(int index, GameObject uiPoint, float scale)
    {
        if (uiPoint == null)
            return null;

        uiPoint.SetActive(false);

        BaseObject obj = CreateModel(index);

        if (obj == null)
            return null;

        float originScale = obj.mConfigRecord.get("UI_SCALE") * 100f;
        obj.mMainObject.transform.parent = uiPoint.transform.parent;
        obj.SetPosition(uiPoint.transform.position);
        obj.mMainObject.transform.localScale = Vector3.one * originScale * scale;
        obj.mMainObject.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        obj.SetVisible(true);
        obj.OnIdle();
        GameCommon.SetLayer(obj.mMainObject, CommonParam.UILayer);
        return obj;
    }

    /// <summary>
    /// 设置模型大小
    /// </summary>
    /// <param name="model"> 活动对象 </param>
    /// <param name="scale"> 缩放 </param>
    public static void SetModelScale(BaseObject model, float scale)
    {
        if (model != null && model.mMainObject != null && model.mConfigRecord != null)
        {
            float originScale = model.mConfigRecord.get("UI_SCALE") * 100f;
            model.mMainObject.transform.localScale = Vector3.one * originScale * scale;
        }
    }

    /// <summary>
    /// 销毁活动对象
    /// </summary>
    /// <param name="obj"> 被销毁的活动对象 </param>
    public static void DestroyModel(BaseObject obj)
    {
        if (obj != null && obj.mMainObject != null)
        {
            GameObject.Destroy(obj.mMainObject);
            obj.mMainObject = null;
        }
    }

    private static Vector3 localScale = Vector3.zero;
    private static int manualHeight = 0;

    /// <summary>
    /// 获取UI界面相对宽度
    /// </summary>
    /// <returns></returns>
    public static float GetLocalWidth()
    {
        return (float)Screen.width / Screen.height * GetManualHeight() / GetLocalScale().x;
    }

    /// <summary>
    /// 获取UI界面相对高度
    /// </summary>
    /// <returns></returns>
    public static float GetLocalHeight()
    {
        return 640f / GetLocalScale().y;
    }

    /// <summary>
    /// 获取UI界面全局缩放
    /// </summary>
    /// <returns> 缩放值 </returns>
    public static Vector3 GetLocalScale()
    {
        if (localScale.sqrMagnitude < 1e-6)
        {
            GameObject center = GameCommon.FindUI("CenterAnchor");

            if (center != null)
            {
                localScale = center.transform.localScale;
                return localScale;
            }

            return Vector3.one;
        }

        return localScale;
    }

    /// <summary>
    /// 获取UI界面指定高度
    /// </summary>
    /// <returns> 高度 </returns>
    public static int GetManualHeight()
    {
        if (manualHeight == 0)
        {
            GameObject root = GameObject.Find(CommonParam.UIRootName);

            if (root != null)
            {
                UIRoot uiRoot = root.GetComponent<UIRoot>();

                if (uiRoot != null)
                {
                    manualHeight = uiRoot.manualHeight;
                    return manualHeight;
                }
            }

            return 640;
        }

        return manualHeight;
    }

    /// <summary>
    /// 通过相机指定屏幕坐标向障碍物投射射线，计算碰撞点
    /// </summary>
    /// <param name="camera"> 相机 </param>
    /// <param name="screenCoord"> 屏幕坐标 </param>
    /// <param name="hit"> 碰撞点 </param>
    /// <returns> 是否有碰撞 </returns>
    public static bool RaycastToObstruct(Camera camera, Vector3 screenCoord, out RaycastHit hit)
    {
        int mask = 1 << CommonParam.ObstructLayer;
        mask = ~mask;
        Ray ray = camera.ScreenPointToRay(screenCoord);
        return Physics.Raycast(ray, out hit, 9999999, mask);
    }

    /// <summary>
    /// 通过世界坐标投射射线，计算碰撞点
    /// </summary>
    /// <param name="worldCoord"> 世界坐标 </param>
    /// <param name="hit"> 碰撞点 </param>
    /// <returns> 是否有碰撞 </returns>
    public static bool RaycastToObstruct(Vector3 worldCoord, out RaycastHit hit)
    {     
        int mask = 1 << CommonParam.ObstructLayer;
        mask = ~mask;
        Ray ray = new Ray(new Vector3(worldCoord.x, worldCoord.y + 9000, worldCoord.z), Vector3.down);
        return Physics.Raycast(ray, out hit, 999999, mask);
    }

    /// <summary>
    /// 设置主角所有的宠物的AI
    /// </summary>
    /// <param name="aiFunc"> AI生成函数，若为null则终止AI </param>
    public static void SetAllFriendsAI(Func<BaseObject, IRoutine> aiFunc)
    {
        foreach (var friend in Character.Self.mFriends)
        {
            if (friend != null && !friend.IsDead())
            {
                if (aiFunc != null)
                {
                    friend.aiMachine.StartTerminateAI(aiFunc(friend));
                }
                else
                {
                    friend.aiMachine.Stop();
                }
            }
        }
    }

    /// <summary>
    /// 测试窗口是否处于开启状态
    /// 开启状态定义为存在且可见
    /// </summary>
    /// <param name="key"> 窗口的键 </param>
    /// <returns> 是否处于开启状态 </returns>
    public static bool IsWindowOpen(string key)
    {
        object obj;
        DataCenter.Self.getData(key, out obj);

        if (obj != null)
        {
            tWindow win = obj as tWindow;
            return win != null && win.mGameObjUI != null && win.mGameObjUI.activeInHierarchy;
        }

        return false;
    }

    /// <summary>
    /// 设定PVE关卡设置按钮状态
    /// </summary>
    /// <param name="isEnabled"> 是否启用 </param>
    public static void SetPVEBattleSettingEnabled(bool isEnabled)
    {
        DataCenter.SetData("PVE_TOP_RIGHT_WINDOW", "ENABLE_SETTING_BTN", isEnabled);
    }

    /// <summary>
    /// 设定Boss战关卡设置按钮状态
    /// </summary>
    /// <param name="isEnabled"> 是否启用 </param>
    public static void SetBossBattleSettingEnabled(bool isEnabled)
    {
        DataCenter.SetData("BOSS_ATTACK_INFO_WINDOW", "ENABLE_SETTING_BTN", isEnabled);
    }

    /// <summary>
    /// 设定技能按钮是否可用
    /// </summary>
    /// <param name="isEnabled"> 是否可用 </param>
    public static void SetSkillButtonEnabled(bool isEnabled)
    {
        GameObject skillWindow = GameCommon.FindUI("battle_skill_window");

        if (skillWindow != null)
        {
            for (int i = 0; i <= 4; ++i)
            {
                string btn = "do_skill_" + i;
                GameCommon.SetUIButtonEnabled(skillWindow, btn, isEnabled);
            }
        }
    }

    /// <summary>
    /// 设置自动战斗按钮是否可用
    /// </summary>
    /// <param name="isEnabled"> 是否可用 </param>
    public static void SetAutoBattleButtonEnabled(bool isEnabled)
    {
        GameObject autoBattleWindow = GameCommon.FindUI("battle_auto_fight_button");

        if (autoBattleWindow != null)
        {
            GameCommon.SetUIButtonEnabled(autoBattleWindow, "auto_fight", isEnabled);
        }
    }

    /// <summary>
    /// 打开跳过序章窗口
    /// </summary>
    public static void OpenSkipPrologueWindow()
    {
        GlobalModule.CancleOnNextLateUpdate(CloseSkipPrologueWindowImmediate);
        DataCenter.OpenWindow("GUIDE_SKIP_WINDOW");
    }

    /// <summary>
    /// 关闭跳过序章窗口
    /// </summary>
    public static void CloseSkipPrologueWindow()
    {
        GlobalModule.DoOnNextLateUpdate(CloseSkipPrologueWindowImmediate);
    }

    /// <summary>
    /// 立即关闭跳过序章窗口
    /// </summary>
    public static void CloseSkipPrologueWindowImmediate()
    {
        DataCenter.CloseWindow("GUIDE_SKIP_WINDOW");
    }

    /// <summary>
    /// 获取当前关卡所在地图的总星数
    /// </summary>
    /// <returns></returns>
    public static int GetCurrentMapPageStar()
    {
        int totalStar = 0;
        List<WorldMapPoint> pointList = new List<WorldMapPoint>();

        for (int i = 0; i < CommonParam.stagePerMap; i++)
        {
            WorldMapPoint pt = WorldMapPoint.Create(ScrollWorldMapWindow.mDifficulty, ScrollWorldMapWindow.mPage, i);

            if (pt != null)
            {
                pointList.Add(pt);
            }
        }

        foreach (var pt in pointList)
        {
            StageProperty prop = StageProperty.Create(pt.mId);

            if (prop != null)
            {
                totalStar += prop.mBestStar;
            }
        }

        return totalStar;
    }
}