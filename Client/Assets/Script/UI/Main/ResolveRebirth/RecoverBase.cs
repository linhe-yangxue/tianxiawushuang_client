using UnityEngine;
using System.Collections;
using Logic;
using System.Linq;
using System.Collections.Generic;
using System;


abstract class RecoverBase<T> : tWindow where T : ItemDataBase {
    protected const string autoAddBtn = "Button_auto_add_btn_";
    protected const string normalAddBtn = "Button_normal_add_btn_";
    protected const string resolveBtn = "Button_resolve_btn_";


    protected GameObject[] nativeUIArr;
    protected T[] itemDataArr;

    protected List<int[]> resolventInfoList = new List<int[]>();//index：0是icon图标编号 1是数量 2是tid值是-1的话说明是装备或者符灵
    protected int nativeGridCount;
    protected int resolventGridMaxCount;

    protected Func<T, bool> resolveCondition;
    protected Func<T, bool> resolveConditionNormalAdd;

    protected void GridInit() {
        UIGridContainer nativeContainer = GetComponent<UIGridContainer>("native_group");
        nativeContainer.MaxCount = nativeGridCount;
        nativeUIArr = nativeContainer.controlList.Select(go => go.transform.FindChild("native_icon_btn").gameObject).ToArray();
        for (int i = 0; i < nativeUIArr.Length; i++)
        {
            nativeUIArr[i].GetComponent<UISprite>().atlas = null;
            GameCommon.FindObject(nativeUIArr[i], "resolve_btn_close").SetActive(false);

        }

        UIGridContainer resolventContainer = GetComponent<UIGridContainer>("resolvent_group");
        resolventContainer.MaxCount = resolventGridMaxCount;
        resolventContainer.controlList.ForEach(go => go.SetActive(false));
    }

    public override void onChange(string keyIndex, object objVal) {
        base.onChange(keyIndex, objVal);
        if (UIWindowString.autoAddMethod == keyIndex) AutoAdd();
        else if (UIWindowString.normalAddMethod == keyIndex) NormalAdd();
        else if (UIWindowString.resolveMethod == keyIndex) Resolve();
    }

    public override void OnOpen() {
        base.OnOpen();
        GridInit();
        if (itemDataArr != null && itemDataArr.Length != 0) {
            CreateResolventInfo();
            ShowNativeUI();
            ShowResolventUI();
        }
    }

    public override void Close() {
        base.Close();
        itemDataArr = null;
        resolventInfoList.Clear();
    }

    public void RemoveResolveCloseBtn()
    {
        if (itemDataArr != null && itemDataArr.Length > 0)
        {
            GameObject board = nativeUIArr[itemDataArr.Length - 1];
            GameCommon.FindObject(board, "resolve_btn_close").SetActive(false);
        }
    }

    public void RemoveitemDataArrByIndex(int index)
    {
        List<T> list = itemDataArr.ToList();
        if (list.Count > index)
        {
            list.RemoveAt(index);
        }
        itemDataArr = list.ToArray();
    }

    public void InsertTtemDataArrByIndex(List<T> itemDataTemp)
    {
        if (itemDataTemp == null)
        {
            return;
        }
        if (itemDataArr != null)
        {
            List<T> list = itemDataArr.ToList();
            for (int i = 0; i < itemDataTemp.Count; i++)
            {
                if (list.Count < 6 && itemDataTemp[i] != null && !list.Contains(itemDataTemp[i]))
                {
                    list.Add(itemDataTemp[i]);
                }
            }
            itemDataArr = list.ToArray();
        }
        else
        {
            itemDataArr = itemDataTemp.ToArray();
        }
    }

    protected virtual void ShowResolventUI() {
        if(itemDataArr==null) return;
        var count = resolventInfoList.Count;
        var list = GetComponent<UIGridContainer>("resolvent_group").controlList;
        for (int i = 0; i < count; i++) {
            //GameCommon.SetItemIcon(list[i].transform.Find("resolvent_icon_btn").GetComponent<UISprite>(),PackageManager.GetItemTypeByTableID(resolventInfoList[i][2]), resolventInfoList[i][2]); 
			GameCommon.SetOnlyItemIcon(list[i].transform.Find("resolvent_icon_btn").gameObject, resolventInfoList[i][2]);
            list[i].SetActive(true);

            var label = list[i].GetComponentInChildren<UILabel>();
            label.text = "×" + resolventInfoList[i][1];
        }
    }

    protected void AddResolventInfoList(int resolventUIIndex, int count, int tid) {
        if (count == 0) return;
        int[] arr = new int[3] { resolventUIIndex, count, tid };
        resolventInfoList.Add(arr);
    }
    protected ItemDataBase[] GetItemDataArr() {
        ItemDataBase[] arr = new ItemDataBase[itemDataArr.Length];

        for (int i = 0; i < itemDataArr.Length; i++) {
            arr[i] = new ItemDataBase();
            arr[i].tid = itemDataArr[i].tid;
            arr[i].itemId = itemDataArr[i].itemId;
            arr[i].itemNum = 1;

        }
        return arr;
    }

    protected ItemDataBase GetItemData() {
        ItemDataBase item = new ItemDataBase();
        item.tid = itemDataArr[0].tid;
        item.itemId = itemDataArr[0].itemId;
        item.itemNum = 1;
        return item;
    }

    protected void RequestResolveFail(string text) {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    protected virtual void RequestResolveSuccess(string text) {
        DEBUG.Log("RequestResolveSuccess:text = " + text);
        SC_PetDisenchant item = JCode.Decode<SC_PetDisenchant>(text);
        PackageManager.UpdateItem(item.arr);
        for (int i = 0; i < itemDataArr.Length; i++) {
            PackageManager.RemoveItem(itemDataArr[i]);
        }

        List<ItemDataBase> listGet = new List<ItemDataBase>(); //get rewards
        for (int i = 0; i < resolventInfoList.Count; i++) {
            var info = resolventInfoList[i];
            ItemDataBase itemDataBase = new ItemDataBase();
            itemDataBase.itemNum = info[1];
            itemDataBase.tid = info[2];
            listGet.Add(itemDataBase);
            if (info[2] > 1000000 && info[2] < 2000000) {
                PackageManager.AddItem(info[2], -1, info[1]);
            }
            
        }
        GlobalModule.DoLater (() => DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", listGet.ToArray()), 0.5f); //显示分解结果
        resolventInfoList.Clear();
        GridInit();
        itemDataArr=null;

        //added by xuke 红点相关
        RecoverNewMarkManager.Self.CheckRecoverPet_NewMark();
        RecoverNewMarkManager.Self.CheckRecoverEquip_NewMark();
        RecoverNewMarkManager.Self.RefreshRecoverNewMark();
        //end
    }
    protected virtual void AutoAdd() { }

    protected virtual void CreateResolventInfo() {
        resolventInfoList.Clear();
    }
    protected abstract void NormalAdd();
    protected abstract void Resolve();
    protected abstract void ShowNativeUI();

}
