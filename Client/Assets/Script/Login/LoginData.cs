using UnityEngine;
using System.Collections;

public class LoginData
{
    private static LoginData msInstance;

    private bool mIsLoginTokenValid = false;        //登录令牌是否有效
    private bool mIsGameTokenValid = false;         //游戏令牌是否有效
    private bool mIsInGameScene = false;            //是否已经在游戏内（不在创角中）

    public static LoginData Instance
    {
        get
        {
            if (msInstance == null)
                msInstance = new LoginData();
            return msInstance;
        }
    }

    /// <summary>
    /// 登录令牌是否有效
    /// </summary>
    public bool IsLoginTokenValid
    {
        set { mIsLoginTokenValid = value; }
        get { return mIsLoginTokenValid; }
    }
    /// <summary>
    /// 游戏令牌是否有效
    /// </summary>
    public bool IsGameTokenValid
    {
        set { mIsGameTokenValid = value; }
        get { return mIsGameTokenValid; }
    }
    /// <summary>
    /// 是否已经在游戏内（不在创角中）
    /// </summary>
    public bool IsInGameScene
    {
        set { mIsInGameScene = value; }
        get { return mIsInGameScene; }
    }
}
