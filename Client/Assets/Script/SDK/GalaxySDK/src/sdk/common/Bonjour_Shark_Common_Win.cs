using UnityEngine;
using System.Collections;
/* 
 * Bonjour_Shark_Common_Win为Unity Windows编译环境下使用，仅为了编译不报错，故无具体实现。
 * 具体Ａｎｄｒｏｉｄ、ＩＯＳ有相关实现，接入方无需过多关注，如有疑问请联系提供方。
 * 
 */
#if UNITY_STANDALONE_WIN
public class Bonjour_Shark_Common_Win : Bonjour_Shark_Base
{


        public override void initSDK()
        {
            DEBUG.Log("CallInitSDK");            
        }

        public override void ShowLogin()
        {
            DEBUG.Log("CallLogin");
           
        }

        public override void ShowLogout()
        {
            DEBUG.Log("CallLogout");
           
        }
        public override void ShowPersonCenter()
        {
            DEBUG.Log("CallPersonCenter");

           
        }
        public override void HidePersonCenter()
        {
            DEBUG.Log("CallHidePersonCenter");

           
        }
        public override void ShowToolBar()
        {
            DEBUG.Log("CallToolBar");

           
        }
        public override void HideToolBar()
        {
            DEBUG.Log("CallHideToolBar");

            
        }
        public override string PayItem(U3DSharkBaseData _in_pay)
        {
            DEBUG.Log("CallPayItem" + "data: " + _in_pay.DataToString());
           
            return "";
        }
        public override int LoginState()
        {
            DEBUG.Log("CallLoginState");            
            return 0;
        }
        public override void ShowShare(U3DSharkBaseData _in_data)
        {
            DEBUG.Log("CallShare" + "data: " + _in_data.DataToString());

        }
        public override void SetPlayerInfo(U3DSharkBaseData _in_data)
        {
            DEBUG.Log("CallSetPlayerInfo" + " data :" + _in_data.DataToString());
            
        }
        public override U3DSharkBaseData GetUserData()
        {
            DEBUG.Log("CallUserData");
            U3DSharkBaseData outData = new U3DSharkBaseData();
            return outData;
        }
        public override U3DSharkBaseData GetPlatformData()
        {
            DEBUG.Log("CallPlatformData");

           
            U3DSharkBaseData outData = new U3DSharkBaseData();
            return outData;

        }

        public override void CopyClipboard(U3DSharkBaseData _in_data)
        {
            DEBUG.Log("CallLogout" + " data: " + _in_data.DataToString());

            
        }
        public override bool IsHasRequest(string requestType)
        {
            DEBUG.Log("IsHasRequest" + " type " + requestType);           
            return true;
        }
        public override void Destory()
        {
            DEBUG.Log("CallDestory");

            
        }
        public override void ExitGame()
        {
            DEBUG.Log("ExitGame");
        }

        /**call any undefine function if success or return error*/
        public override string DoAnyFunction(string funcName, U3DSharkBaseData _in_data)
        {
            DEBUG.Log("DoAnyFunction");
            return "";
        }

        public override string DoPhoneInfo()
        {
            DEBUG.Log("DoPhoneInfo");

            return "";
        }

        public override void AddLocalPush(U3DSharkBaseData _push_data)
        {
            DEBUG.Log("AddLocalPush");

        }
        public override void RemoveLocalPush(string pushid)
        {
            DEBUG.Log("RemoveLocalPush");

        }
        public override void RemoveAllLocalPush()
        {
            DEBUG.Log("RemoveLocalPush");
          
        }
        public override void GetUserFriends()
        {
            DEBUG.Log("GetUserFriends");
           
        }

}
#endif
