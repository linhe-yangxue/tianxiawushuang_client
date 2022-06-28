using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using DataTable;
using Logic;

public enum RECEIVE_RESULT
{
    RECEIVE_FAIL,
    RECEIVE_SUCCEED,
    RECEIVE_SUCCEED_FINISH,
};

public class ResourcesUpdate
{
    int mAllPartCount;
    int mAlreadyReceivePartCount;
    int mScrDataSize;
    int mZipDataSize;
    string mCheckMD5;

    public string mResourcesName = "NONE";
    public CS_RequestResourcesList mUpdateManager;

    DataBuffer mReceivePartData;

    DataBuffer mResultData;
    float mStartTime;

    static public void RegisterNetEvent()
    {
        Net.gNetEventCenter.RegisterEvent("CS_RequestResouresPartData", new DefineFactoryLog<FILE_RequestPartData_R>());
        Net.gNetEventCenter.RegisterEvent("CS_RequestResouresInfo", new DefineFactoryLog<FILE_RequestDataInfo_R>());

        Net.gNetEventCenter.RegisterEvent("SC_NotifyUpdateResources", new DefineFactoryLog<SC_NotifyUpdateResources>());
        Net.gNetEventCenter.RegisterEvent("CS_RequestResourcesList", new DefineFactoryLog<CS_RequestResourcesList>());
      
    }

    public tEvent StartEvent(string szEventName)
    {
        return Net.StartEvent(szEventName);
    }

    public virtual void OnReadyReceiveBegin() { }
    public virtual void OnReceiveOnePartData(int partIndex, DataBuffer partData) { }

    public virtual int GetNextResquestPartIndex() { return mAlreadyReceivePartCount; }

    public void StartReadyReceiveData(int allPartCount, int scrDataSize, int zipDataSize, string md5)
    {
        mStartTime = Time.time;
        mAllPartCount = allPartCount;
        mScrDataSize = scrDataSize;
        mZipDataSize = zipDataSize;
        mCheckMD5 = md5;

        mReceivePartData = new DataBuffer(mZipDataSize);
        mResultData = new DataBuffer();

        mAlreadyReceivePartCount = 0;
        OnReadyReceiveBegin();
    }

    public void StartRequestData()
    {
        FILE_RequestPartData_R hRequestEvt = StartEvent("CS_RequestResouresPartData") as FILE_RequestPartData_R;
        hRequestEvt.mReceiveControl = this;
        hRequestEvt.set("RES_NAME", mResourcesName);
        hRequestEvt.set("RES_MD5", mCheckMD5);
        hRequestEvt.DoEvent();
    }

    public RECEIVE_RESULT OnReceivePartData(int partIndex, DataBuffer partData)
    {
        if (mAlreadyReceivePartCount != partIndex)
        {
            DEBUG.LogError("接收数据序列索引不正确");
            OnReceiveFail();
            return RECEIVE_RESULT.RECEIVE_FAIL;
        }

        ++mAlreadyReceivePartCount;

        mReceivePartData.write(partData.mData, partData.size());
        OnReceiveOnePartData(partIndex, partData);

        if (partIndex + 1 >= mAllPartCount)
        {
            return OnReceiveFinish();
        }

        return RECEIVE_RESULT.RECEIVE_SUCCEED;
    }
    public virtual RECEIVE_RESULT OnReceiveFinish()
    {
        // MD5校验
        string md5 = GameCommon.MakeMD5(mReceivePartData.mData, mReceivePartData.size());
        if (md5 == mCheckMD5)
        {
            OnReceiveSucceed();
            return RECEIVE_RESULT.RECEIVE_SUCCEED_FINISH;
        }
        else
        {
            DEBUG.LogError("MD5 校验失败");
            OnReceiveDataCheckFail();
            //OnReceiveFail();
            return RECEIVE_RESULT.RECEIVE_FAIL;
        }
    }

    public virtual bool OnReceiveSucceed()
    {

        mResultData = new DataBuffer(mScrDataSize);

        mResultData.seek(0);

        if (MyTest.ZipTool.RestoreZipData(mReceivePartData.mData, 0, mZipDataSize, ref mResultData.mData, mScrDataSize))
        {
            DEBUG.Log("解压原数据成功, 正确接收资源数据");

            mUpdateManager.OnSucceedReceiveResourcesData(mResourcesName, mResultData);

            ////string packFile = GameCommon.MakeGamePathFileName("config.pak");
            ////LOG.log("Succeed revice config pack> " + packFile);
            ////if (File.Exists(packFile))
            ////    File.Delete(packFile);
            ////FileStream f = new FileStream(packFile, FileMode.Create);
            ////f.Write(mResultData.mData, 0, mResultData.size());
            ////f.Close();

            if (mResourcesName == "t_configtable.pak")
            {
                GlobalModule.DestroyAllWindow();
                GlobalModule.ReloadConfigTable();
                GlobalModule.RestartGame();
            }
            

            //////!!!
            ////DataCenter.OpenWindow("LOGIN_WINDOW");

            //ResourcesPack mPack = new ResourcesPack();
            //mPack.load(testPackFile);

            //mPack.Export("d:/TestPack/");
            //mPack.close();

            return true;
        }
        else
        {
            DEBUG.LogError("Error: 解压原数据失败");
            OnReceiveDataCheckFail();
        }

        DEBUG.Log("接收文件完成");
        return false;
    }

    public virtual void OnReceiveFail()
    {
        DEBUG.LogError("接收文件失败");
        mUpdateManager.OnUpdateFail(UPDATE_RESULT.eDownLoadFail);
    }

    public virtual void OnReceiveDataCheckFail()
    {
        DEBUG.LogError("接收数据验证失败");
        mUpdateManager.OnUpdateFail(UPDATE_RESULT.eDownLoadDataCheckFail);
    }

}

public class FILE_RequestDataInfo_R : tNetEvent
{
    public ResourcesUpdate mReceiveControl;

    //public override bool _DoEvent()
    //{
    //    WaitTime(20);
    //    return true;
    //}

    public override void _OnResp(tEvent respEvt)
    {
        respEvt.Dump();
        mReceiveControl.StartReadyReceiveData(respEvt["PARTCOUNT"], respEvt["SCRDATASIZE"], respEvt["ZIPSIZE"], respEvt["CHECKMD5"]);
        mReceiveControl.StartRequestData();        
    }
}


public class FILE_RequestPartData_R : tServerEvent
{
    public ResourcesUpdate mReceiveControl;

    public override bool _NeedFinishWhenResponsed() { return false; }

    public override bool _DoEvent()
    {
        int mPartIndex = mReceiveControl.GetNextResquestPartIndex();
        set("PARTINDEX", mPartIndex);

        WaitTime(20);

        return true;
    }

    public override void _OnResp(tEvent respEvt)
    {
        respEvt.Dump();

        int partIndex = respEvt["PARTINDEX"];
        DataBuffer partData = respEvt.getObject("PARTDATA") as DataBuffer;

        if (partData != null)
        {
            RECEIVE_RESULT re = mReceiveControl.OnReceivePartData(partIndex, partData);
            if (re == RECEIVE_RESULT.RECEIVE_SUCCEED)
            {
                SetFinished(false);
                DoEvent();
                return;
            }
            else if (re == RECEIVE_RESULT.RECEIVE_SUCCEED_FINISH)
            {
                //mReceiveControl.OnReceiveFinish();
                Finish();
                return;
            }
        }
        mReceiveControl.OnReceiveFail();
        Finish();
    }
}
//-------------------------------------------------------------------------
public enum UPDATE_RESULT
{
    eSucceed,
    eLoadListFail,
    eDownLoadFail,
    eDownLoadDataCheckFail,
}
// Download update resources
public class CS_RequestResourcesList : tServerEvent
{
    NiceTable mNewResourcesList;
    //NiceTable mLocalReourcesList;
    NiceTable mNeedUpdateList;

    public override bool _NeedFinishWhenResponsed() { return true; }

    public override bool _DoEvent()
    {
        // Load local resources list table
        //string fileName = GameCommon.MakeGamePathFileName("resources_list.tab");
        //mLocalReourcesList = new NiceTable();
        //if (!mLocalReourcesList.LoadBinary(fileName))
        //{
        //    mLocalReourcesList = new NiceTable();
        //    mLocalReourcesList.SetField("INDEX", FIELD_TYPE.FIELD_STRING, 0);
        //    mLocalReourcesList.SetField("MD5", FIELD_TYPE.FIELD_STRING, 1);
        //}       
   
        ///!!! 10 min download time
        WaitTime(10);
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {

		mNewResourcesList = respEvent.getObject("RES_LIST") as NiceTable;

        if (mNewResourcesList == null)
        {
            OnUpdateFail(UPDATE_RESULT.eLoadListFail);
            Finish();
            return;
        }

        mNeedUpdateList = new NiceTable();
        mNeedUpdateList.SetField("INDEX", FIELD_TYPE.FIELD_STRING, 0);
        mNeedUpdateList.SetField("MD5", FIELD_TYPE.FIELD_STRING, 1);
        // Check need udpate resources
        foreach (KeyValuePair<int, DataRecord> kRe in mNewResourcesList.GetAllRecord())
        {
            DataRecord re = kRe.Value;
            string resIndex = re["INDEX"];
            if (resIndex!="")
            {
                // Read from local disk file, and make md5
                string existMD5 = "";
                string localResFileName = GameCommon.MakeGamePathFileName(resIndex);
                if (File.Exists(localResFileName))
                {
                    FileStream f = new FileStream(localResFileName, FileMode.Open);
                    int len = (int)f.Length;
                    DataBuffer d = new DataBuffer(len);
                    f.Read(d.mData, 0, len);
                    f.Close();
                    existMD5 = GameCommon.MakeMD5(d.mData, len);
                }
                //DataRecord existRe = mLocalReourcesList.GetRecord(resIndex);
                string newMD5 = (string)re["MD5"];
                if (existMD5 != newMD5)
                {
                    //DataRecord newRe = mLocalReourcesList.CreateRecord(resIndex);
                    //newRe.set("MD5", newMD5);
                    DataRecord needRe = mNeedUpdateList.CreateRecord(resIndex);
					needRe.set("MD5", newMD5);
                }
            }
        }

        if (mNeedUpdateList.GetRecordCount() <= 0)
            OnSucceedUpdateResources();
        else
        {
            WaitTime(20 * 60);

            ///MainProcess.ClearBattle();
            StartUpdateResources();
        }
    }

    void StartUpdateResources()
    {
        if (mNeedUpdateList.GetRecordCount() <= 0)
        {
            OnSucceedUpdateResources();
            return;
        }

        DataRecord re = mNeedUpdateList.GetFirstRecord();

        FILE_RequestDataInfo_R requestConfig = Net.StartEvent("CS_RequestResouresInfo") as FILE_RequestDataInfo_R;
        requestConfig.set("RES_NAME", (string)re.get(0));
        requestConfig.mReceiveControl = new ResourcesUpdate();
        requestConfig.mReceiveControl.mResourcesName = (string)re.get(0);
        requestConfig.mReceiveControl.mUpdateManager = this;
        requestConfig.WaitTime(60 * 50);

        requestConfig.DoEvent();
    }

    public void OnSucceedReceiveResourcesData(string resName, DataBuffer resourcesData)
    {
        string packFile = GameCommon.MakeGamePathFileName(resName);
        DEBUG.Log("Succeed revice config pack> " + packFile);
        if (File.Exists(packFile))
            File.Delete(packFile);
        FileStream f = new FileStream(packFile, FileMode.Create);
		f.Write(resourcesData.mData, 0, resourcesData.size());
        f.Close();

		mNeedUpdateList.DeleteRecord(resName);

        StartUpdateResources();
    }

    public void OnSucceedUpdateResources()
    {
        string resListInfo = "";
        foreach (KeyValuePair<int, DataRecord> kRe in mNewResourcesList.GetAllRecord())
        {
            DataRecord re = kRe.Value;
            resListInfo += re.get(0);
            resListInfo += re.get(1);
        }

        string resListMD5 = GameCommon.MakeMD5(resListInfo);

        string fileName = GameCommon.MakeGamePathFileName("resources_md5.txt");
        try
        {           
			StreamWriter writer = new StreamWriter(fileName, false, Encoding.Unicode);
            writer.WriteLine(resListMD5);
			writer.Close();
        }
        catch { }

        //string listFileName = GameCommon.MakeGamePathFileName("resources_list.tab");
        //try
        //{
        //    if (mLocalReourcesList.SaveBinary(listFileName))
        //    {
        //        LOG.log("Succeed update resources, save list >" + listFileName);
        //    }
        //    else
        //        DEBUG.LogError("Fail save resources list >" + listFileName);
        //}
        //catch { } 

        DEBUG.LogWarning("Resources update succeed, then restart game");
        GlobalModule.RestartGame();

        //GlobalModule.mInstance.LoadScene("Loading", false);
        //DataCenter.OpenWindow("LOGIN_WINDOW");

    }

    public void OnUpdateFail( UPDATE_RESULT errorType)
    {
        DEBUG.LogError("Update resources fail > "+errorType.ToString());
    }
}

//-------------------------------------------------------------------------
// Server notify must update resource
public class SC_NotifyUpdateResources : DefaultNetEvent
{
    public override bool _DoEvent()
    {
        DEBUG.LogWarning("SC_NotifyUpdateResources > restart game");
        GlobalModule.RestartGame();

        //FILE_RequestDataInfo_R requestConfig = Net.StartEvent("CS_RequestResouresInfo") as FILE_RequestDataInfo_R;
        //requestConfig.set("RES_NAME", "t_resources");
        //requestConfig.mReceiveControl = new ResourcesUpdate();
        //requestConfig.WaitTime(60 * 50);

        //requestConfig.DoEvent();

        return true;
    }
}
//-------------------------------------------------------------------------