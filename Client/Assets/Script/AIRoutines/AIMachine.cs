using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;


public class AI_LAYER
{
    public const int HIGHEST = 10000;
    public const int HIGH = 8000;
    public const int MEDIUM_HIGH = 6500;
    public const int MEDIUM = 5000;
    public const int MEDIUM_LOW = 3500;
    public const int LOW = 2000;
    public const int LOWEST = 1000;
}


public abstract class AIState
{
    public abstract int Layer();
    public abstract IEnumerator Create();
}


public class InteruptIntent
{
    public string name;
    public object arg;

    public InteruptIntent(string name)
        : this(name, null)
    { }

    public InteruptIntent(string name, object arg)
    {
        this.name = name;
        this.arg = arg;
    }
}


public class AIMachine : RoutineBlock
{
    public BaseObject owner { get; private set; }
    public IRoutine currentAI { get; private set; }
    public int currentLayer { get; private set; }
    public bool acceptable { get; private set; }
    public bool isStateAI { get; private set; }
    public float totalRunTime { get; private set; }
    public bool onBeatBack { get; set; }     // 处于击飞状态中
    public Skill currentSkill { get; set; } // 当前正在释放的技能(含普攻)
    public BaseObject currentTarget { get; set; }
    public int fixSkill { get; set; }
    public float lastAttackFixEnemyTime { get; set; }
    public BaseObject lastNormalAttackTarget { get; set; }  // 最近的一次普攻目标
    public BaseObject lastSkillTarget { get; set; }     // 最近的一次技能(非普攻)目标
    public float lastNormalAttackTime { get; set; }   // 最近的普攻释放完毕的时间
    public float lastSkillTime { get; set; }       // 最近的技能(非普攻)释放完毕的时间
    public bool hasNormalAttacked { get; set; }       // 是否已经普攻过
    public BaseObject lastlockedTarget { get; set; }  // 最近被锁定的目标
    public bool hasLastNormalAttacked { get; set; }   // 是否对最近被锁定的目标进行过一次普攻
    public SkillIntentQueue skillIntentQueue { get; private set; }

    private BaseObject _fixEnemy;

    public BaseObject fixEnemy 
    {
        get { return _fixEnemy; }
        set { if (_fixEnemy != value) owner.OnLockEnemy(value); _fixEnemy = value; }
    }
    
    private List<AIState> stateList = new List<AIState>();

    public AIMachine(BaseObject owner)
        : this(owner, new DefaultState(owner))
    { }

    public AIMachine(BaseObject owner, AIState defaultState)
    {
        this.owner = owner;
        this.skillIntentQueue = new SkillIntentQueue(owner);
        PushState(defaultState);
    }

    public BaseObject searchTargetCenter
    {
        get 
        {
            var master = owner.GetOwnerObject();

            if (master != null && !master.IsDead() && owner.IsSameCamp(master))
            {
                return master;
            }

            return owner;
        }
    }

    public bool searchTargetInGlobal
    {
        get
        {
            if (fixEnemy == null)
            {
                if (owner is Character)
                    return MainProcess.mStage.GetBattleControl() != BATTLE_CONTROL.MANUAL || fixSkill > 0;
                else 
                    return MainProcess.mStage.IsPVP() || MainProcess.mStage.IsBOSS() || MainProcess.mStage.IsGuildBOSS();
            }

            return false;
        }
    }

    public bool RequestSwitchAI(string name, object arg, int layer)
    {
        IEnumerator ai = GetSwitchAI(name, arg);

        if (ai == null)
        {
            return false;
        }

        InteruptIntent intent = new InteruptIntent(name, arg);
        return TrySwitchAI(ai, layer, intent);
    }

    protected virtual IEnumerator GetSwitchAI(string name, object arg)
    {
        return null;
    }

    public AIState topState { get { return stateList == null ? null : stateList[0]; } }

    public void PushState(AIState state)
    {
        int index = stateList.FindIndex(x => x.Layer() <= state.Layer());

        if (index >= 0)
        {
            stateList.Insert(index, state);
        }
        else
        {
            stateList.Add(state);
        }

        if (index == 0 || stateList.Count == 1)
        {
            OnRaise(state);
        }
    }

    public void PopState(AIState state)
    {
        int index = stateList.IndexOf(state);

        if (index >= 0)
        {
            stateList.RemoveAt(index);

            if (index == 0 && stateList.Count > 0)
            {
                OnRaise(stateList[0]);
            }
        }
    }

    public void PopState(string stateName)
    {
        List<AIState> willPopList = stateList.FindAll(x => x.GetType().Name == stateName);

        foreach (var w in willPopList)
        {
            PopState(w);
        }
    }

    public void Start()
    {
        if (status == RoutineStatus.Unstart)
        {
            skillIntentQueue.Reset();
            owner.StartRoutine(this);
        }

        acceptable = true;
    }

    public bool TryInteruptCurrentAI(InteruptIntent intent)
    {
        if (Routine.IsActive(currentAI) && (intent == null || currentAI.Request<InteruptIntent>(intent) == null))
        {
            currentAI.Break();
            currentAI = null;
            currentLayer = -1;
            isStateAI = false;
            return true;
        }

        return false;
    }

    public bool TrySwitchAI(IEnumerator ai, int layer, InteruptIntent intent)
    {
        if (!Routine.IsActive(this) || !acceptable)
        {
            return false;
        }

        if (Routine.IsActive(currentAI) && (layer < currentLayer || !TryInteruptCurrentAI(intent)))
        {
            return false;
        }

        currentAI = Append(ai);
        currentLayer = layer;
        isStateAI = false;
        return true;
    }

    public void StartTerminateAI(IEnumerator ai)
    {
        if (!Routine.IsActive(this))
        {
            return;
        }

        if (Routine.IsActive(currentAI))
        {
            currentAI.Break();
        }

        acceptable = false;
        currentAI = Append(ai);
        currentLayer = AI_LAYER.HIGHEST;
        isStateAI = false;
    }

    public void Stop()
    {
        if (Routine.IsActive(currentAI))
        {
            currentAI.Break();
        }

        acceptable = false;
        currentAI = null;
        currentLayer = -1;
        isStateAI = false;
    }

    private void OnRaise(AIState state)
    {
        if (Routine.IsActive(currentAI) && (isStateAI || state.Layer() >= currentLayer))
        {
            InteruptIntent intent = new InteruptIntent(state.GetType().Name, state);

            if (currentAI.Request<InteruptIntent>(intent) == null)
            {
                currentAI.Break();
                currentAI = null;
                currentLayer = -1;
                isStateAI = false;
            }
        }
    }

    protected override bool OnBlock()
    {
        if (acceptable)
        {
            totalRunTime += Time.deltaTime;

            if (topState != null && !Routine.IsActive(currentAI))
            {
                currentAI = Append(topState.Create());
                currentLayer = topState.Layer();
                isStateAI = true;
            }
        }

        return true;
    }
}


public class RoleAIMachine : AIMachine
{
    public RoleAIMachine(BaseObject owner)
        : base(owner)
    { }

    protected override IEnumerator GetSwitchAI(string evt, object arg)
    {
        switch (evt)
        {
            case "MOVE":
                return MoveAI(arg);

            case "SKILL":
                return DoSkillAI(arg);

            case "ATTACK":
                return AttackAI(arg);
        }

        return base.GetSwitchAI(evt, arg);
    }

    private IEnumerator MoveAI(object arg)
    {
        //firedTarget = null; // 强制移动会取消集火状态
        //yield return new MoveToRoutine(owner, (Vector3)arg);
        fixSkill = 0;
        yield return new MoveToRoutine(owner, (Vector3)arg);

        if (owner.aiMachine.fixEnemy != null && !owner.InLookBounds(owner.aiMachine.fixEnemy))
        {
            owner.aiMachine.fixEnemy = null;
        }

        yield return new IdleRoutine(owner, owner.FindNearestEnemy(false) == null ? 1f : 0.2f);
    }

    private IEnumerator DoSkillAI(object arg)
    {
        //SkillButtonData skillData = arg as SkillButtonData;

        //if (skillData != null)
        //{
        //    int skillLevel = AIKit.GetCharacterSkillLevel(skillData.mPetUsePos);
        //    int skillIndex = skillData.get("SKILL_INDEX");
        //    yield return DoSkill(skillIndex, skillLevel, skillData);
        //}
        fixSkill = 0;
        fixEnemy = null;
        SkillButtonData skillData = arg as SkillButtonData;

        if (skillData != null)
        {
            int skillIndex = skillData.get("SKILL_INDEX");
            fixSkill = skillIndex;
        }

        yield break;
    }

    private IEnumerator AttackAI(object arg)
    {
        //firedTarget = arg as BaseObject; // 强制攻击会进入集火状态
        //yield return new TryKillTargetRoutine(owner, firedTarget, 0, 0, null);
        fixEnemy = arg as BaseObject;
        fixSkill = 0;
        yield break;
    }

    //private IEnumerator DoSkill(int skillIndex, int skillLevel, tLogicData skillData)
    //{
    //    var t = new TryLockTargetRoutine(owner, SEARCH_RANGE.GLOBAL, skillIndex, skillData);
    //    yield return t;

    //    if (owner.IsSameCamp(t.lockedTarget))
    //    {
    //        yield return new SkillRoutine(owner, t.lockedTarget, skillIndex, skillLevel, skillData);
    //    }
    //    else
    //    {
    //        yield return new TryKillTargetRoutine(owner, t.lockedTarget, skillIndex, skillLevel, skillData);
    //    }
    //}
}


public class DefaultState : AIState
{
    public BaseObject owner { get; private set; }

    public DefaultState(BaseObject owner)
    {
        this.owner = owner;
    }

    public override int Layer()
    {
        return AI_LAYER.LOWEST;
    }

    public override IEnumerator Create()
    {
        yield return new SkillLoop(owner);//FightRoutine(owner);
    }
}


public class StayState : AIState
{
    public BaseObject owner { get; private set; }
    public string animName { get; private set; }

    public StayState(BaseObject owner, string animName)
    {
        this.owner = owner;
        this.animName = animName;
    }

    public override int Layer()
    {
        return (string.IsNullOrEmpty(animName) || animName == "0") ? AI_LAYER.HIGH : AI_LAYER.HIGH - 10;
    }

    public override IEnumerator Create()
    {
        return new StayRoutine(owner, animName);
    }
}


public class FearState : AIState
{
    public BaseObject owner { get; private set; }

    public FearState(BaseObject owner)
    {
        this.owner = owner;
    }

    public override int Layer()
    {
        return AI_LAYER.HIGH - 20;
    }

    public override IEnumerator Create()
    {
        return new FearRoutine(owner);
    }
}


public class FriendFollowState : AIState
{
    public Friend owner { get; private set; }

    public FriendFollowState(Friend owner)
    {
        this.owner = owner;
    }

    public override int Layer()
    {
        return AI_LAYER.MEDIUM_HIGH;
    }

    public override IEnumerator Create()
    {
        return new FollowRoutine(owner, owner.GetOwnerObject(), owner.mFollowOffset, false);
    }
}