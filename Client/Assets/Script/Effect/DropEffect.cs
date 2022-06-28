using UnityEngine;
using System.Collections;

public class DropEffect : BaseEffect
{
	public float mSpeed = 8f;

    public override bool _DoEvent()
    {
        StartUpdate();
        return base._DoEvent();
    }
	
	// Update is called once per frame
	public override bool Update (float dt) 
    {
		if(Character.Self == null)
			return false;

		Vector3 pos = GetPosition();
        Vector3 charPos = Character.Self.GetPosition();

        if (AIKit.InBounds(charPos, pos, 12f)/*Vector3.Distance(Character.Self.GetPosition(), pos) < 12.0f*/ || (MainProcess.mStage != null && MainProcess.mStage.mbSucceed))
		{
            Vector3 dir = charPos - pos;
			dir.Normalize();		
			mGraphObject.transform.Translate( dir * mSpeed * dt );

            if (AIKit.InBounds(charPos, pos, 0.5f)/*Vector3.Distance(Character.Self.GetPosition(), GetPosition()) < 0.5f*/)
			{
				AddAttribute();
				return false;
			}
		}

		return true;
	}

	void AddAttribute()
	{
        int dropHp = get("DROP_HP");
        int dropMp = get("DROP_MP");
        int dropGold = get("DROP_GOLD");
        int dropItem = get("DROP_ITEM");

        if (dropHp > 0)
        {
			AddAllRoleHP(dropHp);
        }
        else if (dropMp > 0)
        {
            if (!Character.Self.mDied)
            {
                Character.Self.ChangeMp(dropMp);
                //Character.Self.ShowPaoPao(dropMp.ToString());
                Character.Self.PlayEffect(3001, null);
            }
        }
        else if (dropGold > 0)
        {
            Character.Self.AddUIGold(dropGold);
        }
        else if (dropItem > 0)
        {
            Character.Self.AddUIItem(dropItem);
        }

        Finish();
	}

	void AddAllRoleHP(int dropHp)
	{
		AddHP(Character.Self, dropHp);

        for (int i = 0; i < Character.msFriendsCount; ++i)
        {
            AddHP(Character.Self.mFriends[i], dropHp);
        }
	}

	void AddHP(BaseObject obj, int dropHp)
	{
		if(obj != null && !obj.mDied)
		{
			obj.ChangeHp(dropHp);
			//Character.Self.ShowPaoPao(dropHp.ToString());
			obj.PlayEffect(3000, null);
		}
	}
}
