using UnityEngine;
using System.Collections;

public class FriendsSortRule : IComparer {
    public int Compare(object x, object y)
    {
        FriendData l = x as FriendData;
        FriendData r = y as FriendData;
        
        if (l == null || r == null)
            return 0;

        bool bothEnable = l.enableSendSpirit && r.enableSendSpirit;
        bool bothDisable = !l.enableSendSpirit && !r.enableSendSpirit;
        bool onlyLtEnable = l.enableSendSpirit && !r.enableSendSpirit;
        bool onlyRtEnable = !l.enableSendSpirit && r.enableSendSpirit;

        if (onlyLtEnable)
        {
            return -1;
        }
        if (onlyRtEnable)
        {
            return 1;
        }
        if (bothEnable || bothDisable)
        {
            return string.Compare(l.name,r.name);
        }
        return 0;
    }
}