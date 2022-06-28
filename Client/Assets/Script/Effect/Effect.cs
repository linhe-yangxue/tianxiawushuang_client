using UnityEngine;
using System.Collections.Generic;
using DataTable;
using Logic;


public class BaseEffect : CEvent
{
    public GameObject mGraphObject;
	public DataRecord mConfig;
    public BaseEffectWrapper mWrapper;

    static public BaseEffect CreateEffect(BaseObject owner, BaseObject target, int configIndex)
    {
        return CreateEffect(owner, target, configIndex, false);
    }

    static public BaseEffect CreateEffect(BaseObject owner, BaseObject target, int configIndex, bool bIsChild)
    {
        string typeIndex = DataCenter.mEffect.GetData(configIndex, "TYPE");//TableManager.GetData("Effect", configIndex, "TYPE");
        if (typeIndex != "")
        {
            BaseEffect effect = EventCenter.Start(typeIndex) as BaseEffect;
            if (effect != null)
            {
                if (effect.InitCreate(owner, target, configIndex, bIsChild))
                    effect.DoEvent();
            }
            return effect;
        }
        return null;
    }

    static public void StopEmission(GameObject target)
    {
        if (target != null)
        {
            var ps = target.GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < ps.Length; ++i)
                ps[i].Stop();

            var cs = target.GetComponentsInChildren<CPlaySound>();

            for (int i = 0; i < cs.Length; ++i)
                cs[i].StopAllCoroutines();
        }
    }

    public BaseEffect()
    {
        mWrapper = new BaseEffectWrapper(this);
    }

    public virtual bool InitCreate(BaseObject owner, BaseObject target, int configIndex)
    {        
        return InitCreate(owner, target, configIndex, false);
    }

    public virtual bool InitCreate(BaseObject owner, BaseObject target, int configIndex, bool bIsChild)
    {
        //NiceTable table = TableManager.GetTable("Effect");
        //if (table != null)
        //    mConfig = table.GetRecord(configIndex);

        mConfig = DataCenter.mEffect.GetRecord(configIndex);

        if (mConfig == null)
        {
            Log("ERROR: effect config no exist >" + configIndex.ToString());
            return false;
        }

        string graphResName = mConfig.getData("MODEL");

        if (graphResName != "")
        {
            mGraphObject = GameCommon.LoadAndIntanciateEffectPrefabs(graphResName);

            if (mGraphObject != null && owner != null && (owner.mbIsUI || bIsChild))
            {
                mGraphObject.transform.parent = owner.mMainObject.transform;
                owner.mEffect = mGraphObject;
            }
        }

        string sound = mConfig.getData("SOUND");
        if (sound != "")
            return true;

        return mGraphObject != null;
    }

    public virtual void Show(bool bShow)
    {
        if (mGraphObject != null)
        {
            mGraphObject.SetActive(bShow);
            AudioSource sound = mGraphObject.GetComponent<AudioSource>();
            if (sound && !Settings.IsSoundEffectEnabled())
            {
                sound.Stop();
            }
        }
    }

    public virtual void SetPosition(Vector3 pos)
    {
        if (mGraphObject != null)
        {
            pos.x += mConfig.getData("X");
            pos.y += mConfig.getData("Y");
            pos.z += mConfig.getData("Z");
            mGraphObject.transform.position = pos;
        }
    }

	public virtual Vector3 GetPosition()
	{
		if (mGraphObject!=null)
		{
			return mGraphObject.transform.position;
		}
		return Vector3.zero;
	}

    public override bool _DoEvent()
    {
		float lifeTime = (float)mConfig.getData("LIFE_TIME");
		if (lifeTime>0.0001f)
			WaitTime ( lifeTime );
		PlaySound();
        return true;
    }

	public virtual bool PlaySound()
	{
		string sound = mConfig.getData("SOUND");
		if (sound != "")
		{
			float playTime = mConfig.getData("SOUND_START_TIME");
			if (playTime < 0.0001)
			{
				GameCommon.PlaySound(sound, GameCommon.GetMainCamera().transform.position);
			}
			else
			{
				tEvent e = StartEvent("TM_WaitPlaySound");
				if (e != null)
				{
					e.set("SOUND", sound);
					e.WaitTime(playTime);
				}
			}
			return true;
		}
		return false;
	}
    
    public override bool _OnFinish()
    {
        if (mGraphObject != null)
        {
            //var ps = mGraphObject.GetComponentsInChildren<ParticleSystem>();

            //foreach (var p in ps)
            //{
            //    p.Stop();
            //}

            //var ss = mGraphObject.GetComponentsInChildren<CPlaySound>();

            //foreach (var s in ss)
            //{
            //    s.StopAllCoroutines();
            //}

            //GameObject.Destroy(mGraphObject, 3f);
            StopEmission(mGraphObject);
            GameObject.Destroy(mGraphObject, 3f);
            mGraphObject = null;
        }
        
        return true;
    }
    
    public override void _OnOverTime()
    {
        Finish();
    }

    // 立即销毁特效并结束
    public void FinishImmediate()
    {
        if (mGraphObject != null)
        {
            GameObject.Destroy(mGraphObject);
            mGraphObject = null;
        }

        Finish();
    }

	
	public override bool Update(float secondTime){return false; }
}

public class SoundEffect : BaseEffect
{
	public override bool InitCreate(BaseObject owner, BaseObject target, int configIndex)
	{
		base.InitCreate(owner, target, configIndex);
		return mConfig!=null;
	}

	public override bool _DoEvent()
	{
		string sound = mConfig.getData("SOUND");
		if (sound != "")
		{
			float playTime = mConfig.getData("SOUND_START_TIME");
			if (playTime < 0.0001)
			{
				GameCommon.PlaySound(sound, GameCommon.GetMainCamera().transform.position);
			}
			else
			{
				WaitTime(playTime);
            }
        }
		return true;
	}

	public override void _OnOverTime()
	{
		string sound = mConfig.getData("SOUND");
		if (sound != "")
		{
			GameCommon.PlaySound(sound, GameCommon.GetMainCamera().transform.position);
		}
		Finish ();
	}
}

public class Effect : BaseEffect
{
	public BaseObject mOwner;
	public BaseObject mTargetObject;

	public tLocation mShowPosition;

	public override bool InitCreate(BaseObject owner, BaseObject target, int configIndex)
	{
		return InitCreate(owner, target, configIndex, false );
	}

    public override bool InitCreate(BaseObject owner, BaseObject target, int configIndex, bool bIsChild )
    {
        base.InitCreate(owner, target, configIndex, bIsChild);

        mOwner = owner;
        mTargetObject = target;


        OnStart();

        return true;
    }

	public virtual void OnStart()
	{
		if (mConfig!=null)
		{
			string locationType = mConfig.getData ("LOCATION_1");

			Vector3 locationPos = Vector3.zero;
			locationPos.x = mConfig.getData ("X");
			locationPos.y = mConfig.getData ("Y");
			locationPos.z = mConfig.getData ("Z");

			mShowPosition = tLocation.Create(locationType, locationPos);

			mShowPosition.SetParam( (string)mConfig.getData ("BONE") );
			mShowPosition.SetApplyDirection( (string) mConfig.getData ("APPLY_DIRECTION") );
            SetSpeedAndScale(mConfig["EFFECT_SPEED"], mConfig["EFFECT_SIZE"]);

			_UpdatePosition();
			_UpdateDirection();

		}
	}

	public virtual void _UpdatePosition()
	{
        if (mGraphObject != null)
			if (mOwner != null)
            	mGraphObject.transform.position = mShowPosition.Update(mOwner, mTargetObject);
        	else
            	mGraphObject.transform.position = mShowPosition.mPosition;
	}

	public virtual void _UpdateDirection()
	{
        if (mGraphObject != null && mOwner != null)
		{			
			mGraphObject.transform.rotation = mShowPosition._UpdateDirection(mOwner, mTargetObject);
		}
	}

	public virtual bool _NeedUpdate()
	{
		return (int)mConfig.getData("UPDATEPOS")!=0;
	}

	public override bool _DoEvent()
	{
		if (mConfig==null)
		{
			Finish();
			return false;
		}

		//OnStart();

		if ( _NeedUpdate() )
			StartUpdate();
			
		return base._DoEvent();
	}
	
	public override bool Update(float secondTime) 
	{ 
		if (mGraphObject!=null && mShowPosition!=null)
		{
			//mGraphObject.transform.position = mShowPosition.Update(mOwner, mTargetObject);
            _UpdatePosition();
            _UpdateDirection();
			return true;
		}
		return false; 
	}


    private void SetSpeedAndScale(float speed, float scale)
    {
        if (mGraphObject != null)
        {
            ParticleSystem[] systems = mGraphObject.GetComponentsInChildren<ParticleSystem>();

            foreach (var system in systems)
            {
                system.playbackSpeed = speed;
                system.startSize *= scale;
            }           
        }
    }

	//public override bool _OnEvent(ref tEvent evt){ return false; }
}


public class TM_WaitPlaySound : CEvent
{
    public override void _OnOverTime()
    {
        string sound = get("SOUND");
        if (sound != "")
        {
            GameCommon.PlaySound(sound, GameCommon.GetMainCamera().transform.position);
        }
    }
}

//public class SpriteEffect : BaseEffect
//{
//    public virtual bool InitCreate(BaseObject owner, BaseObject target, int configIndex)
//    {
//        base.InitCreate(owner, target, configIndex);
//        return mConfig != null;
//    }
//}