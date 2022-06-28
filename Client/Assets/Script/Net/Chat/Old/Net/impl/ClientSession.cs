using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

class ClientSession : IoSession
{
    public ClientSession(SessionConfig config)
    {
        this._sessionConfig = config;
        this.Status = SessionStatus.DisConnected;
        this.RecvBuffer = new DynamicBuffer(this._sessionConfig.MinRecvBufferSize, this._sessionConfig.MaxRecvBufferSize);
    }

    public bool Connect(string ip, int  port)
    {
        this.Ip = ip;
        this.Port = port;

        if(this.Status == SessionStatus.DisConnected)
            return _ReConnect();
        return false;
    }

    /// <summary>
    /// 每个Tick或者几个tick调用1次，处理网络函数
    /// </summary>
    public void update()
    {
        HandlEvent();
        HandleInput();
        HandleClose();
        HandleIdle();
    }

    private void HandleIdle()
    {
        //throw new NotImplementedException();
    }

    private void HandleClose()
    {
        //throw new NotImplementedException();
    }

    private void HandleInput()
    {
		MessageBase message;
        for (int i = 0; i < this._sessionConfig.MaxReceiveMessageEachTick; i++)
        {
            if (!_recvMessageQueue.TryDequeue(out message))
            {
                break;
            }
            this._sessionConfig.IoHandler.OnMessageReceived(this, message);
        }
    }

    private void HandlEvent()
    {
        NetEvent netEvent = null;
        while (true)
        {
            if (!this._netEvents.TryDequeue(out netEvent))
            { 
                break;
            }
            switch (netEvent.e)
            {
                case NetEvent.Event.JoinServerComplete:
                    this._sessionConfig.IoHandler.OnSessionCreate(this);
                    break;
            }
        }
    }


    public bool _ReConnect() {
        try
        {
            this.Status = SessionStatus.DisConnected;
            IPAddress ipAddress = IPAddress.Parse(this.Ip);
            IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, this.Port);

            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
            arg.RemoteEndPoint = serverEndPoint;
            arg.Completed += new EventHandler<SocketAsyncEventArgs>(_ConnectCompleted);
            arg.UserToken = this.Socket;
            this.Socket.ConnectAsync(arg);

        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
            return false;
        }

        return true;
    
    }

    private void _ConnectCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.LastOperation != SocketAsyncOperation.Connect)
        {
            throw new Exception("Completed  Register Error");
        }
        this.Status = SessionStatus.Connected;
        System.Console.WriteLine("Connect SendCompleted");

        NetEvent call = new NetEvent()
        {
            e = NetEvent.Event.JoinServerComplete,
        };

        _netEvents.Enqueue(call);

        Socket socket = e.UserToken as Socket;
        this.RecvArgs = new SocketAsyncEventArgs();
        this.RecvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(_ReceiveCompleted);
        this.RecvArgs.SetBuffer(RecvBuffer.Buffer, RecvBuffer.WritePosition, RecvBuffer.GetWriteRemainBufferLength());
        socket.ReceiveAsync(this.RecvArgs);
    }


    private void Buffer2Message( int bytesTransferred)
    {
        this.RecvBuffer.AddWritePosition(bytesTransferred);
		MessageBase message = null;
        while (true)
        {
            message = null;
            this.RecvBuffer.MarkReadPostion();

            if (!this._sessionConfig.IoFilter.OnMessageReceived(this, this.RecvBuffer, out message))
            {
                throw new Exception("OnMessageReceived filter return error");
            }

            if (message != null)
            {
                _recvMessageQueue.Enqueue(message);
                this.RecvBuffer.CleanMarkReadPostion();
                this.RecvBuffer.Clean();
                continue;
            }
            else
            {
                this.RecvBuffer.RestReadPostion();
                break;
            }
        }
        int offset;
        int length;
        this.RecvBuffer.GetWriteRemainBuffer(out offset, out length);
        this.RecvArgs.SetBuffer(this.RecvBuffer.Buffer, offset, length);
    }
    private  void _ReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.LastOperation != SocketAsyncOperation.Receive)
        {
            throw new Exception("SocketAsyncEventArgs.Completed register error, this is not ReceiveSocketAsyncEventArgs");
        }
        ProcessReceive(sender, e);
    }

    private void ProcessReceive(object sender, SocketAsyncEventArgs e)
    {
        this.LastUpdateTime = Environment.TickCount;

        if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
        {
            this.Close();
			if(Status == SessionStatus.Connected)
				this._ReConnect();
            return;
        }

        try
        {
            this.Buffer2Message(e.BytesTransferred);
        }
        catch (Exception exp)
        {
            this._sessionConfig.IoFilter.OnException(this, exp);
            this.Close();
			if(Status == SessionStatus.Connected)
				this._ReConnect();
            return;
        }

        try
        {
            //post next recive
            bool willRaiseEvent = Socket.ReceiveAsync(this.RecvArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(sender, this.RecvArgs);
            }
        }
        catch (ObjectDisposedException ode) // socket already closed
        {
            Console.WriteLine(ode.Message);
        }
        catch (SocketException exp)
        {
            this._sessionConfig.IoFilter.OnException(this, exp);
        }
    }


    public override void Close()
    {
        try
        {
            this.Status = SessionStatus.DisConnected;
        	//Socket.Close();
			Socket.Disconnect(true);
			//Socket.EndReceive();
        }
        catch (System.Exception ex)
        {
        	System.Console.WriteLine(ex.ToString());
        }
    }

	public  override bool SendMessage(MessageBase message)
    {
        if (this.Status != SessionStatus.Connected)
            return false;

        if (!this._sessionConfig.IoHandler.BeforeMessageSend(this, message))
        {
                this.Close();
                this._ReConnect();
                return false;
        }
        this._SendMessage.BeginInvoke(this, message, ar =>
        {
            this._SendMessage.EndInvoke(ar);
        }, null);
 
        return false;
    }

	private Func<ClientSession, MessageBase, bool> _SendMessage = ( _self, message) =>
    {
        //TODO
        DynamicBuffer buffer = new DynamicBuffer();

        if(!_self._sessionConfig.IoFilter.BeforeMessageSend(_self,buffer,message))
        {
            throw new Exception("OnMessageSend filter return error");
        }
       
        try
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.UserToken = message;
            args.SetBuffer(buffer.Buffer, 0, buffer.WritePosition);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(_SendCompleted);

            bool willRaiseEvent = _self.Socket.SendAsync(args);
            if (!willRaiseEvent)
            {
                _self.ProcessSend(_self.Socket, args);
            }
            return willRaiseEvent;
        }
        catch (ObjectDisposedException) { _self._ReConnect(); }// socket already close
        catch (Exception e)
        {
            _self._sessionConfig.IoFilter.OnException(_self, e);          
            _self.Close();
            _self._ReConnect();
        }

        return false;
    };


    private void ProcessSend(Socket sender,SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
        {
            this.Close();
            this._ReConnect();
        }
    }

    private static void _SendCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.LastOperation != SocketAsyncOperation.Send)
        {
            throw new Exception("Completed  Register Error");
        }
       // DEBUG.Log("Message SendCompleted");
    }





    private class NetEvent
    {
        internal enum Event
        {
            JoinServerComplete,
            Error
        }

        internal Event e { set; get; }

        internal System.Object[] Args { set; get; }
    }

    private readonly ConcurrentQueue<NetEvent> _netEvents = new ConcurrentQueue<NetEvent>();
	private readonly ConcurrentQueue<MessageBase> _recvMessageQueue = new ConcurrentQueue<MessageBase>();
    private SessionConfig _sessionConfig;
}

