using UnityEngine;
using System;
using System.Collections;
using Utilities.Routines;
using Utilities.Events;
using DataTable;


public class AIRoutine : Routine
{
    public BaseObject owner { get; private set; }

    public AIRoutine(BaseObject owner)
    {
        this.owner = owner;
    }
}


public class TargetedAIRoutine : AIRoutine
{
    public BaseObject target { get; private set; }

    public TargetedAIRoutine(BaseObject owner, BaseObject target)
        : base(owner)
    {
        this.target = target;
    }
}


public class IdleRoutine : AIRoutine
{
    public IdleRoutine(BaseObject owner)
        : this(owner, Mathf.Infinity)
    { }

    public IdleRoutine(BaseObject owner, float time)
        : base(owner)
    {
        Bind(DoIdle(time));
    }

    private IEnumerator DoIdle(float time)
    {
        float destTime = Time.time + time;

        while (Time.time < destTime)
        {
            owner.StopMove();
            owner.ResetIdle();
            yield return new Delay(AIParams.ticks);
        }
    }
}


public class MoveToRoutine : RoutineBlock
{
    public BaseObject owner { get; private set; }
    public Vector3 destination { get; private set; }
    public float bounds { get; private set; }
    public float clampSpeed { get; private set; }
    public NavMeshPathStatus pathStatus { get; private set; }
    public Func<float, float> clampSpeedCurve { get; set; } 

    private bool blocked = true;

    public MoveToRoutine(BaseObject owner, Vector3 destination)
        : this(owner, destination, 0f)
    { }

    public MoveToRoutine(BaseObject owner, Vector3 destination, float bounds)
        : this(owner, destination, bounds, Mathf.Infinity)
    { }

    public MoveToRoutine(BaseObject owner, Vector3 destination, float bounds, float clampSpeed)
    {
        this.owner = owner;
        this.destination = destination;
        this.bounds = bounds;
        this.clampSpeed = clampSpeed;
        this.pathStatus = NavMeshPathStatus.PathInvalid;
    }

    protected override void OnStart()
    {
        if (AIKit.InBounds(owner, destination, bounds))
        {
            pathStatus = NavMeshPathStatus.PathComplete;
            blocked = false;
        }
        else if (!owner.MoveToThenCallback(destination, OnReach, clampSpeed))
        {
            pathStatus = NavMeshPathStatus.PathInvalid;
            blocked = false;
        }
    }

    protected override bool OnBlock()
    {
        if (blocked)
        {
            if (clampSpeedCurve != null)
            {
                float distance = AIKit.Distance(owner.GetPosition(), destination);
                owner.SetClampMoveSpeed(clampSpeedCurve(distance));
            }

            if (AIKit.InBounds(owner, destination, bounds))
            {
                pathStatus = NavMeshPathStatus.PathComplete;
                blocked = false;
                owner.WillStopMove();
            }
        }

        return blocked;
    }

    protected override void OnBreak()
    {
        pathStatus = NavMeshPathStatus.PathPartial;
        blocked = false;
        owner.WillStopMove();
    }

    private void OnReach(bool complete)
    {
        if (blocked)
        {
            pathStatus = complete ? NavMeshPathStatus.PathComplete : NavMeshPathStatus.PathPartial;
            blocked = false;
        }
    }
}


public class MoveTowardsRoutine : AIRoutine
{
    public BaseObject owner { get; private set; }
    public ILocation target { get; private set; }
    public float bounds { get; private set; }
    public float clampSpeed { get; private set; }
    public NavMeshPathStatus pathStatus { get; private set; }
    public Func<float, float> clampSpeedCurve { get; set; }

    public MoveTowardsRoutine(BaseObject owner, ILocation target, float bounds)
        : this(owner, target, bounds, Mathf.Infinity)
    { }

    public MoveTowardsRoutine(BaseObject owner, ILocation target, float bounds, float clampSpeed)
        : base(owner)
    {
        this.owner = owner;
        this.target = target;
        this.bounds = bounds;
        this.pathStatus = NavMeshPathStatus.PathInvalid;
        this.clampSpeed = clampSpeed;
        Bind(DoMoveTowards());
    }

    private IEnumerator DoMoveTowards()
    {
        bool firstLoop = true;

        while (true)
        {
            int frame = Time.frameCount;
            Vector3 orgOwnerPos = owner.GetPosition();
            Vector3 orgTargetPos = target.GetPosition();
            float distance = AIKit.Distance(orgOwnerPos, orgTargetPos);

            if (distance <= bounds || distance <= AIParams.minBounds)
            {
                pathStatus = NavMeshPathStatus.PathComplete;
                yield break;
            }

            var r = new MoveToRoutine(owner, target.GetPosition(), bounds, clampSpeed);
            r.clampSpeedCurve = clampSpeedCurve;

            if (distance < AIParams.minPathRefreshBounds)
            {
                yield return r;

                if (!firstLoop && r.pathStatus == NavMeshPathStatus.PathInvalid)
                {
                    pathStatus = NavMeshPathStatus.PathPartial;
                }
                else 
                {
                    pathStatus = r.pathStatus;
                }

                yield break;
            }

            yield return new Conditional(r, () =>
                AIKit.Distance(orgOwnerPos, owner.GetPosition()) + AIKit.Distance(orgTargetPos, target.GetPosition()) < distance
                && !AIKit.InBounds(owner, target, bounds - 0.001f));

            if (r.pathStatus != NavMeshPathStatus.PathComplete || AIKit.InBounds(orgOwnerPos, owner, AIParams.minBounds))
            {
                pathStatus = firstLoop ? r.pathStatus : NavMeshPathStatus.PathPartial;
                yield break;
            }

            firstLoop = false;

            if (Time.frameCount == frame)
                yield return null;
        }
    }

    protected override void OnBreak()
    {
        pathStatus = NavMeshPathStatus.PathPartial;
    }
}


public class FollowRoutine : TargetedAIRoutine
{
    public BaseObject owner { get; private set; }
    public Vector3 offset { get; private set; }
    public bool absolute { get; private set; }

    public FollowRoutine(BaseObject owner, BaseObject target, Vector3 offset, bool absolute)
        : base(owner, target)
    {
        this.owner = owner;
        this.offset = offset;
        this.absolute = absolute;
        Bind(DoFollow());
    }

    private IEnumerator DoFollow()
    {
        owner.SetAdditionalMoveSpeed(AIParams.followAdditionalMoveSpeed);
        var predictLoc = new FollowLocation(target, offset, 0.3f);
        predictLoc.absolute = absolute;

        while (true)
        {
            int frame = Time.frameCount;
            var targetSpeed = target.GetVelocity().magnitude;

            if (AIKit.InBounds(owner, predictLoc, 1f) && targetSpeed < 0.1f)
            {
                yield return null;
            }
            else 
            {
                var r = new MoveTowardsRoutine(owner, predictLoc, 0.3f);
                r.clampSpeedCurve = x => SpeedCurve(x, predictLoc);

                if (targetSpeed > 0.1f)
                {                  
                    yield return r;

                    if (r.pathStatus == NavMeshPathStatus.PathInvalid)
                    {
                        yield return new Delay(0.5f);
                    }
                }
                else 
                {
                    yield return r;
                    yield return new WaitUntil(() => target.GetVelocity().sqrMagnitude >= 0.01f);
                }
            }

            if (Time.frameCount == frame)
                yield return null;
        }
    }

    protected override void OnBreak()
    {
        owner.SetAdditionalMoveSpeed(0f);
    }

    private float SpeedCurve(float x, ILocation destination)
    {
        Vector3 delta = destination.GetPosition() - owner.GetPosition();
        //return Mathf.Max(target.GetVelocity().magnitude, delta.sqrMagnitude + 1f);
        return delta.sqrMagnitude * AIParams.followSpeedCoeff + AIParams.followSpeedBaseValue;
        //return Mathf.Max(target.GetVelocity().magnitude, x * x + 0.5f);
    }
}


public class SkillRoutine : RoutineBlock, IResponsable<InteruptIntent>
{
    public BaseObject owner { get; private set; }
    public BaseObject target { get; private set; }
    public int skillIndex { get; private set; }
    public int skillLevel { get; private set; }
    public tLogicData skillButtonData { get; private set; }

    private Skill skill;
    private bool blocked = true;
    private float startTime;

    public SkillRoutine(BaseObject owner, BaseObject target, int skillIndex, int skillLevel, tLogicData extData)
    {
        this.owner = owner;
        this.target = target;
        this.skillIndex = skillIndex;
        this.skillLevel = skillLevel;
        this.skillButtonData = extData;
    }

    protected override void OnStart()
    {
        startTime = Time.time;
        skill = owner.DoSkillIndependent(skillIndex, target, skillLevel, skillButtonData, Finish);

        if (skill == null)
            return;

        if (skill.isNormalAttack)
        {
            owner.aiMachine.lastNormalAttackTarget = target;
            owner.aiMachine.fixEnemy = target;
        }
        else 
        {
            owner.aiMachine.lastSkillTarget = target;
        }

        if (target == owner.aiMachine.fixEnemy)
        {
            owner.aiMachine.lastAttackFixEnemyTime = Time.time;
        }

        owner.aiMachine.fixSkill = 0;

        if (blocked)
        {
            owner.aiMachine.currentSkill = skill;
            owner.aiMachine.currentTarget = target;
        }
    }

    protected override bool OnBlock()
    {
        if (Time.time - startTime > 5f || skill == null || skill.GetFinished())
        {
            Finish();
        }

        return blocked;
    }

    protected override void OnBreak()
    {
        if (!skill.dontStopOnAIBreak)
        {
            skill.Stop();
        }

        Finish();
    }

    private void Finish()
    {
        if (skill != null)
        {
            //owner.aiMachine.lastSkillTime = Time.time;

            if (skill.isNormalAttack)
            {
                owner.aiMachine.hasNormalAttacked = true;
                owner.aiMachine.lastNormalAttackTime = Time.time;

                if (owner.aiMachine.lastlockedTarget == skill.mTarget)
                {
                    owner.aiMachine.hasLastNormalAttacked = true;
                }
            }
            else 
            {
                owner.aiMachine.lastSkillTime = Time.time;
            }
        }

        owner.aiMachine.currentSkill = null;
        owner.aiMachine.currentTarget = null;
        blocked = false;
    }

    public int Response(InteruptIntent requester)
    {
        if ((requester.name == "MOVE" || requester.name == "SKILL" || requester.name == "ATTACK")
            && !skill.isNormalAttack
            && !skill.mHasAttacked
            && owner is Character)
        {
            return -1;
        }

        return 1;
    }
}

//public class TryLockTargetRoutine : AIRoutine
//{
//    public BaseObject lockedTarget { get; private set; }
//    public int determinedSkillIndex { get; private set; }
//    public tLogicData determinedSkillData { get; private set; }
//    public int determinedSkillLevel { get; private set; }
//    public SEARCH_RANGE searchRange { get; set; }

//    public TryLockTargetRoutine(BaseObject owner, SEARCH_RANGE searchRange, int lockedSkillIndex, tLogicData extData)
//        : base(owner)
//    {
//        this.searchRange = searchRange;
//        Bind(DoTryLockTarget(lockedSkillIndex, extData));
//    }

//    private IEnumerator DoTryLockTarget(int lockedSkillIndex, tLogicData extData)
//    {
//        bool skillLocked = lockedSkillIndex > 0;
//        bool hasLookEnemy = true;

//        if (skillLocked)
//        {
//            determinedSkillIndex = lockedSkillIndex;// extData.get("SKILL_INDEX");
//            determinedSkillData = extData;
//        }

//        while (true)
//        {
//            var p = new Protecter();

//            if (!skillLocked)
//            {
//                tLogicData skillData;
//                int skillLevel = 0;
//                bool exceptSmallSkill = owner.aiMachine.firedTarget == null || !owner.IsEnemy(owner.aiMachine.firedTarget);
//                bool exceptFriendSkill = !owner.aiMachine.hasPhysicalAttacked || Time.time - owner.aiMachine.lastPhysicalAttackTime > AIParams.friendSkillEnableTime;
//                determinedSkillIndex = AIKit.GetPriSkillIndex(owner, AIKit.InAutoSkillBattle(), exceptSmallSkill, exceptFriendSkill, out skillLevel, out skillData);
//                determinedSkillData = skillData;
//                determinedSkillLevel = skillLevel;
//            }

//            lockedTarget = DetermineTargetBySkill(determinedSkillIndex, searchRange);//owner.FindPriEnemy(searchRange);

//            if (lockedTarget == null)
//            {
//                skillLocked = false;
//            }

//            if (lockedTarget == null || !owner.IsEnemy(lockedTarget) || !AIKit.InBounds(owner.GetPosition(), lockedTarget.GetPosition(), owner.LookBound()))
//            {
//                hasLookEnemy = false;
//            }

//            if (lockedTarget == null || determinedSkillIndex == 0)
//            {
//                yield return new Routine(DetermineTargetAndSkill(skillLocked)).AppendWith(OnNotFoundTarget());
//            }

//            if (!hasLookEnemy && lockedTarget != null && owner.IsEnemy(lockedTarget) && AIKit.InBounds(owner.GetPosition(), lockedTarget.GetPosition(), owner.LookBound()))
//            {
//                (owner as ActiveObject).ShowWarnFlag();
//            }

//            float skillBounds = AIKit.GetSkillBounds(determinedSkillIndex) + lockedTarget.mImpactRadius;

//            if (searchRange == SEARCH_RANGE.AUTO && AIKit.InAutoBattle())
//            {
//                skillBounds = Mathf.Min(owner.LookBound(), skillBounds);
//            }

//            if (AIKit.InBounds(owner.GetPosition(), lockedTarget.GetPosition(), skillBounds))
//            {
//                yield break;
//            }

//            float distance = AIKit.Distance(owner.GetPosition(), lockedTarget.GetPosition());
//            float checkBounds = Mathf.Max(skillBounds - 0.1f, distance / 2f);
//            float t = Time.time;
//            var r = new MoveTowardsRoutine(owner, lockedTarget, checkBounds);
//            yield return new Conditional(r, () => !lockedTarget.IsDead() && Time.time - t < AIParams.maxPathRefreshTime);

//            yield return p;
//        }
//    }

//    private IEnumerator OnNotFoundTarget()
//    {
//        BaseObject master = owner.GetOwnerObject();

//        if (master != null && master is Character)
//        {
//            var followLoc = new FollowLocation(master, owner.mFollowOffset);
//            yield return new Any(new IdleRoutine(owner, 1f), new WaitUntil(() => NeedMove(master, followLoc)));
//            yield return new FollowRoutine(owner, master, owner.mFollowOffset, owner.mFollowPos <= 0 || owner.mFollowPos > 8);
//        }
//        else
//        {
//            yield return new IdleRoutine(owner);
//        }  
//    }

//    private bool NeedMove(BaseObject master, FollowLocation followLoc)
//    {
//        if (master.GetVelocity().sqrMagnitude < 0.1f)
//        {
//            return false;
//        }

//        var dir = owner.GetPosition() - followLoc.GetPosition();
//        float cos = Vector3.Dot(master.GetVelocity().normalized, dir.normalized);
//        return cos < 0f;
//    }

//    private IEnumerator DetermineTargetAndSkill(bool skillLocked)
//    {
//        while (lockedTarget == null || determinedSkillIndex == 0)
//        {
//            yield return new Delay(0.5f);

//            if (!skillLocked)
//            {
//                tLogicData skillData;
//                int skillLevel = 0;
//                bool exceptSmallSkill = owner.aiMachine.firedTarget == null || !owner.IsEnemy(owner.aiMachine.firedTarget);
//                bool exceptFriendSkill = !owner.aiMachine.hasPhysicalAttacked || Time.time - owner.aiMachine.lastPhysicalAttackTime > AIParams.friendSkillEnableTime;
//                determinedSkillIndex = AIKit.GetPriSkillIndex(owner, AIKit.InAutoSkillBattle(), exceptSmallSkill, exceptFriendSkill, out skillLevel, out skillData);
//                determinedSkillData = skillData;
//                determinedSkillLevel = skillLevel;
//            }

//            lockedTarget = DetermineTargetBySkill(determinedSkillIndex, searchRange);//owner.FindPriEnemy(searchRange);
//        }
//    }

//    private BaseObject DetermineTargetBySkill(int skillIndex, SEARCH_RANGE searchRange)
//    {
//        int condition = DataCenter.mSkillConfigTable.GetData(skillIndex, "TARGET_CONDITION");

//        bool global = searchRange == SEARCH_RANGE.GLOBAL
//            || (searchRange == SEARCH_RANGE.AUTO && (owner is Character && AIKit.InAutoBattle()) || AIKit.InPVP4());

//        SEARCH_STRATEGY searchStrategy = SEARCH_STRATEGY.DEFAULT;

//        if (Enum.IsDefined(typeof(SEARCH_STRATEGY), condition))
//        {
//            searchStrategy = (SEARCH_STRATEGY)condition;
//        }

//        if(searchStrategy == SEARCH_STRATEGY.DEFAULT || searchStrategy == SEARCH_STRATEGY.ENEMY_LOWEST_HP || searchStrategy == SEARCH_STRATEGY.ENEMY_NEAREST)
//        {         
//            if (owner.aiMachine.firedTarget != null && owner.IsEnemy(owner.aiMachine.firedTarget))
//            {
//                // 如果处于集火状态，只要是对敌技能，就以集火目标作为施法目标
//                return owner.aiMachine.firedTarget;
//            }
//        }

//        return AIKit.SearchTarget(owner, searchStrategy, global);
//    }
//}


//public class TryKillTargetRoutine : TargetedAIRoutine
//{
//    public bool shoudeEvade { get; private set; }
//    public bool shoudeDoSkill { get; private set; }
//    public int shoudeSkillIndex { get; private set; }
//    public tLogicData shoudeSkillData { get; private set; }
//    //public string interuptEvent { get; private set; }

//    public TryKillTargetRoutine(BaseObject owner, BaseObject target, int firstSkillIndex, int firstSkillLevel, tLogicData firstSkillData)
//        : base(owner, target)
//    {
//        Bind(DoTryKillTarget(firstSkillIndex, firstSkillLevel, firstSkillData));
//    }

//    private IEnumerator DoTryKillTarget(int firstSkillIndex, int firstSkillLevel, tLogicData firstSkillData)
//    {
//        owner.aiMachine.lockedTarget = target;

//        if(owner.aiMachine.lastlockedTarget != target)
//        {
//            owner.aiMachine.lastlockedTarget = target;
//            owner.aiMachine.hasLastPhysicalAttacked = false;
//        }       

//        owner.OnLockEnemy(target);
//        int skillIndex = 0;
//        tLogicData skillData = null;
//        int skillLevel = 0;

//        if (firstSkillIndex == 0)
//        {
//            bool exceptFriendSkill = !owner.aiMachine.hasPhysicalAttacked || Time.time - owner.aiMachine.lastPhysicalAttackTime > AIParams.friendSkillEnableTime;
//            skillIndex = AIKit.GetPriSkillIndex(owner, AIKit.InAutoSkillBattle(), !owner.aiMachine.hasLastPhysicalAttacked, exceptFriendSkill, out skillLevel, out skillData);

//            //SEARCH_STRATEGY search = (SEARCH_STRATEGY)(int)DataCenter.mSkillConfigTable.GetData(skillIndex, "TARGET_CONDITION");

//            if (Skill.IsFriendSkill(skillIndex)/*search == SEARCH_STRATEGY.SELF || search == SEARCH_STRATEGY.SELF_LOWEST_HP || search == SEARCH_STRATEGY.FRIEND_NEAREST*/)
//            {
//                shoudeDoSkill = true;
//                shoudeSkillIndex = skillIndex;
//                shoudeSkillData = skillData;
//                owner.aiMachine.lockedTarget = null;
//                owner.OnLockEnemy(null);
//                yield break;
//            }
//        }
//        else
//        {
//            skillIndex = firstSkillIndex;
//            skillData = firstSkillData;
//            skillLevel = firstSkillLevel;
//        }

//        bool firstLoop = true;

//        while (true)
//        {
//            if (firstLoop)
//            {
//                if (skillIndex != owner.mAttackSkillIndex)
//                {
//                    yield return new IdleRoutine(owner, AIParams.magicSkillDelay);
//                }

//                firstLoop = false;
//            }

//            // 向目标移动
//            float skillBounds = AIKit.GetSkillBounds(skillIndex) + target.mImpactRadius - 0.05f;
//            var move = new MoveTowardsRoutine(owner, target, skillBounds);

//            if (owner.GetCamp() == CommonParam.MonsterCamp)
//            {
//                yield return new Any(move, new Delay(AIParams.monsterKiteflyingTime));

//                // 如果怪物被放风筝超过一定时间，会尝试重新寻找目标
//                if (move.isBroken)
//                {
//                    owner.aiMachine.lockedTarget = null;
//                    owner.OnLockEnemy(null);
//                    yield break;
//                }
//            }
//            else 
//            {
//                yield return move;
//            }

//            if (!owner.IsEnemy(target))
//            {
//                owner.aiMachine.lockedTarget = null;
//                owner.OnLockEnemy(null);
//                yield break;
//            }

//            // 如果是普攻，确保普攻间隔满足要求
//            if (skillIndex == owner.mAttackSkillIndex)
//            {
//                float interval = Time.time - owner.aiMachine.lastPhysicalAttackTime;
//                float coolTime = owner.GetConfig("SKILL_COOLDOWN");
//                float realCoolTime = owner.GetFinalAttackTimeRatio() * coolTime;
//                float diff = realCoolTime - interval;

//                if (diff > 0.01f)
//                {
//                    owner.SetDirection(target.GetPosition());
//                    yield return new IdleRoutine(owner, diff);
//                }
//            }
//            else 
//            {
//                float interval = Time.time - owner.aiMachine.lastMagicSkillTime;
//                float diff = AIParams.magicSkillMinDeltaTime - interval;

//                if (diff > 0.01f)
//                {
//                    yield return new IdleRoutine(owner, diff);
//                }
//            }

//            if (!owner.IsEnemy(target))
//            {
//                owner.aiMachine.lockedTarget = null;
//                owner.OnLockEnemy(null);
//                yield break;
//            }

//            // 攻击目标
//            yield return new SkillRoutine(owner, target, skillIndex, skillLevel, skillData);

//            // 每次攻击完成后检查是否满足逃避条件
//            if (AIKit.ShouldEvade(owner))
//            {
//                // 若满足则设置标志后结束本协程
//                shoudeEvade = true;
//                //interuptEvent = "EVADE";
//                owner.aiMachine.lockedTarget = null;
//                owner.OnLockEnemy(null);
//                yield break;
//            }

//            if (!owner.IsEnemy(target))
//            {
//                owner.aiMachine.lockedTarget = null;
//                owner.OnLockEnemy(null);
//                yield break;
//            }

//            // 攻击间隔
//            float t = owner.GetConfig("SKILL_COOLDOWN");
            
//            if (t > 0.01f)
//            {
//                float realWaitTime = owner.GetFinalAttackTimeRatio() * t;
//                yield return new IdleRoutine(owner, realWaitTime);
//            }

//            if (!owner.IsEnemy(target))
//            {
//                owner.aiMachine.lockedTarget = null;
//                owner.OnLockEnemy(null);
//                yield break;
//            }

//            yield return null;
//            bool exceptFriendSkill = !owner.aiMachine.hasPhysicalAttacked || Time.time - owner.aiMachine.lastPhysicalAttackTime > AIParams.friendSkillEnableTime;
//            skillIndex = AIKit.GetPriSkillIndex(owner, AIKit.InAutoSkillBattle(), !owner.aiMachine.hasLastPhysicalAttacked, exceptFriendSkill, out skillLevel, out skillData);

//            //SEARCH_STRATEGY search = (SEARCH_STRATEGY)(int)DataCenter.mSkillConfigTable.GetData(skillIndex, "TARGET_CONDITION");

//            if (Skill.IsFriendSkill(skillIndex)/*search == SEARCH_STRATEGY.SELF || search == SEARCH_STRATEGY.SELF_LOWEST_HP || search == SEARCH_STRATEGY.FRIEND_NEAREST*/)
//            {
//                shoudeDoSkill = true;
//                shoudeSkillIndex = skillIndex;
//                shoudeSkillData = skillData;
//                owner.aiMachine.lockedTarget = null;
//                owner.OnLockEnemy(null);
//                yield break;
//            }
//        }
//    }

//    protected override void OnBreak()
//    {
//        owner.aiMachine.lockedTarget = null;
//        owner.OnLockEnemy(null);
//    }
//}


//public class FightRoutine : AIRoutine
//{
//    public FightRoutine(BaseObject owner)
//        : base(owner)
//    {
//        Bind(DoFight());
//    }

//    private IEnumerator DoFight()
//    {
//        int skillIndex = 0;
//        tLogicData skillData = null;

//        // 战斗主AI循环
//        while (true)
//        {
//            var p = new Protecter();

//            // 先试图锁定敌人
//            var tryLock = new TryLockTargetRoutine(owner, SEARCH_RANGE.AUTO, skillIndex, skillData);
//            skillIndex = 0;
//            skillData = null;
//            yield return tryLock;

//            if (owner.IsSameCamp(tryLock.lockedTarget))
//            {
//                // 如果是友方就释放技能
//                yield return new SkillRoutine(owner, tryLock.lockedTarget, tryLock.determinedSkillIndex, tryLock.determinedSkillLevel, tryLock.determinedSkillData);
//            }
//            else
//            {
//                // 如果是敌方就试图杀死敌人
//                var tryKill = new TryKillTargetRoutine(owner, tryLock.lockedTarget, tryLock.determinedSkillIndex, tryLock.determinedSkillLevel, tryLock.determinedSkillData);
//                yield return tryKill;

//                // 如果需要躲避
//                if (tryKill.shoudeEvade)
//                {
//                    // 执行躲避行为
//                    yield return new MoveToRoutine(owner, owner.GetPosition() + owner.GetDirection() * Vector3.back * AIParams.maxEvadeDistance);
//                    yield return new IdleRoutine(owner, 0.3f);
//                }
//                else if (tryKill.shoudeDoSkill)
//                {
//                    skillIndex = tryKill.shoudeSkillIndex;
//                    skillData = tryKill.shoudeSkillData;
//                }
//            }

//            yield return p;
//        }
//    }

//    protected override void OnBreak()
//    {
//        owner.ResetIdle();
//    }
//}


public class StayRoutine : AIRoutine
{
    public bool standstill { get; private set; }
    public string animName { get; private set; }

    public StayRoutine(BaseObject owner)
        : this(owner, null)
    { }

    public StayRoutine(BaseObject owner, string animName)
        : this(owner, animName, Mathf.Infinity)
    { }

    public StayRoutine(BaseObject owner, string animName, float time)
        : base(owner)
    {
        this.animName = animName;
        this.standstill = string.IsNullOrEmpty(animName) || animName == "0";
        Bind(DoStay(time));
    }

    private IEnumerator DoStay(float time)
    {
        ActiveObject obj = owner as ActiveObject;       
        float destTime = Time.time + time;

        while (Time.time < destTime)
        {
            if (standstill)
            {
                owner.SetStandStill(true);
            }
            else           
            {
                owner.PlayAnim(animName);
            }

            yield return new Delay(AIParams.ticks);
        }

        if (standstill)
        {
            owner.SetStandStill(false);
        }
    }

    protected override void OnBreak()
    {
        if (standstill)
        {
            owner.SetStandStill(false);
        }
    }
}


public class FearRoutine : AIRoutine
{
    public FearRoutine(BaseObject owner)
        : base(owner)
    {
        Bind(DoFear());
    }

    private IEnumerator DoFear()
    {
        var moveTo = new MoveToRoutine(owner, owner.GetPosition() + owner.GetDirection() * Vector3.back * AIParams.maxEvadeDistance);
        yield return new Any(new Delay(1.5f), moveTo);

        while (true)
        {
            float angle = UnityEngine.Random.Range(0f, 360f);
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.up);
            moveTo = new MoveToRoutine(owner, owner.GetPosition() + (q * owner.GetDirection()) * Vector3.back * AIParams.maxEvadeDistance);
            yield return new Any(new Delay(0.75f), moveTo);
            yield return null;
        }
    }
}


public class BeatBackRoutine : AIRoutine, IResponsable<InteruptIntent>
{
    public Vector3 destination { get; private set; }
    public float height { get; private set; }
    public float gravity { get; private set; }

    private Vector3 originPosition;

    public BeatBackRoutine(BaseObject owner, Vector3 destination, float height, float gravity)
        : base(owner)
    {
        this.destination = destination;
        this.height = height;
        this.gravity = gravity;
        Bind(DoBeatBack());
    }

    private bool CalculateValidDestination()
    {
        int tryCount = 4;
        Vector3 validPoint = Vector3.zero;

        while (tryCount > 0)
        {
            --tryCount;

            if (IsDestinationValid(destination, out validPoint))
            {
                destination = validPoint;
                return true;
            }
            else
            {
                destination = (owner.GetPosition() + destination) / 2f;
            }
        }

        return false;
    }

    private bool IsDestinationValid(Vector3 dest, out Vector3 realDest)
    {
        RaycastHit hit;
        NavMeshPath path;

        if (GuideKit.RaycastToObstruct(destination, out hit))
        {
            realDest = hit.point;
            var status = AIKit.CalculatePath(owner, hit.point, out path);
            return status == NavMeshPathStatus.PathComplete;
        }

        realDest = Vector3.zero;
        return false;
    }

    private IEnumerator DoBeatBack()
    {
        originPosition = owner.GetPosition();

        if (!owner.aiMachine.onBeatBack && CalculateValidDestination())
        {
            yield return new WaitUntilRoutineFinish(owner.StartRoutine(DoParabola()));          
        }
    }

    private IEnumerator DoParabola()
    {
        var agent = owner.mMainObject.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.enabled = false;
        }

        owner.aiMachine.onBeatBack = true;

        float y0 = owner.GetPosition().y;
        float y1 = destination.y;
        float len = height * 2f + y0 - y1;
        float d = Mathf.Sqrt(len * 2f / gravity);
        float v0 = (y1 - y0) / d + gravity * d / 2f;
        float t = 0f;

        while (t < d)
        {
            yield return null;
            t += Time.deltaTime;
            float factor = t / d;
            float x = Mathf.Lerp(originPosition.x, destination.x, factor);
            float z = Mathf.Lerp(originPosition.z, destination.z, factor);
            float y = y0 + v0 * t - gravity * t * t / 2f;
            owner.SetPosition(new Vector3(x, y, z));
        }

        owner.SetPosition(destination);

        if (agent != null)
        {
            agent.enabled = true;
        }

        owner.aiMachine.onBeatBack = false;
    }

    public int Response(InteruptIntent requester)
    {
        if (requester.name == "BeatBack")
        {
            return -1;
        }

        return 1;
    }
}


public class SkillLoop : AIRoutine
{
    private SkillIntentQueue intentQueue;

    public SkillLoop(BaseObject owner)
        : base(owner)
    {
        intentQueue = owner.aiMachine.skillIntentQueue;
        Bind(DoSkillLoop());
    }

    private IEnumerator DoSkillLoop()
    {
        while (true)
        {
            int frame = Time.frameCount;

            if (owner.aiMachine.fixEnemy != null && (!owner.IsEnemy(owner.aiMachine.fixEnemy) || (owner is Monster && Time.time - owner.aiMachine.lastAttackFixEnemyTime > AIParams.monsterKiteflyingTime)))
            {
                owner.aiMachine.fixEnemy = null;
            }

            intentQueue.Refresh();

            if (intentQueue.topIntent == null || !intentQueue.topIntent.isReady)
            {
                yield return new Routine(WaitIntentReady()).AppendWith(OnNotFoundTarget());
            }

            var intent = intentQueue.topIntent;

            if (!intent.inApplyRange)
            {
                float distance = AIKit.Distance(owner.GetPosition(), intent.skillTarget.GetPosition());
                float checkBounds = Mathf.Max(intent.skillBounds - 0.1f, distance / 2f);
                float t = Time.time;
                var r = new MoveTowardsRoutine(owner, intent.skillTarget, checkBounds);
                yield return new Conditional(r, () => !intent.skillTarget.IsDead() && Time.time - t < AIParams.maxPathRefreshTime);
            }

            if (intent.isReady && intent.inApplyRange)
            {
                if (intent.isNormalAttack)
                {
                    float interval = Time.time - owner.aiMachine.lastNormalAttackTime;
                    float coolTime = owner.GetConfig("SKILL_COOLDOWN");
                    float realCoolTime = owner.GetFinalAttackTimeRatio() * coolTime;
                    float diff = realCoolTime - interval;

                    if (diff > 0.01f)
                    {
                        owner.SetDirection(intent.skillTarget.GetPosition());
                        yield return new IdleRoutine(owner, diff);
                    }
                }
                else
                {
                    float interval = Time.time - owner.aiMachine.lastSkillTime;
                    float diff = AIParams.magicSkillMinDeltaTime - interval;

                    if (diff > 0.01f)
                    {
                        if (owner != intent.skillTarget)
                        {
                            owner.SetDirection(intent.skillTarget.GetPosition());
                        }

                        yield return new IdleRoutine(owner, diff);
                    }
                }

                yield return new SkillRoutine(owner, intent.skillTarget, intent.skillIndex, intent.skillLevel, intent.skillButtonData);

                if (AIKit.ShouldEvade(owner))
                {
                    yield return new MoveToRoutine(owner, owner.GetPosition() + owner.GetDirection() * Vector3.back * AIParams.maxEvadeDistance);
                    yield return new IdleRoutine(owner, 0.3f);
                }
            }

            if (Time.frameCount == frame)
                yield return null;
        }
    }

    protected override void OnBreak()
    {
        owner.ResetIdle();
    }

    private IEnumerator WaitIntentReady()
    {
        while (intentQueue.topIntent == null || !intentQueue.topIntent.isReady)
        {
            yield return new Delay(0.5f);
            intentQueue.Refresh();
        }
    }

    private IEnumerator OnNotFoundTarget()
    {
        BaseObject master = owner.GetOwnerObject();

        if (master != null && master is Character)
        {
            var followLoc = new FollowLocation(master, owner.mFollowOffset);
            yield return new Any(new IdleRoutine(owner, 1f), new WaitUntil(() => NeedMove(master, followLoc)));
            yield return new FollowRoutine(owner, master, owner.mFollowOffset, owner.mFollowPos <= 0 || owner.mFollowPos > 8);
        }
        else
        {
            yield return new IdleRoutine(owner);
        }
    }

    private bool NeedMove(BaseObject master, FollowLocation followLoc)
    {
        if (master.GetVelocity().sqrMagnitude < 0.1f)
        {
            return false;
        }

        var dir = owner.GetPosition() - followLoc.GetPosition();
        float cos = Vector3.Dot(master.GetVelocity().normalized, dir.normalized);
        return cos < 0f;
    }
}