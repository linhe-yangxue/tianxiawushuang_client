using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;
using System.Linq;

public static class TaskStarManager
{

    class CS_GetBattleBoxList : GameServerMessage { }
    class SC_GetBattleBoxList : RespMessage
    {
        public string[] boxIdArr;
    }

    static Dictionary<int, List<int>>[] starInfoDictArr = new Dictionary<int, List<int>>[2]{
        new Dictionary<int,List<int>>(),new Dictionary<int,List<int>>()
    };

    static List<int> curRewardList
    {
        get
        {
            return starInfoDictArr[ScrollWorldMapWindow.mDifficulty - 1][ScrollWorldMapWindow.mPage];
        }
    }

    public static void TaskStarInit()
    {
        //by chenliang
        //begin

        //初始化字典
        foreach (var tmpDic in starInfoDictArr)
            tmpDic.Clear();

        //end
        TableManager.GetTable("StarReward").GetAllRecord().Values.Foreach(record =>
        {
            string indexStr = record.getData("INDEX").ToString();
            int diffculty = int.Parse(indexStr[0].ToString());
            int page = int.Parse(indexStr.Substring(1, indexStr.Length - 1));
            starInfoDictArr[diffculty - 1].Add(page, new List<int>());
        });

        HttpModule.Instace.SendGameServerMessageT(new CS_GetBattleBoxList(), text =>
        {
            var arr = JCode.Decode<SC_GetBattleBoxList>(text).boxIdArr;
            AddArr(arr);
        },
        NetManager.RequestFail);
    }

    public static bool IsReceived(int rewardID)
    {
        return curRewardList.Contains(rewardID);
    }
    //added by xuke 红点相关
    public static bool IsReceived(int kDifficulty, int kPageIndex, int kRewardID)
    {
        return starInfoDictArr[kDifficulty - 1][kPageIndex].Contains(kRewardID);
    }
    //end
    public static void AddRewardID(int rewardID)
    {
        curRewardList.Add(rewardID);
    }

    static void AddArr(IEnumerable<string> strs)
    {
        strs.Foreach(str =>
        {
            int diffculty = int.Parse(str[0].ToString());
            int page = int.Parse(str.Substring(1, str.Length - 2));
            int rewardType = int.Parse(str[str.Length - 1].ToString());
            starInfoDictArr[diffculty - 1][page].Add(rewardType);
        });
        //starInfoDictArr.Foreach(dict => dict.Foreach(keyValue => DEBUG.LogError(keyValue.Key+"___"+keyValue.Value.Count)));
    }


}

public class BattleTask
{
    public readonly int taskId;
    public readonly int finished;
}

public enum ReceiveState
{
    CanReceive,
    CanNotReceive,
    Received
}

public static class RewardHelper
{
    static DataRecord record;
    public static ReceiveState GetReceiveState(int starCount, int rewardID)
    {
        record = TableManager.GetTable("StarReward").GetRecord(ScrollWorldMapWindow.mDifficulty * 100 + ScrollWorldMapWindow.mPage);
        int standardStar = 12;
        switch (rewardID)
        {
            case 1: standardStar = record.getData("STARNUMBER_1"); break;
            case 2: standardStar = record.getData("STARNUMBER_2"); break;
            case 3: standardStar = record.getData("STARNUMBER_3"); break;
        }
        if (starCount < standardStar) return ReceiveState.CanNotReceive;
        else
        {
            if (TaskStarManager.IsReceived(rewardID)) return ReceiveState.Received;
            else return ReceiveState.CanReceive;
        }
    }

    //added by xuke 红点相关
    public static ReceiveState GetReceiveState(int kDif, int kPage, int kStarCount, int kRewardID)
    {
        record = TableManager.GetTable("StarReward").GetRecord(kDif * 100 + kPage);
        if (record == null)
            return ReceiveState.CanNotReceive;
        int standardStar = 12;
        switch (kRewardID)
        {
            case 1: standardStar = record.getData("STARNUMBER_1"); break;
            case 2: standardStar = record.getData("STARNUMBER_2"); break;
            case 3: standardStar = record.getData("STARNUMBER_3"); break;
        }
        if (kStarCount < standardStar) return ReceiveState.CanNotReceive;
        else
        {
            if (TaskStarManager.IsReceived(kDif, kPage, kRewardID)) return ReceiveState.Received;
            else return ReceiveState.CanReceive;
        }
    }
    //end
}


class TaskRewardBox : tWindow
{
    DataRecord record;
    GameObject baoxiangSprite;
    UILabel startCountLabel;
    GameObject[] boxItems = new GameObject[3];

    public static int totalStar = 0;

    protected override void OpenInit()
    {
        base.OpenInit();
        startCountLabel = GetUILabel("starTotalCount");

        boxItems[0] = GetSub("boxItem1");
        boxItems[1] = GetSub("boxItem2");
        boxItems[2] = GetSub("boxItem3");

        EventCenter.Register("Button_btn_task_box", new DefineFactory<Button_btn_task_box>());
    }

    public override void OnOpen()
    {
        base.OnOpen();
        record = TableManager.GetTable("StarReward").GetRecord(ScrollWorldMapWindow.mDifficulty * 100 + ScrollWorldMapWindow.mPage);
        GetCurTotalStar();
        startCountLabel.text = totalStar + "/" + record.getData("STARNUMBER_3");

        for (int i = 0; i < 3; i++)
        {
            ReceiveState tmpState = RewardHelper.GetReceiveState(totalStar, i + 1);
            UISprite boxIcon = boxItems[i].transform.Find("btn_task_box").GetComponent<UISprite>();
            boxIcon.spriteName = BoxState(i, tmpState);
            boxIcon.GetComponent<UIButtonEvent>().mData.set("BOXINDEX", i);

            GameObject wumaotexiao = boxItems[i].transform.Find("ui_tgbaoxiangstar").gameObject;
            if (tmpState == ReceiveState.CanReceive)
            {
                wumaotexiao.SetActive(true);
            }
            else
            {
                wumaotexiao.SetActive(false);
            }
            boxItems[i].transform.Find("starCount").GetComponent<UILabel>().text = record.getData("STARNUMBER_" + (i + 1).ToString());
        }
    }

    string BoxState(int index, ReceiveState state)
    {
        string ret = "";
        if (index == 0)
        {
            switch (state)
            {
                case ReceiveState.CanNotReceive:
                    {
                        ret = "a_ui_baoxiang_b_off";
                    }
                    break;
                case ReceiveState.CanReceive:
                    {
                        ret = "a_ui_baoxiang_b_normal";
                    }
                    break;
                case ReceiveState.Received:
                    {
                        ret = "a_ui_baoxiang_b_open";
                    }
                    break;
            }
        }
        else if (index == 1)
        {
            switch (state)
            {
                case ReceiveState.CanNotReceive:
                    {
                        ret = "a_ui_baoxiang_c_off";
                    }
                    break;
                case ReceiveState.CanReceive:
                    {
                        ret = "a_ui_baoxiang_c_normal";
                    }
                    break;
                case ReceiveState.Received:
                    {
                        ret = "a_ui_baoxiang_c_open";
                    }
                    break;
            }
        }
        else
        {
            switch (state)
            {
                case ReceiveState.CanNotReceive:
                    {
                        ret = "a_ui_baoxiang_d_off";
                    }
                    break;
                case ReceiveState.CanReceive:
                    {
                        ret = "a_ui_baoxiang_d_normal";
                    }
                    break;
                case ReceiveState.Received:
                    {
                        ret = "a_ui_baoxiang_d_open";
                    }
                    break;
            }
        }
        return ret;
    }

    void GetCurTotalStar()
    {
        totalStar = 0;
        List<WorldMapPoint> pointList = new List<WorldMapPoint>();
        for (int i = 0; i < CommonParam.stagePerMap; i++)
        {
            WorldMapPoint pt = WorldMapPoint.Create(ScrollWorldMapWindow.mDifficulty, ScrollWorldMapWindow.mPage, i);
            if (pt != null)
            {
                pointList.Add(pt);
            }
        }
        pointList.Select(point => StageProperty.Create(point.mId)).Foreach(stage => totalStar += stage.mBestStar);
    }

    #region 红点相关
    public static int GetTotalStarNum(int kDifficulty, int kPageIndex)
    {
        int _totalStarNum = 0;
        List<WorldMapPoint> pointList = new List<WorldMapPoint>();
        for (int i = 0; i < CommonParam.stagePerMap; i++)
        {
            WorldMapPoint pt = WorldMapPoint.Create(kDifficulty, kPageIndex, i);
            if (pt != null)
            {
                pointList.Add(pt);
            }
        }
        //by chenliang
        //begin

        //        pointList.Select(point => StageProperty.Create(point.mId)).Foreach(stage => _totalStarNum += stage.mBestStar);
        //-----------------
        foreach (WorldMapPoint tmpPt in pointList)
        {
            StageProperty tmpSP = StageProperty.Create(tmpPt.mId);
            _totalStarNum += tmpSP.mBestStar;
        }

        //end
        return _totalStarNum;
    }
    #endregion

}

class TaskRewardWindow : tWindow
{

    class CS_RcvBattleBoxAward : GameServerMessage
    {
        public readonly string boxId;
        public CS_RcvBattleBoxAward(int rewardID)
        {
            int count = ScrollWorldMapWindow.mDifficulty * 100 + ScrollWorldMapWindow.mPage;
            boxId = count.ToString() + rewardID.ToString();
        }
    }

    class SC_RcvBattleBoxAward : RespMessage
    {
        public readonly ItemDataBase[] awards;
    }

    DataRecord record;
    int curStarCount;
    int boxIndex;
    UILabel title;
    UILabel rewardStarCount;
    GameObject receivedIcon;
    GameObject receiveButton;
    GameObject[] rewardItem = new GameObject[4];

    public override void Open(object param)
    {
        base.Open(param);
        if (!(param is int)) DEBUG.LogError("参数类型错误");
        else
        {
            title = GetUILabel("title_label");
            rewardStarCount = GetUILabel("rewardStarCount");
            receivedIcon = GetSub("receivedIcon");
            receiveButton = GetSub("receiveButton");
            for (int i = 0; i < rewardItem.Length; i++)
            {
                rewardItem[i] = GetSub("item_icon_info" + (i + 1).ToString());
            }
            AddButtonAction("close", () =>
            {
                DataCenter.CloseWindow(UIWindowString.task_reward);
                DataCenter.OpenWindow(UIWindowString.task_reward_box);
            });
            boxIndex = (int)param;
            curStarCount = TaskRewardBox.totalStar;
            SetUIByIndex(boxIndex);
        }
    }

    void SetUIByIndex(int boxIndex)
    {
        switch (boxIndex)
        {
            case 0:
                title.text = "铜宝箱";
                break;
            case 1:
                title.text = "银宝箱";
                break;
            case 2:
                title.text = "金宝箱";
                break;
        }

        record = TableManager.GetTable("StarReward").GetRecord(ScrollWorldMapWindow.mDifficulty * 100 + ScrollWorldMapWindow.mPage);
        string starText = "达到{0}星可领取";
        string starCount = record.getData("STARNUMBER_" + (boxIndex + 1).ToString()).ToString();
        rewardStarCount.text = string.Format(starText, starCount);

        RefreshReceiveButton(boxIndex + 1);

        List<ItemDataBase> items = GameCommon.ParseItemList(record.getData("REWARD_" + (boxIndex + 1)));
        items.Sort((a, b) =>
        {
            if (a.tid > b.tid) return 1;
            else if (a.tid < b.tid) return -1;
            else return 0;
        });

        for (int i = 0; i < items.Count; i++)
        {
            rewardItem[i].SetActive(true);
            UILabel itemNumLabel = rewardItem[i].transform.Find("item_icon/item_num").GetComponent<UILabel>();
            itemNumLabel.text = "x" + items[i].itemNum.ToString();
            GameCommon.SetItemIconNew(rewardItem[i], "item_icon", items[i].tid);
            // By   XiaoWen
            // Bug  #14218【关卡】星数奖励宝箱，领取后物品名称显示错误（突破石，元宝显示为体力丹，银币显示为元宝）
            // Note GetUILabel获取的是当前窗口TaskRewardWindow下面的Label组件，所以每次获取时都是获取的奖励窗口里面第一个的奖励名称
            // Begin
            UILabel itemNameLabel = rewardItem[i].transform.Find("item_name").GetComponent<UILabel>();
            itemNameLabel.text = GameCommon.GetItemName(items[i].tid);
            //GetUILabel("item_name").text = GameCommon.GetItemName(items[i].tid);
            // End
        }
        for (int i = items.Count; i < rewardItem.Length; i++)
        {
            rewardItem[i].SetActive(false);
        }
    }

    void RefreshReceiveButton(int rewardID)
    {
        ReceiveState state = RewardHelper.GetReceiveState(curStarCount, rewardID);

        bool isReceived = state == ReceiveState.Received;
        receivedIcon.SetActive(isReceived);
        receiveButton.SetActive(!isReceived);
        if (!isReceived)
        {
            bool canReceive = state == ReceiveState.CanReceive;
            receiveButton.GetComponent<UIImageButton>().isEnabled = canReceive;
            if (canReceive)
            {
                rewardStarCount.text = "";
                AddButtonAction(receiveButton, () =>
                    {
                        HttpModule.Instace.SendGameServerMessageT(new CS_RcvBattleBoxAward(rewardID), text =>
                        {
                            var sc = JCode.Decode<SC_RcvBattleBoxAward>(text);
                            var itemArr = sc.awards;
                            List<ItemDataBase> itemArrNew = PackageManager.UpdateItem(itemArr.ToList());
                            itemArrNew.Sort((a, b) =>
                            {
                                if (a.tid > b.tid) return 1;
                                else if (a.tid < b.tid) return -1;
                                else return 0;
                            });
                            //					DataCenter.OpenWindow("GET_REWARDS_WINDOW",itemArrNew.ToArray());
                            DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemArrNew);
                            TaskStarManager.AddRewardID(rewardID);
                            RefreshReceiveButton(rewardID);

                            //added by xuke 红点提示
                            DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_RIGHT", "REFRESH_TEAM_BTN_NEW_MARK", null);
                            AdventureNewMarkManager.Self.CheckCurPage_NewMark();
                            //end

                            //领取完奖励关闭奖励窗口
                            DataCenter.CloseWindow(UIWindowString.task_reward);
                            DataCenter.OpenWindow(UIWindowString.task_reward_box);
                            //end
                        },
                        NetManager.RequestSendMailFail);
                    });
            }
        }
        else
        {
            rewardStarCount.text = "";
        }
    }
}


class TotalTaskWindow : tWindow
{
    public static event Action<int> onRewardReceived;
    class CS_GetBattleTask : GameServerMessage { }
    class SC_GetBattleTask : RespMessage
    {
        public readonly BattleTask[] taskArr;
    }

    class CS_GetBattleTaskAward : GameServerMessage
    {
        public readonly int taskId;
        public CS_GetBattleTaskAward(int taskId)
        {
            this.taskId = taskId;
        }
    }
    class SC_GetBattleTaskAward : RespMessage
    {
        public readonly BattleTask nextTask;
        public readonly ItemDataBase[] awards;
    }


    GameObject normalTask;
    GameObject hardTask;

    public override void Init()
    {
        base.Init();
        EventCenter.Self.RegisterEvent("Button_totalTaskBtn", new DefineFactory<Button_TotalTaskBtn>());
        EventCenter.Self.RegisterEvent("Button_world_map_CharacterBtn", new DefineFactory<Button_world_map_CharacterBtn>());
    }
    protected override void OpenInit()
    {
        base.OpenInit();
        normalTask = GetCurUIGameObject("normal");
        hardTask = GetCurUIGameObject("hard");
        AddButtonAction("close", () => DataCenter.CloseWindow(UIWindowString.total_task));
    }

    public override void OnOpen()
    {
        base.OnOpen();
        TotalTaskManager.popupFlag = false;

        HttpModule.Instace.SendGameServerMessageT(new CS_GetBattleTask(), text =>
        {
            var taskArr = JCode.Decode<SC_GetBattleTask>(text).taskArr;
            RefreshBoard(normalTask, taskArr[0]);
            RefreshBoard(hardTask, taskArr[1]);
        },
        NetManager.RequestSendMailFail);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "REFRESH_TASK_NEWMARK":
                UpdateNewMarkStateInfo((bool)objVal);
                break;
        }

    }


    private List<bool> mTaskStateInfoList = new List<bool>();   //> 任务状态列表
    /// <summary>
    /// 刷新任务按钮上的小红点
    /// </summary>
    private void UpdateNewMarkStateInfo(bool kNeedRequest)
    {
        // 如果是第一次，则从服务器获得信息
        if (kNeedRequest)
        {
            HttpModule.Instace.SendGameServerMessageT(new CS_GetBattleTask(),
            text =>
            {
                var taskArr = JCode.Decode<SC_GetBattleTask>(text).taskArr;
                //added by xuke
                mTaskStateInfoList.Clear();
                //end
                mTaskStateInfoList.Add(taskArr[0].finished == 1);
                mTaskStateInfoList.Add(taskArr[1].finished == 1);
                DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_RIGHT", "REFRESH_NEW_MARK", mTaskStateInfoList);
            }
            ,
            NetManager.RequestSendMailFail);
        }
        else
        {
            DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_RIGHT", "REFRESH_NEW_MARK", mTaskStateInfoList);
        }
    }
    void RefreshBoard(GameObject board, BattleTask battleTask)
    {
        GameObject end = GameCommon.FindObject(board, "end");
        GameObject noEnd = GameCommon.FindObject(board, "noEnd");
        GameObject getButton = GameCommon.FindObject(board, "get");
        GameObject forwardButton = GameCommon.FindObject(board, "forward");

        bool allFinished = battleTask.taskId == 0;
        end.SetActive(allFinished);
        noEnd.SetActive(!allFinished);
        if (allFinished) return;

        bool isFinished = battleTask.finished == 1;
        getButton.SetActive(isFinished);
        forwardButton.SetActive(!isFinished);
        //added by xuke
        if (board.name.Equals("normal"))
        {
            mTaskStateInfoList[0] = isFinished;
        }
        else if (board.Equals("hard"))
        {
            mTaskStateInfoList[1] = isFinished;
        }
        UpdateNewMarkStateInfo(true);
        //end
        var record = TableManager.GetTable("StageTask").GetRecord(battleTask.taskId);
        GameCommon.FindComponent<UILabel>(board, "taskName").text = record.getData("NAME");
        GameCommon.FindComponent<UILabel>(board, "describe").text = record.getData("DESC");
        GameCommon.FindComponent<UILabel>(board, "stageName").text = record.getData("GOALDESC");
        UISprite sprite = GameCommon.FindComponent<UISprite>(board, "taskIcon");
        GameCommon.SetIcon(sprite, record.getData("ICON_ATLAS"), record.getData("ICON_NAME"));

        var container = GameCommon.FindComponent<UIGridContainer>(board, "grid");
        List<ItemDataBase> list = GameCommon.ParseItemList(record.getData("REWARD"));
        container.MaxCount = list.Count;
        for (int i = 0; i < list.Count; i++)
        {
            int tempTid = list[i].tid;
            var grid = container.controlList[i];
            UILabel itemNumLabel = grid.transform.Find("item_icon/item_num").GetComponent<UILabel>();
            itemNumLabel.text = "x" + list[i].itemNum.ToString();
            GameCommon.SetOnlyItemIcon(GameCommon.FindObject(grid, "item_icon"), tempTid);
            GameCommon.BindItemDescriptionEvent(GameCommon.FindObject(grid,"item_icon"),tempTid);
            //GameCommon.AddButtonAction(GameCommon.FindObject(grid, "item_icon"), () =>
            //{
            //    GameCommon.SetItemDetailsWindow(tempTid);
            //});
        }

        if (!isFinished)
        {
            int pointID = record.getData("POINTID");
            var point = WorldMapPoint.Create(pointID);

            AddButtonAction(forwardButton, () =>
            {
                if (point.unlocked)
                {
                    DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_LEFT", "INIT_POPUP_LIST", point.difficulty);
                    DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_DIFFICULTY", point.difficulty);
                    DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_POINT", pointID);
                    DataCenter.CloseWindow(UIWindowString.total_task);
                }
                else DataCenter.OpenMessageWindow("关卡未解锁");
            });
        }
        else AddButtonAction(getButton, () =>
        {
            //判断背包是否已满
            List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(list);
            if (!CheckPackage.Instance.CanGetStageTaskReward(tmpPackageTypes))
            {
                return;
            }
            HttpModule.Instace.SendGameServerMessageT(new CS_GetBattleTaskAward(battleTask.taskId), text =>
            {
                var sc = JCode.Decode<SC_GetBattleTaskAward>(text);
                var nextTask = sc.nextTask;
                var itemArr = sc.awards;
                PackageManager.UpdateItem(itemArr);

                List<int> _tidList = GameCommon.GetTidByItemData(itemArr.ToList<ItemDataBase>());
                if (itemArr != null && _tidList != null) 
                {
                    for (int i = 0, count = _tidList.Count; i < count; i++)
                    {
                        if (PetGainWindow.CheckPetGainQuality(_tidList[i]))
                        {
                            DataCenter.OpenWindow("PET_GAIN_WINDOW", _tidList.GetEnumerator());
                            break;
                        }
                    }
                }                             
                //list 转数组
                ItemDataBase[] idb = new ItemDataBase[list.Count];// = (ItemDataBase[])param;
                int index = 0;
                foreach (ItemDataBase tmpItem in list)
                {
                    idb[index] = tmpItem;
                    index++;
                }

                //				DataCenter.OpenWindow("GET_REWARDS_WINDOW",idb);
                DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", list);
                RefreshBoard(board, nextTask);

                if (onRewardReceived != null)
                    onRewardReceived(battleTask.taskId);

                //added by xuke 刷新队伍按钮红点状态
                DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_RIGHT", "REFRESH_TEAM_BTN_NEW_MARK", null);
                //end
            },
            NetManager.RequestSendMailFail);
        });
    }
}


public class Button_TotalTaskBtn : Logic.CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow(UIWindowString.total_task);
        return true;
    }
}
public class Button_world_map_CharacterBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("STAGE_INFO_WINDOW");
        //关闭左上角的冒险窗口
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_TOP_LEFT");
        DataCenter.CloseWindow("TASK_REWARD_BOX");
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_BOTTOM_LEFT");
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_BOTTOM_RIGHT");
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");

        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);

        DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);
        MainUIScript.Self.HideMainBGUI();
        MainUIScript.Self.mWindowBackAction = () =>
        {

            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
            DataCenter.SetData("INFO_GROUP_WINDOW", "PRE_WIN", 2);
        };
        return true;
    }

}


public class TotalTaskManager
{
    // 有关卡任务奖励的关卡ID集合
    private static HashSet<int> taskStageIDs = new HashSet<int>();
    private static bool _popupFlag = false;

    // 初始化有关卡任务奖励的关卡ID集合
    // 从持久层读取弹出领取奖励窗口标志位
    // 注册监听器
    public static void Init()
    {
        taskStageIDs.Clear();

        foreach (var pair in DataCenter.mStageTask.GetAllRecord())
        {
            int pointID = pair.Value["POINTID"];
            DataRecord r = DataCenter.mStagePoint.GetRecord(pointID);

            if (r != null)
            {
                int stageId = r["STAGEID"];
                taskStageIDs.Add(stageId);
            }
        }

        string key = CommonParam.mUId + "/" + "STAGE_TASK_FLAG";
        _popupFlag = PlayerPrefs.GetInt(key, 0) > 0;

        MapLogicData.mOnFirstClearStage -= OnFirstClearStage;
        MapLogicData.mOnFirstClearStage += OnFirstClearStage;
    }

    /// <summary>
    /// 当进入关卡列表时是否弹出关卡任务窗口的标志位
    /// </summary>
    public static bool popupFlag
    {
        get
        {
            return _popupFlag;
        }
        set
        {
            if (_popupFlag != value)
            {
                _popupFlag = value;
                string key = CommonParam.mUId + "/" + "STAGE_TASK_FLAG";
                PlayerPrefs.SetInt(key, value ? 1 : 0);
            }
        }
    }

    private static void OnFirstClearStage(int stageID)
    {
        if (taskStageIDs.Contains(stageID))
        {
            popupFlag = true;
        }
    }
}

public class Button_btn_task_box : CEvent
{
    public override bool _DoEvent()
    {
        int boxIndex = (int)get("BOXINDEX");
        DataCenter.OpenWindow(UIWindowString.task_reward, boxIndex);
        return true;
    }
}