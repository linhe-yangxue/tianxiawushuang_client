using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;

/// <summary>
/// 选择VIP等级弹出框
/// </summary>
public class SelectVIPLevelPopupList : PopupListButtonEvent
{
    [SerializeField]
    private GameObject m_TweenerTarget;

    protected override void _RefreshItems()
    {
        __UpdateListNames();
        base._RefreshItems();
    }

    protected override void _OnToggle()
    {
        base._OnToggle();
    }

    /// <summary>
    /// 更新显示元素的名称
    /// </summary>
    private void __UpdateListNames()
    {
        foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mVipListConfig.GetAllRecord())
            m_ListItemName.Add("等级" + tmpPair.Key.ToString());
    }

    protected override void _OnItemClicked(PopupListItemData itemData)
    {
        base._OnItemClicked(itemData);

        OnClicked();

        __DoAction(itemData);
    }

    private void __DoAction(PopupListItemData itemData)
    {
        GlobalModule.DoCoroutine(VIP_ChangeVipLevel_Requester.StartRequest(itemData.Index));
    }
}
