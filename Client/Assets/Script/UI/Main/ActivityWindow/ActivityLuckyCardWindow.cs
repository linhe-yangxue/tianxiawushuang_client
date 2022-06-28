using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using System.Linq;
using DataTable;

/// <summary>
/// 幸运抽牌活动
/// </summary>
public class ActivityLuckyCardWindow : tWindow
{
    private UILabel mStartDrawLbl = null;     //> 开始抽牌提示Label
    private UIGridContainer mGrid = null;
    private List<int> mIndexList = new List<int>();
    private int mMaxCardCount = 0;
    private int mLeftTimes = 0;               //> 当前剩余次数
    private CardRotateSpeed mCardRotSpeedScript = null;
    private int mnRewardNum = 0;              //> 抽到的元宝数量
    private List<int> mRewardList = new List<int>();    //> 奖励数量列表
    List<int> mShowRewardList = new List<int>();        //> 当前奖励列表
    public static bool mbCanDraw = false;     //> 是否可以进行翻牌
    public int mCurDrawIndex = 0;             //> 当前选中的卡牌位置
    public GameObject mEffect = null;         //> 翻牌特效
    private GameObject mMask = null;          //> 遮罩
    private bool mCanDrawReward = false;       //> 是否已经抽取了奖励
    private bool mBreakCoRoutine = false;       //> 是否结束协程动画
    public override void Init()
    {
        base.Init();
        EventCenter.Self.RegisterEvent("Button_activity_start_draw_button", new DefineFactory<Button_activity_start_draw_button>());
        EventCenter.Self.RegisterEvent("Button_reward_item",new DefineFactory<Button_reward_item>());
        EventCenter.Self.RegisterEvent("Button_activity_lucky_card_rule_button", new DefineFactory<Button_activity_lucky_card_rule_button>());
    }
    protected override void OpenInit() 
    {
        mMask = GetCurUIGameObject("mask");
        mEffect = GetCurUIGameObject("effect");
        mCardRotSpeedScript = mGameObjUI.GetComponent<CardRotateSpeed>();
        mStartDrawLbl = GetCurUIGameObject("start_draw_hint_label").GetComponent<UILabel>();
        mGrid = GetCurUIGameObject("card_root_grid").GetComponent<UIGridContainer>();
        mMaxCardCount = mGrid.MaxCount;
        for (int i = 0; i < mMaxCardCount; i++) 
        {
            mIndexList.Add(i);
        }
        mRewardList.Clear();
        foreach(KeyValuePair<int,DataRecord> pair in DataCenter.mLuckCardConfig.GetAllRecord())
        {
            mRewardList.Add((int)pair.Value.getObject("NUMBER"));
        }
    }

    public override void Open(object param)
    {
        base.Open(param);
        InitCard(); 
        NetManager.RequestLuckyCardLeftTimes();
    }

    private void RefreshNewMark() 
    {
        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_LUCKY_CARD);
    }
    public override bool Refresh(object param) 
    {
        if (param != null && param is SC_LuckyCard) 
        {
            SC_LuckyCard _receive = (SC_LuckyCard)param;
            mLeftTimes = _receive.residueNum;
            NiceData tData = GameCommon.GetButtonData(mGameObjUI,"activity_start_draw_button");
            if (tData != null) 
            {
                tData.set("LEFT_TIME",mLeftTimes);
            }
        }
        //剩余次数
        GameObject _leftTimeObj = GetCurUIGameObject("Left_Draw_Time_label");
        UILabel _leftTimesLbl = GameCommon.FindComponent<UILabel>(_leftTimeObj, "num");
        _leftTimesLbl.text = mLeftTimes.ToString();
        //文字提示
        UILabel _hintLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "start_draw_hint_label");
        if (_hintLbl != null)
            _hintLbl.gameObject.SetActive(true);
        if (0 < mLeftTimes)
        {
            _hintLbl.text = "请开始翻牌";
            _hintLbl.SetLabelColor(CommonColorType.WHITE);
        }
        else 
        {
            _hintLbl.text = "请明日再来";
            _hintLbl.SetLabelColor(CommonColorType.DES_RED);
        }
        // 隐藏选中卡牌特效
        mEffect.SetActive(false);
        mMask.SetActive(false);
        return base.Refresh(param);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex) 
        {
            case "START_DRAW":
                if (objVal != null && objVal is SC_LuckyCard_Draw) 
                {
                    mCanDrawReward = true;
                    mMask.SetActive(true) ;
                    SC_LuckyCard_Draw _receive = (SC_LuckyCard_Draw)objVal;
                    mnRewardNum = _receive.diamondIndex;
                    mLeftTimes = _receive.residueNum;
                    StartDrawCard(_receive.diamondIndex);
                    GameObject _hintLblObj = GameCommon.FindObject(mGameObjUI, "start_draw_hint_label");
                    _hintLblObj.SetActive(false);

                    NiceData tData = GameCommon.GetButtonData(mGameObjUI, "activity_start_draw_button");
                    if (tData != null)
                    {
                        tData.set("LEFT_TIME", _receive.residueNum);
                        GameObject _leftTimeObj = GetCurUIGameObject("Left_Draw_Time_label");
                        UILabel _leftTimesLbl = GameCommon.FindComponent<UILabel>(_leftTimeObj, "num");
                        _leftTimesLbl.text = mLeftTimes.ToString();
                    } 
                }
                //added by xuke 红点相关逻辑
                SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_LUCKY_CARD,mLeftTimes > 0);
                RefreshNewMark();
                //end
                break;
            case "TURN_CARD":
                if(objVal != null && objVal is int)
                {
                    int _cardIndex = (int)objVal;
                    mCurDrawIndex = _cardIndex;               
                    TurnCardBack2Forward(mGrid.controlList[_cardIndex], new ItemData() { mType = (int)ITEM_TYPE.YUANBAO, mID = (int)ITEM_TYPE.YUANBAO, mNumber = mnRewardNum }, () => { WaitAndShowAll(); });
                }
                break;
        }
    }

    private void WaitAndShowAll() 
    {
        //1. 显示特效位置
        mEffect.transform.parent = mGrid.controlList[mCurDrawIndex].transform;
        mEffect.transform.localPosition = new Vector3(0f,-90f,0f);
        mEffect.SetActive(true);
        //设置奖励窗口委托
        mCanDrawReward = false;
        ItemData _showItem = new ItemData() { mType = (int)ITEM_TYPE.YUANBAO, mID = (int)ITEM_TYPE.YUANBAO, mNumber = mnRewardNum};
        DataCenter.OpenWindow("ACTIVITY_TIPS_LABEL_UP_WINDOW", _showItem);
        //2. x秒后显示所有卡牌
        GlobalModule.DoLater(() => 
        {
            mShowRewardList.RemoveAt(mRewardIndexInShowRewardList);
            for (int i = 0; i < mMaxCardCount; i++) 
            {
                if (i != mCurDrawIndex) 
                {
                    ItemData _item = new ItemData(){mType = (int)ITEM_TYPE.YUANBAO,mID = (int)ITEM_TYPE.YUANBAO,mNumber = mShowRewardList[0]};
                    mShowRewardList.RemoveAt(0);
                    TurnCardBack2Forward(mGrid.controlList[i], _item);
                }
            }
        },1.5f);

        GlobalModule.DoLater(() => 
        {
            for (int i = 0; i < mMaxCardCount - 1; i++) 
            {
                TurnCardForward2Back(mGrid.controlList[i]);
            }
            if (mEffect != null)
                mEffect.SetActive(false);
            TurnCardForward2Back(mGrid.controlList[mMaxCardCount - 1], () => { NetManager.RequestLuckyCardLeftTimes(); });
        },3f);
    }

    private int mRewardIndexInShowRewardList = 0;
    private void StartDrawCard(int kRewardNum) 
    {
        if (mShowRewardList != null)
            mShowRewardList.Clear();
        // 得到随机奖励
        for (int i = 0; i < mMaxCardCount; i++) 
        {
            mShowRewardList.Add(mRewardList[Random.Range(0, mRewardList.Count)]);
        }
        mRewardIndexInShowRewardList = Random.Range(0, mMaxCardCount);
        mShowRewardList[mRewardIndexInShowRewardList] = kRewardNum;
        //所有牌都显示正面
        for (int i = 0; i < mMaxCardCount-1; i++)
        {
            ItemData _item = new ItemData() { mType = (int)ITEM_TYPE.YUANBAO, mID = (int)ITEM_TYPE.YUANBAO, mNumber = mShowRewardList[i] };
             TurnCardBack2Forward(mGrid.controlList[i], _item);
        }
        // 最后一张牌翻完后执行翻回去的委托
        ItemData _item2 = new ItemData() { mType = (int)ITEM_TYPE.YUANBAO, mID = (int)ITEM_TYPE.YUANBAO, mNumber = mShowRewardList[mMaxCardCount - 1] };
        TurnCardBack2Forward(mGrid.controlList[mMaxCardCount - 1], _item2, () =>
        {
            GlobalModule.DoLater(() =>
            {
                for (int i = 0; i < mMaxCardCount-1; i++)
                {
                    TurnCardForward2Back(mGrid.controlList[i]);
                }
                TurnCardForward2Back(mGrid.controlList[mMaxCardCount - 1], () => { ExchangeCard(); });
            }, 1f);
        });
    }

    // 设置卡牌的正反面背景图
    private void SetCardBGSprite(GameObject kCardObj,bool kIsBack) 
    {
        GameObject _backBGObj = GameCommon.FindObject(kCardObj, "back_bg");
        GameObject _forwardBGObj = GameCommon.FindObject(kCardObj, "forward_bg");
        GameObject _rewardLblObj = GameCommon.FindObject(kCardObj, "reward_num_label");
        GameObject _rewardSpriteObj = GameCommon.FindObject(kCardObj, "reward_sprite");
        if (kIsBack)
        {
            _backBGObj.SetActive(true);
            _forwardBGObj.SetActive(false);
            _rewardLblObj.SetActive(false);
            _rewardSpriteObj.SetActive(false);
        }
        else 
        {
            _backBGObj.SetActive(false);
            _forwardBGObj.SetActive(true);
            _rewardLblObj.SetActive(true);
            _rewardSpriteObj.SetActive(true);
        }
        
    }
    private void InitCard() 
    {
        mCanDrawReward = false;
        mBreakCoRoutine = false;
        for (int i = 0; i < mMaxCardCount; i++) 
        {
            //1.初始化卡牌UI
            SetCardBGSprite(mGrid.controlList[i],true);
            //2.关联按钮ID
            mGrid.controlList[i].GetComponent<UIButtonEvent>().mData.set("CARD_INDEX",i);
            mGrid.controlList[i].transform.localEulerAngles = Vector3.zero;
        }
        //重置位置
        if (mGrid != null) 
        {
            mGrid.MaxCount = 0;
        }
        GlobalModule.DoOnNextUpdate(() => { mGrid.MaxCount = mMaxCardCount; });
        
    }

    // 将指定牌从反面翻到正面
    private void TurnCardBack2Forward(GameObject kObj,ItemData kRewardItem,System.Action kCallback = null)
    {
        GlobalModule.DoCoroutine(RotationAnim(kObj, kRewardItem,false, kCallback));
    }

    // 将指定牌从正面翻到反面
    private void TurnCardForward2Back(GameObject kObj,System.Action kCallback = null)
    {
        GlobalModule.DoCoroutine(RotationAnim(kObj,null, true, kCallback));
    }

    private IEnumerator RotationAnim(GameObject kCardObj,ItemData kRewardItem,bool kToBack,System.Action kCallback)
    {
        if (mBreakCoRoutine)
            yield break;
        //1.从0度转到90度要做个处理
        while (kCardObj.transform.localEulerAngles.y < 90f) 
        {
            if (mBreakCoRoutine)
                yield break;
            kCardObj.transform.Rotate(Vector3.up, mCardRotSpeedScript.mfRotSpeed * Time.deltaTime);
            yield return null;
        }
        kCardObj.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
        //如果是转到正面
        if (!kToBack)
        {
            SetCardBGSprite(kCardObj, false);
            UILabel _rewardLbl = GameCommon.FindComponent<UILabel>(kCardObj, "reward_num_label");
            _rewardLbl.text = kRewardItem.mNumber.ToString() + "元宝";
        }
        else 
        {
            SetCardBGSprite(kCardObj, true);
        }
        //3.旋转回去
        while (0f < kCardObj.transform.localEulerAngles.y && kCardObj.transform.localEulerAngles.y <= 91f) 
        {
            if (mBreakCoRoutine)
                yield break;
            kCardObj.transform.Rotate(Vector3.up, -mCardRotSpeedScript.mfRotSpeed * Time.deltaTime);
            yield return null;
        }
        kCardObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        if (kCallback != null)
            kCallback();
    }



    // 打乱卡牌下标
    private void ShuffleCardIndexList() 
    {
        int _tmpValue = 0;
        int _tmpIndex = 0;
        for (int i = 0, count = 3; i < count; i++) 
        {
            _tmpIndex = Random.Range(0, mMaxCardCount); 
            _tmpValue = mIndexList[_tmpIndex];
        }
    }
    // 交叉变换位置
    private void ExchangeCard() 
    {
        GlobalModule.DoCoroutine(ExchangeCardPos());
    }

    private IEnumerator ExchangeCardPos() 
    {
        Vector3 _firstCardPos = Vector3.zero;
        Vector3 _secondCardPos = Vector3.zero;
        int _firstCardIndex = 0;
        int _secondCardIndex = 0;
        float _deltaPosX = 1f;
        bool _isRight2Left = false;
        for (int i = 0; i < mCardRotSpeedScript.mnExchangeTimes; i++) 
        {
            _deltaPosX = 1f;
            _firstCardIndex = Random.Range(0,mMaxCardCount);
            _secondCardIndex = Random.Range(0, mMaxCardCount);
            while (_secondCardIndex == _firstCardIndex) 
            {
                _secondCardIndex = Random.Range(0,mMaxCardCount);
            }
            _firstCardPos = mGrid.controlList[_firstCardIndex].transform.localPosition;
            _secondCardPos = mGrid.controlList[_secondCardIndex].transform.localPosition;
            //进行位置变换
            _isRight2Left = _firstCardPos.x - _secondCardPos.x > 0f ? true : false;
            while (_deltaPosX > 0f) 
            {
                if (mBreakCoRoutine)
                    yield break;
                if (_isRight2Left)
                {
                    mGrid.controlList[_firstCardIndex].transform.Translate(Vector3.left * mCardRotSpeedScript.mfMoveSpeed * Time.deltaTime);
                    mGrid.controlList[_secondCardIndex].transform.Translate(Vector3.right * mCardRotSpeedScript.mfMoveSpeed * Time.deltaTime);
                    _deltaPosX = mGrid.controlList[_firstCardIndex].transform.localPosition.x - _secondCardPos.x;
                }
                else 
                {
                    mGrid.controlList[_firstCardIndex].transform.Translate(Vector3.right * mCardRotSpeedScript.mfMoveSpeed * Time.deltaTime);
                    mGrid.controlList[_secondCardIndex].transform.Translate(Vector3.left * mCardRotSpeedScript.mfMoveSpeed * Time.deltaTime);
                    _deltaPosX = mGrid.controlList[_secondCardIndex].transform.localPosition.x - _firstCardPos.x;
                }
                yield return null;
            }
            mGrid.controlList[_firstCardIndex].transform.localPosition = _secondCardPos;
            mGrid.controlList[_secondCardIndex].transform.localPosition = _firstCardPos;
        }
        //可以进行抽牌
        UILabel _hintLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "start_draw_hint_label");
        _hintLbl.gameObject.SetActive(true);
        _hintLbl.text = "请选择一张卡牌";
        mbCanDraw = true;
    }

    public override void Close()
    {
        ActivityLuckyCardWindow.mbCanDraw = false;
        mBreakCoRoutine = true;
        if (mMask != null)
            mMask.SetActive(false);
        if (mCanDrawReward)
        {
            RoleLogicData.Self.AddDiamond(mnRewardNum);
            mCanDrawReward = false;
            DataCenter.SetData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND", null);
        }
        GlobalModule.DoOnNextUpdate(() => 
        {
            base.Close();        
        });
    }
}

/// <summary>
/// 开始抽牌按钮
/// </summary>
public class Button_activity_start_draw_button : CEvent 
{
    public override bool _DoEvent()
    {
        int _leftDrawTimes = (int)getObject("LEFT_TIME");
        if (_leftDrawTimes <= 0) 
        {
            //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ACTIVITY_NO_LUCY_CARD_LEFT_TIMES);
            DataCenter.OpenMessageWindow("没有幸运抽卡剩余次数了");
            return true;
        }
        NetManager.RequestLuckCardDraw();
        //test
        //SC_LuckyCard_Draw _receive = new SC_LuckyCard_Draw() { residueNum = 3, diamondIndex = 700 };
        //DataCenter.SetData("ACTIVITY_LUCKY_CARD_WINDOW", "START_DRAW", _receive);
        
        return base._DoEvent();
    }
}

public class Button_reward_item : CEvent
{
    public override bool _DoEvent()
    {
        if (!ActivityLuckyCardWindow.mbCanDraw)
            return true;
        ActivityLuckyCardWindow.mbCanDraw = false;
        int _cardIndex = (int)getObject("CARD_INDEX");
        DataCenter.SetData("ACTIVITY_LUCKY_CARD_WINDOW","TURN_CARD",_cardIndex);
        return base._DoEvent();
    }
}

/// <summary>
/// 游戏规则按钮
/// </summary>
public class Button_activity_lucky_card_rule_button : CEvent 
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("RULE_TIPS_WINDOW",HELP_INDEX.HELP_LUCKCARD);
        return base._DoEvent();
    }
}
