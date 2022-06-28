using UnityEngine;
using System;
using System.Collections;
using Utilities.Routines;


public enum GUIDE_ACTION_TYPE
{
    CLICK_BUTTON = 1,   // 引导点击按钮，若找不到该按钮，会一直等待按钮出现
    // "ui_name"    string[]    控件路径
    // "delay"      float       控件出现之后的引导延迟
    // "sort_type"  int         如果有多个同名控件在同一父节点下，则启用排序查找，1表示从左到右，2表示从上到下
    // "sort_index" int         启用排序查找时的位次，从0开始

    DIALOG = 2,         // 对话
    // "dialog_id"  int[]       对话ID组，根据主角类型选取对话ID

    CLICK_GROUND = 3,   // 引导点击地面，该引导直至主角走到目标点结束
    // "offset"     Vector3     相对于主角的坐标偏移量，以米为单位
    // "size"       Vector2     触发区域大小，以像素为单位，默认100x100

    CLICK_SKILL_BUTTON = 4,     // 引导点击技能按钮
    // "button_id"  int         技能按钮id，0表示主角技能，1-3表示宠物技能
    // "enemy"      int         目标Monster ID，若为0表示最近的Monster

    CLICK_ENEMY = 5,            // 引导集火攻击目标
    // "enemy"      int         目标Monster ID，若为0表示最近的Monster
    // "size"       Vector2     触发区域大小，以像素为单位，默认100x100

    MOVE_CAMERA = 6,        // 摄像机移动，该引导直至摄像机移动到指定点结束
    // "mode"       int     模式，0-跟随主角 1-移动到指定Monster
    // "enemy"      int     目标Monster ID，若为0表示最近的Monster，只在模式1起作用
    // "time"       float   运动时间，只在模式1起作用

    PROLOGUE_PAGE = 7,       // 序章漫画图
    // "path"       string      图片路径
    // "time"       float       最大持续时间
    // "fade_time"  float       进入序章图片或切换图片的淡入时间
    // "out_time"   float       退出序章图片的淡出时间
    // "bgm"        string      背景音乐
    // "txt"        string[]    文字
    // "speed"      float       速度（字数/秒）
    // "size"       int         字号
    // "offset"     Vector2     偏移
    // "txt_delay"  float       文字延迟

    PROLOGUE_BATTLE_START = 8,  // 开始序章战斗

    PROLOGUE_CG = 9,            // 播放序章CG
    // "cg"         int         cg索引

    PROLOGUE_VIDEO = 10,        // 播放视频
    // "video"       string      视频名称，视频必须放置在Assets/StreamingAssets目录下，带后缀名，如"game_mv.mp4"，且只能在真机上播放
}

// 通用配置
// "stop_ai"            int         是否禁用常规AI
// "disable_skill"      int         是否禁用技能按钮
// "resume_skill"       int         是否继续之前暂停的技能（如果有的话） 
// "bgm"                string      背景音乐

// 手指配置（在引导类型1、3、4、5中生效）
// "hide"               int         是否隐藏，默认显示
// "offset"             Vector2     偏移
// "rotate"             float       旋转
// "scale"              float       缩放，默认1
// "mask_scale"         float       触发区域缩放，默认1
// "flip"               int         是否翻转

// Tip配置（在引导类型1、3、4、5中生效）
// "offset"             Vector2     偏移
// "text"               string      提示内容


public class GuideAction : Routine
{
    public GuideData data { get; private set; }
    public GUIDE_ACTION_TYPE type { get; private set; }
    public GuideTrigger trigger { get; private set; }

    private Action lateCallback;

    public GuideAction(GuideData data, GuideTrigger trigger)
    {
        this.data = data;
        this.type = (GUIDE_ACTION_TYPE)data.actionType;
        this.trigger = trigger;
        Bind(DoGuideAction());
    }

    private IEnumerator DoGuideAction()
    {
        // 设置光标偏移旋转和缩放及遮罩触发区域缩放
        var cursorParam = data.cursorParam;
        Vector2 cursorOffset = cursorParam.GetVector2("offset");
        float cursorRotate = cursorParam.GetFloat("rotate");
        float cursorScale = cursorParam.GetFloat("scale", 1f);
        float maskScale = cursorParam.GetFloat("mask_scale", 1f);
        bool cursorFlip = cursorParam.GetInt("flip") > 0;
        GuideKit.SetCursorOffset(cursorOffset.x, cursorOffset.y);
        Vector3 cursorAngles = new Vector3(0f, cursorFlip ? 180f : 0f, cursorRotate);
        GuideKit.SetCursorRotate(cursorAngles);
        GuideKit.SetCursorScale(cursorScale);
        GuideKit.SetMaskButtonScale(maskScale);

        // 设置Tip的偏移
        var tipParam = data.tipParam;
        Vector2 tipOffset = tipParam.GetVector2("offset");
        GuideKit.SetTipOffset(tipOffset.x, tipOffset.y);

        // 设置BGM
        string bgm = data.actionParam.GetString("bgm", "");

        if (bgm != "")
            GameCommon.SetBackgroundSound(bgm, 0.7f);

        // 战斗中设置
        BattleSettine();

        // 记录并设置相关状态    
        int stageId = 0;
        bool isAiEnabled = false;
        bool needReserveTimeScale = false;
        float reservedTimeScale = 1f;

        if (MainProcess.mStage != null && MainProcess.mStage.mbBattleActive && !MainProcess.mStage.mbBattleFinish)
        {
            stageId = MainProcess.mStage.mConfigIndex;
        }

        if (stageId > 0)
        {
            isAiEnabled = MainProcess.mStage.aiEnabled;
            MainProcess.mStage.DisableAI();

            if (type == GUIDE_ACTION_TYPE.PROLOGUE_PAGE)
            {
                needReserveTimeScale = true;
                reservedTimeScale = Time.timeScale;
                Time.timeScale = 1f;
            }
        }

        // 如果在序章战斗中引导战斗，在引导过程中暂停怪物的技能CD
        if (Guide.inPrologue && MainProcess.mStage != null && MainProcess.mStage.mbBattleStart && !MainProcess.mStage.mbBattleFinish)
        {
            MainProcess.mStage.isMonsterSkillCDPaused = true;
            //foreach (var obj in ObjectManager.Self.mObjectMap)
            //{
            //    if (obj != null && !obj.IsDead() && obj.mbVisible && obj.IsEnemy(Character.Self))
            //    {
            //        obj.PauseSkillCD();
            //    }
            //}
        }

        // 主引导行为
        switch (type)
        {
            case GUIDE_ACTION_TYPE.CLICK_BUTTON:
                yield return DoClickButton();
                break;

            case GUIDE_ACTION_TYPE.DIALOG:
                yield return DoDialog();
                break;

            case GUIDE_ACTION_TYPE.CLICK_GROUND:
                yield return DoClickGround();
                break;

            case GUIDE_ACTION_TYPE.CLICK_SKILL_BUTTON:
                yield return DoClickSkillButton();
                break;

            case GUIDE_ACTION_TYPE.CLICK_ENEMY:
                yield return DoClickEnemy();
                break;

            case GUIDE_ACTION_TYPE.MOVE_CAMERA:
                yield return DoMoveCamera();
                break;

            case GUIDE_ACTION_TYPE.PROLOGUE_PAGE:
                yield return DoProloguePage();
                break;

            case GUIDE_ACTION_TYPE.PROLOGUE_BATTLE_START:
                StartPrologueBattle();
                break;

            case GUIDE_ACTION_TYPE.PROLOGUE_CG:
                yield return DoProlugueCG();
                break;

            case GUIDE_ACTION_TYPE.PROLOGUE_VIDEO:
                yield return DoProlugueVideo();
                break;
        }

        // 恢复相关状态
        if (MainProcess.mStage != null 
            && MainProcess.mStage.mbBattleActive 
            && !MainProcess.mStage.mbBattleFinish 
            && MainProcess.mStage.mConfigIndex == stageId
            && data.triggerType != (int)GUIDE_ACTION_TYPE.PROLOGUE_BATTLE_START)
        {
            if (isAiEnabled)
            {
                MainProcess.mStage.EnableAI();
            }
            else
            {
                if (Character.Self != null)
                {
                    Character.Self.aiMachine.StartTerminateAI(new IdleRoutine(Character.Self));
                }

                GuideKit.SetAllFriendsAI(x => new IdleRoutine(x));
            }

            if (needReserveTimeScale)
            {
                Time.timeScale = reservedTimeScale;
            }
        }

        // 如果在序章战斗中引导战斗，在引导完成后继续怪物的技能CD
        if (Guide.inPrologue && MainProcess.mStage != null && MainProcess.mStage.mbBattleStart && !MainProcess.mStage.mbBattleFinish)
        {
            MainProcess.mStage.isMonsterSkillCDPaused = false;
            //foreach (var obj in ObjectManager.Self.mObjectMap)
            //{
            //    if (obj != null && !obj.IsDead() && obj.mbVisible && obj.IsEnemy(Character.Self))
            //    {
            //        obj.ResumeSkillCD();
            //    }
            //}
        }

        if (lateCallback != null)
        {
            lateCallback();
            lateCallback = null;
        }

        // 后续处理
        //if (!Guide.inPrologue && data.triggerType == (int)GUIDE_TRIGGER_TYPE.BATTLE_FINISH)
        //{
        //    PVEStageBattle pve = MainProcess.mStage as PVEStageBattle;

        //    if (pve != null)
        //    {
        //        pve.ManualAccount();
        //    }
        //}
    }

    private IEnumerator DoClickButton()
    {
        GameObject uiTarget = null;

        if (trigger.type == GUIDE_TRIGGER_TYPE.UI_VISIBLE
            && data.triggerParam.rawText == data.actionParam.rawText
            && trigger.uiTarget != null
            && trigger.uiTarget.activeInHierarchy)
        {
            uiTarget = trigger.uiTarget;
        }
        else
        {
            string[] path = data.actionParam.GetStringArray("ui_name");
            int sortArrangement = data.actionParam.GetInt("sort_type", 0);

            if (path.Length > 0)
            {
                WaitUIVisible w;

                if (sortArrangement == 0)
                {
                    w = new WaitUIVisible(path);
                }
                else
                {
                    UIArrangement arrangement = sortArrangement == 1 ? UIArrangement.Horizontal : UIArrangement.Vertical;
                    int index = data.actionParam.GetInt("sort_index", 0);
                    w = new WaitUIVisible(arrangement, index, path);
                }

                yield return w;
                uiTarget = w.target;
            }
        }

        if (uiTarget != null)
        {
            float delay = data.actionParam.GetFloat("delay");
            yield return new Delay(delay);
            yield return new ButtonMaskRoutine(uiTarget, data.tipParam.GetString("text"), data.cursorParam.GetInt("hide", 0) == 0, true);
        }
    }

    private IEnumerator DoDialog()
    {
        int[] dialogIds = data.actionParam.GetIntArray("dialog_id");
        int roleType = GameCommon.GetMainRoleType();

        if (dialogIds.Length > roleType)
        {
            yield return new DialogRoutine(dialogIds[roleType]);
        }
        else if (dialogIds.Length > 0)
        {
            yield return new DialogRoutine(dialogIds[0]);
        }
    }

    private IEnumerator DoClickGround()
    {
        if (MainProcess.mStage == null)
        {
            yield break;
        }

        Vector3 offset = data.actionParam.GetVector3("offset");
        Vector2 size = data.actionParam.GetVector2("size", new Vector2(100f, 100f));
        Vector3 center = Character.Self.GetPosition() + offset;
        string tip = data.tipParam.GetString("text");

        yield return new MaskRoutine(MainProcess.mMainCamera, center, size.x, size.y, tip, data.cursorParam.GetInt("hide", 0) == 0);
        CameraMoveEvent.BindMainObject(Character.Self);

        RaycastHit hit;

        if (GuideKit.RaycastToObstruct(center, out hit))
        {
            GuideKit.SetAllFriendsAI(x => new FollowRoutine(x, Character.Self, x.mFollowOffset, false));
            yield return new MoveToRoutine(Character.Self, hit.point);
            GuideKit.SetAllFriendsAI(null);
        }
    }

    private IEnumerator DoClickSkillButton()
    {
        if (MainProcess.mStage == null)
        {
            yield break;
        }
   
        int monsterId = data.actionParam.GetInt("enemy");
        int buttonId = data.actionParam.GetInt("button_id");
        string tip = data.tipParam.GetString("text");
        BaseObject enemy = null;

        if (monsterId == 0)
        {
            enemy = Character.Self.FindNearestEnemy(true);
        }
        else
        {
            enemy = Character.Self.FindNearestAlived(x => !Character.Self.IsSameCamp(x) && x.mConfigIndex == monsterId);
        }

        if (enemy != null)
        {
            GameObject btn = GameCommon.FindUI("do_skill_" + buttonId);
            yield return new MaskRoutine(btn, tip, data.cursorParam.GetInt("hide", 0) == 0);
            CameraMoveEvent.BindMainObject(Character.Self);

            var skillData = AIKit.GetSkillButtonData(buttonId);
            //int skillIndex = skillData.get("SKILL_INDEX");
            //int skillLevel = AIKit.GetCharacterSkillLevel(skillData.mPetUsePos);
            //float skillBounds = AIKit.GetSkillBounds(skillIndex) + enemy.mImpactRadius - 0.05f;
            //yield return new MoveTowardsRoutine(Character.Self, enemy, skillBounds);
            //yield return new SkillRoutine(Character.Self, enemy, skillIndex, skillLevel, skillData);
            if (skillData != null && MainProcess.Self.mController != null)
            {
                lateCallback = () => MainProcess.Self.mController.OnClickSkill(skillData);
            }
        }
    }

    private IEnumerator DoClickEnemy()
    {
        if (MainProcess.mStage == null)
        {
            yield break;
        }

        int monsterId = data.actionParam.GetInt("enemy");
        BaseObject enemy = null;// AIKit.FindNearestEnemy(Character.Self, true);

        if (monsterId == 0)
        {
            enemy = Character.Self.FindNearestEnemy(true);
        }
        else
        {
            enemy = Character.Self.FindNearestAlived(x => !Character.Self.IsSameCamp(x) && x.mConfigIndex == monsterId);
        }       

        if (enemy != null)
        {         
            Vector2 size = data.actionParam.GetVector2("size", new Vector2(100, 100));
            string tip = data.tipParam.GetString("text");
            yield return new MaskRoutine(MainProcess.mMainCamera, enemy.GetPosition(), size.x, size.y, tip, data.cursorParam.GetInt("hide", 0) == 0);
            CameraMoveEvent.BindMainObject(Character.Self);

            //GuideKit.SetAllFriendsAI(x => new TryKillTargetRoutine(x, enemy, 0, 0, null));

            if (!enemy.IsDead() && MainProcess.Self.mController != null)
            {
                lateCallback = () => MainProcess.Self.mController.OnSelectObject(enemy);//Character.Self.aiMachine.RequestSwitchAI("ATTACK", enemy, AI_LAYER.MEDIUM);
                //yield return new TryKillTargetRoutine(Character.Self, enemy, 0, 0, null);
                //yield return null;
            }
        }
    }

    private void BattleSettine()
    {
        if (MainProcess.mStage != null && MainProcess.mStage.mbBattleActive && !MainProcess.mStage.mbBattleFinish)
        {
            int stopAI = data.actionParam.GetInt("stop_ai", -1);

            if (stopAI == 0)
            {
                MainProcess.mStage.DisableAI();
                MainProcess.mStage.EnableAI();
            }
            else if (stopAI == 1)
            {
                MainProcess.mStage.DisableAI();
            }

            int disableSkill = data.actionParam.GetInt("disable_skill", -1);

            if (disableSkill == 0)
            {
                GuideKit.SetSkillButtonEnabled(true);
                GuideKit.SetAutoBattleButtonEnabled(true);
            }
            else if (disableSkill == 1)
            {
                GuideKit.SetSkillButtonEnabled(false);
                GuideKit.SetAutoBattleButtonEnabled(false);
            }
        }
    }

    private IEnumerator DoMoveCamera()
    {
        int mode = data.actionParam.GetInt("mode");
        
        if (mode == 0)
        {
            CameraMoveEvent.BindMainObject(Character.Self);
        }
        else 
        {
            int enemy = data.actionParam.GetInt("enemy");
            float time = data.actionParam.GetFloat("time", 1f);
            BaseObject target = null;

            if (enemy == 0)
            {
                target = Character.Self.FindNearestEnemy(true);
            }
            else 
            {
                target = Character.Self.FindNearestAlived(x => !Character.Self.IsSameCamp(x) && x.mConfigIndex == enemy);
            }      

            if (target != null)
            {
                yield return new MoveCameraRoutine(target.GetPosition(), time);
            }
        }
    }

    private IEnumerator DoProloguePage()
    {
        //GuideKit.OpenSkipPrologueWindow();
        string path = data.actionParam.GetString("path");
        float duration = data.actionParam.GetFloat("time", 3f);
        float fadeTime = data.actionParam.GetFloat("fade_time", 0.5f);
        float fadeOutTime = data.actionParam.GetFloat("out_time", 0.5f);
        string bgm = data.actionParam.GetString("bgm", "");
        string text = data.actionParam.GetString("txt", "");
        
        if(!string.IsNullOrEmpty(text))
        {
            text = text.Replace('#', '\n');
        }

        float textSpeed = data.actionParam.GetFloat("speed", 5f);
        int textSize = data.actionParam.GetInt("size", 30);
        Vector2 textOffset = data.actionParam.GetVector2("offset");
        float textDelay = data.actionParam.GetFloat("txt_delay", 0f);

        PrologueWindowParam param = new PrologueWindowParam();
        param.path = path;
        param.text = text;
        param.textSpeed = textSpeed;
        param.textSize = textSize;
        param.textOffset = textOffset;
        param.textDelay = textDelay;

        if (bgm != "")
        {
            GameCommon.SetBackgroundSound(bgm, 0.7f);
        }

        bool clicked = false;
        GuideKit.SetProloguePictureFadeTime(fadeTime);
        GuideKit.OpenPrologueWindow(param, () => clicked = true);
        yield return new Any(new Delay(duration), new WaitUntil(() => clicked));

        GuideKit.SetProloguePictureFadeTime(fadeOutTime);
        GuideKit.FadeOutPrologueWindow();
        //GuideKit.CloseSkipPrologueWindow();
    }

    private void StartPrologueBattle()
    {
        PVEStageBattle pve = MainProcess.mStage as PVEStageBattle;

        if (pve != null)
        {
            pve.mManualActivate = false;
            pve.mbBattleActive = true;
            pve.EnableAI();
            MainProcess.mLockCharacterPos = false;
            GlobalModule.DontCheckClick(MainProcess.Self);

            //if (PVEStageBattle.mBattleControl != BATTLE_CONTROL.MANUAL)
            //{
            //    Time.timeScale = AIParams.autoBattleTimeScale;
            //}
            Time.timeScale = StageBattle.GetTimeScale(PVEStageBattle.mBattleSpeed);
        }
    }

    private IEnumerator DoProlugueCG()
    {
        bool hasCG = false;
        int cgIndex = data.actionParam.GetInt("cg");

        if (cgIndex > 0 && MainProcess.mStage != null)
        {
            var cg = CGTimeLine.GetByGroup(cgIndex);

            if (cg != null)
            {
                hasCG = true;
                yield return new WaitUntilRoutineFinish(cg.Play());
                CGTools.ClearCG();
            }
        }

        MainProcess.mCameraMoveTool.InitConfig((int)MainProcess.mStage.GetCameraType());

        if (hasCG && Character.Self != null)
        {
            yield return MainProcess.mStage.SmoothResetCamera(Character.Self, 0.5f);
            MainProcess.mCameraMoveTool._MoveByObject(Character.Self);
        }
        else
        {
            MainProcess.mStage.ResetCamera();

            if (Character.Self != null)
            {
                MainProcess.mCameraMoveTool.MoveTo(Character.Self.GetPosition(), 0.5f, () =>
                {
                    MainProcess.mCameraMoveTool._MoveByObject(Character.Self);
                });

                yield return new Delay(0.5f);
            }
        }    
    }

    private IEnumerator DoProlugueVideo()
    {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        string videoName = data.actionParam.GetString("video");

        if (!string.IsNullOrEmpty(videoName))
        {
            Handheld.PlayFullScreenMovie(videoName, Color.black, FullScreenMovieControlMode.CancelOnInput);
            yield return null;         
        }
#endif
        yield return null;

        //by chenliang
        //begin

////#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
//        string videoName = data.actionParam.GetString("video");
//        
//        //if (!string.IsNullOrEmpty(videoName))
//        //{
//        //    Handheld.PlayFullScreenMovie(videoName, Color.black, FullScreenMovieControlMode.CancelOnInput);
//        //    yield return null;         
//        //}
////#endif
//        string[] name = videoName.Split('.');
//        string[] path = new string[]
//        {
//            name[0] + ".ogv",              //视频路径（StreamingAssets文件夹下）
//            "Sound/" + name[0]             //视频对应音频的路径(Resources/Sound文件夹下)
//        };
//
//        DataCenter.OpenWindow("MV_CONTROL_WINDOW", path as object);
//        int tmpContinue = 0;
//        Action tmpCompleteCallback = () =>
//        {
//            tmpContinue = 1;
//        };
//        DataCenter.SetData("MV_CONTROL_WINDOW", "COMPLETE_CALLBACK", tmpCompleteCallback);
//        while (tmpContinue == 0)
//            yield return null;
//------------------

        //代码备份
        //提供可跳过视频提示开关
        /*
        if (!CommonParam.IsUseMVSkipTip)
        {
            MVControlWindow.EnableAudioVedioPlayer(false);
            yield return new WaitForEndOfFrame();      //隔一帧让EnableAudioVedioPlayer生效
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            string videoName = data.actionParam.GetString("video");

            if (!string.IsNullOrEmpty(videoName))
            {
                Handheld.PlayFullScreenMovie(videoName, Color.black, FullScreenMovieControlMode.CancelOnInput);
                yield return null;         
            }
#endif
            yield return null;
        }
        else
        {
            MVControlWindow.EnableAudioVedioPlayer(true);
            yield return new WaitForEndOfFrame();      //隔一帧让EnableAudioVedioPlayer生效
            string videoName = data.actionParam.GetString("video");

            string[] name = videoName.Split('.');
            string[] path = new string[]
            {
                name[0] + ".ogv",              //视频路径（StreamingAssets文件夹下）
                "Sound/" + name[0]             //视频对应音频的路径(Resources/Sound文件夹下)
            };

            DataCenter.OpenWindow("MV_CONTROL_WINDOW", new MVControlParam()
            {
                AtlasName = "CommonUIAtlas",
                SpriteName = "a_ui_tiaoguojuqing",
                Path = path
            });
            int tmpContinue = 0;
            Action tmpCompleteCallback = () =>
            {
                tmpContinue = 1;
            };
            DataCenter.SetData("MV_CONTROL_WINDOW", "COMPLETE_CALLBACK", tmpCompleteCallback);
            while (tmpContinue == 0)
                yield return null;
        }
         * */
    }
}