using UnityEngine;
using System.Collections;
using Logic;


/// <summary>
/// VIP专属客服窗口
/// </summary>
public class VipExclusiveWindow : tWindow
{
    private string mCustomerQQ = string.Empty;
    public override void Init()
    {
        base.Init();
        EventCenter.Self.RegisterEvent("Button_vip_exclusive_close_mask", new DefineFactory<Button_vip_exclusive_close_mask>());
        EventCenter.Self.RegisterEvent("Button_vip_exclusive_confirm_btn",new DefineFactory<Button_vip_exclusive_confirm_btn>());
        EventCenter.Self.RegisterEvent("Button_vip_exclusive_copy_qq_btn",new DefineFactory<Button_vip_exclusive_copy_qq_btn>());
    }

    public override void Open(object param)
    {
 	     base.Open(param);
         Refresh(param);
    }

    public override bool Refresh(object param)
    {
 	   
        UILabel _mainContentLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "content_label");
        if (_mainContentLbl != null) 
        {
            _mainContentLbl.text = TableCommon.getStringFromStringList(STRING_INDEX.VIP_EXCLUSIVE_MAIN_CONTENT);
        }
        UILabel _outSideLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "content_outside_label");
        if(_outSideLbl != null)
        {
            _outSideLbl.text = TableCommon.getStringFromStringList(STRING_INDEX.VIP_EXCLUESIVE_OUTSIDE_CONTENT);
        }
        UILabel _consumerQQLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "qq_content_label");
        if (_consumerQQLbl != null) 
        {
            mCustomerQQ = TableCommon.getStringFromStringList(STRING_INDEX.VIP_EXCLUSIVE_CONSUMER_QQ_NUMBER);
            _consumerQQLbl.text = mCustomerQQ;
        }

        return base.Refresh(param);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch(keyIndex)
        {
            case "COPY_QQ":
                CopyQQContent();
            break;
        }
    }

    private void CopyQQContent() 
    {
        DEBUG.logError("Consumer QQ is: " + mCustomerQQ);
        TextEditor te = new TextEditor();
        te.content = new GUIContent(mCustomerQQ);
        te.OnFocus();
        te.SelectAll();
        te.Copy();
    }

    public override void Close()
    {
        base.Close();
        
    }
}

public class Button_vip_exclusive_close_mask : CEvent 
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("VIP_EXCLUSIVE_WINDOW");
        return base._DoEvent();
    } 
}

public class Button_vip_exclusive_confirm_btn : CEvent 
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("VIP_EXCLUSIVE_WINDOW");
        return base._DoEvent();
    }
}

public class Button_vip_exclusive_copy_qq_btn : CEvent 
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("VIP_EXCLUSIVE_WINDOW", "COPY_QQ", null);
        return base._DoEvent();
    }
}

public class Button_vip_exclusive_btn : CEvent 
{
    public override bool _DoEvent() 
    {
        DataCenter.OpenWindow("VIP_EXCLUSIVE_WINDOW");
        return base._DoEvent();
    }
}