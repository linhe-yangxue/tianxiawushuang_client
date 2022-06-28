using UnityEngine;
using System.Collections;

public class PopupListButtonEvent : PopupList
{
    [SerializeField]
    protected UIButtonEvent m_ButtonEvent;

    protected override void _Init()
    {
        base._Init();

        if (m_ButtonEvent != null)
            m_ButtonEvent.AddAction(OnClicked);
    }
}
