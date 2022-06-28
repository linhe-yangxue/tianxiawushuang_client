using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class SharkBonjour :
#if UNITY_ANDROID
		Bonjour_Shark_Common
#elif UNITY_IOS
		Bonjour_Shark_Common_IOS
#elif UNITY_STANDALONE_WIN
    Bonjour_Shark_Common_Win
#endif
{
}
