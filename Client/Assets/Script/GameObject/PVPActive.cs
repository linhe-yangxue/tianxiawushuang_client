using UnityEngine;
using System.Collections;
using DataTable;

public class PVPActive : Monster 
{
    public Logic.tEvent mSkillCDEvent;
	public int mPetSkillRate = 90;

    public int mIconIndex = 0;

    public override float LookBounds() { return 90000; }

    public override void AddCharactorAttribute() { }


    public override void OnInitModelObject()
    {
        base.OnInitModelObject();
        if (GetCamp() == CommonParam.PlayerCamp)
        {
            NavMeshAgent nav = mMainObject.GetComponent<NavMeshAgent>();
            nav.avoidancePriority = 99;
        }
    }

    //public override void InitShadow()
    //{
    //    if (GetCamp() == CommonParam.PlayerCamp)
    //        CreateShadow("textures/shadow_friend", 1.5f, 0.5f);
    //    else
    //        base.InitShadow();
    //}

    public override bool SetCurrentEnemy(BaseObject enemy)
    {
        if (base.SetCurrentEnemy(enemy))
        {
            CameraMoveEvent.BindMainObject(this);
            return true;
        }
        return false;
    }

    public override void OnStartAttackEnemy(BaseObject enemy) { }

    //public override BaseObject FindNearestEnemy()
    //{       
    //    foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
    //    {
    //        if (obj != null && IsEnemy(obj))
    //        {
    //            if (GameCommon.GetElmentRaletion(mConfigRecord, obj.mConfigRecord)==ELEMENT_RELATION.ADVANTAGEOUS)
    //            {
    //                return obj;
    //            }
    //        }
    //    }

    //    return base.FindNearestEnemy();
    //}

    public override void OnPositionChanged() 
    {
        base.OnPositionChanged();

        CameraMoveEvent.OnTargetObjectMove(this);
    }

    public override void Start()
    {
        base.Start();

        int skillIndex = GetConfig("PET_SKILL_1");
        if (skillIndex > 0)
        {
            float cdTime = SkillGlobal.GetInfo(skillIndex).skillCD;//TableManager.GetData("Skill", skillIndex, "CD_TIME");
            mSkillCDEvent = Logic.EventCenter.Start("TM_SkillCD");
            mSkillCDEvent.WaitTime(cdTime);
            mSkillCDEvent.set("SKILL_INDEX", skillIndex);
        }

		mPetSkillRate = DataCenter.mGlobalConfig.GetData("PVP_PET_SKILL_RATE", "VALUE");
    }

    public override bool Attack(BaseObject target)
    {
        if (mSkillCDEvent!=null && mSkillCDEvent.GetFinished() && Random.Range(0, 101) <= mPetSkillRate)
        {
            int skillIndex = mSkillCDEvent.get("SKILL_INDEX");
            if (skillIndex > 0)
            {
                mSkillCDEvent.SetFinished(false);

                if (SkillAttack(skillIndex, target, null))
                {
                    float t = 0;
                    DataRecord configRe = DataCenter.mSkillConfigTable.GetRecord(skillIndex);
                    if (configRe != null)
                        t = SkillGlobal.GetInfo(configRe).skillCD;//configRe.get("CD_TIME");

                    if (t < 0.0001f)
                        mSkillCDEvent = null;
                    else
                        mSkillCDEvent.WaitTime(t);

                    return true;
                }
            }
        }

        return base.Attack(target);
    }

    public override void ChangeHp(int nHp)
    {
        base.ChangeHp(nHp);
        if (GetCamp() == CommonParam.PlayerCamp)
            DataCenter.SetData("PVP_PLAYER_WINDOW", "CHANGED_HP", this);
        else
            DataCenter.SetData("PVP_OTHER_PLAYER_WINDOW", "CHANGED_HP", this);
    }

    public override void OnDead()
    {
        base.OnDead();

        if (GetCamp() == CommonParam.PlayerCamp)
            DataCenter.SetData("PVP_PLAYER_WINDOW", "PET_DEAD", this);
        else
            DataCenter.SetData("PVP_OTHER_PLAYER_WINDOW", "PET_DEAD", this);

        PVP6Battle battle = MainProcess.mStage as PVP6Battle;
        if (battle != null)
            battle.OnPetDead(this);
    }

     public override void Drop() { }
}

public class PVPActiveFactory : ObjectFactory
{
	public override OBJECT_TYPE GetObjectType() { return OBJECT_TYPE.PVP_ACTIVE; }
	public override BaseObject NewObject()
	{
		return new PVPActive();
	}
	public override NiceTable GetConfigTable() { return DataCenter.mActiveConfigTable; }
}
