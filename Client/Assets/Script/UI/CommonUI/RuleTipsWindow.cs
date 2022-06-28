using UnityEngine;
using System.Collections;
using DataTable;
using Logic;

public class RuleTipsWindow : tWindow
{

    public override void Init() 
    {
        EventCenter.Self.RegisterEvent("Button_union_rule_tips_window_close_button",new DefineFactory<Button_union_rule_tips_window_close_button>());
        EventCenter.Self.RegisterEvent("Button_union_rule_tips_window_black", new DefineFactory<Button_union_rule_tips_window_close_button>());
    }
    public override void Open(object param)
    {
        base.Open(param);
        if (param != null && param is HELP_INDEX) 
        {
            int _index = (int)param;
            DataRecord _record = DataCenter.mHelpListConfig.GetRecord(_index);
			GameCommon.FindComponent<UILabel>(mGameObjUI, "role_tips_label").text = _record.getObject("STRING_HELP").ToString().Replace("\\n","\n") ;
        }        
    }

	
}

public class Button_union_rule_tips_window_close_button : CEvent 
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RULE_TIPS_WINDOW");
        return base._DoEvent();
    }
}
