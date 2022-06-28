using System;

public class ChatIoHandler : IoHandler
{
	public ChatIoHandler()
	{
	}

	public bool OnSessionCreate(IoSession session)
	{
		CS_Chat_Login chatLogin = new CS_Chat_Login();
		session.SendMessage(chatLogin);

		return true;
	}
	
	public bool OnSessionClosed(IoSession session)
	{
		return true;
	}
	
	public bool OnMessageReceived(IoSession session, MessageBase message)
	{
		string windowName = "";

		switch(message.pt)
		{
		case "SC_LoginChat":
		{
			SC_Chat_Login retLogin = message as SC_Chat_Login;
			DataCenter.Set("WORLD_CHAT_TIMES", retLogin.worldChatTimes);
		}break;
		case "SC_ChatWorld":
		{
			windowName = "CHAT_WORLD_WINDOW";
		}break;
		case "SC_ChatGuild":
		{
			windowName = "CHAT_UNION_WINDOW";
		}break;
		case "SC_ChatPrivate":
		{
			windowName = "CHAT_PRIVATE_WINDOW";
		}break;
		}

		if(windowName != "" && message != null)
			DataCenter.SetData(windowName, "RECIEVE_CHAT", message);

		return true;
	}
	
	public bool BeforeMessageSend(IoSession session, MessageBase message)
	{
		return true;
	}
	
	public bool OnException(IoSession session, Exception exception)
	{
		return true;
	}
}
