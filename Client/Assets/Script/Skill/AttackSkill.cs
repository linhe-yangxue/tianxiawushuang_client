using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

public class tAttackSill : BaseSkill
{
	protected Skill mCurrentSkill = null;

    public override void Stop() 
    {
        if (mNowStage!=EStateStage.FINISH && mCurrentSkill != null)
            mCurrentSkill.Stop();
        Finish(); 
    }

    public override bool _DoSkill()
    {
        if (GetFinished())
            return false;

        switch (mNowStage)
        {
            case EStateStage.START:
                mNowStage = EStateStage.ATTACK;
                if (!_StartStage())
                {
                    //_DoSkill();
                }
                ExtraOnStart();
                break;

            case EStateStage.ATTACK:
                mNowStage = EStateStage.DAMAGE;
                if (!_AttackStage())
                {
                    //_DoSkill();
                }
                ExtraOnAttack();
                break;

            case EStateStage.DAMAGE:
                mNowStage = EStateStage.FINISH;

                if (!_DamageStage())
                {
                    //Finish();
                }

                break;

            default:
                Finish();
                break;
        }

        return true;
    }

    public override bool _StartStage()
    {

        return true;
    }

    public override bool _AttackStage()
    {
        return true;
    }

    public override bool _DamageStage()
    {
        return true;
    }


    public Skill _SkillAttack(int skillIndex)
    {
		if (mTarget==null || mTarget.IsDead())
			return null;

        DataRecord config = DataCenter.mSkillConfigTable.GetRecord(skillIndex);
        if (config == null)
            return null;

        if (!GetOwner().CanDoSkill(config))
            return null;

        string skillTypeName = config.getData("TYPE");
        Skill skill = EventCenter.Start(skillTypeName) as Skill;

        if (skill != null)
        {           
            if (skill.Init(GetOwner(), mTarget, skillIndex))
			{
				mCurrentSkill = skill;
				skill.mCallBackOnFinishControl = this;
                Log(" * skill > " + skillIndex.ToString());
                skill.DoEvent();
            }            
            else
			{
				Log ("Error: skill init fail > "+skillIndex.ToString());
				Finish();
			}
        }

        return skill;
    }

	public override bool _OnFinish()
	{
        _ApplyFinish();
        mCurrentSkill = null;
        //if (mOwner.mCurrentAI == this)
        //{
        //    GetOwner().ResetIdle();
        //    GetOwner().mCurrentAI = null;
        //    RestartAI(0);
        //}	
		return true;
	}

	public override void CallBack(object o)
	{
		if ((!mIndependent && !IsNowAI()) || mTarget == null || mTarget.IsDead() || o!=mCurrentSkill)        
		{
			mNowStage = EStateStage.FINISH;
		}
        mCurrentSkill = null;
		_DoSkill();
        //float waitTime = mCurrentSkill.GetConfig("ATTACK_TIME");
        //WaitTime(waitTime);
	}
}

// use skill for attack
public class RandAllSkill : tAttackSill
{
    List<int>   mSkillIndexList = new List<int>();

    public override bool _DoEvent()
    {
        int index = GetConfig("START_MOTION");
        if (index>0)
            mSkillIndexList.Add(index);
        
        index = GetConfig("ATTACK_MOTION");
        if (index>0)
			mSkillIndexList.Add(index);

        index = GetConfig("HIT_MOTION");
        if (index>0)
			mSkillIndexList.Add(index);

        return base._DoEvent();
    }

    public void _RandAttackSkill()
    {
        if (mSkillIndexList.Count>0)
        {
            int index = Random.Range(0, mSkillIndexList.Count);
            int skillIndex = mSkillIndexList[index];
            mSkillIndexList.RemoveAt(index);

            if (skillIndex>0)
            {
                if (_SkillAttack(skillIndex) != null)
                {
                    return;
                }
            }
        }

        Finish();
    }

    public override bool _StartStage()
    {
        _RandAttackSkill();
        return true;
    }

    public override bool _AttackStage()
    {
        _RandAttackSkill();
        return true;
    }

    public override bool _DamageStage()
    {
        _RandAttackSkill();
        return true;
    }


}

public class RandAttackSkill : RandAllSkill
{

    public override bool _AttackStage()
    {
        return true;
    }
    public override bool _DamageStage()
    {
        return true;
    }

    public override void CallBack(object o)
    {
        mNowStage = EStateStage.FINISH;

        float waitTime = mCurrentSkill.GetConfig("ATTACK_TIME");
        WaitTime(waitTime);
    }
}

// rand use skill for attack
public class AttackSkill : tAttackSill
{
    public override bool _StartStage()
    {
        int index = GetConfig("START_MOTION");
        //if (GetOwner() is Character)
        //    DEBUG.Log (" 1 attack skill > " + index.ToString ());
        if (index>0)
        {
			if (_SkillAttack(index) != null)
				return true;
        }
        _DoSkill();
        return true;
    }

    public override bool _AttackStage()
    {
        int index = GetConfig("ATTACK_MOTION");
        //if (GetOwner() is Character)
        //    DEBUG.Log (" 2 attack skill > " + index.ToString ());
        if (index>0)
        {
			if (_SkillAttack(index) != null)
				return true;
        }
        _DoSkill();
        return true;

    }

    public override bool _DamageStage()
    {
        int index = GetConfig("HIT_MOTION");
        //if (GetOwner() is Character)
        //    DEBUG.Log (" 3 attack skill > " + index.ToString ());
        if (index>0)
        {           
			if (_SkillAttack(index) != null)
                return true;
        }
        _DoSkill();
        return true;
    }



    public override bool _OnFinish()
    {
		//LOG.log (" Finish skill > " + (string)mConfig.getData(0));
        //Log(" !!!! finish >" + GetEventName());
        return base._OnFinish();
    }

}