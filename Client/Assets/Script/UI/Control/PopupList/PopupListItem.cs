using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class PopupListItemData
{
    private int mIndex;         //索引值
    private string mItemName;       //元素显示名称
    private Action<PopupListItemData> mCallback;   //点击回调

    public int Index
    {
        set { mIndex = value; }
        get { return mIndex; }
    }
    public string ItemName
    {
        set { mItemName = value; }
        get { return mItemName; }
    }
    public Action<PopupListItemData> Callback
    {
        set { mCallback = value; }
        get { return mCallback; }
    }
}

/// <summary>
/// 弹出框元素
/// </summary>
public class PopupListItem : MonoBehaviour
{
    protected PopupListItemData m_ItemData;

    [SerializeField]
    protected UIButton m_Button;                //按钮
    [SerializeField]
    protected UIButtonEvent m_ButtonEvent;      //按钮事件

    void Awake()
    {
//         if(m_Button != null)
//             m_Button.onClick.Add(new EventDelegate() { target = this, methodName = "OnClicked" });
        if (m_ButtonEvent != null)
            m_ButtonEvent.AddAction(OnClicked);
    }

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
	}

    public void SetItemData(PopupListItemData itemData)
    {
        m_ItemData = itemData;

        _Refresh();
    }

    /// <summary>
    /// 刷新元素数据
    /// </summary>
    protected virtual void _Refresh()
    {
    }

    public void OnClicked()
    {
        if (m_ItemData != null && m_ItemData.Callback != null)
            m_ItemData.Callback(m_ItemData);
    }
}
