using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class JsonIoFilter :IoFilter
{
    public  bool  OnSessionCreate(IoSession session)
    {
        return true;
    }

    public  bool OnSessionClosed(IoSession session)
    {
        return false;
    }

    public  bool OnSessionIdle(IoSession session)
    {
        return true;
    }

    public  bool OnException(IoSession session, Exception exception)
    {

        Console.WriteLine(exception.Message);

        return true;
    }

	public  bool OnMessageReceived(IoSession session, DynamicBuffer buffer, out MessageBase message)
    {
        message = null;
        if(buffer.GetReadRemainBufferLength() <= 4)
        {
            return true;
        }

        int length;
        buffer.Read(out length);

        byte[] jsonBytes;
        if (buffer.GetReadRemainBufferLength() < length)
        {
            return true;
        }

        buffer.Read(length, out jsonBytes);

        string jsonStr = JCode.GetString(jsonBytes);

        try
        {
            string pt = getPT(jsonStr);

            switch (pt)
            {
				case "SC_LoginChat":
				{
					message = JCode.Decode<SC_Chat_Login>(jsonStr);
				}break;
                case "SC_ChatWorld":
                    message = JCode.Decode<SC_Chat_World>(jsonStr);
                    break;
                case "SC_ChatPrivate":
                    message = JCode.Decode<SC_Chat_Private>(jsonStr);
                    break;
                case "SC_ChatGuild":
                    message = JCode.Decode<SC_Chat_Guild>(jsonStr);
                    break;
                default:
                    message = null;
                    return false;
                    break;
            }

        	
        }
        catch (System.Exception ex)
        {
        	
            Console.WriteLine(ex.Message);
        }


        return true;
    }

	public  bool BeforeMessageSend(IoSession session, DynamicBuffer buffer, MessageBase message)
    {

         String json  =  JCode.Encode(message);
         byte[] bytes =  JCode.GetBytes(json);
		 buffer.Write (bytes.Length);
         buffer.Write(bytes);

         String sendStr = byteToHexStr(buffer.Buffer, buffer.Buffer.Length);
         System.Console.WriteLine("Send: " + sendStr);

        return true;
    }

	public  bool AfterMessageSend(IoSession session, DynamicBuffer buffer, MessageBase message)
    {
        return true;
    }

    /// <summary>
    /// 提取协议号
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static string getPT(string json)
    {
        string start = "\"pt\":\"";
        int startIndex = json.IndexOf(start);
        startIndex += start.Length;
        int endIndex = json.IndexOf("\"", startIndex);

        return json.Substring(startIndex, endIndex - startIndex);

    }


    /// <summary>
    /// 工具函数： 16进制转字符串
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    private static string byteToHexStr(byte[] bytes, int length)
    {
        string returnStr = "";
        if (bytes != null)
        {
            for (int i = 0; i < length; i++)
            {
                returnStr += bytes[i].ToString("X2");
                returnStr += " ";
            }
        }
        return returnStr;
    }

}

