using UnityEngine;
using System.Collections;
using System;
using Logic;
using DataTable;

/// <summary>
/// 副本重置弹窗类型
/// </summary>
public enum ADVENTURE_RESET_WIN_TYPE 
{
    NONE = -1,
    NEXT_VIP_LEVEL = 0,
    MAX_RESET_TIMES = 1,

}
/// <summary>
/// 重置窗口
/// </summary>
public class AdventureResetWindow : tWindow
{
    public override void Init()
    {
        //EventCenter.Self.RegisterEvent("Button_addventure_reset_window_ok_btn",new DefineFactory<Button_addventure_reset_window_ok_btn>());      
    }

    //传递过来的数据是已经重置的次数
    public override void Open(object param)
    {
        base.Open(param);
        // 添加取消按钮事件
        AddButtonAction("addventure_reset_window_cancel_btn", () => 
        {
            Close();
        });
        // 设置默认的确认按钮方法
        AddButtonAction("addventure_reset_window_ok_btn", () => 
        {
            Close();
        });
        string _contentStr = string.Empty;
        if (param != null && param is int) 
        {
            int _todayResetTimes = (int)param;
           
            //1.判断当前是否还有重置次数
            DataRecord _resetRecord = DataCenter.mResetCost.GetRecord(_todayResetTimes + 1);
            int _maxResetTimesWithVip = TableCommon.GetNumberFromVipList(RoleLogicData.Self.vipLevel,"COPYRESET_NUM");
            int _leftResetTimes = _maxResetTimesWithVip - _todayResetTimes;
            if (_leftResetTimes > 0)
            {
                 int _costNum = _resetRecord.getData("COST_NUM");
                _contentStr = "重置元宝需要花费" + _costNum.ToString()+ "元宝，是否继续？今日可重置"+_leftResetTimes.ToString()+"次";              
                //GameCommon.GetButtonData(mGameObjUI, "addventure_reset_window_ok_btn").set("TODAY_RESET_TIMES", _todayResetTimes);
            }
            GameCommon.FindComponent<UILabel>(mGameObjUI, "content_label").text = _contentStr;
            //添加重置事件
            AddButtonAction("addventure_reset_window_ok_btn", () => 
            {
                int _stageID = DataCenter.Get("CURRENT_STAGE_ID");
                int _resetTimes = _todayResetTimes;
                //判断当前元宝是否足够
                int _costNum = (int)DataCenter.mResetCost.GetRecord(_resetTimes + 1).getObject("COST_NUM");
                if (_costNum > RoleLogicData.Self.diamond)
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SHOP_NO_ENOUGH_DIAMOND);
                    return;
                }
                NetManager.RequestResetBattleTimes(_stageID, _resetTimes + 1);           
            });
        }
        else if (param != null && param is ADVENTURE_RESET_WIN_TYPE) 
        {
            ADVENTURE_RESET_WIN_TYPE _type = (ADVENTURE_RESET_WIN_TYPE)Enum.Parse(typeof(ADVENTURE_RESET_WIN_TYPE), param.ToString());
            if (_type == ADVENTURE_RESET_WIN_TYPE.MAX_RESET_TIMES) 
            {
                _contentStr = "今日可重置次数已达上限";
                GameCommon.FindComponent<UILabel>(mGameObjUI, "content_label").text = _contentStr;
            }
        }
    }


}

///// <summary>
///// 重置弹窗确认按钮
///// </summary>
//public class Button_addventure_reset_window_ok_btn : CEvent 
//{
//    public override bool _DoEvent()
//    {
      
//    }
//}