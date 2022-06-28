using UnityEngine;
using System.Collections;
using DataTable;

public class BigBoss : Monster
{
    public static int GetBossMaxHp(int quality, int level)
    {
        if (quality >= 0 && level >= 0)
        {
            int attrId = 80000 + quality * 1000 + level;
            DataRecord attrConfig = DataCenter.mBossConfig.GetRecord(attrId);

            if (attrConfig != null)
            {
                return attrConfig["BASE_HP"];
            }
        }

        return 1;
    }

    public static int GetBossAttack(int quality, int level)
    {
        if (quality >= 0 && level >= 0)
        {
            int attrId = 80000 + quality * 1000 + level;
            DataRecord attrConfig = DataCenter.mBossConfig.GetRecord(attrId);

            if (attrConfig != null)
            {
                return attrConfig["BASE_ATTACK"];
            }
        }

        return 1;
    }

    public DataRecord mLevelConfig;
    public int mLevel = 0;
    public int mQuality = 0;

    public BigBoss()
    {
        mNeedVerifyMD5 = true;
    }

    public override bool Init(int configIndex)
    {
        base.Init(configIndex);
        SetDirection(GetPosition()-Vector3.forward);
		return true;
    }

    public void SetQualityAndLevel(int quality, int level)
    {
        mLevel = level;
        mQuality = quality;
        int attrId = 80000 + quality * 1000 + level;
        DataRecord attrConfig = DataCenter.mBossConfig.GetRecord(attrId);

        if (attrConfig != null)
        {
            mStaticAttack = attrConfig["BASE_ATTACK"];
            mStaticMaxHp = attrConfig["BASE_HP"];
        }
    }

    public override void Start()
    {
		int attr = GetConfig("ELEMENT_INDEX");
		attr += 1;
		mLevelConfig = DataCenter.mBossConfig.GetRecord(attr*10000+mLevel);
		if (mLevelConfig != null)
		{
			Logic.EventCenter.Log(LOG_LEVEL.ERROR, "boss level config>" + (attr * 10000 + mLevel).ToString());
		
		}

        DataCenter.OpenWindow("BATTLE_BOSS_HP_WINDOW");

        DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "MAX_HP", GetMaxHp());
        DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "HP", GetMaxHp());

        base.Start();
    }

    public override bool Attack(BaseObject target)
    {
        int rand = Random.Range(0, 100);

        int rate_1 = 0;
        int skill_1 = GetConfig("ATTACK_SKILL_1");
        if (skill_1 > 0)
        {
            rate_1 = GetConfig("ATTACK_RATE_1");
            if (rand < rate_1)
                return SkillAttack(skill_1, target, null);
        }

        int skill_2 = GetConfig("ATTACK_SKILL_2");
        if (skill_2 > 0)
        {
            int rate_2 = GetConfig("ATTACK_RATE_2");
            if (rand < rate_2 && rand>=rate_1)
                return SkillAttack(skill_2, target, null);
        }

        return base.Attack(target);
    }

    public override void ApplyDamage(BaseObject attacker, Skill skill, int damage)
    {
        int nowHp = GetHp();
        //BossBattle battle = MainProcess.mStage as BossBattle;
        base.ApplyDamage(attacker, skill, damage);

        if (!IsDead())
        {
            int hpSkill = GetConfig("HP_SKILL");
            if (hpSkill > 0)
            {
                int rate = GetConfig("HP_TRIGGER");
                int limit = (int)((float)GetMaxHp()/100 * rate);

                int x = nowHp / limit;

                if (GetHp()/limit < x)
                {                   
                    SkillAttack(hpSkill, GetCurrentEnemy(), null);                    
                }
            }
        }
    }

    public override void OnDead()
    {
        PlayAnim("die");
        //LuaBehaviour.RemoveObject(this);
        base.OnDead();
        //MainProcess.mStage.OnFinish(true);
    }

    public override void ChangeHp(int nHp)
    {
        base.ChangeHp(nHp);

        DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "HP", GetHp());
    }

    public override void InitShadow()
    {
        mShadow = CreateShadow("textures/shadow_boss", 3f/*mBaseSelectRadius * CommonParam.shadowRadiusScale*/, 0.6f);
    }
}

//-------------------------------------------------------------------------
public class BigBossFactory : MonsterBossFactory
{
    public override OBJECT_TYPE GetObjectType() { return OBJECT_TYPE.BIG_BOSS; }
    public override BaseObject NewObject()
    {
        return new BigBoss();
    }

    public override NiceTable GetConfigTable() { return DataCenter.mBossConfig; }

    public override DataRecord GetConfig(int configIndex)
    {
        return DataCenter.mBossConfig.GetRecord(configIndex);
    }
}
//-------------------------------------------------------------------------