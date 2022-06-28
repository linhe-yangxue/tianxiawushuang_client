using UnityEngine;


[RequireComponent(typeof(UIPopupList))]
public class PopupListHelper : MonoBehaviour
{
    public UILabel label;
    private UIPopupList popupList;

    private void Awake()
    {
        popupList = GetComponent<UIPopupList>();
        EventDelegate.Add(popupList.onChange, OnChange);
    }

    private void OnChange()
    {
        if (label != null && UIPopupList.current != null)
        {
            string text = UIPopupList.current.value.Replace(" ", "");
            label.text = text;
        }
    }
}