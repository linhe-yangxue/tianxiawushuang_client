using UnityEngine;
using System.Collections;
using DataTable;
using Logic;
using System.Collections.Generic;
using System;
using Utilities.Routines;
using System.IO;
using System.Text;

//public enum BATTLE_TYPE
//{
//    ePVE,
//    ePVP,
//    eBOSS,
//	eFourVsFour,
//}

public enum BATTLE_CONTROL
{
    MANUAL,
    AUTO_ATTACK,
    AUTO_SKILL
}

public enum BATTLE_SPEED
{
    X1,
    X2,
    X3
}

public class PVEStageBattle : StageBattle
{
    //public static bool mbAutoBattle = false;
    public static BATTLE_CONTROL mBattleControl = BATTLE_CONTROL.MANUAL;
    public static BATTLE_SPEED mBattleSpeed = BATTLE_SPEED.X1;
    //public MonsterItemDropPool mMonsterDropPool = new MonsterItemDropPool();
    public bool mbBossActive = false;
    public float mBossActiveTime;
    public float mBeatBossTime;
    public float mBeatBossLimitTime = Mathf.Infinity;

    //public bool mManualAccountOnFinish = false;      // 若为true，则战斗结束后不再执行后续操作，需要手工处理
    public bool mManualActivate = false;             // 若为true，则进入关卡后需要手工激活战斗

    private int mCurrentReleaseGroup = 0;            // 当前已激活的怪物组
    private float mCurrentReleaseTime = 0f;          // 当前激活怪物组的时间
    private List<ActiveBirth> mMonsterBirthList = new List<ActiveBirth>();    // 有效怪物出生点列表
    private List<ActiveBirth> mCharacterBirthList = new List<ActiveBirth>();  // 主角出生点列表
    private Vector3 mSumOfFirstGroupMonsterPosition = Vector3.zero; // 第一波怪物的出生点位置之和
    private int mFirstMonsterGroup = int.MaxValue;                  // 第一波怪的Group
    private int mFirstGroupMonsterCount = 0;                        // 第一波怪的数量

    public int mStarMask { get; private set; }

    public override BATTLE_CONTROL GetBattleControl()
    {
        return PVEStageBattle.mBattleControl;
    }

    public override void SetBattleControl(BATTLE_CONTROL control)
    {
        PVEStageBattle.mBattleControl = control;
    }

    public override void InitBattleUI()
    {
        BaseUI.OpenWindow("PVE_BATTLE_WINDOW", null, false);
        ShowGuankaName();    
        DataCenter.CloseWindow("BATTLE_BOSS_HP_WINDOW");
    }

    // 如果在关卡配置中配置项"ROLE_BIRTH_POINT"是坐标，则启用此坐标作为出生位置
    // 如果在PVE关卡中且配置项"ROLE_BIRTH_POINT"是整数，则将出生点的mIndex与此整数相等的点的位置作为出生位置
    // 否则使用原始出生点的位置作为出生位置
    public override Vector3 GetMainRoleBirthPoint(Vector3 birthPoint)
    {
        string rolePoint = mStageConfig["ROLE_BIRTH_POINT"];

        if (rolePoint != null)
        {
            Vector3 configRolePoint;

            if (GameCommon.SplitToVector(rolePoint, out configRolePoint))
            {
                return configRolePoint;
            }
            else
            {
                int birthIndex = 0;

                if (int.TryParse(rolePoint, out birthIndex))
                {
                    for (int i = mCharacterBirthList.Count - 1; i >= 0; --i)
                    {
                        if (mCharacterBirthList[i].mIndex == birthIndex)
                        {
                            return mCharacterBirthList[i].transform.position;
                        }
                    }
                }
            }
        }

        return birthPoint;
    }

    public override void RegisterBirthPoint(MonoBehaviour birthPointCom)
    {
        ActiveBirth birthPoint = birthPointCom as ActiveBirth;

        if (birthPoint == null)
            return;

        if (birthPoint.mOwner == CommonParam.MonsterCamp)
        {
            int key = mConfigIndex * 1000 + birthPoint.mIndex;
            DataRecord monsterConfig = DataCenter.mStageBirth.GetRecord(key);

            if (monsterConfig != null)
            {
                // 如果是新关卡且替换原配置怪物，则启用剧情怪物配置
                if (mStageProperty != null && !mStageProperty.passed && mDramaParam.GetInt("replace_monster") == 1)
                {
                    birthPoint.mBirthConfigIndex = monsterConfig["DRAMA_MONSTER"];
                }
                else
                {
                    birthPoint.mBirthConfigIndex = monsterConfig["MONSTER"];
                }

                //birthPoint.mIndex = monsterConfig["WAYPOINT"];
                birthPoint.mDelay = monsterConfig["DELAY"];
                birthPoint.mGroup = monsterConfig["GROUP"];
                mMonsterBirthList.Add(birthPoint);

                if (birthPoint.mGroup < mFirstMonsterGroup)
                {
                    mFirstMonsterGroup = birthPoint.mGroup;
                    mFirstGroupMonsterCount = 1;
                    mSumOfFirstGroupMonsterPosition = birthPoint.transform.position;
                }
                else if (birthPoint.mGroup == mFirstMonsterGroup)
                {
                    ++mFirstGroupMonsterCount;
                    mSumOfFirstGroupMonsterPosition += birthPoint.transform.position;
                }
            }
            else
            {
                birthPoint.mBirthConfigIndex = 0;
            }
        }
        else 
        {
            mCharacterBirthList.Add(birthPoint);
        }
    }

    public override void InitSetBirthPoint(MonoBehaviour birthPointCom)
    {
        ActiveBirth birthPoint = birthPointCom as ActiveBirth;

        if (birthPoint == null)
            return;

        if (birthPoint.mOwner == CommonParam.MonsterCamp)
        {
            if (birthPoint.mBirthConfigIndex > 0)
            {
                AsyncLoadPool.EnqueueBattleObject(birthPoint.mBirthConfigIndex);
            }         
        }
        else if (Character.Self == null)
        {
            Vector3 charPos = GetMainRoleBirthPoint(birthPointCom.transform.position);

            if (mTeamInitAngle == 0 && mFirstGroupMonsterCount > 0)
            {
                Vector3 centerOfFirstGroupMonsters = mSumOfFirstGroupMonsterPosition / mFirstGroupMonsterCount;
                Vector3 dir = charPos - centerOfFirstGroupMonsters;
                dir.y = 0f;
                CreateMainRole(charPos, true, dir);
            }
            else
            {
                CreateMainRole(charPos, true);
            }
        }
    }

    public override bool IsActiveBirthReleased(ActiveBirth birth)
    {
        if (birth.mGroup <= mCurrentReleaseGroup)
        {
            if (mCurrentReleaseGroup == 0)
            {
                return birth.mDelay < 0.001f || birth.mDelay <= mBattleTime;
            }
            else 
            {
                return birth.mDelay <= Time.time - mCurrentReleaseTime;
            }
        }

        return false;
    }

    public override void OnStart()
    {
        base.OnStart();

        DataRecord config = DataCenter.mStageTable.GetRecord(mConfigIndex);

        if (config != null)
        {
            //mMonsterDropCount = UnityEngine.Random.Range((int)config["DROPMIN"], (int)config["DROPMAX"] + 1);
            //DataRecord bossRecord;
            //IEnumerable<DataRecord> monsterRecords = StageInfoWindow.GetMonsterGroup(mConfigIndex, out bossRecord);
            //mMonsterDropPool.Allocate(monsterRecords, mMonsterDropCount - 1);
            //mMonsterDropPool.Add(bossRecord);
            mBeatBossLimitTime = StageStar.GetBeatBossLimitTime(this);
        }

        if (mManualActivate)
        {
            MainProcess.mCameraMoveTool.InitConfig((int)GetCameraType());
            ResetCamera();

            if (Character.Self != null)
            {
                MainProcess.mCameraObject.transform.position = Character.Self.GetPosition();
                MainProcess.mCameraMoveTool._MoveByObject(Character.Self);
            }
        }
        else 
        {
            CastCGThenDo(() =>
            {
                tEvent evt = EventCenter.Start("TM_WaitToPlayPveBeginTextEffect");
                evt.DoEvent();
            });
        }
    }

    protected override void OnActive()
    {
        if (/*mBattleControl*/GetBattleControl() != BATTLE_CONTROL.MANUAL)
        {
            
            MainProcess.mStage.mHasAutoFighted = true;
        }
        if (PlayerPrefs.HasKey(CommonParam.mUId + "_MY_AUTO_BATTLE_SPEED"))
            mBattleSpeed=(BATTLE_SPEED)int.Parse(PlayerPrefs.GetString(CommonParam.mUId + "_MY_AUTO_BATTLE_SPEED"));
        ChangeSpeedWhenLevelEnough();
        //else
        //{

        //    Time.timeScale = GetTimeScale(mBattleSpeed);

        //    DataCenter.SetData("PET_ATTACK_BOSS_WINDOW", "BATTLE_SPEED", (int)mBattleSpeed);
        //}
        if (!Button_battle_speed.CheckCanApply(mBattleSpeed))
            mBattleSpeed = BATTLE_SPEED.X1;

        DataCenter.SetData("PET_ATTACK_BOSS_WINDOW", "BATTLE_SPEED", (int)mBattleSpeed);
        PlayerPrefs.SetString(CommonParam.mUId + "_MY_AUTO_BATTLE_SPEED", ((int)mBattleSpeed).ToString());
        PlayerPrefs.Save();
        Time.timeScale = GetTimeScale(mBattleSpeed);
        MainProcess.battleIntervencer = new PveBattleIntervencer(mConfigIndex);
    }

    private void ChangeSpeedWhenLevelEnough()
    {
        string mtempKey = CommonParam.mUId+ "MAXBATTLESPEED";
        if (PlayerPrefs.HasKey(mtempKey))
        {
            if (PlayerPrefs.GetString(mtempKey) == "3")
            {
                mBattleSpeed = BATTLE_SPEED.X3;
                PlayerPrefs.SetString(mtempKey, "USED3");
                PlayerPrefs.Save();
            }
            else if (PlayerPrefs.GetString(mtempKey) == "2")
            {
                mBattleSpeed = BATTLE_SPEED.X2;
                PlayerPrefs.SetString(mtempKey, "USED2");
                PlayerPrefs.Save();
            }
        }       
    }
    //public override bool CheckCanDropItem(DataRecord record)
    //{
    //    return mMonsterDropPool.CheckCanDrop(record);
    //}

    //public override void Process(float delayTime)
    //{
    //    base.Process(delayTime);

    //    if (mbBossActive)
    //    {
    //        if (!mbBattleFinish)
    //        {
    //            mBeatBossTime = Time.time - mBossActiveTime;
    //            DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "COUNTDOWN", mBeatBossLimitTime - mBeatBossTime);
    //        }
    //    }
    //    else
    //    {
    //        mBeatBossTime = 0f;
    //    }

    //    if (mbBattleActive && !mbBattleFinish)
    //    {
    //        int currentGroupMonster = 0;

    //        foreach (var obj in ObjectManager.Self.mObjectMap)
    //        {
    //            if (obj != null && !obj.IsDead())
    //            {
    //                Monster m = obj as Monster;

    //                if (m != null && m.mGroup == mCurrentReleaseGroup)
    //                {
    //                    ++currentGroupMonster;
    //                }
    //            }
    //        }

    //        foreach (var birth in mMonsterBirthList)
    //        {
    //            if (birth != null && birth.mActiveObject == null && birth.mOwner == CommonParam.MonsterCamp && birth.mGroup == mCurrentReleaseGroup)
    //            {
    //                ++currentGroupMonster;
    //            }
    //        }

    //        if (currentGroupMonster == 0)
    //        {
    //            ++mCurrentReleaseGroup;
    //            mCurrentReleaseTime = Time.time;
    //        }
    //    }
    //}

    protected override void OnProcess(float delayTime)
    {
		base.OnProcess(delayTime);

        if (mbBossActive)
        {
            if (!mbBattleFinish)
            {
                mBeatBossTime = Time.time - mBossActiveTime;
                //DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "COUNTDOWN", mBeatBossLimitTime - mBeatBossTime);
            }
        }
        else
        {
            mBeatBossTime = 0f;
        }

        if (mbBattleActive && !mbBattleFinish)
        {
            if (GetMonsterCountByGroup(mCurrentReleaseGroup) + GetInactiveMonsterBirthCountByGroup(mCurrentReleaseGroup) == 0)
            {
                ++mCurrentReleaseGroup;
                mCurrentReleaseTime = Time.time;

                if (GetMonsterCount() + GetInactiveMonsterBirthCount() == 0)
                {
                    // 胜负条件通过配表控制
                    mStarMask = StageStar.GetStarMask(this);
                    OnFinish(mStarMask > 0);
                }
            }
        }
    }

    public int GetMonsterCount()
    {
        return ObjectManager.Self.CountAlived(x => x is Monster);
    }

    public int GetInactiveMonsterBirthCount()
    {
        int len = mMonsterBirthList.Count;
        int count = 0;

        for (int i = 0; i < len; ++i)
        {
            var birth = mMonsterBirthList[i];

            if (birth != null && birth.mActiveObject == null && birth.mOwner == CommonParam.MonsterCamp)
            {
                ++count;
            }
        }

        return count;
    }

    public int GetMonsterCountByGroup(int group)
    {
        return ObjectManager.Self.CountAlived(x => (x is Monster) && ((Monster)x).mGroup == group);
    }

    public int GetInactiveMonsterBirthCountByGroup(int group)
    {
        int len = mMonsterBirthList.Count;
        int count = 0;

        for (int i = 0; i < len; ++i)
        {
            var birth = mMonsterBirthList[i];

            if (birth != null && birth.mActiveObject == null && birth.mOwner == CommonParam.MonsterCamp && birth.mGroup == group)
            {
                ++count;
            }
        }

        return count;
    }

    protected override void OnBattleUI(float delayTime)
    {
        base.OnBattleUI(delayTime);

        if (mbBossActive && !mbBattleFinish)
        {
            DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "COUNTDOWN", mBeatBossLimitTime - mBeatBossTime);
        }
    }

    public override void OnObjectDead(BaseObject obj)
    {
        MainProcess.OnObjectDead(obj);
        OBJECT_TYPE objType = obj.GetObjectType();

        if (objType == OBJECT_TYPE.CHARATOR)
        {
            OnFinish(false);
        }
        //else if (objType == OBJECT_TYPE.MONSTER_BOSS || objType == OBJECT_TYPE.BIG_BOSS)
        //{
        //    // 胜负条件通过配表控制
        //    mStarMask = StageStar.GetStarMask(this);
        //    OnFinish(mStarMask > 0);
        //}
    }

    //public override void OnFinish(bool bWin)
    //{
    //    if (mbBattleFinish)
    //    {
    //        return;
    //    }

    //    mbBattleFinish = true;
    //    Time.timeScale = 1f;      
    //    mHPRateOnFinish = bWin ? (float)Character.Self.GetHp() / Character.Self.GetMaxHp() : 0f;
    //    mTeamHpRateOnFinish = Character.Self.GetTeamHpRate();
    //    mBattleTime = Time.time - mActiveTime;
    //    DisableAI();     
    //    BaseObject.mModelAddColor = new Color(0, 0, 0, 1);
    //    MainProcess.ReserveNeedCheckClickFlag(); // 恢复点击检测状态
    //    MainProcess.mCameraMoveTool.SetCameraShakeEnabled(false);

    //    // 胜负条件通过配表控制
    //    mStarMask = bWin ? StageStar.GetStarMask(this) : 0;
    //    bWin = mStarMask > 0;
    //    mbSucceed = bWin;
    //    ClearBattle(bWin);

    //    if (bWin)
    //    {
    //        GameCommon.RandPlaySound(GetConfig("WIN_SOUND"), GameCommon.GetMainCamera().transform.position);
    //    }
    //    else
    //    {
    //        GameCommon.RandPlaySound(GetConfig("LOST_SOUND"), GameCommon.GetMainCamera().transform.position);
    //    }

    //    GameCommon.RemoveBackgroundSound();
    //    CheatVerify(bWin);
    //    OnStageFinish(bWin);

    //    if (mOnStageFinish != null)
    //    {
    //        mOnStageFinish();
    //    }

    //    MainProcess.OnBattleFinish(this, bWin);

    //    if (!MainProcess.isOnCheating)
    //    {
    //        OnRequestResult(bWin);
    //    }
    //}

    protected override void OnStageFinish(bool bWin)
    {
        GuideManager.SetPVEBattleSettingEnabled(false);
    }

    //protected override void OnRequestResult(bool win)
    //{
    //    if (!mManualAccountOnFinish)
    //    {
    //        PetMoveToLine();
    //        TM_WaitToPetFollowRole evt = EventCenter.Start("TM_WaitToPetFollowRole") as TM_WaitToPetFollowRole;
    //        evt.mbWin = win;
    //        evt.DoEvent();
    //    }
    //}

    public void OnBossActive()
    {
        mbBossActive = true;
        mBossActiveTime = Time.time;

        string sound = GetConfig("BOSS_SOUND");

        if (sound != "" && sound != "0")
        {
            GameCommon.PlaySound(sound, GameCommon.GetMainCamera().transform.position);
        }

        MainProcess.Self.gameObject.StartRoutine(DoBossDialog()); 
    }

    private IEnumerator DoBossDialog()
    {
        if (/*mDramaParam != null && */IsMainPVE() && mStageProperty != null && !mStageProperty.passed)
        {
            //StageProperty prop = StageProperty.Create(mStageConfig);

            //if (!prop.passed)
            int[] dialogIds = mDramaParam.GetIntArray("boss_dialog");

            if (dialogIds.Length > 0)
            {
                DisableAI();
                yield return DoDramaDialog(dialogIds);
                EnableAI();
            }
        }
    }

    public void ManualActivate()
    {
        if (mManualActivate)
        {
            tEvent evt = EventCenter.Start("TM_WaitToPlayPveBeginTextEffect");
            evt.DoEvent();

            mManualActivate = false;
        }
    }
}

//-------------------------------------------------------------------------
public abstract class StageBattle
{
    public DataRecord mStageConfig;
    public StageProperty mStageProperty;

    public int mConfigIndex = 0;

    public bool mbBattleStart = false;
    public bool mbBattleActive = false;
    public bool mbBattleFinish = false;
    public bool mbHasSendResult = false;

    public float mStartTime;
    public float mActiveTime;
    public float mBattleTime;

    public bool mbSucceed = false;

	public Vector3 mMainRolePoint;

    public int mTeamBuffer = 0;
    public int mTeamExtraBuffer = 0;
    public int mEquipSetBuffer = 0;

	public int mConsumeItemBuffer = 0;

    public int mPetDeadCount = 0;
    public int mPetSummonCount = 0;
    public int mMonsterDropCount = 0;
    public int mDoSkillCount = 0;

    public float mHPRateOnFinish = 1f;

	//Boss普通攻击Buff
	public int mBossNormalAttack = 0;
	//Boss强力一击Buff
	public int mBossPowerAttack = 0;

    public bool mHasAutoFighted = false;    // 此关卡是否启用过自动战斗
    public bool mbForcePetFollow = false;   // 是否强制宠物跟随    

    public static Action mOnStageActive = null;
    public static Action mOnStageFinish = null;

    public float mTeamHpRateOnFinish = 1f;      // 全队在战斗结束时的剩余血量（该值在mbBattleFinish为true的情况下才有效）

    //public BattleController mController; // 关卡控制器
    //public BattleAssist mAssist;         // 关卡辅助器
    //public BattleIntervencer mIntervencer;

    public ConfigParam mDramaParam = new ConfigParam();
    public bool mbSkipFinishAnim = false;   // 是否跳过战斗结束动画
    public bool mManualAccountOnFinish = false;      // 若为true，则战斗结束后不再执行后续操作，需要手工处理

	public float mMaxBattleTime = (float)BattlePlayerExpWindow.GetStageTime();
	public float mRemainTime = 0f;
    public float mTeamInitAngle = 0f;       // 队伍初始角度，正角度表示（从上往下看）从面向摄像机方向开始顺时针旋转的角度
                                            // 对于PVE，0值特殊处理，表示面向第一波怪物的中心点，如果想面向摄像机，可以填360
                                            // 对于其他战斗，0值表示面向摄像机

    //-------------------------------------------------------------------------

    public static float GetTimeScale(BATTLE_SPEED battleSpeed)
    {
        switch (battleSpeed)
        {
            case BATTLE_SPEED.X2:
                return CommonParam.speedX2;
            case BATTLE_SPEED.X3:
                return CommonParam.speedX3;
            default:
                return 1f;
        }
    }

    public virtual STAGE_TYPE GetStageType()
    {
        int type = mStageConfig["TYPE"];
        return (STAGE_TYPE)type;
    }

    // add by LC
    // begin
    /// <summary>
    /// 获得摄像机参数类型
    /// </summary>
    /// <returns></returns>
    public virtual CAMERA_TYPE GetCameraType()
    {
        switch (GetStageType())
        {
            case STAGE_TYPE.PVP4:
                return CAMERA_TYPE.PVP;
            case STAGE_TYPE.CHAOS:
                //return CAMERA_TYPE.CHAOS;
            default:
                return CAMERA_TYPE.DEFAULT;
        }
    }
    // end

    public bool IsMainPVE()
    {
        STAGE_TYPE t = GetStageType();

        return t == STAGE_TYPE.MAIN_COMMON
            || t == STAGE_TYPE.MAIN_ELITE
            || t == STAGE_TYPE.MAIN_MASTER;
    }

    public bool IsPVE() 
    {
        STAGE_TYPE t = GetStageType();

        return t == STAGE_TYPE.MAIN_COMMON
            || t == STAGE_TYPE.MAIN_ELITE
            || t == STAGE_TYPE.MAIN_MASTER
            || t == STAGE_TYPE.EXP
            || t == STAGE_TYPE.MONEY
            //by chenliang
            //begin

//            || t == STAGE_TYPE.TOWER;
//----------------------
            || t == STAGE_TYPE.TOWER
            || t == STAGE_TYPE.FAIRYLAND
            || t == STAGE_TYPE.GUILDBOSS
            || t == STAGE_TYPE.GRAB;
            //end
    }

    public bool IsPVP() 
    {
        return GetStageType() == STAGE_TYPE.PVP4 ;// || GetStageType() == STAGE_TYPE.GRAB
    }

    public bool IsBOSS() 
    { 
        return GetStageType() == STAGE_TYPE.CHAOS; 
    }

    public bool IsGuildBOSS()
    {
        return GetStageType() == STAGE_TYPE.GUILDBOSS;
    }

	public void ShowGuankaName()
	{
		DataCenter.SetData("PVE_TOP_RIGHT_WINDOW", "SHOW_STAGE_NAME", GetStageType());
	}

    public virtual BATTLE_CONTROL GetBattleControl() { return BATTLE_CONTROL.AUTO_SKILL; }

    public virtual void SetBattleControl(BATTLE_CONTROL control) { }

    //public virtual BATTLE_SPEED GetBattleSpeed() { return BATTLE_SPEED.X1; }

    //public virtual void SetBattleSpeed(BATTLE_SPEED speed) { }

	public StageBattle()
	{
		BaseObject.mModelAddColor = new Color(0, 0, 0, 1);
        aiEnabled = false;
	}    

    public void InitConfig(int configIndex)
    {
        mConfigIndex = configIndex;
        mStageConfig = DataCenter.mStageTable.GetRecord(configIndex);
        mStageProperty = StageProperty.Create(mStageConfig);
		string strColor = mStageConfig.getData("MODEL_COLOR");
		char[] space = {' '};
		string[] colorVal = strColor.Split(space, 4);
		if (colorVal.Length>=4)
		{
			BaseObject.mModelAddColor.r = float.Parse(colorVal[0])/255;
			BaseObject.mModelAddColor.g = float.Parse(colorVal[1])/255;
			BaseObject.mModelAddColor.b = float.Parse(colorVal[2])/255;
			BaseObject.mModelAddColor.a = float.Parse(colorVal[3])/255;
		}

        mTeamInitAngle = (int)mStageConfig["ROLE_BIRTH_DIRECTION"];

        string dramaText = mStageConfig["DRAMA_PARAM"];
        // "begin_dialog"       int[]   开始对话
        // "boss_dialog"        int[]   碰见boss时的对话
        // "end_dialog"         int[]   战斗胜利后对话
        // "account_dialog"     int[]   结算时对话
        // "astrology_tip"      string  "凑齐X个星芒石，可以粉碎阴姬的阴谋"
        // "astrology_num"      string  "星芒石X1"
        // "replace_monster"    int     是否替换出生怪物，如果替换从StageBirth表中的DRAMA_MONSTER字段读取怪物，1表示替换，不配或其他值表示不替换
        // "assist_pet"         int[]   助战宠物，数组第1~4个元素分别表示第1~4站位，如 0#0#0#30001 表示前3个站位用原宠物，第4站位使用助战宠物   

        if (dramaText != "" && dramaText != "0")
        {
            mDramaParam = new ConfigParam(dramaText);
        }
    }

    public virtual void InitBattleUI() { }

    public virtual void RegisterBirthPoint(MonoBehaviour birthPointCom) { }

    public virtual void InitSetBirthPoint(MonoBehaviour birthPointCom) { }

    public virtual bool IsActiveBirthReleased(ActiveBirth birth) { return true; }

    // 如果在关卡配置中配置项"ROLE_BIRTH_POINT"是坐标，则启用此坐标作为出生位置
    // 否则使用原始出生点的位置作为出生位置
    public virtual Vector3 GetMainRoleBirthPoint(Vector3 birthPoint)
    {
        string rolePoint = mStageConfig["ROLE_BIRTH_POINT"];

        if (rolePoint != null)
        {
            Vector3 configRolePoint;

            if (GameCommon.SplitToVector(rolePoint, out configRolePoint))
            {
                return configRolePoint;
            }
        }

        return birthPoint;
    }

	public void CreateMainRole(Vector3 rolePosition, bool bNeedStart)
	{
        Vector3 verDirection = Quaternion.Euler(0f, mTeamInitAngle, 0f) * Vector3.forward;
        CreateMainRole(rolePosition, bNeedStart, verDirection);
	}


    public void CreateMainRole(Vector3 rolePosition, bool bNeedStart, Vector3 verDirection)
    {
        BaseObject obj = ObjectManager.Self.CreateObject(OBJECT_TYPE.CHARATOR, RoleLogicData.GetMainRole().tid);

        if (obj != null)
        {
            obj.SetCamp(1000);
            (obj as Role).SetActiveData(RoleLogicData.GetMainRole());
            //transform.position = new Vector3(transform.position.x, 1000, transform.position.z);
            obj.InitPosition(rolePosition);
            obj.SetRealDirection(obj.GetPosition() - verDirection);

            // Initiation of MoveComponent
            //MoveComponent moveComponent = (obj as ActiveObject).mMoveComponent;
            //moveComponent.mLookAtPos = obj.GetPosition() - Vector3.forward;
            //moveComponent.Start(obj.GetPosition(), obj.GetPosition(), 1f);

            //obj.SetVisible(false);
            if (mbBattleStart && bNeedStart)
                obj.Start();
        }
    }

    public virtual PetData GetPetDataAtTeamPos(int teamPos)
    {
        // 如果是进入新关卡，则可能触发助战宠物和相关剧情
        if (mStageProperty != null && !mStageProperty.passed && mDramaParam != null)
        {
            int[] assistPets = mDramaParam.GetIntArray("assist_pet");

            if (teamPos > 0 && teamPos <= assistPets.Length && assistPets[teamPos - 1] > 0)
            {
                var assistPetData = new PetData();
                assistPetData.tid = assistPets[teamPos - 1];
                assistPetData.level = 1;
                assistPetData.breakLevel = 0;
                assistPetData.itemNum = 1;
                assistPetData.starLevel = TableCommon.GetNumberFromActiveCongfig(assistPetData.tid, "STAR_LEVEL");
                return assistPetData;
            }
        }

        PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        return TeamManager.GetPetDataByTeamPos(teamPos);
    }

    public Data GetConfig(string keyIndex)
    {
        Data d = mStageConfig.get(keyIndex);
        if (d.mObj != null)
            return d;

        EventCenter.Log(LOG_LEVEL.ERROR, "no exist config > " + keyIndex);

        return Data.NULL;
    }

	virtual public void OnStart()
	{
        InitTeamBuffer();
        InitEquipSetBuffer();

		InitUseConsumeItemBuffer ();

		if (Character.Self!=null)
			Character.Self.Start ();

		DataCenter.SetData("BATTLE_LOADING_WINDOW", "CONTINUE", true);
	}

    virtual public void Start()
    {
        //LuaBehaviour.ClearObjects(true);
        Preload();
        AsyncLoadPool.Start();
        mbBattleStart = true;

		GameCommon.SetBackgroundSound(GetConfig("BACK_MUSIC"), GetConfig("MUSIC_VOLUME"));

        //		GameObject rootUI = GameObject.Find(CommonParam.UIRootName);
//		if(rootUI != null)
//		{
//			AudioSource sound = rootUI.transform.Find("Camera").GetComponent<AudioSource>();
//            string backSound = GetConfig("BACK_MUSIC");
        //    AudioClip clip = LoadAudioClip(backSound);
//            if (clip != null)
//            {
//                sound.clip = clip;
//                sound.loop = true;
//                float v = GetConfig("MUSIC_VOLUME");
//                if (v > 0.001)
//                    sound.volume = 0.35f;
//                else
//                    sound.volume = CommonParam.MusicVolume;
//                sound.Play();
//                DEBUG.Log(" ! play back music > " + backSound);
//            }
//        }
        GameCommon.RandPlaySound( GetConfig("START_SOUND"), GameCommon.GetMainCamera().transform.position );

        mStartTime = Time.time;

        if (!aiEnabled)
        {
            isMonsterSkillCDPaused = true;
        }

		OnStart();
    }

    public void ActivateBattle()
    {
        if (mbBattleActive || mbBattleFinish)
        {
            return;
        }

        GlobalModule.DontCheckClick(MainProcess.Self); // 战斗中不再限制点击频率
        MainProcess.mLockCharacterPos = false;
        mActiveTime = Time.time;
        mbBattleActive = true;
        EnableAI();

        OnActive();

        if (mOnStageActive != null)
        {
            mOnStageActive();
        }

        MainProcess.OnBattleActive();
    }

    protected virtual void OnActive() { }


    /// <summary>
    /// 播放CG（优先播放CG，如CG未配置则播放摄像机动画，如果摄像机动画也没配置则直接开始战斗）
    /// 播放完后若主角已存在，则摄像机焦点会平滑移动到主角身上并绑定主角
    /// </summary>
    /// <param name="onAnimFinish"> 播放完CG后执行的委托，如果没有配置CG，委托立即执行 </param>
    public void CastCGThenDo(Action onCGFinish)
    {
        MainProcess.Self.gameObject.StartRoutine(DoCastCG(onCGFinish));
    }

    private IEnumerator DoCastCG(Action onCGFinish)
    {
        bool hasCG = false;
        int cgIndex = mStageConfig["CG"];
        int pathIndex = mStageConfig["CAMERA_PATH"];

        if (cgIndex > 0)
        {
            var cg = CGTimeLine.GetByGroup(cgIndex);

            if (cg != null)
            {
                hasCG = true;
                yield return new WaitUntilRoutineFinish(cg.Play());
                CGTools.ClearCG();
            }
        }
        else if (pathIndex > 0)
        {
            var path = CameraPath.GetObjectPathInCurrentScene(pathIndex);

            if (path != null)
            {
                hasCG = true;
                //var cameraTrans = MainProcess.mMainCamera.transform;
                //yield return new MovementRoutine(cameraTrans, path.path);
                yield return new WaitUntilRoutineFinish(path.Play());
            }
        }

        MainProcess.mCameraMoveTool.InitConfig((int)GetCameraType());

        if (hasCG && Character.Self != null)
        {
            yield return SmoothResetCamera(Character.Self, 0.5f);
            MainProcess.mCameraMoveTool._MoveByObject(Character.Self);
        }
        else
        {          
            ResetCamera();

            if (Character.Self != null)
            {
                MainProcess.mCameraMoveTool.MoveTo(Character.Self.GetPosition(), 0.5f, null);
                yield return new Delay(0.5f);
                MainProcess.mCameraMoveTool._MoveByObject(Character.Self);
            }
        }

        if (/*mDramaParam != null && */IsMainPVE() && mStageProperty != null && !mStageProperty.passed)
        {
            //StageProperty prop = StageProperty.Create(mStageConfig);
            //
            //if (!prop.passed)
            //{
                int[] dialogIds = mDramaParam.GetIntArray("begin_dialog");

                if (dialogIds.Length > 0)
                {
                    yield return DoDramaDialog(dialogIds);
                }
            //}
        }

        if (onCGFinish != null)
        {
            onCGFinish();
        }
    }

    protected IEnumerator DoDramaDialog(int[] dialogIds)
    {
        if (dialogIds != null && dialogIds.Length > 0)
        {
            int roleType = GameCommon.GetMainRoleType();

            if (dialogIds.Length > roleType)
            {
                yield return new DialogRoutine(dialogIds[roleType]);
            }
            else
            {
                yield return new DialogRoutine(dialogIds[0]);
            }
        }
    }

    public void ResetCamera()
    {
        MainProcess.mCameraMoveTool.Reset();
        MainProcess.mCameraMoveTool.StartUpdate();
        Effect_ShakeCamera.InitCamera(MainProcess.mMainCamera.gameObject);
        MainProcess.mCameraMoveTool.SetCameraShakeEnabled(true);
    }

    public IEnumerator SmoothResetCamera(ActiveObject target, float duration)
    {
        if (target == null)
        {
            yield break;
        }
        
        var camTrans = MainProcess.mMainCamera.transform;
        var objTrans = MainProcess.mCameraObject.transform;
        Vector3 initPos = camTrans.position;
        objTrans.position = target.GetPosition();
        camTrans.position = initPos;

        MainProcess.mCameraMoveTool.StartUpdate();
        MainProcess.mCameraMoveTool._MoveByObject(target);

        Vector3 fromLocalPos = camTrans.localPosition;
        Quaternion fromLocalRot = camTrans.localRotation;
        Vector3 toLocalPos = CameraMoveEvent.mCameraOffset;
        Quaternion toLocalRot = Quaternion.Euler(CameraMoveEvent.mCameraAngles);

        float t = 0f;

        while (t < duration)
        {
            yield return null;
            t += Time.deltaTime;
            camTrans.localPosition = Vector3.Lerp(fromLocalPos, toLocalPos, t / duration);
            camTrans.localRotation = Quaternion.Lerp(fromLocalRot, toLocalRot, t / duration);
        }

        MainProcess.mCameraMoveTool.Reset();
        Effect_ShakeCamera.InitCamera(MainProcess.mMainCamera.gameObject);
        MainProcess.mCameraMoveTool.SetCameraShakeEnabled(true);
    }

    public virtual void Preload()
    {
        PaoPaoTextPool.Preload();
        //Preloader.PreloadInStage(GetStageType());

        for (int i = 1; i <= 4; ++i)
        {
            var d = GetPetDataAtTeamPos(i);

            if (d != null && d.tid > 0)
            {
                AsyncLoadPool.EnqueueBattleObject(d.tid, 10);
            }
        }
    }

	//public virtual void StartWar(){}

    public virtual void Process(float delayTime) 
    {
        if (mbBattleActive)
        {
            if (!mbBattleFinish)
            {
                mBattleTime = Time.time - mActiveTime;
                //DataCenter.SetData("BATTLE_PLAYER_EXP_WINDOW", "TIME", mBattleTime);
            }
        }
        else
        {
            mBattleTime = 0f;
        }

        OnProcess(delayTime);
        OnBattleUI(delayTime);
    }

    protected virtual void OnProcess(float delayTime)
    { 
		if (mMaxBattleTime != 0 && mbBattleActive && !mbBattleFinish) 
		{
            mRemainTime = mMaxBattleTime - mBattleTime;

            if (mRemainTime <= 0)
            {
                mRemainTime = 0;
                OnFinish(false);
            }
		}
	}
		
	protected virtual void OnBattleUI(float delayTime)
	{
	    if (mbBattleActive && !mbBattleFinish)
	    {
			DataCenter.SetData("BATTLE_PLAYER_EXP_WINDOW", "TIME", mBattleTime);
	    }
    }

    public virtual void OnObjectDead(BaseObject obj) 
    {
        MainProcess.OnObjectDead(obj);
        OBJECT_TYPE objType = obj.GetObjectType();

        if (objType == OBJECT_TYPE.CHARATOR)
        {
            OnFinish(false);
        }
        else if (objType == OBJECT_TYPE.MONSTER_BOSS || objType == OBJECT_TYPE.BIG_BOSS)
        {
            OnFinish(true);
        }
    }

    public virtual void OnDoSkill(Skill skill)
    {
        if (!skill.isNormalAttack)
        {
            skill.GetOwner().IncreaseSkillCount(skill.mConfigIndex);
        }

        if (skill.mLogicData != null && skill.mLogicData is SkillButtonData)
        {
            ++mDoSkillCount;
        }

        MainProcess.OnDoSkill(skill);
    }

    public virtual void OnSkillDamage(Skill skill, BaseObject target, int damageValue, SKILL_DAMAGE_FLAGS flags)
    {
        MainProcess.OnSkillDamage(skill, target, damageValue, flags);
    }

    public virtual void OnObjectStart(BaseObject obj) 
    {
        MainProcess.OnObjectStart(obj);

        if (obj.GetObjectType() == OBJECT_TYPE.CHARATOR)
        {
            OnCharacterStart(obj as Character);
        }
        else if (obj.GetObjectType() == OBJECT_TYPE.PET)
        {
            OnPetStart(obj as Pet);
        }
    }

    protected virtual void OnCharacterStart(Character character) { }
    protected virtual void OnPetStart(Pet pet) { }

    // 当关卡结束时调用的操作
    // 正常情况下不需要重写此方法，只要重写OnStageFinish（结束时的附加操作）和OnRequestResult（结算前表现及请求结算）即可
    public virtual void OnFinish(bool bWin)
    {
        //记录是否赢了
        //DataCenter.Set("STAGE_BATTLE_ISWIN", bWin);

        if (mbBattleFinish)
        {
            return;
        }

        //LuaBehaviour.ClearObjects(false);
        mbBattleFinish = true;
        Time.timeScale = 1f;
        mbSucceed = bWin;
        mHPRateOnFinish = bWin ? (float)Character.Self.GetHp() / Character.Self.GetMaxHp() : 0f;
        mTeamHpRateOnFinish = Character.Self.GetTeamHpRate();
        mBattleTime = Time.time - mActiveTime;
        DisableAI();
        ClearBattle(bWin);
        BaseObject.mModelAddColor = new Color(0, 0, 0, 1);
        GlobalModule.ReleaseCheckClickFlag(MainProcess.Self); // 释放点击检测状态锁
        MainProcess.mCameraMoveTool.SetCameraShakeEnabled(false);

        if (bWin)
        {
            GameCommon.RandPlaySound(GetConfig("WIN_SOUND"), GameCommon.GetMainCamera().transform.position);
        }
        else
        {
            GameCommon.RandPlaySound(GetConfig("LOST_SOUND"), GameCommon.GetMainCamera().transform.position);
        }

        GameCommon.RemoveBackgroundSound();
        CheatVerify(bWin);
        OnStageFinish(bWin);

        if (mOnStageFinish != null)
        {
            mOnStageFinish();
        }

        MainProcess.OnBattleFinish(bWin);

        if (!MainProcess.isOnCheating)
        {
            OnRequestResult(bWin);
            //PVEStageBattle pve = this as PVEStageBattle;

            //if (pve == null || !pve.mManualAccountOnFinish)
            //{
            //    PetMoveToLine();

            //    TM_WaitToPetFollowRole evt = EventCenter.Start("TM_WaitToPetFollowRole") as TM_WaitToPetFollowRole;
            //    evt.mbWin = bWin;
            //    evt.DoEvent();
            //}
        }
    }

    public virtual void SkipBattle() { }

    protected virtual void OnStageFinish(bool bWin) { }

    protected virtual void OnRequestResult(bool bWin)
    {
        if (!mManualAccountOnFinish)
        {
            if (mbSkipFinishAnim)
            {
                SendBattleResult(bWin);
            }
            else
            {
                PetMoveToLine();
                TM_WaitToPetFollowRole evt = EventCenter.Start("TM_WaitToPetFollowRole") as TM_WaitToPetFollowRole;
                evt.mbWin = bWin;
                evt.DoEvent();
            }
        }
    }

    /// <summary>
    /// 作弊检测
    /// </summary>
    /// <param name="bWin"> 是否胜利 </param>
    protected virtual void CheatVerify(bool bWin)
    {
        if (!VerifySkillCount())
        {
            MainProcess.OnCheatVerifyFailed();
        }
    }

    private bool VerifySkillCount()
    {
        if (Character.Self != null)
        {
            if (!VerifySkillCount(Character.Self))
            {
                return false;
            }

            for (int i = 0; i < Character.msFriendsCount; ++i)
            {
                var f = Character.Self.mFriends[i];

                if (f != null && !VerifySkillCount(f))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private int GetNumOfTheSameSkill(int key)
    {
        int ret = 0;
        List<int> skillList = BattleSkillWindow.GetmSkillIdList();
        foreach (var temp in skillList)
        {
            if (temp == key)
            {
                ret++;
            }
        }
        return ret;
    }

    private bool VerifySkillCount(BaseObject target)
    {
        foreach (var pair in target.mSkillCounter)
        {
            float cd = TableCommon.GetNumberFromSkillConfig(pair.Key, "CD_TIME");

            if (cd > 0.001f)
            {
                float maxCount = (mBattleTime + 5f) / cd + 2f;
                int sameSkillNum = GetNumOfTheSameSkill(pair.Key);
                if(sameSkillNum != 0)
                {
                    maxCount *= sameSkillNum;
                }
                
                if (maxCount < pair.Value)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void PetMoveToLine()
    {
        int n = 0;

        if (Character.Self != null && Character.Self.mFriends != null)
        {
            for (int i = 0; i < Character.Self.mFriends.Length; ++i)
            {
                Friend f = Character.Self.mFriends[i] as Friend; 

                if (f != null && !f.IsDead())
                {
                    Vector3 offset;

                    switch (n)
                    {
                        case 0:
                            offset = new Vector3(2f, 0f, 0f);
                            break;

                        case 1:
                            offset = new Vector3(-2f, 0f, 0f);
                            break;

                        case 2:
                            offset = new Vector3(4f, 0f, 0f);
                            break;

                        default:
                            offset = new Vector3(-4f, 0f, 0f);
                            break;
                    }

                    f.aiMachine.StartTerminateAI(new FollowRoutine(f, f.GetOwnerObject(), offset, true));
                    ++n;
                }
            }
        }

        //foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
        //{
        //    if (obj != null && obj is Friend)
        //    {
        //        Vector3 offset;

        //        switch (n)
        //        {
        //            case 0:
        //                offset = new Vector3(2f, 0f, 0f);
        //                break;

        //            case 1:
        //                offset = new Vector3(-2f, 0f, 0f);
        //                break;

        //            case 2:
        //                offset = new Vector3(4f, 0f, 0f);
        //                break;

        //            default:
        //                offset = new Vector3(-4f, 0f, 0f);
        //                break;
        //        }

        //        Friend f = obj as Friend;
        //        f.aiMachine.StartTerminateAI(new FollowRoutine(f, f.GetOwnerObject(), offset, true));
        //        ++n;
        //    }
        //}
    }

	public void ClearBattle(bool bWin)
	{
        DisableAI();
        var objMap = ObjectManager.Self.mObjectMap;

        for (int i = objMap.Length - 1; i >= 0; --i)
        {
            var obj = objMap[i];

            if (obj != null)
            {
                obj.StopAllBuffer();
            }

            if (obj != null && obj.GetCamp() != CommonParam.PlayerCamp)
            {
                obj.StopAI();
                obj.OnIdle();

                if (bWin)
                {
                    obj.mDied = true;
                    obj.StartFadeInOrOut(2, false);
                }
            }
        }
		
		StopAllSkill();
        DataCenter.SetData("BATTLE_PLAYER_WINDOW", "CLEAR_ALL_BUFF", true);
	}

    public void StopAllSkill()
    {
        foreach (tEvent evt in EventCenter.Self.mTimeEventManager.mUpdateList)
        {
            if (evt != null && (evt is Skill))
                evt.Finish();
        }

        foreach (TimeEventManager.WaitTimeEventData waitData in EventCenter.Self.mTimeEventManager.mWaitEventList)
        {
            tEvent evt = waitData.mWaitEvent;
            if (evt != null && (evt is Skill))
                evt.Finish();
        }

        foreach (tEvent evt in EventCenter.Self.mTimeEventManager.mWillUpdateList)
        {
            if (evt != null && (evt is Skill))
                evt.Finish();
        }

        foreach (TimeEventManager.WaitTimeEventData waitData in EventCenter.Self.mTimeEventManager.mWillWaitEventList)
        {
            tEvent evt = waitData.mWaitEvent;
            if (evt != null && (evt is Skill))
                evt.Finish();
        }

    }

    virtual public void BattleResult(bool bWin)
    {
        if (bWin)
        {
            //DataCenter.SetData("BATTLE_UI", "OPEN", true);
        }
        else
        {
            DataCenter.SetData("MINI_MAP", "DEAD", 0);
            //DataCenter.SetData("BATTLE_UI", "OPEN", false);
        }

        SendBattleResult(bWin);
    }

    protected void SendBattleResult(bool iswin)
    {
        if (!mbHasSendResult)
        {           
            _SendBattleResult(iswin);
            mbHasSendResult = true;
        }
    }

    protected virtual void _SendBattleResult(bool iswin)
    {
        GlobalModule.DoCoroutine(DoSendBattleResult());
        //NetManager.RequestBattleMainResult(this);
    }

    private IEnumerator DoSendBattleResult()
    {
        IBattleAccountRequester req = null;

        if (StageProperty.IsMainStage(this.mConfigIndex))
        {
            req = new BattleMainAccountRequester();
        }
        else if (StageProperty.IsActiveStage(this.mConfigIndex))
        {
            if (mbSucceed/*DataCenter.Get("STAGE_BATTLE_ISWIN")*/)
            {
                req = new DailyStageNetManager.DailyStageBattleEndRequester();
            }
            else
            {
                BattleAccountInfo info = new BattleAccountInfo();
                info.isWin = false;
                info.roleExp = 0;
                var mainRole = RoleLogicData.GetMainRole();
                info.preRoleLevel = mainRole.level;
                info.preRoleExp = mainRole.exp;
                DataCenter.OpenWindow("PVE_ACCOUNT_LOSE_WINDOW", info);
            }
        }
        else 
        {
            yield break;
        }

        if (req != null){
            yield return req.Start();
        }


        if (req != null && req.success)
        {
            DataCenter.CloseWindow("BATTLE_AUTO_FIGHT_BUTTON");
            DataCenter.CloseWindow("BATTLE_SKILL_WINDOW");
            var info = req.accountInfo;

            //活动副本设置
            if (req is DailyStageNetManager.DailyStageBattleEndRequester)
            {
                DataCenter.Set("IS_DAILYS_STAGE_BATTLE", true);
            }
            else
            {
                DataCenter.Set("IS_DAILYS_STAGE_BATTLE", false);
            }

            if (info.isWin)
            {
                MainProcess.Self.gameObject.StartRoutine(DoRequestWin(req));               
            }
            else
            {
                DataCenter.OpenWindow("PVE_ACCOUNT_LOSE_WINDOW", info);
            }
        }
        else
        {
            BattleAccountInfo info = new BattleAccountInfo();
            info.isWin = false;
            info.roleExp = 0;
            var mainRole = RoleLogicData.GetMainRole();
            info.preRoleLevel = mainRole.level;
            info.preRoleExp = mainRole.exp;
            DataCenter.OpenWindow("PVE_ACCOUNT_LOSE_WINDOW", info);
        }
    }

    private IEnumerator DoRequestWin(IBattleAccountRequester req)
    {
        //bool triggerDrama = false;

        // 如果是主线副本且首次胜利，触发剧情
        if (/*mDramaParam != null && */IsMainPVE() && mStageProperty != null && !mStageProperty.passed)
        {
            //StageProperty prop = StageProperty.Create(mStageConfig);

            //if (prop.mPassCount == 1)
            //{
            //triggerDrama = true;
            int[] dialogIds = mDramaParam.GetIntArray("end_dialog");

            if (dialogIds.Length > 0)
            {
                yield return DoDramaDialog(dialogIds);
            }

            string astrologyTip = mDramaParam.GetString("astrology_tip");

            if (!string.IsNullOrEmpty(astrologyTip))
            {
                yield return DoAstrologyTip();
            }
            //}
        }

        DataCenter.OpenWindow("PVE_ACCOUNT_WIN_WINDOW", req.accountInfo);

        if (IsMainPVE() && mStageProperty != null && !mStageProperty.passed)
        {
            int[] dialogIds = mDramaParam.GetIntArray("account_dialog");

            if (dialogIds.Length > 0)
            {
                yield return DoDramaDialog(dialogIds);
            }
        }

        if (req is BattleMainAccountRequester)
        {
            var result = (req as BattleMainAccountRequester).respMsg;

            if (/*result.bossIndex != -1*/result.demonBoss != null && result.demonBoss.tid > 0)
            {
                //Boss_GetDemonBossList_BossData bossData = new Boss_GetDemonBossList_BossData();
                ////bossData.bossIndex = result.demonBoss.bossIndex;//result.bossIndex;
                //bossData.tid = result.demonBoss.tid;//result.tid;
                //bossData.finderId = System.Convert.ToString(CommonParam.mUId);
                //bossData.finderName = RoleLogicData.Self.name;
                //bossData.findTime = CommonParam.NowServerTime();
                //DataRecord bossRecord = DataCenter.mBossConfig.GetRecord(bossData.tid);
                //bossRecord.get("BASE_HP", out bossData.hpLeft);
                RequestBossBattleEvent bossEvent = EventCenter.Start("Event_RequestBossBattle") as RequestBossBattleEvent;//new RequestBossBattleEvent();
                bossEvent.mNextActive = eACTIVE_AFTER_APPEAR.SELECT_LEVEL;
                bossEvent.mBossData = result.demonBoss;//bossData;
                bossEvent.DoEvent();
            }
        }
    }

    private IEnumerator DoAstrologyTip()
    {
        DataCenter.OpenWindow("ASTROLOGY_TIP_WINDOW", mDramaParam);
        tWindow win = DataCenter.GetData("ASTROLOGY_TIP_WINDOW") as tWindow;

        if (win != null)
            yield return new WaitUntil(() => win.mGameObjUI == null || !win.mGameObjUI.activeInHierarchy);
    }

    private void InitTeamBuffer()
    {
        int teamElement = PetLogicData.Self.GetCurTeamElement();

        if (teamElement >= 0 && teamElement <= 4)
        {
			DataRecord record = TableCommon.FindRecord(DataCenter.mTacticalFormation, r => r["ELEMENT_ID"] == teamElement);

            if (record != null)
            {
                mTeamBuffer = record["TEAM_BUFF"];

                if (mConfigIndex > 0 && record["TEAM_STAGE"] == mConfigIndex)
                    mTeamExtraBuffer = record["STAGE_BUFF"];
                else
                    mTeamExtraBuffer = 0;
            }
        }
    }

    private void InitEquipSetBuffer()
    {
        int setIndex = GameCommon.GetRoleEquipSetIndex();
        
        if (setIndex > 0)
        {
            DataRecord record = DataCenter.mSetEquipConfig.GetRecord(setIndex);

            if (record != null)
            {
                mEquipSetBuffer = record["BUFF_ID"];
            }
        }
    }

	private void InitUseConsumeItemBuffer()
	{
		ConsumeItemLogicStatus logic = DataCenter.GetData ("CONSUME_ITEM_STATUS") as ConsumeItemLogicStatus;
		if(logic != null)
		{
			List<ConsumeItemStatus> bufferList = logic.GetBufferItemStatusList ();
			if(bufferList != null)
			{
				for(int i = 0; i < bufferList.Count; i++)
				{
					int bufferIndex = TableCommon.GetNumberFromConsumeConfig (bufferList[i].mIndex, "ITEM_BUFF");
					if(bufferIndex > 0)
					{
						mConsumeItemBuffer = bufferIndex;
					}
				}
			}
		}
	}

    //public virtual bool CheckCanDropItem(DataRecord record) { return false; }

#region Routines

    public bool aiEnabled { get; private set; }

    public void EnableAI()
    {
        isMonsterSkillCDPaused = false;
        aiEnabled = true;
        ObjectManager.Self.ForEachAlived(x => x.mbVisible, x => x.StartAI());
    }

    public void DisableAI()
    {
        isMonsterSkillCDPaused = true;
        aiEnabled = false;
        ObjectManager.Self.ForEachAlived(x => x.mbVisible, x => x.StopAI());
    }

    public void ForcePetFollow(bool follow)
    {
        mbForcePetFollow = follow;

        if (Character.Self != null)
        {
            for (int i = 0; i < Character.msFriendsCount; ++i)
            {
                var f = Character.Self.mFriends[i];

                if (f != null && !f.IsDead())
                {
                    if (follow)
                        f.aiMachine.PushState(new FriendFollowState(f as Friend));
                    else
                        f.aiMachine.PopState("FriendFollowState");
                }
            }
        }
    }

#endregion

    private bool _isMonsterSkillCDPaused = false;

    public bool isMonsterSkillCDPaused
    {
        get 
        {
            return _isMonsterSkillCDPaused;
        }
        set 
        {
            if (value != _isMonsterSkillCDPaused)
            {
                _isMonsterSkillCDPaused = value;

                if (_isMonsterSkillCDPaused)
                {
                    ObjectManager.Self.ForEachAlived(x => x.mbVisible && x is Monster, x => x.PauseSkillCD());
                }
                else
                {
                    ObjectManager.Self.ForEachAlived(x => x.mbVisible && x is Monster, x => x.ResumeSkillCD());
                }
            }
        }
    }

    public void ManualAccount()
    {
        if (mManualAccountOnFinish)
        {
            if (mbSkipFinishAnim)
            {
                SendBattleResult(mbSucceed);
            }
            else
            {
                PetMoveToLine();
                TM_WaitToPetFollowRole evt = EventCenter.Start("TM_WaitToPetFollowRole") as TM_WaitToPetFollowRole;
                evt.mbWin = mbSucceed;
                evt.DoEvent();
            }

            mManualAccountOnFinish = false;
        }
    }
}


public class TM_WaitToBattleResult : CEvent
{
	public bool mbWin = false;
	
	tLocation mCameraTargetPos;
    float mOffset;
	
	SubSpeedMove mCameraMoveValue = new SubSpeedMove();
	
	public override bool _DoEvent()
	{
        //if (Effect_ShakeCamera.Self != null && !Effect_ShakeCamera.Self.GetFinished())
        //{
        //    Effect_ShakeCamera.Self.Finish();
        //}

        MainProcess.mCameraMoveTool.SetCameraShakeEnabled(false);
		Character.Self.StopAI();
		Character.Self.StopMove();
		Character.Self.mDied = true;
		Vector3 pos = Character.Self.GetPosition();

        if (mbWin)
        {
            Character.Self.SetDirection(pos - Vector3.forward * 8);
        }

        int n = 1;

        for (int i = 0; i < Character.msFriendsCount; ++i)
        {
            var f = Character.Self.mFriends[i];

            if (f != null && !f.IsDead())
            {
                f.aiMachine.Stop();
                f.StopMove();
                f.SetDirection(f.GetPosition() - Vector3.forward);
                ++n;

                if (AIKit.InBounds(Character.Self, f, 5f))
                    mOffset += f.GetPosition().x - Character.Self.GetPosition().x;
            }
        }

        mOffset /= n;

		if(Character.Self.mBoundEffect != null)
			Character.Self.mBoundEffect.Finish();

        //mCameraTargetPos = tLocation.Create(ELocationType.OBJECT_POS, new Vector3(0f, 4.5f, 5.6f));
        //mCameraTargetPos.SetApplyDirection(EDirectionType.OWNER);
		
        //Vector3 targetPos = mCameraTargetPos.Update(Character.Self, null) + Vector3.right * mOffset;
        Vector3 targetPos = Character.Self.GetPosition() + new Vector3(0f, 4.5f, -5.6f) + Vector3.right * mOffset;
		
        mCameraMoveValue.SetPos( GameCommon.GetMainCamera().transform.position, targetPos, 60);
        CameraMoveEvent.BindMainObject(null);
		
		StartUpdate();
		
		return true;
	}
	
	public override bool Update(float secondTime)
	{
        Guide.resetLockedTimeFlag = true;
		Vector3 nowPos;
        bool bEnd = mCameraMoveValue.Update(secondTime, out nowPos);
        GameCommon.GetMainCamera().transform.position = nowPos;
//        GameCommon.GetMainCamera().transform.LookAt(Character.Self.GetPosition());
		GameCommon.GetMainCamera().transform.LookAt(new Vector3(Character.Self.GetPosition().x + mOffset, Character.Self.GetPosition().y + 0.8f, Character.Self.GetPosition().z));
        MainProcess.mCameraMoveTool.OnCameraPositionChanged();

        if (bEnd)
        {
            if (mbWin)
            {
                Character.Self.PlayAnim("win");

                for (int i = 0; i < Character.msFriendsCount; ++i)
                {
                    var f = Character.Self.mFriends[i];

                    if (f != null && !f.IsDead())
                    {
                        f.PlayAnim("win");
                    }
                }
            }

			tEvent evt = EventCenter.Start("TM_WaitToPlayPveEndTextEffect");
			evt.set ("IS_WIN", mbWin);
			evt.DoEvent ();

            Guide.resetLockedTimeFlag = true;
            WaitTime(mbWin ? 3f : 1f);
            Guide.resetLockedTimeFlag = true;
        }

        return !bEnd;

//        if (mbWin)
//        {
//            Character.Self.PlayAnim("win");
//            foreach (BaseObject obj in Character.Self.mFriends)
//            {
//                if (obj != null && !obj.IsDead())
//                {
//                    obj.PlayAnim("win");
//                }
//            }
//        }
//
//		tEvent evt = EventCenter.Start("TM_WaitToPlayPveEndTextEffect");
//		evt.set ("IS_WIN", mbWin);
//		evt.DoEvent ();
//	
//		WaitTime(3);
//        return false;
    }

    public override void _OnOverTime()
    {
        Finish();
        MainProcess.mStage.BattleResult(mbWin);
    }
}

class TM_WaitToPlayPveBeginTextEffect : TM_WaitToBattleResult
{
	public override bool _DoEvent()
	{
		float startTime = 0.1f;
		float intervlTime = 0.2f;
		int countPet = 0;
        //PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        //for(int i = 1; i < 4; i++)
        //{
        //	PetData petData = petLogicData.GetPetDataByPos(petLogicData.mCurrentTeam, i);
        //	if(petData != null) countPet++;
        //}
        //countPet -= 1;
        //if(countPet < 0) countPet = 0;

		WaitTime(startTime + countPet * intervlTime);
		return true;
	}

    public override void DoOverTime()
    {
        //int stageIndex = DataCenter.Get("CURRENT_STAGE");
        //
        //if (GuideManager.InGuideStage(stageIndex) && (GuideManager.Notify(GuideIndex.ReadyBattle) || GuideManager.Notify(GuideIndex.ReadyBattle2) || GuideManager.Notify(GuideIndex.ReadyBattle3)))
        //    return;
        //
        //StageProperty property = StageProperty.Create(stageIndex);
        //DataRecord stageConfig = DataCenter.mStageTable.GetRecord(stageIndex);
        //
        //if (property != null && property.mFightCount == 0 && stageConfig["DIALOG"] > 0)
        //{
        //    GuideManager.OpenDialogOnStageStart(stageConfig["DIALOG"]);
        //}
        //else
        //{
            Play();
        //}
    }

    public static void Play()
    {
        DataCenter.OpenWindow("PVE_BEGIN_BATTLE_WINDOW");
        tWindow t = DataCenter.GetData("PVE_BEGIN_BATTLE_WINDOW") as tWindow;

        if (t != null) 
            MonoBehaviour.Destroy(t.mGameObjUI, 2f);

        tEvent evt = EventCenter.Start("TM_WaitToShowAutoBattleButton");
        evt.DoEvent();
    }
}

class TM_WaitToShowAutoBattleButton : TM_WaitToPlayPveBeginTextEffect
{
	public override bool _DoEvent()
	{
		WaitTime (1.5f);
		return true;
	}
	public override void DoOverTime()
	{
        //PVEStageBattle.mbAutoBattle = false;
        //DataCenter.SetData("BATTLE_AUTO_FIGHT_BUTTON", "AUTO_BATTLE", PVEStageBattle.mBattleControl != BATTLE_CONTROL.MANUAL);
        //DataCenter.SetData ("BATTLE_AUTO_FIGHT_BUTTON", "AUTO_BATTLE_INVALID", true);
	
        //if (MainProcess.mStage.IsPVE())
        //{
        //    MainProcess.mStage.ActivateBattle();
        //    //if (MainProcess.mStage.IsPVE() && PVEStageBattle.mbAutoBattle)
        //    //{
        //    //	GuideManager.ExecuteDelayed(() => EventCenter.Start("AutoBattleAI").DoEvent(), 0f);
        //    //}
        //}

        if (MainProcess.mStage != null)
        {
            MainProcess.mStage.ActivateBattle();
        }

        //tLogicData skillData = DataCenter.Self.getData("SKILL_UI");

        //if(skillData != null)
        //{
        //    for (int i = 1; i <= 4; ++i)
        //    {
        //        tLogicData d = skillData.getData("do_skill_" + i);

        //        if (d != null)
        //        {
        //            //d.set("START_SUMMON_CD", i == 4 || GuideManager.currentIndex == (int)GuideIndex.ReadyBattle ? 0f : 0.2f);
        //        }
        //    }
        //}
	}
}

class TM_WaitToPlayPveEndTextEffect : TM_WaitToBattleResult
{
	public override bool _DoEvent()
	{
		mbWin = get ("IS_WIN");
		WaitTime (1.0f);
		return true;
	}

	public override void DoOverTime()
	{
		if(mbWin) DataCenter.OpenWindow ("PVE_SUCCESS_WINDOW");
		else DataCenter.OpenWindow ("PVE_FAILE_WINDOW");

		tWindow tWindow = null;
		if(mbWin)
			tWindow = DataCenter.GetData ("PVE_SUCCESS_WINDOW") as tWindow;
		else 
			tWindow = DataCenter.GetData ("PVE_FAILE_WINDOW") as tWindow;
		if(tWindow != null) 
			MonoBehaviour.Destroy (tWindow.mGameObjUI, 2.0f);

	}
}

class TM_WaitToPetFollowRole : TM_WaitToBattleResult
{
	public override bool _DoEvent()
	{
		WaitTime (1.5f);
		return true;
	}
	public override void DoOverTime()
	{
		foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
		{
			if (obj != null && obj is Friend)
			{
                //Friend f = obj as Friend;
                //f.StopAI ();
                //f.StopMove ();
                //f.OnIdle();
                //obj.StopAIRoutine();
                obj.StopAI();
			}
		}

		TM_WaitToBattleResult e = EventCenter.Start("TM_WaitToBattleResult") as TM_WaitToBattleResult;
		e.mbWin = this.mbWin;
		e.DoEvent();
	}
}


public class MonsterItemDropPool
{
    private HashSet<int> droppableMonsterIndexSet = new HashSet<int>();

    public void Allocate(IEnumerable<DataRecord> records, int dropNum)
    {
        List<int> indexList = new List<int>();

        foreach (var record in records)
        {
            if (record != null)
            {
                int index = record["INDEX"];

                if (index > 0)
                {
                    indexList.Add(index);
                }
            }
        }

        _Allocate(indexList, dropNum, droppableMonsterIndexSet);
    }

    public void Add(DataRecord record)
    {
        if (record != null && record["INDEX"] > 0)
        {
            droppableMonsterIndexSet.Add(record["INDEX"]);
        }
    }

    public bool CheckCanDrop(DataRecord record)
    {
        if (record == null)
            return false;

        int index = record["INDEX"];

        if (index <= 0)
            return false;

        return droppableMonsterIndexSet.Contains(index);
    }

    private void _Allocate(List<int> source, int num, HashSet<int> result)
    {
        if (source.Count == 0 || num <= 0)
            return;

        int index = UnityEngine.Random.Range(0, source.Count);
        result.Add(source[index]);
        source.RemoveAt(index);
        --num;

        _Allocate(source, num, result);
    }
}


public class StageStar
{
    public static bool Check(PVEStageBattle pveStage, int checkIndex)
    {
        if (checkIndex <= 0)
            return true;

        DataRecord record = DataCenter.mStageStar.GetRecord(checkIndex);

        if (record == null)
            return false;

        return Check(pveStage, (int)record["STARTYPE"], (int)record["STARVAR"]);
    }

    public static bool Check(int starMask, int index)
    {
        return (starMask & (1 << index)) > 0;
    }

    public static float GetBeatBossLimitTime(int checkIndex)
    {
        if (checkIndex <= 0)
            return Mathf.Infinity;

        DataRecord record = DataCenter.mStageStar.GetRecord(checkIndex);

        if (record == null || record["STARTYPE"] != 5)
            return Mathf.Infinity;

        return (int)record["STARVAR"];
    }

    public static float GetBeatBossLimitTime(PVEStageBattle pveStage)
    {
        if (pveStage == null)
            return -1f;

        int addStar1 = pveStage.mStageConfig["ADDSTAR_1"];
        int addStar2 = pveStage.mStageConfig["ADDSTAR_2"];
        return Mathf.Min(GetBeatBossLimitTime(addStar1), GetBeatBossLimitTime(addStar2));
    }

    public static string GetDescription(int checkIndex)
    {
        DataRecord record = DataCenter.mStageStar.GetRecord(checkIndex);
        int val = (int)record["STARVAR"];
        string str = (string)record["STARNAME"];
        return string.Format(str, val);
    }

    public static int GetStarMask(PVEStageBattle pveStage)
    {
        if (pveStage == null)
            return 0;

        int addStar0 = pveStage.mStageConfig["ADDSTAR_0"]; // 第1颗星表示关卡胜利条件

        if (!Check(pveStage, addStar0))
            return 0;

        int addStar1 = pveStage.mStageConfig["ADDSTAR_1"];
        int addStar2 = pveStage.mStageConfig["ADDSTAR_2"];
        int mask = 1;

        if (Check(pveStage, addStar1))
            mask |= (1 << 1);

        if (Check(pveStage, addStar2))
            mask |= (1 << 2);

        return mask;
    }

    public static int GetStar(int starMask)
    {
        int star = 0;

        for (int i = 0; i < 3; ++i)
        {
            if (Check(starMask, i))
            {
                ++star;
            }
        }

        return star;
    }

    public static int GetStar(PVEStageBattle pveStage)
    {
        return GetStar(GetStarMask(pveStage));
    }

    private static bool Check(PVEStageBattle pveStage, int checkType, int checkArg)
    {
        switch (checkType)
        {
            case 0: // 在{0}秒内过关
                return pveStage.mBattleTime < checkArg;
            case 1: // 关卡中宠物死亡小于{0}次
                return pveStage.mPetDeadCount < checkArg;
            case 2: // 关卡中召唤宠物小于{0}次
                return pveStage.mPetSummonCount < checkArg;
            case 3: // 关卡中使用技能小于{0}次
                return pveStage.mDoSkillCount < checkArg;
            case 4: // 关卡结束时主角生命值大于{0}%
                // By XiaoWen
                // Begin
                return MainProcess.GetTeamHpRate() * 100f > checkArg;
                // End
            case 5: // 在{0}秒内击败关卡BOSS
                return pveStage.mBeatBossTime < checkArg;
            case 6: // 消灭关卡BOSS
                return true;
            default:
                return false;
        }
    }
}

public class BattleAccountInfo
{
    public int battleId = 0;
    public bool isWin = false;
    public bool isSweep = false;
    public float battleTime = 0f;
    public int starRate = 0;
    public int starMask = 0;
    public ItemDataBase[] dropList;
    public int gold = 0;
    public int roleExp = 0;

    public int preRoleLevel = 0;
    public int preRoleExp = 0;

    public int postRoleLevel = 0;
    public int postRoleExp = 0;

    public int staminaCost = 0;

    public int demage = 0;
}


public enum BATTLE_PROVE_TYPE
{
    PVP = 1,
    BOSS = 2,
    GUILD = 3,
}


public class BattleProver
{
    public static double GetRate(BATTLE_PROVE_TYPE type, int currentValue)
    {
        foreach (var pair in DataCenter.mBattleProve.GetAllRecord())
        {
            var rec = pair.Value;

            if (rec["TYPE"] == (int)type && rec["MIN"] <= currentValue && currentValue <= rec["MAX"])
            {
                return rec["NUM"] / 10000d;
            }
        }

        return 1d;
    }

    public static int GetMaxLimit(BATTLE_PROVE_TYPE type, int currentValue)
    {
        double rate = GetRate(type, currentValue);

        if (type == BATTLE_PROVE_TYPE.PVP)
        {
            return (int)(currentValue / rate) - 1;
        }
        else
        {
            return (int)(currentValue * rate) - 1;
        }
    }
}