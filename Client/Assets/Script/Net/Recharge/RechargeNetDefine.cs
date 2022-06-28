using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

//充值协议文件
/// <summary>
/// 生成订单号
/// </summary>
/// 请求
public class CS_GenerateOrder : GameServerMessage
{
    public int shelfId;     //商品在配置中的id

    public CS_GenerateOrder()
        :base()
    {
        pt = "CS_GenerateOrder";
    }
}
// 回复
public class SC_GenerateOrder : RespMessage
{
    public string orderNum = "";        //订单号
    public string productId = "";       //渠道商品ID
}

/// <summary>
/// 每种金额首次充值记录
/// </summary>
/// 请求
public class CS_ChargeFirst : GameServerMessage
{
    public CS_ChargeFirst()
        :base()
    {
    }
}
//回复
public class SC_ChargeFirst : RespMessage
{
    public int[] infoArr;       //包含ChargeConfig配表中的索引数组
}

/// <summary>
/// 模拟充值结果会话
/// </summary>
/// 请求
public class CS_ChargeResult : MessageBase
{
    private static string m_ApiKey = "v8nqcwj7pzwr8uxr";

    public int code = 0;                //错误码，0为成功
    public int id = 0;                  //渠道用户id
    public string order = "";           //渠道订单号
    public string cporder = "";         //订单号
    public string info = "";            //额外信息
    public int amount = 0;              //充值金额
    public string sign = "";            //签名
    public bool flag = true;            //是否模拟充值

    public CS_ChargeResult()
        :base()
    {
        pt = "CS_ChargeResult";
#if !UNITY_EDITOR && !NO_USE_SDK
        flag = CommonParam.isUseSDK;
#else
        flag = true;
#endif
    }

    public void Sign()
    {
        string tmpStr = code.ToString() + "|" + id.ToString() + "|" + order + "|" +
            cporder.ToString() + "|" + info + "|" + m_ApiKey;
        MemoryStream tmpMS = new MemoryStream(Encoding.UTF8.GetBytes(tmpStr));
        sign = MD5.CalculateMD5(tmpMS);
        tmpMS.Close();
        sign = sign.ToLower();
    }
}
//回复
public class SC_ChargeSuccesSimulate : RespMessage
{
    public int code = 0;        //错误码
    public string msg = "";     //错误信息
}

/// <summary>
/// 请求元宝,VIP经验,VIP等级
/// </summary>
public class CS_ChargeFeedBack : GameServerMessage 
{
    public CS_ChargeFeedBack() 
    {
        pt = "CS_ChargeFeedBack";
    }
}
//回复
public class SC_ChargeFeedBack : RespMessage 
{
    public int diamond;      //> 元宝总数
    public int vipLevel;     //> VIP等级
    public int vipExp;       //> VIP经验
}

/// <summary>
/// 用户支付订单
/// </summary>
/// 请求
public class CS_ChargePayOrder : GameServerMessage
{
    public string cporder = "";         //内部订单号

    public CS_ChargePayOrder()
        : base()
    {
        pt = "CS_ChargePayOrder";
    }
}
//回复
public class SC_ChargePayOrder : RespMessage
{
}

/// <summary>
/// 用户取消订单
/// </summary>
/// 请求
public class CS_ChargeCancelOrder : GameServerMessage
{
    public string cporder = "";         //内部订单号

    public CS_ChargeCancelOrder()
        : base()
    {
        pt = "CS_ChargeCancelOrder";
    }
}
//回复
public class SC_ChargeCancelOrder : RespMessage
{
}
