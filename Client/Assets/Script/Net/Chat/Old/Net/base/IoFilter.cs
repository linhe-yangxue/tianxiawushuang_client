using System;

interface IoFilter
{
    bool OnSessionCreate(IoSession session);

    bool OnSessionClosed(IoSession session);

    bool OnSessionIdle(IoSession session);

    bool OnException(IoSession session, Exception exception);

    bool OnMessageReceived(IoSession session, DynamicBuffer buffer, out MessageBase message);

	bool BeforeMessageSend(IoSession session, DynamicBuffer buffer, MessageBase message);

	bool AfterMessageSend(IoSession session, DynamicBuffer buffer, MessageBase message);
}
