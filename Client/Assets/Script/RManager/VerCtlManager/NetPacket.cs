using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace VersionControl
{
    public enum LogLevel
    {
        LOGL_ERROR = -1,
        LOGL_WARN  = 0,
        LOGL_DEBUG = 1,
        LOGL_INFO  = 2,
        LOGL_VERB  = 3
    }

    public struct Buffer
    {
        public byte[] buff;
        public int    offset;
        public int    length;
    }

    public interface ILogger
    {
        void Log(LogLevel lgl, string log);
    }

    public interface IPacketContext
    {
        void Log(LogLevel lgl, string log);
    }

    public interface IPacketHandler
    {
        void SetPrePacket(IPacketHandler pre);
        bool IsReadyToProcess();
        string ExpectedName {get; set;}

        bool ParseData(Buffer dataBuff);

        void Process(IPacketContext ctx);

        IPacketHandler ClonePacketType(string LRI, string param);

        int  WriteToStream(Stream stream);

        bool IsDisconnectFromServer {get; set;}
    }

    public interface INetStat
    {
        void InformReceiving(int len);
        void InformReceived(int len);

        void InformSending(int len);
        void InformSend(int len);

        void InformNetWaiting();
    }

    public class NetWaiting : INetStat
    {
        public Action   OnNetWaiting { get; set; }
        public Action<int>  OnReceived {get;set;}

        public void InformReceiving(int len)
        {
            //throw new NotImplementedException();
        }

        public void InformReceived(int len)
        {
            if(OnReceived != null)
                OnReceived(len);
        }

        public void InformSending(int len)
        {
            //throw new NotImplementedException();
        }

        public void InformSend(int len)
        {
            //throw new NotImplementedException();
        }

        public void InformNetWaiting()
        {
            if(OnNetWaiting != null)
                OnNetWaiting();
        }
    }

    class DefaultPacketHandler<T> : IPacketHandler where T : DefaultPacketHandler<T>, new()
    {
        bool mIsPreparedToDisconnet = false;
        bool mIsHasError = false;
        int  mErrorCode = 0;
        string mError = "";

        protected string PackLRI { get; private set; }
        protected string PackParam {get; private set; }

        protected Dictionary<string, string> PackParamPairs {get; private  set;}

        protected void SetError(int errorCode, string error)
        {
            mIsHasError = true;
            mErrorCode = errorCode;

            if(!string.IsNullOrEmpty(mError))
                mError = string.Format("{0};{1}", mError, error);
            else
                mError = error;
        }

        public bool HasError { get { return mIsHasError; }}
        public int ErrorCode { get {return mErrorCode; }}
        public string Error { get { return mError; }}

        public virtual void SetPrePacket(IPacketHandler pre)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsReadyToProcess()
        {
            return true;
        }

        public virtual string ExpectedName
        {
            get { return "Default"; }
            set { }
        }

        public virtual bool ParseData(Buffer dataBuff)
        {
            return false;
        }

        public virtual void Process(IPacketContext ctx)
        {
            throw new NotImplementedException();
        }

        public virtual IPacketHandler ClonePacketType(string LRI, string param)
        {
            var res = new T();
            res.PackLRI = LRI;
            res.PackParam = param;
            if(!string.IsNullOrEmpty(param) && param != "")
            {
                res.PackParamPairs = new Dictionary<string,string>();
                var pairs = param.Split("&".ToCharArray());
                foreach(var pair in pairs)
                {
                    var index = pair.IndexOf("=");
                    if(index <= 0)
                        continue;
                    var name = pair.Substring(0, index);
                    var value = pair.Substring(index + "=".Length);

                    res.PackParamPairs.Add(name, value);
                }
            }
            return res;
        }

        public virtual int  WriteToStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public bool IsDisconnectFromServer {get { return mIsPreparedToDisconnet; } set { mIsPreparedToDisconnet = value; } }

        public void DisconnectServer()
        {
            mIsPreparedToDisconnet = true;
        }
    }

    class UnkownPacketHandler : DefaultPacketHandler<UnkownPacketHandler>
    {}

    public class NetPacket
    {
        const  int NetTimeOutMilliSec = 3000;
        static int LenTypeLen = BitConverter.GetBytes((Int32)0).Length;
        static int HeadLenLen = BitConverter.GetBytes((Int16)0).Length;
        const  int MAX_PACK_BODY_LEN = Int32.MaxValue;
        static byte[] PacketName = new byte[16];

        IPacketHandler mCurrentPacket;
        bool    mIsDerferedToProcess;
        IPacketHandler mExpectedPacket;
        int     mCurrentExpectedLen;
        ILogger mLogger;

        DateTime    mNetStart;
        bool        mIsAboutToReset;

        Dictionary<string, IPacketHandler> mAllPacketHandlerPrototypes;

        public NetPacket(ILogger logger)
        {
            mLogger = logger;
            mCurrentPacket = null;
            mExpectedPacket = null;
            mCurrentExpectedLen = -1;
            mIsDerferedToProcess = false;
            mIsAboutToReset = false;

            mAllPacketHandlerPrototypes = new Dictionary<string,IPacketHandler>();
        }

        public int PacketHeadLen { get {return LenTypeLen; } }

        public void RegisterPacketHandler(string name, IPacketHandler prototype)
        {
            mAllPacketHandlerPrototypes.Add(name, prototype);
        }

        void Log(LogLevel lgl, string log)
        {
            if(mLogger!= null)
            {
                mLogger.Log(lgl, log);
            }
        }

        void AddPacket(IPacketHandler packet)
        {
            if(packet == null)
            {
                mCurrentPacket = null;
                return;
            }

            if(mCurrentPacket != null)
                packet.SetPrePacket(mCurrentPacket);

            mCurrentPacket = packet;

            if(!mCurrentPacket.IsReadyToProcess())
                mIsDerferedToProcess = true;
            else
                mIsDerferedToProcess = false;
        }

        public void SetExpectedPacket(IPacketHandler expected)
        {
            mExpectedPacket = expected;
            if (expected != null)
            {
                mCurrentExpectedLen = -1;
                mIsDerferedToProcess = false;
            }
        }

        public int ExtractPacketBodySize(Buffer headDataBuff)
        {
            if(headDataBuff.length < LenTypeLen)
                return headDataBuff.length - LenTypeLen;

            var result = BitConverter.ToInt32(headDataBuff.buff, headDataBuff.offset);
            if(result < 0)
                result = 0;

            return result;
        }

        public int ConsumeData(Buffer dataBuff)
        {
            if(mCurrentExpectedLen <= 0)
            {
                var readHead = ExtractPacketBodySize(dataBuff);

                if(readHead < 0)
                    return readHead;

                mCurrentExpectedLen = readHead;
                if (mCurrentExpectedLen == 0 || mCurrentExpectedLen > MAX_PACK_BODY_LEN)
                {
                    var error = "Invalide Pack Data !!";
                    Log(LogLevel.LOGL_ERROR, "Protocol: " + error);
                    mCurrentExpectedLen = -1;
                    throw new Exception(error);
                }

                return LenTypeLen;
            }

            if(dataBuff.length < mCurrentExpectedLen)
                return dataBuff.length - mCurrentExpectedLen;

            var consumed = 0;
            var nameLen = BitConverter.ToInt16(dataBuff.buff, dataBuff.offset);
            consumed += HeadLenLen;

            if (nameLen + consumed > dataBuff.length)
            {
                var error = "Invalide Pack Data, length mismatch !!";
                Log(LogLevel.LOGL_ERROR, "Protocol: " + error);
                mCurrentExpectedLen = -1;
                throw new Exception(error);
            }

            string packString = Encoding.UTF8.GetString(dataBuff.buff, dataBuff.offset + consumed, nameLen);

            if (string.IsNullOrEmpty(packString) || packString == "")
            {
                var error = "Invalide Pack Data: No packet name provided!!";
                Log(LogLevel.LOGL_ERROR, "Protocol: " + error);
                mCurrentExpectedLen = -1;
                throw new Exception(error);
            }

            var index = packString.IndexOf("]:");
            var packName = packString;
            var packLRI = "";
            var packParam = "";

            if(index > 0)
            {
                packName = packString.Substring(0, index);
                packLRI = packString.Substring(index + "]:".Length);

                index = packLRI.IndexOf("?");
                if(index > 0)
                {
                    packParam = packLRI.Substring(index + "?".Length);
                    packLRI = packLRI.Substring(0, index);
                }
            }

            if(mExpectedPacket != null && !packName.Equals(mExpectedPacket.ExpectedName))
            {
                var error = "Invalide Pack Data: Not expected packet name!!";
                Log(LogLevel.LOGL_ERROR, "Protocol: " + error);
                mCurrentExpectedLen = -1;
                throw new Exception(error);
            }

            consumed += nameLen;

            var bodyBuff = new Buffer
                {
                    buff = dataBuff.buff,
                    offset = dataBuff.offset + consumed,
                    length = dataBuff.length - consumed
                };
            
            if(mExpectedPacket != null)
            {
                if(mExpectedPacket.ParseData(bodyBuff))
                    AddPacket(mExpectedPacket);
            }
            else
            {
                if(!mAllPacketHandlerPrototypes.ContainsKey(packName.ToLower()))
                {
                    var error = "Invalide Pack Data: Unkown packet: " + packName;
                    Log(LogLevel.LOGL_ERROR, "Protocol: " + error);
                    mCurrentExpectedLen = -1;
                    throw new Exception(error);
                }

                var protoType = mAllPacketHandlerPrototypes[packName.ToLower()];
                if(protoType == null)
                {
                    var error = "Invalide Pack Data: Unkown packet: " + packName;
                    Log(LogLevel.LOGL_ERROR, "Protocol: " + error);
                    mCurrentExpectedLen = -1;
                    throw new Exception(error);
                }

                var packet = protoType.ClonePacketType(packLRI, packParam);

                try
                {
                    var isOK = packet.ParseData(bodyBuff);
                    if(!isOK)
                        throw new Exception("Packet Parse failed!");
                }
                catch(Exception ex)
                {
                    var error = string.Format("Invalide Pack Data: cannot parse data body; packet name = {0}; error : {1}", packName, ex);
                    Log(LogLevel.LOGL_ERROR, "Protocol: " + error);
                    mCurrentExpectedLen = -1;
                    throw new Exception(error);
                }

                AddPacket(packet);
                consumed += bodyBuff.length;
            }

            mCurrentExpectedLen = -1;
            return consumed;
        }

        public IPacketHandler FetchPacket()
        {
            if(mIsDerferedToProcess)
            {
                return null;
            }
            else
            {
                var result = mCurrentPacket;
                mCurrentPacket = null;
                return result;
            }
        }

        public bool IsResetNet()
        {
            return mIsAboutToReset;
        }

        public void InformNetRunStart()
        {
            mNetStart = DateTime.Now;
            mIsAboutToReset = false;
        }

        public void InformNetRunning()
        {
            var now = DateTime.Now;
            if ((now - mNetStart).TotalMilliseconds > NetTimeOutMilliSec)
                mIsAboutToReset = true;
        }

        public void InformNetRunEnd()
        {
            mIsAboutToReset = false;
        }
    }

    public class NetPacketHelper
    {
        public struct PacketReadResult
        {
            public IPacketHandler   packet;
            public int          packetSize;
            public string       errorMsg;
            public Exception    exception;
        }

        public class PacketBuffer
        {
            public byte[] headBuff;
            public byte[] bodyBuff;

            public byte[] testBuff;

            public PacketBuffer()
            {
                testBuff = new byte[8];
            }
        }

        // debug
        //static List<PacketReadResult> allPackets = new List<PacketReadResult>();

        static PacketReadResult ConstructPacket(NetPacket netProtocol, Buffer head, Buffer body)
        {
            var totalRead = 0;
            var consumed = 0;
            try
            {
                consumed = netProtocol.ConsumeData(head);
                if (consumed <= 0)
                {
                    return new PacketReadResult
                    {
                        packet = null,
                        errorMsg = "ReadPacket : Invalid protocol provided.",
                        exception = null
                    };
                }

                totalRead += consumed;

                consumed = netProtocol.ConsumeData(body);
                if (consumed <= 0)
                {
                    return new PacketReadResult
                    {
                        packet = null,
                        errorMsg = "ReadPacket : Invalid protocol provided.",
                        exception = null
                    };
                }

                totalRead += consumed;
            }
            catch (Exception ex)
            {
                return new PacketReadResult
                {
                    packet = null,
                    errorMsg = "",
                    exception = ex
                };
            }

            var packet = netProtocol.FetchPacket();
            return new PacketReadResult
            {
                packet = packet,
                packetSize = totalRead,
                errorMsg = "",
                exception = null
            };
        }

        /// <summary>
        /// NOTE: DO NOT use in multithreaded
        /// </summary>
        /// <param name="netClient"></param>
        /// <param name="netProtocol"></param>
        /// <param name="stream"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IEnumerator ReceiveOnePacket(
            TcpClient netClient, 
            NetPacket netProtocol,
            NetworkStream stream,
            PacketBuffer  packBuffer,
            INetStat  dataStat,
            List<PacketReadResult> result)
        {
            var headBuff = packBuffer.headBuff;
            //if(headBuff == null)
            //    headBuff = new byte[netProtocol.PacketHeadLen];

            var isHeadRead  = false;
            var totalRead   = 0;

            var needToRead      = netProtocol.PacketHeadLen;
            var read            = 0;
            var currentOffset   = 0;
            var currentBuff     = headBuff;
            var currentLen      = netProtocol.PacketHeadLen;
            Buffer head     = new Buffer();

        READ_DATA:
            do
            {
                netProtocol.InformNetRunStart();

                if(netClient.Available <= 0)
                {
                    var tested          = 0;
                    bool isEndRead      = false;
                    IAsyncResult async  = null;

                    try
                    {
                        async = stream.BeginRead(packBuffer.testBuff, 0, 1, ar => 
                        { 
                            tested = stream.EndRead(ar);
                            Thread.MemoryBarrier();
                            isEndRead = true;
                            Thread.MemoryBarrier();
                        }, null);
                    }
                    catch (Exception ex)
                    {
                        var r = new PacketReadResult
                        {
                            packet = null,
                            errorMsg = "",
                            exception = ex
                        };
                        //allPackets.Add(r);
                        result.Add(r);
                        yield break;
                    }

                    Thread.MemoryBarrier();
                    while (!async.IsCompleted || !isEndRead)
                    {
                        yield return null;
                        if (!netClient.Connected)
                        {
                            var r = new PacketReadResult
                            {
                                packet = null,
                                errorMsg = "disconnected from server.",
                                exception = null
                            };
                            //allPackets.Add(r);
                            result.Add(r);
                            yield break;
                        }

                        if(dataStat != null)
                            dataStat.InformNetWaiting();

                        netProtocol.InformNetRunning();
                        if(netProtocol.IsResetNet())
                        {
                            netClient.Close();
                            var r = new PacketReadResult
                            {
                                packet = null,
                                errorMsg = "disconnected from server(time out).",
                                exception = null
                            };
                            //allPackets.Add(r);
                            result.Add(r);
                            yield break;
                        }
                    }

                    Thread.MemoryBarrier();
                    netProtocol.InformNetRunEnd();

                    if (tested <= 0)
                    {
                        yield return null;
                        if (tested <= 0)
                        {
                            var r = new PacketReadResult
                            {
                                packet = null,
                                errorMsg = "disconnected from server",
                                exception = null
                            };

                            //allPackets.Add(r);
                            result.Add(r);
                            yield break;
                        }
                    }

                    currentBuff[currentOffset++] = packBuffer.testBuff[0];
                    needToRead--;
                    read++;
                    if(needToRead <= 0)
                        break;
                }

                var maxRead = needToRead <= netClient.Available ? needToRead : netClient.Available;

                if (dataStat != null)
                    dataStat.InformReceiving(maxRead);
                try
                {
                    maxRead = stream.Read(currentBuff, currentOffset, maxRead);
                }
                catch (Exception ex)
                {
                    var r = new PacketReadResult
                    {
                        packet = null,
                        errorMsg = "",
                        exception = ex
                    };
                    //allPackets.Add(r);
                    result.Add(r);
                    yield break;
                }

                netProtocol.InformNetRunEnd();

                needToRead  -= maxRead;
                read        += maxRead;
                currentOffset += maxRead;

                if (dataStat != null)
                    dataStat.InformReceived(maxRead);

            }while(needToRead > 0);

            totalRead += read;

            if(!isHeadRead)
            {
                isHeadRead = true;
                head.buff = currentBuff; head.offset = 0; head.length = read;

                needToRead = netProtocol.ExtractPacketBodySize(head);
                if(needToRead <= 0)
                {
                    var r = new PacketReadResult
                    {
                        packet      = null,
                        errorMsg    = "Invalid packet head data",
                        exception   = null
                    };

                    //allPackets.Add(r);
                    result.Add(r);
                    yield break;
                }

                currentBuff     = packBuffer.bodyBuff;
                currentOffset   = 0;
                currentLen      = needToRead;
                read            = 0;

                if(needToRead > packBuffer.bodyBuff.Length)
                {
                    var r = new PacketReadResult
                    {
                        packet      = null,
                        errorMsg    = "Invalid protocol: packet buffer too small!",
                        exception   = null
                    };

                    //allPackets.Add(r);
                    result.Add(r);
                    yield break;
                }

                goto READ_DATA;
            }

            // construct packet
            var body = new Buffer{ buff = currentBuff, offset = 0, length = read };
            var resultPacket = ConstructPacket(netProtocol, head, body);
            //allPackets.Add(resultPacket);
            result.Add(resultPacket);
        }

        public static int WriteAsPacketToStream(Stream stream, string data)
        {
            var buff = Encoding.UTF8.GetBytes(data.ToCharArray());
            var head = BitConverter.GetBytes((Int32)buff.Length);
            stream.Write(head, 0, head.Length);
            stream.Write(buff, 0, buff.Length);

            return head.Length + buff.Length;
        }

        public static string InterpretAsString(Buffer buff)
        {
            var result = "";
            result = Encoding.UTF8.GetString(buff.buff, buff.offset, buff.length);

            return result;
        }
    }
}
