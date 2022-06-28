using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 属性变化反馈文字的类型
/// </summary>
public enum ChangeTipValueType
{
    NONE,
    
    ATTACK,             //> 攻击
    HP,                 //> 生命
    PHYSICAL_DEFENCE,   //> 物防
    MAGIC_DEFENCE,      //> 法防
    
    RELATE,             //> 缘分
    MASTER,             //> 强化大师
    POWER,              //> 战斗力
    
    EQUIP_STRENGTHEN_LEVEL,     //> 装备强化等级
    EQUIP_STRENGTHEN_ATTR_BASE, //> 装备强化属性
    EQUIP_STRENGTHEN_ATTR_0,    //> 装备强化属性
    
    EQUIP_REFINE_LEVEL,         //> 装备精炼等级
    EQUIP_REFINE_BASE_ATTR,     //> 装备精炼基础属性
    EQUIP_REFINE_SUB_ATTR,      //> 装备精炼附加属性
    
    MAGIC_STRENGTHEN_LEVEL,     //> 法器强化等级
    MAGIC_STRENGTHEN_ATTR_BASE, //> 法器强化属性
    MAGIC_STRENGTHEN_ATTR_0,    //> 法器强化基础属性
    MAGIC_STRENGTHEN_ATTR_1,    //> 法器强化附加属性

    MAGIC_REFINE_LEVEL,         //> 法器精炼等级
    MAGIC_REFINE_ATTR_BASE,     //> 法器精炼属性
    MAGIC_REFINE_ATTR_0,        //> 法器精炼基础属性
    MAGIC_REFINE_ATTR_1,        //> 法器精炼附加属性

    PET_UPGRADE_LEVEL,          //> 符灵升级面板等级
    PET_BREAK_LEVEL,            //> 符灵突破等级
    PET_BREAK_FINAL_LEVEL,      //> 符灵突破最终等级

    MAX,
}
/// <summary>
/// 表示属性变化反馈文字所在的面板
/// </summary>
public enum ChangeTipPanelType
{
    NONE,
    PET_LEVEL_UP_INFO_WINDOW,       //> 符灵升级面板
    BREAK_INFO_WINDOW,              //> 符灵突破面板
    TEAM_POS_INFO_WINDOW,           //> 符灵属性面板
    TEAM_INFO_WINDOW,               //> 队伍面板
    EQUIP_STRENGTHEN_INFO_WINDOW,   //> 装备强化面板
    EQUIP_REFINE_INFO_WINDOW,       //> 装备精炼面板
    MAGIC_STRENGTHEN_INFO_WINDOW,   //> 法器强化面板
    MAGIC_REFINE_INFO_WINDOW,       //> 法器精炼面板

    COMMON,                         //> 通用,战斗力等显示
    MAX,
}
/// <summary>
/// 反馈文字优先级，越大优先级越高
/// </summary>
public enum ChangeTipPriority 
{
    STRENGTHEN_ATTR = -2,    //> 强化属性
    STRENGTHEN_LEVEL = -1,   //> 强化等级
    NONE,    
    PET_LEVEL,               //> 符灵升级等级
    PET_BREAK_LEVEL,         //> 符灵突破等级
    POWER,                   //> 战斗力
    MASTER,                  //> 强化大师
    RELATE,                  //> 缘分
}

public enum GRID_POS 
{
    TEAM_INFO,              //> 队伍-队伍页签
    TEAM_PET_PACKAGE,       //> 队伍-符灵页签
    
}
/// <summary>
/// 反馈文字管理
/// </summary>
public class ChangeTipManager : ManagerSingleton<ChangeTipManager>
{
    /// <summary>
    /// 带优先级的反馈内容
    /// </summary>
    public class PriorityChangeTip
    {
        public ChangeTip mChangeTipContent;     //> 反馈内容
        public int mPriority;                   //> 排序优先级
        public PriorityChangeTip(ChangeTip kContent, int kPriority)
        {
            this.mChangeTipContent = kContent;
            this.mPriority = kPriority;
        }
    }

    public ChangeTipPanelType CurShowPanelType; //> 当前反馈文字所在界面
    public List<PriorityChangeTip> mChangeTipList = new List<PriorityChangeTip>();  //> 反馈文字列表
    public UIGridContainer mGridContainer = null;
    private Dictionary<ChangeTipPanelType, Dictionary<ChangeTipValueType, string>> mTipTargetDic = new Dictionary<ChangeTipPanelType, Dictionary<ChangeTipValueType, string>>(); //> 存放目标物体的名称
    public Dictionary<ChangeTipPanelType, Dictionary<ChangeTipValueType, string>> TargetDic 
    {
        get { return mTipTargetDic; }
    }
    public void InitTargetDic() 
    {
        mTipTargetDic.Clear();
        //1.通用面板
        Dictionary<ChangeTipValueType, string> _commonDic = new Dictionary<ChangeTipValueType, string>();
        _commonDic.Add(ChangeTipValueType.POWER,"");
        _commonDic.Add(ChangeTipValueType.ATTACK, "");
        _commonDic.Add(ChangeTipValueType.HP, "");
        _commonDic.Add(ChangeTipValueType.PHYSICAL_DEFENCE, "");
        _commonDic.Add(ChangeTipValueType.MAGIC_DEFENCE, "");
        mTipTargetDic.Add(ChangeTipPanelType.COMMON, _commonDic);
        //2.符灵属性面板
        Dictionary<ChangeTipValueType,string> _petAttrInfoDic = new Dictionary<ChangeTipValueType,string>();
        _petAttrInfoDic.Add(ChangeTipValueType.ATTACK, "base_attribute_group/num");
        _petAttrInfoDic.Add(ChangeTipValueType.HP, "hp/num");
        _petAttrInfoDic.Add(ChangeTipValueType.PHYSICAL_DEFENCE, "physical_defense/num");
        _petAttrInfoDic.Add(ChangeTipValueType.MAGIC_DEFENCE, "magic_defense/num");
        mTipTargetDic.Add(ChangeTipPanelType.TEAM_POS_INFO_WINDOW, _petAttrInfoDic);
        //3.队伍面板
        Dictionary<ChangeTipValueType, string> _teamInfoDic = new Dictionary<ChangeTipValueType, string>();
        _teamInfoDic.Add(ChangeTipValueType.MASTER, "strengthen_master_btn");
        mTipTargetDic.Add(ChangeTipPanelType.TEAM_INFO_WINDOW, _teamInfoDic);
        //4.装备强化信息面板
        Dictionary<ChangeTipValueType, string> _equipstrengthenDic = new Dictionary<ChangeTipValueType, string>();
        _equipstrengthenDic.Add(ChangeTipValueType.EQUIP_STRENGTHEN_LEVEL, "cur_attribute_group/LevelLabel");
        _equipstrengthenDic.Add(ChangeTipValueType.EQUIP_STRENGTHEN_ATTR_0, "cur_attribute_group/cur_attribute_0/num_label");
        mTipTargetDic.Add(ChangeTipPanelType.EQUIP_STRENGTHEN_INFO_WINDOW, _equipstrengthenDic);
        //5.装备精炼信息面板
        Dictionary<ChangeTipValueType, string> _equipRefineDic = new Dictionary<ChangeTipValueType, string>();
        _equipRefineDic.Add(ChangeTipValueType.EQUIP_REFINE_LEVEL, "refine_level_label/base_number_label");
        _equipRefineDic.Add(ChangeTipValueType.EQUIP_REFINE_BASE_ATTR, "base_attuibute_label/base_number_label");
        _equipRefineDic.Add(ChangeTipValueType.EQUIP_REFINE_SUB_ATTR, "subjoin_attuibute_label/base_number_label");
        mTipTargetDic.Add(ChangeTipPanelType.EQUIP_REFINE_INFO_WINDOW, _equipRefineDic);
        //6.法器强化信息面板
        Dictionary<ChangeTipValueType, string> _magicStrengthenDic = new Dictionary<ChangeTipValueType, string>();
        _magicStrengthenDic.Add(ChangeTipValueType.MAGIC_STRENGTHEN_LEVEL, "strengthen_exp_info/cur_strengthen_bar/level_label");
        _magicStrengthenDic.Add(ChangeTipValueType.MAGIC_STRENGTHEN_ATTR_0, "attribute_label_Grid/attribute_label(Clone)_0/base_number_label");
        _magicStrengthenDic.Add(ChangeTipValueType.MAGIC_STRENGTHEN_ATTR_1, "attribute_label_Grid/attribute_label(Clone)_1/base_number_label");
        mTipTargetDic.Add(ChangeTipPanelType.MAGIC_STRENGTHEN_INFO_WINDOW, _magicStrengthenDic);
        //7.法器精炼信息面板
        Dictionary<ChangeTipValueType, string> _magicRefineDic = new Dictionary<ChangeTipValueType, string>();
        _magicRefineDic.Add(ChangeTipValueType.MAGIC_REFINE_LEVEL, "attribute_change_label/num");
        _magicRefineDic.Add(ChangeTipValueType.MAGIC_REFINE_ATTR_0, "attribute(Clone)_0/cur_number_label");
        _magicRefineDic.Add(ChangeTipValueType.MAGIC_REFINE_ATTR_1, "attribute(Clone)_1/cur_number_label");
        mTipTargetDic.Add(ChangeTipPanelType.MAGIC_REFINE_INFO_WINDOW, _magicRefineDic);
        //8.符灵升级信息面板
        Dictionary<ChangeTipValueType, string> _petUpgradeInfo = new Dictionary<ChangeTipValueType, string>();
        _petUpgradeInfo.Add(ChangeTipValueType.PET_UPGRADE_LEVEL, "cur_level_up_bar/level_label");
        _petUpgradeInfo.Add(ChangeTipValueType.ATTACK, "attribute_label(Clone)_0/pet_base_number_label");
        _petUpgradeInfo.Add(ChangeTipValueType.HP, "attribute_label(Clone)_1/pet_base_number_label");
        _petUpgradeInfo.Add(ChangeTipValueType.PHYSICAL_DEFENCE, "attribute_label(Clone)_2/pet_base_number_label");
        _petUpgradeInfo.Add(ChangeTipValueType.MAGIC_DEFENCE, "attribute_label(Clone)_3/pet_base_number_label");
        mTipTargetDic.Add(ChangeTipPanelType.PET_LEVEL_UP_INFO_WINDOW, _petUpgradeInfo);
        //9.符灵突破信息面板
        Dictionary<ChangeTipValueType, string> _petBreakInfo = new Dictionary<ChangeTipValueType, string>();
        _petBreakInfo.Add(ChangeTipValueType.PET_BREAK_LEVEL, "pet_break_info/label_info/break_number");
        _petBreakInfo.Add(ChangeTipValueType.PET_BREAK_FINAL_LEVEL, "last_pet_info/break_number");
        _petBreakInfo.Add(ChangeTipValueType.ATTACK, "basic_attribute(Clone)_2/basic_attribute_number_label1");
        _petBreakInfo.Add(ChangeTipValueType.HP, "basic_attribute(Clone)_3/basic_attribute_number_label1");
        _petBreakInfo.Add(ChangeTipValueType.PHYSICAL_DEFENCE, "basic_attribute(Clone)_1/basic_attribute_number_label1");
        _petBreakInfo.Add(ChangeTipValueType.MAGIC_DEFENCE, "basic_attribute(Clone)_0/basic_attribute_number_label1");
        mTipTargetDic.Add(ChangeTipPanelType.BREAK_INFO_WINDOW, _petBreakInfo);
    }
    public GameObject FindTargetObj(GameObject kParentObj,string kTargetName)
    {
        string[] _targetNameArr = kTargetName.Split('/');
        GameObject _targetObj = null;
        GameObject _parentObj = kParentObj;
        for (int i = 0, count = _targetNameArr.Length; i < count; i++) 
        {
            _targetObj = GameCommon.FindObject(_parentObj,_targetNameArr[i]);
            _parentObj = _targetObj;
        }
        return _targetObj;
    }

    /// <summary>
    /// 按照默认优先级0入队，数字越大，优先级越高
    /// </summary>
    /// <param name="kContent"></param>
    public void Enqueue(ChangeTip kContent) 
    {
        Enqueue(kContent,0);
    }
    /// <summary>
    /// 按指定优先级入队，数字越大，优先级越高
    /// </summary>
    /// <param name="kContent"></param>
    /// <param name="kPriority"></param>
    public void Enqueue(ChangeTip kContent,int kPriority) 
    {
        //if (mClearFlag)
        //{
        //    Clear();
        //    mClearFlag = false;
        //}
        bool inserted = false;
        PriorityChangeTip _pTip = new PriorityChangeTip(kContent,kPriority);
        for (int i = mChangeTipList.Count - 1; i >= 0; --i)
        {
            if (_pTip.mPriority <= mChangeTipList[i].mPriority)
            {
                inserted = true;
                mChangeTipList.Insert(i + 1, new PriorityChangeTip(kContent, kPriority));
                break;
            }
        }
        if (!inserted)
        {
            mChangeTipList.Insert(0, _pTip);
        }
    }

    public void Clear() 
    {
        mChangeTipList.RemoveRange(0, mChangeTipList.Count);
    }

    private void SetChangeTipTweenPosAnim(GameObject kParentObj,ChangeTip kTip,System.Action kAction = null) 
    {
        GameObject _changeLblObj = GameCommon.FindObject(kParentObj, "change_tip_label");
        TweenPosition[] _tweenPosArr = _changeLblObj.GetComponents<TweenPosition>();
        if (_tweenPosArr == null)
            return;
        TweenScale[] _tweenScaleArr = _changeLblObj.GetComponents<TweenScale>();
        if (_tweenScaleArr == null)
            return;
        if (_tweenPosArr.Length < 2 || _tweenScaleArr.Length < 2)
            return;

        //TweenPosition _tweenPos = GameCommon.FindComponent<TweenPosition>(kParentObj, "change_tip_label");
        //if (_tweenPos == null)
        //    return;
        //TweenScale _tweenScale = GameCommon.FindComponent<TweenScale>(kParentObj, "change_tip_label");
        TweenAlpha _tweenAlpha = GameCommon.FindComponent<TweenAlpha>(kParentObj,"change_tip_label");

        if (kTip.TargetObj != null) 
        {
            _tweenPosArr[0].onFinished.Add(new EventDelegate(() =>
            {
                if (kTip.TargetObj != null)
                {
                    ChangeLabel _changeLabel = kTip.TargetObj.GetComponent<ChangeLabel>();
                    if (_changeLabel == null)
                    {
                        _changeLabel = kTip.TargetObj.AddComponent<ChangeLabel>();
                    }
                    _changeLabel.TargetValue = kTip.TargetValue;
                    _changeLabel.ShowType = kTip.ShowType;
                    _changeLabel.Precision = kTip.Precision;
                    _changeLabel.PlayAnim();
                }

                GameObject.Destroy(_changeLblObj);
                ChangeTipManager.Self.CurShowPanelType = ChangeTipPanelType.NONE;
                if (kAction != null)
                {
                    kAction();
                }
            }) { oneShot = true });
        }
       
        // 需要飘到目标物体处
        if (kTip.TargetObj != null)
        {
            _tweenPosArr[1].enabled = false;
            _tweenScaleArr[1].enabled = false;

            _tweenPosArr[0].enabled = false;
            _tweenScaleArr[0].enabled = false;
            if (_tweenAlpha != null)
                _tweenAlpha.enabled = false;
            //by chenliang
            //begin
            GlobalModule.DoOnNextUpdate(5, () =>
            {
                if (kParentObj.gameObject != null && kTip.TargetObj.gameObject != null)
                    kParentObj.transform.parent = kTip.TargetObj.transform.parent;
                if (_tweenPosArr != null && _tweenPosArr[0] != null)
                {
                    _tweenPosArr[0].worldSpace = true;
					_tweenPosArr[0].from = _tweenPosArr[0].transform.position;
					
					GameObject kTipParentObj = kTip.TargetObj.transform.parent.gameObject ;
					if(kTipParentObj != null )
					{
                        if (kTipParentObj.name == "cur_attribute_0" || kTipParentObj.name == "cur_attribute_group")//装备强化 位置要在右边
						{
							_tweenPosArr[0].from = new Vector3(kTip.TargetObj.transform.position.x + 0.25f, kTip.TargetObj.transform.position.y + 0.2f, kTip.TargetObj.transform.position.z);
						}
                        else if (kTip.TargetValueType == ChangeTipValueType.EQUIP_REFINE_SUB_ATTR || kTip.TargetValueType == ChangeTipValueType.EQUIP_REFINE_LEVEL || kTip.TargetValueType == ChangeTipValueType.EQUIP_REFINE_BASE_ATTR) //装备精炼
                        {
                            _tweenPosArr[0].from = new Vector3(kTip.TargetObj.transform.position.x + 0.25f, kTip.TargetObj.transform.position.y + 0.2f, kTip.TargetObj.transform.position.z);
                        }
                        else if (kTip.TargetValueType == ChangeTipValueType.MAGIC_STRENGTHEN_LEVEL || kTip.TargetValueType == ChangeTipValueType.MAGIC_STRENGTHEN_ATTR_0 || kTip.TargetValueType == ChangeTipValueType.MAGIC_STRENGTHEN_ATTR_1)//法器强化
                        {
                            _tweenPosArr[0].gameObject.transform.localPosition = new Vector3(300f,0f,0f);
                            _tweenPosArr[0].from = new Vector3(_tweenPosArr[0].gameObject.transform.position.x , kTip.TargetObj.transform.position.y + 0.2f, kTip.TargetObj.transform.position.z);
                        }
                        else if (kTip.TargetValueType == ChangeTipValueType.MAGIC_REFINE_LEVEL || kTip.TargetValueType == ChangeTipValueType.MAGIC_REFINE_ATTR_0 || kTip.TargetValueType == ChangeTipValueType.MAGIC_REFINE_ATTR_1)//法器精炼
                        {
                            _tweenPosArr[0].from = new Vector3(kTip.TargetObj.transform.position.x + 0.25f, kTip.TargetObj.transform.position.y + 0.2f, kTip.TargetObj.transform.position.z);
                        }
					}
					//
                    _tweenPosArr[0].to = kTip.TargetObj.transform.position;
                    _tweenPosArr[0].ResetToBeginning();
                    _tweenPosArr[0].enabled = true;
                }
                if (_tweenScaleArr != null && _tweenScaleArr[0] != null) 
                    _tweenScaleArr[0].enabled = true;
            });

            //end
  
        }
        else 
        {
            //by chenliang
            //begin
            GlobalModule.DoOnNextUpdate(5, () =>
            {
                if (_tweenPosArr != null && _tweenPosArr[0] != null)
                {
                    _tweenPosArr[0].enabled = false;
                    _tweenScaleArr[0].enabled = false;
                }

                if (_tweenScaleArr != null && _tweenScaleArr[1] != null)
                {
                    _tweenScaleArr[1].enabled = true;
                    _tweenPosArr[1].enabled = true;
                }
                if (_tweenAlpha != null)
                    _tweenAlpha.enabled = true;
            });

            //end
        }
        
    }

    public void ChangeGridPos(GRID_POS kGridPosType) 
    {
        if (mGridContainer != null) 
        {
            if (mGridContainer.transform.parent != null) 
            {
                Vector3 _originPos = mGridContainer.transform.parent.localPosition;
                Vector3 _newPos = Vector3.zero;
                switch (kGridPosType) 
                {
                    case GRID_POS.TEAM_INFO:
                        _newPos = new Vector3(0f, _originPos.y, _originPos.z);
                        break;
                    case GRID_POS.TEAM_PET_PACKAGE:
                        _newPos = new Vector3(100f, _originPos.y, _originPos.z);
                        break;
                    default:
                        _newPos = _originPos;
                        break;
                }
                mGridContainer.transform.parent.localPosition = _newPos;
            }
        }
    }
    private void SetWidgetPivot(UIWidget kSource,UIWidget kTarget) 
    {
        if (kSource == null || kTarget == null)
            return;
        kSource.pivot = kTarget.pivot;
    }

    public void PlayAnim(System.Action kAction = null) 
    {
        //设置默认面板类型
        DataCenter.Set("CHANGE_TIP_PANEL_TYPE", ChangeTipPanelType.COMMON);

        if (mChangeTipList == null || mChangeTipList.Count == 0)
            return;
        if (mGridContainer != null)
        {
			int iCount = 0;
            mGridContainer.MaxCount = 0;
            GlobalModule.DoOnNextUpdate(() => 
            {
                mGridContainer.MaxCount = mChangeTipList.Count;
                mGridContainer.transform.localPosition = new Vector3(0f, (mChangeTipList.Count - 1) * 0.5f * mGridContainer.CellHeight, 0f);
                for (int i = 0, count = mChangeTipList.Count; i < count; i++)
                {
                    GameObject _item = mGridContainer.controlList[i];
                    GameObject _aimTarget = mChangeTipList[i].mChangeTipContent.TargetObj;
                    if (_aimTarget != null)
                    {
                        GameObject _aimPos = _aimTarget.transform.parent.gameObject;
                        if (_aimPos.name == "cur_attribute_0" || _aimPos.name == "cur_attribute_group")
                        {
                            iCount++;
                            GameCommon.FindObject(_item, "change_tip_label").transform.position = new Vector3(_aimPos.transform.position.x, _aimPos.transform.position.y + 20f, _aimPos.transform.position.z);
                        }
                        else if (mChangeTipList[i].mChangeTipContent.TargetValueType == ChangeTipValueType.EQUIP_REFINE_SUB_ATTR || mChangeTipList[i].mChangeTipContent.TargetValueType == ChangeTipValueType.EQUIP_REFINE_LEVEL || mChangeTipList[i].mChangeTipContent.TargetValueType == ChangeTipValueType.EQUIP_REFINE_BASE_ATTR)
                        {
                            iCount++;
                            GameCommon.FindObject(_item, "change_tip_label").transform.position = new Vector3(_aimPos.transform.position.x, _aimPos.transform.position.y + 20f, _aimPos.transform.position.z);
                        }
                        else if (mChangeTipList[i].mChangeTipContent.TargetValueType == ChangeTipValueType.MAGIC_STRENGTHEN_LEVEL || mChangeTipList[i].mChangeTipContent.TargetValueType == ChangeTipValueType.MAGIC_STRENGTHEN_ATTR_0 || mChangeTipList[i].mChangeTipContent.TargetValueType == ChangeTipValueType.MAGIC_STRENGTHEN_ATTR_1)
                        {
                            iCount++;
                            GameCommon.FindObject(_item, "change_tip_label").transform.position = new Vector3(_aimPos.transform.position.x, _aimPos.transform.position.y + 20f, _aimPos.transform.position.z);
                        }
                        else if (mChangeTipList[i].mChangeTipContent.TargetValueType == ChangeTipValueType.MAGIC_REFINE_LEVEL || mChangeTipList[i].mChangeTipContent.TargetValueType == ChangeTipValueType.MAGIC_REFINE_ATTR_0 || mChangeTipList[i].mChangeTipContent.TargetValueType == ChangeTipValueType.MAGIC_REFINE_ATTR_1)
                        {
                            iCount++;
                            GameCommon.FindObject(_item, "change_tip_label").transform.position = new Vector3(_aimPos.transform.position.x, _aimPos.transform.position.y + 20f, _aimPos.transform.position.z);
                        }
                    }
                    UILabel _tipLbl = GameCommon.FindComponent<UILabel>(_item, "change_tip_label");
                    if (_tipLbl != null)
                    {
                        _tipLbl.text = mChangeTipList[i].mChangeTipContent.Content;
                        SetChangeTipTweenPosAnim(_item, mChangeTipList[i].mChangeTipContent, kAction);
                    }
                }
                if (iCount != 0)
                    mGridContainer.transform.localPosition = new Vector3(0f, (mChangeTipList.Count - iCount - 1) * 0.5f * mGridContainer.CellHeight, 0f);

                Clear();
            });

        }
     
    }

    /// <summary>
    /// 检测反馈文字在切换界面的时候是否需要销毁
    /// </summary>
    public void  CheckChangeTipShowState(GameObject kGameObjectUI) 
    {
        DestroyChangeTip();
        ChangeTipManager.Self.CurShowPanelType = ChangeTipPanelType.NONE;
        //if (ChangeTipPanelType.NONE == CurShowPanelType)
        //    return;
        //object _tWinObj = DataCenter.GetData(CurShowPanelType.ToString());
        //if (_tWinObj == null)
        //    return;
        //tWindow _tWin = _tWinObj as tWindow;
        //if (_tWin != null && !_tWin.IsVisible())
        //{
        //    DestroyChangeTip();
        //    ChangeTipManager.Self.CurShowPanelType = ChangeTipPanelType.NONE;
        //}      
    }
    private void DestroyChangeTip() 
    {
        if (mGridContainer == null)
            return;
        for (int i = 0, count = mGridContainer.controlList.Count; i < count; i++) 
        {
            GameObject.Destroy(mGridContainer.controlList[i]);
        }
        mGridContainer.MaxCount = 0;
        mChangeTipList.Clear();
    }
}
