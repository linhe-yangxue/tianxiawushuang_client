using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

//选择免战牌

/// <summary>
/// 免战牌类型
/// </summary>
public enum GRABTREASURE_PEACE_TYPE
{
    NONE,
    EIGHT_HOURS,
    ONE_HOUR
}

/// <summary>
/// 免战牌数据
/// </summary>
public class GrabTreasurePeaceData : ItemDataBase
{
    private GRABTREASURE_PEACE_TYPE mPeaceType;

    public GRABTREASURE_PEACE_TYPE PeaceType
    {
        set { mPeaceType = value; }
        get { return mPeaceType; }
    }
}

/// <summary>
/// 免战牌Tid、购买免战牌sIndex管理
/// </summary>
public class GrabTreasurePeaceID
{
    private static GrabTreasurePeaceID msInstance;

    private Dictionary<GRABTREASURE_PEACE_TYPE, int> mDicPeaceID = new Dictionary<GRABTREASURE_PEACE_TYPE, int>();
    private Dictionary<int, int> mDicSIndex = new Dictionary<int, int>();

    private GrabTreasurePeaceID()
    {
        mDicPeaceID.Add(GRABTREASURE_PEACE_TYPE.EIGHT_HOURS, 2000016);
        mDicPeaceID.Add(GRABTREASURE_PEACE_TYPE.ONE_HOUR, 2000017);

        __InitSIndex();
    }

    public static GrabTreasurePeaceID Instace
    {
        get
        {
            if (msInstance == null)
                msInstance = new GrabTreasurePeaceID();
            return msInstance;
        }
    }

    private void __InitSIndex()
    {
        foreach (KeyValuePair<GRABTREASURE_PEACE_TYPE, int> pair in mDicPeaceID)
        {
            int tmpTid = pair.Value;
            DataRecord tmpConfig = null;
            foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mMallShopConfig.GetAllRecord())
            {
                int tmpPeaceTid = (int)tmpPair.Value.getObject("ITEM_ID");
                if (tmpPeaceTid == tmpTid)
                {
                    tmpConfig = tmpPair.Value;
                    break;
                }
            }
            if (tmpConfig == null)
                continue;
            int tmpSIndex = (int)tmpConfig.getObject("INDEX");
            mDicSIndex.Add(tmpTid, tmpSIndex);
        }
    }

    public int this[GRABTREASURE_PEACE_TYPE peaceType]
    {
        get
        {
            if (peaceType == GRABTREASURE_PEACE_TYPE.NONE || mDicPeaceID == null || mDicPeaceID.Count <= 0)
                return 0;
            int peaceID = 0;
            if (!mDicPeaceID.TryGetValue(peaceType, out peaceID))
                return 0;
            return peaceID;
        }
    }
    public int this[int peaceTid]
    {
        get
        {
            if (mDicSIndex == null)
                return 0;
            int sIndex = 0;
            if (!mDicSIndex.TryGetValue(peaceTid, out sIndex))
                return 0;
            return sIndex;
        }
    }
}

public class GrabTreasureSelectPeaceWindow : tWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_grab_use_peace_one_btn", new DefineFactoryLog<Button_grab_use_peace_one_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_use_peace_eight_btn", new DefineFactoryLog<Button_grab_use_peace_eight_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_use_peace_close_button", new DefineFactoryLog<Button_grab_use_peace_close_button>());
        EventCenter.Self.RegisterEvent("Button_grab_use_peace_window", new DefineFactoryLog<Button_grab_use_peace_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        //设置免战牌图标
        //8小时图标
        int tmp8Id = GrabTreasurePeaceID.Instace[GRABTREASURE_PEACE_TYPE.EIGHT_HOURS];
        GameCommon.SetItemIcon(GetSub("grab_use_peace_eight_btn"), new ItemData() { mID = tmp8Id, mType = (int)PackageManager.GetItemTypeByTableID(tmp8Id) });
        //1小时图标
        int tmp1Id = GrabTreasurePeaceID.Instace[GRABTREASURE_PEACE_TYPE.ONE_HOUR ];
        GameCommon.SetItemIcon(GetSub("grab_use_peace_one_btn"), new ItemData() { mID = tmp1Id, mType = (int)PackageManager.GetItemTypeByTableID(tmp1Id) });

        return true;
    }

    public static string GetPeaceNameByType(GRABTREASURE_PEACE_TYPE peaceType)
    {
        string peaceName = "";
        switch (peaceType)
        {
            case GRABTREASURE_PEACE_TYPE.EIGHT_HOURS: peaceName = "8个小时免战令牌"; break;
            case GRABTREASURE_PEACE_TYPE.ONE_HOUR: peaceName = "1个小时免战令牌"; break;
        }
        return peaceName;
    }
    public static long GetPeaceTimeByType(GRABTREASURE_PEACE_TYPE peaceType)
    {
        long peaceTime = 0;
        switch (peaceType)
        {
            case GRABTREASURE_PEACE_TYPE.EIGHT_HOURS: peaceTime = 28800; break;
            case GRABTREASURE_PEACE_TYPE.ONE_HOUR: peaceTime = 3600; break;
        }
        return peaceTime;
    }
}

/// <summary>
/// 8小时免战
/// </summary>
class Button_grab_use_peace_eight_btn : CEvent
{
    public override bool _DoEvent()
    {
        GrabTreasurePeaceData tmpPeaceData = new GrabTreasurePeaceData();
        //设置数据
        tmpPeaceData.tid = GrabTreasurePeaceID.Instace[GRABTREASURE_PEACE_TYPE.EIGHT_HOURS];
        tmpPeaceData.PeaceType = GRABTREASURE_PEACE_TYPE.EIGHT_HOURS;
        DataCenter.OpenWindow("GRABTREASURE_USE_PEACE_WINDOW", tmpPeaceData);

        return true;
    }
}

/// <summary>
/// 1小时免战
/// </summary>
class Button_grab_use_peace_one_btn : CEvent
{
    public override bool _DoEvent()
    {
        GrabTreasurePeaceData tmpPeaceData = new GrabTreasurePeaceData();
        //设置数据
        tmpPeaceData.tid = GrabTreasurePeaceID.Instace[GRABTREASURE_PEACE_TYPE.ONE_HOUR];
        tmpPeaceData.PeaceType = GRABTREASURE_PEACE_TYPE.ONE_HOUR;
        DataCenter.OpenWindow("GRABTREASURE_USE_PEACE_WINDOW", tmpPeaceData);

        return true;
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_grab_use_peace_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GRABTREASURE_SELECT_PEACE_WINDOW");

        return true;
    }
}
