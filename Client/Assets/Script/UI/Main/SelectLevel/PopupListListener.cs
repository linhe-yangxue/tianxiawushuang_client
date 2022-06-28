using UnityEngine;
using System;


[RequireComponent(typeof(UIPopupList))]
public class PopupListListener : MonoBehaviour
{
    public Action<int> onSelect;

    private UIPopupList popupList;

    public void Select(int index)
    {
        if (index >= 0 && index < popupList.items.Count)
        {
            popupList.value = popupList.items[index];
        }
    }

    private void Awake()
    {
        popupList = GetComponent<UIPopupList>();
        EventDelegate.Add(popupList.onChange, OnChange);
    }

    private void OnChange()
    {
        if (UIPopupList.current != null)
        {
            string text = UIPopupList.current.value;
            int index = popupList.items.IndexOf(text);

            if (index >= 0 && onSelect != null)
                onSelect(index);
        }
    }
}