using UnityEngine;
using System.Collections;

public class ChatManager : Singleton<ChatManager>
{
	private ClientSession m_Session;

	void Start()
	{
	}

	void Update()
	{
	}

	void FixedUpdate()
	{
		if(m_Session != null && m_Session.Status == SessionStatus.Connected)
			m_Session.update();
	}

	void OnApplicationQuit()
	{
		if(m_Session != null && m_Session.Status == SessionStatus.Connected)
			m_Session.Close();
	}

	public void Init()
	{
		if(m_Session != null)
			return;

		SessionConfig config = new SessionConfig();
		config.IoFilter = new JsonIoFilter();
		config.IoHandler = new ChatIoHandler();
		m_Session = new ClientSession(config);
	}

	public void StartConnect(string ip, int port)
	{
		if(m_Session == null || m_Session.Status == SessionStatus.Connected)
			return;

		m_Session.Connect(ip, port);
	}
	public void Stop()
	{
		if(m_Session == null || m_Session.Status == SessionStatus.DisConnected)
			return;

		m_Session.Close();
	}

	public bool SendChatMessage(MessageBase message)
	{
		if(m_Session == null || m_Session.Status != SessionStatus.Connected)
			return false;

		m_Session.SendMessage(message);

		return true;
	}
}
