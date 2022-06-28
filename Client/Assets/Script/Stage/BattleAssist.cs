using UnityEngine;
using System.Collections;
using Utilities.Routines;


public class BattleAssist : RoutineBlock
{
    private float refreshDeltaTime = 1f;
    private float damping = 5f;
    private BattleIndicator indicator = new BattleIndicator();
    private Vector3 lastDir;
    private float lastTime;

    protected override bool OnBlock()
    {
        Update();
        return true;
    }

    protected virtual void Update()
    {
        if (MainProcess.mStage != null
            //PVEStageBattle.mBattleControl == BATTLE_CONTROL.MANUAL
            //&& MainProcess.mStage.GetBattleControl() == BATTLE_CONTROL.MANUAL
            && MainProcess.mStage.mbBattleActive
            && !MainProcess.mStage.mbBattleFinish
            && Character.Self != null
            && !Character.Self.IsDead())
        {
            var nearest = Character.Self.FindNearestEnemy(true);

            if (nearest != null)
            {
                Vector3 dir = nearest.GetPosition() - Character.Self.GetPosition();
                dir.y = 0f;

                if (dir.sqrMagnitude > AIParams.battleIndicateDistance * AIParams.battleIndicateDistance)
                {
                    dir = CalculateDirection(nearest);

                    if (indicator.visible)
                    {
                        indicator.position = Character.Self.GetPosition();
                        indicator.rotation = Quaternion.Lerp(indicator.rotation, Quaternion.LookRotation(dir), damping * Time.deltaTime);
                    }
                    else 
                    {
                        indicator.position = Character.Self.GetPosition();
                        indicator.rotation = Quaternion.LookRotation(dir);
                        indicator.visible = true;
                    }
                }
                else
                {
                    indicator.visible = false;
                }
            }
            else
            {
                indicator.visible = false;
            }
        }
        else 
        {
            indicator.visible = false;
        }
    }

    private Vector3 CalculateDirection(BaseObject target)
    {
        if (Time.time - lastTime < refreshDeltaTime)
        {
            return lastDir;
        }
        else
        {
            lastTime = Time.time;
            NavMeshAgent navAgent = Character.Self.mMainObject.GetComponent<NavMeshAgent>();

            if (navAgent != null && navAgent.enabled)
            {
                NavMeshPath path = new NavMeshPath();

                if (navAgent.CalculatePath(target.GetPosition(), path) && path.status == NavMeshPathStatus.PathComplete && path.corners.Length >= 2)
                {
                    lastDir = path.corners[1] - path.corners[0];
                    lastDir.y = 0f;
                }
            }

            return lastDir;
        }
    }
}


public class BattleIndicator
{
    private GameObject effect;

    public bool visible
    {
        get { return effect.activeSelf; }
        set { effect.SetActive(value); }
    }

    public Vector3 position
    {
        get { return effect.transform.position; }
        set { effect.transform.position = value; }
    }

    public Quaternion rotation
    {
        get { return effect.transform.rotation; }
        set { effect.transform.rotation = value; }
    }

    public BattleIndicator()
    {
        var res = GameCommon.LoadPrefabs("Effect/Skill/zxjt");
        effect = GameObject.Instantiate(res) as GameObject;
        effect.SetActive(false);
    }
}