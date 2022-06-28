using UnityEngine;
using System.Collections;

public class SelectVIPLevelPopupListItem : PopupListItem
{
    [SerializeField]
    private UILabel m_LabelName;

    void Awake()
    {
        if(m_Button != null)
            m_Button.onClick.Add(new EventDelegate() { target = this, methodName = "OnClicked" });
    }

    protected override void _Refresh()
    {
        base._Refresh();

        if (m_LabelName != null && m_ItemData != null)
            m_LabelName.text = m_ItemData.ItemName;
    }
}
