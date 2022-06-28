using UnityEngine;
using System.Collections.Generic;
using Logic;


public class PaoPaoTextInitData
{
    public BaseObject parentObj;
    public string info;

    public PaoPaoTextInitData(BaseObject parentObj, string info)
    {
        this.parentObj = parentObj;
        this.info = info;
    }
}


public class PaoPaoTextHandler : IObjectHandler<UI_PaoPaoText>
{
    private PAOTEXT_TYPE _textType;

    public PaoPaoTextHandler(PAOTEXT_TYPE textType)
    {
        _textType = textType;
    }

    public UI_PaoPaoText New()
    {
        UI_PaoPaoText t = EventCenter.Self.StartEvent("UI_PaoPaoText") as UI_PaoPaoText;
        t.InitText(_textType);
        return t;
    }

    public void Enable(UI_PaoPaoText t, object initData)
    {
        PaoPaoTextInitData d = initData as PaoPaoTextInitData;

        if (d != null)
        {
            t.Start(d.parentObj, d.info);
        }
    }

    public void Disable(UI_PaoPaoText t)
    {
        t.Finish();
    }

    public void Destroy(UI_PaoPaoText t)
    {
        t.Destroy();
    }

    public bool CheckValid(UI_PaoPaoText t)
    {
        return t != null && t.mTextObject != null;
    }

    public bool CheckEnabled(UI_PaoPaoText t)
    {
        return t != null && t.mTextObject != null && t.mTextObject.activeSelf;
    }
}


public class PaoPaoTextPool
{
    public static List<SimpleObjectPool<UI_PaoPaoText>> pools = new List<SimpleObjectPool<UI_PaoPaoText>>();

    static PaoPaoTextPool()
    {
        for (int i = 0; i <= (int)PAOTEXT_TYPE.RESIST; ++i)
        {
            PaoPaoTextHandler handler = new PaoPaoTextHandler((PAOTEXT_TYPE)i);
            pools.Add(new SimpleObjectPool<UI_PaoPaoText>(handler));
        }
    }

    public static void Preload()
    {
        // total count : 34
        pools[(int)PAOTEXT_TYPE.WHITE].Preload(6);
        pools[(int)PAOTEXT_TYPE.RED].Preload(6);
        pools[(int)PAOTEXT_TYPE.YELLOW].Preload(4);
        pools[(int)PAOTEXT_TYPE.BLUE].Preload(6);
        pools[(int)PAOTEXT_TYPE.GREEN].Preload(4);
        pools[(int)PAOTEXT_TYPE.GOLD].Preload(4);
        pools[(int)PAOTEXT_TYPE.DODGE].Preload(2);
        pools[(int)PAOTEXT_TYPE.RESIST].Preload(2);
    }

    public static UI_PaoPaoText Start(PAOTEXT_TYPE textType, BaseObject parentObj, string info)
    {
        return pools[(int)textType].New(new PaoPaoTextInitData(parentObj, info));
    }

    public static void Disable(UI_PaoPaoText t)
    {
        pools[(int)t.mTextType].Disable(t);
    }

    public static void Clear()
    {
        foreach (var pool in pools)
        {
            pool.Clear();
        }
    }
}


public class PaoPaoTextQueue
{
    public static float mMinDeltaTime = 0.2f;
    public static int mMaxCount = 5;

    private class PaoPaoTextData
    {
        public string mStr = "";
        public PAOTEXT_TYPE mType = PAOTEXT_TYPE.WHITE;
    }

    private List<PaoPaoTextData> mDataQueue = new List<PaoPaoTextData>();
    private BaseObject mOwner;
    private float mTime = 0f;

    public PaoPaoTextQueue(BaseObject owner)
    {
        this.mOwner = owner;
    }

    public void Push(string str, PAOTEXT_TYPE type)
    {
        var data = new PaoPaoTextData { mStr = str, mType = type };

        if (mDataQueue.Count >= mMaxCount)
        {
            mDataQueue.RemoveAt(0);
        }

        mDataQueue.Add(data);
    }

    public void Update(float secondTime)
    {
        if (mTime <= 0f && mDataQueue.Count > 0)
        {
            var d = mDataQueue[0];
            PaoPaoTextPool.Start(d.mType, mOwner, d.mStr);
            mDataQueue.RemoveAt(0);
            mTime = mMinDeltaTime;
        }

        mTime -= secondTime;
    }
}