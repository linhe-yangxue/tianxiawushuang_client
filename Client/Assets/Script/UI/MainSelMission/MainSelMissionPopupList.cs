using UnityEngine;
using System.Collections;
using System;

public enum MAIN_POPUP_LIST_BUTTON_TYPE
{
    EASY,
    NORMAL,
    HARD
}

public class MainSelMissionPopupList : PopupListButtonEvent//PopupListButton
{
    [SerializeField]
    protected UIButton m_Button;            //触发按钮

    [SerializeField]
    private GameObject[] m_BtnTarget;
    [SerializeField]
    private GameObject[] m_Items;

    [SerializeField]
    private GameObject m_TweenTarget;

    private Action<int> m_SelCallback;

    protected override void _RefreshItems()
    {
        if (m_ListItemName == null || m_Items == null)
            return;

        for (int i = 0, count = m_Items.Length; i < count; i++)
        {
            GameObject tmpGOItem = m_Items[i];

            PopupListItem tmpItem = tmpGOItem.GetComponent<MainSelMissionPopupListItem>();
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

    public Action<int> SelectCallback
    {
        set { m_SelCallback = value; }
        get { return m_SelCallback; }
    }

    public void Init()
    {
        m_IsOpened = false;
        if (m_TweenTarget != null)
            m_TweenTarget.transform.localPosition = Vector3.zero;
    }

    protected override void _OnToggle()
    {
        for (int i = 0, count = m_Items.Length; i < count; i++)
            m_Items[i].GetComponent<BoxCollider>().enabled = m_IsOpened;
    }

    public void SetCurrentButton(MAIN_POPUP_LIST_BUTTON_TYPE type)
    {
        if (m_BtnTarget == null || m_BtnTarget.Length <= 0)
            return;
        if (m_Button == null)
            return;

        for (int i = 0, count = m_BtnTarget.Length; i < count; i++)
            m_BtnTarget[i].gameObject.SetActive(i == (int)type);
        m_Button.tweenTarget = m_BtnTarget[(int)type];
    }

    protected override void _OnItemClicked(PopupListItemData itemData)
    {
        SetCurrentButton((MAIN_POPUP_LIST_BUTTON_TYPE)itemData.Index);

        OnClicked();

        if (m_SelCallback != null)
            m_SelCallback(itemData.Index);
    }
}
