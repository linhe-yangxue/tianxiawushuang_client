using System;

interface IoHandler
{
    bool OnSessionCreate(IoSession session);

    bool OnSessionClosed(IoSession session);

    bool OnMessageReceived(IoSession session, MessageBase message);

	bool BeforeMessageSend(IoSession session, MessageBase message);

    bool OnException(IoSession session, Exception exception);
}
