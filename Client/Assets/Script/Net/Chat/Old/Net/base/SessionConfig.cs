using System;

public sealed class SessionConfig
{
    internal int MaxSession { get; set; }

    internal int ListenPort { get; set; }

    internal int IdleTime { get; set; }

    internal int MinRecvBufferSize { get; set; }

    internal int MaxRecvBufferSize { get; set; }

    internal int MinSendBufferSize { get; set; }

    internal int MaxSendBufferSize { get; set; }

    internal int MaxSendMessage { get; set; }

    internal int MaxReceiveMessageEachTick { get; set; }

    internal IoHandler IoHandler { get; set; }

    internal IoFilter IoFilter { get; set; }
	
    internal SessionConfig()
    {
        this.ListenPort = 33333;
        this.MaxSession = 1000;
        this.MinRecvBufferSize = 4096;
        this.MaxRecvBufferSize = 4096 * 3;
        this.MinSendBufferSize = 4096;
        this.MaxSendBufferSize = 4096 * 3;
        this.MaxSendMessage = this.MaxSession * 10;
        this.MaxReceiveMessageEachTick = 50;
        this.IdleTime = 3 * 60 * 1000;
    }
}
