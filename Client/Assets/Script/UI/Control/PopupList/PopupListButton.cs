using UnityEngine;
using System.Collections;

/// <summary>
/// PopupList按钮为UIButton
/// </summary>
public class PopupListButton : PopupList
{
    [SerializeField]
    protected UIButton m_Button;            //触发按钮

    protected override void _Init()
    {
        base._Init();

        if (m_Button != null)
            m_Button.onClick.Add(new EventDelegate() { target = this, methodName = "OnClicked" });
    }
}
