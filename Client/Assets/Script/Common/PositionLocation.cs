using UnityEngine;
using System.Collections;

public enum ELocationType
{
    POSITION,
    BONE,
    OBJECT_POS,
}

public enum EDirectionType
{
    NONE,
    BONE,
    OTHER,
    OWNER,
    LINE,
}

public class tLocation
{
    public Vector3 mPosition = Vector3.zero;

    public EDirectionType mDirectionType = EDirectionType.NONE;

    static public tLocation Create(ELocationType locationType, Vector3 location)
    {
        tLocation resultPos;
        switch (locationType)
        {
            case ELocationType.BONE:
                resultPos = new BoneLocation();
                break;

            case ELocationType.OBJECT_POS:
                resultPos = new ObjectLocation();
                break;

            default:
                resultPos = new tLocation();
                break;
        }

        resultPos.mPosition = location;

        return resultPos;
    }

    static public tLocation Create(string strType, Vector3 location)
    {
        ELocationType type = ELocationType.POSITION;
        switch (strType)
        {
            case "BONE":
                type = ELocationType.BONE;
                break;

            case "OBJECT":
                type = ELocationType.OBJECT_POS;
                break;

            case "POSITION":
            default:

                break;
        }

        return Create(type, location);
    }

    public virtual Vector3 Update(BaseObject baseObject, BaseObject targetOther)
    {
        return mPosition;
    }

    public virtual Vector3 Update(BaseObject baseObject, BaseObject targetOther, out Quaternion nowDir)
    {
        nowDir = _UpdateDirection(baseObject, targetOther);
        return mPosition;
    }

    public virtual void SetParam(object param) { }

    public virtual void SetApplyDirection(EDirectionType directionType) { mDirectionType = directionType; }

    public virtual void SetApplyDirection(string strDirType)
    {
        switch (strDirType)
        {
            case "BONE":
                SetApplyDirection(EDirectionType.BONE);
                break;

            case "OWNER":
                SetApplyDirection(EDirectionType.OWNER);
                break;

            case "OTHER":
                SetApplyDirection(EDirectionType.OTHER);
                break;

            case "LINE":
                SetApplyDirection(EDirectionType.LINE);
                break;

            default:
                SetApplyDirection(EDirectionType.NONE);
                break;
        }


    }

    public virtual Quaternion _UpdateDirection(BaseObject owner, BaseObject other)
    {
        switch (mDirectionType)
        {
            case EDirectionType.OWNER:
                return owner.GetRealDirection();

            case EDirectionType.OTHER:
                return other.GetRealDirection();

            case EDirectionType.LINE:
				if (owner!=null && other!=null && (other.GetPosition() - owner.GetPosition()) != Vector3.zero)
                	return Quaternion.LookRotation(other.GetPosition() - owner.GetPosition(), Vector3.up);
				else if (owner!=null)
                    return owner.GetRealDirection();
				break;
            default:
                return Quaternion.identity;
        }

		return Quaternion.identity;
    }
}

public class BoneLocation : tLocation
{
    string mBoneName;
    Transform mBoneForm;

    public override void SetParam(object param)
    {
        if (param is string)
            mBoneName = (string)param;

		if (mBoneName==null)
			mBoneName = "";
    }

    public override Quaternion _UpdateDirection(BaseObject owner, BaseObject other)
    {
        if (mDirectionType == EDirectionType.BONE)
        {
            if (mBoneForm != null)
                return mBoneForm.rotation;
            else
                return Quaternion.identity;
        }
        else
            return base._UpdateDirection(owner, other);
    }

    public override Vector3 Update(BaseObject baseObject, BaseObject targetOther)
    {
        if (mBoneForm == null && mBoneName != "")
        {
            GameObject bone = GameCommon.FindObject(baseObject.mMainObject, mBoneName);
				
			if (bone == null && baseObject.mMainObject!=null)
			{
				if (mBoneName!=null)
                	bone = GameCommon.FindObject(baseObject.mMainObject, mBoneName.ToLower());
				else
					mBoneName = "";
			}
            if (bone != null)
                mBoneForm = bone.transform;
        }

        if (mBoneForm != null)
        {
            if (mDirectionType == EDirectionType.BONE)
                return mBoneForm.position + mBoneForm.rotation * mPosition;
            else if (mDirectionType == EDirectionType.NONE)
                return mBoneForm.position + mPosition;
            else
                return mBoneForm.position + _UpdateDirection(baseObject, targetOther) * mPosition;
        }
        else
            return mPosition;

    }

    public override Vector3 Update(BaseObject baseObject, BaseObject targetOther, out Quaternion nowDir)
    {
        if (mBoneForm == null && mBoneName != "")
        {
            GameObject bone = GameCommon.FindObject(baseObject.mMainObject, mBoneName);

            if (bone == null && baseObject.mMainObject != null)
            {
                if (mBoneName != null)
                    bone = GameCommon.FindObject(baseObject.mMainObject, mBoneName.ToLower());
                else
                    mBoneName = "";
            }
            if (bone != null)
                mBoneForm = bone.transform;
        }

        if (mBoneForm != null)
        {
            if (mDirectionType == EDirectionType.BONE)
            {
				nowDir = mBoneForm.rotation;
                return mBoneForm.position + mBoneForm.rotation * mPosition;
            }
            else if (mDirectionType == EDirectionType.NONE)
            {
                nowDir = Quaternion.identity;
                return mBoneForm.position + mPosition;
            }
            else
            {
                nowDir = _UpdateDirection(baseObject, targetOther);
                return mBoneForm.position + nowDir * mPosition;
            }
        }
        else
        {
            nowDir = _UpdateDirection(baseObject, targetOther);
            return mPosition;
        }
    }
}

public class ObjectLocation : tLocation
{
    public override Vector3 Update(BaseObject baseObject, BaseObject targetOther)
    {
        if (mDirectionType == EDirectionType.NONE)
            return baseObject.GetPosition() + mPosition;
        else
            return baseObject.GetPosition() + _UpdateDirection(baseObject, targetOther) * mPosition;
    }
}

public class ObjectLocationForPet : tLocation
{
    public Quaternion mRotation;
    public float mAngle = 0;
    public Vector3 mOffsetPos;

    public override void SetParam(object param)
    {
        if (param is float)
        {
            mAngle = (float)param;
            mRotation = Quaternion.Euler(0, (float)param, 0);
        }
        else
            mRotation = Quaternion.identity;

        mOffsetPos = mRotation * mPosition;
    }

    public override Vector3 Update(BaseObject baseObject, BaseObject targetOther)
    {
        //Vector3 ra = baseObject.mMainObject.transform.eulerAngles;
        //float r = ra.y + mAngle;

        return baseObject.GetPosition() + baseObject.GetDirection() * mOffsetPos; // (mRotation * mPosition));
    }

    public Vector3 Update(Vector3 ownerPos, Vector3 dir)
    {
        Quaternion q = Quaternion.LookRotation(dir);
        return ownerPos + q * mOffsetPos;
    }

}
