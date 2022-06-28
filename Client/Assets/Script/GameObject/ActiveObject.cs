using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;

public class Shadow
{
	public tLocation mLocation;
	public GameObject mShadow;
	
	public void Init(string prefabName, Vector3 locatPos)
	{
		mShadow = GameCommon.LoadAndIntanciatePrefabs(prefabName);
		mLocation = tLocation.Create(ELocationType.OBJECT_POS, locatPos);
	}
	
	public void Update(BaseObject ownerObject)
	{
		if (mShadow != null)
		{
			mShadow.transform.position = mLocation.Update(ownerObject, null);
		}
	}
}

public class LockFlag
{
    public static ActiveObject mTarget = null;
    public static NiceSprite mFlag = null;

    public static void Lock(ActiveObject target)
    {
        if (target == mTarget)
        {
            return;
        }

        Unlock();
        mTarget = target;

        if (target != null && target.mMainObject != null)
        {
            mFlag = target.CreateShadow("textures/lock_target", 1.5f/*target.mBaseSelectRadius * CommonParam.lockRadiusScale*/, 1f);
            mFlag.mMainTransform.gameObject.AddComponent<MouseCoordAnimation>();
        }
    }

    public static void Unlock()
    {
        if (mFlag != null)
        {
            mFlag.Destroy();
        }

        //if (mTarget != null && !mTarget.IsDead())
        //{
        //    mTarget.InitShadow();
        //}

        mTarget = null;
        mFlag = null;
    }
}

public class tObjectAnim
{
	public string mNowAnimState;

	public virtual bool InitAnim(BaseObject baseObj) { return false; }
    public virtual bool Play(string animName) { return false; }
    public virtual bool Play(string animName, bool bLoop) { return false; }

    public virtual bool IsStop() { return true; }

    public virtual string NowAnim() { return mNowAnimState; }

    public virtual void Stop() { }
    public virtual void SetSpeed(float speed) { }
	
	public virtual Animator GetAnimator(){return null;}
    public virtual AnimatorStateInfo GetCurrentStateInfo() { return new AnimatorStateInfo(); }
}


public class ObjectAnimtor : tObjectAnim
{
    public Animator mAnimator;    

    public override bool InitAnim(BaseObject baseObj)
    {
        mNowAnimState = "";

        bool bVisi = baseObj.mbVisible;
        if (!bVisi)
		    baseObj.SetVisible(true);

        var anims = baseObj.mMainObject.GetComponentsInChildren<Animator>();

        if (!bVisi)
		    baseObj.SetVisible(bVisi);

        if (anims != null && anims.Length > 0)
        {
            mAnimator = anims[0];
            return true;
        }

        return false;
    }

    public override void Stop() 
    {
        if (mAnimator != null)
        {
            mAnimator.StopPlayback();
            //foreach (Animator an in mAnimator)
            //{
            //    if (an!=null)
            //        an.StopPlayback();
            //}
        }
    }

    public override void SetSpeed(float speed)
    {
        if (mAnimator != null)
        {
            mAnimator.speed = speed;
            //foreach (Animator an in mAnimator)
            //{
            //    if (an != null)
            //        an.speed = speed;
            //}           
        }
    }

    public override bool IsStop()
    {
        //if (mAnimator != null && mAnimator.Length>0 && mAnimator[0]!=null)
        //{
        //    AnimatorStateInfo state = mAnimator[0].GetCurrentAnimatorStateInfo(0);
        //    return !state.loop && (state.normalizedTime >= 1);
        //}  
        //return true;

        if (mAnimator != null)
        {
            AnimatorStateInfo state = mAnimator.GetCurrentAnimatorStateInfo(0);
            return !state.loop && (state.normalizedTime >= 1);
        }

        return true;
    }

    public override bool Play(string animName)
    {
        try
        {
            if (mNowAnimState == animName)
            {
                if (mAnimator != null)
                {
                    AnimatorStateInfo state = mAnimator.GetCurrentAnimatorStateInfo(0);

                    if (!state.loop && state.IsName(animName))
                    {
                        //foreach (Animator an in mAnimator)
                        //{
                        //    an.StopPlayback();
                        //    an.Play(animName, 0, 0);
                        //    //an.CrossFade(animName, 0.3f, 0, 0f);
                        //}
                        //mAnimator.StopPlayback();
                        //mAnimator.Play(animName, 0, 0);
                        mAnimator.CrossFade(animName, 0.2f, 0, 0f);
                    }
                }

                return true;
            }

            //DEBUG.Log("[" + animName + "]");

            //bool bHave = false;

            if (mAnimator != null)
            {
                //bHave = true;
                //mAnimatior.Play(animName, 0, 0);
                //foreach (Animator an in mAnimator)
                //{
                //    an.StopPlayback();
                //    an.Play(animName);
                //    //an.CrossFade(animName, 0.3f, 0, 0f);
                //}
                //mAnimator.StopPlayback();
                //mAnimator.Play(animName);
                mAnimator.CrossFade(animName, 0.2f, 0, 0f);
                mNowAnimState = animName;
            }

            //if (bHave)
            //    mNowAnimState = animName;

        }
        catch
        {
            Logic.EventCenter.Log(LOG_LEVEL.ERROR, " play animation fail>" + animName);
			return false;
        }
    
		return true;
    }

    public override bool Play(string animName, bool bLoop) 
    {
        Logic.EventCenter.Log(LOG_LEVEL.WARN, "Play with loop no finish function");
        return Play(animName);        
    }

	public override  Animator GetAnimator()
	{
		return mAnimator;
	}

    public override AnimatorStateInfo GetCurrentStateInfo()
    {
        if (mAnimator != null)
        {
            return mAnimator.GetCurrentAnimatorStateInfo(0);
        }

        return base.GetCurrentStateInfo();
    }
}

public class MoveComponent 
{
    public Vector3[] mMovePath = null;
	public int		mCurrentIndex = 0;

    public bool mNeedMove = false;
    //public float mMoveSpeed = 1;
    public Vector3 mLookAtPos;

    public ObjectAI mCallBackAI;
    private Action<bool> mOnReach;
    private float mClampSpeed = Mathf.Infinity;

    float mTotalLength = 0;
    float mTargetDistance = 0;
    Vector3 mCurrentPos;
    Vector3 mTargetPos = Vector3.zero;
    public Vector3 mDirection = Vector3.forward;
    private bool mPathPatial = false;

    public bool StartMoveWithCallback(BaseObject ownerObject, Vector3 targetPosition, Action<bool> onReach, float clampSpeed)    
    {
        if (ownerObject == null || ownerObject.mMainObject == null)
            return false;

        mOnReach = onReach;
        mClampSpeed = clampSpeed;
        return StartMove(ownerObject, targetPosition, null, true);
    }

    public void SetClampSpeed(float speed)
    {
        mClampSpeed = speed;
    }

    public bool StartMove(BaseObject ownerObject, Vector3 targetPosition, ObjectAI finishAI, bool bTry)
    {
        mCurrentPos = ownerObject.GetPosition();

        mCurrentIndex = 0;
        mMovePath = null;
		mNeedMove = false;
		mCallBackAI = null;
        mPathPatial = false;

        if (ownerObject == null || ownerObject.mMainObject==null)
            return false;

        mNeedMove = true;
        mCallBackAI = finishAI;
        if (Vector3.Distance(ownerObject.GetPosition(), targetPosition) > 0.001f)
        {
            NavMeshAgent navAgent = ownerObject.mMainObject.GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                NavMeshPath path = new NavMeshPath();
                try
                {
                    if (navAgent.CalculatePath(targetPosition, path))
                    {
                        if (path.status != NavMeshPathStatus.PathPartial)
                        {
                            mCallBackAI = finishAI;
                            mMovePath = path.corners;
                            mNeedMove = true;
                            ownerObject.PlayAnim("run");
                            _MoveStep(ownerObject);                           
							return true;
                        }
                    }
                    if (bTry)
                    {
                        mPathPatial = true;

                        while (Vector3.Distance(ownerObject.GetPosition(), targetPosition) > 1)
                        {
                            targetPosition = (targetPosition + ownerObject.GetPosition()) / 2;
                            if (navAgent.CalculatePath(targetPosition, path))
                            {
                                if (path.status != NavMeshPathStatus.PathPartial)
                                {
                                    mCallBackAI = finishAI;
                                    mMovePath = path.corners;
                                    mNeedMove = true;
                                    ownerObject.PlayAnim("run");
                                    _MoveStep(ownerObject);
                                    return true;
                                }
                            }
                        }
                    }

                }
                catch (Exception e)
                {
					DEBUG.LogError(e.ToString());
                }
            }
        }
		mNeedMove = false;
        Finish(ownerObject);
        //DEBUG.Log("WARN: No find way >" + ownerObject.mMainObject.name);
		//Finish (ownerObject);
		return false;
    }

    public bool _MoveStep(BaseObject ownerObject)
    {
        if (mMovePath != null)
        {
            if (mCurrentIndex < mMovePath.Length)
            {
				++mCurrentIndex;
                //Vector3 pos = ownerObject.GetPosition();
                return _MoveTo(ownerObject, mMovePath[mCurrentIndex-1]);
            }
        }
       
		Finish(ownerObject);
        ownerObject.OnMoveEnd();
        return false;
    }


    protected bool _MoveTo(BaseObject ownerObject, Vector3 targetPos)
    {
        mTargetDistance = Vector3.Distance(targetPos, mCurrentPos);

        if (mTargetDistance < 0.01f)
        {
            mCurrentPos = targetPos;
            ownerObject.SetPosition(targetPos);
            _MoveStep(ownerObject);
            return false;
        }

        mTotalLength = 0;
        mTargetPos = targetPos;
        mDirection = targetPos - mCurrentPos;
        mDirection.Normalize();

		//base.Start(ownerObject.GetPosition(), targetPos, speed);
		mLookAtPos = targetPos + (mDirection * 10);
        ownerObject.OnWillMoveTo(ref targetPos);
        return true;
    }

    public virtual void Update(BaseObject ownerObject, float onceTime)
    {
        if (mNeedMove)
        {            
			//bool bEnd = base.Update(onceTime);
            mTotalLength += Mathf.Min(ownerObject.GetMoveSpeed(), mClampSpeed) * onceTime;
            Vector3 pos = mCurrentPos + mDirection * mTotalLength;
           
            bool bEnd = mTotalLength>=mTargetDistance;
            if (bEnd)
                pos = mTargetPos;

            ownerObject.SetPosition(pos);
			ownerObject.SetDirection(mLookAtPos);
            ownerObject.OnPositionChanged();

            if (bEnd)
            {
                mCurrentPos = pos;
				_MoveStep(ownerObject);
            }
        }
    }

	public virtual void Stop(BaseObject ownerObject)
	{
		if (mNeedMove)
		{
			mNeedMove = false;
			mMovePath = null;
			mCurrentIndex = 0;
		}
	}

	public virtual void Finish(BaseObject ownerObject)
    {
		if (mNeedMove)
		{
        	mNeedMove = false;
        	mMovePath = null;
        	mCurrentIndex = 0;

            ObjectAI nowCall = mCallBackAI;
            mCallBackAI = null;
            if (nowCall != null && !nowCall.GetFinished())
                nowCall.CallBack(ownerObject);
            else
                ownerObject.ResetIdle();

            if (mOnReach != null)
            {
                mOnReach(!mPathPatial);
                mOnReach = null;
            }
		}
    }
}

public class ActiveObject : BaseObject
{

	//public float mMoveSpeed = 2.6f;
	
	public MoveComponent mMoveComponent = new MoveComponent();
	public Vector3 mLookAtPos;
	
	public tObjectAnim mAnim = new ObjectAnimtor();

    public NiceSprite mShadow;
    public GameObject mCampShadow;
    public GameObject mQualityShadow;
	
	public WarnEffect mWarnEffect;

    public List<AffectBuffer> mAffectList = new List<AffectBuffer>();
    //private List<AffectBuffer> mFinishedBufferList = new List<AffectBuffer>();
	
	public tLocation mHeadLocation = tLocation.Create(ELocationType.BONE, new Vector3(0, 1.2f, 0));

    protected BaseObject mLookEnemy;

    private float mAureolaTimer = AttackState.AUREOLA_RATE;
	
	public Animator mAnimator;
	public string mCurrentAnimatorName;

	public override void OnInitModelObject()
	{
		base.OnInitModelObject();

		SetCastShadow(false);

        //MoveComponent.mMoveSpeed = GetConfig("MOVE_SPEED");
				
		mHeadLocation.SetApplyDirection(EDirectionType.NONE);
		mHeadLocation.SetParam("Bip01 Head");

        mAnim.InitAnim(this);

        if (mWarnEffect == null)
            mWarnEffect = BaseEffect.CreateEffect(this, null, 3006) as WarnEffect;

        // 音效管理
        if (mMainObject)
        {
            AudioSource audioSource = mMainObject.GetComponent<AudioSource>();
            if (audioSource != null && !Settings.IsSoundEffectEnabled())
                audioSource.Stop();
        }
    }

    public override void OnModelChanged() 
    {
        string nowAnim = mAnim.mNowAnimState;
        mAnim.InitAnim(this);

        if (nowAnim != "")
        {         
            mAnim.Play(nowAnim);
            SetAnimDefaultSpeed(nowAnim);
        }
    }
	
	public override void OnDestroy()
	{
		foreach (AffectBuffer buffer in mAffectList)
		{
			if (buffer!=null)
				buffer.Finish();
		}
		mAffectList.Clear();

        if (mShadow != null)
        {
            mShadow.Destroy();
            mShadow = null;
        }

        if (mCampShadow != null)
        {
            GameObject.Destroy(mCampShadow);
            mCampShadow = null;
        }

        if (mQualityShadow != null)
        {
            GameObject.Destroy(mQualityShadow);
            mQualityShadow = null;
        }

		if (mWarnEffect != null)
		{
			mWarnEffect.Finish();
			mWarnEffect = null;
		}	

		base.OnDestroy();
		mAnim = null;
	}

    //public NiceSprite CreateShadow(string texName, float size, float alpha)
    //{
    //    return CreateShadow(texName, size, alpha, true);
    //}
	
	public NiceSprite CreateShadow(string texName, float size, float alpha/*, bool replaceShadow*/)
	{
        //if (replaceShadow && mShadow != null)
        //    mShadow.Destroy();

        if (mMainObject == null)
            return null;
		
		//Material mat = new Material(GameCommon.FindShader("Transparent/VertexLit"));
        Material mat = new Material(GameCommon.FindShader("Particles/Alpha Blended"));
		
		Color c = Color.white;
		c.a = alpha;
		mat.color = c;
		
		mat.mainTexture = GameCommon.LoadTexture(texName, LOAD_MODE.RESOURCE);
		//mat.SetColor("_Emission", Color.white);
        //modified by xuke
        //mat.SetColor("_TintColor", new Color(1f, 1f, 1f, 0.5f));
        mat.SetColor("_TintColor", new Color(1f, 1f, 1f, 1.0f));
        //end       
        //added by xuke
        GameCommon.SetMat_ETC1(mat);
        //end
		
		NiceSprite shadow = NiceSprite.Create("shadow", null, true);
		shadow.SetMaterail(mat);

        shadow.mMainTransform.name = texName;
		shadow.mMainTransform.parent = mMainObject.transform;
		shadow.mMainTransform.localPosition = Vector3.zero;
		shadow.mMainTransform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
		shadow.mMainTransform.localScale = new Vector3(size, size);
		
        //if(replaceShadow)
        //    mShadow = shadow;

        return shadow;		
	}
	
	public virtual void InitShadow()
	{
		//CreateShadow("textures/Shadow", 1.5f/*mBaseSelectRadius * CommonParam.shadowRadiusScale*/, 1);
		//GameObject o = ResourceManager.Load("Prefabs/Shadow", typeof(GameObject)) as GameObject;
		//if (o != null)
		//{
		//    GameObject s = GameObject.Instantiate(o) as GameObject;
		//    s.transform.parent = mMainObject.transform;
		//    s.transform.localPosition = new Vector3(0, 2, 0);
		//}
        if (mMainObject != null)
        {
            if (GetCamp() == CommonParam.PlayerCamp)
            {
                if(GetObjectType() == OBJECT_TYPE.CHARATOR)
                    mCampShadow = GameCommon.LoadAndIntanciateEffectPrefabs("EFFECT/UIEffect/new_ui_zhujiaohuan");
                else
                    mCampShadow = GameCommon.LoadAndIntanciateEffectPrefabs("EFFECT/UIEffect/new_ui_npchuan");
            }
            else
            {
                mCampShadow = GameCommon.LoadAndIntanciateEffectPrefabs("EFFECT/UIEffect/new_ui_enemyhuan");
            }

            if (mCampShadow != null)
            {              
                mCampShadow.transform.parent = mMainObject.transform;
                mCampShadow.transform.localPosition = Vector3.zero;
            }
        }
	}
	
	public override void InitPosition(Vector3 pos)
	{
        NavMeshAgent navAgent = mMainObject.GetComponent<NavMeshAgent>();
        if (navAgent != null)
            navAgent.enabled = false;
               
        Ray ray = new Ray(new Vector3(pos.x, pos.y+9000, pos.z), Vector3.down);
        RaycastHit rayInfo;
        int mask = 1 << CommonParam.ObstructLayer;
        mask = ~mask;
        if (Physics.Raycast(ray, out rayInfo, 999999, mask))
        {
            pos = rayInfo.point;
        }

        base.InitPosition(pos);
       
        if (navAgent != null)
        {
            navAgent.enabled = true;
        }
		
		OnPositionChanged();
	}

    public override void SetVisible(bool bV)
    {
        mbVisible = bV;

        if (!bV)
            mAnim.Stop();

        foreach (Transform form in mMainObject.transform)
        {
            form.gameObject.SetActive(bV);
        }

        if (bV)
        {
            string nowAnim = mAnim.NowAnim();
            //mAnim.InitAnim(this);
            if (nowAnim != "")
            {
                mAnim.Play(nowAnim);
                SetAnimDefaultSpeed(nowAnim);
            }
        }

        SetVisibleHp(bV);

        if (mShadow != null)
            mShadow.SetVisible(bV);

        if (mCampShadow != null)
            mCampShadow.SetActive(bV);

        if (mQualityShadow != null)
            mQualityShadow.SetActive(bV);
    }

    public virtual bool TryMoveTo(Vector3 targetPos, ObjectAI finishAI)
    {
        if (mMoveComponent.StartMove(this, targetPos, finishAI, true))
        {
            return true;
        }
        return false;
    }
	// move to target position
	public override bool MoveTo(Vector3 targetPos, ObjectAI finishAI)
	{
        base.MoveTo(targetPos, finishAI);

        if (mMoveComponent.StartMove(this, targetPos, finishAI, false))
        {
			return true;
        }
		return false;

        //if (Vector3.Distance (targetPos, GetPosition ()) <= 0.0001f) 
        //{
        //    SetPosition(targetPos);
        //    mNeedMove = false;
			
        //    _OnStopMove();
        //    //StopMove ();
        //    return;
        //}
		
        //float speed = mMoveSpeed;
        //if (mCurrentAI != null)
        //    speed += mCurrentAI.AffectSpeed();
		
        //mMoveComponent.Start(GetPosition(), targetPos, speed);
        //mLookAtPos = GetPosition() + mMoveComponent.mDirection * (mMoveComponent.mDestValue + 10);
        //mNeedMove = true;
		
        //mState = EObjectState.MOVING;
		
		//OnMoveBegin ();
	}

    public override bool MoveToThenCallback(Vector3 targetPos, Action<bool> onReach, float clampSpeed)
    {
        base.MoveToThenCallback(targetPos, onReach, clampSpeed);
        return mMoveComponent.StartMoveWithCallback(this, targetPos, onReach, clampSpeed);
    }

    //public virtual void ShowLockFlag()
    //{
    //    if (mMainObject != null && GameCommon.FindObject(mMainObject, "textures/lock_target") == null)
    //    {
    //        NiceSprite s = CreateShadow("textures/lock_target", 2.0f, 1);
    //        s.mMainTransform.gameObject.AddComponent<MouseCoordAnimation>();
    //    }
    //}

	public override void Attack(Vector3 attackPos)
	{
		mState = EObjectState.ATTACK;
		SetDirection (attackPos);
		PlayAnim ("attack");
	}

    public override void PursueAttack(BaseObject enmey)
    {
        SetCurrentEnemy(null);
        mEnemy = enmey;
        float dis = Vector3.Distance(GetPosition(), enmey.GetPosition());
        int skill = mAttackSkillIndex;
        if (dis <= SkillBound(skill))
        {
            OnStartAttackEnemy(enmey);
            //obj.SetCurrentEnemy( GetOwner() );
            Attack(enmey);
        }
        else
        {
            AI_MoveTo mto = StartAI("AI_MoveToTargetThenAttack") as AI_MoveTo;
            if (mto != null)
            {
                mto.SetTarget(enmey);
                mto.DoEvent();
            }
            else
                DEBUG.LogError("AI_MoveToTargetThenAttack NO exist");
        }
    }

	public override void Start()
	{
		base.Start ();
		InitShadow();
        SetVisible(true);
		mAffectList.Clear();

		for (int i=1; i<=3; ++i)
		{
            string key = "ATTACK_STATE_" + i.ToString();
            int stage = GetConfig(key);
            if (stage > 0)
            {
                mAttackStage[i - 1] = new AttackState(stage);
            }
            else
                mAttackStage[i - 1] = null;
		}

        //for (int i=1; i<=3; ++i)
        //{
        //    string key = "AFFECT_BUFFER_" + i.ToString();
        //    int bufferIndex = GetConfig(key);
        //    if (bufferIndex>0)
        //    {
        //        StartAffect(this, bufferIndex);
        //    }
        //}
        for (int i = 0; i < 4; ++i)
        {
            int skillIndex = GetConfig("PET_SKILL_" + (i + 1));
            int skillCategory = skillIndex / 100000;

            if (skillCategory == 1)
            {
                InitSkillOnStart(i, skillIndex);
            }
            else if (skillCategory == 3)
            {
                InitBuffOnStart(i, skillIndex);
            }
        }

        mAffectPool.Update();
        //StartAI();
	}

    protected virtual void InitSkillOnStart(int fieldIndex, int skillIndex) 
    {
        ActivateCDLooper(skillIndex, SKILL_POS.SPECIAL_ATTACK, GetConfigSkillLevel(fieldIndex), 0f);
    }

    protected virtual void InitBuffOnStart(int fieldIndex, int buffIndex) 
    {
        StartAffect(this, buffIndex, GetConfigSkillLevel(fieldIndex));
    }
	
	
	// Update is called once per frame
	public override void Update (float onceTime) 
	{
        if (mbHasStarted)
        {
            RemoveFinishedBuffer();
        }

		if (mNeedDestroy)
			return;
		
		base.Update (onceTime);

        if (mbHasStarted)
        {
            mMoveComponent.Update(this, onceTime);

            if (mEnemy != null && mEnemy != mLookEnemy)
            {
                float dis = Vector3.Distance(GetPosition(), mEnemy.GetPosition());
                if (dis < LookBounds())
                {
                    mLookEnemy = mEnemy;
                    OnLookEnemy();
                }
            }

            //if (mNeedMove)
            //{
            //    if (IsDead())
            //    {
            //        StopMove();
            //    }
            //    else
            //    {

            //        if (mMoveComponent.Update(onceTime))
            //        {
            //            mNeedMove = false;
            //        }

            //        Vector3 pos = mMoveComponent.NowValue();
            //        SetPosition(pos);
            //        SetDirection(mLookAtPos);

            //        OnPositionChanged();

            //        if (GetCurrentAnimName() != "run")
            //            Log(LOG_LEVEL.ERROR, " anim >" + GetCurrentAnimName());

            //        if (!mNeedMove)
            //        {
            //            //StopMove ();
            //            _OnStopMove();
            //        }
            //    }
            //}

            if (mShowHpBar != null)
            {
                _UpdateHpBarPos();
                mShowHpBar.Update(onceTime);
            }
            //if (mCurrentAI!=null && mCurrentAI.GetFinished())
            //	mCurrentAI.RestartAI();

            // Aureola buff on battle when alive
            UpdateAureola(onceTime);
            OnPoolAffect();
        }

		if(mAnimator != null)
		{
            var state = mAnimator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(mCurrentAnimatorName))
            {
                if (state.normalizedTime >= 1f)
                {
                    //mAnimator.StopPlayback();
                    if (mMoveComponent.mNeedMove)
                    {
                        PlayAnim("run");
                    }
                    else
                    {
                        ResetIdle();                 
                    }

                    mAnimator = null;
                }
            }
            //foreach (Animator an in mAnimator)
            //{
            //    if(an.GetCurrentAnimatorStateInfo(0).IsName (mCurrentAnimatorName))
            //    {
            //        if(an.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            //        {
            //            an.StopPlayback ();
            //            ResetIdle ();
            //            mAnimator = null;
            //        }
            //    }
            //}
		}
	}

    //public void UpdatePose()
    //{
    //    if (mMainObject != null)
    //    {
    //        //base.SetDirection(mLookDirection);
    //        mMainObject.transform.rotation = Quaternion.AxisAngle(GameCommon.GetMainCamera().transform.right, (float)(System.Math.PI * 0.4f));
    //    }
    //}

    public override void OnPositionChanged()
    {
        // Alway top on grounp
        if (mMainObject != null)
        {
            NavMeshAgent navAgent = mMainObject.GetComponent<NavMeshAgent>();
            if (navAgent != null && navAgent.enabled)
                navAgent.Move(Vector3.zero);
        }

        //Vector3 pos = GetPosition();
        //pos.y += 90000;
        //Ray ray = new Ray(pos, Vector3.down);
        //RaycastHit rayInfo;
        //int mask = 1 << CommonParam.ObstructLayer;
        //mask = ~mask;
        //if (Physics.Raycast(ray, out rayInfo, 999999999, mask))
        //{
        //    pos = rayInfo.point;
        //    SetPosition(pos);
        //}
    }
	
	public override void _UpdateHpBarPos()
	{
		if (mShowHpBar==null || mMainObject==null)
			return;
		
		Vector3 pos = mHeadLocation.Update(this, null);
		
		//if (mHeadBoneForm!=null)
		//	pos.y += mHeadBoneForm.position.y;
		
		mShowHpBar.SetPos(pos);
		
	}
	

	
	public override void StopMove()
	{
        base.StopMove();

		mMoveComponent.Finish(this);
		//mNeedMove = false;
		
		//PlayAnim ("idle");
		OnMoveEnd();		
	}

	public override void _StopMove()
	{
		mMoveComponent.Stop(this);
		OnMoveEnd();
	}

	
	public override bool IsNeedMove()
    {
		return mMoveComponent.mNeedMove;
	}

    public void ShowWarnFlag()
    {
        if (mWarnEffect != null)
        {
            mWarnEffect.SetVisible(true);
        }
    }
	
	public override void _PlayAnim(string animName)
	{
        var state = mAnim.GetCurrentStateInfo();
        
        if (animName == "" || animName == "0" || mMainObject == null || standStill
            //|| (mCurrentAI != null && !mCurrentAI.GetFinished() && !mCurrentAI.AllowPlayAnimation(animName))
            || (animName == "idle" && mAnim.mNowAnimState == "hit" && state.normalizedTime < 1f))
        {
            return;
        }

        string tempAnimName = animName;

        if (animName.LastIndexOf("hit") > 0)
        {
            animName = "hit";
        }

        if (!mAnim.Play(animName))
		{
			DEBUG.LogError(mMainObject.name + " > No exist animation > "+ animName);
		}

        //DEBUG.Log(">> " + GetConfig("NAME") + "(ID = " + mConfigIndex + ") play animation \"" + animName + "\"");

        SetAnimDefaultSpeed(animName);

        if (animName == "hit")
        {
            mAnimator = mAnim.GetAnimator();
            mCurrentAnimatorName = "hit";
        }
        else 
        {
            mAnimator = null;
            mCurrentAnimatorName = null;
        }
        //mAnimator = animName == "hit" ? mAnim.GetAnimator() : null;

        //if (this is Character)
        //{
        //    //DEBUG.Log("anim > " + animName);
        //    if (IsNeedMove() && animName!="run")
        //        Logic.EventCenter.Log(LOG_LEVEL.GENERAL, " == anim > " + animName);
        //}


		OBJECT_TYPE type = ObjectManager.Self.GetObjectType(mConfigIndex);
		int iModel = 0;

		if(type == OBJECT_TYPE.CHARATOR || type == OBJECT_TYPE.PET)
		{
			iModel = TableManager.GetData("ActiveObject", mConfigIndex, "MODEL");
		}
		else
		{
			iModel = TableManager.GetData("MonsterObject", mConfigIndex, "MODEL");
		}

		string key = "";

		if(animName == "hit")
		{
			int iGuardType = TableManager.GetData("ModelConfig", iModel, "GUARD_TYPE");
			if(iGuardType == (int)GUARD_TYPE.ARMOR)
			{
				key = "armor";
			}
			else if(iGuardType == (int)GUARD_TYPE.ARMOR)
			{
				key = "flesh";
			}
			key = key + "_" + tempAnimName;
		}
		else
		{
			key = iModel.ToString() + "_" + tempAnimName;
		}

		DataRecord re = DataCenter.mMotionSoundTable.GetRecord(key);

        if (re != null)
        {
			bool bIsPlay = true;
			PLAY_SOUND_MODEL_TYPE playSoundModelType = (PLAY_SOUND_MODEL_TYPE)((int)re.getData("PLAY_SOUND_MODEL_TYPE"));

			if(PLAY_SOUND_MODEL_TYPE.PET == playSoundModelType)
			{
				if(type != OBJECT_TYPE.CHARATOR && type != OBJECT_TYPE.PET)
					bIsPlay = false;
			}
			else if(PLAY_SOUND_MODEL_TYPE.MONSTER == playSoundModelType)
			{
				if(type == OBJECT_TYPE.CHARATOR || type == OBJECT_TYPE.PET)
					bIsPlay = false;
			}
			else if(PLAY_SOUND_MODEL_TYPE.NONE == playSoundModelType)
			{
				bIsPlay = false;
			}

            // 世界地图打开时不播放音效
            if (GameCommon.bIsWindowOpen("SCROLL_WORLD_MAP_WINDOW"))
                bIsPlay = false;

			if(bIsPlay)
			{
				string soundFile = re.getData("SOUND_FILE");
                int soundtype = re.getData("SOUND_TYPE");
                GameCommon.PlaySound(soundFile, GameCommon.GetMainCamera().transform.position, soundtype);
			}
        }
    }

    public override void _PlayAnim(string animName, bool bLoop) 
    {
        if (animName == "" || animName == "0")
            return;

        if (mAnim != null)
        {
            //DEBUG.Log(">> " + GetConfig("NAME") + "(ID = " + mConfigIndex + ") play animation \"" + animName + "\"");
            mAnim.Play(animName, bLoop);
            SetAnimDefaultSpeed(animName);

			if(!bLoop)
			{
//			  mAniStateInfoList = mAnim.GetAnimatorStateInfo (animName);
				mAnimator = mAnim.GetAnimator ();
				mCurrentAnimatorName = animName;
			}
        }
    }

    public override void StopAnim()
    {
        if (mAnim != null)
            mAnim.Stop();
    }

    public override void SetAnimSpeed(float speed)
    {
        if (mAnim != null)
            mAnim.SetSpeed(speed);
    }

    public override string GetCurrentAnimName() { return mAnim.mNowAnimState; }
	
	public override bool IsAnimStoped()
	{
        return mAnim!=null && mAnim.IsStop();
	}
	
	public override void OnHit(string hitAnim, BaseObject attacker)
	{
		base.OnHit(hitAnim, attacker);

        //if (GameCommon.GetElmentRaletion(mConfigRecord, mbIsPvpOpponent, attacker.mConfigRecord, attacker.mbIsPvpOpponent) != ELEMENT_RELATION.BLANCE)
        //{
        //    // 属性相克时,播放特效
        //    PlayEffect(9026, attacker);
        //}
		
        //if (!IsDead() 
        //    && ( mAnim.mNowAnimState != "attack" || IsAnimStoped() ) 
        //    && !IsNeedMove()
        //    && (mCurrentAI == null || !mCurrentAI.HoldControl())
        //    )
        //{
        //    //SetDirection(attacker.GetPosition());

        //    float dis = Vector3.Distance(GetPosition(), attacker.GetPosition());
        //    if(dis > 4.5f)
        //    {
        //        hitAnim = "remote_" + hitAnim;
        //    }

        //    PlayAnim(hitAnim);
        //}   
        if (!IsDead() && (mAnim.mNowAnimState == "hit" || mAnim.mNowAnimState == "idle"))
        {
            if (!AIKit.InBounds(this, attacker, 4.5f))
            {
                hitAnim = "remote_" + hitAnim;
            }

            PlayAnim(hitAnim);
        }
	}

    public override void ApplyBufferOnHit(BaseObject attacker)
    {
        foreach (AttackState state in mAttackStage)
        {
            if (state != null)
                state.ApplyBuffer(this, attacker, AFFECT_TIME.ON_HIT);
        }       
    }

    protected void ForEachActiveBuff(Action<AffectBuffer> action)
    {
        int count = mAffectList.Count;

        for (int i = 0; i < count; ++i)
        {
            if (mAffectList[i] != null && !mAffectList[i].GetFinished())
            {
                action(mAffectList[i]);
            }
        }
    }

    public override void OnHitBuff(BaseObject target, int damage, SKILL_DAMAGE_FLAGS flags)
    {
        ForEachActiveBuff(x => x.OnHit(target, damage, flags));
    }

    public override void OnDamageBuff(BaseObject attacker, int damage, SKILL_DAMAGE_FLAGS flags)
    {
        ForEachActiveBuff(x => x.OnDamage(attacker, damage, flags));
    }

    //public override int FinalHit(BaseObject target, int damage)
    //{
    //    int val = 0;
    //    ForEachActiveBuff(x => val += x.FinalHit(target, damage));
    //    return val;
    //}

    //public override int FinalDamage(BaseObject attacker, int damage)
    //{
    //    int val = 0;
    //    ForEachActiveBuff(x => val += x.FinalDamage(attacker, damage));
    //    return val;
    //}

    public override void OnAttack(BaseObject target)
    {
        base.OnAttack(target);

        ForEachActiveBuff(x => x.OnAttack(target));
    }

    public override void OnDoSkill(BaseObject target, int index)
    {
        ForEachActiveBuff(x => x.OnSkill(target, index));
    }
	
	public override void OnDead() 
	{
		base.OnDead();

        NavMeshAgent agent = mMainObject.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;
	
		StopMove();
		
		PlayAnim("die");
		//ShowPaoPao("Dead");
		
		if (mShowHpBar!=null)
			mShowHpBar.SetVisible(false);

        StopAllBuffer();

        BaseEffect.StopEmission(mCampShadow);
        BaseEffect.StopEmission(mQualityShadow);
		//Projector pro = mMainObject.GetComponentInChildren<Projector>();
		//if (pro != null)
		//    pro.enabled = false;
	}
	
	public override void OnIdle()
	{
		base.OnIdle();
		if (!IsDead())
		{
			//StopMove();
            if (mAnim.NowAnim() != "idle")
            {
                PlayAnim("idle");
            }         
		}
	}

	
	public override Vector3 GetTopPosition()
	{
		return mHeadLocation.Update(this, null);
	}
	
	public override void BeatBack(Vector3 dir, float backLength)
	{
		StopMove();
		Vector3 targetPos = GetPosition() + dir * backLength;
		//targetPos.y += 0.5f;
		
		AI_BeatBackMove move = ForceStartAI("AI_BeatBackMove") as AI_BeatBackMove;
		if (move != null)
		{
			move.mTargetPos = targetPos;
			move.mSpeed = 10;
			move.DoEvent();
		}
	}

    public override AffectBuffer StartAffect(BaseObject startObject, int affectConfig, int level)
    {
        if (IsDead())
            return null;

        if (!CheckCanBeApplyAffect(affectConfig))
        {
            this.ShowPaoPao("", PAOTEXT_TYPE.RESIST);
            return null;
        }

        AffectBuffer buffer = GetBufferByIndex(affectConfig);

        if (buffer != null && !buffer.GetFinished())
        {
            buffer.ChangeStartObject(startObject);
            buffer.Restart();
            InitBufferUI(affectConfig);
            return buffer;
        }      

        buffer = AffectBuffer.StartBuffer(startObject, this, affectConfig, level);

        if (buffer!=null)
        {
            mAffectList.Add(buffer);
            InitBufferUI(affectConfig);
            buffer.ApplyAffect();
            return buffer;
        }
        return null;
    }

    public override void StopAffect(int affectConfig)
    {
        AffectBuffer buffer = GetBufferByIndex(affectConfig);

        if (buffer != null && !buffer.GetFinished())
        {
            buffer.Finish();
            RemoveBufferUI(affectConfig);
        }
    }

    public ScriptBuffer CreateScriptBuffer()
    {
        ScriptBuffer buffer = EventCenter.Start("ScriptBuffer") as ScriptBuffer;   
        buffer.mConfigIndex = 0;
        buffer.mAffectConfig = null;
        buffer.mStartObject = this;    
        buffer.Start(this);
        buffer.buff = LuaBuff.New(buffer);
        mAffectList.Add(buffer);
        return buffer;
    }

    public virtual void InitBufferUI(int iAffectConfig)
    {
        //if (GetObjectType() == OBJECT_TYPE.CHARATOR)
        //{
        //    DataCenter.SetData("BATTLE_PLAYER_WINDOW", "START_CHAR_BUFF", iAffectConfig);
        //}
        //else if (GetObjectType() == OBJECT_TYPE.PET)
        //{
        //    PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        //	int iPos = petLogicData.GetPosInCurTeam((this as Friend).mPetData.mDBID);
        //
        //    if (iPos >= 1 && iPos <= 3)
        //        DataCenter.SetData("BATTLE_PLAYER_WINDOW", "START_PET_BUFF_" + iPos.ToString(), iAffectConfig);
        //    else
        //        DataCenter.SetData("BATTLE_PLAYER_WINDOW", "START_FRIEND_BUFF", iAffectConfig);
        //}
    }

    public virtual void RemoveBufferUI(int iAffectConfig) { }

	public void RefreshAllBufferUIPos()
	{
		if (GetObjectType() == OBJECT_TYPE.CHARATOR)
		{
			CharInfo charInfo = DataCenter.Self.getData("BATTLE_PLAYER_WINDOW") as CharInfo;

			charInfo.RefreshAllBufferUIPos();
		}
		else if (GetObjectType() == OBJECT_TYPE.PET)
		{
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			Friend firendobj = this as Friend;
            //int iNum = petLogicData.GetPetUseNumByPos(firendobj.mPetData.mUsePos);
			int iNum = petLogicData.GetPosInCurTeam((this as Friend).mActiveData.itemId);
			
			FriendInfo friendInfo = DataCenter.GetData("FRIEND_INFO_" + iNum.ToString()) as FriendInfo;

			friendInfo.RefreshAllBufferUIPos();
		}
	}

	public void HideAllBuffer()
	{
		if (GetObjectType() == OBJECT_TYPE.CHARATOR)
		{
			CharInfo charInfo = DataCenter.Self.getData("BATTLE_PLAYER_WINDOW") as CharInfo;
            if (charInfo!= null)
			    charInfo.HideAllBuffer();
		}
		else if (GetObjectType() == OBJECT_TYPE.PET)
		{
            
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
            Friend firendobj = this as Friend;
            //int iNum = petLogicData.GetPetUseNumByPos(firendobj.mPetData.mUsePos);
//            int iNum = (this as Friend).mPetData.mUsePos;
			int iNum = petLogicData.GetPosInCurTeam((this as Friend).mActiveData.itemId);
			
			FriendInfo friendInfo = DataCenter.GetData("FRIEND_INFO_" + iNum.ToString()) as FriendInfo;
			
            if(friendInfo != null)
			    friendInfo.HideAllBuffer();
		}
	}

    //public override float ApplyAffect(AFFECT_TYPE affectType, float fValue)
    //{
    //    if (mAffectList.Count <= 0)
    //        return fValue;
    //    int rate = 0;
    //    int total = 0;
    //    foreach (AffectBuffer buf in mAffectList)
    //    {
    //        if (buf != null && !buf.GetFinished())
    //        {
    //            total += buf.Affect(affectType);
    //            rate += buf.AffectRate(affectType);
    //        }
    //    }

    //    float nowValue = total + fValue * (1.0f + rate * 0.01f);
    //    return nowValue;
    //}

    //public override int ApplyAffect(AFFECT_TYPE affectType, int nValue)
    //{
    //    if (mAffectList.Count <= 0)
    //        return nValue;
    //    int rate = 0;
    //    int total = 0;
    //    foreach (AffectBuffer buf in mAffectList)
    //    {
    //        if (buf != null && !buf.GetFinished())
    //        {
    //            total += buf.Affect(affectType);
    //            rate += buf.AffectRate(affectType);
    //        }
    //    }

    //    int nowValue = (int)(total + nValue * (1.0f + rate * 0.01f));
    //    return nowValue;
    //}

    public override float GetBufferAddValue(AFFECT_TYPE type)
    {
        float total = 0f;

        for (int i = mAffectList.Count - 1; i >= 0; --i)
        {
            total += mAffectList[i].GetAffectValue(type);
        }

        total += (float)mAffectPool.GetValue(type.ToString());
        return total;
    }

    public override AffectBuffer GetBufferByIndex(int index)
    {
        for (int i = mAffectList.Count - 1; i >= 0; --i)
        {
            if (mAffectList[i].mConfigIndex == index)
            {
                return mAffectList[i];
            }
        }

        return null;
    }

    //public override void NotifyBufferFinished(AffectBuffer buffer)
    //{
    //    //mAffectList.Remove(buffer);
    //    mFinishedBufferList.Add(buffer);
    //}

    private void RemoveFinishedBuffer()
    {
        mAffectList.RemoveAll(x => x.GetFinished());
        //foreach (AffectBuffer buffer in mFinishedBufferList)
        //{
        //    mAffectList.Remove(buffer);
        //}
        //mFinishedBufferList.Clear();
    }

    private void UpdateAureola(float onceTime)
    {
        if (MainProcess.mStage != null && !MainProcess.mStage.mbBattleFinish && !IsDead())
        {
            mAureolaTimer += onceTime;

            if (mAureolaTimer >= AttackState.AUREOLA_RATE && !mDied)
            {
                // not "mAureolaTimer = 0f" to avoid error accumulation
                mAureolaTimer -= AttackState.AUREOLA_RATE;
                AffectAureola();
            }
        }
    }

    private void AffectAureola()
    {
        foreach (AttackState state in mAttackStage)
        {
            if (state != null && !state.AureolaOverTime())
                state.ApplyBuffer(this, null, AFFECT_TIME.AUREOLA);
        }
    }

    private void SetAnimDefaultSpeed(string anim)
    {
        if (anim == "attack" || anim == "attack_1" || anim == "attack_2")
        {
            int model = mConfigRecord["MODEL"];
            DataRecord modelConfig = DataCenter.mModelTable.GetRecord(model);

            if (modelConfig != null)
            {
                SetAnimSpeed(modelConfig["MODEL_ATTACK_SPEED"]);
                return;
            }
        }

        SetAnimSpeed(1f);
    }

    public override void StopAllBuffer()
    {
        for (int i = mAffectList.Count - 1; i >= 0; --i)
        {
            if (mAffectList[i] != null)
            {
                mAffectList[i].Finish();
            }
        }

        mAureolaTimer = -99999f;
    }

    protected virtual void OnPoolAffect()
    {
        int hp = (int)mAffectPool.GetValue("HP");

        if (hp != 0)
        {
            ChangeHp(hp);
            ShowHPPaoPao(hp);
            mAffectPool.ResetValue("HP");
        }
    }

    public override void SetClampMoveSpeed(float clampSpeed)
    {
        mMoveComponent.SetClampSpeed(clampSpeed);
    }
}