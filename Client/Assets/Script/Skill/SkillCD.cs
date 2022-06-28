using UnityEngine;
using System.Collections;

public class TM_SkillCD : Logic.CEvent 
{
    public override void _OnOverTime()
    {
        Finish();
       
    }
}
