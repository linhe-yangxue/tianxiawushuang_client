using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;
using DataTable;


// 视野定义：技能释放目标的搜寻范围，以自己或主人为中心的圆形区域
// 视野种类：局部（由"LOOKBOUND"字段配置视野半径），全局（整个战场）
// 
// 主角：在PVE非自动战斗且未点击技能按钮的情况下——局部 其他——全局
// 宠物：PVP&BOSS——全局 PVE——主人或自身为中心的局部视野；
// 怪物：PVP&BOSS——全局 PVE——主人或自身为中心的局部视野；
// 所有对象在锁定攻击状态下视野是局部的

// 技能队列默认优先级
// 第一优先级：可用 > 不可用
// 第二优先级：魔法 > 物理（普攻）
// 第三优先级：行进到可施法范围的距离近 > 行进到可施法范围的距离远

// 目标锁定的概念：当有单位被锁定时，将只会对此单位进行普攻，即使中途对其他单位释放了魔法技能，释放完毕后依然会对此单位普攻，直到锁定解除
//
// 被锁定目标的判定：手动点击敌方目标（集火）或者在自发状态下普攻的目标
//
// 解除当前锁定的条件：
// 1 被锁定目标死亡或被魅惑改变阵营，解除锁定态
// 2 集火其他目标，锁定目标会切换至新的目标
// 3 如果是主角被玩家点击技能按钮且技能可用，解除锁定态
// 4 如果是主角被玩家点击地面移动，当移动到目的地时如果被锁定目标超出局部视野，解除锁定态
// 5 如果是怪物，则被放风筝超过指定时间，解除锁定态

// 技能可释放的条件
//
// 手动释放条件下：
// 1 CD冷却完成
// 2 魔法量足够
// 3 技能目标在视野内存在

// 自动释放条件下，除了必须满足手动释放的所有条件（强条件）外，还需要额外满足以下条件（弱条件）
// 1 如果是主角可手动释放的魔法技能，只有在自动技能模式下会自动释放，并且若此技能是友方技能，只能在普攻后的一定时间内可用
// 2 其他魔法技能必须对锁定目标进行一次普攻后再释放
// 3 需要满足技能表"SKILL_CONDITION"字段配置的条件

// 技能目标优先级 (优先级中含全局的只在全局视野下发挥作用)
// DEFAULT = 0,                 1 局部锁定目标  2 局部敌方距离最近（或局部优先级最高敌方）  3 全局敌方最近
// SELF = 1,                    1 自己
// ENEMY_NEAREST = 2,           1 局部敌方最近  2 全局敌方最近
// FRIEND_NEAREST = 3,          1 局部友方最近  2 全局友方最近 
// ENEMY_LOWEST_HP = 4,         1 局部敌方血量最少  2 全局敌方最近
// SELF_LOWEST_HP = 5,          1 局部友方血量最少  2 全局友方最近
// ENEMY_LOWEST_HP_RATE = 6,    1 局部敌方剩余血量比例最少  2 全局敌方最近
// SELF_LOWEST_HP_RATE = 7,     1 局部友方剩余血量比例最少  2 全局友方最近
// ENEMY_CHARACTER = 8,         1 局部敌方最近主角  2 局部敌方最近  3 全局敌方最近
// SELF_CHARACTER = 9,          1 局部友方最近主角  2 局部友方最近  3 全局友方最近
// ENEMY_DEFENSIVE = 10,        1 局部敌方最近防御型  2 局部敌方最近  3 全局敌方最近
// SELF_DEFENSIVE = 11,         1 局部友方最近防御型  2 局部友方最近  3 全局友方最近
// ENEMY_OFFENSIVE = 12,        1 局部敌方最近进攻型  2 局部敌方最近  3 全局敌方最近
// SELF_OFFENSIVE = 13,         1 局部友方最近进攻型  2 局部友方最近  3 全局友方最近
// ENEMY_ASSISTANT = 14,        1 局部敌方最近辅助型  2 局部敌方最近  3 全局敌方最近
// SELF_ASSISTANT = 15,         1 局部友方最近辅助型  2 局部友方最近  3 全局友方最近

public class AIParams
{
    //public static readonly float autoBattleTimeScale = 2f;      // 自动战斗速度倍率（现在通过读表获取）

    public static readonly float maxPathRefreshTime = 1.0f;         // 最大路径刷新间隔
    public static readonly float followRadius = 3f;                 // 宠物跟随半径
    public static readonly float maxEvadeDistance = 5f;             // 逃避距离  
    public static readonly float evadeCheckBounds = 2f;             // 逃避触发检测半径
    public static readonly float monsterKiteflyingTime = 2f;        // 怪物最大放风筝时间
    public static readonly float battleIndicateDistance = 12f;      // 手动战斗中当最近的怪物距离主角大于此距离，显示指示箭头
    public static readonly float friendSkillEnableTime = 10f;       // 辅助性技能可释放时间（从最近一次普攻起算）
    public static readonly float followAdditionalMoveSpeed = 3f;  // 跟随附加速度
    public static readonly float followSpeedBaseValue = 1.5f;       // 跟随基础速度
    public static readonly float followSpeedCoeff = 1f;             // 跟随速度系数
    // 跟随速度 = min(宠物基础速度 + 附加速度, 跟随基础速度 + 跟随速度系数 * 与跟随目标点的距离差)

    public static readonly float magicSkillDelay = 0.5f;            // 施法延时
    public static readonly float magicSkillMinDeltaTime = 0.5f;     // 最小施法间隔

    public static readonly float ticks = 0.1f;
    public static readonly float minBounds = 0.1f;
    public static readonly float minPathRefreshBounds = 1f;


    //static AIParams()
    //{
    //    string strAutoSpeed = DataCenter.mGlobalConfig.GetData("AUTO_BATTLE_SPEED", "VALUE");

    //    if (!string.IsNullOrEmpty(strAutoSpeed))
    //    {
    //        autoBattleTimeScale = float.Parse(strAutoSpeed);
    //    }
    //    else 
    //    {
    //        autoBattleTimeScale = 1f;
    //    }
    //}
}


// 战斗定位
public enum OBJECT_FIGHT_TYPE
{   
    DEFENSIVE, // 防御型
    OFFENSIVE, // 进攻型
    ASSISTANT, // 辅助型
    UNDEFINED, // 未定义
}


// 搜索范围
public enum SEARCH_RANGE
{
    ONLY_VISIBLE,
    GLOBAL,
    AUTO
}


// Buff目标类型
public enum BUFF_TARGET_TYPE
{
    DEFAULT = 0,            // 默认，技能目标作为Buff目标
    SELF = 1,               // 自己
    ENEMY_NEAREST = 2,      // 敌方最近 
    FRIEND_NEAREST = 3,     // 友方最近  
    ENEMY_LOWEST_HP = 4,    // 敌方血量最少
    SELF_LOWEST_HP = 5,     // 友方血量最少   
    ENEMY_RAND = 6,         // 敌方随机单体
    SELF_RAND = 7,          // 友方随机单体    
    ENEMY_ALL = 8,          // 敌方所有
    SELF_ALL = 9,           // 友方所有
}


// 寻找目标策略
public enum SEARCH_STRATEGY
{
    DEFAULT = 0,            // 默认，优先级最高敌人
    SELF = 1,               // 自己
    ENEMY_NEAREST = 2,      // 敌方最近 
    FRIEND_NEAREST = 3,     // 友方最近  
    ENEMY_LOWEST_HP = 4,    // 敌方血量最少
    SELF_LOWEST_HP = 5,     // 友方血量最少  
    ENEMY_LOWEST_HP_RATE = 6,   // 敌方剩余血量比例最少
    SELF_LOWEST_HP_RATE = 7,    // 友方剩余血量比例最少
    ENEMY_CHARACTER = 8,        // 敌方主角
    SELF_CHARACTER = 9,         // 友方主角
    ENEMY_DEFENSIVE = 10,       // 敌方防御型
    SELF_DEFENSIVE = 11,        // 友方防御型
    ENEMY_OFFENSIVE = 12,       // 敌方进攻型
    SELF_OFFENSIVE = 13,        // 友方进攻型
    ENEMY_ASSISTANT = 14,       // 敌方辅助型
    SELF_ASSISTANT = 15,        // 友方辅助型
}


public interface ILocation
{
    Vector3 GetPosition();
    Quaternion GetDirection();
}


public class FollowLocation : ILocation
{
    private BaseObject target;
    private Vector3 offset;
    private float predictTime = 0f;

    public bool absolute { get; set; }

    public FollowLocation(BaseObject target, Vector3 offset)
        : this(target, offset, 0f)
    { }

    public FollowLocation(BaseObject target, Vector3 offset, float predictTime)
    {
        this.target = target;
        this.offset = offset;
        this.predictTime = predictTime;
        this.absolute = false;
    }

    public Vector3 GetPosition()
    {
        if (absolute)
        {
            return target.GetPosition() + offset + target.GetVelocity() * predictTime;
        }
        else
        {
            return target.GetPosition() + target.GetDirection() * offset + target.GetVelocity() * predictTime;
        }
    }

    public Quaternion GetDirection()
    {
        return target.GetDirection();
    }
}


public class AIKit
{
    /// <summary>
    /// 是否触发概率事件
    /// </summary>
    /// <param name="prob"> 触发概率 </param>
    /// <returns> 是否触发 </returns>
    public static bool Triggered(float prob)
    {
        return UnityEngine.Random.Range(0f, 1f) < prob;
    }

    public static float Distance(Vector3 from, Vector3 to)
    {
        return Mathf.Sqrt((to.x - from.x) * (to.x - from.x) + (to.z - from.z) * (to.z - from.z));
    }

    public static float SqrDistance(Vector3 from, Vector3 to)
    {
        return (to.x - from.x) * (to.x - from.x) + (to.z - from.z) * (to.z - from.z);
    }

    public static float GetSkillBounds(int skillIndex)
    {
        if (skillIndex > 0)
        {
            float dis = SkillGlobal.GetInfo(skillIndex).distance;

            if (dis > 0.0001f)
            {
                return dis;
            }
        }

        return 1.0f;
    }

    public static bool InBounds(Vector3 from, Vector3 to, float bounds)
    {
        return bounds >= 0 && (to.x - from.x) * (to.x - from.x) + (to.z - from.z) * (to.z - from.z) <= bounds * bounds;
    }

    public static bool InBounds(ILocation from, Vector3 to, float bounds)
    {
        return InBounds(from.GetPosition(), to, bounds);
    }

    public static bool InBounds(Vector3 from, ILocation to, float bounds)
    {
        return InBounds(from, to.GetPosition(), bounds);
    }

    public static bool InBounds(ILocation from, ILocation to, float bounds)
    {
        return InBounds(from.GetPosition(), to.GetPosition(), bounds);
    }

    public static BaseObject FindLowestHpTarget(BaseObject owner, float range, bool isEnemy)
    {
        int lowestHp = 99999999;
        BaseObject result = null;

        Vector3 pos = owner.GetPosition();
        var objMap = ObjectManager.Self.mObjectMap;

        for (int i = objMap.Length - 1; i >= 0; --i)
        {
            var obj = objMap[i];

            if (obj != null
                && !obj.IsDead()
                && (owner.IsSameCamp(obj) ^ isEnemy)
                && AIKit.InBounds(pos, obj, range))
            {
                int hp = obj.GetHp();

                if (hp < lowestHp)
                {
                    lowestHp = hp;
                    result = obj;
                }
            }
        }

        return result;
    }

    public static BaseObject FindLowestHpRateTarget(BaseObject owner, float range, bool isEnemy)
    {
        float lowestHpRate = 10f;
        BaseObject result = null;

        Vector3 pos = owner.GetPosition();
        var objMap = ObjectManager.Self.mObjectMap;

        for (int i = objMap.Length - 1; i >= 0; --i)
        {
            var obj = objMap[i];

            if (obj != null
                && !obj.IsDead()
                && (owner.IsSameCamp(obj) ^ isEnemy)
                && AIKit.InBounds(pos, obj, range))
            {
                float hpRate = obj.GetHitRate();

                if (hpRate < lowestHpRate)
                {
                    lowestHpRate = hpRate;
                    result = obj;
                }
            }
        }

        return result;
    }

    public static BaseObject FindRandTarget(BaseObject owner, float range, bool isEnemy)
    {
        var targetArray = ObjectManager.Self.FindAllAlived(x => (owner.IsSameCamp(x) ^ isEnemy) && AIKit.InBounds(owner, x, range));
        int len = targetArray.Length;

        if (len > 0)
        {
            int index = UnityEngine.Random.Range(0, len);
            return targetArray[index];
        }

        return null;
    }

    public static NavMeshPathStatus CalculatePath(BaseObject owner, Vector3 targetPos, out NavMeshPath path)
    {
        NavMeshAgent navAgent = owner.mMainObject.GetComponent<NavMeshAgent>();

        if (navAgent != null && navAgent.enabled)
        {
            path = new NavMeshPath();

            if (navAgent.CalculatePath(targetPos, path))
            {
                return path.status;
            }
        }

        path = null;
        return NavMeshPathStatus.PathInvalid;
    }

    //public static bool InPVE()
    //{
    //    return MainProcess.mStage != null && MainProcess.mStage.IsPVE();
    //}

    //public static bool InPVP4()
    //{
    //    return MainProcess.mStage != null && MainProcess.mStage.IsPVP();
    //}

    //public static bool InBossBattle()
    //{
    //    return MainProcess.mStage != null && MainProcess.mStage.IsBOSS();
    //}

    //public static bool InAutoBattle()
    //{
    //    //return MainProcess.mStage != null && 
    //    //    (
    //    //        MainProcess.mStage.IsPVP() || MainProcess.mStage.IsBOSS()
    //    //        || (MainProcess.mStage.IsPVE() && PVEStageBattle.mBattleControl != BATTLE_CONTROL.MANUAL)
    //    //        || (MainProcess.mStage.IsBOSS() && BossBattle.mBattleControl != BATTLE_CONTROL.MANUAL)
    //    //    );
    //    return MainProcess.mStage != null && MainProcess.mStage.GetBattleControl() != BATTLE_CONTROL.MANUAL;
    //}

    ////public static bool InAutoAttackBattle()
    ////{
    ////    return MainProcess.mStage != null &&
    ////        (
    ////            (MainProcess.mStage.IsPVE() && PVEStageBattle.mBattleControl == BATTLE_CONTROL.AUTO_ATTACK)
    ////            || (MainProcess.mStage.IsBOSS() && BossBattle.mBattleControl == BATTLE_CONTROL.AUTO_ATTACK)
    ////        );
    ////}

    //public static bool InAutoSkillBattle()
    //{
    //    //return MainProcess.mStage != null &&
    //    //    (
    //    //        MainProcess.mStage.IsPVP() || MainProcess.mStage.IsBOSS()
    //    //        || (MainProcess.mStage.IsPVE() && PVEStageBattle.mBattleControl == BATTLE_CONTROL.AUTO_SKILL)
    //    //        || (MainProcess.mStage.IsBOSS() && BossBattle.mBattleControl == BATTLE_CONTROL.AUTO_SKILL)
    //    //    );
    //    return MainProcess.mStage != null && MainProcess.mStage.GetBattleControl() == BATTLE_CONTROL.AUTO_SKILL;
    //}

    //public static bool SearchOnlyVisible(BaseObject owner)
    //{
    //    return MainProcess.mStage.IsBOSS()
    //        || (MainProcess.mStage.IsPVE() && (PVEStageBattle.mBattleControl == BATTLE_CONTROL.MANUAL || !(owner is Character)));
    //}

    //public static int GetPriSkillIndex(BaseObject owner, bool autoBattle, bool exceptSpecialAttack, bool exceptFriendSkill, out int skillLevel, out tLogicData skillButtonData)
    //{
    //    skillButtonData = null;
    //    bool autoSkill = autoBattle;
    //    OBJECT_TYPE objectType = owner.GetObjectType();

    //    if (objectType == OBJECT_TYPE.MONSTER || objectType == OBJECT_TYPE.MONSTER_BOSS || objectType == OBJECT_TYPE.BIG_BOSS)
    //    {
    //        autoSkill = true;
    //    }

    //    foreach (var looper in owner.CoolDownedSkillCDLoopers())
    //    {
    //        if (looper.skillPos == SKILL_POS.SPECIAL_ATTACK && !exceptSpecialAttack && owner.CanDoSkill(looper.skillIndex) && !(exceptFriendSkill && looper.isFriendSkill) && CanDoSkillByCondition(owner, looper.skillIndex, looper.isFriendSkill))
    //        {
    //            skillLevel = looper.skillLevel;
    //            return looper.skillIndex;
    //        }

    //        if (looper.skillPos != SKILL_POS.SPECIAL_ATTACK && autoSkill && owner.CanDoSkill(looper.skillIndex) && !(exceptFriendSkill && looper.isFriendSkill) && CanDoSkillByCondition(owner, looper.skillIndex, looper.isFriendSkill))
    //        {
    //            Character c = owner as Character;

    //            if (/*looper.usePos < 0 || */c == null)
    //            {
    //                skillLevel = looper.skillLevel;
    //                return looper.skillIndex;
    //            }

    //            if (looper.skillPos == SKILL_POS.CHAR_MAIN)
    //            {
    //                if (c.GetObjectType() == OBJECT_TYPE.CHARATOR)
    //                {
    //                    skillButtonData = GetSkillButtonData(0);
    //                }

    //                skillLevel = looper.skillLevel;
    //                return looper.skillIndex;
    //            }

    //            BaseObject f = c.mFriends[(int)looper.skillPos - 1];

    //            if (f != null)// && !f.IsDead())
    //            {
    //                if (c.GetObjectType() == OBJECT_TYPE.CHARATOR)
    //                {
    //                    skillButtonData = GetSkillButtonData((int)looper.skillPos);
    //                }

    //                skillLevel = looper.skillLevel;
    //                return looper.skillIndex;
    //            }  
    //        }
    //    }

    //    skillLevel = owner.GetAttackSkillLevel();
    //    return owner.mAttackSkillIndex;
    //}

    //public SkillIntent GetSkillIntent(BaseObject owner, bool includeMagicSkill, bool global)
    //{
    //    return null;
    //}

    public static bool CanDoSkillByCondition(BaseObject owner, int skillIndex, bool isFriendSkill)
    {
        ConfigParam param = Skill.GetSkillConditionParam(skillIndex);

        if (param == null)
            return true;

        float hpRate = param.GetFloat("hp_max_rate", 10f);

        if (hpRate < 1f)
        {
            BaseObject target = AIKit.FindLowestHpRateTarget(owner, owner.LookBounds(), !isFriendSkill);
            return target != null && target.GetHpRate() <= hpRate;
        }

        return true;
    }

    public static SkillButtonData GetSkillButtonData(int usePos)
    {
        if (usePos >= 0 && usePos <= 4)
        {
            tLogicData skillData = DataCenter.GetData("SKILL_UI");
            return skillData.getData("do_skill_" + usePos) as SkillButtonData;
        }

        return null;
    }

    public static float GetNormalAttackDistance(BaseObject owner)
    {
        return GetSkillBounds(owner.mAttackSkillIndex);
    }

    public static bool ShouldEvade(BaseObject owner)
    {
        if (owner is Character)
        {
            Character c = (Character)owner;
            Vector3 pos = c.GetPosition();

            if (c.GetAlivedPet() != null)
            {
                return ObjectManager.Self.FindAlived(x => !owner.IsSameCamp(x) && InBounds(pos, x, AIParams.evadeCheckBounds) && x.aiMachine.fixEnemy != owner) != null;
            }
        }

        return false;
    }

    public static Vector3 GetFollowOffset(int standPos)
    {
        float angle = 0f;

        if (standPos <= 0)
        {
            angle = UnityEngine.Random.Range(0f, 360f);
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * AIParams.followRadius;
        }
        else if (standPos <= 8)
        {
            switch (standPos)
            {
                case 1: angle = 5f; break;
                case 2: angle = 3f; break;
                case 3: angle = 7f; break;
                case 4: angle = 1f; break;
                case 5: angle = -5f; break;
                case 6: angle = -3f; break;
                case 7: angle = -7f; break;
                case 8: angle = -1f; break;
            }

            angle *= Mathf.PI / 8f;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * AIParams.followRadius;
        }
        else 
        {
            int m = (standPos - 1) / 8;
            int n = (standPos - 1) % 8;
            angle = Mathf.PI / 4f * n;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (AIParams.followRadius + m);
        }
    }

    // 查找范围分为可视范围和全局
    // 如果没有主人，总是先从可视范围内查找满足条件的目标
    // 如果不能找到目标，或者处于无目标状态，或者寻找全局的最近目标
    // 
    // 如果有主人，则在以主人为中心的可视范围内查找满足条件的目标，若查找不到，处于无目标状态
    // 
    //      情形			        首要查找策略	        查找失败后续策略
    // 
    // 主角手动战斗，点击Skill	自身可视范围	        全局范围最近目标
    // 主角自动战斗		        自身可视范围	        全局范围最近目标
    // 
    // 主角手动战斗，Idle态	    自身可视范围	        无目标
    // 怪物无主人		        自身可视范围	        无目标
    // 
    // 宠物			            主人可视范围	        无目标
    // 怪物有主人		        主人可视范围	        无目标	
    public static BaseObject SearchTarget(BaseObject owner, SEARCH_STRATEGY searchStrategy)
    {
        BaseObject target = null;
        BaseObject center = owner.aiMachine.searchTargetCenter;
        bool global = owner.aiMachine.searchTargetInGlobal;

        switch (searchStrategy)
        {
            case SEARCH_STRATEGY.DEFAULT:
                target = owner.aiMachine.fixEnemy;
                
                if (target == null || !owner.IsEnemy(target))
                {
                    target = owner.SearchEnemyByDefaultStrategy();
                }
             
                break;

            case SEARCH_STRATEGY.SELF:
                target = owner;
                break;

            case SEARCH_STRATEGY.FRIEND_NEAREST:
                target = center.FindNearestAlived(x => center.IsSameCamp(x));

                if (target == null)
                    target = center;

                break;

            case SEARCH_STRATEGY.ENEMY_NEAREST:
                target = center.FindNearestEnemy(global);
                break;

            case SEARCH_STRATEGY.ENEMY_LOWEST_HP:
                target = FindLowestHpTarget(center, center.LookBounds(), true);

                if (target == null && global)
                    target = center.FindNearestEnemy(true);

                break;

            case SEARCH_STRATEGY.SELF_LOWEST_HP:
                target = FindLowestHpTarget(center, center.LookBounds(), false);

                if (target == null)
                    target = center;

                break;

            case SEARCH_STRATEGY.ENEMY_LOWEST_HP_RATE:
                target = FindLowestHpRateTarget(center, center.LookBounds(), true);

                if(target == null && global)
                    target = center.FindNearestEnemy(true);

                break;

            case SEARCH_STRATEGY.SELF_LOWEST_HP_RATE:
                target = FindLowestHpRateTarget(center, center.LookBounds(), false);

                if (target == null)
                    target = center;

                break;

            case SEARCH_STRATEGY.ENEMY_CHARACTER:
                target = center.FindNearestAlived(x => x is Character && !center.IsSameCamp(x) && center.InLookBounds(x));

                if(target == null)
                    target = center.FindNearestEnemy(global);

                break;

            case SEARCH_STRATEGY.SELF_CHARACTER:
                target = center.FindNearestAlived(x => x is Character && center.IsSameCamp(x) && center.InLookBounds(x));

                if (target == null)
                    target = center;

                break;

            case SEARCH_STRATEGY.ENEMY_DEFENSIVE:
                target = center.FindNearestAlived(x => x.fightintType == OBJECT_FIGHT_TYPE.DEFENSIVE && !center.IsSameCamp(x) && center.InLookBounds(x));

                if(target == null)
                    target = center.FindNearestEnemy(global);

                break;

            case SEARCH_STRATEGY.SELF_DEFENSIVE:
                target = center.FindNearestAlived(x => x.fightintType == OBJECT_FIGHT_TYPE.DEFENSIVE && center.IsSameCamp(x) && center.InLookBounds(x));

                if (target == null)
                    target = center;

                break;

            case SEARCH_STRATEGY.ENEMY_OFFENSIVE:
                target = center.FindNearestAlived(x => x.fightintType == OBJECT_FIGHT_TYPE.OFFENSIVE && !center.IsSameCamp(x) && center.InLookBounds(x));

                if (target == null)
                    target = center.FindNearestEnemy(global);

                break;

            case SEARCH_STRATEGY.SELF_OFFENSIVE:
                target = center.FindNearestAlived(x => x.fightintType == OBJECT_FIGHT_TYPE.OFFENSIVE && center.IsSameCamp(x) && center.InLookBounds(x));

                if (target == null)
                    target = center;

                break;

            case SEARCH_STRATEGY.ENEMY_ASSISTANT:
                target = center.FindNearestAlived(x => x.fightintType == OBJECT_FIGHT_TYPE.ASSISTANT && !center.IsSameCamp(x) && center.InLookBounds(x));

                if (target == null)
                    target = center.FindNearestEnemy(global);

                break;

            case SEARCH_STRATEGY.SELF_ASSISTANT:
                target = center.FindNearestAlived(x => x.fightintType == OBJECT_FIGHT_TYPE.ASSISTANT && center.IsSameCamp(x) && center.InLookBounds(x));

                if (target == null)
                    target = center;

                break;
        }

        return target;
    }

    // skillPos = 0, 1, 2, 3
    public static int GetCharacterSkillLevel(int skillPos)
    {
        if (Character.Self == null || skillPos < 0)
        {
            return 0;
        }

        if (skillPos == 0)
        {
            return Character.Self.GetConfigSkillLevel(0);
        }
        else
        {
            BaseObject[] friends = Character.Self.mFriends;

            if (friends != null && friends.Length > skillPos && friends[skillPos] != null)
            {
                return friends[skillPos].GetConfigSkillLevel(0);
            }
        }

        return 0;
    }
}

public class CDLooper : RoutineBlock
{
    public bool isCoolDown { get; private set; }
    public float cdTime { get; set; }
    public float timeScale { get; set; }
    public float rate { get; set; } // 0 -> 1
    public bool paused { get; set; }

    public event Action onCoolDown;

    private IRoutine doCDRoutine;

    public CDLooper(float cdTime)
    {
        this.cdTime = cdTime;
        this.isCoolDown = true;
        this.timeScale = 1f;
        this.rate = 1f;
        this.paused = false;
    }

    public void Restart()
    {
        if (Routine.IsActive(doCDRoutine))
        {
            doCDRoutine.Break();
        }

        doCDRoutine = Append(DoCD());
    }

    public void CooldownImmediate()
    {
        if (Routine.IsActive(doCDRoutine))
        {
            doCDRoutine.Break();
            rate = 1f;
            isCoolDown = true;

            if (onCoolDown != null)
            {
                onCoolDown();
            }
        }
    }

    private IEnumerator DoCD()
    {
        rate = 0f;
        isCoolDown = false;

        while (rate < 1f)
        {
            if (cdTime < 0.001f)
            {
                break;
            }

            yield return null;

            if (!paused)
            {
                rate += (Time.deltaTime * timeScale) / cdTime;
            }
        }

        rate = 1f;
        isCoolDown = true;

        if (onCoolDown != null)
        {
            onCoolDown();
        }
    }
}

public class SkillCDLooper : CDLooper
{
    public int skillIndex { get; private set; }
    public SKILL_POS skillPos { get; set; }
    public bool isFriendSkill { get; private set; }
    public int skillLevel { get; set; }

    public SkillCDLooper(int index)
        : base(1f)
    {
        DataRecord r = DataCenter.mSkillConfigTable.GetRecord(index);
        this.skillIndex = index;

        if (r == null)
        {
            DEBUG.LogError("Skill " + index + " does not exist!");
        }
        else
        {
            this.cdTime = (float)r["CD_TIME"];
            this.skillPos = SKILL_POS.SPECIAL_ATTACK;
            this.isFriendSkill = Skill.IsFriendSkill(r);
        }
    }
}


public class SkillIntent
{
    public BaseObject owner { get; private set; }
    public int skillIndex { get; private set; }
    public int skillLevel { get; private set; }
    public tLogicData skillButtonData { get; private set; }
    public bool available { get; set; }
    public SkillCDLooper cdLooper { get; private set; }
    //public bool searchTargetInGlobal { get; set; }
    public DataRecord skillConfig { get; private set; }
    public bool isNormalAttack { get; private set; }
    public bool isFriendSkill { get; private set; }
    public SEARCH_STRATEGY searchStrategy { get; private set; }
    public BaseObject skillTarget { get; set; }
    public float skillBounds { get; private set; }

    public bool isReady
    {
        get { return available && skillTarget != null && !skillTarget.IsDead(); }
    }

    public bool inApplyRange
    {
        get 
        {
            if (skillTarget != null)
            {
                float weakBounds = skillBounds + skillTarget.mImpactRadius - 0.05f;
                return AIKit.SqrDistance(owner.GetPosition(), skillTarget.GetPosition()) <= weakBounds * weakBounds;
            }

            return false;
        }
    }

    public SkillIntent(BaseObject owner, SkillCDLooper cdLooper)
        : this(owner, cdLooper.skillIndex, cdLooper.skillLevel)
    {
        this.cdLooper = cdLooper;

        if ((int)cdLooper.skillPos > 0 && owner.GetObjectType() == OBJECT_TYPE.CHARATOR)
        {
            skillButtonData = AIKit.GetSkillButtonData((int)cdLooper.skillPos);
        }
    }

    public SkillIntent(BaseObject owner, int skillIndex, int skillLevel)
    {
        this.owner = owner;
        this.skillIndex = skillIndex;
        this.skillLevel = skillLevel;
        skillConfig = DataCenter.mSkillConfigTable.GetRecord(skillIndex);

        if (skillConfig != null)
        {
            isNormalAttack = skillConfig["ATTACK_SKILL"] == 1;
            searchStrategy = (SEARCH_STRATEGY)(int)skillConfig["TARGET_CONDITION"];
            isFriendSkill = Skill.IsFriendSkill(searchStrategy);
            skillBounds = AIKit.GetSkillBounds(skillIndex);
        }
    }

    public bool RefreshAvailable(bool includeWeakCondition)
    {
        if (skillConfig == null)
        {
            available = false;
        }
        else if (includeWeakCondition)
        {
            available = (cdLooper == null || (cdLooper.isCoolDown && ((int)cdLooper.skillPos < 0 || MainProcess.mStage.GetBattleControl() == BATTLE_CONTROL.AUTO_SKILL)))
                && owner.CanDoSkill(skillIndex)
                && (isNormalAttack || (owner.aiMachine.hasNormalAttacked && Time.time - owner.aiMachine.lastNormalAttackTime < AIParams.friendSkillEnableTime))
                && AIKit.CanDoSkillByCondition(owner, skillIndex, isFriendSkill);
        }
        else
        {
            available = (cdLooper == null || cdLooper.isCoolDown)
                && owner.CanDoSkill(skillIndex);
        }

        return available;
    }

    public bool TrySearchTarget()
    {
        skillTarget = available ? AIKit.SearchTarget(owner, searchStrategy) : null;
        return skillTarget != null;
    }
}


// SkillIntent队列的若干种状态汇总
// 1 自由非锁定态             技能按照默认视野，默认优先级排序
// 2 自由锁定态               技能按照局部视野，默认优先级排序
// 3 集火第一次普攻前         普攻赋予集火目标并提升至顶部
// 4 手动释放技能前           被释放技能全局视野并提升至顶部
public class SkillIntentQueue
{
    public enum Status
    {
        FreeUnlock = 0,
        FreeLock = 1,
        FixTarget = 2,
        FixSkill = 3,
    }

    public BaseObject owner { get; private set; }
    public bool resetFlag { get; set; }

    private List<SkillIntent> intentQueue = new List<SkillIntent>();

    public SkillIntent topIntent
    {
        get { return intentQueue.Count > 0 ? intentQueue[0] : null; }
    }

    public SkillIntentQueue(BaseObject owner)
    {
        this.owner = owner;
    }

    public void Reset()
    {
        intentQueue.Clear();

        using (var iter = owner.AllSkillCDLoopers().GetEnumerator())
        {
            while (iter.MoveNext())
            {
                intentQueue.Add(new SkillIntent(owner, iter.Current));
            }
        }

        intentQueue.Add(new SkillIntent(owner, owner.mAttackSkillIndex, 1));
    }

    public void Refresh()
    {
        if (resetFlag)
        {
            Reset();
            resetFlag = false;
        }

        if (owner.aiMachine.fixSkill > 0)
        {
            int index = intentQueue.FindIndex(x => x.skillIndex == owner.aiMachine.fixSkill);

            if (index >= 0)
            {
                if (intentQueue[index].RefreshAvailable(false))
                {
                    intentQueue[index].TrySearchTarget();
                }

                RaiseToTop(index);
            }
            else
            {
                for (int i = intentQueue.Count - 1; i >= 0; --i)
                {
                    intentQueue[i].available = false;
                }
            }
        }
        else if (owner.aiMachine.fixEnemy != null)
        {
            int index = intentQueue.FindIndex(x => x.isNormalAttack);

            if (index >= 0)
            {
                if (intentQueue[index].RefreshAvailable(true))
                {
                    intentQueue[index].skillTarget = owner.aiMachine.fixEnemy;
                }

                if (owner.aiMachine.fixEnemy != owner.aiMachine.lastNormalAttackTarget)
                {
                    RaiseToTop(index);
                }
                else 
                {
                    for (int i = intentQueue.Count - 1; i >= 0; --i)
                    {
                        if (i != index && intentQueue[i].RefreshAvailable(true))
                        {
                            intentQueue[i].TrySearchTarget();
                        }
                    }

                    intentQueue.Sort(CompareIntent);
                }
            }
            else
            {
                for (int i = intentQueue.Count - 1; i >= 0; --i)
                {
                    intentQueue[i].available = false;
                }
            }
        }
        else
        {
            for (int i = intentQueue.Count - 1; i >= 0; --i)
            {
                if (intentQueue[i].RefreshAvailable(true))
                {
                    intentQueue[i].TrySearchTarget();
                }
            }

            intentQueue.Sort(CompareIntent);
        }
    }

    private void RaiseToTop(int index)
    {
        var targetIntent = intentQueue[index];
        intentQueue.RemoveAt(index);
        intentQueue.Insert(0, targetIntent);
    }

    private int CompareIntent(SkillIntent lhs, SkillIntent rhs)
    {
        if (lhs == rhs)
            return 0;
        else
            return GetIntentPri(lhs) > GetIntentPri(rhs) ? -1 : 1;
    }

    private float GetIntentPri(SkillIntent intent)
    {
        float value = intent.isReady ? 10 : 0;
        value += intent.isNormalAttack ? 0 : 2;

        if (intent.skillTarget != null)
        {
            float distanceToTarget = AIKit.Distance(intent.owner.GetPosition(), intent.skillTarget.GetPosition());
            float distanceNeedMove = Mathf.Max(0f, distanceToTarget - intent.skillBounds);
            value += 1f / (1f + distanceNeedMove);
        }

        return value;
    }
}