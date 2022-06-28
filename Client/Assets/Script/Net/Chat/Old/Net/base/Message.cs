using System;

/*
public  class Message : MessageBase
{

    public String vs { get; set; }

    public String pt { get; set; }

    public int pi { get; set; }

    //public MessageHandler Handler { get; set; }

}
*/

public class MessageHandler
{
    public virtual MessageBase Buffer2Message(DynamicBuffer buffer)
    {
       throw new NotImplementedException();
    }

	public virtual DynamicBuffer Message2Buffer(MessageBase message)
    {
        DynamicBuffer buffer = new DynamicBuffer();
        return buffer;
    }

	public virtual bool Handle(IoSession session, MessageBase message)
    {
        throw new NotImplementedException();
    }
}
