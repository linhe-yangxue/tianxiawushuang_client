using UnityEngine;
using System.Collections;
using System;

public enum HOTUPDATE_MESSAGEBOX_TYPE
{
    NONE,
    OK,
    OK_CANCEL,
    RETRY
}

public class HotUpdateMessageBox : MonoBehaviour
{
    private static HotUpdateMessageBox m_HotUpdateMessageBox;

    [SerializeField]
    private UIButton m_BtnOk;
    [SerializeField]
    private UIButton m_BtnCancel;
    [SerializeField]
    private UIButton m_BtnMiddleOk;
    [SerializeField]
    private UIButton m_BtnRetry;
    [SerializeField]
    private UILabel m_LBContent;

    private Action m_OkCallback;
    private Action m_CancelCallback;

    // Use this for initialization
    void Start()
    {
        m_BtnOk.onClick.Add(new EventDelegate(__OnOkClicked));
        m_BtnCancel.onClick.Add(new EventDelegate(__OnCancelClicked));
        m_BtnMiddleOk.onClick.Add(new EventDelegate(__OnOkClicked));
        m_BtnRetry.onClick.Add(new EventDelegate(__OnRetryClicked));
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// 显示消息框
    /// </summary>
    /// <param name="type"></param>
    /// <param name="content"></param>
    public static void ShowMessageBox(HOTUPDATE_MESSAGEBOX_TYPE type, string content)
    {
        if (m_HotUpdateMessageBox == null)
        {
            GameObject tmpGOHotUpdateMessageBox = GameCommon.LoadAndIntanciateUIPrefabs("HotUpdate/hotupdate_message_box", "CenterAnchor");
            m_HotUpdateMessageBox = tmpGOHotUpdateMessageBox.GetComponent<HotUpdateMessageBox>();
            if (m_HotUpdateMessageBox == null)
                return;
        }
        m_HotUpdateMessageBox.Show(type, content);
    }
    /// <summary>
    /// 设置确定、取消点击回调
    /// </summary>
    /// <param name="okCallback"></param>
    /// <param name="cancelCallback"></param>
    public static void SetOkCancelCallback(Action okCallback, Action cancelCallback)
    {
        if (m_HotUpdateMessageBox == null)
            return;
        m_HotUpdateMessageBox.m_OkCallback = okCallback;
        m_HotUpdateMessageBox.m_CancelCallback = cancelCallback;
    }
    /// <summary>
    /// 设置中间确认点击回调
    /// </summary>
    /// <param name="callback"></param>
    public static void SetMiddleOkCallback(Action callback)
    {
        if (m_HotUpdateMessageBox == null)
            return;
        m_HotUpdateMessageBox.m_OkCallback = callback;
    }
    /// <summary>
    /// 设置重试点击回调
    /// </summary>
    /// <param name="callback"></param>
    public static void SetRetryCallback(Action callback)
    {
        if (m_HotUpdateMessageBox == null)
            return;
        m_HotUpdateMessageBox.m_OkCallback = callback;
    }

    /// <summary>
    /// 显示指定类型
    /// </summary>
    /// <param name="type"></param>
    public void Show(HOTUPDATE_MESSAGEBOX_TYPE type, string content)
    {
        switch (type)
        {
            case HOTUPDATE_MESSAGEBOX_TYPE.OK: __ShowOk(); break;
            case HOTUPDATE_MESSAGEBOX_TYPE.OK_CANCEL: __ShowOkCancel(); break;
            case HOTUPDATE_MESSAGEBOX_TYPE.RETRY: __ShowRetry(); break;
        }
        if (m_LBContent != null)
            m_LBContent.text = content;

        gameObject.SetActive(true);
    }
    /// <summary>
    /// 关闭窗口
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void __ShowOk()
    {
        if (m_BtnOk != null)
            m_BtnOk.gameObject.SetActive(false);
        if (m_BtnCancel != null)
            m_BtnCancel.gameObject.SetActive(false);
        if (m_BtnRetry != null)
            m_BtnRetry.gameObject.SetActive(false);
        if (m_BtnMiddleOk != null)
            m_BtnMiddleOk.gameObject.SetActive(true);
    }
    private void __ShowOkCancel()
    {
        if (m_BtnMiddleOk != null)
            m_BtnMiddleOk.gameObject.SetActive(false);
        if (m_BtnRetry != null)
            m_BtnRetry.gameObject.SetActive(false);
        if (m_BtnOk != null)
            m_BtnOk.gameObject.SetActive(true);
        if (m_BtnCancel != null)
            m_BtnCancel.gameObject.SetActive(true);
    }
    private void __ShowRetry()
    {
        if (m_BtnOk != null)
            m_BtnOk.gameObject.SetActive(false);
        if (m_BtnCancel != null)
            m_BtnCancel.gameObject.SetActive(false);
        if (m_BtnMiddleOk != null)
            m_BtnMiddleOk.gameObject.SetActive(false);
        if (m_BtnRetry != null)
            m_BtnRetry.gameObject.SetActive(true);
    }

    /// <summary>
    /// 点击确定
    /// </summary>
    private void __OnOkClicked()
    {
        Close();
        if (m_OkCallback != null)
            m_OkCallback();
    }
    /// <summary>
    /// 点击取消
    /// </summary>
    private void __OnCancelClicked()
    {
        Close();
        if (m_CancelCallback != null)
            m_CancelCallback();
    }
    /// <summary>
    /// 点击重连
    /// </summary>
    private void __OnRetryClicked()
    {
        Close();
        if (m_OkCallback != null)
            m_OkCallback();
    }
}
