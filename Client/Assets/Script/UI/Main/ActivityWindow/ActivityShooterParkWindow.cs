using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using System;
using System.Linq;

/// <summary>
/// 射手乐园活动
/// </summary>
public class ActivityShooterParkWindow : tWindow
{
    public static int MAX_DAY = 3;                       //> 射手乐园持续时间
    public UIGridContainer mGrid = null;
    private GameObject _turnTableRoot = null;
    private int mTurnPanelRewardCount = 6;               //> 转盘奖励项目的个数 
    private List<int> mRewardNumList = new List<int>();  //> 奖励元宝的数量的列表
    private BaseObject mModel;
    private int mTotalShootTimes = 0;                    //> 总的射击次数
    private TurnTableRotate mTurnTableScript = null;
    private int mRewardNum = 0;                          //> 奖励元宝数量
    private GameObject mBulletHole = null;               //> 弹孔
    private UIImageButton mShootBtn;                     //> 射击按钮
    private GameObject mMask = null;
    private GameObject mPointerObj = null;               //> 结束后指示奖励位置的指针
    private GameObject mCurRewardGridItem = null;        //> 当前中奖的位置
    private long mEndSeconds = 0;                        //> 活动结束时间
    private bool m_bCanGetReward = false;                //> 退出时是否需要结算奖励
    // 判断射手乐园当前是否开启
    public static bool GetIsActivityOpen() 
    {
         // 判断射手乐园是否开启
        DateTime startData = DateTime.Parse(CommonParam.mOpenDate);
        Int64 startSeconds = GameCommon.DateTime2TotalSeconds(startData);
        Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime());
        int _elapsedDay = (int)(nowSeconds - startSeconds) / (24 * 60 * 60);
        if (_elapsedDay >= ActivityShooterParkWindow.MAX_DAY)
            return false;
        return true;
    }
    private long ActivityGetOverTime() 
    {
        DateTime startData = DateTime.Parse(CommonParam.mOpenDate);
        Int64 startSeconds = GameCommon.DateTime2TotalSeconds(startData);
        return startSeconds + MAX_DAY * 24 * 60 * 60;
    }
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_activity_shoot_button", new DefineFactory<Button_activity_shoot_button>());
    }

    protected override void OpenInit() 
    {

        mEndSeconds = ActivityGetOverTime();

        mPointerObj = GetCurUIGameObject("pointer");
        mMask = GetCurUIGameObject("mask");
        mMask.SetActive(false);
        mBulletHole = GetCurUIGameObject("bullet_hole");
        mTotalShootTimes = DataCenter.mShooterParkConfig.GetRecordCount(); // 获得总的射击次数
        _turnTableRoot = GetCurUIGameObject("zhuanpan_root_grid");
        mShootBtn = GetCurUIGameObject("activity_shoot_button").GetComponent<UIImageButton>();
        mTurnTableScript = _turnTableRoot.GetComponent<TurnTableRotate>();
        mTurnTableScript.OnRotationCompleted += TurnTableCompletedHandler;  // 注册委托
        // 第一次打开的时候初始化转盘的结构
        mGrid = _turnTableRoot.GetComponent<UIGridContainer>();
        mGrid.MaxCount = mTurnPanelRewardCount;
        for (int i = 0, count = mGrid.MaxCount; i < count; i++) 
        {
            mGrid.controlList[i].transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, i * (360.0f / count)));
            mGrid.controlList[i].transform.localPosition = Vector3.zero;
        }
    }

    private void InitData() 
    {
        if (mMask != null)
            mMask.SetActive(false);
        m_bCanGetReward = false;
        mGetRewardCoroutine = null;
    }
    public override void Open(object param)
    {
        base.Open(param);
        InitData();
        if(mModel != null)
        {
            mModel.Destroy();
            mModel.OnDestroy();
            mModel = null;
        }
        LoadModel();
        NetManager.RequstShooterParkInfo();
    }

    public override bool Refresh(object param)
    {
        if (param != null && param is string) 
        {
            SC_ShootPark _shootParkReceive = JCode.Decode<SC_ShootPark>(param.ToString());
            int _curShootTimes = _shootParkReceive.usingTimes;
            NiceData tData = GameCommon.GetButtonData(mGameObjUI, "activity_shoot_button");
            if (tData != null)
            {
                tData.set("ShootTimes", _curShootTimes);    // 设置当前射击次数
            }
            //隐藏指针
            mPointerObj.SetActive(false);
            //隐藏弹孔
            mBulletHole.SetActive(false);
            //剩余时间
            //long _endSeconds = _shootParkReceive.endTime;
            SetCountdown(mGameObjUI, "LeftTimeCount", mEndSeconds, new CallBack(this, "RemainTimeOver", null));
            string _readerIndex = (_curShootTimes + 1).ToString();
            DataRecord _record = DataCenter.mShooterParkConfig.GetRecord(_readerIndex);
            //1.累计获得元宝
            GameObject _accumulateObj = GetCurUIGameObject("Accumulate_num_label");
            UILabel _accumulateNumLbl = GameCommon.FindComponent<UILabel>(_accumulateObj, "num");
            _accumulateNumLbl.text = _shootParkReceive.addDiamond.ToString();
            //2.射击剩余次数
            GameObject _leftTimeObj = GetCurUIGameObject("Left_Shoot_Time_label");
            UILabel _leftTimeLbl = GameCommon.FindComponent<UILabel>(_leftTimeObj, "num");
            _leftTimeLbl.text = (mTotalShootTimes - _curShootTimes).ToString();
            //3.本次消耗
            GameObject _consumeObj = GetCurUIGameObject("ConsumeTitle");
            UILabel _consumeLbl = GameCommon.FindComponent<UILabel>(_consumeObj, "ConsumeCount");
            //4.设置最大获得
            GameObject _canGetObj = GetCurUIGameObject("CanGetTitle");
            UILabel _maxGetLbl = GameCommon.FindComponent<UILabel>(_canGetObj, "ConsumeCount");

            if (_shootParkReceive.usingTimes < mTotalShootTimes)
            {
                _consumeLbl.text = _record.getObject("COST").ToString();
                if (tData != null) 
                {
                    tData.set("CONSUME_YUANBAO", _record.getObject("COST"));
                }
                //5.获得奖励数量
                mRewardNumList.Clear();
                mRewardNumList = GetRewardNumList(_record.getObject("NUMBER").ToString());

                _maxGetLbl.text = mRewardNumList.Max().ToString();
                for (int i = 0; i < mRewardNumList.Count; i++) 
                {
                    GameCommon.FindComponent<UILabel>(mGrid.controlList[i], "reward_num_label").text = mRewardNumList[i].ToString();
                }
                mShootBtn.isEnabled = true;
            }
            else 
            {
                _consumeLbl.text = "";
                _maxGetLbl.text = "";
                for (int i = 0, count = mGrid.MaxCount; i < count; i++)
                {
                    GameCommon.FindComponent<UILabel>(mGrid.controlList[i], "reward_num_label").text = "";
                }
                mShootBtn.isEnabled = false;
            }
        }
        return base.Refresh(param);
    }

    private Coroutine mGetRewardCoroutine = null;
    private void TurnTableCompletedHandler() 
    {
        if (mCurRewardGridItem != null) 
        {
            mPointerObj.transform.parent = mCurRewardGridItem.transform;
            mPointerObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
            mPointerObj.transform.localPosition = new Vector3(-185f, 0f, 0f);
            mPointerObj.SetActive(true);
        }
        mGetRewardCoroutine = GlobalModule.Instance.StartCoroutine(ShowRewardInfo(1.5f));
    }

    private IEnumerator ShowRewardInfo(float kWaitTime) 
    {
        yield return new WaitForSeconds(kWaitTime);
        m_bCanGetReward = false;
         ItemData item = new ItemData() { mType = (int)ITEM_TYPE.YUANBAO, mID = (int)ITEM_TYPE.YUANBAO, mNumber = mRewardNum };
        ActivityTipsLabelUpWindow _tWin = DataCenter.GetData("ACTIVITY_TIPS_LABEL_UP_WINDOW") as ActivityTipsLabelUpWindow;
        _tWin.mCallback = () =>
        {
            mMask.SetActive(false);
            NetManager.RequstShooterParkInfo();
        };
        DataCenter.OpenWindow("ACTIVITY_TIPS_LABEL_UP_WINDOW", item);
    }
    
    public override void Close()
    {
        if (m_bCanGetReward) 
        {
            RoleLogicData.Self.AddDiamond(mRewardNum);
            DataCenter.SetData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND", null);

            if (mGetRewardCoroutine != null) 
            {
                GlobalModule.Instance.StopCoroutine(mGetRewardCoroutine);
                mGetRewardCoroutine = null;
            }
            
            m_bCanGetReward = false;
        }
        base.Close();
    }

    private List<int> GetRewardNumList(string kRewardNumStr)
    {
        List<int> _rewardNumList = new List<int>();
        string[] _str = kRewardNumStr.Split('|');
        for (int i = 0, count = _str.Length; i < count; i++) 
        {
            _rewardNumList.Add(int.Parse(_str[i].Split('#')[0]));
        }
        return _rewardNumList;
    }

    public void RemainTimeOver(object param) 
    {
        
    }
    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex) 
        {
            case "SHOOT":
                mMask.SetActive(true);
                SC_ShootParkShoot _receiveInfo = JCode.Decode<SC_ShootParkShoot>(objVal.ToString());
                m_bCanGetReward = true;
                mRewardNum = (int)_receiveInfo.earnRatio;
                //1.匀速旋转转盘
                mTurnTableScript.StartRotate();
                Logic.tEvent idleEvent = Logic.EventCenter.Self.StartEvent("ShootPark_RoleShoot");
                GlobalModule.DoLater(() => { mModel.PlayMotion("attack_2", idleEvent); }, mTurnTableScript.WaitTime);
                break;
            case "SHOW_BULLET_HOLE":
                if (mRewardNumList != null) 
                {
                    int _rewardIndex = FindIndex(mRewardNum);
                    mCurRewardGridItem = mGrid.controlList[_rewardIndex];
                    mBulletHole.SetActive(true);
                    mBulletHole.transform.parent = mCurRewardGridItem.transform;
                    mBulletHole.transform.localPosition = new Vector3(-70f,0f,0f);
                }
                mTurnTableScript.StartSlowDown();
                break;
        }
    }

    private int FindIndex(int kRewardNum)
    {
        for (int i = 0, count = mRewardNumList.Count; i < count; i++) 
        {
            if (kRewardNum == mRewardNumList[i])
                return i;
        }
        return -1;
    }

    // 加载无双模型
    private void LoadModel()
    {
        GameObject tmpGOUIPoint = GameCommon.FindObject(mGameObjUI, "uipoint");
        mModel = GameCommon.ShowModel(50001, tmpGOUIPoint, 1.6f);
        if (mModel != null) 
        {
            mModel.SetVisible(true);
            mModel.mMainObject.transform.localRotation = Quaternion.Euler(0f,140f,0f); 
        }
    }
}


public class Button_activity_shoot_button : CEvent 
{
    public override bool _DoEvent()
    {
        // 判断当前是否能够进行射击
        //0.判断当前活动是否开启
        if (!ActivityShooterParkWindow.GetIsActivityOpen()) 
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ACTIVITY_SHOOTER_PARK_ACTIVITY_IS_END);
            return true;
        }
        //1.当前是否还有射击机会
        int _maxShootTimes = DataCenter.mShooterParkConfig.GetRecordCount();
        int _alreadyShootTimes = (int)getObject("ShootTimes");
        if (_alreadyShootTimes >= _maxShootTimes)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ACTIVITY_SHOOTERPARK_NO_SHOOT_TIMES);

            return true;
        }
        //2.当前元宝是否充足
        int _curConsume = int.Parse(DataCenter.mShooterParkConfig.GetRecord((_alreadyShootTimes + 1).ToString()).getObject("COST").ToString());
        if (_curConsume > RoleLogicData.Self.diamond)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ACTIVITY_NO_ENOUGH_YUANBAO);
            return true;
        }
        // 消耗元宝
        int _consumeYuanBao = int.Parse(getObject("CONSUME_YUANBAO").ToString());
        RoleLogicData.Self.AddDiamond(-_consumeYuanBao);
        DataCenter.SetData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND",null);
        // 请求
        NetManager.RequestShooterPark_Shoot();

        //added by xuke 红点逻辑
        if (_alreadyShootTimes + 1 >= _maxShootTimes) 
        {
            //SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_SHOOTER_PARK,false);
            DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_SHOOTER_PARK);
        }
        //end
        return true;
    }
}

public class ShooterPark_RoleShoot : CEvent
{
    public override void CallBack(object caller)
    {
        BaseObject obj = caller as BaseObject;
        if (obj != null)
            obj.ResetIdle();
        DataCenter.SetData("ACTIVITY_SHOOTER_PARK_WINDOW", "SHOW_BULLET_HOLE", null);
    }
}