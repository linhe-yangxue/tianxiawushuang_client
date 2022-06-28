using UnityEngine;
using System.Collections;
using DataTable;

public class Pet : Friend
{
    public Pet()
    {
        if (mShowHpBar != null)
            mShowHpBar.Init(msHpBarWidth, msHpBarHeight, Color.green, Color.yellow);
    }

    //public override float LookBound() { return 5; }

    public override float AttackTime() { return 3.6f; }

    public override float HitTime() { return 1; }

    //public override void OnDead()
    //{
    //    base.OnDead();

    //    if (MainProcess.mStage != null && MainProcess.mStage is PVP4Battle)
    //    {
    //        if (mbIsPvpOpponent)
    //        {
    //            if (OpponentCharacter.mInstance.CheckAllDead())
    //            {
    //                MainProcess.mStage.OnFinish(true);
    //            }
    //        }
    //        else
    //        {
    //            if (Character.Self.CheckAllDead())
    //            {
    //                MainProcess.mStage.OnFinish(false);
    //            }
    //            else if (CameraMoveEvent.mMainTarget == this)
    //            {
    //                CameraMoveEvent.BindMainObject(Character.Self.GetAlivePet());
    //            }
    //        }
    //    }
    //}

    public override void OnPositionChanged()
    {
        base.OnPositionChanged();

        if (this == CameraMoveEvent.mMainTarget)
        {
            CameraMoveEvent.OnTargetObjectMove(this);
        }
    }
}
//------------------------------------------------------------------------------
public class PetFactory : ObjectFactory
{
    public override OBJECT_TYPE GetObjectType() { return OBJECT_TYPE.PET; }
    public override BaseObject NewObject()
    {
        return new Pet();
    }

    public override NiceTable GetConfigTable() { return DataCenter.mActiveConfigTable; }

}