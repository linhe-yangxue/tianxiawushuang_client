using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PopupList : MonoBehaviour
{
    [SerializeField]
    protected UITweener[] m_ShowTweener;    //触发动画
    [SerializeField]
    protected UIGridContainer m_GridContainer;

    //元素名称
    [SerializeField]
    protected List<string> m_ListItemName = new List<string>();
    protected bool m_IsOpened = false;

    //选择元素时回调
    protected Action<PopupListItemData> m_Callback;

    void Awake()
    {
        _Init();
    }

    // Use this for initialization
    void Start()
    {
        _RefreshItems();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public List<string> ListItemName
    {
        get { return m_ListItemName; }
    }

    protected virtual void _Init()
    {
    }

    protected virtual void _RefreshItems()
    {
        if (m_GridContainer == null || m_ListItemName == null)
            return;

        int tmpCount = m_ListItemName.Count;
        m_GridContainer.MaxCount = tmpCount;
        for (int i = 0; i < tmpCount; i++)
        {
            GameObject tmpGOItem = m_GridContainer.controlList[i];

            PopupListItem tmpItem = tmpGOItem.GetComponent<PopupListItem>();
            if (tmpItem == null)
                continue;

            tmpItem.SetItemData(new PopupListItemData()
            {
                Index = i,
                ItemName = m_ListItemName[i],
                Callback = _OnItemClicked
            });
        }
    }

    /// <summary>
    /// 点击
    /// </summary>
    public void OnClicked()
    {
        m_IsOpened = !m_IsOpened;
        if (m_ShowTweener != null)
        {
            if (m_IsOpened)
            {
                for (int i = 0, count = m_ShowTweener.Length; i < count; i++)
                    m_ShowTweener[i].PlayForward();
            }
            else
            {
                for (int i = 0, count = m_ShowTweener.Length; i < count; i++)
                    m_ShowTweener[i].PlayReverse();
            }
        }
        _OnToggle();
    }
    /// <summary>
    /// 开关状态转换时调用
    /// </summary>
    protected virtual void _OnToggle()
    {
    }
    /// <summary>
    /// 元素被点击
    /// </summary>
    /// <param name="itemData"></param>
    protected virtual void _OnItemClicked(PopupListItemData itemData)
    {
        if (m_Callback != null)
            m_Callback(itemData);
    }
}
