using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;

public class UDPNet
{
    public ManualResetEvent receiveAsynEvent = new ManualResetEvent(false);
    public ManualResetEvent sendAsynEvent = new ManualResetEvent(false);
    IAsyncResult mAsynResult;
    IAsyncResult mSendAsynResult;

    IPEndPoint mIPEndPoint;

    UdpClient mUdpClient = null;

    //-------------------------------------------------
    public void InitClear()
    {      
        if (mUdpClient != null)
            mUdpClient.Close();
        mUdpClient = null;        
    }

    public void InitNet(string strIP, int port)
    {
        InitClear();
        mUdpClient = new UdpClient();
        mUdpClient.Connect(strIP, port);

		IPAddress addrIP = IPAddress.Parse(strIP);

		mIPEndPoint = new IPEndPoint(addrIP, port);
    }

    public void Log(string info)
    {
        Logic.EventCenter.Log(LOG_LEVEL.GENERAL, info);
    }

    // 直接发送数据
    public bool SendData(byte[] sendData, int beginIndex, int count)
    {
        try
        {
            if (beginIndex == 0)
                return mUdpClient.Send(sendData, count) == count;
            else
            {
                //byte[] temp = new byte[count];
                //Array.Copy(sendData, beginIndex, temp, 0, count);
                return mUdpClient.Send(sendData, count) == count;
            }
        }
        catch (Exception e)
        {
            Log("XXX Error: 发送失败 need size >" + e.ToString());
        }
        return false;
    }

    public int _AsynReceiveData(ref byte[] destData, int beginIndex, int freeSpace)
    {
        if (receiveAsynEvent.WaitOne(1, false))
        {
            receiveAsynEvent.Reset();
            byte[] revData = null;
            try
            {
                revData = mUdpClient.EndReceive(mAsynResult, ref mIPEndPoint);
            }
            catch (Exception e)
            {
				Log("UDP Rev Error: >" + e.ToString());
                return -1;
            }
            int revSize = 0;
            if (revData != null && revData.Length > 0)
            {
                revSize = revData.Length;
                Log("~~~~~~~~~~~~~Succeed Receive size >" + revSize.ToString());
                // may be to process msg
                if (revSize <= freeSpace)
                    revData.CopyTo(destData, beginIndex);
                else
                    Log("XXX Error: 接收缓存已满");
            }
            mAsynResult = mUdpClient.BeginReceive(ReceiveCallback, this);
            return revSize;
        }
        else if (mAsynResult == null && mIPEndPoint != null)
            mAsynResult = mUdpClient.BeginReceive(ReceiveCallback, this);

        return 0;
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            UDPNet net = (UDPNet)ar.AsyncState;
            net.receiveAsynEvent.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
