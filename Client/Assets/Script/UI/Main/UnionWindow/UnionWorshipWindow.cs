using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

class UnionWorshipWindow : UnionBase//此页面仅向服务器获取动态请求，公会信息以及公会全部成员信息没有向服务器请求
{
    
    UILabel mNewsLabel;
    UISlider mExpSlider;
    UISlider mContriSlider;
    UILabel mCurExpLabel;
    UILabel mNextExpLabel;
    UILabel mGuildLevel;
    UILabel mMyContribute;
    UILabel mTodayContriCount;
    UILabel mMemberTotal;
	UILabel mContributionFate;
	GameObject mScheduleBtnOpen;
    GameObject mScheduleBtnOAccept;
	GameObject mScheduleBtnClose;
	GameObject mUnionGetRewadsTips;

    List<int[]> mScheduleInfoList;//0是schedule 1是group
    List<int> rewardInfoList;
    int mLiveness;
    int worshipCount;
    bool haveDone = false;

    protected override void OpenInit() {
        base.OpenInit();
        mCurExpLabel = GetUILabel("curExp");
        mNextExpLabel = GetUILabel("nextExp");
        mNewsLabel = GetUILabel("newsLabel");
        mExpSlider = GetUISlider("expSlider");
        mContriSlider = GetUISlider("conSlider");
        mGuildLevel = GetUILabel("guildLevel");
        mMyContribute = GetUILabel("myContribute");
        mTodayContriCount = GetUILabel("todayContriCount");
        mMemberTotal = GetUILabel("memberTotal");
		mContributionFate = GetUILabel("num_label");
    }

    public override void OnOpen() {
        base.OnOpen();
        SetWorshipBoard(GetCurUIGameObject("normal"), 1);
        SetWorshipBoard(GetCurUIGameObject("middle"), 2);
        SetWorshipBoard(GetCurUIGameObject("high"), 3);
        DataCenter.OpenBackWindow(UIWindowString.union_worship,"a_ui_zongmenggx_logo",() => DataCenter.OpenWindow(UIWindowString.union_main));
        GetNews(mNewsLabel);
        HttpModule.CallBack requestSuccess = text => {
            var item = JCode.Decode<SC_GetGuildWorshipInfo>(text);
            Action action = () => {
                rewardInfoList=item.rewardInfo.ToList();
                ScheduleInfoListInit();
              
                mExpSlider.value = guildBaseObject.expPercent;
                mContriSlider.value = GetCorrectedValuePercent(mScheduleInfoList.Select(arr => arr[0]).ToList(), item.worshipInfo.liveness);
                mLiveness = item.worshipInfo.liveness;
                worshipCount = item.worshipInfo.worshipCount;
                mGuildLevel.text = guildBaseObject.level.ToString();
//                mMyContribute.text=Mathf.Clamp(myMember.unionContr,0,999999).ToString();
				mMyContribute.text=GameCommon.ShowNumUI(RoleLogicData.Self.unionContr);
                mTodayContriCount.text = worshipCount.ToString();
                mMemberTotal.text = memberArr.Length.ToString();
                mCurExpLabel.text=guildBaseObject.exp.ToString();
                mNextExpLabel.text=guildBaseObject.nextExp.ToString();
				mContributionFate.text = ((int)(mContriSlider.value * 100)).ToString() + "%";
				SetRewordsBos(mLiveness);
            };
            UnionBase.InGuildThenDo(item, action);
        };
        CS_GetGuildWorshipInfo cs=new CS_GetGuildWorshipInfo(guildId);
        HttpModule.Instace.SendGameServerMessage(cs, "CS_GetGuildWorshipInfo", requestSuccess, NetManager.RequestFail);
    }

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex,objVal);
		if (keyIndex == "UPDATE_GONGXIAN") {
			OnOpen();
		}
	}

    public override void OnClose() {
        base.OnClose();
        haveDone=false;
        DataCenter.CloseWindow(UIWindowString.common_back);
    }

    void SetWorshipBoard(GameObject board, int worshipLevel) {
        var record = DataCenter.mWorshipConfig.GetRecord(worshipLevel);
        Action<string> setLabel = name => GameCommon.FindComponent<UILabel>(board, name).text = record.getData(name).ToString();

        int addExp=record.getData("EXP");
        int contribute = record.getData("CONTRIBUTE");

        setLabel("PRICE");
        setLabel("SCHEDULE");
        setLabel("EXP");
        setLabel("CONTRIBUTE");

        var priceType = record.getData("PRICE_TYPE");
        var priceTypeLabel = GameCommon.FindComponent<UILabel>(board, "PRICE_TYPE");
        var price = (int)record.getData("PRICE");

        bool canContri=false;
        string costType="";
        if (priceType == (int)ITEM_TYPE.YUANBAO) {
            priceTypeLabel.text = "元宝";
            costType="元宝";
            canContri = RoleLogicData.Self.diamond >= price;
            //之后更换图标
        }
        else if (priceType == (int)ITEM_TYPE.GOLD) {
            priceTypeLabel.text = "银币";
            costType="银币";
            canContri = RoleLogicData.Self.gold >= price;
            //之后更换图标
        }
        
        AddButtonAction(GameCommon.FindObject(board, "OK"), () => {
            string text = "";
            switch (worshipLevel) {
                case 1: text = "确定普通捐献？"; break;
                case 2: text = "确定中级捐献？"; break;
                case 3: text = "确定高级捐献？"; break;
            }
            DEBUG.Log("myMember.todayWorship = " + myMember.todayWorship);
            if (myMember.todayWorship > 0||haveDone) DataCenter.OpenMessageWindow("今日已捐献");
            else{
                if (canContri) {
                    if (worshipLevel == 3&&RoleLogicData.Self.vipLevel<=1) {
                        DataCenter.OpenMessageWindow("只对VIP2级以上开放");
                        return;
                    }
                    DataCenter.OpenMessageOkWindow(text, () => {
                        HttpModule.CallBack requestSuccess = _text => {
                            var item = JCode.Decode<SC_GuildWorship>(_text);
                            Action action = () => {
                                //update data 
                                PackageManager.RemoveItem((int)priceType, -1, price);
                                RoleLogicData.Self.unionContr += contribute;

                                haveDone = true;
                                mTodayContriCount.text = (worshipCount+1).ToString();
                                guildBaseObject.exp+=addExp;
								//refresh data or updateui
								DataCenter.SetData(UIWindowString.union_infoNews, "REFRESH_INFO", null);
                                //mExpSlider.value 

                                SetWorkshipFalse();
                            };
                            UnionBase.InGuildThenDo(item,action);
                        };
                        CS_GuildWorship cs = new CS_GuildWorship(guildId, worshipLevel);
                        HttpModule.Instace.SendGameServerMessage(cs, "CS_GuildWorship", requestSuccess, NetManager.RequestFail);
                    });
                }
				else {
					string diamon ="元宝";
					if(costType == diamon)
                    {
                        GameCommon.ToGetDiamond();
                    }
					else
                    {
                        if (priceType == (int)ITEM_TYPE.GOLD)
                        {
                            DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)priceType);
                        }
                        else
                        {
                            DataCenter.OpenMessageWindow(costType + "不足");
                        }
                    }
				}
            }
        });
    }


    void ScheduleInfoListInit() {
        mScheduleInfoList = new List<int[]>();
        var record = DataCenter.mWorshipSchedule.GetRecord(guildBaseObject.level);
        for (int i = 0; i < 5; i++) {
            int[] arr = new int[2] { record.getData("SCHEDULE_" + (i + 1)), record.getData("GROUP_ID_" + (i+1)) };
            mScheduleInfoList.Add(arr);
        }


        for (int i = 0; i < 4; i++) {
            GetUILabel("schedule_" + (i + 1)).text = mScheduleInfoList[i][0].ToString();
            var btnName = "scheduleBtn_" + (i + 1);
            var liveness = mScheduleInfoList[i][0];
            var rewardType = i+1;
            AddButtonAction(btnName, () => {
                DEBUG.Log(rewardInfoList.Contains(rewardType).ToString());

                if (mLiveness >= liveness) {
                    if(rewardInfoList.Contains(rewardType)) DataCenter.ErrorTipsLabelMessage("今日已领取该奖励");
                    else {
                        HttpModule.CallBack requestSuccess=text => {
                            var item=JCode.Decode<SC_GetGuildWorshipReward>(text);
                            Action action=() => {
                                List<ItemDataBase> itemList = PackageManager.UpdateItem(item.arr);
                                DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemList);
                                OnOpen();
                            };
                            UnionBase.InGuildThenDo(item,action);
                        };
                        rewardInfoList.Add(rewardType);
                        CS_GetGuildWorshipReward cs=new CS_GetGuildWorshipReward(guildId,rewardType);
                        HttpModule.Instace.SendGameServerMessage(cs,"CS_GetGuildWorshipReward",requestSuccess,NetManager.RequestFail);    
                    }
                } else DataCenter.ErrorTipsLabelMessage("进度不足");

            });
        };
    }

    //数据间隔不一致，但在显示时的进度条上有一样的间隔，方法返回值为修正后在整个进度条的百分比
    float GetCorrectedValuePercent(List<int> dividePointList, int value) {
        dividePointList.Insert(0, 0);//传进来的数组代表各个节点的数字，所以没有0，在最前面加上零方便计算
        int pointCount = dividePointList.Count;
        float resultValue = 0;
        for (int i = 1; i < pointCount; i++) {
            if (value >= dividePointList[i - 1] && value < dividePointList[i]) {
                int dValue = value - dividePointList[i - 1];
                resultValue += dValue / (float)(dividePointList[i] - dividePointList[i - 1]) / (float)(pointCount - 1);
                break;
            }
            resultValue += 1 / (float)(pointCount - 1);
        }
        return resultValue;
    }

	void SetRewordsBos(int mLiveness)
	{
		mScheduleInfoList = new List<int[]>();
		var record = DataCenter.mWorshipSchedule.GetRecord(guildBaseObject.level);
		for (int i = 0; i < 5; i++) 
		{
			int[] arr = new int[2] { record.getData("SCHEDULE_" + (i + 1)), record.getData("GROUP_ID_" + (i+1)) };
			mScheduleInfoList.Add(arr);
		}
	
		for(int i = 1; i < 5; i++)
		{
			GameObject scheduleBtnObj = GameCommon.FindObject (mGameObjUI, "scheduleBtn_" + i).gameObject;
			mScheduleBtnOpen = GameCommon.FindObject (scheduleBtnObj, "scheduleBtn_open").gameObject;
            mScheduleBtnOAccept = GameCommon.FindObject(scheduleBtnObj, "scheduleBtn_accept").gameObject;
			mScheduleBtnClose = GameCommon.FindObject (scheduleBtnObj, "scheduleBtn_close").gameObject;
			mUnionGetRewadsTips = GameCommon.FindObject (scheduleBtnObj, "union_get_rewads_tips").gameObject;

			mScheduleBtnClose.SetActive(false);
			mScheduleBtnOpen.SetActive(false);
			mUnionGetRewadsTips.SetActive(false);
			var liveness = mScheduleInfoList[i - 1][0];
			
			if (mLiveness >= liveness)
			{
				if(rewardInfoList.Contains(i))
				{
					mScheduleBtnClose.SetActive(false);
                    mScheduleBtnOAccept.SetActive(false);
					mScheduleBtnOpen.SetActive(true);
					mUnionGetRewadsTips.SetActive(false);
				}else
				{
					mScheduleBtnClose.SetActive(false);
                    mScheduleBtnOAccept.SetActive(true);
					mScheduleBtnOpen.SetActive(false);
					mUnionGetRewadsTips.SetActive(true);
				}
			}else
			{
				mScheduleBtnClose.SetActive(true);
                mScheduleBtnOAccept.SetActive(false);
				mScheduleBtnOpen.SetActive(false);
				mUnionGetRewadsTips.SetActive(false);
			}
		}
	}
}

public class GuildWorshipObject
{
    public readonly int liveness;
    public readonly int worshipCount;

    public GuildWorshipObject(int liveness,int worshipCount) {
        this.liveness = liveness;
        this.worshipCount = worshipCount;
    }
    public GuildWorshipObject() { }
}
