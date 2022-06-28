using UnityEngine;


public class ScriptSkill : Skill
{
    private LuaSkill skill;

    public override bool _DoEvent()
    {
        ActiveObject obj = GetOwner() as ActiveObject;

        if (obj != null)
        {
            skill = LuaSkill.New(this);
        }

        if (skill != null)
        {
            skill.Activate();
        }
        else
        {
            _ApplyFinish();
        }

        return true;
    }
}