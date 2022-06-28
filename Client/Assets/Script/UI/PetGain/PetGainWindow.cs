using UnityEngine;
using System.Collections;
using System;
using DataTable;
using System.Collections.Generic;

public struct PetGainDeliverData 
{
    public int mPetTid;
    public Action mCallback;
};

/// <summary>
/// 获得高品质符灵(紫/橙)的时候的弹窗
/// </summary>
public class PetGainWindow : tWindow
{
    //顺序
    //1.显示特效
    //2.出现符灵模型
    //3.播放动画
    //4.文字信息动态UI进入
    //5.点击屏幕继续

    /// <summary>
    /// 检测获得的物品是否是高品质符灵(紫色品质以上)
    /// </summary>
    /// <param name="kPetTid"></param>
    /// <returns></returns>
    public static bool CheckPetGainQuality(int kPetTid) 
    {
        if (ITEM_TYPE.PET == PackageManager.GetItemTypeByTableID(kPetTid))
        {
            if (GameCommon.GetItemQuality(kPetTid) >= (int)PET_QUALITY.PURPLE)
            {
                if(!GameCommon.IsExpPet(kPetTid))
                    return true;
            }
        }
        return false;
    }

    public override void Init()
    {
        base.Init();
        Logic.EventCenter.Self.RegisterEvent("PlayPetGainDynamicUI",new Logic.DefineFactory<PlayPetGainDynamicUI>());
    }

    private BaseObject mModel = null;
    private bool mAnimEnd = false;
    private IEnumerator<int> mTidEnumerator;

    public override void Open(object param) 
    {
        base.Open(param);
        mTidEnumerator = null;

        CleanModel();
        if (param != null && param is PetGainDeliverData) 
        {
            mPetGainData = (PetGainDeliverData)param;
            RefreshPetInfo(mPetGainData.mPetTid);
        }
        else if (param != null && param is int) 
        {
            mPetGainData.mPetTid = (int)param;
            mPetGainData.mCallback = null;
            RefreshPetInfo((int)param);
        }
        else if (param != null && param is IEnumerator<int>) 
        {
            mTidEnumerator = QualityFilter((IEnumerator<int>)param);
            if (mTidEnumerator.MoveNext())
            {
                RefreshPetInfo(mTidEnumerator.Current);
            }
            else 
            {
                DEBUG.logError("没有包含数据");
                Close();
            }
        }
    }
    public override void OnOpen()
    {
        base.OnOpen();
        mAnimEnd = false;
    }

    public PetGainDeliverData mPetGainData;

    private IEnumerator<int> QualityFilter(IEnumerator<int> param) 
    {
        List<int> _highTidList = new List<int>();
        while (param.MoveNext()) 
        {
            if (CheckPetGainQuality(param.Current))
            {
                _highTidList.Add((param.Current));
            }
        }
        return _highTidList.GetEnumerator();
    }
    //如果传过来的是一组tid，则设置该回调
    private void ContinueCallback() 
    {
        if (mTidEnumerator.MoveNext())
        {
            CleanModel();
            mAnimEnd = false;
            RefreshPetInfo(mTidEnumerator.Current);
        }
        else 
        {
            mTidEnumerator = null;
            Close();
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex) 
        {
            case "SET_PET_GAIN_DATA":
                if (objVal != null && objVal is PetGainDeliverData)
                {
                    mPetGainData = (PetGainDeliverData)objVal;
                }
                break;
            case "ANIM_END_CALLBACK":
                AnimEndCallback();
                break;
        }
    }

    private string[] qualityName = { "a_ui_gpzfuling01", "a_ui_gpzfuling02" };
    /// <summary>
    /// 刷新符灵信息
    /// </summary>
    /// <param name="kPetTid"></param>
    private void RefreshPetInfo(int kPetTid)
    {
        //1.名称
        UILabel _nameLbl = GameCommon.FindComponent<UILabel>(mGameObjUI,"name_label");
        if(_nameLbl != null)
        {
            _nameLbl.text = GameCommon.GetItemName(kPetTid);
            _nameLbl.color = GameCommon.GetNameColor(kPetTid);
        }
        //2.属性
        GameCommon.SetPetElementIcon(mGameObjUI, "element_icon",kPetTid);
        GameCommon.SetPetElementName(mGameObjUI, "element_label",kPetTid);
        //3.攻击类型
        GameCommon.SetPetTypeIcon(mGameObjUI, "type_icon",kPetTid);
        GameCommon.SetPetTypeName(mGameObjUI, "type_label",kPetTid);
        //4.符灵颜色(品质4是紫色)
        UISprite _qualitySprite = GameCommon.FindComponent<UISprite>(mGameObjUI, "quality_color_sprite");
        int _qualityIndex = GameCommon.GetItemQuality(kPetTid) - (int)PET_QUALITY.PURPLE;
        if (0 <= _qualityIndex && _qualityIndex < qualityName.Length)
        {
            _qualitySprite.spriteName = qualityName[_qualityIndex];
        }       
        //5.绑定回调
        SetClickContinueCallback();
        //6.播放特效
        GameObject _advanceEffect = GameCommon.FindObject(mGameObjUI, "new_ui_choukaadvanced");
        if (_advanceEffect != null) 
        {
            if (!_advanceEffect.activeSelf)
            {
                _advanceEffect.SetActive(true);
            }
            ParticleSystem _ps = GameCommon.FindComponent<ParticleSystem>(_advanceEffect, "hit_3");
            if (_ps != null) 
            {
                _ps.Play(true);
                if (Settings.IsSoundEffectEnabled())
                {
                    DataRecord record = DataCenter.mEffectSound.GetRecord("new_ui_choukaadvanced");
                    if (record != null)
                    {
                        string filePath = record.get("SOUND_FILE");
                        int fileLevel = record.get("SOUND_TYPE");
                        GameCommon.PlaySound(filePath, GameCommon.GetMainCamera().transform.position, fileLevel);
                    }
                }
            }
        }
        
        GlobalModule.DoLater(()=>
        {
            //7.加载模型
            LoadModel(kPetTid);
            //8.播放动画
            mModel.mMotionFinishCallBack = Logic.EventCenter.Start("PlayPetGainDynamicUI");
            mModel.PlayAnim("cute", false);
        },0.43f);
        
        //9.点击继续
        GameCommon.SetUIText(mGameObjUI, "click_continue","");

        //10.初始化文字位置
        TweenPosition _upTweenPos = GameCommon.FindComponent<TweenPosition>(mGameObjUI,"up_root");
        if (_upTweenPos != null) 
        {
            _upTweenPos.ResetToBeginning();
            _upTweenPos.enabled = false;
        }
        TweenPosition _downTweenPos = GameCommon.FindComponent<TweenPosition>(mGameObjUI,"down_root");
        if (_downTweenPos != null) 
        {
            _downTweenPos.ResetToBeginning();
            _downTweenPos.enabled = false;
        }
    }

    private void JumpAnim() 
    {
        if (mModel != null)
            mModel.ResetIdle();
        DataCenter.SetData("PET_GAIN_WINDOW", "ANIM_END_CALLBACK", null);
    }

    private void SetClickContinueCallback() 
    {
        GameObject _continueObj = GameCommon.FindObject(mGameObjUI, "collider_mask");
        if (mTidEnumerator != null) 
        {
            AddButtonAction(_continueObj, () =>
            {
                if (!mAnimEnd)
                {
                    JumpAnim();
                    return;
                }
                ContinueCallback();
            });
            return;
        }

        if (mPetGainData.mCallback == null) 
        {
            AddButtonAction(_continueObj, () => 
            {
                if (!mAnimEnd) 
                {
                    JumpAnim();
                    return;
                }
                Close();
            });
        }
        else
        {
            AddButtonAction(_continueObj, () => 
            {
                if (!mAnimEnd) 
                {
                    JumpAnim();
                    return;
                }
                mPetGainData.mCallback.Invoke();
                mPetGainData.mCallback = null;
                Close();
            });
        }
    }

    private void LoadModel(int kPetTid)
    {
        GameObject tmpGOUIPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
        mModel = GameCommon.ShowModel(kPetTid, tmpGOUIPoint, 1.0f);
        if (mModel != null)
        {
            mModel.SetVisible(true);
            mModel.mMainObject.transform.localRotation = Quaternion.Euler(0f, 180, 0f);
        }
    }

    //动画结束后的回调方法
    private void AnimEndCallback() 
    {
        string _continueHint = TableCommon.getStringFromStringList(STRING_INDEX.COMMON_CLICK_TO_CONTINUE);
        GameCommon.SetUIText(mGameObjUI, "click_continue", _continueHint);

        //播放文字动态效果
        TweenPosition _upTweenPos = GameCommon.FindComponent<TweenPosition>(mGameObjUI,"up_root");
        if (_upTweenPos != null) 
        {
            _upTweenPos.PlayForward();
            _upTweenPos.onFinished.Add(new EventDelegate(() => { mAnimEnd = true; }) { oneShot = true});
        }
        TweenPosition _downTweenPos = GameCommon.FindComponent<TweenPosition>(mGameObjUI,"down_root");
        if (_downTweenPos != null) 
        {
            _downTweenPos.PlayForward();
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        CleanModel();
    }

    private void CleanModel()
    {
        if (mModel != null)
        {
            mModel.Destroy();
            mModel.OnDestroy();
            mModel = null;
        }
    }
}


public class PlayPetGainDynamicUI : Logic.CEvent 
{
    public override void CallBack(object caller)
    {
        base.CallBack(caller);
        BaseObject obj = caller as BaseObject;
        if (obj != null)
            obj.ResetIdle();
        DataCenter.SetData("PET_GAIN_WINDOW", "ANIM_END_CALLBACK",null);
    }

}