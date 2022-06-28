using System;
using System.Net.Sockets;


public enum SessionStatus
{
    Connected,
    DisConnected,
}

public abstract class IoSession
{

    public int Id { get; set; }

    public string Ip { get; set; }

    public int Port { get; set; }

    public SessionStatus Status { get; set; }

    public int LastUpdateTime { get; set; }

    public Socket Socket { get; set; }

    public SocketAsyncEventArgs RecvArgs { get; set; }

    public DynamicBuffer RecvBuffer { get; protected set; }

    public Object UserToken { get; set; }

    public abstract void Close();

	public abstract bool SendMessage(MessageBase message);
}
