using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;

public class AnnounceInfoWindow : tWindow
{
    public class AnnounceObject
    {
        public string annid = "";
        public string content = "";
        public string title = "";
    }

    public class CS_GetAnnounce : MessageBase
    {
        public int channelId;   //> 渠道id
    }

    public class SC_GetAnnounce : RespMessage
    {
        public AnnounceObject[] arr;
    }

    Dictionary<int, GameObject> dictTableBtn = new Dictionary<int, GameObject>();
    public AnnounceObject[] arrDate;
    UITable m_table;

    public override void Init()
    {
        base.Init();
        EventCenter.Register("Button_btn_title_table", new DefineFactory<Button_BtnTitle>());
    }

    protected override void OpenInit()
    {
        base.OpenInit();
        AddButtonAction("close", () =>
        {
            //by chenliang
            //begin

            //            DataCenter.OpenWindow ("FIRST_LOAD_WINDOW");
            //-------------------
#if !UNITY_EDITOR && !NO_USE_SDK
            if (CommonParam.isUseSDK)
                DataCenter.OpenWindow("FIRST_LOAD_SDK_WINDOW");
            else
#endif
            DataCenter.OpenWindow("FIRST_LOAD_WINDOW");

            //end
            DataCenter.CloseWindow(UIWindowString.announce_info);
        });


    }

    void AnnounceInit()
    {
        m_table = GetSub("Tabletest").GetComponent<UITable>();
        for (int i = 0; i < arrDate.Length; i++)
        {
            GameObject itemObj = GameObject.Instantiate(GetSub("table_item")) as GameObject;
            itemObj.SetActive(true);
            GameObject btnObj = itemObj.transform.Find("btn_title_table").gameObject;
            itemObj.transform.parent = m_table.gameObject.transform;
            itemObj.transform.localPosition = Vector3.zero;
            itemObj.transform.localScale = Vector3.one;
            itemObj.name = "table_item" + i.ToString();

            btnObj.transform.Find("Label_title").GetComponent<UILabel>().text = arrDate[i].title;
            btnObj.transform.Find("tween/lbl_content").GetComponent<UILabel>().text = arrDate[i].content;

            int n = i;
            if (n == 0)
            {
                AnnounceItemIsOpen(true, btnObj);
            }
            else
            {
                DoLater(() =>
                {
                    AnnounceItemIsOpen(false, btnObj);
                    if (n == arrDate.Length - 1)
                    {
                        DoLater(() =>
                        {
                            m_table.repositionNow = true;
                        }, 0.02f);
                    }
                }, 0.01f);
            }

            btnObj.GetComponent<UIButtonEvent>().mData.set("INDEX", i);
            dictTableBtn.Add(i, btnObj);
        }
    }

    void AnnounceItemIsOpen(bool isOpen, GameObject btnObj)
    {
        btnObj.GetComponent<UIPlayTween>().Play(isOpen);
        SetAnnounceItemBg(isOpen, btnObj);
    }

    void SetAnnounceItemBg(bool isOpen, GameObject btnObj)
    {
        if (isOpen)
        {
            btnObj.transform.Find("triangle").transform.localEulerAngles = new Vector3(0, 0, -90);
            btnObj.GetComponent<UISprite>().spriteName = "a_ui_dengluliuchengchanghongsekuang";
        }
        else
        {
            btnObj.transform.Find("triangle").transform.localEulerAngles = new Vector3(0, 0, -180);
            btnObj.GetComponent<UISprite>().spriteName = "a_ui_dengluliuchengchangzisekuang";
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();
        //by chenliang
        //begin

        DataCenter.OpenWindow("VERSION_NUMBER_WINDOW");

        //end

        HttpModule.CallBack requestSuccess = text =>
        {
            arrDate = JCode.Decode<SC_GetAnnounce>(text).arr;

            Array.Sort(arrDate, (AnnounceObject a, AnnounceObject b) =>
                {
                    int tempA = Convert.ToInt32(a.annid);
                    int tempB = Convert.ToInt32(b.annid);
                    
                    return tempB - tempA;
                });

            AnnounceInit();
        };

        HttpModule.Instace.SendGameServerMessageT(new CS_GetAnnounce() { channelId = int.Parse(DeviceBaseData.channel) }, requestSuccess, NetManager.RequestFail);
    }

    public override void onChange(string keyIndex, object objVal)
    {

        base.onChange(keyIndex, objVal);

        if (keyIndex == "RRPOSITION")
        {
            SetTableItem((int)objVal);
        }

    }

    void SetTableItem(int tableIndex)
    {
        foreach (KeyValuePair<int, GameObject> pair in dictTableBtn)
        {
            GameObject obj = pair.Value;
            int iIndex = pair.Key;
            if (iIndex == tableIndex)
            {
                SetAnnounceItemBg(obj.transform.Find("tween").transform.localScale.y != 1, obj);
            }
            else
            {
                SetAnnounceItemBg(false, obj);
                if (obj.transform.Find("tween").transform.localScale.y == 1)
                {
                    obj.GetComponent<UIPlayTween>().Play(false);
                }
            }
        }

        DoLater(() =>
        {
            m_table.repositionNow = true;
        }, 0.01f);
    }

}

public class Button_BtnTitle : CEvent
{
    public override bool _DoEvent()
    {
        int tableIndex = (int)get("INDEX");
        DataCenter.SetData("ANNOUNCE_INFO_WINDOW", "RRPOSITION", tableIndex);
        return true;
    }
}
