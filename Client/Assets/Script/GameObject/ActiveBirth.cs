using UnityEngine;
using System.Collections;

public class ActiveBirth : MonoBehaviour 
{
    public static bool mEnableMonsterBossUIInfo = true;

    public int mIndex = 0;

    [HideInInspector]
	public int mBirthConfigIndex = 0;

	public int mOwner = 2000;

    // 组别，只对怪物有效
    [HideInInspector]
    public int mGroup = 0; 

    // 创建延时，只对怪物有效。
    // 当组别为0且延时为0时，刚进关卡即创建
    // 当组别为0且延时不为0时，从关卡开始时刻计时到满足延时后创建
    // 当组别大于0时，从上一组怪物被全部杀死时刻计时至满足延时后创建
    [HideInInspector]
    public float mDelay = 0f;   

    public BaseObject mActiveObject = null;

    private void Awake()
    {
        if (MainProcess.mStage != null)
            MainProcess.mStage.RegisterBirthPoint(this);
    }

	// Use this for initialization
	void Start () 
    {
        //by chenliang
        //begin

//        if (MainProcess.mStage != null)
//            MainProcess.mStage.InitSetBirthPoint(this);
//
//		if (mIndex == -1 || mOwner == CommonParam.PlayerCamp)
//		{
//			Destroy(gameObject);
//			return;
//			//mBirthConfigIndex = RoleLogicData.GetMainRole().mModelIndex;
//		}
//
//        if (mBirthConfigIndex<=0)
//        {
//            Destroy(gameObject);
//            return;
//        }
//
//		MeshRenderer r = gameObject.GetComponentInChildren<MeshRenderer>();
//		r.enabled = false;
//
//        tLogicData battleData = DataCenter.GetData("BATTLE");
//
//		if (mOwner != 1000 && battleData!=null)
//        {
//            int monsterCount = battleData.get("MONSTER_COUNT");
//            battleData.set("MONSTER_COUNT", ++monsterCount);
//        }		
//----------------

        //end

	}
    //by chenliang
    //begin

    private bool mIsInited = false;         //是否初始化
    /// <summary>
    /// 初始化数据
    /// </summary>
    private void __Init()
    {
        if (MainProcess.mStage != null)
            MainProcess.mStage.InitSetBirthPoint(this);

        MeshRenderer r = gameObject.GetComponentInChildren<MeshRenderer>();
        r.enabled = false;

        tLogicData battleData = DataCenter.GetData("BATTLE");

        if (mOwner != 1000 && battleData != null)
        {
            int monsterCount = battleData.get("MONSTER_COUNT");
            battleData.set("MONSTER_COUNT", ++monsterCount);
        }
    }

    //end
	
	// Update is called once per frame
	void Update () 
    {
        //by chenliang
        //begin

        if (!GlobalModule.IsSceneLoadComplete)
            return;
        if (!mIsInited)
        {
            mIsInited = true;
            __Init();
        }

        if (mIndex == -1 || mOwner == CommonParam.PlayerCamp)
        {
            Destroy(gameObject);
            return;
        }

        if (mBirthConfigIndex <= 0)
        {
            Destroy(gameObject);
            return;
        }

        //end
        if (MainProcess.mStage == null || !MainProcess.mStage.IsActiveBirthReleased(this))
        {
            return;
        }

        if (GameCommon.GetMainCamera()!=null && mActiveObject==null)
        {            
			BaseObject obj = ObjectManager.Self.CreateObject(mBirthConfigIndex);
            if (obj != null)
            {
                obj.SetCamp(mOwner);
                //transform.position = new Vector3(transform.position.x, 1000, transform.position.z);
                obj.InitPosition(transform.position);
                obj.SetDirection(obj.GetPosition()-Vector3.forward);
                //obj.SetVisible(false);
                mActiveObject = obj;

                //if (mActiveObject.GetCamp()==2000)
                //    AutoBattleAI.AddMonster(mIndex, mActiveObject);

                Monster monster = obj as Monster;

                if (monster != null)
                {
                    monster.mGroup = mGroup;
                }
            }
            else
            {
                GameObject.Destroy(gameObject);
                return;
            }
        }

        if ((MainProcess.mStage != null && MainProcess.mStage.mbBattleStart && mOwner == CommonParam.PlayerCamp)
            || (Character.Self != null && CommonParam.StartBirth && AIKit.InBounds(transform.position, Character.Self, CommonParam.BirthBound)/*Vector3.Distance(transform.position, Character.Self.GetPosition()) < CommonParam.BirthBound*/))
		{
            if (mActiveObject != null && !mActiveObject.IsDead())
            {
				bool isNeedDestroy = true;
				if (!mActiveObject.mbHasStarted)
				{
					mActiveObject.SetVisible(true);
					if (Character.Self!=mActiveObject)
						mActiveObject.Start();

                    if (!AIKit.InBounds(transform.position, Character.Self, CommonParam.BirthBound - 4)/*Vector3.Distance(transform.position, Character.Self.GetPosition()) > (CommonParam.BirthBound - 4)*/)
					{
						isNeedDestroy = false;
					}
				}

				if(isNeedDestroy)
				{
					MonsterBoss boss = mActiveObject as MonsterBoss;
                    if (boss != null && mEnableMonsterBossUIInfo)
					{
						boss.ShowInfoUI();
					}
					Destroy(gameObject);  
				}
            }
		}
	}

}
