using UnityEngine;
using System.Collections;

/// <summary>
/// 聊天红点管理
/// </summary>
public class ChatNewMarkManager : ManagerSingleton<ChatNewMarkManager>
{
    //--------------
    // 检测项
    //--------------
    /// <summary>
    /// 检测世界聊天是否有红点
    /// </summary>
    public bool CheckWorldChat { set; get; }
    /// <summary>
    /// 检测私聊是否有红点
    /// </summary>
    public bool CheckPrivateChat { set; get; }
    /// <summary>
    /// 检测宗门聊天是否有红点
    /// </summary>
    public bool CheckUnionChat { set; get; }
    
    
    //--------------
    // 可见性
    //--------------
    /// <summary>
    /// 世界聊天红点是否可见
    /// </summary>
    public bool WorldChatVisible { get { return CheckWorldChat; } }
    /// <summary>
    /// 私聊红点是否可见
    /// </summary>
    public bool PrivateChatVisible { get { return CheckPrivateChat; } }
    /// <summary>
    /// 宗门聊天红点是否可见
    /// </summary>
    public bool UnionChatVisible { get { return CheckUnionChat; } }
    /// <summary>
    /// 聊天红点是否可见
    /// </summary>
    public bool ChatVisible { get { return WorldChatVisible || PrivateChatVisible || UnionChatVisible; } }

    //--------------
    // 检测逻辑
    //--------------
    /// <summary>
    /// 根据聊天类型来设置聊天红点状态
    /// </summary>
    /// <param name="kChatType"></param>
    /// <param name="kVisible"></param>
    public void SetChatStateByType(ChatType kChatType,bool kVisible) 
    {
        switch (kChatType) 
        {
            case ChatType.World:
                CheckWorldChat = kVisible;
                break;
            case ChatType.Union:
                CheckUnionChat = kVisible;
                break;
            case ChatType.Private:
                CheckPrivateChat = kVisible;
                break;
        }
    }

    //----------------
    // 刷新逻辑
    //----------------
    /// <summary>
    /// 刷新主界面聊天红点
    /// </summary>
    public void RefreshChatNewMark() 
    {
        GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_CHAT_MARK", ChatVisible);
    }

    public void RefreshAllChatNewMark() 
    {
        DataCenter.SetData("CHAT_WINDOW", "REFRESH_ALL_CHAT_NEWMARK",null);
    }
}
